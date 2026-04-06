using LinkPara.Identity.Application.Common.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.Identity.Application.Features.Users.Commands.UpdateUser;
public class RemoveUserLockCommand : IRequest
{
    public Guid UserId { get; set; }
}
public class RemoveUserLockCommandHandler : IRequestHandler<RemoveUserLockCommand>
{
    private readonly IRepository<User> _userRepository;
    private readonly ILogger<UpdateUserCommandHandler> _logger;
    public RemoveUserLockCommandHandler(
        IRepository<User> userRepository,
        ILogger<UpdateUserCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }
    public async Task<Unit> Handle(RemoveUserLockCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

            if (user == null)
            {
                throw new UserNotFoundException();
            }

            user.LockoutEnd = null;

            await _userRepository.UpdateAsync(user);
        }
        catch (Exception exception)
        {
            _logger.LogError($"UserLock could not be removed : {exception}");
            throw new RemoveUserLockException();
        }
        return Unit.Value;
    }
}
