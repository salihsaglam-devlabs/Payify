using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Queries.GetAllDocuments;

public class GetAllDocumentsQuery : IRequest<List<AgreementDocumentVersionDto>>
{
}

public class GetAllDocumentsQueryHandler : IRequestHandler<GetAllDocumentsQuery, List<AgreementDocumentVersionDto>>
{
    private readonly IMapper _mapper;
    private readonly IRepository<AgreementDocumentVersion> _repository;
    private readonly IContextProvider _contextProvider;

    public GetAllDocumentsQueryHandler(IMapper mapper,
        IRepository<AgreementDocumentVersion> repository,
        IContextProvider contextProvider)
    {
        _mapper = mapper;
        _repository = repository;
        _contextProvider = contextProvider;
    }

    public async Task<List<AgreementDocumentVersionDto>> Handle(GetAllDocumentsQuery request, CancellationToken cancellationToken)
    {
        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var documentVersions = await _repository.GetAll(s => s.AgreementDocument)
            .Where(q => q.IsLatest && q.LanguageCode == languageCode && q.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);

        if (!documentVersions.Any())
        {
            documentVersions = await _repository.GetAll(s => s.AgreementDocument)
            .Where(q => q.IsLatest && q.LanguageCode == "tr" && q.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);
        }

        return _mapper.Map<List<AgreementDocumentVersion>, List<AgreementDocumentVersionDto>>(documentVersions);
    }
}
