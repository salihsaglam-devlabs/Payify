using LinkPara.Identity.Application.Common.Models.AccountModels;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace LinkPara.Identity.Application.Features.Account.Commands.SendResetPasswordToken;
public class SendResetPasswordTokenCommand : IRequest<ResetPasswordTokenResponse>
{
    public string UserName { get; set; }
}
public class SendResetPasswordTokenCommandHandler : IRequestHandler<SendResetPasswordTokenCommand, ResetPasswordTokenResponse>
{
    private readonly UserManager<User> _userManager;
    private readonly ILogger<SendResetPasswordTokenCommandHandler> _logger;
    public SendResetPasswordTokenCommandHandler(
        UserManager<User> userManager, 
        ILogger<SendResetPasswordTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }
    public async Task<ResetPasswordTokenResponse> Handle(SendResetPasswordTokenCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByNameAsync(command.UserName);

        if (user is null)
        {
            throw new NotFoundException(nameof(User));
        }
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var encodedToken = Base64UrlEncoder.Encode(token);

        return new ResetPasswordTokenResponse
        {
            Email = user.Email,
            Token= encodedToken
        };
    }
}
