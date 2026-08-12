namespace backend.Dtos;

public record CreateExpenseRequest(int RouteId, int CategoryId, decimal Value, string? Note);

// RouteId não é editável aqui — mover um gasto entre rotas não é um requisito
// documentado; só faz sentido criar um novo lançamento na rota correta.
public record UpdateExpenseRequest(int CategoryId, decimal Value, string? Note);

public record ExpenseResponse(int Id, int RouteId, int CategoryId, string CategoryName, decimal Value, string? Note);
