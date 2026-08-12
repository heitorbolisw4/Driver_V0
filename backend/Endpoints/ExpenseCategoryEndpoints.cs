using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class ExpenseCategoryEndpoints
{
    public static IEndpointRouteBuilder MapExpenseCategoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/expense-categories").RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<IResult> GetAll(ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var categories = await db.ExpenseCategories
            .Where(c => c.DriverId == driverId)
            .Select(c => ToResponse(c))
            .ToListAsync();

        return Results.Ok(categories);
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var category = await FindOwned(db, id, driverId.Value);
        if (category is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(category));
    }

    private static async Task<IResult> Create(CreateExpenseCategoryRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var error = ValidateName(request.Name);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        // RN-18: categorias são específicas de cada motorista, não globais.
        if (await db.ExpenseCategories.AnyAsync(c => c.DriverId == driverId && c.Name == request.Name))
        {
            return Results.Conflict(new { message = "A category with this name already exists." });
        }

        var category = new ExpenseCategory
        {
            Name = request.Name,
            DriverId = driverId.Value
        };

        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync();

        return Results.Created($"/expense-categories/{category.Id}", ToResponse(category));
    }

    private static async Task<IResult> Update(int id, UpdateExpenseCategoryRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var category = await FindOwned(db, id, driverId.Value);
        if (category is null)
        {
            return Results.NotFound();
        }

        var error = ValidateName(request.Name);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (await db.ExpenseCategories.AnyAsync(c => c.DriverId == driverId && c.Name == request.Name && c.Id != id))
        {
            return Results.Conflict(new { message = "A category with this name already exists." });
        }

        category.Name = request.Name;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(category));
    }

    private static async Task<IResult> Delete(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var category = await FindOwned(db, id, driverId.Value);
        if (category is null)
        {
            return Results.NotFound();
        }

        // FK Restrict (ExpenseCategoryConfiguration): checar antes de excluir
        // evita estourar DbUpdateException por violação de FK.
        if (await db.Expenses.AnyAsync(e => e.CategoryId == id))
        {
            return Results.Conflict(new { message = "Cannot delete a category that has expenses linked to it." });
        }

        db.ExpenseCategories.Remove(category);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static Task<ExpenseCategory?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static ExpenseCategoryResponse ToResponse(ExpenseCategory category)
    {
        return new ExpenseCategoryResponse(category.Id, category.Name);
    }

    private static string? ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        return null;
    }
}
