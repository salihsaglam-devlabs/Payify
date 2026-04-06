using AutoMapper;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Office2010.Excel;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchants.Command.ApproveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.DeleteSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.SaveSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateMultipleSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Command.UpdateSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants.Queries.GetAllSubMerchant;
using LinkPara.PF.Application.Features.SubMerchantUsers;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class SubMerchantService : ISubMerchantService
{
    private readonly ILogger<SubMerchantService> _logger;
    private readonly IGenericRepository<SubMerchant> _repository;
    private readonly IGenericRepository<SubMerchantLimit> _subMerchantLimitRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IRestrictionService _restrictionService;
    private readonly PfDbContext _context;
    private readonly IParameterService _parameterService;
    private readonly IStringLocalizer _localizer;

    private const string Prefix = "12";

    public SubMerchantService(ILogger<SubMerchantService> logger,
        IGenericRepository<SubMerchant> repository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<SubMerchantLimit> subMerchantLimitRepository,
        IRestrictionService restrictionService,
        IStringLocalizerFactory factory,
        PfDbContext context,
        IParameterService parameterService)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _subMerchantLimitRepository = subMerchantLimitRepository;
        _restrictionService = restrictionService;
        _context = context;
        _parameterService = parameterService;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
    }

    public async Task DeleteAsync(DeleteSubMerchantCommand request)
    {
        var subMerchant = await _repository.GetByIdAsync(request.Id);

        if (subMerchant is null)
        {
            throw new NotFoundException(nameof(SubMerchant), request.Id);
        }

        try
        {
            subMerchant.RecordStatus = RecordStatus.Passive;
            await _repository.UpdateAsync(subMerchant);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteSubMerchant",
                    SourceApplication = "PF",
                    Resource = "SubMerchant",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantDeleteError : {exception}");
            throw;
        }
    }

    public async Task<SubMerchantDto> GetByIdAsync(Guid id)
    {
        var subMerchant = await _context.SubMerchant
            .Include(b => b.SubMerchantLimits)
            .Include(b => b.SubMerchantDocuments)
            .Where(b => b.Id == id).Join(_context.Merchant,
         sub => sub.MerchantId,
         merchant => merchant.Id,
         (sub, merchant) => new SubMerchantDto
         {
             Id = sub.Id,
             MerchantId = sub.MerchantId,
             MerchantName = merchant.Name,
             MerchantNumber = merchant.Number,
             SubMerchantLimits = _mapper.Map<List<SubMerchantLimitDto>>(sub.SubMerchantLimits),
             SubMerchantDocuments = _mapper.Map<List<SubMerchantDocumentDto>>(sub.SubMerchantDocuments),
             IsOnUsPaymentPageAllowed = sub.IsOnUsPaymentPageAllowed,
             City = sub.City,
             CityName = sub.CityName,
             CreateDate = sub.CreateDate,
             InstallmentAllowed = sub.InstallmentAllowed,
             InternationalCardAllowed = sub.InternationalCardAllowed,
             Is3dRequired = sub.Is3dRequired,
             IsExcessReturnAllowed = sub.IsExcessReturnAllowed,
             IsLinkPaymentPageAllowed = sub.IsLinkPaymentPageAllowed,
             IsManuelPaymentPageAllowed = sub.IsManuelPaymentPageAllowed,
             MerchantType = sub.MerchantType,
             Name = sub.Name,
             Number = sub.Number,
             PaymentReturnAllowed = sub.PaymentReturnAllowed,
             PaymentReverseAllowed = sub.PaymentReverseAllowed,
             PreAuthorizationAllowed = sub.PreAuthorizationAllowed,
             RejectReason = sub.RejectReason,
             RecordStatus = sub.RecordStatus

         }).FirstOrDefaultAsync();

        if (subMerchant is null)
        {
            throw new NotFoundException(nameof(SubMerchant), id);
        }

        await _restrictionService.IsUserAuthorizedAsync(subMerchant.MerchantId);

        await _restrictionService.RestrictMerchantTypes(new List<MerchantType> { MerchantType.StandartMerchant });

        return subMerchant;
    }

    public async Task<PaginatedList<SubMerchantDto>> GetListAsync(GetAllSubMerchantQuery request)
    {
        await _restrictionService.RestrictMerchantTypes(new List<MerchantType> { MerchantType.StandartMerchant });

        var subMerchantList = _context.SubMerchant
                .Include(b => b.SubMerchantUsers)
                .AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            subMerchantList = subMerchantList.Where(b => b.Name.Contains(request.Q));
        }

        if (!string.IsNullOrEmpty(request.Number))
        {
            subMerchantList = subMerchantList.Where(b => b.Number.Contains(request.Number));
        }

        if (request.MerchantId is not null)
        {
            await _restrictionService.IsUserAuthorizedAsync(request.MerchantId.Value);

            subMerchantList = subMerchantList.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.CityCode is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.City
                               == request.CityCode);
        }

        if (request.IsLinkPaymentPageAllowed is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.IsLinkPaymentPageAllowed
                               == request.IsLinkPaymentPageAllowed);
        }

        if (request.IsOnUsPaymentPageAllowed is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.IsOnUsPaymentPageAllowed
                               == request.IsOnUsPaymentPageAllowed);
        }

        if (request.IsManuelPaymentPageAllowed is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.IsManuelPaymentPageAllowed
                               == request.IsManuelPaymentPageAllowed);
        }

        if (request.PaymentReturnAllowed is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.PaymentReturnAllowed
                               == request.PaymentReturnAllowed);
        }

        if (request.PaymentReverseAllowed is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.PaymentReverseAllowed
                               == request.PaymentReverseAllowed);
        }

        if (request.CreateDateStart is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.RecordStatus is not null)
        {
            subMerchantList = subMerchantList.Where(b => b.RecordStatus
                               == request.RecordStatus);
        }

        var mapSubMerchantList = await subMerchantList.Join(_context.Merchant,
          sub => sub.MerchantId,
          merchant => merchant.Id,
          (sub, merchant) => new SubMerchantDto
          {
              Id = sub.Id,
              MerchantId = sub.MerchantId,
              MerchantName = merchant.Name,
              MerchantNumber = merchant.Number,
              SubMerchantUsers = _mapper.Map<List<SubMerchantUserModel>>(sub.SubMerchantUsers),
              IsOnUsPaymentPageAllowed = sub.IsOnUsPaymentPageAllowed,
              City = sub.City,
              CityName = sub.CityName,
              CreateDate = sub.CreateDate,
              InstallmentAllowed = sub.InstallmentAllowed,
              InternationalCardAllowed = sub.InternationalCardAllowed,
              Is3dRequired = sub.Is3dRequired,
              IsExcessReturnAllowed = sub.IsExcessReturnAllowed,
              IsLinkPaymentPageAllowed = sub.IsLinkPaymentPageAllowed,
              IsManuelPaymentPageAllowed = sub.IsManuelPaymentPageAllowed,
              MerchantType = sub.MerchantType,
              Name = sub.Name,
              Number = sub.Number,
              PaymentReturnAllowed = sub.PaymentReturnAllowed,
              PaymentReverseAllowed = sub.PaymentReverseAllowed,
              PreAuthorizationAllowed = sub.PreAuthorizationAllowed,
              RejectReason = sub.RejectReason,
              RecordStatus = sub.RecordStatus

          })
          .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);

        return mapSubMerchantList;
    }

    public async Task<Guid> SaveAsync(SaveSubMerchantCommand request)
    {
        await _restrictionService.RestrictMerchantTypes(new List<MerchantType> { MerchantType.StandartMerchant });

        var maxSubMerchantCount = (await _parameterService.GetParameterAsync("SubMerchants", "MaxSubMerchantCount")).ParameterValue;
        var totalSubMerchantCount = await _repository
                .GetAll()
                .Where(b => b.MerchantId == request.MerchantId && b.RecordStatus == RecordStatus.Active)
                .CountAsync();

        if (totalSubMerchantCount > Convert.ToInt32(maxSubMerchantCount))
        {
            throw new SubMerchantCountException(_localizer.GetString("SubMerchantCountException").Value);
        }

        var activeSubMerchant = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Name == request.Name && b.MerchantId == request.MerchantId);

        if (activeSubMerchant is not null)
        {
            throw new DuplicateRecordException(nameof(SubMerchant), request.Name);
        }

        var totalSubMerchants = await _repository.GetAll().CountAsync();
        var subMerchantNumber = $"{Prefix}{(totalSubMerchants + 1).ToString().PadLeft(8, '0')}";

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            var subMerchant = _mapper.Map<SubMerchant>(request);
            subMerchant.Number = subMerchantNumber;
            subMerchant.CreateDate = DateTime.Now;
            subMerchant.CreatedBy = parseUserId.ToString();

            _context.Add(subMerchant);
            _context.SaveChanges();

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveSubMerchant",
                    SourceApplication = "PF",
                    Resource = "SubMerchant",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Name", request.Name },
                        {"Number", subMerchant.Number},
                        {"CityName", request.CityName }
                    }
                });

            return subMerchant.Id;
        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantCreateError : {exception}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateSubMerchantCommand request)
    {
        var subMerchant = await _repository.GetAll()
            .Include(b => b.SubMerchantLimits)
            .Include(b => b.SubMerchantDocuments)
            .FirstOrDefaultAsync(b => b.Id == request.Id);

        if (subMerchant is null)
        {
            throw new NotFoundException(nameof(SubMerchant), request.Id);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            subMerchant.IsManuelPaymentPageAllowed = request.IsManuelPaymentPageAllowed;
            subMerchant.IsLinkPaymentPageAllowed = request.IsLinkPaymentPageAllowed;
            subMerchant.IsOnUsPaymentPageAllowed = request.IsOnUsPaymentPageAllowed;
            subMerchant.Is3dRequired = request.Is3dRequired;
            subMerchant.InstallmentAllowed = request.InstallmentAllowed;
            subMerchant.InternationalCardAllowed = request.InternationalCardAllowed;
            subMerchant.IsExcessReturnAllowed = request.IsExcessReturnAllowed;
            subMerchant.PaymentReturnAllowed = request.PaymentReturnAllowed;
            subMerchant.PaymentReverseAllowed = request.PaymentReverseAllowed;
            subMerchant.PreAuthorizationAllowed = request.PreAuthorizationAllowed;

            await _repository.UpdateAsync(subMerchant);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantUpdateError : {exception}");
            throw;
        }
    }

    public async Task ApproveAsync(ApproveSubMerchantCommand request)
    {
        var subMerchant = await _repository.GetByIdAsync(request.SubMerchantId);

        if (subMerchant is null)
        {
            throw new NotFoundException(nameof(SubMerchant), request.SubMerchantId);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        try
        {
            if (request.IsApprove)
            {

                var maxSubMerchantCount = (await _parameterService.GetParameterAsync("SubMerchants", "MaxSubMerchantCount")).ParameterValue;
                var totalSubMerchantCount = await _repository
                        .GetAll()
                        .Where(b => b.MerchantId == subMerchant.MerchantId && b.RecordStatus == RecordStatus.Active)
                        .CountAsync();

                if (totalSubMerchantCount > Convert.ToInt32(maxSubMerchantCount))
                {
                    throw new SubMerchantCountException(_localizer.GetString("SubMerchantCountException").Value);
                }

                subMerchant.RecordStatus = RecordStatus.Active;
                subMerchant.RejectReason = string.Empty;
                subMerchant.ParameterValue = string.Empty;
            }
            else
            {
                subMerchant.RecordStatus = RecordStatus.Passive;
                subMerchant.RejectReason = request.RejectReason;
                subMerchant.ParameterValue = request.ParameterValue;
            }

            await _repository.UpdateAsync(subMerchant);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantApproveError : {exception}");
            throw;
        }
    }

    public async Task UpdateMultipleAsync(UpdateMultipleSubMerchantCommand request)
    {
        try
        {
            foreach (var subMerchantId in request.SubMerchants)
            {
                var subMerchant = await _repository.GetAll().Include(b => b.SubMerchantLimits).FirstOrDefaultAsync(b => b.Id == subMerchantId);

                if (subMerchant is null)
                {
                    throw new NotFoundException(nameof(SubMerchant), subMerchantId);
                }

                subMerchant.IsManuelPaymentPageAllowed =
                    request.IsManuelPaymentPageAllowed ?? subMerchant.IsManuelPaymentPageAllowed;

                subMerchant.IsLinkPaymentPageAllowed =
                    request.IsLinkPaymentPageAllowed ?? subMerchant.IsLinkPaymentPageAllowed;

                subMerchant.IsOnUsPaymentPageAllowed =
                   request.IsOnUsPaymentPageAllowed ?? subMerchant.IsOnUsPaymentPageAllowed;

                subMerchant.PreAuthorizationAllowed =
                    request.PreAuthorizationAllowed ?? subMerchant.PreAuthorizationAllowed;

                subMerchant.PaymentReverseAllowed =
                    request.PaymentReverseAllowed ?? subMerchant.PaymentReverseAllowed;

                subMerchant.PaymentReturnAllowed =
                    request.PaymentReturnAllowed ?? subMerchant.PaymentReturnAllowed;

                subMerchant.InstallmentAllowed =
                    request.InstallmentAllowed ?? subMerchant.InstallmentAllowed;

                subMerchant.Is3dRequired =
                    request.Is3dRequired ?? subMerchant.Is3dRequired;

                subMerchant.IsExcessReturnAllowed =
                    request.IsExcessReturnAllowed ?? subMerchant.IsExcessReturnAllowed;

                subMerchant.InternationalCardAllowed =
                    request.InternationalCardAllowed ?? subMerchant.InternationalCardAllowed;

                if (request.Limits is not null && request.Limits.Count > 0)
                {
                    var userId = _contextProvider.CurrentContext.UserId;
                    var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                    var limitList = new List<SubMerchantLimitDto>();

                    foreach (var limit in request.Limits)
                    {
                        var activeLimits = await _subMerchantLimitRepository.GetAll().Where(d => d.SubMerchantId == subMerchantId &&
                                     d.TransactionLimitType == limit.TransactionLimitType &&
                                     d.Period == limit.Period &&
                                     d.LimitType == limit.LimitType &&
                                     d.RecordStatus == RecordStatus.Active).ToListAsync();

                        if (activeLimits.Any())
                        {
                            activeLimits.ForEach(b => b.RecordStatus = RecordStatus.Passive);
                            await _subMerchantLimitRepository.UpdateRangeAsync(activeLimits);
                        }

                        limitList.Add(limit);
                    }

                    var map = _mapper.Map<List<SubMerchantLimit>>(limitList).Select(b =>
                    {
                        b.SubMerchantId = subMerchant.Id;
                        b.CreatedBy = parseUserId.ToString();
                        b.CreateDate = DateTime.Now;
                        return b;
                    }).ToList();

                    await _subMerchantLimitRepository.AddRangeAsync(map);
                }
                await _repository.UpdateAsync(subMerchant);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SubMerchantUpdateMultipleError : {exception}");
            throw;
        }
    }
}
