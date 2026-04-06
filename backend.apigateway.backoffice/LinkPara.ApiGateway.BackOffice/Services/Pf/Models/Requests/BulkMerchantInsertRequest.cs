using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class BulkMerchantInsertRequest
{
    public string Key { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public string Language { get; set; }
    public Guid? SalesPersonId { get; set; }
    public string PricingProfileNumber { get; set; }
    public string HostingTaxNo { get; set; }
    public string HostingUrl { get; set; }
    public string HostingTradeName { get; set; }
    public Guid? MerchantIntegratorId { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public DateTime AgreementDate { get; set; }
    public IntegrationMode[] IntegrationModes { get; set; }
    public Guid AdminUserRoleId { get; set; }
    public bool IsInvoiceCommissionReflected { get; set; }
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
    public bool IsPaymentToMainMerchant { get; set; }
}

public class BulkMerchantInsertServiceRequest : BulkMerchantInsertRequest
{
    public BulkMerchantInsertServiceRequest(BulkMerchantInsertRequest request)
    {
        MerchantType = request.MerchantType;
        ParentMerchantId = request.ParentMerchantId;
        Language = request.Language;
        SalesPersonId = request.SalesPersonId;
        PricingProfileNumber = request.PricingProfileNumber;
        HostingTaxNo = request.HostingTaxNo;
        HostingUrl = request.HostingUrl;
        HostingTradeName = request.HostingTradeName;
        MerchantIntegratorId = request.MerchantIntegratorId;
        ApplicationChannel = request.ApplicationChannel;
        AgreementDate = request.AgreementDate;
        IntegrationMode = request.IntegrationModes.Aggregate(
            IntegrationMode.Unknown, (current, next) => current | next);
        AdminUserRoleId = request.AdminUserRoleId;
        IsInvoiceCommissionReflected = request.IsInvoiceCommissionReflected;
        IsDocumentRequired = request.IsDocumentRequired;
        Is3dRequired = request.Is3dRequired;
        IsManuelPayment3dRequired = request.IsManuelPayment3dRequired;
        IsLinkPayment3dRequired = request.IsLinkPayment3dRequired;
        IsHostedPayment3dRequired = request.IsHostedPayment3dRequired;
        IsCvvPaymentAllowed = request.IsCvvPaymentAllowed;
        IsPostAuthAmountHigherAllowed = request.IsPostAuthAmountHigherAllowed;
        IsReturnApproved = request.IsReturnApproved;
        HalfSecureAllowed = request.HalfSecureAllowed;
        InstallmentAllowed = request.InstallmentAllowed;
        IsExcessReturnAllowed = request.IsExcessReturnAllowed;
        InternationalCardAllowed = request.InternationalCardAllowed;
        PreAuthorizationAllowed = request.PreAuthorizationAllowed;
        FinancialTransactionAllowed = request.FinancialTransactionAllowed;
        PaymentAllowed = request.PaymentAllowed;
        PaymentReverseAllowed = request.PaymentReverseAllowed;
        PaymentReturnAllowed = request.PaymentReturnAllowed;
        IsPaymentToMainMerchant = request.IsPaymentToMainMerchant;
    }
    
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
}