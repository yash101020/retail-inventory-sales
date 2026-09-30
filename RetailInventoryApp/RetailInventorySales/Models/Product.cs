namespace RetailInventorySales.Models;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string NormalizedName { get; set; } = "";
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
}
