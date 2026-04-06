using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Helpers;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Events;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using Microsoft.IdentityModel.Tokens;
using System.Text.RegularExpressions;
using LinkPara.SharedModels.Notification.NotificationModels.Identity;
using MassTransit;
using LinkPara.SharedModels.Boa.Enums;

namespace LinkPara.Identity.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommand : IRequest<UserCreateResponse>
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public List<Guid> Roles { get; set; }
    public bool IysPermission { get; set; }
    public string IdentityNumber { get; set; }
    public bool? IsBlacklistControl { get; set; }
    public string AmlReferenceNumber { get; set; }
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, UserCreateResponse>
{
    private readonly IRepository<User> _userRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<Role> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditLogService;
    private readonly ISearchService _searchService;
    private readonly IParameterService _parameterService;
    private readonly IVaultClient _vaultClient;
    private readonly IStringLocalizer _localizer;
    private const int MatchRate = 90;
    private readonly ISecureRandomGenerator _randomGenerator;
    private readonly IRepository<UserAgreementDocument> _userAgreementRepository;
    private readonly IRepository<AgreementDocumentVersion> _agreementDocumentVersionRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IBus _bus;

    public CreateUserCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager,
        IConfiguration configuration,
        IAuditLogService auditLogService,
        ISearchService searchService,
        IParameterService parameterService,
        IVaultClient vaultClient,
        IRepository<User> userRepository,
        IStringLocalizerFactory factory,
        ISecureRandomGenerator randomGenerator,
        IRepository<UserAgreementDocument> userAgreementRepository,
        IRepository<AgreementDocumentVersion> agreementDocumentVersionRepository,
        IContextProvider contextProvider,
        IBus bus)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _auditLogService = auditLogService;
        _searchService = searchService;
        _parameterService = parameterService;
        _vaultClient = vaultClient;
        _userRepository = userRepository;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
        _randomGenerator = randomGenerator;
        _userAgreementRepository = userAgreementRepository;
        _agreementDocumentVersionRepository = agreementDocumentVersionRepository;
        _contextProvider = contextProvider;
        _bus = bus;
    }

    public async Task<UserCreateResponse> Handle(CreateUserCommand command, CancellationToken cancellationToken)
    {

        var checkUser = await _userRepository.GetAll()
           .AnyAsync(b =>
               (
                 b.UserName.Trim() == command.UserName.Trim() ||
                 b.Email.Trim() == command.Email.Trim()
               ) &&
               (
                   (command.UserType == UserType.Corporate || command.UserType == UserType.CorporateSubMerchant) ?
                   (b.UserType == UserType.Corporate || b.UserType == UserType.CorporateSubMerchant) :
                   b.UserType == command.UserType
               ) &&
               b.UserStatus == UserStatus.Active);

        if (checkUser)
        {
            throw new AlreadyInUseException(nameof(command.PhoneNumber));
        }

        var IsBlacklistCheckEnabled =
         _vaultClient.GetSecretValue<bool>("/SharedSecrets", "ServiceState", "BlacklistEnabled");
        string amlReferenceNumber = string.Empty;
        if (IsBlacklistCheckEnabled && command.IsBlacklistControl != true)
        {
            if (command.UserType is UserType.Corporate or UserType.CorporateSubMerchant or UserType.Individual)
            {
                SearchByNameRequest searchRequest = new()
                {
                    Name = $"{command.FirstName} {command.LastName}",
                    SearchType = command.UserType is UserType.Corporate or UserType.CorporateSubMerchant ? SearchType.Corporate : SearchType.Individual,
                    FraudChannelType = command.UserType is UserType.Corporate or UserType.CorporateSubMerchant ? FraudChannelType.Backoffice : FraudChannelType.Web
                };
                if (command.BirthDate != DateTime.MinValue)
                {
                    searchRequest.BirthYear = command.BirthDate.Year.ToString();
                }

                var blackListControl = await _searchService.GetSearchByName(searchRequest);
                if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= MatchRate)
                {
                    var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

                    var exceptionMessage = _localizer.GetString("UserInBlacklistException");

                    throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
                }

                amlReferenceNumber = blackListControl.ReferenceNumber;
            }
        }
        if (IsBlacklistCheckEnabled && command.IsBlacklistControl == true)
        {
            amlReferenceNumber = command.AmlReferenceNumber;
        }

        var user = new User
        {
            UserName = command.UserName,
            Email = command.Email.ToLower(),
            PhoneCode = command.PhoneCode,
            PhoneNumber = command.PhoneNumber,
            FirstName = PascalCaseHelper.CapitalFirstLetter(command.FirstName).Trim(),
            LastName = PascalCaseHelper.CapitalFirstLetter(command.LastName).Trim(),
            BirthDate = command.BirthDate,
            UserStatus = UserStatus.Active,
            UserType = command.UserType,
            PasswordModifiedDate = DateTime.Now,
            RecordStatus = RecordStatus.Active,
            CreateDate = DateTime.Now,
            IdentityNumber = command.IdentityNumber,
            AmlReferenceNumber = amlReferenceNumber,
        };

        user.CreatedBy = user.Id.ToString();
        var tempPassword = await GeneratePassword();

        var result = user.UserType != UserType.ApplicationUser
        ? await _userManager.CreateAsync(user, tempPassword)
        : await _userManager.CreateAsync(user, command.Password);


        if (!result.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "CreateUser",
                    SourceApplication = "Identity",
                    Resource = "User",
                    UserId = user.Id,
                    Details = new Dictionary<string, string>
                    {
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , result.Errors?.FirstOrDefault()?.Description}
                    }
                });

            throw new InvalidRegisterException();
        }

        switch (user.UserType)
        {
            case UserType.Corporate:
            case UserType.Internal:
            case UserType.Representative:
            case UserType.Branch:
            case UserType.CorporateWallet:
            case UserType.CorporateSubMerchant:
                await UserCreatedEvent(user);
                break;
            default:
                break;
        }

        if (user.UserType == UserType.Individual)
        {
            await SaveUserRoleAsync(user);
            await SaveAgreementDocuments(user, command.IysPermission, cancellationToken);
            await SendResetPasswordMailAsync(user, await GetResetPasswordToken(user));

            return new UserCreateResponse { UserId = user.Id };
        }

        if (user.UserType != UserType.ApplicationUser)
        {
            var roles = await _roleManager.Roles
                .Where(x =>
                    user.UserType == UserType.Representative
                    ? x.RoleScope == RoleScope.Representative
                    : user.UserType == UserType.Branch
                    ? x.RoleScope == RoleScope.Branch
                    : command.Roles.Contains(x.Id)
                )
                .Select(x => new { x.Name, x.RoleScope })
                .ToListAsync();

            if (user.UserType == UserType.Corporate)
            {
                if (roles.Any(r => r.RoleScope != RoleScope.Merchant))
                {
                    throw new InvalidRoleUpdateException();
                }
            }

            if (user.UserType == UserType.CorporateSubMerchant)
            {
                if (roles.Any(r => r.RoleScope != RoleScope.CorporateSubMerchant))
                {
                    throw new InvalidRoleUpdateException();
                }
            }

            var roleNames = roles.Select(x => x.Name).ToList();

            var roleResult = await _userManager.AddToRolesAsync(user, roleNames);

            if (!roleResult.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "AddUserRole",
                        SourceApplication = "Identity",
                        Resource = "UserRole",
                        UserId = user.Id,
                        Details = new Dictionary<string, string>
                        {
                        {"UserId", user.Id.ToString() },
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , roleResult.Errors?.FirstOrDefault()?.Description}
                        }
                    });
            }
        }
        else
        {
            var role = await _roleManager.FindByNameAsync(command.UserName);
            if (role == null)
            {
                role = new Role
                {
                    Name = command.UserName,
                    RoleScope = RoleScope.Application,
                    RecordStatus = RecordStatus.Active,
                    CreateDate = DateTime.Now,
                    CreatedBy = user.Id.ToString()
                };

                await _roleManager.CreateAsync(role);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, role?.Name);


            if (!roleResult.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "AddUserRole",
                        SourceApplication = "Identity",
                        Resource = "UserRole",
                        UserId = user.Id,
                        Details = new Dictionary<string, string>
                        {
                        {"UserId", user.Id.ToString() },
                        {"UserName", user.UserName },
                        {"Email", user.Email },
                        {"ErrorMessage" , roleResult.Errors?.FirstOrDefault()?.Description}
                        }
                    });
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
                UserId = user.Id,
                Details = new Dictionary<string, string>
                {
                    {"UserId", user.Id.ToString() },
                    {"UserName", user.UserName },
                    {"Email", user.Email },
                }
            });

        return new UserCreateResponse { UserId = user.Id };
    }
    private async Task UserCreatedEvent(User user)
    {
        user.DomainEvents.Add(new UserCreatedEvent(user, await GetResetPasswordToken(user)));
    }

    private async Task<Dictionary<string, string>> GetResetPasswordToken(User user)
    {
        var resetPasswordToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        Dictionary<string, string> mailParameters = new()
        {
            { "ResetPasswordToken", Base64UrlEncoder.Encode(resetPasswordToken) }
        };
        return mailParameters;
    }

    private async Task<string> GeneratePassword()
    {
        var password = string.Empty;
        var passwordLength = _vaultClient.GetSecretValue<int>("IdentitySecrets", "PasswordSettings", "RequiredLength");

        var passwordCharValues = _configuration["PasswordValidation:CurrentPasswordCharValues"];

        do
        {
            password = new string(Enumerable.Repeat(passwordCharValues, passwordLength)
                .Select(s => s[(int)_randomGenerator.GenerateSecureRandomNumber(0, s.Length)]).ToArray());

        } while (!await ValidatePassword(password));

        return password;
    }
    private Task<bool> ValidatePassword(string password)
    {
        var repetitiveRegexPattern = _configuration["PasswordValidation:RepetitiveRegexPattern"];
        if (Regex.IsMatch(password, repetitiveRegexPattern))
        {
            return Task.FromResult(false);
        }

        var successiveRegexPattern = _configuration["PasswordValidation:SuccessiveRegexPattern"];
        if (Regex.IsMatch(password, successiveRegexPattern))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }
    private async Task SaveUserRoleAsync(User user)
    {
        var role = await _roleManager.FindByNameAsync("Individual");

        if (role is not null)
        {
            var roleResult = await _userManager.AddToRoleAsync(user, role.Name);

            if (!roleResult.Succeeded)
            {
                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = false,
                        LogDate = DateTime.Now,
                        Operation = "CreateUserRole",
                        SourceApplication = "Identity",
                        Resource = "UserRole",
                        UserId = user.Id,
                        Details = new Dictionary<string, string>
                        {
                            {"UserName", user.UserName },
                            {"Email", user.Email },
                            {"ErrorMessage" , roleResult.Errors.FirstOrDefault()?.Description}
                        }
                    });
            }
        }
    }
    private async Task SaveAgreementDocuments(User user, bool iysPermission,
        CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var latestVersionDocuments = await _agreementDocumentVersionRepository
            .GetAll()
            .Where(f => f.IsLatest && f.LanguageCode == languageCode
            && f.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);
        string IysDocumentId = _vaultClient.GetSecretValue<string>("IdentitySecrets", "LoginSettings", "IysDocumentId");
        foreach (var document in latestVersionDocuments)
        {
            if (document.AgreementDocumentId == Guid.Parse(IysDocumentId))
            {
                if (!iysPermission)
                    continue;
            }

            var userAgreementDocument = new UserAgreementDocument
            {
                UserId = user.Id,
                AgreementDocumentVersionId = document.Id,
                CreatedBy = user.Id.ToString(),
                ApprovalChannel = _contextProvider.CurrentContext.Channel,
            };

            await _userAgreementRepository.AddAsync(userAgreementDocument);
        }
    }
    private async Task SendResetPasswordMailAsync(User user, Dictionary<string, string> parameters)
    {
        var link = $"{_vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "Web")}reset-password?token={parameters["ResetPasswordToken"]}&username={user.UserName}";

        await _bus.Publish(new UserOnboarding
        {
            UserId = user.Id,
            Link = link,
            Username = $"{user.PhoneCode}{user.PhoneNumber}"
        }, CancellationToken.None);
    }
}