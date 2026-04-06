using LinkPara.PF.Application.Features.MerchantIntegrators;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.DeleteMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.UpdateMerchantIntegrator;
using LinkPara.SharedModels.Pagination;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantIntegratorService
{
    Task<PaginatedList<MerchantIntegratorDto>> GetListAsync(SearchQueryParams request);
    Task<MerchantIntegratorDto> GetByIdAsync(Guid id);
    Task SaveAsync(SaveMerchantIntegratorCommand request);
    Task DeleteAsync(DeleteMerchantIntegratorCommand request);
    Task UpdateAsync(UpdateMerchantIntegratorCommand request);
}
