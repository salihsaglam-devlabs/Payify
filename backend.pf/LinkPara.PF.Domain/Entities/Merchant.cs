using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.DomainEvents;
using LinkPara.SharedModels.Persistence;
using ApplicationChannel = LinkPara.PF.Domain.Enums.ApplicationChannel;
using IntegrationMode = LinkPara.PF.Domain.Enums.IntegrationMode;
using MerchantStatus = LinkPara.PF.Domain.Enums.MerchantStatus;
using MerchantType = LinkPara.PF.Domain.Enums.MerchantType;
using PostingPaymentChannel = LinkPara.PF.Domain.Enums.PostingPaymentChannel;

namespace LinkPara.PF.Domain.Entities;

public class Merchant : AuditEntity, IHasDomainEvent,ITrackChange
{
    public string Name { get; set; }
    public string Number { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public string ParentMerchantName { get; set; }
    public string ParentMerchantNumber { get; set; }
    public bool IsInvoiceCommissionReflected { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string MccCode { get; set; }
    public Mcc Mcc { get; set; }
    public string NaceCode { get; set; }
    public Nace Nace { get; set; }
    public Guid CustomerId  { get; set; }
    public Customer Customer  { get; set; }
    public string Language { get; set; }
    public string WebSiteUrl { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public string PhoneCode { get; set; }
    public DateTime AgreementDate { get; set; }
    public Guid? SalesPersonId { get; set; }
    public int PaymentDueDay { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsDocumentRequired { get; set; }
    public bool IsManuelPayment3dRequired { get; set; }
    public bool IsLinkPayment3dRequired { get; set; }
    public bool IsHostedPayment3dRequired { get; set; }
    public bool IsCvvPaymentAllowed { get; set; }
    public bool IsPostAuthAmountHigherAllowed { get; set; }
    public bool HalfSecureAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool FinancialTransactionAllowed { get; set; }
    public bool PaymentAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public bool InsurancePaymentAllowed { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
    public string PricingProfileNumber { get; set; }
    public string AnnulmentId { get; set; }
    public Guid MerchantPoolId { get; set; }
    public Guid? MerchantIntegratorId { get; set; }
    public MerchantIntegrator MerchantIntegrator { get; set; }
    public Guid? ContactPersonId { get; set; }
    public ContactPerson TechnicalContact { get; set; }
    public string GlobalMerchantId { get; set; }
    public string AnnulmentCode { get; set; }
    public string AnnulmentDescription { get; set; }
    public DateTime AnnulmentDate { get; set; }
    public string AnnulmentAdditionalInfo { get; set; }
    public bool? IsAnnulment { get; set; }
    public List<MerchantUser> MerchantUsers { get; set; }
    public List<MerchantDocument> MerchantDocuments { get; set; }
    public List<MerchantVpos> MerchantVposList { get; set; }
    public List<MerchantEmail> MerchantEmails { get; set; }
    public List<MerchantScore> MerchantScores { get; set; }
    public List<MerchantBankAccount> MerchantBankAccounts { get; set; }
    public List<MerchantLimit> MerchantLimits { get; set; }
    public List<MerchantHistory> MerchantHistoryList { get; set; }
    public List<MerchantBlockage> MerchantBlockageList { get; set; }
    public List<MerchantApiKey> MerchantApiKeyList { get; set; }
    public List<DomainEvent> DomainEvents { get; set; } = new List<DomainEvent>();
    public List<MerchantBusinessPartner> MerchantBusinessPartner { get; set; }
    public bool IsReturnApproved { get; set; }
    public string HostingTaxNo { get; set; }
    public string HostingTradeName { get; set; }
    public string HostingUrl { get; set; }
    public List<MerchantWallet> MerchantWallets { get; set; }
    public List<SubMerchant> SubMerchants { get; set; }
    public List<MerchantPhysicalDevice> MerchantPhysicalDevices { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public string Information { get; set; }
    public DateTime EstablishmentDate { get; set; }
    public BusinessModel BusinessModel { get; set; }
    public string BusinessActivity { get; set; }
    public int BranchCount { get; set; }
    public int EmployeeCount { get; set; }
    public PosType PosType { get; set; }
    public int? MoneyTransferStartHour { get; set; }
    public int? MoneyTransferStartMinute { get; set; }
}