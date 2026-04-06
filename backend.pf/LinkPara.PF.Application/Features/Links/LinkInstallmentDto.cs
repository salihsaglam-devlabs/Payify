using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;


namespace LinkPara.PF.Application.Features.Links;

public class LinkInstallmentDto : IMapFrom<LinkInstallment>
{
    public int Installment { get; set; }
}
