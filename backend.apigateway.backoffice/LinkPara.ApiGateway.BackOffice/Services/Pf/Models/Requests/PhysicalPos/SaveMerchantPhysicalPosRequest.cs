namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class SaveMerchantPhysicalPosRequest
{
    public Guid Id { get; set; }
    public List<Guid> PhysicalPosIdList { get; set; }
}
