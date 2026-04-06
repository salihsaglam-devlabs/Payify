using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using MediatR;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetBinInformation;

public class GetBinInformationCommand : IRequest<GetBinInformationResponse>
{
    public string CardToken { get; set; }
    public string BinNumber { get; set; }
    public string ConversationId { get; set; }
    public string LanguageCode { get; set; }
    public Guid MerchantId { get; set; }
}

public class GetBinInformationCommandHandler : IRequestHandler<GetBinInformationCommand, GetBinInformationResponse>
{
    private readonly ICardBinService _binService;

    public GetBinInformationCommandHandler(ICardBinService binService)
    {
        _binService = binService;
    }

    public async Task<GetBinInformationResponse> Handle(GetBinInformationCommand request,
        CancellationToken cancellationToken)
    {
        return await _binService.GetBinInformationAsync(request);
    }
}