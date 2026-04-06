using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.VerifyLogin;

public class VerifyLoginCommand: BaseRequest, IRequest<VerifyLoginOtpResponse>
{
    public string LoginOtp { get; set; }
    public string PhoneNumber { get; set; }
}

public class VerifyLoginCommandHandler : IRequestHandler<VerifyLoginCommand, VerifyLoginOtpResponse>
{
    private IMultifactorService _multifactorService;

    public VerifyLoginCommandHandler(IMultifactorService multifactorService)
    {
        _multifactorService = multifactorService;
    }

    public async Task<VerifyLoginOtpResponse> Handle(VerifyLoginCommand command, CancellationToken cancellationToken)
    {
        var request = new VerifyLoginOtpRequest
        {
            LoginOtp = command.LoginOtp,
            CustomerId = long.Parse(command.PhoneNumber.Replace("+", string.Empty))
        };

        var response = await _multifactorService.VerifyLoginOtpAsync(request);
        
        return response;
    }
}