using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Fields;
using LinkPara.Billing.Application.Features.Fields.Queries;
using LinkPara.Billing.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Infrastructure.Services.FieldServices;

public class FieldService : IFieldService
{
    private readonly IGenericRepository<Field> _fieldRepository;
    private readonly IMapper _mapper;

    public FieldService(IGenericRepository<Field> fieldRepository, IMapper mapper)
    {
        _fieldRepository = fieldRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<FieldDto>> GetByInstitutionIdAsync(GetByInstitutionIdQuery request)
    {
        return await _fieldRepository.GetAll()
            .Where(f => f.InstitutionId == request.InstitutionId)
            .PaginatedListWithMappingAsync<Field,FieldDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}