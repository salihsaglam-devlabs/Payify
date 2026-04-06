using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetCostProfileById;
using LinkPara.PF.Application.Features.CostProfiles.Queries.GetFilterCostProfile;
using LinkPara.PF.Application.Features.CostProfiles;
using LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ICostProfileService
{
    Task<CostProfilesDto> GetByIdAsync(GetCostProfileByIdQuery request);
    Task<PaginatedList<CostProfilesDto>> GetFilterListAsync(GetFilterCostProfileQuery request);
    Task SaveAsync(SaveCostProfileCommand command);
    Task PatchCostProfile(CostProfile command);
    void ValidateInstallment(List<CostProfileItemDto> costProfileItems, PosType posType, ProfileSettlementMode settlementMode);
}
