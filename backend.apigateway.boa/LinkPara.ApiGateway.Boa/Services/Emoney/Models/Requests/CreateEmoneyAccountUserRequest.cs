namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class CreateEmoneyAccountUserRequest
{
    public Guid AccountId { get; set; }
    public Guid UserId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
}
