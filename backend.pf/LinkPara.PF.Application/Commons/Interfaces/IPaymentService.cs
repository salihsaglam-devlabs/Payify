using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPaymentService
{
    Task<ProvisionResponse> ProvisionAsync(ProvisionCommand request);
}
