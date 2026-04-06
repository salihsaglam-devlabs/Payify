namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests.AgreementDocuments
{
    public class AgreementDocumentVersionRequest
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public string LanguageCode { get; set; }
        public bool IsOptional { get; set; }
    }
}
