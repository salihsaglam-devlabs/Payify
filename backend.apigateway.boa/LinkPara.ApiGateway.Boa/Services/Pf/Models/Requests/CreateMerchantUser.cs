namespace LinkPara.ApiGateway.Boa.Services.Pf.Models.Requests;

public class CreateMerchantUser
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public DateTime BirthDate { get; set; }
    public Guid RoleId { get; set; }
    public Guid ExternalPersonId { get; set; }
}