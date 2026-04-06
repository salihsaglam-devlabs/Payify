using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.DeleteSubMerchantDocument;

public class DeleteSubMerchantDocumentCommand : IRequest
{
    public Guid DocumentId { get; set; }
}

public class DeleteSubMerchantDocumentCommandHandler : IRequestHandler<DeleteSubMerchantDocumentCommand>
{
    private readonly ISubMerchantDocumentService _subMerchantDocumentService;

    public DeleteSubMerchantDocumentCommandHandler(ISubMerchantDocumentService subMerchantDocumentService)
    {
        _subMerchantDocumentService = subMerchantDocumentService;
    }

    public async Task<Unit> Handle(DeleteSubMerchantDocumentCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantDocumentService.DeleteAsync(request.DocumentId);

        return Unit.Value;
    }
}
