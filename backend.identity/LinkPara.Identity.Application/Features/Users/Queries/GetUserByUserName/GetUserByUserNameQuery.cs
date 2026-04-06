using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserById;

public class GetUserByUserNameQuery : IRequest<UserDto>
{
    public string UserName { get; set; }
}

public class GetUserByUserNameQueryHandler : IRequestHandler<GetUserByUserNameQuery, UserDto>
{
    private readonly IRepository<User> _repository;
    private readonly IMapper _mapper;

    public GetUserByUserNameQueryHandler(IRepository<User> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<UserDto> Handle(GetUserByUserNameQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetAll()
            .Include(x => x.LoginLastActivity)
            .SingleOrDefaultAsync(x => x.UserName == request.UserName);

        if (user is null)
        {
            throw new NotFoundException(nameof(User), request.UserName);
        }

        var result = _mapper.Map<User, UserDto>(user);

        return result;
    }
}