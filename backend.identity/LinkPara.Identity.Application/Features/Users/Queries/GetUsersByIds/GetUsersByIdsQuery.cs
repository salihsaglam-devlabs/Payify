using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUsersByIds;
public class GetUsersByIdsQuery : IRequest<List<UserDto>>
{
    public List<Guid> UserIds { get; set; }
}
public class GetUsersByIdsQueryHandler : IRequestHandler<GetUsersByIdsQuery, List<UserDto>>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IMapper _mapper;
    public GetUsersByIdsQueryHandler(IRepository<User> usersRepository, IMapper mapper)
    {
        _usersRepository = usersRepository;
        _mapper = mapper;
    }
    public async Task<List<UserDto>> Handle(GetUsersByIdsQuery request, CancellationToken cancellationToken)
    {
        var userList = await _usersRepository
            .GetAll()
            .Where(x => 
                   request.UserIds.Contains(x.Id) && 
                   x.RecordStatus == RecordStatus.Active)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<UserDto>>(userList);
    }
}
