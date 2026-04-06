using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class VposDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public SecurityType SecurityType { get; set; }
    public VposType VposType { get; set; }
    public string MainMerchantId { get; set; }
    public int? BlockageCode { get; set; }
    public bool IsOnUsVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public virtual List<VposBankApiInfoDto> VposBankApiInfos { get; set; }
    public bool? HasActiveCostProfile { get; set; }
}

public class VposBankApiInfoDto
{
    public BankApiKeyDto Key { get; set; }
    public string Value { get; set; }
}
