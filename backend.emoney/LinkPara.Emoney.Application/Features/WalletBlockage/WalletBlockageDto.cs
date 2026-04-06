using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.WalletBlockages;

public class WalletBlockageDto : IMapFrom<WalletBlockage>
{
    public Guid Id { get; set; }
    public Guid WalletId { get; set; }
    public string WalletNumber { get; set; }
    public string AccountName { get; set; }
    public string WalletCurrencyCode { get; set; }
    public decimal CashBlockageAmount { get; set; }
    public decimal CreditBlockageAmount { get; set; }
    public WalletBlockageOperationType OperationType { get; set; }
    public WalletBlockageStatus BlockageStatus { get; set; }
    public string BlockageType { get; set; }
    public string BlockageDescription { get; set; }
    public DateTime BlockageStartDate { get; set; }
    public DateTime? BlockageEndDate { get; set; }

}
