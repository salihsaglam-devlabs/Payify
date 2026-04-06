using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserToken;

public class GetUserTokenQuery : IRequest<TokenResponse>
{
    public Guid UserId { get; set; }
    public string SecureKey { get; set; }
}

public class GetUserTokenQueryHandler : IRequestHandler<GetUserTokenQuery, TokenResponse>
{

    private readonly UserManager<User> _userManager;
    private readonly IJwtHelper _jwtHelper;
    private readonly RoleManager<Role> _roleManager;
    private readonly IHashGenerator _hashGenerator;
    private readonly IHttpContextAccessor _contextAccessor;
    private readonly IVaultClient _vaultClient;

    public GetUserTokenQueryHandler(UserManager<User> userManager,
        IJwtHelper jwtHelper,
        RoleManager<Role> roleManager,
        IHashGenerator hashGenerator,
        IHttpContextAccessor contextAccessor,
        IVaultClient vaultClient)
    {
        _userManager = userManager;
        _jwtHelper = jwtHelper;
        _roleManager = roleManager;
        _hashGenerator = hashGenerator;
        _contextAccessor = contextAccessor;
        _vaultClient = vaultClient;
    }

    public async Task<TokenResponse> Handle(GetUserTokenQuery request, CancellationToken cancellationToken)
    {
        var channel = _contextAccessor.HttpContext.Request.Headers["Gateway"];

        if(!string.IsNullOrEmpty(channel) && channel != "BackOffice")
        {
            throw new AuthorizationException(null,"UnAuthorized");
        }

        var user = await _userManager.FindByIdAsync(request.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var userFullName = $"{user.FirstName} {user.LastName}";

        var tokenHashKey = await _vaultClient.GetSecretValueAsync<string>("SharedSecrets", "ApprovalConfiguration", "TokenHashKey");

        var secureKey = _hashGenerator.Generate(userFullName + user.Id.ToString(), tokenHashKey);

        if (secureKey != request.SecureKey)
        {
            throw new AuthorizationException(String.Empty, "SecureKeyIsNotValid");
        }

        var credentials = await _jwtHelper.GenerateJwtTokenAsync(user);
        return new TokenResponse { Token = credentials };
    }
}