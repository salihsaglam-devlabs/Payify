using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using LinkPara.Epin.Application.Features.Publishers;
using LinkPara.Epin.Application.Features.Publishers.Queries;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Epin.Application.Commons.Interfaces;

public interface IPublisherService
{
    Task AddOrUpdatePublishers(List<PublisherServiceDto> publishers);
    Task<PaginatedList<PublisherDto>> GetFilterPublishersAsync(GetFilterPublishersQuery request);
    Task<List<PublisherServiceDto>> GetPublishersFromServiceAsync();
}
