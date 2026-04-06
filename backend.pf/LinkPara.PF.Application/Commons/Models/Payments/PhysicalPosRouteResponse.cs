using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.Payments;

public class PhysicalPosRouteResponse : ResponseBase
{
    public CardType CardType { get; set; }
    public CardTransactionType CardTransactionType { get; set; }
    public AcquireBank AcquireBank { get; set; }
    public PricingProfileItem PricingProfileItem { get; set; }
    public bool IsAmex { get; set; }
    public bool IsInternational { get; set; }
    public int InstallmentCount { get; set; }
    public string BinNumber { get; set; }
    public int IssuerBankCode { get; set; }
    public int CurrencyNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public decimal PointCommissionRate { get; set; }
    public decimal PointCommissionAmount { get; set; }
    public decimal BankCommissionRate { get; set; }
    public decimal BankCommissionAmount { get; set; }
    public decimal PfCommissionRate { get; set; }
    public decimal PfCommissionAmount { get; set; }
    public decimal PfNetCommissionAmount { get; set; }
    public decimal PfPerTransactionFee { get; set; }
    public decimal ParentMerchantCommissionRate { get; set; }
    public decimal ParentMerchantCommissionAmount { get; set; }
    public decimal ServiceCommissionRate { get; set; }
    public decimal ServiceCommissionAmount { get; set; }
    public decimal AmountWithoutCommissions { get; set; }
    public decimal AmountWithoutBankCommission { get; set; }
    public decimal AmountWithoutParentMerchantCommission { get; set; }
    public decimal BsmvAmount { get; set; }
    public DateTime BankPaymentDate { get; set; }
    public DateTime PfPaymentDate { get; set; }
    public Guid CostProfileItemId { get; set; }
    public ProfileSettlementMode ProfileSettlementMode { get; set; }
    public List<InstallmentItem> Installments { get; set; }
}