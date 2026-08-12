namespace backend.Dtos;

public record CreateExpenseCategoryRequest(string Name);

public record UpdateExpenseCategoryRequest(string Name);

public record ExpenseCategoryResponse(int Id, string Name);
