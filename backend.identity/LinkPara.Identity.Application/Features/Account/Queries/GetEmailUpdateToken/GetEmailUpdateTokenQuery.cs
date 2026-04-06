using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.Identity.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Application.Features.Account.Queries.GetEmailUpdateToken;

public class GetEmailUpdateTokenQuery : IRequest<UpdateEmailTokenDto>
{
    public string UserId { get; set; }
    public string NewEmail { get; set; }
}

public class GetEmailUpdateTokenCommandHandler : IRequestHandler<GetEmailUpdateTokenQuery, UpdateEmailTokenDto>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<User> _userRepository;

    public GetEmailUpdateTokenCommandHandler(UserManager<User> userManager,
        IRepository<User> userRepository)
    {
        _userManager = userManager;
        _userRepository = userRepository;
    }

    public async Task<UpdateEmailTokenDto> Handle(GetEmailUpdateTokenQuery command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId);

        if (user is null)
        {
            throw new UserNotFoundException();
        }

        var dbChecker = await _userRepository
            .GetAll()
            .Select(x => new
            {
                x.Email,
                x.UserStatus,
                x.UserType
            })
            .FirstOrDefaultAsync(x =>
                x.Email == command.NewEmail.ToLower()
                && (x.UserStatus == UserStatus.Active || x.UserStatus == UserStatus.Suspended)
                && x.UserType == user.UserType, cancellationToken);
        
        if (dbChecker is not null)
        {
            throw new AlreadyInUseException(nameof(command.NewEmail));
        }

        var token = await _userManager.GenerateChangeEmailTokenAsync(user, command.NewEmail);

        return new UpdateEmailTokenDto
        {
            Token = Base64UrlEncoder.Encode(token)
        };
    }
}