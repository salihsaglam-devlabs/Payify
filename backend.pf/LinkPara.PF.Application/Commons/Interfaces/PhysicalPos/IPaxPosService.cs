using LinkPara.PF.Application.Commons.Models.PhysicalPos;
using LinkPara.PF.Application.Commons.Models.PhysicalPos.Response;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxEndOfDayCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxParameterCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxReconciliationCommand;
using LinkPara.PF.Application.Features.PhysicalPos.Pax.Command.PaxTransactionCommand;
using LinkPara.PF.Domain.Entities.PhysicalPos;

namespace LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;

public interface IPaxPosService
{
    Task<PaxTransactionResponse> TransactionAsync(PaxTransactionCommand request);
    Task<PaxParameterResponse> ParametersAsync(PaxParameterCommand request);
    Task<PaxEndOfDayResponse> EndOfDayAsync(PaxEndOfDayCommand request);
    Task<PaxReconciliationResponse> ReconciliationAsync(PaxReconciliationCommand request);
    Task RetryTransactionAsync(PhysicalPosUnacceptableTransaction unacceptableTransaction);
}