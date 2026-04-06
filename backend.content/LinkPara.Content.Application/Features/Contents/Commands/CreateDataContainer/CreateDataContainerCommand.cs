using System.Text.Json;
using LinkPara.Content.Domain.Entities;
using MediatR;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Content.Application.Features.Contents.Commands.CreateDataContainer;

public class CreateDataContainerCommand : IRequest
{
    public string Key { get; set; }
    public JsonElement Value { get; set; }
}


public class CreateDataContainerCommandHandler : IRequestHandler<CreateDataContainerCommand>
{
    private readonly IGenericRepository<DataContainer> _dataContainerRepository;
    private readonly IAuditLogService _auditLogService;

    public CreateDataContainerCommandHandler(IGenericRepository<DataContainer> dataContainerRepository,
        IAuditLogService auditLogService)
    {
        _dataContainerRepository = dataContainerRepository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(CreateDataContainerCommand request, CancellationToken cancellationToken)
    {
        if (await IsExistAsync(request))
            throw new DuplicateRecordException();

        await _dataContainerRepository.AddAsync(new()
        {
            Key = request.Key,
            Value = request.Value.ToString(),
        });

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateDataContainer",
                SourceApplication = "Content",
                Resource = "DataContainer",
                Details = new Dictionary<string, string>
                {
                     {"Key", request.Key },
                     {"Value", request.Value.ToString() }
                }
            });

        return Unit.Value;
    }

    private async Task<bool> IsExistAsync(CreateDataContainerCommand request)
    {
        return await _dataContainerRepository.GetAll().AnyAsync(x =>
            x.Key == request.Key
        );
    }


}
