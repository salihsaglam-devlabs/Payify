namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Request;

public class SecurityContextCollection
{
    public string OrganizationBranchCode { get; set; }
    public string OrganizationBranchUserCode { get; set; }
    public int OrganizationId { get; set; }
    public string OrganizationPassword { get; set; }
    public string ServiceUserName { get; set; }
    public string ServicePassword { get; set; }
}