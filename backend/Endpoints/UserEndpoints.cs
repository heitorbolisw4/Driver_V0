using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.Dtos;
using backend.Entities;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/me").RequireAuthorization();

        group.MapGet("/", GetProfile);
        group.MapPut("/password", UpdatePassword);
        group.MapPut("/email", UpdateEmail);

        return app;
    }

    private static async Task<IResult> GetProfile(ClaimsPrincipal user, AppDbContext db)
    {
        var driver = await GetCurrentDriver(user, db);
        if (driver is null)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(ToProfile(driver));
    }

    private static async Task<IResult> UpdatePassword(UpdatePasswordRequest request, ClaimsPrincipal user, AppDbContext db, IPasswordHasher passwordHasher)
    {
        var driver = await GetCurrentDriver(user, db);
        if (driver is null)
        {
            return Results.Unauthorized();
        }

        if (!passwordHasher.Verify(request.CurrentPassword, driver.Password))
        {
            return Results.BadRequest(new { message = "Current password is incorrect." });
        }

        if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
        {
            return Results.BadRequest(new { message = "New password must be at least 6 characters long." });
        }

        driver.Password = passwordHasher.Hash(request.NewPassword);
        await db.SaveChangesAsync();

        return Results.NoContent();
    }

    private static async Task<IResult> UpdateEmail(UpdateEmailRequest request, ClaimsPrincipal user, AppDbContext db, IPasswordHasher passwordHasher)
    {
        var driver = await GetCurrentDriver(user, db);
        if (driver is null)
        {
            return Results.Unauthorized();
        }

        if (!passwordHasher.Verify(request.Password, driver.Password))
        {
            return Results.BadRequest(new { message = "Password is incorrect." });
        }

        if (string.IsNullOrWhiteSpace(request.NewEmail) || !request.NewEmail.Contains('@'))
        {
            return Results.BadRequest(new { message = "A valid email is required." });
        }

        if (await db.Drivers.AnyAsync(d => d.Email == request.NewEmail && d.Id != driver.Id))
        {
            return Results.Conflict(new { message = "A driver with this email already exists." });
        }

        driver.Email = request.NewEmail;
        await db.SaveChangesAsync();

        return Results.Ok(ToProfile(driver));
    }

    private static async Task<Driver?> GetCurrentDriver(ClaimsPrincipal user, AppDbContext db)
    {
        var id = user.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (id is null || !int.TryParse(id, out var driverId))
        {
            return null;
        }

        return await db.Drivers.FindAsync(driverId);
    }

    private static DriverProfileResponse ToProfile(Driver driver)
    {
        return new DriverProfileResponse(driver.Id, driver.Name, driver.Email);
    }
}
