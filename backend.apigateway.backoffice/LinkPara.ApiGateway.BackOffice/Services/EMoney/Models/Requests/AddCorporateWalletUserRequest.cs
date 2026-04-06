namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;

public class AddCorporateWalletUserRequest
{
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime BirthDate { get; set; }
    public List<Guid> Roles { get; set; }
    public Guid AccountId { get; set; }
}