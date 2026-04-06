namespace LinkPara.Emoney.Application.Commons.Models.AccountModels;

public class CustodyAccountResponse
{
    public Guid AccountId { get; set; }
    public Guid ParentAccountId { get; set; }
    public string ParentIdentityNumber { get; set; }
    public string ParentNameSurname { get; set; }
    public string ParentPhoneNumber { get; set; }
    public string ParentEmail { get; set; }
    public string ChildIdentityNumber { get; set; }
    public string ChildNameSurname { get; set; }
    public string ChildPhoneNumber { get; set; }
    public string ChildEmail { get; set; }
}
