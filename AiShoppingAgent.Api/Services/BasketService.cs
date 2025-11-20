using AiShoppingAgent.Api.Models;

namespace AiShoppingAgent.Api.Services;

public interface IBasketService
{
    void AddToBasket(int productId, int quantity = 1);
    IEnumerable<(Product Product, int Quantity)> GetBasket();
    void Clear();
}

public class BasketService : IBasketService
{
    private readonly IProductService _productService;
    private readonly Dictionary<int, int> _basket = new();

    public BasketService(IProductService productService)
    {
        _productService = productService;
    }

    public void AddToBasket(int productId, int quantity = 1)
    {
        if (_basket.ContainsKey(productId))
        {
            _basket[productId] += quantity;
        }
        else
        {
            _basket[productId] = quantity;
        }
    }

    public IEnumerable<(Product Product, int Quantity)> GetBasket()
    {
        foreach (var kvp in _basket)
        {
            var product = _productService.GetById(kvp.Key);
            if (product != null)
                yield return (product, kvp.Value);
        }
    }

    public void Clear() => _basket.Clear();
}
