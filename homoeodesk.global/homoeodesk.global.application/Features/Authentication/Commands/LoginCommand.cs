using System.ComponentModel.DataAnnotations;
using HomoeoDesk.Global.Application.Common.Interfaces;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using HomoeoDesk.Global.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace HomoeoDesk.Global.Application.Features.Authentication.Commands;

public class LoginCommand : IRequest<Result<LoginResponse>>
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

public class LoginResponse
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserInfo User { get; set; } = new();
}

public class UserInfo
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
{
    private readonly IGlobalDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IMemoryCache _cache;

    public LoginCommandHandler(
        IGlobalDbContext context,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IMemoryCache cache)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _cache = cache;
    }

    public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var systemUser = await _context.SystemUsers
            .FirstOrDefaultAsync(x => x.Email == request.Email && x.IsActive, cancellationToken);

        if (systemUser == null || !_passwordHasher.VerifyPassword(request.Password, systemUser.PasswordHash))
        {
            return Result<LoginResponse>.Failure("Invalid email or password");
        }

        systemUser.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync(cancellationToken);

        var (accessToken, refreshToken, expiresAt) = await _tokenService.GenerateTokensAsync(systemUser);
        RefreshTokenStore.Save(_cache, refreshToken, systemUser.Id);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = MapUser(systemUser)
        });
    }

    internal static UserInfo MapUser(Domain.Entities.SystemUser systemUser) => new()
    {
        Id = systemUser.Id,
        Email = systemUser.Email,
        FirstName = systemUser.FirstName,
        LastName = systemUser.LastName,
        FullName = systemUser.FullName,
        Role = (UserRole)systemUser.Role
    };
}
