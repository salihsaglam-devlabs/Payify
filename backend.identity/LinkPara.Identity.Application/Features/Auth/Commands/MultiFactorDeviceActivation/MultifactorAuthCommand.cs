using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.Notification;
using LinkPara.HttpProviders.Notification.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.Auth.Commands.MultiFactorDeviceActivation
{
    public class MultifactorAuthCommand : IRequest
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
    }
    public class MultifactorAuthCommandHandler : IRequestHandler<MultifactorAuthCommand>
    {
        private readonly IMultiFactorService _multiFactorService;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IStringLocalizer _localizer;
        private readonly INotificationService _notificationService;
        private readonly IUserLoginService _userLoginService;
        private readonly IVaultClient _vaultClient;

        public MultifactorAuthCommandHandler(
            IMultiFactorService multiFactorService,
            SignInManager<User> signInManager,
            UserManager<User> userManager,
            IStringLocalizerFactory localizerFactory,
            INotificationService notificationService,
            IVaultClient vaultClient, 
            IUserLoginService userLoginService)
        {
            _multiFactorService = multiFactorService;
            _signInManager = signInManager;
            _userManager = userManager;
            _localizer = localizerFactory.Create("Exceptions", "LinkPara.Identity.API");
            _notificationService = notificationService;
            _vaultClient = vaultClient;
            _userLoginService = userLoginService;
        }
        public async Task<Unit> Handle(MultifactorAuthCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(request.Username);

            if (user is null)
            {
                throw new WrongUsernamePasswordException();
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, true);

            await _userLoginService.SaveLoginInfoAsync(user, result);
            
            if (result.IsLockedOut && user is { LockoutEnd: { } }) 
            {
                throw new LockedOutException(user.LockoutEnd.Value.DateTime); 
            }
            
            if (!result.Succeeded)
            {
                var maxFailedAccessAttempts = _vaultClient.GetSecretValue<int>("IdentitySecrets", "LockoutSettings", "MaxFailedAccessAttempts");
                
                if(user.AccessFailedCount >= maxFailedAccessAttempts)
                {
                    throw new LockedOutException(user.LockoutEnd.Value.DateTime);
                }
                
                throw new LoginFailedException(ApiErrorCode.LoginFailed, _localizer.GetString("LoginFailedException"));
            }

            var response = await _multiFactorService.SendActivationOtpAsync(new ActivationOtpRequest
            {
                Username = request.Username,
                Password = request.Password,
                PhoneNumber = string.Concat(user.PhoneCode, user.PhoneNumber)
            });

            if (response.Success)
            {
                string[] receiver = { string.Concat(user.PhoneCode, user.PhoneNumber).Replace("+", string.Empty) };
                var smsResult = await _notificationService.SendSmsNotificationAsync(new SmsRequest
                {
                    To = receiver,
                    IsOtp = true,
                    TemplateName = "PWFActivationOTP",
                    TemplateParameters = new Dictionary<string, string>
                        {
                            { "code", response.Otp },
                        }
                });
                if (!smsResult.Success)
                {
                    throw new CustomApiException(smsResult.ResponseCode, smsResult.ResponseMessage);
                }
            }

            return Unit.Value;
        }
    }
}