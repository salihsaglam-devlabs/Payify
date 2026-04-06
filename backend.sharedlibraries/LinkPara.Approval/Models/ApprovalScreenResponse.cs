namespace LinkPara.Approval.Models;

public class ApprovalScreenResponse
{
    public string Resource { get; set; }
    public Dictionary<string, object> DisplayScreenFields { get; set; }
    public Dictionary<string, object> UpdatedFields { get; set; }
}