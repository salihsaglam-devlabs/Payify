using AutoMapper;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.NaceCodes;
using LinkPara.PF.Application.Features.NaceCodes.Queries.GetAllNaceCodes;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Infrastructure.Services;

public class NaceService : INaceService
{
    private readonly IGenericRepository<Nace> _naceRepository;
    private readonly IMapper _mapper;

    public NaceService(IGenericRepository<Nace> naceRepository, IMapper mapper)
    {
        _naceRepository = naceRepository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<NaceDto>> GetListAsync(GetAllNaceCodesQuery request)
    {
        var naceList = _naceRepository.GetAll();
        
        if (!string.IsNullOrEmpty(request.SectorCode))
        {
            naceList = naceList.Where(n => n.SectorCode == request.SectorCode);
        }

        if (!string.IsNullOrEmpty(request.ProfessionCode))
        {
            naceList = naceList.Where(n => n.ProfessionCode == request.ProfessionCode);
        }

        if (!string.IsNullOrEmpty(request.Code))
        {
            naceList = naceList.Where(n => n.Code == request.Code);
        }

        if (!string.IsNullOrEmpty(request.Q))
        {
            naceList = naceList.Where(b => b.Description.Contains(request.Q));
        }

        return await naceList.OrderBy(b=>b.Code)
            .PaginatedListWithMappingAsync<Nace,NaceDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<NaceDto> GetByIdAsync(Guid id)
    {
        var nace = await _naceRepository.GetByIdAsync(id);
        
        if (nace is null)
        {
            throw new NotFoundException(nameof(Nace), id);
        }

        return _mapper.Map<NaceDto>(nace);
    }
}