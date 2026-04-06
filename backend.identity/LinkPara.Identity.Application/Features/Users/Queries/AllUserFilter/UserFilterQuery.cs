using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Users.Queries;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

public class AllUserFilterQuery : SearchQueryParams, IRequest<PaginatedList<UserDto>>
{
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string PhoneCode { get; set; }
    public string FullName { get; set; }
    public string IdentityNumber { get; set; }
    public string UserName { get; set; }
    public UserType? UserType { get; set; }
    public UserStatus? UserStatus { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class AllUserFilterQueryHandler : IRequestHandler<AllUserFilterQuery, PaginatedList<UserDto>>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IMapper _mapper;

    public AllUserFilterQueryHandler(IMapper mapper, IRepository<User> usersRepository)
    {
        _mapper = mapper;
        _usersRepository = usersRepository;
    }

    public async Task<PaginatedList<UserDto>> Handle(AllUserFilterQuery request, CancellationToken cancellationToken)
    {
        var query = _usersRepository.GetAll();

        #region String Filters
        if (request.Email is not null)
        {
            query = query.Where(x => x.Email.Contains(request.Email));
        }

        if (request.PhoneNumber is not null)
        {
            query = query.Where(x => x.UserName.Contains(request.PhoneNumber));
        }

        if (request.UserName is not null)
        {
            query = query.Where(x => x.UserName.Contains(request.UserName));
        }

        if (request.FullName is not null)
        {
            query = query.Where(x => x.FirstName.Contains(request.FullName)
            || x.LastName.Contains(request.FullName)
            || (x.FirstName + " " + x.LastName).Contains(request.FullName));
        }

        if (request.IdentityNumber is not null)
        {
            query = query.Where(x => x.IdentityNumber.Contains(request.IdentityNumber));
        }
        if (request.PhoneCode is not null)
        {
            query = query.Where(x => x.PhoneCode.Contains(request.PhoneCode));
        }
        #endregion

        if (request.UserType.HasValue)
        {
            query = query.Where(x => x.UserType == request.UserType);
        }
        if (request.UserStatus.HasValue)
        {
            query = query.Where(x => x.UserStatus == request.UserStatus);
        }
        if(request.RecordStatus.HasValue)
        {
            query = query.Where(x => x.RecordStatus == request.RecordStatus);
        }
        return await query
            .PaginatedListWithMappingAsync<User,UserDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}