using LinkPara.PF.Application.Features.Links;
using LinkPara.PF.Application.Features.Links.Command.SaveLink;
using LinkPara.PF.Application.Features.Links.Queries.GetCreateLinkRequirement;
using LinkPara.SharedModels.Pagination;
using LinkPara.PF.Application.Features.Links.Queries.GetAllLink;
using LinkPara.PF.Application.Features.Links.Command.DeleteLink;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ILinkService
{
    Task<LinkResponse> SaveAsync(SaveLinkCommand request);
    Task<LinkRequirementResponse> GetCreateLinkRequirements (GetCreateLinkRequirementQuery request);
    Task<PaginatedList<LinkDto>> GetListAsync(GetAllLinkQuery request);
    Task DeleteAsync(DeleteLinkCommand command); 
}
