using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Authorization.Models;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Infrastructure.Authorization;

public class JwtHelper : IJwtHelper
{
    private readonly TokenProviderOptions _options;
    private readonly IContextProvider _currentContextProvider;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IRepository<UserSession> _userSessionTokenRepository;
    private int MaxTokenExpireInDays = 3650;
    public JwtHelper(TokenProviderOptions options,
        IContextProvider currentContextProvider,
        UserManager<User> um, 
        RoleManager<Role> rm,
        IRepository<UserSession> userSessionTokenRepository)
    {
        _options = options;
        _currentContextProvider = currentContextProvider;
        _userManager = um;
        _roleManager = rm;
        _userSessionTokenRepository = userSessionTokenRepository;
    }

    public async Task<string> GenerateJwtTokenAsync(User user, bool rememberMe = false, string sessionId = null, UserSession session = null, TimeSpan? expireIn = null)
    {
        var now = DateTime.Now;
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
            new Claim(JwtRegisteredClaimNames.Sid, await _options.NonceGenerator()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
            new Claim(ClaimKey.UserName, user.UserName),
            new Claim(ClaimKey.UserFullName, user.ToString()),
            new Claim(ClaimKey.UserType, user.UserType.ToString()),
            new Claim(ClaimKey.RememberMe, $"{rememberMe}"),
            new Claim(ClaimKey.SessionId, $"{sessionId}"),
        };
       
        var roleNames = await _userManager.GetRolesAsync(user);
         
        if (roleNames.Any())
        {
            foreach (var roleName in roleNames)
            {
                claims.Add(new Claim(ClaimTypes.Role, roleName));

                var role = await _roleManager.FindByNameAsync(roleName);

                claims.Add(new Claim("RoleId", role.Id.ToString()));
                claims.Add(new Claim("RoleType", role.RoleScope.ToString()));
                claims.Add(new Claim("CanSeeSensitiveData", role.CanSeeSensitiveData.ToString()));

                if (role is not null)
                {
                    var roleClaims = await _roleManager.GetClaimsAsync(role);
                    foreach (var claim in roleClaims)
                    {
                        if (!claims.Any(s => s.Value == claim.Value))
                        {
                            claims.Add(claim);
                        }
                    }
                }
            }
        }

        if (user.UserType == UserType.ApplicationUser)
        {
            expireIn = TimeSpan.FromDays(MaxTokenExpireInDays);
        }


        // Create the JWT and write it to a string
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: now.Add(expireIn ??_options.Expiration),
            signingCredentials: _options.SigningCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwt);
    }
    public async Task<UserSession> GenerateUserRefreshTokenAsync(User user)
    {
        var existRefreshTokens = await _userSessionTokenRepository.GetAll()
            .Where(q => q.UserId == user.Id && q.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        if (existRefreshTokens.Any())
        {
            foreach (var token in existRefreshTokens)
            {
                await _userSessionTokenRepository.DeleteAsync(token);
            }
        }

        var refreshToken = CreateRefreshToken();

        var userRefreshToken = new UserSession()
        {
            RefreshToken = refreshToken,
            UserId = user.Id,
            CreatedBy = user.Id.ToString(),
        };

        switch (_currentContextProvider.CurrentContext.Channel)
        {
            case "Web":
                userRefreshToken.RefreshTokenExpiration = DateTime.UtcNow.Add(_options.WebRefreshTokenExpiration);
                break;
            case "Backoffice":
                userRefreshToken.RefreshTokenExpiration = DateTime.UtcNow.Add(_options.BackofficeRefreshTokenExpiration);
                break;
            case "MerchantPortal":
                userRefreshToken.RefreshTokenExpiration = DateTime.UtcNow.Add(_options.MerchantRefreshTokenExpiration);
                break;
            default:
                userRefreshToken.RefreshTokenExpiration = DateTime.UtcNow.Add(_options.WebRefreshTokenExpiration);
                break;
        }

        await _userSessionTokenRepository.AddAsync(userRefreshToken);

        return userRefreshToken;
    }
    private string CreateRefreshToken()
    {
        var numberByte = new byte[32];

        using var rnd = RandomNumberGenerator.Create();

        rnd.GetBytes(numberByte);

        return Convert.ToBase64String(numberByte);

    }
}