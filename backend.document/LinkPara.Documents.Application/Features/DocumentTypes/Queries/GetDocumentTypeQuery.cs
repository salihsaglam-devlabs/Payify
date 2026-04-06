using LinkPara.Documents.Domain.Entities;
using MediatR;
using AutoMapper;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Documents.Application.Features.DocumentTypes.Queries;
public class GetDocumentTypeQuery : IRequest<DocumentTypeDto>
{
    public Guid Id { get; set; }
}

public class GetDocumentTypeQueryHandler : IRequestHandler<GetDocumentTypeQuery, DocumentTypeDto>
{
    private readonly IGenericRepository<DocumentType> _documentTypesRepository;
    private readonly IMapper _mapper;

    public GetDocumentTypeQueryHandler(IGenericRepository<DocumentType> documentTypesRepository, IMapper mapper)
    {
        _documentTypesRepository = documentTypesRepository;
        _mapper = mapper;
    }

    public async Task<DocumentTypeDto> Handle(GetDocumentTypeQuery command, CancellationToken cancellationToken)
    {
        var type = await _documentTypesRepository.GetByIdAsync(command.Id);

        return _mapper.Map<DocumentTypeDto>(type);
    }
}