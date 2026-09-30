using RetailInventorySales.Services;

namespace RetailInventorySales.Forms;

public class LowStockForm : Form
{
    private readonly StockService service;
    private readonly DataGridView grid = Ui.Grid();
    private readonly Label status = Ui.Label("");
    public LowStockForm(string databasePath)
    {
        service = new StockService(databasePath);
        Ui.Style(this, "Low Stock Alerts");
        Ui.Column(grid, "Name", "Product"); Ui.Column(grid, "StockQuantity", "Remaining stock");
        grid.DefaultCellStyle.ForeColor = Color.FromArgb(153, 70, 10);
        Controls.Add(grid);
        Controls.Add(Ui.Row(status, Ui.Button("Refresh", RefreshAlerts)));
        Controls.Add(Ui.Row(Ui.Label($"Reorder reminder: products with stock at or below {StockService.LowStockThreshold}, including out-of-stock products.")));
        Shown += (_, _) => Ui.Run(this, RefreshAlerts);
    }
    private void RefreshAlerts()
    {
        var products = service.GetLowStockProducts(); grid.DataSource = products;
        status.Text = products.Count == 0 ? "No low-stock products." : $"{products.Count} product(s) need attention. Update stock in Stock Management.";
    }
}
