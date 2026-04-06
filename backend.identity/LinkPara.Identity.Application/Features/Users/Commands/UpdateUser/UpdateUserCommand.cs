using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.Identity.Application.Common.Helpers;
using LinkPara.Identity.Domain.Events;
using System.Web;
using LinkPara.HttpProviders.BusinessParameter;
using Microsoft.IdentityModel.Tokens;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.DigitalKyc;
using Microsoft.Extensions.Localization;
using LinkPara.ContextProvider;
using LinkPara.SharedModels.Boa.Enums;

namespace LinkPara.Identity.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public List<Guid> Roles { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string UserName { get; set; }
    public bool? IsBlacklistControl { get; set; }
    public string AmlReferenceNumber { get; set; }
}

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand>
{
    private readonly IRepository<User> _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IAuditLogService _auditLogService;
    private readonly ISearchService _searchService;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    private readonly IRepository<UserLoginLastActivity> _userLoginRepository;
    private readonly IParameterService _parameterService;
    private readonly IVaultClient _vaultClient;
    private readonly IDigitalKycService _digitalKycService;
    private readonly IStringLocalizer _localizer;

    public UpdateUserCommandHandler(IRepository<User> userRepository,
        UserManager<User> userManager,
        IAuditLogService auditLogService,
        ISearchService searchService,
        ILogger<UpdateUserCommandHandler> logger,
        IRepository<UserLoginLastActivity> userLoginRepository,
        IParameterService parameterService,
        IVaultClient vaultClient,
        IDigitalKycService digitalKycService,
        IStringLocalizerFactory factory,
        RoleManager<Role> roleManager)
    {
        _userRepository = userRepository;
        _userManager = userManager;
        _auditLogService = auditLogService;
        _searchService = searchService;
        _logger = logger;
        _userLoginRepository = userLoginRepository;
        _parameterService = parameterService;
        _vaultClient = vaultClient;
        _digitalKycService = digitalKycService;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
        _roleManager = roleManager;
    }
    public async Task<Unit> Handle(UpdateUserCommand command, CancellationToken cancellationToken)
    {
        var dbCurrentUser = await _userRepository.GetAll()
            .Include(b => b.Roles)
            .Where(q => q.Id == command.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (dbCurrentUser is null)
        {
            throw new NotFoundException(nameof(User), command.Id!);
        }

        var checkUser = await _userRepository.GetAll()
            .FirstOrDefaultAsync(b => (b.UserName.Trim() == command.UserName.Trim()
                                    || b.Email.Trim() == command.Email.Trim())
                                    && b.Id != command.Id
                                    && b.UserType == command.UserType
                                    && b.UserStatus == UserStatus.Active);

        if (checkUser is not null)
        {
            throw new AlreadyInUseException(nameof(command.PhoneNumber));
        }

        var oldEmail = dbCurrentUser.Email;
        var oldPhoneNumber = dbCurrentUser.PhoneNumber;

        var IsBlacklistCheckEnabled =
        _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "BlacklistEnabled");
        string amlReferenceNumber = dbCurrentUser.AmlReferenceNumber;
        if (IsBlacklistCheckEnabled && command.IsBlacklistControl != true)
        {
            var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

            if (command.UserType is UserType.Corporate or UserType.CorporateSubMerchant)
            {
                var oldUserName = $"{dbCurrentUser.FirstName}{dbCurrentUser.LastName}";

                if (!dbCurrentUser.Equals($"{command.FirstName}{command.LastName}"))
                {
                    SearchByNameRequest searchRequest = new()
                    {
                        Name = $"{command.FirstName} {command.LastName}",
                        SearchType = SearchType.Corporate,
                        FraudChannelType = FraudChannelType.Backoffice
                    };
                    var blackListControl = await _searchService.GetSearchByName(searchRequest);
                    if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
                    {
                        var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

                        var exceptionMessage = _localizer.GetString("UserInBlacklistException");

                        throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
                    }
                    amlReferenceNumber = blackListControl.ReferenceNumber;
                }                
            }
        }
        if (IsBlacklistCheckEnabled && command.IsBlacklistControl == true)
        {
            amlReferenceNumber = command.AmlReferenceNumber;
        }

        var IsOngoingEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "OngoingEnabled");
        if (dbCurrentUser.RecordStatus == RecordStatus.Active && command.RecordStatus == RecordStatus.Passive && IsOngoingEnabled)
        {
            var removeOngoing = await _searchService.RemoveOngoingMonitoringAsync(dbCurrentUser.AmlReferenceNumber);
            if (!removeOngoing.IsSuccess)
            {
                _logger.LogError($"RemoveOngoingMonitoringError : {removeOngoing.ErrorCode + removeOngoing.ErrorMessage}");
            }
        }

        try
        {
            dbCurrentUser.UserName = command.UserName;
            dbCurrentUser.Email = command.Email;
            dbCurrentUser.PhoneNumber = command.PhoneNumber;
            dbCurrentUser.FirstName = PascalCaseHelper.CapitalFirstLetter(command.FirstName).Trim();
            dbCurrentUser.LastName = PascalCaseHelper.CapitalFirstLetter(command.LastName).Trim();
            dbCurrentUser.UserType = command.UserType;
            dbCurrentUser.PasswordModifiedDate = DateTime.Now;
            dbCurrentUser.RecordStatus = command.RecordStatus;
            dbCurrentUser.UserStatus = command.RecordStatus == RecordStatus.Active ? UserStatus.Active : UserStatus.Inactive;
            dbCurrentUser.BirthDate = command.BirthDate;
            dbCurrentUser.AmlReferenceNumber = amlReferenceNumber;

            if (command.RecordStatus == RecordStatus.Passive)
            {
                await _digitalKycService.UpdateIntegrationStateAsync(new UpdateIntegrationState
                {
                    UserId = dbCurrentUser.Id,
                    IdentityNumber = dbCurrentUser.IdentityNumber
                });

                DateTime timestamp = DateTime.Now;
                dbCurrentUser.UserName = $"d_{timestamp.Ticks}_{dbCurrentUser.UserName}";
                dbCurrentUser.NormalizedUserName = $"D_{timestamp.Ticks}_{dbCurrentUser.NormalizedUserName}";
            }

            if (command.Email != oldEmail || command.PhoneNumber != oldPhoneNumber)
            {
                var userLoginActivity = await _userLoginRepository
                    .GetAll()
                    .FirstOrDefaultAsync(b => b.UserId == dbCurrentUser.Id);

                if (userLoginActivity is null)
                {
                    await SendEmailAsync(dbCurrentUser);
                }
            }

            var result = await _userManager.UpdateAsync(dbCurrentUser);

            if (!result.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "UpdateUser",
                        SourceApplication = "Identity",
                        Resource = "User",
                        UserId = dbCurrentUser.Id,
                        Details = new Dictionary<string, string>
                        {
                        {"UserName", dbCurrentUser.UserName },
                        {"Email", dbCurrentUser.Email },
                        {"ErrorMessage" , result.Errors?.FirstOrDefault()?.Description}
                        }
                    });

                throw new InvalidRegisterException();
            }
            else
            {
                if (command.Roles.Any())
                {
                    if (!dbCurrentUser.Roles.Any(item => command.Roles.Contains(item.Id)))
                    {
                        var roleNames = new List<string>();

                        var newRoles = await _roleManager.Roles
                            .Where(x => command.Roles.Contains(x.Id))
                            .ToListAsync();

                        foreach (var role in newRoles)
                        {
                            if (role is null)
                            {
                                throw new NotFoundException(nameof(Role), role);
                            }

                            if (dbCurrentUser.Roles.Any(r => r.RoleScope != role.RoleScope))
                            {
                                throw new InvalidRoleUpdateException();
                            }

                            roleNames.Add(role.Name);
                        }

                        var dbUserRoles = await _userManager.GetRolesAsync(dbCurrentUser);

                        _ = await _userManager.RemoveFromRolesAsync(dbCurrentUser, dbUserRoles);

                        var resultRole = await _userManager.AddToRolesAsync(dbCurrentUser, roleNames);

                        if (!resultRole.Succeeded)
                        {
                            _logger.LogError($"UpdateRoleProcess Error: \"{result.Errors.FirstOrDefault()?.Code}\"");
                        }
                    }
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "CreateUser",
                        SourceApplication = "Identity",
                        Resource = "User",
                        UserId = dbCurrentUser.Id,
                        Details = new Dictionary<string, string>
                        {
                             {"UserId", dbCurrentUser.Id.ToString() },
                             {"UserName", dbCurrentUser.UserName },
                             {"Email", dbCurrentUser.Email },
                        }
                    });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateUserError : {exception}");
            throw;
        }
        return Unit.Value;
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
