using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Sectors;
using LinkPara.Billing.Application.Features.Sectors.Queries;
using LinkPara.Billing.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Infrastructure.Services.SectorServices;

public class SectorService : ISectorService
{
    private readonly IGenericRepository<Sector> _sectorRepository;
    private readonly IMapper _mapper;

    public SectorService(IGenericRepository<Sector> sectorRepository, IMapper mapper)
    {
        _sectorRepository = sectorRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SectorDto>> GetListAsync(GetAllSectorQuery request) 
    {
        var sectorList = _sectorRepository.GetAll();
        
        if (request.RecordStatus is not null)
        {
            sectorList = sectorList.Where(s => s.RecordStatus == request.RecordStatus);
        }

        return await sectorList
            .PaginatedListWithMappingAsync<Sector,SectorDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}