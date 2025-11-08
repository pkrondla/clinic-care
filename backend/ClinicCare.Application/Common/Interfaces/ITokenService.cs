using ClinicCare.Domain.Entities;

namespace ClinicCare.Application.Common.Interfaces;

public interface ITokenService
{
    Task<(string accessToken, string refreshToken, DateTime expiresAt)> GenerateTokensAsync(User user, int? selectedClinicId = null);
    Task<(string accessToken, DateTime expiresAt)> RefreshTokenAsync(string refreshToken);
    Task RevokeTokenAsync(string refreshToken);
    Task<bool> ValidateTokenAsync(string token);
    
    // Additional methods for Minimal API compatibility
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
