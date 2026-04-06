namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class SaveMerchantDueRequest
{
    public Guid MerchantId { get; set; }
    public Guid DueProfileId { get; set; }
}