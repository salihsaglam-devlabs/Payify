using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Queries;

public class GetWalletBlockageDocumentQuery : IRequest<List<WalletBlockageDocumentDto>>
{
    public string WalletNumber { get; set; }
}

public class GetWalletBlockageDocumentCommandHandler : IRequestHandler<GetWalletBlockageDocumentQuery, List<WalletBlockageDocumentDto>>
{

    private readonly IWalletBlockageService _WalletBlockageService;

    public GetWalletBlockageDocumentCommandHandler(IWalletBlockageService WalletBlockageService)
    {
        _WalletBlockageService = WalletBlockageService;
    }

    public async Task<List<WalletBlockageDocumentDto>> Handle(GetWalletBlockageDocumentQuery request, CancellationToken cancellationToken)
    {
        return await _WalletBlockageService.GetWalletBlockageDocumentsAsync(request);
    }
}