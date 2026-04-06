using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Command.UpdateSubMerchantUser;

public class UpdateSubMerchantUserCommand : IRequest, IMapFrom<SubMerchantUser>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string IdentityNumber { get; set; }
    public Guid UserId { get; set; }
    public Guid SubMerchantId { get; set; }
    public RecordStatus RecordStatus { get; set; }
}

public class UpdateSubMerchantUserCommandHandler : IRequestHandler<UpdateSubMerchantUserCommand>
{
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IMapper _mapper;
    private readonly IVaultClient _vaultClient;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<UpdateSubMerchantUserCommandHandler> _logger;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    private readonly ISearchService _searchService;
    private readonly IUserService _userService;

    public UpdateSubMerchantUserCommandHandler(
        IGenericRepository<SubMerchantUser> subMerchantUserRepository,
        IMapper mapper,
        IVaultClient vaultClient,
        IContextProvider contextProvider,
        IAuditLogService auditLogService,
        ILogger<UpdateSubMerchantUserCommandHandler> logger,
        IParameterService parameterService,
        IStringLocalizerFactory factory,
        ISearchService searchService,
        IUserService userService)
    {
        _subMerchantUserRepository = subMerchantUserRepository;
        _mapper = mapper;
        _vaultClient = vaultClient;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
        _logger = logger;
        _parameterService = parameterService;
        _searchService = searchService;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _userService = userService;
    }

    public async Task<Unit> Handle(UpdateSubMerchantUserCommand request, CancellationToken cancellationToken)
    {
        var subMerchantUser = await _subMerchantUserRepository.GetByIdAsync(request.Id);

        if (subMerchantUser is null)
        {
            throw new NotFoundException(nameof(SubMerchantUser), request.Id);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            if (request.RecordStatus == RecordStatus.Passive)
            {
                var activeSubMerchants = await _subMerchantUserRepository
                    .GetAll().Include(b => b.SubMerchant)
                    .Where(b => b.SubMerchantId == subMerchantUser.SubMerchantId && b.RecordStatus == RecordStatus.Active)
                    .ToListAsync();

                if (activeSubMerchants.Count() < 2 && activeSubMerchants.FirstOrDefault()?.SubMerchant.RecordStatus == RecordStatus.Active)
                {
                    var exceptionMessage = _localizer.GetString("SubMerchantRemoveException");
                    throw new SubMerchantRemoveException(exceptionMessage.Value);
                }
            }

            var maxSubMerchantUserCount = (await _parameterService.GetParameterAsync("SubMerchants", "MaxSubMerchantUserCount")).ParameterValue;
            var totalSubMerchantUserCount = await _subMerchantUserRepository
                    .GetAll()
                    .Where(b => b.SubMerchantId == request.SubMerchantId && b.RecordStatus == RecordStatus.Active)
                    .CountAsync();

            if (totalSubMerchantUserCount > Convert.ToInt32(maxSubMerchantUserCount))
            {
                throw new SubMerchantUserCountException(_localizer.GetString("SubMerchantUserCountException").Value);
            }

            subMerchantUser = _mapper.Map(request, subMerchantUser);

            var IsBlacklistCheckEnabled =
                _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");

            if (IsBlacklistCheckEnabled)
            {
                await UserBlacklistControlAsync(subMerchantUser);
            }
            
            var username = 
                string.Concat(UserTypePrefix.CorporateSubMerchant, request.PhoneCode, request.MobilePhoneNumber).Replace("+", "");

            if (request.UserId == Guid.Empty)
            {
                var user = await _userService.CreateUserAsync(new CreateUserRequest()
                {
                    Email = request.Email,
                    FirstName = request.Name,
                    LastName = request.Surname,
                    BirthDate = request.BirthDate,
                    PhoneNumber = request.MobilePhoneNumber,
                    Roles = new List<Guid> { Guid.Parse(request.RoleId) },
                    UserType = UserType.CorporateSubMerchant,
                    PhoneCode = request.PhoneCode,
                    UserName = username,
                    IsBlacklistControl = true // true ise blacklistten donen reference number ı gonder
                });
                request.UserId = user.UserId;
            }
            else
            {
                await _userService.UpdateUserAsync(new UpdateUserWithUserName
                {
                    RecordStatus = request.RecordStatus,
                    Email = request.Email,
                    FirstName = request.Name,
                    LastName = request.Surname,
                    BirthDate = request.BirthDate,
                    PhoneCode = request.PhoneCode,
                    PhoneNumber = request.MobilePhoneNumber,
                    Roles = new List<Guid> { Guid.Parse(request.RoleId) },
                    UserType = UserType.CorporateSubMerchant,
                    Id = request.UserId,
                    UserName = username,
                    IsBlacklistControl = true
                });
            }
            
            await _subMerchantUserRepository.UpdateAsync(subMerchantUser);
            
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateSubMerchantUser",
                    SourceApplication = "PF",
                    Resource = "SubMerchantUser",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", subMerchantUser.Id.ToString()},
                        {"Name", request.Name},
                        {"PhoneNumber", request.MobilePhoneNumber},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantUserUpdateError : {exception}");
            throw;
        }

        return Unit.Value;
    }

    private async Task UserBlacklistControlAsync(SubMerchantUser subMerchantUser)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        SearchByNameRequest searchRequest = new()
        {
            Name = $"{subMerchantUser.Name} {subMerchantUser.Surname}",
            BirthYear = subMerchantUser.BirthDate.Year.ToString(),
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
    }
}
