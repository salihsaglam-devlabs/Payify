using LinkPara.CampaignManagement.Application.Commons.Interfaces.Authorization;
using LinkPara.CampaignManagement.Application.Features.IWalletLogins;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace LinkPara.CampaignManagement.Infrastructure.Services.Authorization;

public class JwtHelper : IJwtHelper
{
    private readonly TokenProviderOptions _options;

    public JwtHelper(TokenProviderOptions options)
    {
        _options = options;
    }

    public async Task<LoginResponseDto> GenerateJwtTokenAsync(string id, TimeSpan? expireIn = null)
    {
        var now = DateTime.Now;
        var expireDate = now.Add(expireIn ?? _options.Expiration);
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, id),
            new Claim(JwtRegisteredClaimNames.Jti, await _options.NonceGenerator()),
            new Claim(JwtRegisteredClaimNames.Sid, await _options.NonceGenerator()),
            new Claim(JwtRegisteredClaimNames.Iat, new DateTimeOffset(now).ToUniversalTime().ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        // Create the JWT and write it to a string
        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now,
            expires: expireDate,
            signingCredentials: _options.SigningCredentials);

        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new LoginResponseDto
        {
            Token = token,
            ExpireDate = expireDate,
            TokenType = "Bearer"
        };
    }
}
