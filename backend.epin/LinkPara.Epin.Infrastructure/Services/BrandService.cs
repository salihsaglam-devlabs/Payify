using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using Entities = LinkPara.Epin.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SystemUser;
using LinkPara.SharedModels.Pagination;
using LinkPara.Epin.Application.Features.Brands;
using LinkPara.Epin.Application.Features.Brands.Queries.GetFilterBrands;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.Epin.Domain.Entities;

namespace LinkPara.Epin.Infrastructure.Services;

public class BrandService : IBrandService
{
    private readonly IEpinHttpClient _httpClient;
    private readonly IGenericRepository<Entities.Brand> _brandRepository;
    private readonly IGenericRepository<Entities.Publisher> _publisherRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;

    public BrandService(IEpinHttpClient httpClient,
        IGenericRepository<Entities.Brand> brandRepository,
        IGenericRepository<Entities.Publisher> publisherRepository,
        IApplicationUserService applicationUserService,
        IMapper mapper)
    {
        _httpClient = httpClient;
        _brandRepository = brandRepository;
        _publisherRepository = publisherRepository;
        _applicationUserService = applicationUserService;
        _mapper = mapper;
    }

    public async Task AddOrUpdateBrands(List<BrandServiceDto> brands)
    {
        var dbBrands = await _brandRepository.GetAll().ToListAsync();
        var dbPublishers = await _publisherRepository.GetAll().Where(x => x.RecordStatus == RecordStatus.Active).ToListAsync();

        foreach (var item in brands)
        {
            var publisher = dbPublishers.FirstOrDefault(x => x.ExternalId == item.ExternalPublisherId);

            if (publisher is null)
            {
                throw new NotFoundException(nameof(publisher));
            }

            if (!dbBrands.Any(x => x.ExternalId == item.Id))
            {
                var entity = new Entities.Brand
                {
                    ExternalId = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Image = item.Image,
                    Summary = item.Summary,
                    PublisherId = publisher.Id,
                    Type = item.Type,
                    RecordStatus = RecordStatus.Active,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                };
                await _brandRepository.AddAsync(entity);
            }
            else
            {
                var brand = dbBrands.FirstOrDefault(x => x.ExternalId == item.Id);

                if (brand is null)
                {
                    throw new NotFoundException(nameof(brand));
                }

                brand.ExternalId = item.Id;
                brand.Name = item.Name;
                brand.Description = item.Description;
                brand.Image = item.Image;
                brand.Summary = item.Summary;
                brand.PublisherId = publisher.Id;
                brand.Type = item.Type;
                brand.RecordStatus = RecordStatus.Active;

                await _brandRepository.UpdateAsync(brand);

                dbBrands.Remove(brand);

            }
        }

        foreach (var item in dbBrands)
        {
            item.RecordStatus = RecordStatus.Passive;
            await _brandRepository.UpdateAsync(item);
        }
    }

    public async Task<List<BrandServiceDto>> GetAllBrandsFromService(List<PublisherServiceDto> publishers)
    {
        var result = new List<BrandServiceDto>();
        foreach (var publisherId in publishers.Select(x => x.Id))
        {
            var response = await _httpClient.GetBrandsAsync(publisherId);
            response.Brands.ForEach(x => x.ExternalPublisherId = publisherId);

            result.AddRange(response.Brands);
        }
        return result;
    }

    public async Task<PaginatedList<BrandDto>> GetFilterBrandsAsync(GetFilterBrandsQuery request)
    {
        var brands = _brandRepository.GetAll().Where(b => b.RecordStatus == RecordStatus.Active);

        if (!string.IsNullOrEmpty(request.Q))
        {
            brands = brands.Where(b => b.Name.Contains(request.Q));
        }

        if (request.PublisherId is not null && request.PublisherId != Guid.Empty)
        {
            brands = brands.Where(b => b.PublisherId == request.PublisherId);
        }

        return await brands
           .PaginatedListWithMappingAsync<Brand,BrandDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<Brand> GetBrandByIdAsync (Guid id)
    {
        var brand  = await _brandRepository.GetByIdAsync(id);

        if(brand is null)
        {
            throw new NotFoundException(nameof(brand));
        }
        return brand;
    }
}
