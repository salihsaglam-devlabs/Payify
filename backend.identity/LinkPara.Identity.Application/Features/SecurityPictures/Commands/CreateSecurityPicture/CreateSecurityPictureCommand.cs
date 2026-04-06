using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.SecurityPictures.Commands.CreateSecurityPicture;

public class CreateSecurityPictureCommand : IRequest, IMapFrom<SecurityPicture>
{
    public string Name { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
}

public class CreateSecurityPictureCommandHandler : IRequestHandler<CreateSecurityPictureCommand>
{
    private readonly IRepository<SecurityPicture> _repository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;

    public CreateSecurityPictureCommandHandler(
        IRepository<SecurityPicture> repository,
        IAuditLogService auditLogService,
        IMapper mapper,
        IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _applicationUserService = applicationUserService;
    }

    public async Task<Unit> Handle(CreateSecurityPictureCommand command, CancellationToken cancellationToken)
    {
        var securityPicture = _mapper.Map<SecurityPicture>(command);

        if (await _repository.GetAll().AnyAsync(x =>
                x.Name == command.Name &&
                x.RecordStatus == RecordStatus.Active, cancellationToken))
        {
            throw new DuplicateRecordException();
        }

        await _repository.AddAsync(securityPicture);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateSecurityPicture",
                SourceApplication = "Identity",
                Resource = "SecurityPicture",
                UserId = _applicationUserService.ApplicationUserId,
                Details = new Dictionary<string, string>
                {
                    { "Name", command.Name },
                    { "ContentType", command.ContentType }
                }
            });

        return Unit.Value;
    }
}
