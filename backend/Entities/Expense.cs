namespace backend.Entities;

public class Expense
{
    public int Id { get; set; }

    // Valor monetário: decimal (não float) evita erro de arredondamento.
    public decimal Value { get; set; }
    public string? Note { get; set; }

    public int RouteId { get; set; }
    public Route Route { get; set; } = null!;

    public int CategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = null!;
}
