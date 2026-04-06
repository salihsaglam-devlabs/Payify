namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses.AgreementDocuments
{
    public class DocumentVersionDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string LanguageCode { get; set; }
        public bool IsOptional { get; set; }
    }
}
