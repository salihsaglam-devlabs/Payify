using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Infrastructure.Services;

public class AccountActivityService : IAccountActivityService
{
    private readonly IGenericRepository<AccountActivity> _accountActivityRepository;

    public AccountActivityService(
        IGenericRepository<AccountActivity> accountActivityRepository)
    {
        _accountActivityRepository = accountActivityRepository;
    }

    public async Task<bool> IsWithdrawIbanDistinctAsync(Guid accountId, TimeInterval timeInterval, string iban)
    {
        var withdrawDistinctIbanList =
            await _accountActivityRepository
                .GetAll()
                .Where(a => 
                    a.AccountId == accountId && 
                    a.TransferType == PricingCommercialType.Iban.ToString() &&
                    a.TransactionDirection == TransactionDirection.MoneyOut &&
                    (timeInterval == TimeInterval.Daily ? 
                        a.CreateDate.Day == DateTime.Today.Day : a.Month == DateTime.Today.Month ))
                .Select(a => a.Receiver)
                .Distinct()
                .ToListAsync();

        return withdrawDistinctIbanList.Contains(iban);
    }
}