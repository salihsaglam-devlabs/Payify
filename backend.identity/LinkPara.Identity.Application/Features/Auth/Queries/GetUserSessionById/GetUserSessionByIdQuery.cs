using AutoMapper;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.Auth.Queries.GetUserSessionById;

public  class GetUserSessionByIdQuery : IRequest<UserSessionDto>
{
    public Guid SessionId { get; set; }
}

public class GetUserSessionByIdQueryHandler : IRequestHandler<GetUserSessionByIdQuery, UserSessionDto>
{
    private readonly IRepository<UserSession> _userSessionTokenRepository;
    private readonly IMapper _mapper;

    public GetUserSessionByIdQueryHandler(IRepository<UserSession> userSessionTokenRepository, 
        IMapper mapper)
    {
        _userSessionTokenRepository = userSessionTokenRepository;
        _mapper = mapper;
    }

    public async Task<UserSessionDto> Handle(GetUserSessionByIdQuery request, CancellationToken cancellationToken)
    {
        var userSession = await _userSessionTokenRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == request.SessionId);

        if(userSession == null)
        {
            throw new NotFoundException("User Session Not Found");
        }

        return _mapper.Map<UserSessionDto>(userSession);
    }
}