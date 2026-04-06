using LinkPara.PF.Application.Commons.Models.BankLimits;
using LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;
using LinkPara.PF.Application.Features.BankLimits;
using LinkPara.PF.Application.Features.BankLimits.Command.DeleteBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.UpdateBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Queries.GetAllBankLimit;
using LinkPara.SharedModels.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IBankLimitService
{
    Task<PaginatedList<BankLimitDto>> GetListAsync(GetAllBankLimitQuery request);
    Task<BankLimitDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveBankLimitCommand request);
    Task UpdateAsync(UpdateBankLimitCommand request);
    Task IncrementLimitAsync(UpdateBankLimitRequest request);
    Task DecrementLimitAsync(UpdateBankLimitRequest request);
    Task DeleteAsync(DeleteBankLimitCommand request);
}
