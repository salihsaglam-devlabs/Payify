using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Posting;

public class PostingTransferErrorDto : IMapFrom<PostingTransferError>
{
    public DateTime PostingDate { get; set; }
    public Guid? MerchantId { get; set; }
    public string MerchantName { get; set; }
    public Guid MerchantTransactionId { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
    public string ErrorMessage { get; set; }
    public TransferErrorCategory TransferErrorCategory { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantOrderId { get; set; }
}