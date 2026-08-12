using backend.Entities;

namespace backend.Services;

public interface ITokenService
{
    TokenResult GenerateToken(Driver driver);
}
