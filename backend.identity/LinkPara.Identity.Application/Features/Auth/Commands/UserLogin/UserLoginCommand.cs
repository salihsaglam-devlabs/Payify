using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.IdentityConfiguration;
using LinkPara.Identity.Application.Features.OAuth;
using LinkPara.Identity.Application.Features.SecurityPictures.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.Auth.Commands.UserLogin
{
    public class UserLoginCommand : IRequest<UserTokenDto>
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
        public string LoginOtp { get; set; }
    }

    public class LoginUserCommandHandler : IRequestHandler<UserLoginCommand, UserTokenDto>
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IUserLoginService _userLoginService;
        private readonly IJwtHelper _jwtHelper;
        private readonly IAuditLogService _auditLogService;
        private readonly IRepository<LoginWhitelist> _loginWhitelistRepository;
        private readonly IVaultClient _vaultClient;
        private readonly IStringLocalizer _localizer;
        private readonly IMultiFactorService _multiFactorService;
        private readonly IParameterService _parameterService;
        private readonly IRepository<UserSecurityPicture> _userSecurityPictureRepository;

        public LoginUserCommandHandler(UserManager<User> userManager,
            SignInManager<User> signInManager,
            IUserLoginService userLoginService,
            IJwtHelper jwtHelper,
            IAuditLogService auditLogService,
            IRepository<LoginWhitelist> loginWhitelistRepository,
            IVaultClient vaultClient,
            IStringLocalizerFactory localizerFactory,
            IMultiFactorService multiFactorService,
            IParameterService parameterService, 
            IRepository<UserSecurityPicture> userSecurityPictureRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _userLoginService = userLoginService;
            _jwtHelper = jwtHelper;
            _auditLogService = auditLogService;
            _loginWhitelistRepository = loginWhitelistRepository;
            _vaultClient = vaultClient;
            _multiFactorService = multiFactorService;
            _parameterService = parameterService;
            _userSecurityPictureRepository = userSecurityPictureRepository;
            _localizer = localizerFactory.Create("Exceptions", "LinkPara.Identity.API");
        }

        public async Task<UserTokenDto> Handle(UserLoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user is null)
            {
                throw new WrongUsernamePasswordException();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            if (result.Succeeded && user.UserStatus == UserStatus.Suspended)
            {
                throw new SuspendedUserLoginException();
            }

            string accessToken = "";
            var userRefreshToken = new UserSession();

            switch (user.UserType)
            {
                case UserType.Individual:
                    userRefreshToken = await _jwtHelper.GenerateUserRefreshTokenAsync(user);
                    accessToken = await LoginIndividualUserAsync(user, result, request, userRefreshToken.Id);
                    break;
                case UserType.Internal:
                case UserType.Corporate:
                case UserType.CorporateSubMerchant:
                case UserType.Representative:
                case UserType.Branch:
                case UserType.CorporateWallet:
                    userRefreshToken = await _jwtHelper.GenerateUserRefreshTokenAsync(user);
                    accessToken = await LoginUserAsync(user, result, request, userRefreshToken.Id);
                    break;
                case UserType.ApplicationUser:
                    accessToken = await LoginAppUserAsync(user, result, request);
                    break;
            }
            
            var userSecurityPictureState =
                _vaultClient.GetSecretValue<UserSecurityPictureState>(
                    "IdentitySecrets",
                    "UserSecurityPictureState") 
                ?? new UserSecurityPictureState();      
            var userSecurityPicture = new SecurityPictureDto();
            bool userSecurityPictureEnabled = false;
            switch (user.UserType)
            {
                case UserType.Corporate:
                case UserType.CorporateSubMerchant:
                    if (userSecurityPictureState.MerchantEnabled)
                    {
                         userSecurityPicture = await _userSecurityPictureRepository.GetAll()
                            .Where(x => x.UserId == user.Id && x.RecordStatus == RecordStatus.Active)
                            .Select(x => new SecurityPictureDto
                            {
                                Name = x.SecurityPicture.Name,
                                Bytes = x.SecurityPicture.Bytes,
                            })
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                    userSecurityPictureEnabled = userSecurityPictureState.MerchantEnabled;
                    break;

            }
            
            await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = true,
                   LogDate = DateTime.Now,
                   Operation = "UserLoginSucceeded ",
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
                UserSecurityPicture = userSecurityPicture?.Bytes ,
                UserSecurityPictureEnabled = userSecurityPictureEnabled,
            };
        }


        private async Task<string> LoginIndividualUserAsync(User user, SignInResult result, UserLoginCommand request, Guid sessionId)
        {
            await CheckPilotModeLoginWhitelist(user.PhoneCode, user.PhoneNumber);

            await _userLoginService.SaveLoginInfoAsync(user, result);

            await ValidateSignInResult(result, user);

            await ValidatePasswordExpiry(user);

            if (request.LoginOtp is not null)
            {
                var customerId = string.Concat(user.PhoneCode, user.PhoneNumber).Replace("+", string.Empty);

                var pfsResponse = await _multiFactorService.VerifyLoginOtpAsync(new VerifyLoginRequest
                {
                    LoginOtp = request.LoginOtp,
                    PhoneNumber = customerId
                });

                if (!pfsResponse.Success)
                {
                    var responseMessage = _localizer.GetString("MultifactorAuthException");
                    throw new LoginFailedException(ApiErrorCode.LoginFailed, responseMessage);
                }
            }

            return await _jwtHelper.GenerateJwtTokenAsync(user, request.RememberMe, sessionId.ToString());
        }

        private async Task<string> LoginAppUserAsync(User user, SignInResult result, UserLoginCommand request)
        {
            var credentials = await _jwtHelper.GenerateJwtTokenAsync(user, request.RememberMe);

            return credentials;
        }
        private async Task<string> LoginUserAsync(User user, SignInResult result, UserLoginCommand request, Guid sessionId)
        {
            await _userLoginService.SaveLoginInfoAsync(user, result);

            await ValidateSignInResult(result, user);

            await ValidatePasswordExpiry(user);

            return await _jwtHelper.GenerateJwtTokenAsync(user, request.RememberMe, sessionId.ToString());
        }

        private async Task ValidateSignInResult(SignInResult result, User user)
        {
            if (result.IsLockedOut && user is { LockoutEnd: { } })
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "UserLoginFailed",
                        SourceApplication = "Identity",
                        Resource = "User",
                        UserId = user.Id,
                        Details = new Dictionary<string, string>
                        {
                            {"UserId", user.Id.ToString()},
                            {"UserName", user.UserName },
                            {"Email", user.Email },
                            {"ErrorMessage" , "LockedOutException"}
                        }
                    });

                var autoUnlock = _vaultClient.GetSecretValue<bool>("IdentitySecrets", "LockoutSettings", "AutoUnlock");

                if (autoUnlock)
                {
                    throw new LockedOutException((DateTimeOffset)user.LockoutEnd);
                }

                throw new PermanentLockedOutException();
            }

            if (!result.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                   new AuditLog
                   {
                       IsSuccess = false,
                       LogDate = DateTime.Now,
                       Operation = "UserLoginFailed",
                       SourceApplication = "Identity",
                       Resource = "User",
                       UserId = user.Id,
                       Details = new Dictionary<string, string>
                       {
                        {"UserId", user.Id.ToString()},
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , "LoginFailedException"}
                       }
                   });

                throw new LoginFailedException(ApiErrorCode.LoginFailed, _localizer.GetString("LoginFailedException"));
            }
        }

        private async Task ValidatePasswordExpiry(User user)
        {
            var passwordExpiredDateInDays = _vaultClient.GetSecretValue<int>("IdentitySecrets", "PasswordSettings", "PasswordExpiredDateInDays");

            if (DateTime.Now > user.PasswordModifiedDate.AddDays(passwordExpiredDateInDays))
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "UserLoginFailed",
                        SourceApplication = "Identity",
                        Resource = "User",
                        UserId = user.Id,
                        Details = new Dictionary<string, string>
                        {
                            {"UserId", user.Id.ToString()},
                            {"UserName", user.UserName },
                            {"Email", user.Email },
                            {"ErrorMessage" , "PasswordExpiredException"}
                        }
                    });

                throw new PasswordExpiredException();
            }
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
}