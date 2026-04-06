using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Domain.Entities;

public class UserSecurityAnswer: AuditEntity, ITrackChange
{
    public Guid UserId { get; set; }
    public User User { get; set; }
    public Guid SecurityQuestionId { get; set; }
    public SecurityQuestion SecurityQuestion { get; set; }
    public string AnswerHash { get; set; }
}