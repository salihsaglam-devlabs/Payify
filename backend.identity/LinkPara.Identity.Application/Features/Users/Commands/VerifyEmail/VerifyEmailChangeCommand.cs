using LinkPara.Identity.Application.Common.Interfaces;
using MediatR;

namespace LinkPara.Identity.Application.Features.Users.Commands.VerifyEmail
{
    public class VerifyEmailChangeCommand : IRequest<bool>
    {
        public string Token { get; set; }
        public string Username { get; set; }
        public string NewMail { get; set; }
    }
    public class VerifyEmailChangeCommandHandler : IRequestHandler<VerifyEmailChangeCommand, bool>
    {
        private readonly IUserEmailService _userEmailService;

        public VerifyEmailChangeCommandHandler(IUserEmailService userEmailService)
        {
            _userEmailService = userEmailService;
        }
        public async Task<bool> Handle(VerifyEmailChangeCommand command, CancellationToken cancellationToken)
        {
            var result = await _userEmailService.VerifyEmailChangeTokenAsync(command, cancellationToken);
            return result;
        }
    }
}
