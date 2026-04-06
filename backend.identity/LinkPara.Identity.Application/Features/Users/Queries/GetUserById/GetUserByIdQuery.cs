using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<UserDto>
{
    public Guid UserId { get; set; }
}

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
{
    private readonly IRepository<User> _repository;
    private readonly IMapper _mapper;

    public GetUserByIdQueryHandler(IRepository<User> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetAll()
            .Include(x => x.LoginLastActivity)
            .SingleOrDefaultAsync(x => x.Id == request.UserId);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserId);
        }

        var result = _mapper.Map<User, UserDto>(user);

        return result;
    }
}