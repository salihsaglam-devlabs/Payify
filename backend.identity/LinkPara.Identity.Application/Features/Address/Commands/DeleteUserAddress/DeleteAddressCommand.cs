using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using static MassTransit.ValidationResultExtensions;

namespace LinkPara.Identity.Application.Features.Address.Commands.DeleteUserAddress;
public class DeleteAddressCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand>
{
    private readonly IRepository<UserAddress> _repository;
    private readonly IAuditLogService _auditLogService;

    public DeleteAddressCommandHandler(IRepository<UserAddress> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(DeleteAddressCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(UserAddress), command.Id);
        }

        await _repository.DeleteAsync(entity);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DeleteUserAddress",
            SourceApplication = "Identity",
            Resource = "UserAddress",
            Details = new Dictionary<string, string>
            {
                {"Id", entity.Id.ToString() },
                {"CountryId", entity.CountryId.ToString() }
            }
        });

        return Unit.Value;
    }

}