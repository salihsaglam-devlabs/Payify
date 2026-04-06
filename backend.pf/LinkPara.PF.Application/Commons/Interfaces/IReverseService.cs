using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Reverse;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IReverseService
{
    Task<ReverseResponse> ReverseAsync(ReverseCommand request);
}
