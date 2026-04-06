using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetIsUserExist;

public class GetIsUserExistQuery : IRequest<bool>
{
    public string UserName { get; set; }
}

public class GetIsUserExistQueryHandler : IRequestHandler<GetIsUserExistQuery, bool>
{
    private readonly IRepository<User> _repository;

    public GetIsUserExistQueryHandler(IRepository<User> repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(GetIsUserExistQuery request, CancellationToken cancellationToken)
    {
        var user = await _repository.GetAll()
            .SingleOrDefaultAsync(x => x.UserName == request.UserName && x.UserStatus != UserStatus.Inactive);

        return user is not null;
    }
}