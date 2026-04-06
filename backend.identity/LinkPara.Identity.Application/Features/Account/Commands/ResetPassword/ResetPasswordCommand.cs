using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.MultiFactor;
using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels.Request;
using LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using MassTransit;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Account.Commands.ResetPassword;

public class ResetPasswordCommand : IRequest
{
    public string OldPassword { get; set; }
    public string NewPassword { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
}

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMultiFactorService _multiFactorService;
    private readonly IBus _bus;
    
    public ResetPasswordCommandHandler(UserManager<User> userManager,
        IPasswordHistoryService passwordHistoryService,
        IAuditLogService auditLogService,
        IMultiFactorService multiFactorService,
        IBus bus)
    {
        _userManager = userManager;
        _passwordHistoryService = passwordHistoryService;
        _auditLogService = auditLogService;
        _multiFactorService = multiFactorService;
        _bus = bus;
    }

    public async Task<Unit> Handle(ResetPasswordCommand command, CancellationToken cancellationToken)
    {
        var user = string.IsNullOrWhiteSpace(command.UserId)
            ? await _userManager.FindByNameAsync(command.UserName)
            : await _userManager.FindByIdAsync(command.UserId);

        var isPasswordsMatched = _userManager.PasswordHasher
            .VerifyHashedPassword(user, user.PasswordHash, command.OldPassword);

        if (isPasswordsMatched == PasswordVerificationResult.Failed)
        {
            await _auditLogService.AuditLogAsync(
             new AuditLog
             {
                 IsSuccess = false,
                 LogDate = DateTime.Now,
                 Operation = "ResetPassword",
                 SourceApplication = "Identity",
                 Resource = "User",
                 UserId = user.Id,
                 Details = new Dictionary<string, string>
                     {
                          {"UserName", user.UserName },
                          {"Email", user.Email },
                          {"ErrorMessage" , "Passwords not matched!"}
                     }
             });

            throw new PasswordsNotMatchedException();
        }

        if (!await ValidatePasswordHistoryAsync(user, command.NewPassword))
        {
            throw new PasswordHistoryRequirementException();
        }

        if (ContainsBirthDate(user, command.NewPassword))
        {
            throw new PasswordContainsBirthDateException();
        }

        var result = await _userManager.ChangePasswordAsync(user, command.OldPassword, command.NewPassword);

        if (!result.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
              new AuditLog
              {
                  IsSuccess = false,
                  LogDate = DateTime.Now,
                  Operation = "ResetPassword",
                  SourceApplication = "Identity",
                  Resource = "User",
                  UserId = user.Id,
                  Details = new Dictionary<string, string>
                  {
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , result.Errors.FirstOrDefault().Description}
                  }
              });

            var errorCode = result.Errors.FirstOrDefault()?.Code;

            switch (errorCode)
            {
                case "PasswordContentError":
                    throw new PasswordContentException();
                case "PasswordLengthError":
                    throw new PasswordLengthException();
                case "PasswordRepetitiveCharacterError":
                    throw new PasswordRepetitiveCharacterException();
                case "PasswordSuccessiveCharacterError":
                    throw new PasswordSuccessiveCharacterException();
                case "PasswordHistoryRequirementError":
                    throw new PasswordHistoryRequirementException();
                default:
                    throw new InvalidNewPasswordException();
            }
        }

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "ResetPassword",
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
        await _userManager.UpdateAsync(user);

        if (user.UserType == UserType.Individual)
        {
            await _bus.Publish(new PasswordChanged
            {
                UserId = user.Id
            }, cancellationToken);
        }

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
            password.Contains($"{(birthDate.Day < 10
                                    ? $"0{birthDate.Day}"
                                    : birthDate.Day)}" +
                              $"{(birthDate.Month < 10
                                    ? $"0{birthDate.Month}"
                                    : birthDate.Month)}" +
                              $"{birthDate.Year.ToString()[2..]}");
    }
}