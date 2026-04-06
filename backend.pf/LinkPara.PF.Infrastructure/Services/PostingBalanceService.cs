using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.PostingBalances;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetAllPostingBalances;
using LinkPara.PF.Application.Features.PostingBalances.Queries.GetPostingBalanceStatistics;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.Services;

public class PostingBalanceService : IPostingBalanceService
{
    private readonly PfDbContext _dbContext;
    private readonly IMapper _mapper;
    public PostingBalanceService(PfDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }
    public async Task<PostingBalanceResponse> GetAllAsync(GetAllPostingBalanceQuery request)
    {
        var postingBalances = GetPostingBalanceQuery();

        if (request.MerchantId is not null)
        {
            postingBalances = postingBalances.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.ParentMerchantId is not null)
        {
            postingBalances = postingBalances.Where(b => b.ParentMerchantId == request.ParentMerchantId);
        }

        if (request.BlockageStatus is not null)
        {
            postingBalances = postingBalances.Where(b => b.BlockageStatus == request.BlockageStatus);
        }

        if (request.TransactionDateStart is not null)
        {
            postingBalances = postingBalances.Where(b => b.TransactionDate
                               >= request.TransactionDateStart);
        }

        if (request.TransactionDateEnd is not null)
        {
            postingBalances = postingBalances.Where(b => b.TransactionDate
                               <= request.TransactionDateEnd);
        }

        if (request.PaymentDateStart is not null)
        {
            postingBalances = postingBalances.Where(b => b.PaymentDate
                               >= request.PaymentDateStart);
        }

        if (request.PaymentDateEnd is not null)
        {
            postingBalances = postingBalances.Where(b => b.PaymentDate
                               <= request.PaymentDateEnd);
        }
        
        if (request.MoneyTransferStatus is not null)
        {
            postingBalances = postingBalances.Where(b => request.MoneyTransferStatus.Contains(b.MoneyTransferStatus));
        }
        
        if (request.PostingBalanceType is not null)
        {
            postingBalances = postingBalances.Where(b => request.PostingBalanceType.Contains(b.PostingBalanceType));
        }
        
        if (request.PostingPaymentChannel is not null)
        {
            postingBalances = postingBalances.Where(b => b.PostingPaymentChannel == request.PostingPaymentChannel);
        }
        
        var response = new PostingBalanceResponse
        {
            TotalAmount = postingBalances.Sum(b => b.TotalAmount),
            TotalTransactionCount = postingBalances.Sum(b => b.TransactionCount),
            TotalChargebackAmount = postingBalances.Sum(b => b.TotalChargebackAmount),
            TotalChargebackCommissionAmount = postingBalances.Sum(b => b.TotalChargebackCommissionAmount),
            TotalChargebackTransferAmount = postingBalances.Sum(b => b.TotalChargebackTransferAmount),
            TotalSuspiciousAmount = postingBalances.Sum(b => b.TotalSuspiciousAmount),
            TotalSuspiciousCommissionAmount = postingBalances.Sum(b => b.TotalSuspiciousCommissionAmount),
            TotalSuspiciousTransferAmount = postingBalances.Sum(b => b.TotalSuspiciousTransferAmount),
            TotalPayingAmount = postingBalances.Sum(b => b.TotalPayingAmount),
            TotalPfCommissionAmount = postingBalances.Sum(b => b.TotalPfCommissionAmount),
            TotalBankCommissionAmount = postingBalances.Sum(b => b.TotalBankCommissionAmount),
            TotalDueAmount = postingBalances.Sum(b => b.TotalDueAmount),
            TotalDueTransferAmount = postingBalances.Sum(b => b.TotalDueTransferAmount),
            TotalExcessReturnAmount = postingBalances.Sum(b => b.TotalExcessReturnAmount),
            TotalExcessReturnTransferAmount = postingBalances.Sum(b => b.TotalExcessReturnTransferAmount),
            TotalExcessReturnOnCommissionAmount = postingBalances.Sum(b => b.TotalExcessReturnOnCommissionAmount),
            TotalSubmerchantDeductionAmount = postingBalances.Sum(b => b.TotalSubmerchantDeductionAmount),
            TotalParentMerchantCommissionAmount = postingBalances.Sum(b => b.TotalParentMerchantCommissionAmount),
            
            PostingBalances = await postingBalances
               .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy)
        };

