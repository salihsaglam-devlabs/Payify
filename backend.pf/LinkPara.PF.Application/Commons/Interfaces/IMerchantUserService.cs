using LinkPara.PF.Application.Features.MerchantUsers;
using LinkPara.PF.Application.Features.MerchantUsers.Command.SaveMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Command.UpdateMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Queries.GetAllMerchantUser;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantUserService
{
    Task<PaginatedList<MerchantUserDto>> GetAllAsync(GetAllMerchantUserQuery query);
    Task<MerchantUserDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveMerchantUserCommand request);
    Task UpdateAsync(UpdateMerchantUserCommand request);
}
