using RetailInventorySales.Models;
using RetailInventorySales.Services;

namespace RetailInventorySales.Forms;

public class HistoryForm : Form
{
    private readonly HistoryService service;
    private readonly DataGridView transactions = Ui.Grid();
    private readonly DataGridView items = Ui.Grid();
    private readonly Label details = Ui.Label("Select a transaction to see its items.");
    public HistoryForm(string databasePath)
    {
        service = new HistoryService(databasePath);
        Ui.Style(this, "Transaction History");
        Ui.Column(transactions, "Id", "Transaction ID");
        Ui.Column(transactions, "TransactionDate", "Date / time", "yyyy-MM-dd HH:mm:ss");
        Ui.Column(transactions, "TotalAmount", "Total (LKR)", "N2");
        Ui.Column(items, "ProductName", "Product sold"); Ui.Column(items, "Quantity", "Quantity");
        Ui.Column(items, "UnitPrice", "Unit price (LKR)", "N2"); Ui.Column(items, "Subtotal", "Subtotal (LKR)", "N2");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        layout.Controls.Add(transactions, 0, 0); layout.Controls.Add(details, 0, 1); layout.Controls.Add(items, 0, 2);
        Controls.Add(layout);
        Controls.Add(Ui.Row(Ui.Label("Completed sales - select a row to view its receipt."), Ui.Button("Refresh", RefreshHistory)));
        transactions.SelectionChanged += (_, _) => Ui.Run(this, ShowDetails);
        Shown += (_, _) => Ui.Run(this, RefreshHistory);
    }
    private void RefreshHistory()
    {
        transactions.DataSource = service.GetTransactions(); ShowDetails();
    }
    private void ShowDetails()
    {
        if (transactions.CurrentRow?.DataBoundItem is not SaleTransaction sale)
        {
            items.DataSource = null; details.Text = "No completed transactions yet."; return;
        }
        items.DataSource = service.GetItems(sale.Id);
        details.Text = $"Receipt #{sale.Id}  |  {sale.TransactionDate:yyyy-MM-dd HH:mm:ss}  |  Total: LKR {sale.TotalAmount:N2}";
    }
}
