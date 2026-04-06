using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserSecurityPictures.Commands.ResetUserSecurityPicture;

public class ResetUserSecurityPictureCommand : IRequest
{
    public Guid UserId { get; set; }
}

public class ResetUserSecurityPictureCommandHandler : IRequestHandler<ResetUserSecurityPictureCommand>
{
    private readonly IRepository<UserSecurityPicture> _userSecurityPictureRepository;
    private readonly IAuditLogService _auditLogService;

    public ResetUserSecurityPictureCommandHandler(
        IRepository<UserSecurityPicture> userSecurityPictureRepository,
        IAuditLogService auditLogService)
    {
        _userSecurityPictureRepository = userSecurityPictureRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(ResetUserSecurityPictureCommand command, CancellationToken cancellationToken)
    {
        var userSecurityPicture = await _userSecurityPictureRepository.GetAll()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId && x.RecordStatus == RecordStatus.Active, cancellationToken);

        if (userSecurityPicture is null)
            throw new NotFoundException(nameof(UserSecurityPicture));

        userSecurityPicture.RecordStatus = RecordStatus.Passive;
        await _userSecurityPictureRepository.UpdateAsync(userSecurityPicture);

        await _auditLogService.AuditLogAsync(new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "ResetUserSecurityPicture",
            SourceApplication = "Identity",
            Resource = "UserSecurityPicture",
            UserId = command.UserId,
            Details = new Dictionary<string, string>
            {
                { "UserId", command.UserId.ToString() }
            }
        });

        return Unit.Value;
    }
}
