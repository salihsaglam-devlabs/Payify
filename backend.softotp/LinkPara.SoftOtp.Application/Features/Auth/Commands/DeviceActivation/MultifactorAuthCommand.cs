using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using MediatR;
using Microsoft.Extensions.Localization;

namespace LinkPara.SoftOtp.Application.Features.Auth.Commands.DeviceActivation
{
    public class MultifactorAuthCommand : IRequest<GenerateActivationOtpResponse>
    {
        public string PhoneNumber { get; set; }
    }
    public class MultifactorAuthCommandHandler: IRequestHandler<MultifactorAuthCommand, GenerateActivationOtpResponse>
    {
        private readonly IMultifactorService _multifactorService;
        private readonly IStringLocalizer _localizer;

        public MultifactorAuthCommandHandler(
            IMultifactorService multifactorService, 
            IStringLocalizerFactory localizerFactory)
        {
            _multifactorService = multifactorService;
            _localizer = localizerFactory.Create("Exceptions", "LinkPara.SoftOtp.API");

        }
        public async Task<GenerateActivationOtpResponse> Handle(MultifactorAuthCommand request, CancellationToken cancellationToken)
        {
            request.PhoneNumber = string.Concat(request.PhoneNumber).Replace("+", string.Empty);
                
          return await _multifactorService.SendActivationOtpAsync(request);
        }
    }
}