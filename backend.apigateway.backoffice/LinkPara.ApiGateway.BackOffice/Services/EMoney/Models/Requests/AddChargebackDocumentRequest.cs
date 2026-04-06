namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests
{
    public class AddChargebackDocumentRequest
    {
        public Guid ChargebackId { get; set; }
        public string DocumentDescription { get; set; }
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string OriginalFileName { get; set; }
        public Guid DocumentTypeId { get; set; }
    }
}
