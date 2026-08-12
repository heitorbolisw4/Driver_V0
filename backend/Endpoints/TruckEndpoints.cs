using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class TruckEndpoints
{
    public static IEndpointRouteBuilder MapTruckEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/trucks").RequireAuthorization();

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

        var trucks = await db.Trucks
            .Where(t => t.DriverId == driverId)
            .Select(t => ToResponse(t))
            .ToListAsync();

        return Results.Ok(trucks);
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truck = await FindOwned(db, id, driverId.Value);
        if (truck is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(truck));
    }

    private static async Task<IResult> Create(CreateTruckRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var error = ValidatePlate(request.Plate) ?? ValidateHodometer(request.Hodometer);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (await db.Trucks.AnyAsync(t => t.DriverId == driverId && t.Plate == request.Plate))
        {
            return Results.Conflict(new { message = "A truck with this plate already exists." });
        }

        var truck = new Truck
        {
            Plate = request.Plate,
            Hodometer = request.Hodometer,
            DriverId = driverId.Value
        };

        db.Trucks.Add(truck);
        await db.SaveChangesAsync();

        return Results.Created($"/trucks/{truck.Id}", ToResponse(truck));
    }

    private static async Task<IResult> Update(int id, UpdateTruckRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truck = await FindOwned(db, id, driverId.Value);
        if (truck is null)
        {
            return Results.NotFound();
        }

        var error = ValidatePlate(request.Plate);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (await db.Trucks.AnyAsync(t => t.DriverId == driverId && t.Plate == request.Plate && t.Id != id))
        {
            return Results.Conflict(new { message = "A truck with this plate already exists." });
        }

        truck.Plate = request.Plate;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(truck));
    }

    private static async Task<IResult> Delete(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var truck = await FindOwned(db, id, driverId.Value);
        if (truck is null)
        {
            return Results.NotFound();
        }

        db.Trucks.Remove(truck);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static Task<Truck?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.Trucks.SingleOrDefaultAsync(t => t.Id == id && t.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static TruckResponse ToResponse(Truck truck)
    {
        return new TruckResponse(truck.Id, truck.Plate, truck.Hodometer);
    }

    private static string? ValidatePlate(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
        {
            return "Plate is required.";
        }

        return null;
    }

    private static string? ValidateHodometer(float hodometer)
    {
        // RN-06: hodômetro inicial deve ser >= 0.
        if (hodometer < 0)
        {
            return "Hodometer must be greater than or equal to zero.";
        }

        return null;
    }
}
