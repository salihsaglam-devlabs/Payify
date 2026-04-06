using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Identity.Application.Features.Roles.Queries.RoleFilter;

public class RoleFilterQuery : SearchQueryParams, IRequest<PaginatedList<RoleDto>>
{
    public string Name { get; set; }
    public RoleScope[] RoleScopes { get; set; }
    public RecordStatus? RecordStatus { get; set; }

}
public class RoleFilterQueryHandler : IRequestHandler<RoleFilterQuery, PaginatedList<RoleDto>>
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IMapper _mapper;

    public RoleFilterQueryHandler(IRepository<Role> roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<RoleDto>> Handle(RoleFilterQuery request, CancellationToken cancellationToken)
    {
        var query = _roleRepository.GetAll();

        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(x => x.Name.ToLower()
                         .Contains(request.Name.ToLower()));
        }

        if (request.RoleScopes is not null && request.RoleScopes.Length > 0)
        {
            query = query.Where(x =>request.RoleScopes.Contains(x.RoleScope));
        }

        if (request.RecordStatus is not null)
        {
            query = query.Where(x => x.RecordStatus == request.RecordStatus);
        }

        return await query.PaginatedListWithMappingAsync<Role,RoleDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}