using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes;

public class MccDto : IMapFrom<Mcc>
{
    public Guid Id { get; set; }
    public string Code { get; set; }
    public string Name { get; set; }
    public int MaxIndividualInstallmentCount { get; set; }
    public int MaxCorporateInstallmentCount { get; set; }
    public string Description { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
