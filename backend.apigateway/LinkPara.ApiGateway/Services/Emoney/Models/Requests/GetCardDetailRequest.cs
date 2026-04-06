namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;
public class GetCardDetailRequest 
{
    public string HhsCode { get; set; }
    public string AppUserId { get; set; }
    public string ConsentId { get; set; }
    public string CardRefNo { get; set; }
    public string StatementType { get; set; }
}
