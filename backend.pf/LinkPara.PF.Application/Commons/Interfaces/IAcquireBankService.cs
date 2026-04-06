using LinkPara.PF.Application.Features.AcquireBanks;
using LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAllAcquireBank;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IAcquireBankService
{
    Task<PaginatedList<AcquireBankDto>> GetListAsync(GetAllAcquireBankQuery request);
    Task<AcquireBankDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveAcquireBankCommand request);
    Task DeleteAsync(DeleteAcquireBankCommand request);
    Task UpdateAsync(UpdateAcquireBankCommand request);
}
