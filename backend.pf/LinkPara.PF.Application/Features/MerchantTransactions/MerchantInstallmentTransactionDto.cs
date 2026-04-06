using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Application.Features.TimeoutTransactions;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Features.MerchantTransactions;

public class MerchantInstallmentTransactionDto : IMapFrom<MerchantInstallmentTransaction>
{
    public Guid Id { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantNumber { get; set; }
    public string ParentMerchantName { get; set; }
    public string ParentMerchantNumber { get; set; }
    public TransactionMerchantResponse Merchant { get; set; }
    public string ConversationId { get; set; }
    public string IpAddress { get; set; }
    public TransactionType TransactionType { get; set; }
    public TransactionStatus TransactionStatus { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public decimal PointCommissionAmount { get; set; }
    public decimal PointCommissionRate { get; set; }
    public decimal ServiceCommissionAmount { get; set; }
    public decimal ServiceCommissionRate { get; set; }
    public int Currency { get; set; }
    public int InstallmentCount { get; set; }
    public string BinNumber { get; set; }
    public string CardNumber { get; set; }
    public bool HasCvv { get; set; }
    public bool HasExpiryDate { get; set; }
    public bool IsInternational { get; set; }
    public bool IsAmex { get; set; }
    public bool IsReverse { get; set; }
    public bool IsManualReturn { get; set; }
    public bool IsOnUsPayment { get; set; }
    public bool IsInsurancePayment { get; set; }
    public bool? IsTopUpPayment { get; set; }
    public DateTime ReverseDate { get; set; }
    public bool IsReturn { get; set; }
    public DateTime ReturnDate { get; set; }
    public decimal ReturnAmount { get; set; }
    public string ReturnedTransactionId { get; set; }
    public bool IsPreClose { get; set; }
    public DateTime PreCloseDate { get; set; }
    public string PreCloseTransactionId { get; set; }
    public decimal? PreCloseAmount { get; set; }
    public bool Is3ds { get; set; }
    public string ThreeDSessionId { get; set; }
    public decimal BankCommissionRate { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public int IssuerBankCode { get; set; }
    public Bank IssuerBank { get; set; }
    public int AcquireBankCode { get; set; }
    public Bank AcquireBank { get; set; }
    public CardTransactionType? CardTransactionType { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseDescription { get; set; }
    public DateTime TransactionStartDate { get; set; }
    public DateTime TransactionEndDate { get; set; }
    public Guid VposId { get; set; }
    public string LanguageCode { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public CardType CardType { get; set; }
    public DateTime TransactionDate { get; set; }
    public BankTransactionDto BankTransaction { get; set; }
    public VposDto Vpos { get; set; }
    public bool IsChargeBack { get; set; }
    public bool IsSuspecious { get; set; }
    public string SuspeciousDescription { get; set; }
    public DateTime LastChargebackActivityDate { get; set; }
    public DateTime CreateDate { get; set; }
    public string MerchantCustomerName { get; set; }
    public string MerchantCustomerPhoneCode { get; set; }
    public string MerchantCustomerPhoneNumber { get; set; }
    public string Description { get; set; }
    public string CardHolderName { get; set; }
    public string CardHolderIdentityNumber { get; set; }
    public ReturnStatus ReturnStatus { get; set; }
    public string CreatedNameBy { get; set; }
    public decimal PfCommissionAmount { get; set; }
    public decimal PfNetCommissionAmount { get; set; }
    public decimal PfCommissionRate { get; set; }
    public decimal PfPerTransactionFee { get; set; }
    public decimal ParentMerchantCommissionAmount { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public decimal AmountWithoutCommissions { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal AmountWithoutParentMerchantCommission { get; set; }
    public Guid PricingProfileItemId { get; set; }
    public decimal BsmvAmount { get; set; }
    public string ProvisionNumber { get; set; }
    public string VposName { get; set; }
    public DateTime PfPaymentDate { get; set; }
    public DateTime BankPaymentDate { get; set; }
    public BlockageStatus BlockageStatus { get; set; }
    public PfTransactionSource PfTransactionSource { get; set; }
    public Guid MerchantPhysicalPosId { get; set; }
    public Guid PhysicalPosEodId { get; set; }
    public Guid PhysicalPosOldEodId { get; set; }
    public EndOfDayStatus EndOfDayStatus { get; set; }
}
