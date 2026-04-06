namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPureSqlStore
{
    Task<Guid?> ReservePostingBalancesForMoneyTransferAsync(TimeSpan defaultTransferHour);
    Task<Guid?> ReserveMerchantDeductionsAsync(List<Guid> merchantIds);
}