using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.BulkOperations.Merchants.Command.BulkImportMerchant;

public class BulkImportMerchantCommand : IRequest<BulkImportMerchantResponse>
{
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string FileName { get; set; }
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
    public IntegrationMode IntegrationMode { get; set; }
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

public class BulkImportMerchantCommandHandler : IRequestHandler<BulkImportMerchantCommand, BulkImportMerchantResponse>
{
    private readonly IBulkOperationsService _bulkOperationsService;
    
    public BulkImportMerchantCommandHandler(IBulkOperationsService bulkOperationsService)
    {
        _bulkOperationsService = bulkOperationsService;
    }

    public async Task<BulkImportMerchantResponse> Handle(BulkImportMerchantCommand command, CancellationToken cancellationToken)
    {
        return await _bulkOperationsService.BulkImportMerchantAsync(command);
    }
}