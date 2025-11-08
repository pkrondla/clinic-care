using ClinicCare.Application.Common.Interfaces;
using ClinicCare.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ClinicCare.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IApplicationDbContext _context;

    public TokenService(IConfiguration configuration, IApplicationDbContext context)
    {
        _configuration = configuration;
        _context = context;
    }

    public async Task<(string accessToken, string refreshToken, DateTime expiresAt)> GenerateTokensAsync(User user, int? selectedClinicId = null)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationDays = int.Parse(jwtSettings["ExpirationInDays"]!);
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("role", user.Role.ToString()),
            new("organizationId", user.OrganizationId.ToString())
        };

        if (selectedClinicId.HasValue)
        {
            claims.Add(new("clinicId", selectedClinicId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);
        var refreshToken = GenerateRefreshToken();

        // Store refresh token (you might want to create a RefreshToken entity)
        // For now, we'll return it directly

        return (accessToken, refreshToken, expiresAt);
    }

    public async Task<(string accessToken, DateTime expiresAt)> RefreshTokenAsync(string refreshToken)
    {
        // Implement refresh token validation and new token generation
        // This would typically involve checking the refresh token in the database
        // and generating a new access token if valid
        
        throw new NotImplementedException("Refresh token functionality to be implemented");
    }

    public async Task RevokeTokenAsync(string refreshToken)
    {
        // Implement token revocation
        // Mark the refresh token as revoked in the database
        
        throw new NotImplementedException("Token revocation functionality to be implemented");
    }

    public async Task<bool> ValidateTokenAsync(string token)
    {
        try
        {
            var jwtSettings = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = key,
                ClockSkew = TimeSpan.Zero
            };

            tokenHandler.ValidateToken(token, validationParameters, out _);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // Additional methods for Minimal API compatibility
    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var expirationDays = int.Parse(jwtSettings["ExpirationInDays"]!);
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.FullName),
            new("role", user.Role.ToString()),
            new("organizationId", user.OrganizationId.ToString())
        };

        if (user.SelectedClinicId.HasValue)
        {
            claims.Add(new("clinicId", user.SelectedClinicId.Value.ToString()));
        }

        var token = new JwtSecurityToken(
            issuer: jwtSettings["Issuer"],
            audience: jwtSettings["Audience"],
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
