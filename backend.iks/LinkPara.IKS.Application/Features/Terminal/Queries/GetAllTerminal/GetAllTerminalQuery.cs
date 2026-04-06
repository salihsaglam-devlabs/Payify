using AutoMapper;
using LinkPara.IKS.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.IKS.Application.Features.Terminal.Queries.GetAllTerminal;

public class GetAllTerminalQuery : SearchQueryParams, IRequest<PaginatedList<IksTerminalDto>>
{
    public Guid? MerchantId { get; set; }
    public Guid? VposId { get; set; }
    public string ReferenceCode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetAllTerminalQueryHandler : IRequestHandler<GetAllTerminalQuery, PaginatedList<IksTerminalDto>>
{
    private readonly IGenericRepository<IksTerminal> _repository;
    private readonly IMapper _mapper;

    public GetAllTerminalQueryHandler(IGenericRepository<IksTerminal> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<IksTerminalDto>> Handle(GetAllTerminalQuery request, CancellationToken cancellationToken)
    {
        var iksTerminalList = _repository.GetAll()
            .OrderBy(b => b.CreateDate).AsQueryable();

        if (!string.IsNullOrEmpty(request.ReferenceCode))
        {
            iksTerminalList = iksTerminalList.Where(b => b.ReferenceCode.Contains(request.ReferenceCode));
        }

        if (request.CreateDateStart is not null)
        {
            iksTerminalList = iksTerminalList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            iksTerminalList = iksTerminalList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.MerchantId is not null)
        {
            iksTerminalList = iksTerminalList.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.VposId is not null)
        {
            iksTerminalList = iksTerminalList.Where(b => b.VposId
                               == request.VposId);
        }

        return await iksTerminalList
            .PaginatedListWithMappingAsync<IksTerminal, IksTerminalDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}