using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Identity.Application.Features.Address.Commands.UpdateUserAddress;

public class UpdateAddressCommand : IRequest
{
    public Guid Id { get; set; }
    public int CountryId { get; set; }
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Neighbourhood { get; set; }
    public string Street { get; set; }
    public string FullAddress { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand>
{
    private readonly IRepository<UserAddress> _repository;
    private readonly IAuditLogService _auditLogService;

    public UpdateAddressCommandHandler(IRepository<UserAddress> repository,
        IAuditLogService auditLogService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(UpdateAddressCommand command, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(command.Id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(UserAddress), command.Id);
        }

        if (entity.UserId != command.UserId)
        {
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = false,
                    LogDate = DateTime.Now,
                    Operation = "UpdateUserAddress",
                    SourceApplication = "Identity",
                    Resource = "UserAddress",
                    Details = new Dictionary<string, string>
                    {
                          {"Id", entity.Id.ToString() },
                          {"CommandUserId", command.UserId.ToString() },
                          {"Message", "UnAuthorized"}
                    }
                });

            throw new UnauthorizedAccessException();
        }

        entity.Street = command.Street;
        entity.Neighbourhood = command.Neighbourhood;
        entity.FullAddress = command.FullAddress;
        entity.DistrictId = command.DistrictId;
        entity.CountryId = command.CountryId;
        entity.CityId = command.CityId;

        await _repository.UpdateAsync(entity);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "UpdateUserAddress",
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