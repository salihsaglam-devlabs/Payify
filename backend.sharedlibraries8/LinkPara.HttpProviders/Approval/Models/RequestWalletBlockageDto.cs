namespace LinkPara.HttpProviders.Approval.Models;

public class RequestWalletBlockageDto
{
    public long? ReferenceId { get; set; }
    public string DisplayName { get; set; }
    public string Body { get; set; }
    public int Status { get; set; }
    public Guid MakerUserId { get; set; }
    public Guid CheckerUserId { get; set; }
    public Guid SecondCheckerUserId { get; set; }
}
