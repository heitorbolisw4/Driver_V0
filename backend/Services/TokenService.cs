using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Entities;
using Microsoft.IdentityModel.Tokens;

namespace backend.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public TokenResult GenerateToken(Driver driver)
    {
        var jwtSection = configuration.GetSection("Jwt");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, driver.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, driver.Email),
            new(ClaimTypes.Name, driver.Name)
        };

        var expiresAtUtc = DateTime.UtcNow.AddMinutes(jwtSection.GetValue<double>("ExpirationInMinutes"));

        var token = new JwtSecurityToken(
            issuer: jwtSection["Issuer"],
            audience: jwtSection["Audience"],
            claims: claims,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new TokenResult(tokenString, expiresAtUtc);
    }
}
