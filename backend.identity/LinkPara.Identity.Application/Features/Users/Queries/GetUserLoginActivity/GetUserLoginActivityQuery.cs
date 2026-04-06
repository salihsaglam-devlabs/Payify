using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Users.Queries.GetUserLoginActivity;

public class GetUserLoginActivityQuery : IRequest<GetUserLoginActivityResponse>
{
    public Guid UserId { get; set; }
    public string Channel { get; set; }
}
public class GetUserLoginActivityQueryHandler : IRequestHandler<GetUserLoginActivityQuery, GetUserLoginActivityResponse>
{
    private readonly IRepository<LoginActivity> _loginActivity;
    private readonly IRepository<UserLoginLastActivity> _userLoginLastActivity;
    private readonly IMapper _mapper;
    public GetUserLoginActivityQueryHandler(
        IRepository<LoginActivity> loginActivity,
        IRepository<UserLoginLastActivity> userLoginLastActivity,
        IMapper mapper)
    {
        _loginActivity = loginActivity;
        _userLoginLastActivity = userLoginLastActivity;
        _mapper = mapper;
    }
    public async Task<GetUserLoginActivityResponse> Handle(GetUserLoginActivityQuery request, CancellationToken cancellationToken)
    {
        var userLastActivity = await _userLoginLastActivity.GetAll()
                               .SingleOrDefaultAsync(x => x.UserId == request.UserId);

        if (userLastActivity is null)
        {
            throw new NotFoundException(nameof(UserLoginLastActivity), request.UserId);
        }

        var loginActivities = _loginActivity.GetAll()
            .Where(x => x.UserId == request.UserId);

        if (!string.IsNullOrEmpty(request.Channel))
        {
            loginActivities = loginActivities.Where(x => x.Channel == request.Channel);
        }

        loginActivities = loginActivities.OrderByDescending(x => x.Date);

        var lastFailedLogin = await loginActivities
            .FirstOrDefaultAsync(x => x.LoginResult == LoginResult.Failed);

        var lastTenActivity = await loginActivities
            .Take(10)
            .ToListAsync();

        return new GetUserLoginActivityResponse
        {
            LastFailedLogin = userLastActivity.LastFailedLogin,
            LastSucceededLogin = userLastActivity.LastSucceededLogin,
            LoginActivities = _mapper.Map<List<LoginActivity>, List<LoginActivityDto>>(lastTenActivity),
            FailedLoginCount = userLastActivity.FailedLoginCount,
            LastFailedLoginIPAddress = lastFailedLogin?.IP
        };
    }
}
