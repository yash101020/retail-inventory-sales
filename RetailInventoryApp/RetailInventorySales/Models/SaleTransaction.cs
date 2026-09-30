namespace RetailInventorySales.Models;

public class SaleTransaction
{
    public int Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal TotalAmount { get; set; }
    public List<TransactionItem> Items { get; set; } = [];
}
