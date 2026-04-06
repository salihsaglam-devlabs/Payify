using System.ComponentModel.DataAnnotations;
using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Commons.Models.IdentityModels;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.SubMerchantUsers.Command.SaveSubMerchantUser;

public class SaveSubMerchantUserCommand : IRequest, IMapFrom<SubMerchantUser>
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string IdentityNumber { get; set; }
    public Guid SubMerchantId { get; set; }
}

public class SaveSubMerchantUserCommandHandler : IRequestHandler<SaveSubMerchantUserCommand>
{
    private readonly IGenericRepository<SubMerchant> _subMerchantRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<SubMerchantUser> _subMerchantUserRepository;
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SaveSubMerchantUserCommandHandler> _logger;
    private readonly IUserService _userService;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;
    private readonly IRestrictionService _restrictionService;

    public SaveSubMerchantUserCommandHandler(
        IGenericRepository<SubMerchant> subMerchantRepository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<SubMerchantUser> subMerchantUserRepository,
        IGenericRepository<MerchantUser> merchantUserRepository,
        IContextProvider contextProvider,
        IMapper mapper,
        IAuditLogService auditLogService,
        ILogger<SaveSubMerchantUserCommandHandler> logger,
        IUserService userService,
        IParameterService parameterService,
        IStringLocalizerFactory factory,
        IRestrictionService restrictionService)
    {
        _subMerchantRepository = subMerchantRepository;
        _merchantRepository = merchantRepository;
        _subMerchantUserRepository = subMerchantUserRepository;
        _merchantUserRepository = merchantUserRepository;
        _contextProvider = contextProvider;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _logger = logger;
        _userService = userService;
        _parameterService = parameterService;
        _restrictionService = restrictionService;

        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task<Unit> Handle(SaveSubMerchantUserCommand request, CancellationToken cancellationToken)
    {
        await _restrictionService.RestrictMerchantTypes(new List<MerchantType> { MerchantType.StandartMerchant });

        var maxSubMerchantUserCount = (await _parameterService.GetParameterAsync("SubMerchants", "MaxSubMerchantUserCount")).ParameterValue;
        var totalSubMerchantUserCount = await _subMerchantUserRepository
                .GetAll()
                .Where(b => b.SubMerchantId == request.SubMerchantId && b.RecordStatus == RecordStatus.Active)
                .CountAsync();

        if (totalSubMerchantUserCount > Convert.ToInt32(maxSubMerchantUserCount))
        {
            throw new SubMerchantUserCountException(_localizer.GetString("SubMerchantUserCountException").Value);
        }

        var subMerchant = await _subMerchantRepository.GetByIdAsync(request.SubMerchantId) ??
                          throw new NotFoundException(nameof(SubMerchant), request.SubMerchantId);
        
        var merchant = await _merchantRepository.GetByIdAsync(subMerchant.MerchantId) ??
                          throw new NotFoundException(nameof(Merchant), subMerchant.MerchantId);
        
        var activeMerchantUser = await _merchantUserRepository.GetAll()
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == request.MobilePhoneNumber || b.Email == request.Email)
                && b.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        var activeUser = await _subMerchantUserRepository.GetAll()
            .FirstOrDefaultAsync(b =>
                (b.MobilePhoneNumber == request.MobilePhoneNumber || b.Email == request.Email)
                && b.RecordStatus == RecordStatus.Active, cancellationToken: cancellationToken);

        if (activeUser is not null || activeMerchantUser is not null)
        {
            throw new DuplicateRecordException();
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var user = await CreateUserAsync(request, merchant);

            var subMerchantUser = _mapper.Map<SubMerchantUser>(request);
            subMerchantUser.RecordStatus = RecordStatus.Active;
            subMerchantUser.UserId = user.UserId;

            await _subMerchantUserRepository.AddAsync(subMerchantUser);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "CreateSubMerchantUser",
                    SourceApplication = "PF",
                    Resource = "SubMerchantUser",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        { "Id", subMerchantUser.Id.ToString() },
                        { "Name", request.Name },
                        { "PhoneNumber", request.MobilePhoneNumber },
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantUserCreateError : {exception}");

            var informationMail =
                await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

            var exceptionMessage = _localizer.GetString("UserInBlacklistException");

            throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail",
                informationMail.ParameterValue));
        }

        return Unit.Value;
    }

    private async Task<UserCreateResponse> CreateUserAsync(SaveSubMerchantUserCommand subMerchantUser, Merchant merchant)
    {
        var roles = new List<Guid> { Guid.Parse(subMerchantUser.RoleId) };

        CreateUserRequest createUserRequest = new()
        {
            Email = subMerchantUser.Email,
            FirstName = subMerchantUser.Name,
            LastName = subMerchantUser.Surname,
            BirthDate = subMerchantUser.BirthDate,
            PhoneCode = merchant.PhoneCode,
            PhoneNumber = subMerchantUser.MobilePhoneNumber,
            UserType = UserType.CorporateSubMerchant,
            IsBlacklistControl = false,
            Roles = roles,
            UserName =
                string.Concat(UserTypePrefix.CorporateSubMerchant,
                    merchant.PhoneCode.Replace("+", ""),
                    subMerchantUser.MobilePhoneNumber)
        };

        var result = await _userService.CreateUserAsync(createUserRequest);

        return result;
    }
}