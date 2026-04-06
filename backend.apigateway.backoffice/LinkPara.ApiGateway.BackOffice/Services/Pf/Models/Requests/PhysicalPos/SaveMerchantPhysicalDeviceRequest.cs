namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests.PhysicalPos;

public class SaveMerchantPhysicalDeviceRequest
{
    public Guid MerchantId { get; set; }
    public List<SaveMerchantPhysicalRequest> SaveMerchantPhysicalDeviceList { get; set; }
    public List<Guid> PhysicalPosIdList { get; set; }
}
