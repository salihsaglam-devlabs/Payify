using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.Banks.Queries.GetAllBank;

public class GetAllBankQuery : SearchQueryParams, IRequest<PaginatedList<BankDto>>
{
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllBankQueryHandler : IRequestHandler<GetAllBankQuery, PaginatedList<BankDto>>
{
    private readonly IGenericRepository<Bank> _repository;
    private readonly IMapper _mapper;

    public GetAllBankQueryHandler(IGenericRepository<Bank> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<BankDto>> Handle(GetAllBankQuery request, CancellationToken cancellationToken)
    {
        var list = _repository.GetAll().Where(b => b.Code != 0);

        if (request.RecordStatus is not null)
        {
            list = list.Where(b => b.RecordStatus == request.RecordStatus);
        }

        return await list.OrderBy(b => b.Name)
           .PaginatedListWithMappingAsync<Bank, BankDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
