using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Identity.Application.Features.SecurityPictures.Queries.GetAllSecurityPicture;

public class GetAllSecurityPictureQuery : SearchQueryParams, IRequest<PaginatedList<SecurityPictureDto>>
{
    public RecordStatus? RecordStatus { get; set; }
    public string Name { get; set; }
}

public class GetAllSecurityPictureQueryHandler : IRequestHandler<GetAllSecurityPictureQuery, PaginatedList<SecurityPictureDto>>
{
    private readonly IRepository<SecurityPicture> _repository;
    private readonly IMapper _mapper;

    public GetAllSecurityPictureQueryHandler(IRepository<SecurityPicture> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<PaginatedList<SecurityPictureDto>> Handle(GetAllSecurityPictureQuery request, CancellationToken cancellationToken)
    {
        var query = _repository.GetAll();

        if (request.RecordStatus.HasValue)
            query = query.Where(x => x.RecordStatus == request.RecordStatus);

        if (!string.IsNullOrEmpty(request.Name))
            query = query.Where(x => x.Name.Contains(request.Name));

        return await query
            .PaginatedListWithMappingAsync<SecurityPicture, SecurityPictureDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
