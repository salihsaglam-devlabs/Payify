using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Application.Features.MerchantIntegrators;

public class MerchantIntegratorDto : IMapFrom<MerchantIntegrator>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal CommissionRate { get; set; }
    public DateTime CreateDate { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
