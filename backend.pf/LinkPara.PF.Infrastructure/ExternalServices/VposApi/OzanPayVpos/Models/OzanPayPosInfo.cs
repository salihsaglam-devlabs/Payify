namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Models;

public class OzanPayPosInfo : IPosInfo
{
    public string BaseUrl { get; set; }
    public Guid VposId { get; set; }
}
