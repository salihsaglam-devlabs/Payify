using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Billing.Application.Features.Vendors.Queries.GetAllVendor;

public class GetAllVendorQuery : SearchQueryParams, IRequest<PaginatedList<VendorDto>>
{
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllVendorQueryHandler : IRequestHandler<GetAllVendorQuery, PaginatedList<VendorDto>>
{
    private readonly IVendorService _vendorService;

    public GetAllVendorQueryHandler(IVendorService vendorService)
    {
        _vendorService = vendorService;
    }

    public async Task<PaginatedList<VendorDto>> Handle(GetAllVendorQuery request, CancellationToken cancellationToken)
    {
        return await _vendorService.GetAllAsync(request);
    }
}