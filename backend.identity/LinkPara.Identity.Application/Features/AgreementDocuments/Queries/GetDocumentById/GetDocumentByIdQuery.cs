using MediatR;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using AutoMapper;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetDocumentById;
public class GetDocumentByIdQuery : IRequest<AgreementDocumentResponse>
{
    public Guid AgreementDocumentId { get; set; }
}

public class GetDocumentByIdQueryHandler : IRequestHandler<GetDocumentByIdQuery, AgreementDocumentResponse>
{
    private readonly IRepository<AgreementDocument> _repository;
    private readonly IRepository<AgreementDocumentVersion> _documentVersionRepository;
    private readonly IMapper _mapper;

    public GetDocumentByIdQueryHandler(IRepository<AgreementDocument> repository,
        IRepository<AgreementDocumentVersion> documentVersionRepository,
        IMapper mapper)
    {
        _repository = repository;
        _documentVersionRepository = documentVersionRepository;
        _mapper = mapper;
    }
    public async Task<AgreementDocumentResponse> Handle(GetDocumentByIdQuery request, CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(request.AgreementDocumentId);

        if (document is null)
        {
            throw new NotFoundException(nameof(AgreementDocument), request.AgreementDocumentId);
        }

        var documentVersionList = _documentVersionRepository.GetAll().
            Where(a => a.AgreementDocumentId == request.AgreementDocumentId
            && a.RecordStatus == RecordStatus.Active && a.IsLatest);

        var agreementDocument = _mapper.Map<AgreementDocumentResponse>(document);
        agreementDocument.Agreements = new List<DocumentVersionDto>();

        foreach (var documentVersion in documentVersionList)
        {
            var docVersion = _mapper.Map<DocumentVersionDto>(documentVersion);
            agreementDocument.Agreements.Add(docVersion);
        }

        return agreementDocument;
    }
}
