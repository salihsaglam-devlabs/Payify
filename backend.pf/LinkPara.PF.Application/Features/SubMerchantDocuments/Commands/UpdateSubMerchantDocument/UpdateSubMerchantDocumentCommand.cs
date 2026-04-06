using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.UpdateSubMerchantDocument;

public class UpdateSubMerchantDocumentCommand : IRequest
{
    public SubMerchantDocumentDto SubMerchantDocument { get; set; }
}

public class UpdateSubMerchantDocumentCommandHandler : IRequestHandler<UpdateSubMerchantDocumentCommand>
{
    private readonly ISubMerchantDocumentService _service;

    public UpdateSubMerchantDocumentCommandHandler(ISubMerchantDocumentService service)
    {
        _service = service;
    }

    public async Task<Unit> Handle(UpdateSubMerchantDocumentCommand request, CancellationToken cancellationToken)
    {
        await _service.UpdateAsync(request);
        
        return Unit.Value;
    }
}