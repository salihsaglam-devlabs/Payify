using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class RouterMerchantVposDto : IMapFrom<MerchantVpos>
{
    public Guid MerchantId { get; set; }
    public Merchant Merchant { get; set; }
    public Guid VposId { get; set; }
    public Vpos Vpos { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public string TerminalNo { get; set; }
    public string Password { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool IsOptionalPos { get; set; }
    public bool IsHealthCheckOptionalPos { get; set; }
}
