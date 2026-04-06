namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Requests
{
    public class UpdateSecurityQuestionRequest
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string LanguageCode { get; set; }
    }
}
