using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail
{
    public class ResendEmailVerifyCommand : IRequest<bool>
    {
        public string UserId { get; set; }
    }
    public class ResendEmailVerifyCommandHandler : IRequestHandler<ResendEmailVerifyCommand,bool>
    {
        private readonly UserManager<User> _userManager;
        private readonly IUserEmailService _userEmailService;
        public ResendEmailVerifyCommandHandler(UserManager<User> userManager,IUserEmailService userEmailService)
        {
            _userManager = userManager;
            _userEmailService = userEmailService;
        }

        public async Task<bool> Handle(ResendEmailVerifyCommand command, CancellationToken cancellationToken)
        {
            
            var user = await _userManager.FindByIdAsync(command.UserId);

            if (user == null)
            {
                throw new UserNotFoundException();
            }
            if(user.EmailConfirmed)
                return false;
            await _userEmailService.SendEmailVerificationMailAsync(user);
            return true;
        }
    }

}
