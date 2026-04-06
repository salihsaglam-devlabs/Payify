namespace LinkPara.PF.Application.Commons.Models.VposModels.Response;

public class PosResponseBase
{
    public bool IsSuccess { get; set; }
    public string TransId { get; set; }
    public string AuthCode { get; set; }
    public string OrderNumber { get; set; }
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public DateTime TrxDate { get; set; }
    public string RrnNumber { get; set; }
    public string Stan { get; set; }
}
