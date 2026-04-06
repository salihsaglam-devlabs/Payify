using LinkPara.Billing.Application.Commons.Mappings;
using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Features.SavedBills;

public class SavedBillDto : IMapFrom<SavedBill>
{
    public Guid Id { get; set; }
    public Guid InstitutionId { get; set; }
    public string SectorName { get; set; }
    public string InstitutionName { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
    public List<Field> Fields { get; set; }
}