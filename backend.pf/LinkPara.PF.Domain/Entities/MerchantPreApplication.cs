using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantPreApplication : AuditEntity
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string Website { get; set; }
    public bool ConsentConfirmation { get; set; }
    public bool KvkkConfirmation { get; set; }
    public string ResponsiblePerson { get; set; }
    public List<MerchantPreApplicationHistory> ApplicationHistories { get; set; }
}