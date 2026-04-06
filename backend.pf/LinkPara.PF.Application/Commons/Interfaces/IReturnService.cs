using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;
using LinkPara.PF.Application.Features.Payments.Commands.Return;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IReturnService
{
    Task<ReturnResponse> ReturnAsync(ReturnCommand request);
    Task ManualReturnAsync(ManualReturnCommand request);
}
