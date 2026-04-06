using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class VposDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public VposStatus VposStatus { get; set; }
    public Guid AcquireBankId { get; set; }
    public AcquireBankDto AcquireBank { get; set; }
    public SecurityType SecurityType { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public bool IsInsuranceVpos { get; set; }
    public virtual List<VposBankApiInfoDto> VposBankApiInfos { get; set; }
}

public class VposBankApiInfoDto
{
    public BankApiKeyDto Key { get; set; }
    public string Value { get; set; }
}
