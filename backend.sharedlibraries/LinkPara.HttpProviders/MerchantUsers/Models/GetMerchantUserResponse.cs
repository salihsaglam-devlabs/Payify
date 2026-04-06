namespace LinkPara.HttpProviders.MerchantUsers.Models;

public class GetMerchantUserResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Email { get; set; }
    public string MobilePhoneNumber { get; set; }
    public string RoleId { get; set; }
    public string RoleName { get; set; }
    public Guid UserId { get; set; }
    public Guid MerchantId { get; set; }
}
