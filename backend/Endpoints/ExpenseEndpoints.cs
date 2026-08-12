using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class ExpenseEndpoints
{
    public static IEndpointRouteBuilder MapExpenseEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/expenses").RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapDelete("/{id:int}", Delete);

        return app;
    }

    private static async Task<IResult> GetAll(int? routeId, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var query = db.Expenses
            .Include(e => e.Category)
            .Where(e => e.Route.DriverId == driverId);

        if (routeId is not null)
        {
            query = query.Where(e => e.RouteId == routeId);
        }

        var expenses = await query.ToListAsync();

        return Results.Ok(expenses.Select(e => ToResponse(e, e.Category.Name)));
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var expense = await FindOwned(db, id, driverId.Value);
        if (expense is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(expense, expense.Category.Name));
    }

    private static async Task<IResult> Create(CreateExpenseRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var error = ValidateValue(request.Value);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (!await db.Routes.AnyAsync(r => r.Id == request.RouteId && r.DriverId == driverId))
        {
            return Results.BadRequest(new { message = "Route not found." });
        }

        var category = await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == request.CategoryId && c.DriverId == driverId);
        if (category is null)
        {
            return Results.BadRequest(new { message = "ExpenseCategory not found." });
        }

        var expense = new Expense
        {
            RouteId = request.RouteId,
            CategoryId = category.Id,
            Value = request.Value,
            Note = request.Note
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();

        return Results.Created($"/expenses/{expense.Id}", ToResponse(expense, category.Name));
    }

    private static async Task<IResult> Update(int id, UpdateExpenseRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var expense = await FindOwned(db, id, driverId.Value);
        if (expense is null)
        {
            return Results.NotFound();
        }

        var error = ValidateValue(request.Value);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        var category = await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == request.CategoryId && c.DriverId == driverId);
        if (category is null)
        {
            return Results.BadRequest(new { message = "ExpenseCategory not found." });
        }

        expense.CategoryId = category.Id;
        expense.Value = request.Value;
        expense.Note = request.Note;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(expense, category.Name));
    }

    private static async Task<IResult> Delete(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var expense = await FindOwned(db, id, driverId.Value);
        if (expense is null)
        {
            return Results.NotFound();
        }

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static Task<Expense?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.Expenses
            .Include(e => e.Category)
            .SingleOrDefaultAsync(e => e.Id == id && e.Route.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static ExpenseResponse ToResponse(Expense expense, string categoryName)
    {
        return new ExpenseResponse(expense.Id, expense.RouteId, expense.CategoryId, categoryName, expense.Value, expense.Note);
    }

    private static string? ValidateValue(decimal value)
    {
        // RN-20: valor deve ser >= 0.
        if (value < 0)
        {
            return "Value must be greater than or equal to zero.";
        }

        return null;
    }
}
