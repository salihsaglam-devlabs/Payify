using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Infrastructure.Authorization;

public class TokenProviderOptions
{
    /// <summary>
    ///  The Issuer (iss) claim for generated tokens.
    /// </summary>
    public string Issuer { get; set; }

    /// <summary>
    /// The Audience (aud) claim for the generated tokens.
    /// </summary>
    public string Audience { get; set; }

    /// <summary>
    /// The expiration time for the generated tokens.
    /// </summary>
    /// <remarks>The default is 20 minutes.</remarks>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);
    /// <summary>
    /// The expiration time for the generated tokens.
    /// </summary>
    /// <remarks>The default is 6 minutes.</remarks>
    /// 
    public TimeSpan RefreshTokenExpiration { get; set; } = TimeSpan.FromMinutes(6);
    /// <summary>
    /// The expiration time for the generated tokens.
    /// </summary>
    /// <remarks>The default is 6 minutes.</remarks>
    /// 
    public TimeSpan WebRefreshTokenExpiration { get; set; } = TimeSpan.FromMinutes(6);
    /// <summary>
    /// The expiration time for the generated tokens.
    /// </summary>
    /// <remarks>The default is 30 minutes.</remarks>
    public TimeSpan BackofficeRefreshTokenExpiration { get; set; } = TimeSpan.FromMinutes(30);
    /// <summary>
    /// The expiration time for the generated tokens.
    /// </summary>
    /// <remarks>The default is 30 minutes.</remarks>
    public TimeSpan MerchantRefreshTokenExpiration { get; set; } = TimeSpan.FromMinutes(30);
    /// <summary>
    /// The signing key to use when generating tokens.
    /// </summary>
    public SigningCredentials SigningCredentials { get; set; }

    /// <summary>
    /// Resolves a user identity given a username and password.
    /// </summary>
    public Func<string, string, Task<ClaimsIdentity>> IdentityResolver { get; set; }

    /// <summary>
    /// Enable otp login
    /// </summary>
    public bool IsOtpTokenPathEnabled { get; set; }

    /// <summary>
    /// Generates a random value (nonce) for each generated token.
    /// </summary>
    /// <remarks>The default nonce is a random GUID.</remarks>
    public Func<Task<string>> NonceGenerator { get; set; }
        = () => Task.FromResult(Guid.NewGuid().ToString());
}