        return response;
    }

    public async Task<PostingBalanceDto> GetByIdAsync(Guid id)
    {
        var postingBalance = await (
             from p in _dbContext.PostingBalance
             where p.Id == id
             join m in _dbContext.Merchant on p.MerchantId equals m.Id
             join parent in _dbContext.Merchant on m.ParentMerchantId equals parent.Id into parentGroup
             from parentMerchant in parentGroup.DefaultIfEmpty()
             select new PostingBalanceDto
             {
                 Id = p.Id,
                 BlockageStatus = p.BlockageStatus,
                 PostingDate = p.PostingDate,
                 PaymentDate = p.PaymentDate,
                 ParentMerchantName = parentMerchant != null ? parentMerchant.Name : null,
                 ParentMerchantNumber = parentMerchant != null ? parentMerchant.Number : null,
                 ParentMerchantId = p.Merchant.ParentMerchantId,
                 CreateDate = p.CreateDate,
                 OldPaymentDate = p.OldPaymentDate,
                 TotalAmount = p.TotalAmount,
                 Currency = p.Currency,
                 TransactionDate = p.TransactionDate,
                 TotalAmountWithoutBankCommission = p.TotalAmountWithoutBankCommission,
                 TotalBankCommissionAmount = p.TotalBankCommissionAmount,
                 TotalAmountWithoutCommissions = p.TotalAmountWithoutCommissions,
                 TotalPfCommissionAmount = p.TotalPfCommissionAmount,
                 TotalPfNetCommissionAmount = p.TotalPfNetCommissionAmount,
                 TotalDueAmount = p.TotalDueAmount,
                 TotalDueTransferAmount = p.TotalDueTransferAmount,
                 TotalPointAmount = p.TotalPointAmount,
                 TotalChargebackAmount = p.TotalChargebackAmount,
                 TotalChargebackCommissionAmount = p.TotalChargebackCommissionAmount,
                 TotalChargebackTransferAmount = p.TotalChargebackTransferAmount,
                 TotalSuspiciousAmount = p.TotalSuspiciousAmount,
                 TotalSuspiciousCommissionAmount = p.TotalSuspiciousCommissionAmount,
                 TotalSuspiciousTransferAmount = p.TotalSuspiciousTransferAmount,
                 TotalExcessReturnAmount = p.TotalExcessReturnAmount,
                 TotalExcessReturnTransferAmount = p.TotalExcessReturnTransferAmount,
                 TotalExcessReturnOnCommissionAmount = p.TotalExcessReturnOnCommissionAmount,
                 TotalSubmerchantDeductionAmount = 
                     p.TotalChargebackCommissionAmount +
                     p.TotalSuspiciousCommissionAmount +
                     p.TotalChargebackTransferAmount +
                     p.TotalSuspiciousTransferAmount +
                     p.TotalDueTransferAmount +
                     p.TotalExcessReturnTransferAmount +
                     p.TotalExcessReturnOnCommissionAmount,
                 TotalNegativeBalanceAmount = p.TotalNegativeBalanceAmount,
                 TotalPayingAmount = p.TotalPayingAmount,
                 TransactionCount = p.TransactionCount,
                 MerchantId = p.MerchantId,
                 Merchant = _mapper.Map<TransactionMerchantResponse>(p.Merchant),
                 MoneyTransferBankName = p.MoneyTransferBankName,
                 MoneyTransferBankCode = p.MoneyTransferBankCode,
                 MoneyTransferStatus = p.MoneyTransferStatus,
                 PostingPaymentChannel = p.PostingPaymentChannel
             }
         ).FirstOrDefaultAsync();

        if (postingBalance is null)
        {
            throw new NotFoundException(nameof(PostingBalance), id);
        }

        return postingBalance;
    }

    public async Task<PostingBalanceStatisticsResponse> GetStatisticsAsync(GetPostingBalanceStatisticsQuery request)
    {
        var postingBalances = GetPostingBalanceQuery();

        if (request.MerchantId is not null)
        {
            postingBalances = postingBalances.Where(b => b.MerchantId == request.MerchantId);
        }
        
        if (request.BlockageStatus is not null)
        {
            postingBalances = postingBalances.Where(b => b.BlockageStatus == request.BlockageStatus);
        }
        
        if (request.PaymentDateStart is not null)
        {
            postingBalances = postingBalances.Where(b => b.PaymentDate
                                                         >= request.PaymentDateStart);
        }
        
        if (request.PaymentDateEnd is not null)
        {
            postingBalances = postingBalances.Where(b => b.PaymentDate
                                                         <= request.PaymentDateEnd);
        }
        
        if (request.MoneyTransferStatus is not null)
        {
            postingBalances = postingBalances.Where(b => request.MoneyTransferStatus.Contains(b.MoneyTransferStatus));
        }

        return new PostingBalanceStatisticsResponse
        {
            TotalAmount = postingBalances.Sum(b => b.TotalAmount),
            TotalTransactionCount = postingBalances.Sum(b => b.TransactionCount),
            TotalChargebackAmount = postingBalances.Sum(b => b.TotalChargebackAmount),
            TotalChargebackCommissionAmount = postingBalances.Sum(b => b.TotalChargebackCommissionAmount),
            TotalChargebackTransferAmount = postingBalances.Sum(b => b.TotalChargebackTransferAmount),
            TotalSuspiciousAmount = postingBalances.Sum(b => b.TotalSuspiciousAmount),
            TotalSuspiciousCommissionAmount = postingBalances.Sum(b => b.TotalSuspiciousCommissionAmount),
            TotalSuspiciousTransferAmount = postingBalances.Sum(b => b.TotalSuspiciousTransferAmount),
            TotalPayingAmount = postingBalances.Sum(b => b.TotalPayingAmount),
            TotalPfCommissionAmount = postingBalances.Sum(b => b.TotalPfCommissionAmount),
            TotalBankCommissionAmount = postingBalances.Sum(b => b.TotalBankCommissionAmount),
            TotalDueAmount = postingBalances.Sum(b => b.TotalDueAmount),
            TotalDueTransferAmount = postingBalances.Sum(b => b.TotalDueTransferAmount),
            TotalExcessReturnAmount = postingBalances.Sum(b => b.TotalExcessReturnAmount),
            TotalExcessReturnTransferAmount = postingBalances.Sum(b => b.TotalExcessReturnTransferAmount),
            TotalExcessReturnOnCommissionAmount = postingBalances.Sum(b => b.TotalExcessReturnOnCommissionAmount),
            TotalSubmerchantDeductionAmount = postingBalances.Sum(b => b.TotalSubmerchantDeductionAmount),
            TotalParentMerchantCommissionAmount = postingBalances.Sum(b => b.TotalParentMerchantCommissionAmount),
            
            MerchantStatistics = 
                (await postingBalances
                    .GroupBy(s => s.MerchantId)
                    .Select(s => 
                        new
                        {
                            Id = s.Key,
                            MerchantName = s.FirstOrDefault().Merchant.Name,
                            TotalAmount = s.Sum(b => b.TotalAmount),
                            TotalTransactionCount = s.Sum(b => b.TransactionCount),
                            TotalChargebackAmount = s.Sum(b => b.TotalChargebackAmount),
                            TotalChargebackCommissionAmount = s.Sum(b => b.TotalChargebackCommissionAmount),
                            TotalChargebackTransferAmount = s.Sum(b => b.TotalChargebackTransferAmount),
                            TotalSuspiciousAmount = s.Sum(b => b.TotalSuspiciousAmount),
                            TotalSuspiciousCommissionAmount = s.Sum(b => b.TotalSuspiciousCommissionAmount),
                            TotalSuspiciousTransferAmount = s.Sum(b => b.TotalSuspiciousTransferAmount),
                            TotalPayingAmount = s.Sum(b => b.TotalPayingAmount),
                            TotalPfCommissionAmount = s.Sum(b => b.TotalPfCommissionAmount),
                            TotalBankCommissionAmount = s.Sum(b => b.TotalBankCommissionAmount),
                            TotalDueAmount = s.Sum(b => b.TotalDueAmount),
                            TotalDueTransferAmount = s.Sum(b => b.TotalDueTransferAmount),
                            TotalExcessReturnAmount = s.Sum(b => b.TotalExcessReturnAmount),
                            TotalExcessReturnTransferAmount = s.Sum(b => b.TotalExcessReturnTransferAmount),
                            TotalExcessReturnOnCommissionAmount = s.Sum(b => b.TotalExcessReturnOnCommissionAmount),
                            TotalSubmerchantDeductionAmount = s.Sum(b => b.TotalSubmerchantDeductionAmount)
                        })
                    .ToListAsync())
                    .ToDictionary(
                        x => x.Id,
                        x => new MerchantPostingBalanceStatistic
                        {
                            MerchantName = x.MerchantName,
                            TotalAmount = x.TotalAmount,
                            TotalTransactionCount = x.TotalTransactionCount,
                            TotalChargebackAmount = x.TotalChargebackAmount,
                            TotalChargebackCommissionAmount = x.TotalChargebackCommissionAmount,
                            TotalChargebackTransferAmount = x.TotalChargebackTransferAmount,
                            TotalSuspiciousAmount = x.TotalSuspiciousAmount,
                            TotalSuspiciousCommissionAmount = x.TotalSuspiciousCommissionAmount,
                            TotalSuspiciousTransferAmount = x.TotalSuspiciousTransferAmount,
                            TotalPayingAmount = x.TotalPayingAmount,
                            TotalPfCommissionAmount = x.TotalPfCommissionAmount,
                            TotalBankCommissionAmount = x.TotalBankCommissionAmount,
                            TotalDueAmount = x.TotalDueAmount,
                            TotalDueTransferAmount = x.TotalDueTransferAmount,
                            TotalExcessReturnAmount = x.TotalExcessReturnAmount,
                            TotalExcessReturnTransferAmount = x.TotalExcessReturnTransferAmount,
                            TotalExcessReturnOnCommissionAmount = x.TotalExcessReturnOnCommissionAmount,
                            TotalSubmerchantDeductionAmount = x.TotalSubmerchantDeductionAmount
                        })
        };
    }

    private IQueryable<PostingBalanceDto> GetPostingBalanceQuery()
    {
        return from p in _dbContext.PostingBalance
               join m in _dbContext.Merchant on p.MerchantId equals m.Id
               join parent in _dbContext.Merchant
                   on m.ParentMerchantId equals parent.Id into parentGroup
               from parentMerchant in parentGroup.DefaultIfEmpty() 
               select new PostingBalanceDto
               {
                   Id = p.Id,
                   BlockageStatus = p.BlockageStatus,
                   PostingDate = p.PostingDate,
                   PaymentDate = p.PaymentDate,
                   ParentMerchantName = parentMerchant != null ? parentMerchant.Name : null,
                   ParentMerchantNumber = parentMerchant != null ? parentMerchant.Number : null,
                   ParentMerchantId = p.Merchant.ParentMerchantId,
                   CreateDate = p.CreateDate,
                   OldPaymentDate = p.OldPaymentDate,
                   TotalAmount = p.TotalAmount,
                   Currency = p.Currency,
                   TransactionDate = p.TransactionDate,
                   TotalAmountWithoutBankCommission = p.TotalAmountWithoutBankCommission,
                   TotalBankCommissionAmount = p.TotalBankCommissionAmount,
                   TotalAmountWithoutCommissions = p.TotalAmountWithoutCommissions,
                   TotalPfCommissionAmount = p.TotalPfCommissionAmount,
                   TotalPfNetCommissionAmount = p.TotalPfNetCommissionAmount,
                   TotalDueAmount = p.TotalDueAmount,
                   TotalDueTransferAmount = p.TotalDueTransferAmount,
                   TotalPointAmount = p.TotalPointAmount,
                   TotalChargebackAmount = p.TotalChargebackAmount,
                   TotalChargebackCommissionAmount = p.TotalChargebackCommissionAmount,
                   TotalChargebackTransferAmount = p.TotalChargebackTransferAmount,
                   TotalSuspiciousAmount = p.TotalSuspiciousAmount,
                   TotalSuspiciousCommissionAmount = p.TotalSuspiciousCommissionAmount,
                   TotalSuspiciousTransferAmount = p.TotalSuspiciousTransferAmount,
                   TotalExcessReturnAmount = p.TotalExcessReturnAmount,
                   TotalExcessReturnTransferAmount = p.TotalExcessReturnTransferAmount,
                   TotalExcessReturnOnCommissionAmount = p.TotalExcessReturnOnCommissionAmount,
                   TotalSubmerchantDeductionAmount = 
                       p.TotalChargebackCommissionAmount +
                       p.TotalSuspiciousCommissionAmount +
                       p.TotalChargebackTransferAmount +
                       p.TotalSuspiciousTransferAmount +
                       p.TotalDueTransferAmount +
                       p.TotalExcessReturnTransferAmount +
                       p.TotalExcessReturnOnCommissionAmount,
                   TotalNegativeBalanceAmount = p.TotalNegativeBalanceAmount,
                   TotalPayingAmount = p.TotalPayingAmount,
                   TransactionCount = p.TransactionCount,
                   MerchantId = p.MerchantId,
                   Merchant = _mapper.Map<TransactionMerchantResponse>(p.Merchant),
                   MoneyTransferBankName = p.MoneyTransferBankName,
                   MoneyTransferBankCode = p.MoneyTransferBankCode,
                   MoneyTransferStatus = p.MoneyTransferStatus,
                   PostingPaymentChannel = p.PostingPaymentChannel,
                   TotalParentMerchantCommissionAmount = p.TotalParentMerchantCommissionAmount,
                   PostingBalanceType = p.PostingBalanceType
               };
    }
}
