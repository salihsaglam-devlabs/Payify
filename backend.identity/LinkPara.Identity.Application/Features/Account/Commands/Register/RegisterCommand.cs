using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Exceptions.PasswordValidations;
using LinkPara.Identity.Application.Common.Helpers;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.Identity.Domain.Events;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.Account.Commands.Register;

public class RegisterCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public DateTime BirthDate { get; set; }
    public UserType UserType { get; set; }
    public UserStatus UserStatus { get; set; }
    public string IdentityNumber { get; set; }
    public string UserName { get; set; }
    public bool IysPermission { get; set; }
    public Guid ParentAccountId { get; set; }
    public List<Guid> AgreedDocuments { get; set; }
    public Guid? SecurityQuestionId { get; set; }
    public string SecurityAnswer { get; set; }
}

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, RegisterResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserAgreementDocument> _userAgreementRepository;
    private readonly IRepository<AgreementDocumentVersion> _agreementDocumentVersionRepository;
    private readonly RoleManager<Role> _roleManager;
    private readonly IJwtHelper _jwtHelper;
    private readonly IAuditLogService _auditLogService;
    private readonly ISearchService _searchService;
    private readonly IContextProvider _contextProvider;
    private readonly IParameterService _parameterService;
    private readonly IRepository<User> _userRepository;
    private readonly IRepository<LoginWhitelist> _loginWhitelistRepository;
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IVaultClient _vaultClient;
    private readonly IStringLocalizer _localizer;
    private readonly IRepository<UserSecurityAnswer> _securityAnswerRepository;
    private const int MatchRate = 90;

    public RegisterCommandHandler(UserManager<User> userManager,
        IRepository<UserAgreementDocument> userAgreementRepository,
        IRepository<AgreementDocumentVersion> agreementDocumentVersionRepository,
        IJwtHelper jwtHelper,
        IAuditLogService auditLogService,
        ISearchService searchService,
        RoleManager<Role> roleManager,
        IParameterService parameterService,
        IRepository<LoginWhitelist> loginWhitelistRepository,
        IRepository<User> userRepository,
        IPasswordHistoryService passwordHistoryService,
        IVaultClient vaultClient,
        IStringLocalizerFactory factory,
        IRepository<UserSecurityAnswer> securityAnswerRepository,
        IContextProvider contextProvider)
    {
        _userManager = userManager;
        _jwtHelper = jwtHelper;
        _userAgreementRepository = userAgreementRepository;
        _agreementDocumentVersionRepository = agreementDocumentVersionRepository;
        _auditLogService = auditLogService;
        _searchService = searchService;
        _roleManager = roleManager;
        _parameterService = parameterService;
        _userRepository = userRepository;
        _loginWhitelistRepository = loginWhitelistRepository;
        _passwordHistoryService = passwordHistoryService;
        _vaultClient = vaultClient;
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");
        _securityAnswerRepository = securityAnswerRepository;
        _contextProvider = contextProvider;
    }

    public async Task<RegisterResponse> Handle(RegisterCommand command, CancellationToken cancellationToken)
    {
        var phoneUser = await _userRepository
            .GetAll()
            .FirstOrDefaultAsync(s =>
                s.UserName == command.UserName &&
                (s.UserStatus == UserStatus.Active || s.UserStatus == UserStatus.Suspended));

        if (phoneUser is not null)
        {
            throw new AlreadyInUseException(nameof(command.PhoneNumber));
        }
        if (command.BirthDate != DateTime.MinValue)
        {
            await IsBirthdateBetweenAllowedRangeAsync(command.BirthDate, (command.ParentAccountId != Guid.Empty));
        }
        await CheckPilotModeLoginWhitelist(command.PhoneCode, command.PhoneNumber);

        var user = new User
        {
            UserName = command.UserName,
            Email = command.Email.ToLower(),
            PhoneCode = command.PhoneCode,
            PhoneNumber = command.PhoneNumber,
            FirstName = PascalCaseHelper.CapitalFirstLetter(command.FirstName),
            LastName = PascalCaseHelper.CapitalFirstLetter(command.LastName),
            BirthDate = command.BirthDate,
            CreateDate = DateTime.Now,
            IdentityNumber = command.IdentityNumber,
            UserStatus = command.UserStatus,
            UserType = command.UserType,
            PasswordModifiedDate = DateTime.Now,
            RecordStatus = RecordStatus.Active,
        };

        user.CreatedBy = user.Id.ToString();

        user.DomainEvents.Add(new UserCreatedEvent(user));

        var isBlacklistCheckEnabled =
            (await _parameterService.GetParameterAsync("BlackListParameters", "CheckAtRegister")).ParameterValue == "1";
        string amlReferenceNumber = null;
        if (isBlacklistCheckEnabled)
        {
            amlReferenceNumber = await CheckBlackListAsync(command);
        }

        user.AmlReferenceNumber = amlReferenceNumber;
        var result = await _userManager.CreateAsync(user, command.Password);

        if (!result.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "RegisterUser",
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

            var errorCode = result.Errors.FirstOrDefault()?.Code;

            throw errorCode switch
            {
                "PasswordContentError" => new PasswordContentException(),
                "PasswordLengthError" => new PasswordLengthException(),
                "PasswordRepetitiveCharacterError" => new PasswordRepetitiveCharacterException(),
                "PasswordSuccessiveCharacterError" => new PasswordSuccessiveCharacterException(),
                "PasswordHistoryRequirementError" => new PasswordHistoryRequirementException(),
                _ => new InvalidRegisterException(),
            };
        }

        await SaveUserRoleAsync(user, cancellationToken);
        if (command.AgreedDocuments is null || command.AgreedDocuments.Count == 0)
        {
            await SaveAgreementDocuments(user, command, cancellationToken);
        }
        else
        {
            await SaveAgreementDocumentsFromList(user, command, cancellationToken);
        }

        if (command.SecurityQuestionId.HasValue)
        {
            await SaveUserAnswerAsync(user, command.SecurityAnswer, (Guid)command.SecurityQuestionId);
        }

        await _passwordHistoryService.SavePasswordAsync(user, user.PasswordHash);

        var accessToken = await _jwtHelper.GenerateJwtTokenAsync(user);

        var refreshToken = await _jwtHelper.GenerateUserRefreshTokenAsync(user);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "RegisterUser",
                SourceApplication = "Identity",
                Resource = "User",
                UserId = user.Id,
                Details = new Dictionary<string, string>
                {
                   {"UserName", user.UserName },
                   {"Email", user.Email }
                }
            });

        return new RegisterResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.RefreshToken,
            RefreshTokenExpiration = refreshToken.RefreshTokenExpiration,
            UserId = user.Id
        };
    }

    private async Task SaveUserAnswerAsync(User user, string securityAnswer, Guid securityQuestionId)
    {
        var userSecurityAnswer = new UserSecurityAnswer
        {
            UserId = user.Id,
            SecurityQuestionId = securityQuestionId,
            AnswerHash = _userManager.PasswordHasher.HashPassword(user, securityAnswer),
            CreatedBy = user.Id.ToString()
        };
        await _securityAnswerRepository.AddAsync(userSecurityAnswer);
    }

    private async Task SaveUserRoleAsync(User user, CancellationToken cancellationToken)
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
                        Operation = "RegisterUserRole",
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

    private async Task<string> CheckBlackListAsync(RegisterCommand command)
    {
        SearchByNameRequest searchRequest = new()
        {
            Name = $"{command.FirstName} {command.LastName}",
            SearchType = SearchType.Any,
            BirthYear = command.BirthDate.Year.ToString(),
            FraudChannelType = FraudChannelType.Web
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
        return blackListControl.ReferenceNumber;
    }

    private async Task SaveAgreementDocumentsFromList(User user, RegisterCommand registerCommand,
        CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                                ? "tr"
                                : _contextProvider.CurrentContext.Language.Substring(0, 2);

        foreach (var documentId in registerCommand.AgreedDocuments)
        {
            var latestVersionDocument = await _agreementDocumentVersionRepository
                                                .GetAll()
                                                .Where(f => f.AgreementDocumentId == documentId
                                                && f.IsLatest && f.LanguageCode == languageCode
                                                && f.RecordStatus == RecordStatus.Active)
                                         .FirstOrDefaultAsync();

            var userAgreementDocument = new UserAgreementDocument
            {
                UserId = user.Id,
                AgreementDocumentVersionId = latestVersionDocument.Id,
                CreatedBy = user.Id.ToString(),
                ApprovalChannel = _contextProvider.CurrentContext.Channel,
            };

            await _userAgreementRepository.AddAsync(userAgreementDocument);
        }
    }

    private async Task SaveAgreementDocuments(User user, RegisterCommand registerCommand,
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
                if (!registerCommand.IysPermission)
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

    private async Task IsBirthdateBetweenAllowedRangeAsync(DateTime dt, bool isChildAccount)
    {
        DateTime rangeStart, rangeEnd;
        var customerAgeRequirements = await _parameterService.GetParametersAsync("CustomerAgeRequirements");

        if (!isChildAccount)
        {
            _ = int.TryParse(
                customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var minAge);
            rangeStart = DateTime.Now.AddYears(-1 * minAge);

            _ = int.TryParse(
                customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MaxAge")?.ParameterValue, out var maxAge);
            rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

            if (!(dt <= rangeStart && dt >= rangeEnd))
            {
                var exceptionMessage = _localizer.GetString("BirthdateOutOfRange")
                    .Value.Replace("@@minAge", minAge.ToString());

                throw new BirthdateOutOfRangeException(exceptionMessage);
            }
        }
        else
        {
            _ = int.TryParse(
                customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinChildAge")?.ParameterValue, out var minAge);
            rangeStart = DateTime.Now.AddYears(-1 * minAge);

            _ = int.TryParse(
               customerAgeRequirements.FirstOrDefault(p => p.ParameterCode == "MinAge")?.ParameterValue, out var maxAge);
            rangeEnd = DateTime.Now.AddYears(-1 * maxAge);

            if (!(dt <= rangeStart && dt >= rangeEnd))
            {
                var exceptionMessage = _localizer.GetString("ChildAccountBirthdateOutOfRange").Value
                    .Replace("@@minAge", minAge.ToString())
                    .Replace("@@maxAge", maxAge.ToString());

                throw new BirthdateOutOfRangeException(exceptionMessage);
            }
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