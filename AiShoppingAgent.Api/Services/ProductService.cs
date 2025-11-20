using AiShoppingAgent.Api.Models;

namespace AiShoppingAgent.Api.Services;

public interface IProductService
{
    IEnumerable<Product> Search(string? query, int? minRam, string? cpu);
    Product? GetById(int id);
    IEnumerable<Product> GetAll();
}

public class ProductService : IProductService
{
    private readonly List<Product> _products = new()
    {
        new Product { Id = 1, Name = "Laptop HP 15", Brand = "HP", Cpu = "Core i5", RamGb = 8, Price = 2500, Shop = "emag" },
        new Product { Id = 2, Name = "Laptop Lenovo IdeaPad", Brand = "Lenovo", Cpu = "Core i5", RamGb = 12, Price = 2800, Shop = "emag" },
        new Product { Id = 3, Name = "Laptop Dell Inspiron", Brand = "Dell", Cpu = "Core i7", RamGb = 16, Price = 3500, Shop = "emag" },
        new Product { Id = 4, Name = "Laptop Asus VivoBook", Brand = "Asus", Cpu = "Core i5", RamGb = 12, Price = 3000, Shop = "emag" },
    };

    public IEnumerable<Product> Search(string? query, int? minRam, string? cpu)
    {
        var q = _products.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            q = q.Where(p =>
                p.Name.ToLower().Contains(query) ||
                p.Brand.ToLower().Contains(query) ||
                p.Cpu.ToLower().Contains(query));
        }

        if (minRam.HasValue)
        {
            q = q.Where(p => p.RamGb >= minRam.Value);
        }

        if (!string.IsNullOrEmpty(cpu))
        {
            cpu = cpu.ToLower();
            q = q.Where(p => p.Cpu.ToLower().Contains(cpu));
        }

        return q.ToList();
    }

    public Product? GetById(int id) => _products.FirstOrDefault(p => p.Id == id);

    public IEnumerable<Product> GetAll() => _products;
}
