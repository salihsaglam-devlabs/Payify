using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.UserQuestions.Queries;

public class SecurityQuestionDto : IMapFrom<SecurityQuestion>
{
    public Guid Id { get; set; }
    public string Question { get; set; }
    public string LanguageCode { get; set; }
    public RecordStatus RecordStatus { get; set; }

}