using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class UpdateMerchantPreApplicationRequest
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
    public List<MerchantPreApplicationHistory> ApplicationHistories { get; set; }
}