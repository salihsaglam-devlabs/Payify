using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Features.CompanyPools;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetAllBulkTransfer;

public class GetAllBulkTransferQuery : SearchQueryParams, IRequest<PaginatedList<BulkTransferDto>>
{
    public BulkTransferStatus? BulkTransferStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class GetAllBulkTransferQueryHandler : IRequestHandler<GetAllBulkTransferQuery, PaginatedList<BulkTransferDto>>
{
    private readonly IGenericRepository<BulkTransfer> _bulkTransferRepository;
    private readonly IMapper _mapper;
    private readonly IAccountService _accountService;
    private readonly IContextProvider _contextProvider;

    public GetAllBulkTransferQueryHandler(IGenericRepository<BulkTransfer> bulkTransferRepository,
        IMapper mapper,
        IAccountService accountService,
        IContextProvider contextProvider)
    {
        _bulkTransferRepository = bulkTransferRepository;
        _mapper = mapper;
        _accountService = accountService;
        _contextProvider = contextProvider;
    }

    public async Task<PaginatedList<BulkTransferDto>> Handle(GetAllBulkTransferQuery request, CancellationToken cancellationToken)
    {
        var createdUserId = _contextProvider.CurrentContext.UserId;

        var accountUser = await _accountService.GetCorporateAccountUserAsync(Guid.Parse(createdUserId));

        var query = _bulkTransferRepository.GetAll()
         .Where(x => x.AccountId == accountUser.AccountId)
         .AsQueryable();

        if (request.StartDate.HasValue)
        {
            query = query.Where(x => x.CreateDate >= request.StartDate);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(x => x.CreateDate <= request.EndDate);
        }

        if (request.BulkTransferStatus.HasValue)
        {
            query = query.Where(s => s.BulkTransferStatus == request.BulkTransferStatus.Value);
        }

        return await query
          .PaginatedListWithMappingAsync<BulkTransfer, BulkTransferDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}