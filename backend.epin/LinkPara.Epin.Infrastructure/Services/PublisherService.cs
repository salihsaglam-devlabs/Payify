using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using Entities = LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SystemUser;
using LinkPara.SharedModels.Pagination;
using LinkPara.Epin.Application.Features.Publishers;
using LinkPara.Epin.Application.Features.Publishers.Queries;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;

namespace LinkPara.Epin.Infrastructure.Services;

public class PublisherService : IPublisherService
{
    private readonly IEpinHttpClient _httpClient;
    private readonly IGenericRepository<Entities.Publisher> _publisherRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;

    public PublisherService(IEpinHttpClient httpClient,
        IGenericRepository<Entities.Publisher> publisherRepository,
        IApplicationUserService applicationUserService,
        IMapper mapper)
    {
        _httpClient = httpClient;
        _publisherRepository = publisherRepository;
        _applicationUserService = applicationUserService;
        _mapper = mapper;
    }

    public async Task AddOrUpdatePublishers(List<PublisherServiceDto> publishers)
    {
        var dbPublishers = await _publisherRepository.GetAll().ToListAsync();

        foreach (var item in publishers)
        {
            if (!dbPublishers.Any(x => x.ExternalId == item.Id))
            {
                var entity = new Entities.Publisher
                {
                    ExternalId = item.Id,
                    Name = item.Name,
                    RecordStatus = RecordStatus.Active,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                };
                await _publisherRepository.AddAsync(entity);
            }
            else
            {
                var publisher = dbPublishers.FirstOrDefault(x => x.ExternalId == item.Id);

                if (publisher is null)
                {
                    throw new NotFoundException(nameof(publisher));
                }

                publisher.ExternalId = item.Id;
                publisher.Name = item.Name;
                publisher.RecordStatus = RecordStatus.Active;

                await _publisherRepository.UpdateAsync(publisher);

                dbPublishers.Remove(publisher);

            }
        }

        foreach (var item in dbPublishers)
        {
            item.RecordStatus = RecordStatus.Passive;
            await _publisherRepository.UpdateAsync(item);
        }
    }

    public async Task<PaginatedList<PublisherDto>> GetFilterPublishersAsync(GetFilterPublishersQuery request)
    {
        var publishers = _publisherRepository.GetAll().Where(b => b.RecordStatus == RecordStatus.Active);

        if (!string.IsNullOrEmpty(request.Q))
        {
            publishers = publishers.Where(b => b.Name.Contains(request.Q));
        }

        return await publishers
           .PaginatedListWithMappingAsync<Entities.Publisher, PublisherDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<List<PublisherServiceDto>> GetPublishersFromServiceAsync()
    {
        var response = await _httpClient.GetPublishersAsync();
        return response.Publishers;
    }
}
