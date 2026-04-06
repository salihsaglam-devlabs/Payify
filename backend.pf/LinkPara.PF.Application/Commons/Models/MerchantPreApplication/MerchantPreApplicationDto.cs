using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.MerchantPreApplication;

public class MerchantPreApplicationDto : IMapFrom<Domain.Entities.MerchantPreApplication>
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ResponsiblePerson { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string Website { get; set; }
    public bool ConsentConfirmation { get; set; }
    public bool KvkkConfirmation { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public List<MerchantPreApplicationHistoryDto> ApplicationHistories { get; set; }
}