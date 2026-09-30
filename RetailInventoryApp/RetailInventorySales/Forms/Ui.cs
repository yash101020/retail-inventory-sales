using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using RetailInventorySales.Data;

namespace RetailInventorySales.Forms;

public static class Ui
{
    public static void Style(Form form, string title)
    {
        form.Text = title;
        form.Font = new Font("Segoe UI", 10);
        form.BackColor = Color.FromArgb(246, 248, 251);
        form.ClientSize = new Size(1000, 650);
        form.MinimumSize = new Size(900, 600);
        form.StartPosition = FormStartPosition.CenterParent;
        form.AutoScaleMode = AutoScaleMode.Dpi;
        form.Padding = new Padding(20);
    }

    public static Label Label(string text) => new() { Text = text, AutoSize = true, MaximumSize = new Size(820, 0), Margin = new Padding(6, 10, 6, 8) };
    public static Button Button(string text, Action action)
    {
        var button = new Button { Text = text, AutoSize = true, MinimumSize = new Size(120, 36), Margin = new Padding(6) };
        button.Click += (_, _) => Run(button.FindForm(), action);
        return button;
    }
    public static FlowLayoutPanel Row(params Control[] controls)
    {
        var row = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, Padding = new Padding(0, 6, 0, 6), WrapContents = true };
        row.Controls.AddRange(controls);
        return row;
    }
    public static DataGridView Grid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        AutoGenerateColumns = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, RowHeadersVisible = false,
        BackgroundColor = Color.White, BorderStyle = BorderStyle.None, RowTemplate = { Height = 32 },
        ColumnHeadersHeight = 38, EnableHeadersVisualStyles = false,
        ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(224, 232, 241), ForeColor = Color.FromArgb(24, 45, 67) },
        AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle { BackColor = Color.FromArgb(245, 248, 251) }
    };
    public static void Column(DataGridView grid, string property, string title, string? format = null)
    {
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = property, HeaderText = title,
            DefaultCellStyle = new DataGridViewCellStyle { Format = format ?? "" } });
    }
    public static void Run(IWin32Window? owner, Action action)
    {
        try { action(); } catch (Exception ex) { ShowError(owner, ex); }
    }
    public static void ShowError(IWin32Window? owner, Exception ex)
    {
        string message = ex switch
        {
            ArgumentException => ex.Message,
            DbUpdateConcurrencyException => "The product changed while you were working. Refresh and try again.",
            DbUpdateException or SqliteException => "The database operation failed. Check that the data folder is writable and close other copies of the app, then try again. Your sale was not partially saved.",
            _ => "The operation could not be completed. Please restart the application and try again."
        };
        if (ex is not ArgumentException)
        {
            try
            {
                Directory.CreateDirectory(AppPaths.DataFolder);
                File.AppendAllText(Path.Combine(AppPaths.DataFolder, "errors.log"), $"{DateTime.Now:O} {ex}\n");
            }
            catch (Exception logError)
            {
                message += " The error log could not be written.";
                System.Diagnostics.Debug.WriteLine(logError);
            }
        }
        MessageBox.Show(owner, message, "Unable to complete action", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }
}
