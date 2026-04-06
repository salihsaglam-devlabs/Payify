using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class UpdateMerchantRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string WebSiteUrl { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public string MccCode { get; set; }
    public string Language { get; set; }
    public string PhoneCode { get; set; }
    public DateTime AgreementDate { get; set; }
    public Guid SalesPersonId { get; set; }
    public Guid CustomerId { get; set; }
    public int PaymentDueDay { get; set; }
    public bool IsDocumentRequired { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsManuelPayment3dRequired { get; set; }
    public bool IsLinkPayment3dRequired { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool FinancialTransactionAllowed { get; set; }
    public bool PaymentAllowed { get; set; }
    public bool InsurancePaymentAllowed { get; set; }
    public string PricingProfileNumber { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
    public Guid? MerchantIntegratorId { get; set; }
    public CustomerDto Customer { get; set; }
    public List<SaveMerchantVposRequest> MerchantVposList { get; set; }
    public ContactPersonDto TechnicalContact { get; set; }
    public List<MerchantBankAccountDto> MerchantBankAccounts { get; set; }
    public List<MerchantScoreDto> MerchantScores { get; set; }
    public List<MerchantDocumentDto> MerchantDocuments { get; set; }
    public List<MerchantLimitDto> MerchantLimits { get; set; }
}
