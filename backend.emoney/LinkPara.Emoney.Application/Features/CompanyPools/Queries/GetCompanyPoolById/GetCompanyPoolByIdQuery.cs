using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.CompanyPools.Queries.GetCompanyPoolById;

public class GetCompanyPoolByIdQuery : IRequest<CompanyPoolDto>
{
    public Guid Id { get; set; }
    public bool LoadDocument { get; set; }
}

public class GetCompanyPoolByIdQueryHandler : IRequestHandler<GetCompanyPoolByIdQuery, CompanyPoolDto>
{
    private readonly IGenericRepository<CompanyPool> _repository;
    private readonly IMapper _mapper;
    private readonly IDocumentService _documentService;

    public GetCompanyPoolByIdQueryHandler(IGenericRepository<CompanyPool> repository,
        IMapper mapper,
        IDocumentService documentService)
    {
        _repository = repository;
        _mapper = mapper;
        _documentService = documentService;
    }

    public async Task<CompanyPoolDto> Handle(GetCompanyPoolByIdQuery request, CancellationToken cancellationToken)
    {
        var companyPool = await _repository.GetByIdAsync(request.Id);

        var result = _mapper.Map<CompanyPoolDto>(companyPool);

        if (request.LoadDocument)
        {
            var documents = await _documentService.GetDocuments(new GetDocumentListRequest { AccountId = request.Id});
            result.Documents = documents.Items;
        }

        return result;
    }
}
