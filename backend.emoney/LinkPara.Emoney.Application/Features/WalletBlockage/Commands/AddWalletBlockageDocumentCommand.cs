using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.Emoney.Application.Features.WalletBlockages.Commands;

public class AddWalletBlockageDocumentCommand : IRequest<WalletBlockageDocumentDto>
{
    public Guid WalletId { get; set; }
    public string DocumentDescription { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public string OriginalFileName { get; set; }
    public Guid DocumentTypeId { get; set; }
}

public class AddWalletBlockageDocumentCommandHandler : IRequestHandler<AddWalletBlockageDocumentCommand, WalletBlockageDocumentDto>
{
    
    private readonly IWalletBlockageService _WalletBlockageService;

    public AddWalletBlockageDocumentCommandHandler(IWalletBlockageService WalletBlockageService)
    {
        _WalletBlockageService = WalletBlockageService;
    }

    public async Task<WalletBlockageDocumentDto> Handle(AddWalletBlockageDocumentCommand request, CancellationToken cancellationToken)
    {
        return await _WalletBlockageService.AddWalletBlockageDocumentAsync(request);        
    }
}