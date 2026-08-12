using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class TruckLoadEndpoints
{
    public static IEndpointRouteBuilder MapTruckLoadEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/truckloads").RequireAuthorization();

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

        var truckLoads = await db.TruckLoads
            .Where(tl => tl.DriverId == driverId)
            .Select(tl => ToResponse(tl))
            .ToListAsync();

        return Results.Ok(truckLoads);
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truckLoad = await FindOwned(db, id, driverId.Value);
        if (truckLoad is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(truckLoad));
    }

    private static async Task<IResult> Create(CreateTruckLoadRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var error = Validate(request.Type, request.Weight);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        var truckLoad = new TruckLoad
        {
            Type = request.Type,
            Weight = request.Weight,
            IsShipped = request.IsShipped,
            DriverId = driverId.Value
        };

        db.TruckLoads.Add(truckLoad);
        await db.SaveChangesAsync();

        return Results.Created($"/truckloads/{truckLoad.Id}", ToResponse(truckLoad));
    }

    private static async Task<IResult> Update(int id, UpdateTruckLoadRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truckLoad = await FindOwned(db, id, driverId.Value);
        if (truckLoad is null)
        {
            return Results.NotFound();
        }

        var error = Validate(request.Type, request.Weight);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        truckLoad.Type = request.Type;
        truckLoad.Weight = request.Weight;
        truckLoad.IsShipped = request.IsShipped;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(truckLoad));
    }

    private static async Task<IResult> Delete(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truckLoad = await FindOwned(db, id, driverId.Value);
        if (truckLoad is null)
        {
            return Results.NotFound();
        }

        db.TruckLoads.Remove(truckLoad);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static Task<TruckLoad?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.TruckLoads.SingleOrDefaultAsync(tl => tl.Id == id && tl.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static TruckLoadResponse ToResponse(TruckLoad truckLoad)
    {
        return new TruckLoadResponse(truckLoad.Id, truckLoad.Type, truckLoad.Weight, truckLoad.IsShipped);
    }

    private static string? Validate(string type, float weight)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            return "Type is required.";
        }

        // RN-17: peso deve ser maior que zero (CK_TruckLoad_Weight_Positive).
        if (weight <= 0)
        {
            return "Weight must be greater than zero.";
        }

        return null;
    }
}
