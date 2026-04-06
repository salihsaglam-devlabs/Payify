using AutoMapper;
using LinkPara.Documents.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Documents.Application.Features.Documents.Queries;

public class GetDocumentQuery : IRequest<DocumentDto>
{
    public Guid Id { get; set; }
}

public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentDto>
{
    private readonly IGenericRepository<Document> _documentsRepository;
    private readonly IMapper _mapper;

    public GetDocumentQueryHandler(IGenericRepository<Document> documentsRepository,
        IMapper mapper)
    {
        _documentsRepository = documentsRepository;
        _mapper = mapper;

    }

    public async Task<DocumentDto> Handle(GetDocumentQuery request, CancellationToken cancellationToken)
    {
        var document = await _documentsRepository
                .GetAll()
                .Where(x => x.RecordStatus == RecordStatus.Active && x.Id == request.Id)
                .FirstOrDefaultAsync(cancellationToken);

        if (document == null)
        {
            throw new NotFoundException($"Document with id {request.Id} not found.");
        }

        var response = _mapper.Map<DocumentDto>(document);
        response.Bytes = File.ReadAllBytes(document.FilePath);

        return response;
    }
}
