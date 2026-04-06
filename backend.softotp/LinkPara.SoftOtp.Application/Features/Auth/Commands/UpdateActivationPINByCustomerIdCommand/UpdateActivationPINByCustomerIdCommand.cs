using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.UpdateActivationPINByCustomerIdTransaction;

public class UpdateActivationPINByCustomerIdCommand : BaseRequest, IRequest<UpdateActivationPINByCustomerIdResponse>
{
    public long CustomerId { get; set; }
    public string PIN { get; set; }
}

public class UpdateActivationPINByCustomerIdCommandHandler : IRequestHandler<UpdateActivationPINByCustomerIdCommand, UpdateActivationPINByCustomerIdResponse>
{
    private IMultifactorService _multifactorService;

    public UpdateActivationPINByCustomerIdCommandHandler(IMultifactorService multifactorService)
    {
        _multifactorService = multifactorService;
    }

    public async Task<UpdateActivationPINByCustomerIdResponse> Handle(UpdateActivationPINByCustomerIdCommand request, CancellationToken cancellationToken)
    {
        var response = await _multifactorService.UpdateActivationPINByCustomerIdAsync(request);

        return new UpdateActivationPINByCustomerIdResponse
        {
            Success = response.Success,
            Results = response.Success ? response.Results : new List<Result>()
        };
    }
}