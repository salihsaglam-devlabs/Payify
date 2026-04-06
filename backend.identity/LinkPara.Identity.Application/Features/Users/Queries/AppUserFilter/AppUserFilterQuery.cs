using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.Account.Commands.ForgotPassword;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Identity.Application.Features.Users.Queries.AppUserFilter;

public class AppUserFilterQuery : IRequest<List<UserDto>>
{
    public string Email { get; set; }
    public string UserName { get; set; }
}

public class AppUserFilterQueryHandler : IRequestHandler<AppUserFilterQuery, List<UserDto>>
{
    private readonly IRepository<User> _usersRepository;
    private readonly IMapper _mapper;
    private readonly ILogger<AppUserFilterQueryHandler> _logger;

    public AppUserFilterQueryHandler(IMapper mapper,
        IRepository<User> usersRepository,
        ILogger<AppUserFilterQueryHandler> logger)
    {
        _mapper = mapper;
        _usersRepository = usersRepository;
        _logger = logger;
    }

    public async Task<List<UserDto>> Handle(AppUserFilterQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var query = _usersRepository
                .GetAll()
                .Where(x => x.UserType == UserType.ApplicationUser);

            if (request.Email is not null)
            {
                query = query.Where(x => x.Email.ToLower()
                             .Contains(request.Email.ToLower()));
            }
            if (request.UserName is not null)
            {
                query = query.Where(x => x.UserName.Contains(request.UserName));
            }

            var response = await query.ToListAsync();

            return _mapper.Map<List<UserDto>>(response);
        }
        catch (Exception ex)
        {
            _logger.LogError($"AppUser: \"{ex}");
        }

        return new List<UserDto>();
    }
}