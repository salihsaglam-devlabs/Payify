using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantDeductions;
using LinkPara.PF.Application.Features.MerchantDeductions.Queries.GetFilterMerchantDeductionQuery;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantDeductionService : IMerchantDeductionService
{
    private readonly PfDbContext _dbContext;
    private readonly IMapper _mapper;
    
    public MerchantDeductionService(PfDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<PaginatedList<MerchantDeductionDto>> GetFilterMerchantDeductionsAsync(
        GetFilterMerchantDeductionQuery request)
    {
        var merchantDeductions = _dbContext.MerchantDeduction
                .Include(d => d.Merchant)
                .AsQueryable();

            merchantDeductions = request.RecordStatus is not null ?
            merchantDeductions.Where(b => b.RecordStatus == request.RecordStatus) :
            merchantDeductions.Where(b => b.RecordStatus == RecordStatus.Active);

            if (request.MerchantTransactionId != Guid.Empty)
            {
                merchantDeductions = merchantDeductions.Where(d => d.MerchantTransactionId == request.MerchantTransactionId);
            }

            if (request.MerchantId != Guid.Empty)
            {
                merchantDeductions = merchantDeductions.Where(d => d.MerchantId == request.MerchantId);
            }

            if (request.MerchantDueId != Guid.Empty)
            {
                merchantDeductions = merchantDeductions.Where(d => d.MerchantDueId == request.MerchantDueId);
            }

            if (request.TotalDeductionAmountBiggerThan is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.TotalDeductionAmount >= request.TotalDeductionAmountBiggerThan);
            }

            if (request.TotalDeductionAmountSmallerThan is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.TotalDeductionAmount <= request.TotalDeductionAmountSmallerThan);
            }

            if (request.RemainingDeductionAmountBiggerThan is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.RemainingDeductionAmount >= request.RemainingDeductionAmountBiggerThan);
            }

            if (request.RemainingDeductionAmountSmallerThan is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.RemainingDeductionAmount <= request.RemainingDeductionAmountSmallerThan);
            }

            if (request.ExecutionDateStart is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.ExecutionDate >= request.ExecutionDateStart);
            }
            if (request.ExecutionDateEnd is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.ExecutionDate <= request.ExecutionDateEnd);
            }

            if (request.DeductionStatus is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.DeductionStatus == request.DeductionStatus);
            }

            if (request.DeductionType is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.DeductionType == request.DeductionType);
            }

            if (request.Currency is not null)
            {
                merchantDeductions = merchantDeductions.Where(d => d.Currency == request.Currency);
            }

            if (!string.IsNullOrEmpty(request.ConversationId))
            {
                merchantDeductions = merchantDeductions.Where(d => d.ConversationId.Contains(request.ConversationId));
            }

            return await merchantDeductions
            .PaginatedListWithMappingAsync<MerchantDeduction, MerchantDeductionDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}