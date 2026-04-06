using LinkPara.PF.Application.Features.BankHealthChecks;
using LinkPara.PF.Application.Features.BankHealthChecks.Command.UpdateBankHealthCheck;
using LinkPara.PF.Application.Features.BankHealthChecks.Queries.GetAllBankHealthCheck;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IBankHealthCheckService
{
    Task UpdateAsync(UpdateBankHealthCheckCommand request);
    Task UpdateHealthCheckAsync(Guid acquireBankId);
    Task<PaginatedList<BankHealthCheckDto>> GetListAsync(GetAllBankHealthCheckQuery request);
}
