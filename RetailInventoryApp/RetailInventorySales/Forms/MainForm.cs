namespace RetailInventorySales.Forms;

public class MainForm : Form
{
    public MainForm(string databasePath)
    {
        Ui.Style(this, "Retail Inventory & Sales");
        var heading = Ui.Label("RETAIL INVENTORY & SALES");
        heading.Font = new Font("Segoe UI", 23, FontStyle.Bold);
        var menu = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        menu.Controls.Add(heading);
        menu.Controls.Add(Ui.Label("Manage products, record sales, and keep track of your stock."));
        menu.Controls.Add(Ui.Button("Product Management", () => Open(new ProductsForm(databasePath))));
        menu.Controls.Add(Ui.Button("Sales", () => Open(new SalesForm(databasePath))));
        menu.Controls.Add(Ui.Button("Stock Management", () => Open(new StockForm(databasePath))));
        menu.Controls.Add(Ui.Button("Transaction History", () => Open(new HistoryForm(databasePath))));
        menu.Controls.Add(Ui.Button("Low Stock Alerts", () => Open(new LowStockForm(databasePath))));
        menu.Controls.Add(Ui.Label("Getting started: add products, then open Sales. All amounts are in LKR."));
        menu.Controls.Add(Ui.Label("Data is saved automatically after each successful action."));
        foreach (var button in menu.Controls.OfType<Button>())
        {
            button.AutoSize = false;
            button.Size = new Size(270, 46);
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(12, 0, 0, 0);
        }
        menu.AutoScroll = true;
        Controls.Add(menu);
    }
    private void Open(Form form)
    {
        using (form) form.ShowDialog(this);
    }
}
