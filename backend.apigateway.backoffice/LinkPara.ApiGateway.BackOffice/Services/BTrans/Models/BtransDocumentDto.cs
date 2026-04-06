using LinkPara.ApiGateway.BackOffice.Services.BTrans.Models.Enum;

namespace LinkPara.ApiGateway.BackOffice.Services.BTrans.Models;

public class BtransDocumentDto
{
    public Guid Id { get; set; }
    public string DocumentCode { get; set; }
    public string FileName { get; set; }
    public int Counter { get; set; }
    public DocumentStatus Status { get; set; }
    public string Description { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
}