using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Billing.Application.Features.SavedBills.Queries.GetAllSavedBill;

public class GetAllSavedBillQuery : SearchQueryParams, IRequest<PaginatedList<SavedBillDto>>
{
    public Guid UserId { get; set; }
    public Guid InstitutionId { get; set; }
    public string SubscriberNumber1 { get; set; }
    public string SubscriberNumber2 { get; set; }
    public string SubscriberNumber3 { get; set; }
    public string BillName { get; set; }
}

public class GetAllSavedBillQueryHandler : IRequestHandler<GetAllSavedBillQuery, PaginatedList<SavedBillDto>>
{
    private readonly ISavedBillService _savedBillService;

    public GetAllSavedBillQueryHandler(ISavedBillService savedBillService)
    {
        _savedBillService = savedBillService;
    }

    public async Task<PaginatedList<SavedBillDto>> Handle(GetAllSavedBillQuery request, CancellationToken cancellationToken)
    {
        return await _savedBillService.GetAllAsync(request);
    }
}