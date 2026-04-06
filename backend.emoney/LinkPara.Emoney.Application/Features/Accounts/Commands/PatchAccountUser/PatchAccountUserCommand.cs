using LinkPara.Emoney.Application.Commons.Interfaces;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.PatchAccountUser
{
    public class PatchAccountUserCommand : IRequest
    {
        public Guid AccountId { get; set; }
        public Guid AccountUserId { get; set; }
        public JsonPatchDocument<PatchAccountUserDto> PatchAccountUserDto { get; set; }
    }
    public class PatchAccountUserCommandHandler : IRequestHandler<PatchAccountUserCommand>
    {
        private readonly IAccountService _accountService;

        public PatchAccountUserCommandHandler(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<Unit> Handle(PatchAccountUserCommand request, CancellationToken cancellationToken)
        {
            await _accountService.PatchAccountUserAsync(request);
            return Unit.Value;
        }
    }
}
