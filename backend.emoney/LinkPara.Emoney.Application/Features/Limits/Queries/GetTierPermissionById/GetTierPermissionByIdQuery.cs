using AutoMapper;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionById;

public class GetTierPermissionByIdQuery : IRequest<TierPermissionDto>
{
    public Guid Id { get; set; }
}

public class GetTierPermissionByIdQueryHandler : IRequestHandler<GetTierPermissionByIdQuery, TierPermissionDto>
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IMapper _mapper;
    public GetTierPermissionByIdQueryHandler(
        IGenericRepository<TierPermission> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<TierPermissionDto> Handle(GetTierPermissionByIdQuery request, CancellationToken cancellationToken)
    {
        var tierPermission = await _repository.GetAll()
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (tierPermission == null)
        {
            throw new NotFoundException(nameof(TierPermission), request.Id);
        }
        
        return _mapper.Map<TierPermissionDto>(tierPermission);
    }
}