namespace RetailInventorySales.Services;

public static class InputValidation
{
    public static int WholeNumber(string text, string field)
    {
        if (!int.TryParse(text, out int value))
            throw new ArgumentException($"Enter a valid whole number for {field}.");
        return value;
    }
    public static decimal Price(string text)
    {
        if (!decimal.TryParse(text, out decimal value)) throw new ArgumentException("Enter a valid numeric price.");
        return value;
    }
    public static int SaleQuantity(string text)
    {
        int value = WholeNumber(text, "quantity");
        if (value <= 0) throw new ArgumentException("Quantity must be greater than zero.");
        return value;
    }
}
