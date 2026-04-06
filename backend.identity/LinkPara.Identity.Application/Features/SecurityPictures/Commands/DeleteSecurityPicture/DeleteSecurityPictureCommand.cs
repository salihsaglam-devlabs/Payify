using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.SecurityPictures.Commands.DeleteSecurityPicture;

public class DeleteSecurityPictureCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteSecurityPictureCommandHandler : IRequestHandler<DeleteSecurityPictureCommand>
{
    private readonly IRepository<SecurityPicture> _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IApplicationUserService _applicationUserService;

    public DeleteSecurityPictureCommandHandler(
        IRepository<SecurityPicture> repository,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _applicationUserService = applicationUserService;
    }

    public async Task<Unit> Handle(DeleteSecurityPictureCommand command, CancellationToken cancellationToken)
    {
        var securityPicture = await _repository.GetAll()
            .FirstOrDefaultAsync(x => x.Id == command.Id && x.RecordStatus == RecordStatus.Active, cancellationToken);

        if (securityPicture is null)
            throw new NotFoundException(nameof(SecurityPicture));

        securityPicture.RecordStatus = RecordStatus.Passive;
        await _repository.UpdateAsync(securityPicture);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "DeleteSecurityPicture",
                SourceApplication = "Identity",
                Resource = "SecurityPicture",
                UserId = _applicationUserService.ApplicationUserId,
                Details = new Dictionary<string, string>
                {
                    { "Id", command.Id.ToString() }
                }
            });

        return Unit.Value;
    }
}
