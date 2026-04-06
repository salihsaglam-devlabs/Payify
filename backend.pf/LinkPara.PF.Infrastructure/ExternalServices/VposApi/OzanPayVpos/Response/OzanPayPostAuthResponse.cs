using System.Text.Json;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Response;

public class OzanPayPostAuthResponse : OzanPayResponseBase
{
    public bool IsLive { get; set; }
}
