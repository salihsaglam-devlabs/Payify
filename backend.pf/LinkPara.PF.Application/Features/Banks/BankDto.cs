using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.Banks;

public class BankDto : IMapFrom<Bank>
{
    public Guid Id { get; set; }
    public int Code { get; set; }
    public string Name { get; set; }
}
