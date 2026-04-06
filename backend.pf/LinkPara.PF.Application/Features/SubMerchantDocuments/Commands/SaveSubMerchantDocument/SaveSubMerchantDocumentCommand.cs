using AutoMapper;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantDocuments.Commands.SaveSubMerchantDocument;

public class SaveSubMerchantDocumentCommand : IRequest, IMapFrom<SubMerchantDocument>
{
    public Guid DocumentId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public string DocumentName { get; set; }
    public Guid SubMerchantId { get; set; }
}

public class SaveSubMerchantDocumentCommandHandler : IRequestHandler<SaveSubMerchantDocumentCommand, Unit>
{
    private readonly ISubMerchantDocumentService _subMerchantDocumentService;

    public SaveSubMerchantDocumentCommandHandler(ISubMerchantDocumentService subMerchantDocumentService)
    {
        _subMerchantDocumentService = subMerchantDocumentService;
    }

    public async Task<Unit> Handle(SaveSubMerchantDocumentCommand request, CancellationToken cancellationToken)
    {
        await _subMerchantDocumentService.SaveAsync(request);
        
        return Unit.Value;
    }
}