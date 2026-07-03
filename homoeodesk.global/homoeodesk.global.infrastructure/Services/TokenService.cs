using HomoeoDesk.Global.Application.Common.Interfaces;
using HomoeoDesk.Global.Domain.Enums;
using HomoeoDesk.Global.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace HomoeoDesk.Global.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<(string accessToken, string refreshToken, DateTime expiresAt)> GenerateTokensAsync(SystemUser user)
    {
        var jwtSettings = _configuration.GetSection("Jwt");
        var expirationDays = int.Parse(jwtSettings["ExpirationInDays"]!);
        var expiresAt = DateTime.UtcNow.AddDays(expirationDays);
        var accessToken = GenerateAccessToken(user);
        var refreshToken = GenerateRefreshToken();

        return Task.FromResult((accessToken, refreshToken, expiresAt));
    }

    public Task<(string accessToken, DateTime expiresAt)> RefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException("Refresh token functionality to be implemented");
    }

    public Task RevokeTokenAsync(string refreshToken)
    {
        throw new NotImplementedException("Token revocation functionality to be implemented");
    }

    public Task<bool> ValidateTokenAsync(string token)
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
            return Task.FromResult(true);
        }
        catch
        {
            return Task.FromResult(false);
        }
    }

    public string GenerateAccessToken(SystemUser user)
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
            new(ClaimTypes.Role, ((UserRole)user.Role).ToString()),
            new("role", user.Role.ToString())
        };

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
