using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class ParentMerchantResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public MerchantType MerchantType { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public string ParentMerchantName { get; set; }
    public string ParentMerchantNumber { get; set; }
    public IntegrationMode ParentMerchantIntegrationMode { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public bool FinancialTransactionAllowed { get; set; }
    public bool PaymentAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsPostAuthAmountHigherAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool IsCvvPaymentAllowed { get; set; }
    public bool IsReturnApproved { get; set; }
    public bool ParentMerchantIs3dRequired { get; set; }
    public bool ParentMerchantFinancialTransactionAllowed { get; set; }
    public bool ParentMerchantPaymentAllowed { get; set; }
    public bool ParentMerchantInstallmentAllowed { get; set; }
    public bool ParentMerchantPreAuthorizationAllowed { get; set; }
    public bool ParentMerchantIsPostAuthAmountHigherAllowed { get; set; }
    public bool ParentMerchantInternationalCardAllowed { get; set; }
    public bool ParentMerchantPaymentReturnAllowed { get; set; }
    public bool ParentMerchantIsReturnApproved { get; set; }
    public bool ParentMerchantIsExcessReturnAllowed { get; set; }
    public bool ParentMerchantPaymentReverseAllowed { get; set; }
    public bool ParentMerchantIsCvvPaymentAllowed { get; set; }
    public string ParentMerchantPricingProfileNumber { get; set; }
    public string PricingProfileNumber { get; set; }
}
