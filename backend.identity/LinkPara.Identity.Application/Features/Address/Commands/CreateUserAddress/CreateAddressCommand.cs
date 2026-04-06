using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static MassTransit.ValidationResultExtensions;

namespace LinkPara.Identity.Application.Features.Address.Commands.CreateUserAddress;

public class CreateAddressCommand : IRequest
{
    public Guid UserId { get; set; }
    public int CountryId { get; set; }
    public int CityId { get; set; }
    public int DistrictId { get; set; }
    public string Neighbourhood { get; set; }
    public string Street { get; set; }
    public string FullAddress { get; set; }
}

public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserAddress> _repository;
    private readonly IAuditLogService _auditLogService;

    public CreateAddressCommandHandler(UserManager<User> userManager,
        IRepository<UserAddress> repository,
        IAuditLogService auditLogService)
    {
        _userManager = userManager;
        _repository = repository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(CreateAddressCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        var userAddress = new UserAddress
        {
            CountryId = command.CountryId,
            CityId = command.CityId,
            DistrictId = command.DistrictId,
            Neighbourhood = command.Neighbourhood,
            Street = command.Street,
            FullAddress = command.FullAddress,
            UserId = command.UserId,
            RecordStatus = RecordStatus.Active
        };

        if (await IsExistAsync(userAddress))
            throw new DuplicateRecordException();

        await _repository.AddAsync(userAddress);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateUserAddress",
            SourceApplication = "Identity",
            Resource = "UserAddress",
            UserId = user.Id,
            Details = new Dictionary<string, string>
            {
                {"Id", userAddress.Id.ToString() },
                {"CountryId", userAddress.CountryId.ToString() }
            }
        });

        return Unit.Value;
    }

    private async Task<bool> IsExistAsync(UserAddress userAddress)
    {
        return await _repository.GetAll().AnyAsync(x =>
            x.CountryId == userAddress.CountryId &&
            x.CityId == userAddress.CityId &&
            x.DistrictId == userAddress.DistrictId &&
            x.Neighbourhood == userAddress.Neighbourhood &&
            x.Street == userAddress.Street &&
            x.FullAddress == userAddress.FullAddress &&
            x.UserId == userAddress.UserId &&
            x.RecordStatus == RecordStatus.Active
        );
    }
}