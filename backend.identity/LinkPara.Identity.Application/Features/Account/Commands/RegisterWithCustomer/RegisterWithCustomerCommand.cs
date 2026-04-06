using LinkPara.Audit;
using LinkPara.Audit.Models;
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
using LinkPara.Security;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System.Data;
using System.Text.RegularExpressions;

namespace LinkPara.Identity.Application.Features.Account.Commands.RegisterWithCustomer;

public class RegisterWithCustomerCommand : IRequest<RegisterResponse>
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ExternalPersonId { get; set; }
    public string ExternalCustomerId { get; set; }
    public string UserName { get; set; }
}

public class RegisterWithCustomerCommandHandler : IRequestHandler<RegisterWithCustomerCommand, RegisterResponse>
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
    private readonly IPasswordHistoryService _passwordHistoryService;
    private readonly IJwtHelper _jwtHelper;

    public RegisterWithCustomerCommandHandler(UserManager<User> userManager, RoleManager<Role> roleManager,
        IConfiguration configuration,
        IAuditLogService auditLogService,
        ISearchService searchService,
        IParameterService parameterService,
        IVaultClient vaultClient,
        IRepository<User> userRepository,
        IStringLocalizerFactory factory,
        ISecureRandomGenerator randomGenerator,
        IPasswordHistoryService passwordHistoryService, 
        IJwtHelper jwtHelper)
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
        _passwordHistoryService = passwordHistoryService;
        _jwtHelper = jwtHelper;
    }

    public async Task<RegisterResponse> Handle(RegisterWithCustomerCommand command, CancellationToken cancellationToken)
    {

        var phoneUser = await _userRepository
            .GetAll()
            .AnyAsync(s =>
                s.UserName == command.UserName &&
                (s.UserStatus == UserStatus.Active || s.UserStatus == UserStatus.Suspended));

        if (phoneUser)
        {
            throw new AlreadyInUseException(nameof(command.PhoneNumber));
        }

        var customerCheck = await _userRepository.GetAll()
            .AnyAsync(s => s.ExternalCustomerId == command.ExternalCustomerId && s.ExternalPersonId == command.ExternalPersonId);

        if (customerCheck)
        {
            throw new AlreadyInUseException(nameof(command.ExternalCustomerId));
        }

        var user = new User
        {
            UserName = command.UserName,
            Email = command.Email.ToLower(),
            PhoneCode = command.PhoneCode,
            PhoneNumber = command.PhoneNumber,
            FirstName = PascalCaseHelper.CapitalFirstLetter(command.FirstName),
            LastName = PascalCaseHelper.CapitalFirstLetter(command.LastName),
            CreateDate = DateTime.Now,
            UserStatus = UserStatus.Active,
            UserType = UserType.Individual,
            PasswordModifiedDate = DateTime.Now,
            RecordStatus = RecordStatus.Active,
            ExternalCustomerId = command.ExternalCustomerId,
            ExternalPersonId = command.ExternalPersonId,
        };

        user.CreatedBy = user.Id.ToString();
        var tempPassword = await GeneratePassword();

        var isBlacklistCheckEnabled =
           (await _parameterService.GetParameterAsync("BlackListParameters", "CheckAtRegister")).ParameterValue == "1";
        string amlReferenceNumber = null;
        if (isBlacklistCheckEnabled)
        {
            amlReferenceNumber = await CheckBlackListAsync(command);
        }

        user.AmlReferenceNumber = amlReferenceNumber;   
        var result = await _userManager.CreateAsync(user, tempPassword);

        if (!result.Succeeded)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "RegisterWithCustomer",
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

        await SaveUserRoleAsync(user);

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

    private async Task<string> CheckBlackListAsync(RegisterWithCustomerCommand command)
    {
        SearchByNameRequest searchRequest = new()
        {
            Name = $"{command.FirstName} {command.LastName}",
            SearchType = SearchType.Any,
            FraudChannelType = FraudChannelType.Web
        };

        var blackListControl = await _searchService.GetSearchByName(searchRequest);
        if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= MatchRate)
        {
            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
        }
        return blackListControl.ReferenceNumber;
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

}