namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SaveSubMerchantUserRequest
{
    public string Name { get; set; }
    public string Surname { get; set; }
    public DateTime BirthDate { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public string IdentityNumber { get; set; }
    public Guid SubMerchantId { get; set; }
}