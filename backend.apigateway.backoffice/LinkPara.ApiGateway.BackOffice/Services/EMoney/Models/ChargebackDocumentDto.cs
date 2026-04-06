namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models;

public class ChargebackDocumentDto
{
    public Guid Id { get; set; }
    public Guid ChargebackId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string Description { get; set; }
    public string FileName { get; set; }
}
