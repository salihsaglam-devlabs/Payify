namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;
public class TriggerFuturePaymentOrderRequest 
{
    public string ConsentId { get; set; }
    public string AppUserId { get; set; }
    public string HhsCode { get; set; }
    public string RizaNo { get; set; }
    public string RizaTip { get; set; }
    public string RizaDrm { get; set; }
    public string YetTip { get; set; }
    public string YetKod { get; set; }
    public string DrmKod { get; set; }    
}

