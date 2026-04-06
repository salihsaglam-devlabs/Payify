using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Application.Features.Account.Commands.ForgotPassword;

public class ForgotPasswordCommand : IRequest
{
    public string Token { get; set; }
    public string NewPassword { get; set; }
    public string UserName { get; set; }
}

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMultiFactorService _multiFactorService;

    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(UserManager<User> userManager,
        IAuditLogService auditLogService,
        IPasswordHistoryService passwordHistoryService,
            IMultiFactorService multiFactorService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _auditLogService = auditLogService;
        _multiFactorService = multiFactorService;
        _passwordHistoryService = passwordHistoryService;
        _logger = logger;
    }

    public async Task<Unit> Handle(ForgotPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.UserName);

        if (user is null)
        {
            _logger.LogError($"ForgotPassword Error -> NotFoundException User:  {command.UserName}");
            throw new InvalidInputException();
        }

        if (!(await ValidatePasswordHistoryAsync(user, command.NewPassword)))
        {
            _logger.LogError($"ForgotPassword Error -> PasswordHistoryRequirementException ");
            throw new InvalidInputException();
        }

        if (ContainsBirthDate(user, command.NewPassword))
        {
            _logger.LogError($"ForgotPassword Error -> PasswordContainsBirthDateException ");
            throw new InvalidInputException();
        }

        var decodedToken = Base64UrlEncoder.Decode(command.Token);
        var result = await _userManager.ResetPasswordAsync(user, decodedToken, command.NewPassword);
        if (!result.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "ForgotPassword",
                    SourceApplication = "Identity",
                    Resource = "User",
                    UserId = user.Id,
                    Details = new Dictionary<string, string>
                    {
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , result.Errors.FirstOrDefault()?.Description}
                    }
                });

            var errorMessages = string.Join(",", result.Errors.Select(s => s.Description).ToList());
            
            _logger.LogError($"ForgotPassword Error ExpireResetPasswordTokenException : {errorMessages}");
            
            throw new InvalidInputException();
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "ForgotPassword",
                SourceApplication = "Identity",
                Resource = "User",
                UserId = user.Id,
                Details = new Dictionary<string, string>
                 {
                       {"UserName", user.UserName },
                       {"Email", user.Email },
                 }
            });

        await _passwordHistoryService.SavePasswordAsync(user, user.PasswordHash);

        user.PasswordModifiedDate = DateTime.Now;
        user.LockoutEnd = null;
        await _userManager.UpdateAsync(user);

        var formattedPhoneNumber = long.Parse(user.PhoneCode.Replace("+", "") + user.PhoneNumber);

        if (user.UserType == UserType.Individual)
        {
            await _multiFactorService.UpdateActivationPINByCustomerIdAsync(new UpdateActivationPINByCustomerIdRequest
            {
                CustomerId = formattedPhoneNumber,
                PIN = command.NewPassword,
            });
        }

        return Unit.Value;
    }

    private async Task<bool> ValidatePasswordHistoryAsync(User user, string password)
    {
        var oldPasswords = await _passwordHistoryService.GetOldPasswordsAsync(user);

        if (oldPasswords.Count > 3)
        {
            oldPasswords = oldPasswords.OrderByDescending(x => x.CreateDate).Take(3).ToList();
        }

        foreach (var oldPassword in oldPasswords)
        {
            var oldPasswordVerifiedResult = _userManager.PasswordHasher.VerifyHashedPassword(user, oldPassword.PasswordHash, password);

            if (oldPasswordVerifiedResult is not PasswordVerificationResult.Failed)
            {
                return false;
            }
        }
        return true;
    }

    private bool ContainsBirthDate(User user, string password)
    {
        var birthDate = user.BirthDate;

        if (birthDate.Year == 1)
        {
            return false;
        }

        return password.Contains(birthDate.Year.ToString()) ||
            password.Contains($"{birthDate.Day}{birthDate.Month}{birthDate.Year.ToString()[2..]}") ||
            password.Contains($"{(birthDate.Day < 10 ? $"0{birthDate.Day}" : birthDate.Day)}{(birthDate.Month < 10 ? $"0{birthDate.Month}" : birthDate.Month)}{birthDate.Year.ToString()[2..]}");
    }

}