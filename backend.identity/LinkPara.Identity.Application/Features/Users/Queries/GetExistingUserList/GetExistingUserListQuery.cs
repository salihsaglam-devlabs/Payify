using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetExistingUserList;

public class GetExistingUserListQuery : IRequest<ExistingUsersDto>
{
    public string Email { get; set; }
    public string UserName { get; set; }
}

public class GetExistingUserListQueryHandler : IRequestHandler<GetExistingUserListQuery, ExistingUsersDto>
{
    private readonly IRepository<User> _usersRepository;

    public GetExistingUserListQueryHandler(IRepository<User> usersRepository)
    {
        _usersRepository = usersRepository;
    }

    public async Task<ExistingUsersDto> Handle(GetExistingUserListQuery request, CancellationToken cancellationToken)
    {
        var query = _usersRepository
            .GetAll()
            .Where(x => x.RecordStatus == RecordStatus.Active);

        if (request.Email is not null)
        {
            query = query.Where(x => x.Email.ToLower()
                .Contains(request.Email.ToLower()));
        }
        if (request.UserName is not null)
        {
            query = query.Where(x => x.UserName.Contains(request.UserName));
        }

        var users = await query.Select(x => x.UserName).ToListAsync(cancellationToken: cancellationToken);

        return new ExistingUsersDto
        {
            IsExists = users.Count > 0,
            UserNames = users
        };
    }
}