using HomoeoDesk.Global.Domain.Entities;

namespace HomoeoDesk.Global.Application.Common.Interfaces;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken, DateTime expiresAt)> GenerateTokensAsync(SystemUser user);
    Task<(string accessToken, DateTime expiresAt)> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    string GenerateAccessToken(SystemUser user);
    string GenerateRefreshToken();
}
