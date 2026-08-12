using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class RouteEndpoints
{
    public static IEndpointRouteBuilder MapRouteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/routes").RequireAuthorization();

        group.MapGet("/", GetAll);
        group.MapGet("/{id:int}", GetById);
        group.MapPost("/", Create);
        group.MapPut("/{id:int}", Update);
        group.MapPost("/{id:int}/close", Close);
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

        var routes = await db.Routes
            .Include(r => r.Expenses)
            .Where(r => r.DriverId == driverId)
            .ToListAsync();

        return Results.Ok(routes.Select(ToResponse));
    }

    private static async Task<IResult> GetById(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var route = await FindOwned(db, id, driverId.Value);
        if (route is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(ToResponse(route));
    }

    private static async Task<IResult> Create(CreateRouteRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var error = ValidateRouteName(request.RouteName) ?? ValidateKilometers(request.Kilometers);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        // RN-09: exige um caminhão cadastrado e pertencente ao motorista.
        var truck = await db.Trucks.SingleOrDefaultAsync(t => t.Id == request.TruckId && t.DriverId == driverId);
        if (truck is null)
        {
            return Results.BadRequest(new { message = "Truck not found." });
        }

        // RN-08/RN-11: um caminhão só pode ter uma rota em andamento por vez.
        if (await db.Routes.AnyAsync(r => r.TruckId == truck.Id && r.InProcess))
        {
            return Results.Conflict(new { message = "This truck already has a route in process." });
        }

        if (request.TruckLoadId is not null &&
            !await db.TruckLoads.AnyAsync(tl => tl.Id == request.TruckLoadId && tl.DriverId == driverId))
        {
            return Results.BadRequest(new { message = "TruckLoad not found." });
        }

        var route = new Route
        {
            RouteName = request.RouteName,
            Kilometers = request.Kilometers,
            StartOdometer = truck.Hodometer, // RN-10
            StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
            InProcess = true,
            DriverId = driverId.Value,
            TruckId = truck.Id,
            TruckLoadId = request.TruckLoadId
        };

        db.Routes.Add(route);
        await db.SaveChangesAsync();

        return Results.Created($"/routes/{route.Id}", ToResponse(route));
    }

    private static async Task<IResult> Update(int id, UpdateRouteRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var route = await FindOwned(db, id, driverId.Value);
        if (route is null)
        {
            return Results.NotFound();
        }

        var error = ValidateRouteName(request.RouteName) ?? ValidateKilometers(request.Kilometers);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (request.TruckLoadId is not null &&
            !await db.TruckLoads.AnyAsync(tl => tl.Id == request.TruckLoadId && tl.DriverId == driverId))
        {
            return Results.BadRequest(new { message = "TruckLoad not found." });
        }

        // RN-14: rota segue editável mesmo já concluída.
        route.RouteName = request.RouteName;
        route.Kilometers = request.Kilometers;
        route.TruckLoadId = request.TruckLoadId;
        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(route));
    }

    private static async Task<IResult> Close(int id, CloseRouteRequest request, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var route = await FindOwned(db, id, driverId.Value);
        if (route is null)
        {
            return Results.NotFound();
        }

        if (!route.InProcess)
        {
            return Results.BadRequest(new { message = "Route is already closed." });
        }

        if (route.TruckId is null)
        {
            return Results.BadRequest(new { message = "Route has no truck assigned; cannot close." });
        }

        // RN-13: Km Rodado = leitura final - hodômetro de abertura; não pode ser <= 0.
        var kilometersCovered = request.FinalOdometer - route.StartOdometer;
        if (kilometersCovered <= 0)
        {
            return Results.BadRequest(new { message = "Final odometer must be greater than the route's start odometer." });
        }

        var truck = await db.Trucks.FindAsync(route.TruckId.Value);

        route.KilometersCovered = kilometersCovered;
        route.EndDate = DateOnly.FromDateTime(DateTime.UtcNow);
        route.InProcess = false;

        // RN-07: hodômetro do caminhão só é atualizado ao encerrar a rota.
        truck!.Hodometer = request.FinalOdometer;

        // RN-21: leitura de hodômetro gerada automaticamente no encerramento.
        db.OdometerReadings.Add(new OdometerReading
        {
            TruckId = truck.Id,
            Value = request.FinalOdometer,
            ReadingDate = route.EndDate.Value,
            Note = request.Note
        });

        await db.SaveChangesAsync();

        return Results.Ok(ToResponse(route));
    }

    private static async Task<IResult> Delete(int id, ClaimsPrincipal user, AppDbContext db)
    {
        var driverId = GetCurrentDriverId(user);
        if (driverId is null)
        {
            return Results.Unauthorized();
        }

        var route = await FindOwned(db, id, driverId.Value);
        if (route is null)
        {
            return Results.NotFound();
        }

        db.Routes.Remove(route);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static Task<Route?> FindOwned(AppDbContext db, int id, int driverId)
    {
        return db.Routes
            .Include(r => r.Expenses)
            .SingleOrDefaultAsync(r => r.Id == id && r.DriverId == driverId);
    }

    private static int? GetCurrentDriverId(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        return id is not null && int.TryParse(id, out var driverId) ? driverId : null;
    }

    private static RouteResponse ToResponse(Route route)
    {
        // Seção 5: % Aproveitamento = (Km Acordado ÷ Km Rodado) × 100. Gasto Real
        // (RN-19) é só informativo, não entra nesse cálculo.
        float? utilization = route.KilometersCovered is float covered && covered > 0
            ? route.Kilometers / covered * 100f
            : null;

        return new RouteResponse(
            route.Id,
            route.RouteName,
            route.Kilometers,
            route.KilometersCovered,
            route.StartOdometer,
            route.StartDate,
            route.EndDate,
            route.InProcess,
            route.TruckId,
            route.TruckLoadId,
            route.Expenses.Sum(e => e.Value),
            utilization);
    }

    private static string? ValidateRouteName(string routeName)
    {
        if (string.IsNullOrWhiteSpace(routeName))
        {
            return "RouteName is required.";
        }

        return null;
    }

    private static string? ValidateKilometers(float kilometers)
    {
        // RN-09: Km Acordado deve ser maior que zero.
        if (kilometers <= 0)
        {
            return "Kilometers must be greater than zero.";
        }

        return null;
    }
}
