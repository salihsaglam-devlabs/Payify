using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace LinkPara.IWallet.ApiGateway.Authorization;

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
    /// <remarks>The default is five minutes (5 minutes).</remarks>
    public TimeSpan Expiration { get; set; } = TimeSpan.FromMinutes(5);

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
