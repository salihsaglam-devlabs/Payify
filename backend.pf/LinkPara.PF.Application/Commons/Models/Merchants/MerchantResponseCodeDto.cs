namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantResponseCodeDto
{
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public string DisplayMessage { get; set; }
    
    public bool ProcessTimeoutManagement { get; set; }
}