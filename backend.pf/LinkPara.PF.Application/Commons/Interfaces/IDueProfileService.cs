using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Application.Features.DueProfiles.Queries.GetFilterDueProfile;
using LinkPara.PF.Application.Features.DueProfiles.Command.UpdateDueProfile;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using LinkPara.PF.Application.Features.DueProfiles.Command.DeleteDueProfile;

namespace LinkPara.PF.Application.Commons.Interfaces
{
    public interface IDueProfileService
    {
        Task<PaginatedList<DueProfileDto>> GetFilterListAsync(GetFilterDueProfileQuery request);
        Task<DueProfileDto> GetByIdAsync(Guid id);
        Task UpdateAsync(UpdateDueProfileCommand command);
        Task DeleteAsync(DeleteDueProfileCommand command);
    }
}
