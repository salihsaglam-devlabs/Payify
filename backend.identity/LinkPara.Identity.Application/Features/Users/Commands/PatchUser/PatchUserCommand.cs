using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.Emoney;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Events;
using LinkPara.SharedModels.BusModels.IntegrationEvents.DigitalKyc;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Application.Features.Users.Commands.PatchUser;

public class PatchUserCommand : IRequest<UserDto>
{
    public Guid UserId { get; set; }
    public JsonPatchDocument<PatchUserDto> PatchUserDto { get; set; }
}

public class PatchUserCommandHandler : IRequestHandler<PatchUserCommand, UserDto>
{
    private readonly ILogger<PatchUserCommand> _logger;
    private readonly IRepository<User> _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IRepository<UserLoginLastActivity> _userLoginRepository;
    private readonly IDigitalKycService _digitalKycService;
    private readonly IAccountService _accountService;
    private readonly ISearchService _searchService;
    private readonly IVaultClient _vaultClient;

    public PatchUserCommandHandler(IRepository<User> userRepository,
        UserManager<User> userManager,
        RoleManager<Role> roleManager,
        IMapper mapper,
        ILogger<PatchUserCommand> logger,
        IAuditLogService auditLogService,
        IRepository<UserLoginLastActivity> userLoginRepository,
        IDigitalKycService digitalKycService,
        IAccountService accountService,
        ISearchService searchService,
        IVaultClient vaultClient)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
        _userLoginRepository = userLoginRepository;
        _digitalKycService = digitalKycService;
        _accountService = accountService;
        _searchService = searchService;
        _vaultClient = vaultClient;
    }

    public async Task<UserDto> Handle(PatchUserCommand command, CancellationToken cancellationToken)
    {
        var dbCurrentUser = await _userRepository.GetAll()
            .Include(s => s.Roles)
            .Where(q => q.Id == command.UserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (dbCurrentUser is null)
        {
            throw new NotFoundException(nameof(User), command.UserId!);
        }

        if (dbCurrentUser.UserStatus is UserStatus.Inactive)
        {
            throw new AlreadyDeactivatedException();
        }

        var oldEmail = dbCurrentUser.Email;
        var oldPhoneNumber = dbCurrentUser.PhoneNumber;

        var requestUserDto = _mapper.Map<PatchUserDto>(dbCurrentUser);

        command.PatchUserDto.ApplyTo(requestUserDto);

        if (requestUserDto.Email != oldEmail)
        {
            var checkUser = await _userRepository.GetAll()
                    .AnyAsync(b => (b.Email.Trim() == requestUserDto.Email.Trim())
                        && dbCurrentUser.UserType == b.UserType
                        && b.UserStatus == UserStatus.Active);

            if (checkUser)
            {
                var parentAccount = await _accountService.GetParentAccountByUserIdAsync(command.UserId);
                if (!parentAccount.IsExist || parentAccount.AccountDto?.Email != requestUserDto.Email)
                {
                    throw new DuplicateRecordException();
                }
            }
        }

        var IsOngoingEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "OngoingEnabled");
        if ((requestUserDto.UserStatus == UserStatus.Inactive || requestUserDto.UserStatus == UserStatus.Suspended) && dbCurrentUser.UserStatus == UserStatus.Active && IsOngoingEnabled)
        {
            var removeOngoing = await _searchService.RemoveOngoingMonitoringAsync(dbCurrentUser.AmlReferenceNumber);
            if (!removeOngoing.IsSuccess)
            {
                _logger.LogError($"RemoveOngoingMonitoringError : {removeOngoing.ErrorCode + removeOngoing.ErrorMessage}");
            }
        }

        if (requestUserDto.UserStatus != dbCurrentUser.UserStatus)
        {
            switch (requestUserDto.UserStatus)
            {
                case UserStatus.Suspended:
                    dbCurrentUser.RecordStatus = RecordStatus.Passive;
                    dbCurrentUser.UserStatus = UserStatus.Suspended;
                    break;

                case UserStatus.Inactive:
                    DateTime timestamp = DateTime.Now;
                    dbCurrentUser.UserName = $"d_{timestamp.Ticks}_{dbCurrentUser.UserName}";
                    requestUserDto.IdentityNumber = $"d_{timestamp.Ticks}_{dbCurrentUser.IdentityNumber}";
                    dbCurrentUser.RecordStatus = RecordStatus.Passive;
                    dbCurrentUser.UserStatus = UserStatus.Inactive;
                    await _digitalKycService.UpdateIntegrationStateAsync(new UpdateIntegrationState
                    {
                        UserId = dbCurrentUser.Id,
                        IdentityNumber = dbCurrentUser.IdentityNumber
                    });

                    var dbUserRoles = await _userManager.GetRolesAsync(dbCurrentUser);

                    await _userManager.RemoveFromRolesAsync(dbCurrentUser, dbUserRoles);
                    break;

                case UserStatus.Active:
                    dbCurrentUser.RecordStatus = RecordStatus.Active;
                    dbCurrentUser.UserStatus = UserStatus.Active;
                    break;
            }

            dbCurrentUser.UpdateDate = DateTime.Now;
        }

        dbCurrentUser.FirstName = requestUserDto.FirstName;
        dbCurrentUser.LastName = requestUserDto.LastName;
        dbCurrentUser.Email = requestUserDto.Email;
        dbCurrentUser.UserStatus = requestUserDto.UserStatus;
        dbCurrentUser.IdentityNumber = requestUserDto.IdentityNumber;
        dbCurrentUser.BirthDate = requestUserDto.BirthDate;

        if (dbCurrentUser.PhoneNumber != requestUserDto.PhoneNumber)
        {
            var newPhoneCode = string.Empty;
            switch (dbCurrentUser.UserType)
            {
                case UserType.Individual:
                    newPhoneCode = requestUserDto.PhoneCode.Replace("+", "I");
                    break;
                case UserType.Corporate:
                    newPhoneCode = requestUserDto.PhoneCode.Replace("+", "C");
                    break;
                case UserType.Internal:
                    newPhoneCode = requestUserDto.PhoneCode.Replace("+", "U");
                    break;
                case UserType.CorporateWallet:
                    newPhoneCode = requestUserDto.PhoneCode.Replace("+", "CW");
                    break;
                case UserType.CorporateSubMerchant:
                    newPhoneCode = requestUserDto.PhoneCode.Replace("+", "CS");
                    break;
                default:
                    break;
            }
            var newUserName = $"{newPhoneCode}{requestUserDto.PhoneNumber}";

            var user = await _userManager.FindByNameAsync(newUserName);

            if (user is not null)
            {
                throw new DuplicateRecordException();
            }

            dbCurrentUser.UserName = newUserName;
            dbCurrentUser.PhoneCode = requestUserDto.PhoneCode;
            dbCurrentUser.PhoneNumber = requestUserDto.PhoneNumber;
        }

        if (requestUserDto.Email != oldEmail || requestUserDto.PhoneNumber != oldPhoneNumber)
        {
            var userLoginActivity = await _userLoginRepository
                      .GetAll()
                      .FirstOrDefaultAsync(b => b.UserId == dbCurrentUser.Id);

            if (userLoginActivity is null)
            {
                await SendEmailAsync(dbCurrentUser);
            }
        }

        await _userManager.UpdateNormalizedEmailAsync(dbCurrentUser);
        await _userManager.UpdateNormalizedUserNameAsync(dbCurrentUser);

        await _userRepository.UpdateAsync(dbCurrentUser);

        var roleProcess = command.PatchUserDto.Operations
            .Where(s => s.path.ToLowerInvariant().Contains("/roles") && s.OperationType == OperationType.Replace)
            .ToList();

        if (roleProcess.Count > 0)
        {
            await PatchRoleProcess(requestUserDto, dbCurrentUser);
        }

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "PatchUser",
            SourceApplication = "Identity",
            Resource = "User",
            Details = new Dictionary<string, string>
            {
                {"UserId", command.UserId.ToString() }
            }
        });

        return _mapper.Map<UserDto>(dbCurrentUser);
    }

    private async Task PatchRoleProcess(PatchUserDto requestUserDto, User dbCurrentUser)
    {
        var roleNames = new List<string>();

        var newRoles = await _roleManager.Roles
            .Where(x => requestUserDto.Roles.Contains(x.Id))
            .ToListAsync();

        foreach (var role in newRoles)
        {
            if (role is null)
            {
                throw new NotFoundException(nameof(Role), role);
            }

            roleNames.Add(role.Name);
        }

        var dbUserRoles = await _userManager.GetRolesAsync(dbCurrentUser);

        _ = await _userManager.RemoveFromRolesAsync(dbCurrentUser, dbUserRoles);

        var result = await _userManager.AddToRolesAsync(dbCurrentUser, roleNames);

        if (!result.Succeeded)
        {
            _logger.LogError($"PatchRoleProcess Error: \"{result.Errors.FirstOrDefault()?.Code}\"");
        }
    }

    private async Task SendEmailAsync(User user)
    {
        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);

        Dictionary<string, string> mailParameters = new()
        {
            { "ResetPasswordToken", Base64UrlEncoder.Encode(resetPasswordToken) }
        };

        user.DomainEvents.Add(new UserCreatedEvent(user, mailParameters));
    }
}
