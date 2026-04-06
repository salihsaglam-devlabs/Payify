using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;

namespace LinkPara.Emoney.Application.Features.BulkTransfers;

public class BulkTransferDto : IMapFrom<BulkTransfer>
{
    public Guid Id { get; set; }
    public BulkTransferStatus BulkTransferStatus { get; set; }
    public string FileName { get; set; }
    public int ReferenceNumber { get; set; }
    public Guid? ActionUser { get; set; }
    public string ActionUserName { get; set; }
    public DateTime ActionDate { get; set; }
    public string SenderWalletNumber { get; set; }
    public BulkTransferType BulkTransferType { get; set; }
    public List<BulkTransferDetailDto> BulkTransferDetails { get; set; }
    public DateTime CreateDate { get; set; }
}
