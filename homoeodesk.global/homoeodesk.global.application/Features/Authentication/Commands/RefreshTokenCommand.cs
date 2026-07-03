using HomoeoDesk.Global.Application.Common.Interfaces;
using HomoeoDesk.Global.Application.Common.Interfaces.Global;
using HomoeoDesk.Global.Application.Common.Models;
using MediatR;
using Microsoft.Extensions.Caching.Memory;

namespace HomoeoDesk.Global.Application.Features.Authentication.Commands;

public class RefreshTokenCommand : IRequest<Result<LoginResponse>>
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<LoginResponse>>
{
    private readonly IGlobalDbContext _context;
    private readonly ITokenService _tokenService;
    private readonly IMemoryCache _cache;

    public RefreshTokenCommandHandler(
        IGlobalDbContext context,
        ITokenService tokenService,
        IMemoryCache cache)
    {
        _context = context;
        _tokenService = tokenService;
        _cache = cache;
    }

    public async Task<Result<LoginResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken)
            || !_cache.TryGetValue(RefreshTokenStore.Key(request.RefreshToken), out int userId))
        {
            return Result<LoginResponse>.Failure("Invalid refresh token");
        }

        var systemUser = await _context.SystemUsers.FindAsync(new object[] { userId }, cancellationToken);
        if (systemUser == null || !systemUser.IsActive)
        {
            return Result<LoginResponse>.Failure("User not found");
        }

        _cache.Remove(RefreshTokenStore.Key(request.RefreshToken));

        var (accessToken, refreshToken, expiresAt) = await _tokenService.GenerateTokensAsync(systemUser);
        RefreshTokenStore.Save(_cache, refreshToken, systemUser.Id);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresAt = expiresAt,
            User = LoginCommandHandler.MapUser(systemUser)
        });
    }
}

public class LogoutCommand : IRequest<Result>
{
    public string? RefreshToken { get; set; }
}

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IMemoryCache _cache;

    public LogoutCommandHandler(IMemoryCache cache) => _cache = cache;

    public Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            _cache.Remove(RefreshTokenStore.Key(request.RefreshToken));
        }
        return Task.FromResult(Result.Success());
    }
}

public static class RefreshTokenStore
{
    private static readonly TimeSpan Ttl = TimeSpan.FromDays(14);

    public static string Key(string token) => $"global-refresh:{token}";

    public static void Save(IMemoryCache cache, string token, int userId) =>
        cache.Set(Key(token), userId, Ttl);
}
