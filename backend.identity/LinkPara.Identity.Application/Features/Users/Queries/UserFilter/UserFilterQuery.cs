using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.UserFilter;

public class UserFilterQuery : SearchQueryParams, IRequest<PaginatedList<UserDtoWithRoles>>
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string UserName { get; set; }
    public string FullName { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public UserStatus? UserStatus { get; set; }
    public Guid? RoleId { get; set; }
    public UserType? UserType { get; set; }
}

public class UserFilterQueryHandler : IRequestHandler<UserFilterQuery, PaginatedList<UserDtoWithRoles>>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IMapper _mapper;

    public UserFilterQueryHandler(IMapper mapper,
        IRepository<User> usersRepository)
    {
        _mapper = mapper;
        _usersRepository = usersRepository;
    }

    public async Task<PaginatedList<UserDtoWithRoles>> Handle(UserFilterQuery request, CancellationToken cancellationToken)
    {
        var query = _usersRepository.GetAll();

        if (request.Email is not null)
        {
            query = query.Where(x => x.Email.ToLower()
                         .Contains(request.Email.ToLower()));
        }
        if (request.UserName is not null)
        {
            query = query.Where(x => x.UserName.Contains(request.UserName));
        }
        if (request.PhoneNumber is not null)
        {
            query = query.Where(x =>
                x.UserName.Contains(request.PhoneNumber) ||
                x.PhoneNumber.Contains(request.PhoneNumber));
        }

        if (request.FullName is not null)
        {
            query = query.Where(x => x.FirstName.ToLower()
                         .Contains(request.FullName.ToLower())
                         || x.LastName.ToLower()
                         .Contains(request.FullName.ToLower())
                         || (x.FirstName.ToLower() + " " + x.LastName.ToLower())
                         .Contains(request.FullName.ToLower()));
        }

        if (request.UserType.HasValue)
        {
            query = query.Where(x => x.UserType == request.UserType.Value);
        }

        if (request.UserStatus.HasValue)
        {
            query = query.Where(x => x.UserStatus == request.UserStatus);
        }

        if (request.StartDate.HasValue && request.EndDate.HasValue)
        {
            query = query.Where(x =>
                x.CreateDate.Date >= request.StartDate.Value.Date &&
                x.CreateDate.Date <= request.EndDate.Value.Date);
        }

        query = query.Include(s => s.Roles);

        if (request.RoleId.HasValue)
        {
            query = query.Where(s => s.Roles.Select(s => s.Id).Contains(request.RoleId.Value));
        }

        var response = await query
            .PaginatedListWithMappingAsync<User,UserDtoWithRoles>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);

        return response;
    }
}