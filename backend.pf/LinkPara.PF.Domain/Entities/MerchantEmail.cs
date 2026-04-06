using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantEmail : AuditEntity, ITrackChange
{
    public string Email { get; set; }
    public EmailType EmailType { get; set; }
    public bool ReportAllowed { get; set; }

    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
}

