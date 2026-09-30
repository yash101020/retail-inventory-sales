namespace RetailInventorySales.Models;

public class TransactionItem
{
    public int Id { get; set; }
    public int TransactionId { get; set; }
    public SaleTransaction Transaction { get; set; } = null!;
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    // A receipt must keep the name and price that applied at the time of sale.
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}
