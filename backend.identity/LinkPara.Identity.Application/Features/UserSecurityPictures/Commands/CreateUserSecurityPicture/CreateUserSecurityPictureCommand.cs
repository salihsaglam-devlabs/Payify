using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.UserSecurityPictures.Commands.CreateUserSecurityPicture;

public class CreateUserSecurityPictureCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid SecurityPictureId { get; set; }
}

public class CreateUserSecurityPictureCommandHandler : IRequestHandler<CreateUserSecurityPictureCommand>
{
    private readonly IRepository<UserSecurityPicture> _userSecurityPictureRepository;
    private readonly IRepository<SecurityPicture> _securityPictureRepository;
    private readonly IAuditLogService _auditLogService;

    public CreateUserSecurityPictureCommandHandler(
        IRepository<UserSecurityPicture> userSecurityPictureRepository,
        IRepository<SecurityPicture> securityPictureRepository,
        IAuditLogService auditLogService)
    {
        _userSecurityPictureRepository = userSecurityPictureRepository;
        _securityPictureRepository = securityPictureRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateUserSecurityPictureCommand command, CancellationToken cancellationToken)
    {
        var securityPicture = await _securityPictureRepository.GetAll()
            .FirstOrDefaultAsync(x => x.Id == command.SecurityPictureId && x.RecordStatus == RecordStatus.Active, cancellationToken);

        if (securityPicture is null)
            throw new NotFoundException(nameof(SecurityPicture));

        var existing = await _userSecurityPictureRepository.GetAll()
            .SingleOrDefaultAsync(x => x.UserId == command.UserId && x.RecordStatus == RecordStatus.Active, cancellationToken);

        if (existing is not null)
        {
            existing.SecurityPictureId = command.SecurityPictureId;
            await _userSecurityPictureRepository.UpdateAsync(existing);

            await _auditLogService.AuditLogAsync(new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateUserSecurityPicture",
                SourceApplication = "Identity",
                Resource = "UserSecurityPicture",
                UserId = command.UserId,
                Details = new Dictionary<string, string>
                {
                    { "UserId", command.UserId.ToString() },
                    { "SecurityPictureId", command.SecurityPictureId.ToString() }
                }
            });
        }
        else
        {
            var userSecurityPicture = new UserSecurityPicture
            {
                UserId = command.UserId,
                SecurityPictureId = command.SecurityPictureId
            };

            await _userSecurityPictureRepository.AddAsync(userSecurityPicture);

            await _auditLogService.AuditLogAsync(new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateUserSecurityPicture",
                SourceApplication = "Identity",
                Resource = "UserSecurityPicture",
                UserId = command.UserId,
                Details = new Dictionary<string, string>
                {
                    { "UserId", command.UserId.ToString() }
                }
            });
        }

        return Unit.Value;
    }
}
