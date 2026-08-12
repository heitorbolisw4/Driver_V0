using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class OdometerReadingEndpoints
{
    public static IEndpointRouteBuilder MapOdometerReadingEndpoints(this IEndpointRouteBuilder app)
    {
        // RN-21/22: leituras só são criadas automaticamente ao encerrar uma rota
        // (ver RouteEndpoints.Close) — por isso não há POST aqui.
        var group = app.MapGroup("/odometer-readings").RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPut("/{id:int}", Update);

        return app;
    }

    private static async Task<IResult> GetAll(int? truckId, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var query = db.OdometerReadings.Where(o => o.Truck.DriverId == driverId);

        if (truckId is not null)
        {
            query = query.Where(o => o.TruckId == truckId);
        }

        var readings = await query
            .OrderByDescending(o => o.ReadingDate)
            .ToListAsync();

        return Results.Ok(readings.Select(ToResponse));
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var reading = await FindOwned(db, id, driverId.Value);
        if (reading is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(reading));
    }

    private static async Task<IResult> Update(int id, UpdateOdometerReadingRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var reading = await FindOwned(db, id, driverId.Value);
        if (reading is null)
        {
            return Results.NotFound();
        }

        reading.Note = request.Note;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(reading));
    }

    private static Task<OdometerReading?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.OdometerReadings.SingleOrDefaultAsync(o => o.Id == id && o.Truck.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static OdometerReadingResponse ToResponse(OdometerReading reading)
    {
        return new OdometerReadingResponse(reading.Id, reading.TruckId, reading.Value, reading.ReadingDate, reading.Note);
    }
}
