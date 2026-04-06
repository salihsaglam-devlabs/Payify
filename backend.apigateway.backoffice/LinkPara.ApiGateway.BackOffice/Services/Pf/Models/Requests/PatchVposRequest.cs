using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class PatchVposRequest
{
    public string Name { get; set; }
    public Guid AcquireBankId { get; set; }
    public SecurityType SecurityType { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public VposStatus VposStatus { get; set; }
    public VposType VposType { get; set; }
    public string MainMerchantId { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public List<SaveBankApiInfoDto> VposBankApiInfos { get; set; }
}
