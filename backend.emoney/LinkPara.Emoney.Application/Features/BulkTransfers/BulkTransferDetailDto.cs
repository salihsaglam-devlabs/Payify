using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.BulkTransfers;

public class BulkTransferDetailDto : IMapFrom<BulkTransferDetail>
{
    public Guid Id { get; set; }
    public Guid BulkTransferId { get; set; }
    public string FullName { get; set; }
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public string Description { get; set; }
    public string Receiver { get; set; }
    public BulkTransferDetailStatus BulkTransferDetailStatus { get; set; }
    public Guid? TransactionId { get; set; }
    public DateTime CreateDate { get; set; }
    public decimal BsmvAmount { get; set; }
    public decimal CommissionAmount { get; set; }
    public decimal PricingAmount => BsmvAmount + CommissionAmount;
    public decimal TotalAmount => PricingAmount + Amount;
    public string ExceptionMessage { get; set; }
}
