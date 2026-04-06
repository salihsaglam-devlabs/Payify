using LinkPara.HttpProviders.CustomerManagement.Models;
using LinkPara.HttpProviders.PF.Models.Enums;

namespace LinkPara.HttpProviders.PF.Models.Response;

public class MerchantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Number { get; set; }
    public MerchantType MerchantType { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public string ParentMerchantName { get; set; }
    public string ParentMerchantNumber { get; set; }
    public bool IsInvoiceCommissionReflected { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string MccCode { get; set; }
    public Guid CustomerId { get; set; }
    public CustomerDto Customer { get; set; }
    public string Language { get; set; }
    public string WebSiteUrl { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public string PhoneCode { get; set; }
    public DateTime AgreementDate { get; set; }
    public Guid? SalesPersonId { get; set; }
    public int PaymentDueDay { get; set; }
    public bool IsDocumentRequired { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsManuelPayment3dRequired { get; set; }
    public bool IsLinkPayment3dRequired { get; set; }
    public bool IsHostedPayment3dRequired { get; set; }
    public bool IsCvvPaymentAllowed { get; set; }
    public bool IsPostAuthAmountHigherAllowed { get; set; }
    public bool IsReturnApproved { get; set; }
    public bool HalfSecureAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool FinancialTransactionAllowed { get; set; }
    public bool PaymentAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public string PricingProfileNumber { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
    public string AnnulmentDescription { get; set; }
    public string AnnulmentId { get; set; }
    public string GlobalMerchantId { get; set; }
    public string AnnulmentAdditionalInfo { get; set; }
    public string HostingTaxNo { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public MerchantContactPersonDto TechnicalContact { get; set; }
    public List<MerchantUserDto> MerchantUsers { get; set; }
}