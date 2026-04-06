using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Application.Features.Account.Commands.UpdatePhoneNumber;
public class UpdatePhoneNumberCommand : IRequest
{
    public Guid UserId { get; set; }
    public string Token { get; set; }
    public string NewPhoneNumber { get; set; }
    public string NewPhoneCode { get; set; }
}
public class UpdatePhoneNumberCommandHandler : IRequestHandler<UpdatePhoneNumberCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<UpdatePhoneNumberCommandHandler> _logger;
    private readonly IAuditLogService _auditLogService;
    public UpdatePhoneNumberCommandHandler(
        UserManager<User> userManager,
        IRepository<User> userRepository,
        ILogger<UpdatePhoneNumberCommandHandler> logger,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _userRepository = userRepository;
        _logger = logger;
        _auditLogService = auditLogService;
    }
    public async Task<Unit> Handle(UpdatePhoneNumberCommand command, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(command.UserId) ?? throw new UserNotFoundException();

        var dbChecker = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(x =>
                                 x.PhoneNumber == command.NewPhoneNumber &&
                                 x.UserStatus != UserStatus.Inactive &&
                                 x.UserType == UserType.Individual, cancellationToken);
        if (dbChecker is not null)
        {
            throw new AlreadyInUseException(nameof(command.NewPhoneNumber));
        }

        var result = await _userManager.VerifyChangePhoneNumberTokenAsync(user, Base64UrlEncoder.Decode(command.Token), command.NewPhoneNumber);

        if (!result)
        {
            _logger.LogError($"UpdatePhoneNumberError : {command.NewPhoneNumber}");
            throw new InvalidNewPhoneNumberException();
        }

        user.PhoneNumber = command.NewPhoneNumber;
        user.PhoneCode = command.NewPhoneCode;
        user.PhoneNumberConfirmed = true;
        user.UserName = $"I{command.NewPhoneCode.Replace("+", "")}{command.NewPhoneNumber}";
        user.SecurityStamp = _userManager.GenerateNewAuthenticatorKey();

        var updateResult = await _userManager.UpdateAsync(user);

        if (!updateResult.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "UpdateUserPhoneNumber",
                    SourceApplication = "Identity",
                    Resource = "User",
                    UserId = user.Id,
                    Details = new Dictionary<string, string>
                    {
                        {"PhoneNumber", user.PhoneNumber },
                        {"ErrorMessage" , updateResult.Errors?.FirstOrDefault()?.Description}
                    }
                });
            throw new InvalidOperationException();
        }
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateUserPhoneNumber",
                SourceApplication = "Identity",
                Resource = "User",
                UserId = user.Id,
                Details = new Dictionary<string, string>
                {
                    {"PhoneNumber", user.PhoneNumber }
                }
            });
        return Unit.Value;
    }
}
