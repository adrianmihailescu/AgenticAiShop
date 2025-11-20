namespace AiShoppingAgent.Api.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Cpu { get; set; } = string.Empty;
    public int RamGb { get; set; }
    public decimal Price { get; set; }
    public string Shop { get; set; } = "emag";
}
