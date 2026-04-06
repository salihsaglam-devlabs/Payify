using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class CreateMerchantPreApplicationRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public string Website { get; set; }
    public bool ConsentConfirmation { get; set; }
    public bool KvkkConfirmation { get; set; }
}