using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AiShoppingAgent.Api.Models;

namespace AiShoppingAgent.Api.Services;

public record AgentAction(
    string Type,
    string? Query,
    int? MinRam,
    string? Cpu,
    int? ProductId
);

public record AgentPlan(
    List<AgentAction> Actions
);

public class GeminiService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly IProductService _productService;
    private readonly IBasketService _basketService;

    public GeminiService(
        HttpClient httpClient,
        IConfiguration config,
        IProductService productService,
        IBasketService basketService)
    {
        _httpClient = httpClient;
        _config = config;
        _productService = productService;
        _basketService = basketService;
    }

    public async Task<object> HandleUserQueryAsync(string userText)
    {
        // 1. Ask Gemini to create a plan
        var plan = await GetPlanFromGeminiAsync(userText);

        // 2. Execute the plan with local tools
        var results = new List<object>();

        foreach (var action in plan.Actions)
        {
            switch (action.Type)
            {
                case "searchProducts":
                    var products = _productService.Search(
                        action.Query,
                        action.MinRam,
                        action.Cpu
                    );
                    results.Add(new
                    {
                        tool = "searchProducts",
                        products
                    });
                    break;

                case "addToBasket":
                    if (action.ProductId.HasValue)
                    {
                        _basketService.AddToBasket(action.ProductId.Value);
                        results.Add(new
                        {
                            tool = "addToBasket",
                            productId = action.ProductId
                        });
                    }
                    break;

                case "showBasket":
                    var basket = _basketService.GetBasket()
                        .Select(x => new
                        {
                            x.Product.Id,
                            x.Product.Name,
                            x.Quantity,
                            x.Product.Price
                        })
                        .ToList();
                    results.Add(new
                    {
                        tool = "showBasket",
                        basket
                    });
                    break;
            }
        }

        // 3. Return both the plan and execution results
        return new
        {
            plan,
            results
        };
    }

    private async Task<AgentPlan> GetPlanFromGeminiAsync(string userText)
    {
        var apiKey = _config["Gemini:ApiKey"] ?? Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        var model = _config["Gemini:Model"] ?? "gemini-1.5-flash";

        if (string.IsNullOrEmpty(apiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        // Google Gemini REST endpoint pattern (v1beta, generative model)
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{model}:generateContent?key={apiKey}";

        var systemInstruction = @"
You are a shopping agent that works with a local product catalog.
The user might mention emag.ro but you only use LOCAL tools.

You MUST respond ONLY with a JSON object of the form:

{
  ""actions"": [
    {
      ""type"": ""searchProducts"",
      ""query"": ""laptop"",
      ""minRam"": 12,
      ""cpu"": ""core i5""
    },
    {
      ""type"": ""addToBasket"",
      ""productId"": 2
    },
    {
      ""type"": ""showBasket""
    }
  ]
}

Allowed action types:
- ""searchProducts"" (query: string?, minRam: int?, cpu: string?)
- ""addToBasket"" (productId: int)
- ""showBasket"" (no extra fields)

Do NOT write any explanation outside JSON.
If unsure which productId to pick, choose the BEST MATCHING product.
";

        var payload = new
        {
            contents = new[]
            {
                new {
                    role = "user",
                    parts = new object[]
                    {
                        new { text = systemInstruction + "\n\nUser: " + userText }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2,
                responseMimeType = "application/json" // Ask for JSON directly
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var candidates = root.GetProperty("candidates");
        var content = candidates[0].GetProperty("content");
        var parts = content.GetProperty("parts");
        var text = parts[0].GetProperty("text").GetString();

        // text should be a JSON string we can now parse
        var plan = JsonSerializer.Deserialize<AgentPlan>(text!, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (plan == null || plan.Actions == null)
        {
            return new AgentPlan(new List<AgentAction>());
        }

        return plan;
    }
}
