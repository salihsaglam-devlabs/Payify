using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class WalletBlockageDocumentDto
{
    public Guid Id { get; set; }
    public Guid WalletBlockageId { get; set; }
    public Guid WalletId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}
