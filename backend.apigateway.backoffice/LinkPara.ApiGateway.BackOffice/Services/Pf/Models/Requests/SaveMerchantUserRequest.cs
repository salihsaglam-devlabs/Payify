namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveMerchantUserRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid MerchantId { get; set; }
}
