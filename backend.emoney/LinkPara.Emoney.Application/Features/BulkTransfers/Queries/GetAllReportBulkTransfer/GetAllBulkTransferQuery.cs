using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.BulkTransfers.Queries.GetAllReportBulkTransfer;

public class GetAllReportBulkTransferQuery : SearchQueryParams, IRequest<PaginatedList<BulkTransferDto>>
{
    public BulkTransferStatus? BulkTransferStatus { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string FileName { get; set; }
}

public class GetAllReportBulkTransferQueryHandler : IRequestHandler<GetAllReportBulkTransferQuery, PaginatedList<BulkTransferDto>>
{
    private readonly IGenericRepository<BulkTransfer> _bulkTransferRepository;
    private readonly IMapper _mapper;
    private readonly IAccountService _accountService;
    private readonly IContextProvider _contextProvider;

    public GetAllReportBulkTransferQueryHandler(IGenericRepository<BulkTransfer> bulkTransferRepository,
        IMapper mapper,
        IAccountService accountService,
        IContextProvider contextProvider)
    {
        _bulkTransferRepository = bulkTransferRepository;
        _mapper = mapper;
        _accountService = accountService;
        _contextProvider = contextProvider;
    }

    public async Task<PaginatedList<BulkTransferDto>> Handle(GetAllReportBulkTransferQuery request, CancellationToken cancellationToken)
    {
        var createdUserId = _contextProvider.CurrentContext.UserId;

        var accountUser = await _accountService.GetCorporateAccountUserAsync(Guid.Parse(createdUserId));
        var notDisplayBulkTransferStatus = new List<BulkTransferStatus> { BulkTransferStatus.WaitingMoneyTransfer, BulkTransferStatus.Waiting};
        var query = _bulkTransferRepository.GetAll()
         .Where(x => x.AccountId == accountUser.AccountId && !notDisplayBulkTransferStatus.Contains(x.BulkTransferStatus))
         .OrderBy(x => x.CreateDate)
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

        if (!string.IsNullOrWhiteSpace(request.FileName))
        {
            query = query.Where(s => s.FileName.ToLower().Contains(request.FileName.ToLower()));
        }

        return await query
          .PaginatedListWithMappingAsync<BulkTransfer, BulkTransferDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}