namespace LinkPara.HttpProviders.Documents.Models
{
    public class CreateDocumentRequest
    {
        public byte[] Bytes { get; set; }
        public string ContentType { get; set; }
        public string OriginalFileName { get; set; }

        public Guid? UserId { get; set; }
        public Guid? AccountId { get; set; }
        public Guid? MerchantId { get; set; }
        public Guid DocumentTypeId { get; set; }
    }
}
