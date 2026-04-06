using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.SaveSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Commands.UpdateSubMerchantLimit;
using LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetAllSubMerchantLimits;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class SubMerchantLimitService : ISubMerchantLimitService
{
    private readonly IGenericRepository<SubMerchantLimit> _subMerchantLimitRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<SubMerchantLimitService> _logger;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    
    public SubMerchantLimitService(IGenericRepository<SubMerchantLimit> subMerchantLimitRepository, IMapper mapper, ILogger<SubMerchantLimitService> logger, IContextProvider contextProvider, IAuditLogService auditLogService)
    {
        _subMerchantLimitRepository = subMerchantLimitRepository;
        _mapper = mapper;
        _logger = logger;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }
    public async Task<PaginatedList<SubMerchantLimitDto>> GetListAsync(GetAllSubMerchantLimitsQuery request)
    {
        var queryResponse = _subMerchantLimitRepository.GetAll().AsQueryable();

        if (request.Id is not null)
        {
            queryResponse = queryResponse.Where(d => d.Id == request.Id);
        }

        if (!string.IsNullOrEmpty(request.Currency))
        {
            queryResponse = queryResponse.Where(b => b.Currency.Contains(request.Currency));
        }

        if (request.SubMerchantId is not null)
        {
            queryResponse = queryResponse.Where(d => d.SubMerchantId == request.SubMerchantId);
        }

        if (request.TransactionLimitType is not null)
        {
            queryResponse = queryResponse.Where(d => d.TransactionLimitType == request.TransactionLimitType);
        }
        
        if (request.Period is not null)
        {
            queryResponse = queryResponse.Where(d => d.Period == request.Period);
        }
        
        if (request.LimitType is not null)
        {
            queryResponse = queryResponse.Where(d => d.LimitType == request.LimitType);
        }
        
        if (request.MaxAmount is not null)
        {
            queryResponse = queryResponse.Where(d => d.MaxAmount == request.MaxAmount);
        }
        
        if (request.MaxPiece is not null)
        {
            queryResponse = queryResponse.Where(d => d.MaxPiece == request.MaxPiece);
        }
        
        if (request.TransactionLimitType is not null)
        {
            queryResponse = queryResponse.Where(d => d.TransactionLimitType == request.TransactionLimitType);
        }
        
        if (request.RecordStatus is not null)
        {
            queryResponse = queryResponse.Where(d => d.RecordStatus == request.RecordStatus);
        }

        return await queryResponse
            .PaginatedListWithMappingAsync<SubMerchantLimit, SubMerchantLimitDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<SubMerchantLimitDto> GetByIdAsync(Guid id)
    {
        var subMerchantLimit = await _subMerchantLimitRepository.GetByIdAsync(id);

        if (subMerchantLimit is null)
        {
            throw new NotFoundException(nameof(SubMerchantLimit), id);
        }

        return _mapper.Map<SubMerchantLimitDto>(subMerchantLimit);
    }

    public async Task DeleteAsync(Guid id)
    {
        var subMerchantLimit = await _subMerchantLimitRepository.GetByIdAsync(id);

        if (subMerchantLimit is null)
        {
            throw new NotFoundException(nameof(subMerchantLimit), id);
        }

        await _subMerchantLimitRepository.DeleteAsync(subMerchantLimit);
    }

    public async Task SaveAsync(SaveSubMerchantLimitCommand request)
    {
        var activeSubMerchantLimit = await _subMerchantLimitRepository.GetAll()
            .FirstOrDefaultAsync(d => d.SubMerchantId == request.SubMerchantId &&
                                      d.TransactionLimitType == request.TransactionLimitType &&
                                      d.Period == request.Period &&
                                      d.LimitType == request.LimitType &&
                                      d.RecordStatus == RecordStatus.Active);

        if (activeSubMerchantLimit is not null)
        {
            throw new DuplicateRecordException(nameof(SubMerchantLimit), request);
        }

        try
        {
            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        
            var newSubMerhantLimit = _mapper.Map<SubMerchantLimit>(request);
            newSubMerhantLimit.CreateDate = DateTime.Now;
            newSubMerhantLimit.CreatedBy = parseUserId.ToString();
            newSubMerhantLimit.RecordStatus = RecordStatus.Active;
        
            await _subMerchantLimitRepository.AddAsync(newSubMerhantLimit);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "SaveSubMerchantLimit",
                    SourceApplication = "PF",
                    Resource = "SubMerchantLimit",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        { "TransactionLimitType", request.TransactionLimitType.ToString() },
                        { "LimitType", request.LimitType.ToString() },
                        { "Period", request.Period.ToString() },
                        { "SubMerchantNumber", request.SubMerchantId.ToString() }
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "SubMerchantDocumentCreateError : {exception}", exception);
            throw;
        }
    }

    public async Task UpdateAsync(UpdateSubMerchantLimitCommand request)
    {
        var requestDto = request.SubMerchantLimit;
        
        var subMerchantLimit = await _subMerchantLimitRepository.GetByIdAsync(requestDto.Id);

        if (subMerchantLimit is null)
        {
            throw new NotFoundException(nameof(SubMerchantLimit), requestDto.Id);
        }

        var activeSubMerchantLimit = await _subMerchantLimitRepository.GetAll()
           .FirstOrDefaultAsync(d => d.SubMerchantId == request.SubMerchantLimit.SubMerchantId &&
                                     d.TransactionLimitType == request.SubMerchantLimit.TransactionLimitType &&
                                     d.Period == request.SubMerchantLimit.Period &&
                                     d.LimitType == request.SubMerchantLimit.LimitType &&
                                     d.RecordStatus == RecordStatus.Active &&
                                     d.Id != requestDto.Id);

        if (activeSubMerchantLimit is not null)
        {
            throw new DuplicateRecordException(nameof(SubMerchantLimit), request.SubMerchantLimit);
        }

        subMerchantLimit.LimitType = requestDto.LimitType;
        subMerchantLimit.Period = requestDto.Period;
        subMerchantLimit.TransactionLimitType = requestDto.TransactionLimitType;
        subMerchantLimit.MaxAmount = requestDto.MaxAmount;
        subMerchantLimit.Currency = requestDto.Currency;
        subMerchantLimit.MaxPiece = requestDto.MaxPiece;
        subMerchantLimit.UpdateDate = DateTime.Now;
        subMerchantLimit.RecordStatus = requestDto.RecordStatus;
        
        await _subMerchantLimitRepository.UpdateAsync(subMerchantLimit);
    }
}