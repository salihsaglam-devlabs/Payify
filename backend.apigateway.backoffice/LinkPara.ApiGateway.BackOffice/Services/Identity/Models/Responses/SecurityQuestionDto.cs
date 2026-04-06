using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses
{
    public class SecurityQuestionDto
    {
        public Guid Id { get; set; }
        public string Question { get; set; }
        public string LanguageCode { get; set; }
        public RecordStatus RecordStatus { get; set; }
    }
}
