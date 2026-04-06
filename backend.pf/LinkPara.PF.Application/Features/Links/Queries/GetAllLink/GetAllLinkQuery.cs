using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;


namespace LinkPara.PF.Application.Features.Links.Queries.GetAllLink;

public class GetAllLinkQuery : SearchQueryParams, IRequest<PaginatedList<LinkDto>>
{
    public Guid MerchantId { get; set; }
    public Guid? SubMerchantId { get; set; }
    public LinkSearchType LinkSearchType { get; set; }
    public LinkInfoSearchRequest LinkInfoSearchRequest { get; set; }
    public LinkTransactionSearchRequest LinkTransactionSearchRequest { get; set; }
    public LinkCustomerSearchRequest LinkCustomerSearchRequest { get; set; }
}
public class GetAllLinkQueryHandler : IRequestHandler<GetAllLinkQuery, PaginatedList<LinkDto>>
{
    private readonly ILinkService _linkService;

    public GetAllLinkQueryHandler(ILinkService linkService)
    {
        _linkService = linkService;
    }
    public async Task<PaginatedList<LinkDto>> Handle(GetAllLinkQuery request, CancellationToken cancellationToken)
    {
        return await _linkService.GetListAsync(request);
    }
}