using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IBankHealthCheckTransactionService
{
    Task SaveAsync(MerchantTransaction request);
}
