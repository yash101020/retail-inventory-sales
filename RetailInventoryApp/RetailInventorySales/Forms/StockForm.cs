using RetailInventorySales.Models;
using RetailInventorySales.Services;

namespace RetailInventorySales.Forms;

public class StockForm : Form
{
    private readonly ProductService service;
    private readonly DataGridView grid = Ui.Grid();
    private readonly TextBox stockInput = new() { Width = 120, AccessibleName = "New stock quantity" };
    public StockForm(string databasePath)
    {
        service = new ProductService(databasePath);
        Ui.Style(this, "Stock Management");
        Ui.Column(grid, "Id", "ID"); Ui.Column(grid, "Name", "Product"); Ui.Column(grid, "StockQuantity", "Current stock");
        Controls.Add(grid);
        Controls.Add(Ui.Row(Ui.Label("New stock quantity"), stockInput, Ui.Button("Set selected stock", Save), Ui.Button("Refresh", RefreshStock)));
        Controls.Add(Ui.Row(Ui.Label("Select a product to enter its new total stock (not the quantity to add). Completed sales reduce this automatically.")));
        grid.SelectionChanged += (_, _) =>
        {
            if (grid.CurrentRow?.DataBoundItem is Product p) stockInput.Text = p.StockQuantity.ToString();
        };
        Shown += (_, _) => Ui.Run(this, RefreshStock);
    }
    private void RefreshStock() => grid.DataSource = service.GetProducts();
    private void Save()
    {
        if (grid.CurrentRow?.DataBoundItem is not Product p) throw new ArgumentException("Select a product first.");
        int stock = InputValidation.WholeNumber(stockInput.Text, "stock");
        ProductService.ValidateStock(stock);
        if (MessageBox.Show(this, $"Set stock for {p.Name} to {stock}?", "Confirm stock adjustment", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        service.SetStock(p.Id, stock); RefreshStock();
        MessageBox.Show(this, "Stock updated.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
