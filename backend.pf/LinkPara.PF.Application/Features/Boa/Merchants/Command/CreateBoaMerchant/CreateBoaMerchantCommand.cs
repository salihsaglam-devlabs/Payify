using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.PF.Application.Commons.Interfaces.Boa;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Boa.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.Boa.Merchants.Command.CreateBoaMerchant;

public class CreateBoaMerchantCommand : IRequest<CreateBoaMerchantResponse>
{
    public string MerchantName { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public CreateMerchantCustomer Customer { get; set; }
    public CreateMerchantContactPerson TechnicalContact { get; set; }
    public CreateMerchantUser AdminUser { get; set; }
    public List<CreateMerchantBusinessPartner> MerchantBusinessPartner { get; set; }
    public string WebSiteUrl { get; set; }
    public string Language { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public Guid? SalesPersonId { get; set; }
    public string MccCode { get; set; }
    public string HostingTaxNo { get; set; }
    public string HostingUrl { get; set; }
    public string HostingTradeName { get; set; }
    public Guid? MerchantIntegratorId { get; set; }
    public ApplicationChannel ApplicationChannel { get; set; }
    public FraudChannelType FraudChannelType { get; set; }
    public DateTime AgreementDate { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string PhoneCode { get; set; }
    public string PricingProfileNumber { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public string MerchantWalletNumber { get; set; }
    public int MerchantIbanBankCode { get; set; }
    public string MerchantIban { get; set; }
    public bool? IsAllBanksEnabled { get; set; }
    public List<Guid> VposList { get; set; }
    public List<CreateMerchantLimit> MerchantLimits { get; set; }
    public bool? IsInvoiceCommissionReflected { get; set; }
    public bool? IsDocumentRequired { get; set; }
    public bool? Is3dRequired { get; set; }
    public bool? IsManuelPayment3dRequired { get; set; }
    public bool? IsLinkPayment3dRequired { get; set; }
    public bool? IsHostedPayment3dRequired { get; set; }
    public bool? IsCvvPaymentAllowed { get; set; }
    public bool? IsPostAuthAmountHigherAllowed { get; set; }
    public bool? IsReturnApproved { get; set; }
    public bool? HalfSecureAllowed { get; set; }
    public bool? InstallmentAllowed { get; set; }
    public bool? IsExcessReturnAllowed { get; set; }
    public bool? InternationalCardAllowed { get; set; }
    public bool? PreAuthorizationAllowed { get; set; }
    public bool? FinancialTransactionAllowed { get; set; }
    public bool? PaymentAllowed { get; set; }
    public bool? PaymentReverseAllowed { get; set; }
    public bool? PaymentReturnAllowed { get; set; }
    public DateTime EstablishmentDate { get; set; }
    public BusinessModel BusinessModel { get; set; }
    public string BusinessActivity { get; set; }
    public int BranchCount { get; set; }
    public int EmployeeCount { get; set; }
    public int? MoneyTransferStartHour { get; set; }
    public int? MoneyTransferStartMinute { get; set; }
}

public class CreateBoaMerchantCommandHandler : IRequestHandler<CreateBoaMerchantCommand, CreateBoaMerchantResponse>
{
    private readonly IBoaMerchantService _boaMerchantService;
    
    public CreateBoaMerchantCommandHandler(IBoaMerchantService boaMerchantService)
    {
        _boaMerchantService = boaMerchantService;
    }

    public async Task<CreateBoaMerchantResponse> Handle(CreateBoaMerchantCommand command, CancellationToken cancellationToken)
    {
        return await _boaMerchantService.CreateBoaMerchantAsync(command);
    }
}