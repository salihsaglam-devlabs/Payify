using AutoMapper;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.IKS.Application.Features.Terminal.Queries.GetAllTerminalHistory;

public class GetAllTerminalHistoryQuery : SearchQueryParams, IRequest<PaginatedList<IksTerminalHistoryDto>>
{
    public Guid? MerchantId { get; set; }
    public Guid? VposId { get; set; }
    public string ReferenceCode { get; set; }
    public DateTime? QueryDateStart { get; set; }
    public DateTime? QueryDateEnd { get; set; }
    public TerminalRecordType? TerminalRecordType { get; set; }
}
public class GetAllTerminalHistoryQueryHandler : IRequestHandler<GetAllTerminalHistoryQuery, PaginatedList<IksTerminalHistoryDto>>
{
    private readonly IGenericRepository<IksTerminalHistory> _repository;
    private readonly IMapper _mapper;

    public GetAllTerminalHistoryQueryHandler(IGenericRepository<IksTerminalHistory> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<IksTerminalHistoryDto>> Handle(GetAllTerminalHistoryQuery request, CancellationToken cancellationToken)
    {
        var iksTerminalHistoryList = _repository.GetAll()
            .OrderBy(b => b.CreateDate).AsQueryable();

        if (!string.IsNullOrEmpty(request.ReferenceCode))
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.ReferenceCode.Contains(request.ReferenceCode));
        }

        if (request.QueryDateStart is not null)
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.QueryDate
                               >= request.QueryDateStart);
        }

        if (request.QueryDateEnd is not null)
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.QueryDate
                               <= request.QueryDateEnd);
        }

        if (request.MerchantId is not null)
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.VposId is not null)
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.VposId
                               == request.VposId);
        }

        if (request.TerminalRecordType is not null)
        {
            iksTerminalHistoryList = iksTerminalHistoryList.Where(b => b.TerminalRecordType
                               == request.TerminalRecordType);
        }

        return await iksTerminalHistoryList
            .PaginatedListWithMappingAsync<IksTerminalHistory, IksTerminalHistoryDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
