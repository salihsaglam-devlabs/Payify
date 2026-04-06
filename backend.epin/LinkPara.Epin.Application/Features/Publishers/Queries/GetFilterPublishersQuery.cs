using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Epin.Application.Features.Publishers.Queries;

public class GetFilterPublishersQuery : SearchQueryParams, IRequest<PaginatedList<PublisherDto>>
{
}


public class GetFilterPublishersQueryHandler : IRequestHandler<GetFilterPublishersQuery, PaginatedList<PublisherDto>>
{
    private readonly IPublisherService _publisherService;

    public GetFilterPublishersQueryHandler(IPublisherService publisherService)
    {
        _publisherService = publisherService;
    }

    public async Task<PaginatedList<PublisherDto>> Handle(GetFilterPublishersQuery request, CancellationToken cancellationToken)
    {
        return await _publisherService.GetFilterPublishersAsync(request);
    }
}