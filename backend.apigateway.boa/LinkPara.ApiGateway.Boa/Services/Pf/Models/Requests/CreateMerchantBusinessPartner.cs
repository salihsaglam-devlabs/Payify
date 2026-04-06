namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;

public class CreateMerchantBusinessPartner
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string IdentityNumber { get; set; }
    public DateTime BirthDate { get; set; }
}
