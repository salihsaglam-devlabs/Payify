using LinkPara.PF.Application.Commons.Models.Customers;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.Merchants;
using LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.DeleteMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.UpdateMerchant;
using LinkPara.PF.Application.Features.Merchants.Queries.GetFilterMerchant;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantInstallmentTransactions;
using LinkPara.PF.Application.Features.MerchantTransactions.Queries.GetAllMerchantTransactions;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkIntegrationModeUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPermissionUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.BulkPricingProfileUpdate;
using LinkPara.PF.Application.Features.ParentMerchants.Command.UpdateMultiplePermission;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IMerchantService 
{
    Task<PaginatedList<MerchantDto>> GetFilterListAsync(GetFilterMerchantQuery request);
    Task<MerchantDto> GetByIdAsync(Guid id);
    Task<MerchantTransactionDto> GetMerchantTransactionByIdAsync(Guid id);
    Task<MerchantDto> GetByIdWithOptionsAsync(Guid id, SubQueryOptions options);
    Task DeleteAsync(DeleteMerchantCommand command);
    Task<MerchantResponse> UpdateAsync(UpdateMerchantCommand command);
    Task ApproveMerchant(ApproveMerchantCommand command);
    Task PatchMerchant(Merchant command);
    Task UpdateMerchantHistory(List<MerchantHistoryDto> command);
    Task<PaginatedList<MerchantTransactionDto>> GetMerchantTransactionList(GetAllMerchantTransactionQuery request);
    Task<PaginatedList<MerchantInstallmentTransactionDto>> GetMerchantInstallmentTransactionList(GetAllMerchantInstallmentTransactionQuery request);
    Task MerchantPermissionBatchUpdateAsync(BulkPermissionUpdateCommand request);
    Task MerchantIntegrationModeBatchUpdateAsync(BulkIntegrationModeUpdateCommand request);
    Task PricingProfileBatchUpdateAsync(BulkPricingProfileUpdateCommand request);
}
