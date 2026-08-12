namespace backend.Entities;

public class ExpenseCategory
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public int DriverId { get; set; }
    public Driver Driver { get; set; } = null!;

    public ICollection<Expense> Expenses { get; set; } = [];
}
