using System.Net.Http.Json;
using System.Text.Json;
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
        Console.WriteLine("GeminiService initialized");
    }

    public async Task<object> HandleUserQueryAsync(string userText)
    {
        Console.WriteLine("HandleUserQueryAsync() hit");

        var plan = await GetPlanFromGeminiAsync(userText);

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
                    results.Add(new { tool = "searchProducts", products });
                    break;

                case "addToBasket":
                    if (action.ProductId.HasValue)
                    {
                        _basketService.AddToBasket(action.ProductId.Value);
                        results.Add(new { tool = "addToBasket", productId = action.ProductId });
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
                        }).ToList();

                    results.Add(new { tool = "showBasket", basket });
                    break;
            }
        }

        return new { plan, results };
    }

    private static string RemoveLeadingThoughts(string text)
    {
        text = text.Trim();

        int idx = text.IndexOf('{');
        if (idx > 0)
            return text.Substring(idx).Trim();

        return text;
    }

    private async Task<AgentPlan> GetPlanFromGeminiAsync(string userText)
    {
        var apiKey = _config["Gemini:ApiKey"];
        if (string.IsNullOrEmpty(apiKey))
            throw new InvalidOperationException("Gemini API key is not configured.");

        Console.WriteLine("Gemini API key OK.");

        var model = _config["Gemini:Model"];

        var url = $"https://generativelanguage.googleapis.com/v1/models/{model}:generateContent?key={apiKey}";
        Console.WriteLine("Gemini URL: " + url);

        var systemInstruction = @"
You are a shopping agent that works with a LOCAL product catalog only.
Respond ONLY with JSON, no explanations.

Example:
{
  ""actions"": [
    { ""type"": ""searchProducts"", ""query"": ""laptop"", ""minRam"": 12, ""cpu"": ""i5"" },
    { ""type"": ""addToBasket"", ""productId"": 2 },
    { ""type"": ""showBasket"" }
  ]
}
";

        var payload = new
        {
            contents = new[]
            {
                new
                {
                    role = "user",
                    parts = new[]
                    {
                        new { text = systemInstruction },
                        new { text = userText }
                    }
                }
            },
            generationConfig = new
            {
                temperature = 0.2
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, payload);

        var raw = await response.Content.ReadAsStringAsync();
        Console.WriteLine("RAW GEMINI RESPONSE:");
        Console.WriteLine(raw);

        response.EnsureSuccessStatusCode();

        var doc = JsonDocument.Parse(raw);

        var text = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        Console.WriteLine("MODEL OUTPUT:");
        Console.WriteLine(text);

        // --------------------------------------------
        // Remove Markdown code fences
        // --------------------------------------------
        var cleaned = text!
            .Trim()
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        Console.WriteLine("CLEANED OUTPUT:");
        Console.WriteLine(cleaned);

        // --------------------------------------------
        // Parse JSON safely
        // --------------------------------------------
        AgentPlan? plan = null;

        try
        {
            // Extra safety: prevent Gemini "thinking" from breaking JSON
            cleaned = RemoveLeadingThoughts(cleaned);

            plan = JsonSerializer.Deserialize<AgentPlan>(
                cleaned,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine("JSON PARSE ERROR:");
            Console.WriteLine(ex);
        }

        // If parsing failed → return safe error structure
        if (plan == null || plan.Actions == null || plan.Actions.Count == 0)
        {
            Console.WriteLine("Returning fallback AgentPlan.");
            return new AgentPlan(
                new List<AgentAction>
                {
                    new AgentAction("error", null, null, null, null)
                }
            );
        }

        return plan;
    }
}
