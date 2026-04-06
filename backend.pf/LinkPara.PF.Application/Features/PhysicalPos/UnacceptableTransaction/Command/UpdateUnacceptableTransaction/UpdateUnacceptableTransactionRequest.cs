using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums.PhysicalPos;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.UpdateUnacceptableTransaction;

public class UpdateUnacceptableTransactionRequest : IMapFrom<PhysicalPosUnacceptableTransaction>
{
    public UnacceptableTransactionStatus CurrentStatus {get; set;}
}