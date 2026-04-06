using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.OAuth;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LinkPara.Identity.Application.Features.Auth.Commands.GenerateToken;

public class GenerateTokenCommand : IRequest<UserTokenDto>
{
    public string ExternalCustomerId { get; set; }
    public string ExternalPersonId { get; set; }
}

public class GenerateTokenCommandHandler : IRequestHandler<GenerateTokenCommand, UserTokenDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly SignInManager<User> _signInManager;
    private readonly IUserLoginService _userLoginService;
    private readonly IJwtHelper _jwtHelper;
    private readonly IAuditLogService _auditLogService;
    private readonly IRepository<LoginWhitelist> _loginWhitelistRepository;
    private readonly IVaultClient _vaultClient;
    private readonly IParameterService _parameterService;

    public GenerateTokenCommandHandler(IRepository<User> userRepository,
        SignInManager<User> signInManager,
        IUserLoginService userLoginService,
        IJwtHelper jwtHelper,
        IAuditLogService auditLogService,
        IRepository<LoginWhitelist> loginWhitelistRepository,
        IVaultClient vaultClient,
        IParameterService parameterService)
    {
        _userRepository = userRepository;
        _signInManager = signInManager;
        _userLoginService = userLoginService;
        _jwtHelper = jwtHelper;
        _auditLogService = auditLogService;
        _loginWhitelistRepository = loginWhitelistRepository;
        _vaultClient = vaultClient;
        _parameterService = parameterService;
    }

    public async Task<UserTokenDto> Handle(GenerateTokenCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetAll()
            .Where(s => s.ExternalPersonId == request.ExternalPersonId && s.ExternalCustomerId == request.ExternalCustomerId)
            .FirstOrDefaultAsync();

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.ExternalCustomerId);
        }

        await _signInManager.SignInAsync(user, false);

        if (user.UserStatus == UserStatus.Suspended)
        {
            throw new SuspendedUserLoginException();
        }

        await CheckPilotModeLoginWhitelist(user.PhoneCode, user.PhoneNumber);

        await _userLoginService.SaveLoginInfoAsync(user, SignInResult.Success);

        var userRefreshToken = await _jwtHelper.GenerateUserRefreshTokenAsync(user);

        var accessToken = await _jwtHelper.GenerateJwtTokenAsync(user, false, userRefreshToken.Id.ToString());

        await _auditLogService.AuditLogAsync(
           new AuditLog
           {
               IsSuccess = true,
               LogDate = DateTime.Now,
               Operation = "CustomerLoginSucceeded",
               SourceApplication = "Identity",
               Resource = "User",
               UserId = user.Id,
               Details = new Dictionary<string, string>
               {
                    {"UserId", user.Id.ToString()},
                    {"UserName", user.UserName },
                    {"Email", user.Email },
               }
           });

        return new UserTokenDto
        {
            UserId = userRefreshToken.UserId,
            AccessToken = accessToken,
            RefreshToken = userRefreshToken.RefreshToken,
            RefreshTokenExpiration = userRefreshToken.RefreshTokenExpiration,
        };
    }

    private async Task CheckPilotModeLoginWhitelist(string phoneCode, string phoneNumber)
    {
        var isPilotModeEnabled = _vaultClient.GetSecretValue<bool>("IdentitySecrets", "LoginSettings", "PilotModeEnabled");
        if (!isPilotModeEnabled)
        {
            return;
        }
        var whiteListUser = await _loginWhitelistRepository
            .GetAll()
            .FirstOrDefaultAsync(s => s.PhoneNumber == phoneNumber
                && s.PhoneCode == phoneCode
                && s.RecordStatus == RecordStatus.Active);

        if (whiteListUser is null)
        {
            var pilotModeMessage = await _parameterService.GetParameterAsync("IdentityParameters", "PilotModeMessage");
            throw new PilotModeLoginFailedException(pilotModeMessage.ParameterValue);
        }
    }
}
