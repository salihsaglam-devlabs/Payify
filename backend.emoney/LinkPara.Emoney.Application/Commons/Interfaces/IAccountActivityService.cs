using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IAccountActivityService
{
    Task<bool> IsWithdrawIbanDistinctAsync(Guid accountId, TimeInterval timeInterval, string iban);
}