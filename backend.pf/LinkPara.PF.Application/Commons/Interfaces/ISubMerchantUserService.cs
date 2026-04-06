using LinkPara.PF.Application.Features.SubMerchants.Queries.GetAllSubMerchant;
using LinkPara.PF.Application.Features.SubMerchants;
using LinkPara.SharedModels.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LinkPara.PF.Application.Features.SubMerchantUsers.Queries.GetAllSubMerchantUsers;
using LinkPara.PF.Application.Features.SubMerchantUsers;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ISubMerchantUserService
{
    Task<PaginatedList<SubMerchantUserDto>> GetListAsync(GetAllSubMerchantUserQuery query);
    Task<SubMerchantUserDto> GetByIdAsync(Guid id);
}
