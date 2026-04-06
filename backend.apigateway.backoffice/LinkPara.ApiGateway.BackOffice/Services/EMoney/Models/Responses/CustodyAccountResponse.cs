namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;

public class CustodyAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ParentAccountId { get; set; }
    public string ParentIdentityNumber { get; set; }
    public string ParentNameSurname { get; set; }
    public string ParentPhoneNumber { get; set; }
    public string ChildIdentityNumber { get; set; }
    public string ChildNameSurname { get; set; }
    public string ChildPhoneNumber { get; set; }
}
