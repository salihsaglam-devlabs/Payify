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
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Application.Features.MerchantUsers;
using LinkPara.PF.Application.Features.MerchantUsers.Command.SaveMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Command.UpdateMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Queries.GetAllMerchantUser;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantUserService : IMerchantUserService
{
    private readonly IGenericRepository<MerchantUser> _repository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<MerchantUserService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IUserService _userService;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    private readonly IVaultClient _vaultClient;
    private readonly ISearchService _searchService;

    public MerchantUserService(
        IGenericRepository<MerchantUser> repository,
        IGenericRepository<SubMerchantUser> subMerchantUserRepository,
        IMapper mapper,
        ILogger<MerchantUserService> logger,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<Merchant> merchantRepository,
        IUserService userService,
        IStringLocalizerFactory factory,
        IParameterService parameterService,
        IVaultClient vaultClient,
        ISearchService searchService)
    {
        _repository = repository;
        _subMerchantUserRepository = subMerchantUserRepository;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _merchantRepository = merchantRepository;
        _userService = userService;
        _parameterService = parameterService;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _vaultClient = vaultClient;
        _searchService = searchService;
    }

    public async Task<PaginatedList<MerchantUserDto>> GetAllAsync(GetAllMerchantUserQuery query)
    {
        var merchantUsers = _repository.GetAll();

        if (query.MerchantId is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.MerchantId == query.MerchantId);
        }

        if (!string.IsNullOrEmpty(query.Fullname))
        {
            merchantUsers = merchantUsers.Where(b =>
                                       (b.Name.ToLower() + " " + b.Surname.ToLower())
                                       .Contains(query.Fullname.ToLower()));
        }

        if (!string.IsNullOrEmpty(query.Email))
        {
            merchantUsers = merchantUsers.Where(b => b.Email.ToLower().Contains(query.Email.ToLower()));
        }

        if (query.BirthDate is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.BirthDate
                               == query.BirthDate);
        }

        if (!string.IsNullOrEmpty(query.PhoneNumber))
        {
            merchantUsers = merchantUsers.Where(b => (b.MobilePhoneNumber).Contains(query.PhoneNumber));
        }

        if (!string.IsNullOrEmpty(query.RoleId))
        {
            merchantUsers = merchantUsers.Where(b => b.RoleId == query.RoleId);
        }

        if (query.UserId is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.UserId == query.UserId);
        }

        if (query.CreateDateStart is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.CreateDate
                               >= query.CreateDateStart);
        }

        if (query.CreateDateEnd is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.CreateDate
                               <= query.CreateDateEnd);
        }

        if (query.RecordStatus is not null)
        {
            merchantUsers = merchantUsers.Where(b => b.RecordStatus
                               == query.RecordStatus);
        }

        return await merchantUsers
          .PaginatedListWithMappingAsync<MerchantUser, MerchantUserDto>(_mapper, query.Page, query.Size, query.OrderBy, query.SortBy);
    }

    public async Task<MerchantUserDto> GetByIdAsync(Guid id)
    {
        var merchantUser = await _repository.GetByIdAsync(id);

        if (merchantUser is null)
        {
            throw new NotFoundException(nameof(MerchantUser), id);
        }

        var merchant = await _merchantRepository.GetByIdAsync(merchantUser.MerchantId);

        var map = _mapper.Map<MerchantUserDto>(merchantUser);
        map.PhoneCode = merchant?.PhoneCode;

        return map;
    }

    public async Task SaveAsync(SaveMerchantUserCommand request)
    {
        var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId) ?? throw new NotFoundException(nameof(Merchant), request.MerchantId);

        var activeUser = await _repository.GetAll()
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == request.MobilePhoneNumber || b.Email == request.Email)
                && b.RecordStatus == RecordStatus.Active);

        var activeSubMerchantUser = await _subMerchantUserRepository.GetAll()
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == request.MobilePhoneNumber || b.Email == request.Email)
                && b.RecordStatus == RecordStatus.Active);

        if (activeUser is not null || activeSubMerchantUser is not null)
        {
            throw new DuplicateRecordException();
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var user = await CreateUserAsync(request, merchant);

            var merchantUser = _mapper.Map<MerchantUser>(request);
            merchantUser.RecordStatus = RecordStatus.Active;
            merchantUser.UserId = user.UserId;

            await _repository.AddAsync(merchantUser);

            await _auditLogService.AuditLogAsync(
           new AuditLog
           {
               IsSuccess = true,
               LogDate = DateTime.Now,
               Operation = "CreateMerchantUser",
               SourceApplication = "PF",
               Resource = "MerchantUser",
               UserId = parseUserId,
               Details = new Dictionary<string, string>
               {
                    {"Id", merchantUser.Id.ToString()},
                    {"Name", request.Name},
                    {"PhoneNumber", request.MobilePhoneNumber},
               }
           });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantUserCreateError : {exception}");

            var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
        }
    }

    public async Task UpdateAsync(UpdateMerchantUserCommand request)
    {
        var merchantUser = await _repository.GetByIdAsync(request.Id);

        if (merchantUser is null)
        {
            throw new NotFoundException(nameof(MerchantUser), request.Id);
        }

        var oldmerchantUser = $"{merchantUser.Name}{merchantUser.Surname}";

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            merchantUser = _mapper.Map(request, merchantUser);

            string amlReferenceNumber = null;
            if (!oldmerchantUser.Equals($"{merchantUser.Name}{merchantUser.Surname}"))
            {
                var IsBlacklistCheckEnabled =
            _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");

                if (IsBlacklistCheckEnabled)
                {
                    amlReferenceNumber = await UserBlacklistControlAsync(merchantUser);
                }
            }

            var username =
                string.Concat(UserTypePrefix.Corporate, request.PhoneCode, request.MobilePhoneNumber).Replace("+", "");

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
                    UserType = UserType.Corporate,
                    PhoneCode = request.PhoneCode,
                    UserName = username,
                    IsBlacklistControl = true,
                    AmlReferenceNumber = amlReferenceNumber
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
                    UserType = UserType.Corporate,
                    Id = request.UserId,
                    UserName = username,
                    IsBlacklistControl = true,
                    AmlReferenceNumber = amlReferenceNumber
                });
            }

            await _repository.UpdateAsync(merchantUser);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateMerchantUser",
                    SourceApplication = "PF",
                    Resource = "MerchantUser",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", merchantUser.Id.ToString()},
                        {"Name", request.Name},
                        {"PhoneNumber", request.MobilePhoneNumber},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantUserUpdateError : {exception}");
            throw;
        }
    }
    private async Task<string> UserBlacklistControlAsync(MerchantUser merchantUser)
    {
        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

        SearchByNameRequest searchRequest = new()
        {
            Name = $"{merchantUser.Name} {merchantUser.Surname}",
            BirthYear = merchantUser.BirthDate.Year.ToString(),
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
        return blackListControl.ReferenceNumber;
    }
    private async Task<UserCreateResponse> CreateUserAsync(SaveMerchantUserCommand merchantUser, Merchant merchant)
    {
        var roles = new List<Guid> { Guid.Parse(merchantUser.RoleId) };

        CreateUserRequest createUserRequest = new()
        {
            Email = merchantUser.Email,
            FirstName = merchantUser.Name,
            LastName = merchantUser.Surname,
            BirthDate = merchantUser.BirthDate,
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = merchantUser.MobilePhoneNumber,
            UserType = UserType.Corporate,
            IsBlacklistControl = false,
            Roles = roles,
            UserName =
                string.Concat(UserTypePrefix.Corporate,
                    merchant.PhoneCode.Replace("+", ""),
                    merchantUser.MobilePhoneNumber)
        };

        var result = await _userService.CreateUserAsync(createUserRequest);

        return result;
    }
}
