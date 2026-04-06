namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class ValidateIdentityRequest
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime DateOfBirth { get; set; }
    public long IdentityNo { get; set; }
    public string Profession { get; set; }
}