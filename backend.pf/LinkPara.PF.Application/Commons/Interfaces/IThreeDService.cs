using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSession;
using LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSessionResult;
using LinkPara.PF.Application.Features.Payments.Commands.Init3ds;
using LinkPara.PF.Application.Features.Payments.Commands.Verify3ds;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IThreeDService
{
    Task<GetThreeDSessionResponse> GetThreeDSessionAsync(GetThreeDSessionCommand request);
    Task<GetThreeDSessionResultResponse> GetThreeDSessionResultAsync(GetThreeDSessionResultCommand request);
    Task<Init3dsResponse> Init3ds(Init3dsCommand request);
    Task<Verify3dsResponse> Verify3ds(Verify3dsCommand request);
}
