using LinkPara.Billing.Application.Features.Commissions;
using LinkPara.Billing.Application.Features.Commissions.Commands.CreateCommission;
using LinkPara.Billing.Application.Features.Commissions.Commands.SaveCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetAllCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetByDetail;
using LinkPara.Billing.Domain.Enums;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface ICommissionService
{
    Task<PaginatedList<CommissionDto>> GetAllAsync(GetAllCommissionQuery request);
    Task AddAsync(CreateCommissionQuery request);
    Task UpdateAsync(SaveCommissionCommand request);
    Task<CommissionDto> GetByInstitutionAsync(GetByDetailQuery request);
    Task<CommissionWithAmountDetailDto> CalculateCommissionWithAmountDetailAsync(Guid institutionId, decimal amount, PaymentSource paymentSource);
    Task DeleteAsync(Guid commissionId);
    Task<CommissionDto> GetByIdAsync(Guid commissionId);
}