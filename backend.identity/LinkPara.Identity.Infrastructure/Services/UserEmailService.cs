using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using MassTransit;
using MassTransit.Internals;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Infrastructure.Services
{
    public class UserEmailService : IUserEmailService
    {
        private readonly UserManager<User> _userManager;
        private readonly IRepository<User> _userRepository;
        private readonly IEmailSender _emailSender;
        private readonly IVaultClient _vaultClient;
        private readonly IBus _bus;

        public UserEmailService(UserManager<User> userManager,
            IRepository<User> repository,
            IEmailSender emailSender,
            IVaultClient vaultClient,
            IBus bus)
        {
            _userManager = userManager;
            _userRepository = repository;
            _emailSender = emailSender;
            _vaultClient = vaultClient;
            _bus = bus;
        }
        public async Task SendEmailVerificationMailAsync(User user)
        {
            var mailParameters = new Dictionary<string, string>();
            var emailConfirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Base64UrlEncoder.Encode(emailConfirmationToken);

            var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Merchant")}/account/verify-email?token={encodedToken}&username={user.UserName}";
            mailParameters.Add("link", link);
            mailParameters.Add("username", $"{user.PhoneCode}{user.PhoneNumber}");
            await _emailSender.SendEmailAsync(new SendEmail
            {
                ToEmail = user.Email,
                TemplateName = "CorporateUserEmailVerify",
                DynamicTemplateData = mailParameters
            });
        }
        public async Task SendEmailChangeMailAsync(User user,string newMail)
        {
            var emailChangeToken= await _userManager.GenerateChangeEmailTokenAsync(user, newMail);
            var encodedToken = Base64UrlEncoder.Encode(emailChangeToken);
            var mailParameters = new Dictionary<string, string>();

            var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Merchant")}/users/change-email?token={encodedToken}&username={user.UserName}";
            mailParameters.Add("link", link);
            mailParameters.Add("username", $"{user.PhoneCode}{user.PhoneNumber}");
            await _emailSender.SendEmailAsync(new SendEmail
            {
                ToEmail = user.Email,
                TemplateName = "CorporateUserEmailChange",
                DynamicTemplateData = mailParameters
            });
        }
        public async Task<bool> VerifyEmailChangeTokenAsync(VerifyEmailChangeCommand command, CancellationToken cancellationToken)
        {
           

            var user = await _userManager.FindByNameAsync(command.Username);

            if (user == null)
            {
                throw new UserNotFoundException();
            }
            var decodedToken = Base64UrlEncoder.Decode(command.Token);
            var emailChangeResult= await _userManager.ChangeEmailAsync(user, command.NewMail, decodedToken);

            if (emailChangeResult == null || !emailChangeResult.Succeeded) 
                return false;
            return true;
        }
        public async Task<bool> VerifyEmailTokenAsync(VerifyEmailCommand command, CancellationToken cancellationToken)
        {

            var user = await _userRepository.GetAll()
            .Where(q => q.UserName == command.Username)
            .FirstOrDefaultAsync(cancellationToken);

            var isAlreadyConfirmed = user.EmailConfirmed;
            if (user == null)
            {
                throw new UserNotFoundException();
            }
            if (!isAlreadyConfirmed)
            {
                var decodedToken = Base64UrlEncoder.Decode(command.Token);
                var confirmResult = await _userManager.ConfirmEmailAsync(user, decodedToken);

                if (confirmResult == null || !confirmResult.Succeeded)
                    return false;

                user.EmailConfirmed = confirmResult.Succeeded;
            await _userRepository.UpdateAsync(user);
                var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                Dictionary<string, string> mailParameters = new()
            {
            { "ResetPasswordToken", Base64UrlEncoder.Encode(resetPasswordToken) }
            };
                await SendCorporateUserOnboardingMailAsync(user, mailParameters);
            }
       
            return true;
        }

        public async Task SendCorporateUserOnboardingMailAsync(User user, Dictionary<string, string> mailParameters)
        {
            var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Merchant")}/account/reset-password?token={mailParameters["ResetPasswordToken"]}&username={user.UserName}";

            await _bus.Publish(new UserOnboarding
            {
                UserId = user.Id,
                Link = link,
                Username = $"{user.PhoneCode}{user.PhoneNumber}"
            });
        }
    }
}
