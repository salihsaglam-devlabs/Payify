using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Commands;

public class RemoveWalletBlockageDocumentCommand : IRequest<bool>
{
    public Guid WalletBlockageDocumentId { get; set; }   
}

public class RemoveWalletBlockageDocumentCommandHandler : IRequestHandler<RemoveWalletBlockageDocumentCommand, bool>
{
    
    private readonly IWalletBlockageService _WalletBlockageService;

    public RemoveWalletBlockageDocumentCommandHandler(IWalletBlockageService WalletBlockageService)
    {
        _WalletBlockageService = WalletBlockageService;
    }

    public async Task<bool> Handle(RemoveWalletBlockageDocumentCommand request, CancellationToken cancellationToken)
    {
        return await _WalletBlockageService.RemoveWalletBlockageDocumentAsync(request);        
    }
}