using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.WithdrawRequests;
using LinkPara.Emoney.Application.Features.WithdrawRequests.Queries.GetWithdrawRequestList;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services;

public class WithdrawRequestService : IWithdrawRequestService
{
    private readonly EmoneyDbContext _dbContext;

    public WithdrawRequestService(EmoneyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaginatedList<WithdrawRequestAdminDto>> GetWithdrawRequestListAsync(GetWithdrawRequestListQuery request)
    {
        var query = _dbContext.WithdrawRequest.Join(
            _dbContext.Wallet,
            w => w.WalletNumber,
            wa => wa.WalletNumber,
            (wr, wa) => new WithdrawRequestAdminDto
            {
                Id = wr.Id,
                Amount = wr.Amount,
                CreateDate = wr.CreateDate,
                CurrencyCode = wr.CurrencyCode,
                Description = wr.Description,
                IsReceiverIbanOwned = wr.IsReceiverIbanOwned,
                ReceiverIbanNumber = wr.ReceiverIbanNumber,
                ReceiverName = wr.ReceiverName,
                RecordStatus = wr.RecordStatus,
                TransferType = wr.TransferType,
                WalletNumber = wr.WalletNumber,
                WithdrawStatus = wr.WithdrawStatus,
                AccountId = wa.AccountId
            }).AsQueryable();

        if (request.UserId is not null)
        {
            var accountUser = await _dbContext.AccountUser
                .FirstOrDefaultAsync(s => s.UserId == request.UserId);

            if(accountUser != null)
            {
                query = query.Where(s => s.AccountId == accountUser.AccountId);
            }            
        }

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            query = query.Where(x => x.WalletNumber == request.WalletNumber);
        }

        if (request.WithdrawStatus.HasValue)
        {
            query = query.Where(x => x.WithdrawStatus == request.WithdrawStatus);
        }

        if (request.RecordStatus.HasValue)
        {
            query = query.Where(x => x.RecordStatus == request.RecordStatus);
        }

        if (!string.IsNullOrEmpty(request.CurrencyCode))
        {
            query = query.Where(x => x.CurrencyCode == request.CurrencyCode.ToUpper());
        }

        if (!string.IsNullOrEmpty(request.Description))
        {
            query = query.Where(x => x.Description.ToLower().Contains(request.Description.ToLower()));
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }
        if (request.TransferType.HasValue)
        {
            query = query.Where(x => x.TransferType == request.TransferType);
        }

        return await query.PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
