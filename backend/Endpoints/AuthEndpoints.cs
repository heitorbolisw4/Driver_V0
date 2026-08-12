using backend.Data;
using backend.Dtos;
using backend.Entities;
using backend.Services;
using Microsoft.EntityFrameworkCore;

namespace backend.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/register", Register);
        app.MapPost("/login", Login);

        return app;
    }

    private static async Task<IResult> Register(RegisterRequest request, AppDbContext db, IPasswordHasher passwordHasher)
    {
        var error = ValidateRegistration(request.Name, request.Email, request.Password);
        if (error is not null)
        {
            return Results.BadRequest(new { message = error });
        }

        if (await db.Drivers.AnyAsync(d => d.Email == request.Email))
        {
            return Results.Conflict(new { message = "A driver with this email already exists." });
        }

        var driver = new Driver
        {
            Name = request.Name,
            Email = request.Email,
            Password = passwordHasher.Hash(request.Password),
            IsActive = true
        };

        db.Drivers.Add(driver);
        await db.SaveChangesAsync();

        return Results.Created($"/drivers/{driver.Id}", ToProfile(driver));
    }

    private static async Task<IResult> Login(LoginRequest request, AppDbContext db, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        var driver = await db.Drivers.SingleOrDefaultAsync(d => d.Email == request.Email);
        if (driver is null || !passwordHasher.Verify(request.Password, driver.Password))
        {
            return Results.Unauthorized();
        }

        if (!driver.IsActive)
        {
            return Results.Json(new { message = "This driver is inactive." }, statusCode: StatusCodes.Status403Forbidden);
        }

        var token = tokenService.GenerateToken(driver);

        return Results.Ok(new AuthResponse(token.Token, token.ExpiresAtUtc, ToProfile(driver)));
    }

    private static DriverProfileResponse ToProfile(Driver driver)
    {
        return new DriverProfileResponse(driver.Id, driver.Name, driver.Email);
    }

    private static string? ValidateRegistration(string name, string email, string password)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return "Name is required.";
        }

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
        {
            return "A valid email is required.";
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
        {
            return "Password must be at least 6 characters long.";
        }

        return null;
    }
}
