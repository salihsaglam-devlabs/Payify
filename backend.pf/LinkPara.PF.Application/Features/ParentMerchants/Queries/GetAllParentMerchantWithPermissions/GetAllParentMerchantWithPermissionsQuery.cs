using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.ParentMerchants.Queries.GetAllParentMerchantWithPermissions;

public class GetAllParentMerchantWithPermissionsQuery : SearchQueryParams, IRequest<PaginatedList<ParentMerchantDto>>
{
    public Guid[] MainSubMerchantIdList { get; set; }
}
public class GetAllParentMerchantWithPermissionsQueryHandler : IRequestHandler<GetAllParentMerchantWithPermissionsQuery, PaginatedList<ParentMerchantDto>>
{
    private readonly IGenericRepository<Merchant> _repository;

    public GetAllParentMerchantWithPermissionsQueryHandler(IGenericRepository<Merchant> repository)
    {
        _repository = repository;
    }
    public async Task<PaginatedList<ParentMerchantDto>> Handle(GetAllParentMerchantWithPermissionsQuery request, CancellationToken cancellationToken)
    {
        var merchantList = from merchant in _repository.GetAll()
                           join parent in _repository.GetAll()
                               on merchant.ParentMerchantId equals parent.Id into parentJoin
                           from parent in parentJoin.DefaultIfEmpty()
                           where
                           (request.MainSubMerchantIdList == null || !request.MainSubMerchantIdList.Any() ||
                           (request.MainSubMerchantIdList.Contains(merchant.Id)))
                           select new ParentMerchantDto
                           {
                               Id = merchant.Id,
                               Name = merchant.Name,
                               Number = merchant.Number,
                               MerchantType = merchant.MerchantType,
                               MerchantStatus = merchant.MerchantStatus,
                               PricingProfileNumber = merchant.PricingProfileNumber,
                               FinancialTransactionAllowed = merchant.FinancialTransactionAllowed,
                               PaymentAllowed = merchant.PaymentAllowed,
                               InstallmentAllowed = merchant.InstallmentAllowed,
                               Is3dRequired = merchant.Is3dRequired, 
                               IsPostAuthAmountHigherAllowed = merchant.IsPostAuthAmountHigherAllowed,
                               PreAuthorizationAllowed = merchant.PreAuthorizationAllowed,
                               InternationalCardAllowed = merchant.InternationalCardAllowed,
                               IntegrationMode = merchant.IntegrationMode,
                               PaymentReturnAllowed = merchant.PaymentReturnAllowed,
                               IsExcessReturnAllowed = merchant.IsExcessReturnAllowed,
                               PaymentReverseAllowed = merchant.PaymentReverseAllowed,
                               IsCvvPaymentAllowed = merchant.IsCvvPaymentAllowed,
                               IsReturnApproved = merchant.IsReturnApproved,
                               // 
                               ParentMerchantId = merchant.ParentMerchantId,
                               ParentMerchantName = merchant.ParentMerchantName,
                               ParentMerchantNumber = merchant.ParentMerchantNumber,
                               ParentMerchantIntegrationMode = parent.IntegrationMode,
                               ParentMerchantFinancialTransactionAllowed = parent != null && parent.FinancialTransactionAllowed,
                               ParentMerchantPaymentAllowed = parent != null && parent.PaymentAllowed,
                               ParentMerchantInstallmentAllowed = parent != null && parent.InstallmentAllowed,
                               ParentMerchantPreAuthorizationAllowed = parent != null && parent.PreAuthorizationAllowed,
                               ParentMerchantIsPostAuthAmountHigherAllowed = parent != null && parent.IsPostAuthAmountHigherAllowed,
                               ParentMerchantInternationalCardAllowed = parent != null && parent.InternationalCardAllowed,
                               ParentMerchantPaymentReturnAllowed = parent != null && parent.PaymentReturnAllowed,
                               ParentMerchantIsReturnApproved = parent != null && parent.IsReturnApproved, //
                               ParentMerchantIsExcessReturnAllowed = parent != null && parent.IsExcessReturnAllowed,
                               ParentMerchantPaymentReverseAllowed = parent != null && parent.PaymentReverseAllowed,
                               ParentMerchantIsCvvPaymentAllowed = parent != null && parent.IsCvvPaymentAllowed,
                               ParentMerchantIs3dRequired = parent != null && parent.Is3dRequired,
                               ParentMerchantPricingProfileNumber = parent.PricingProfileNumber,
                           };


        return await merchantList.PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}