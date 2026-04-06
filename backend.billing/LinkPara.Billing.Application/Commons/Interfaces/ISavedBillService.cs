using LinkPara.Billing.Application.Features.SavedBills;
using LinkPara.Billing.Application.Features.SavedBills.Commands.CreateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.DeleteSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.UpdateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Queries.GetAllSavedBill;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface ISavedBillService
{
    Task<PaginatedList<SavedBillDto>> GetAllAsync(GetAllSavedBillQuery request);
    Task SaveAsync(CreateSavedBillCommand request);
    Task UpdateAsync(UpdateSavedBillCommand request);
    Task DeleteAsync(DeleteSavedBillCommand request);
}