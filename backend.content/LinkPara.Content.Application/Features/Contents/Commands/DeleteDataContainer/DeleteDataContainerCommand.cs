using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Content.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Content.Application.Features.Contents.Commands.DeleteDataContainer;

public class DeleteDataContainerCommand : IRequest
{
    public string Key { get; set; }
}


public class UpdateDataContainerCommandHandler : IRequestHandler<DeleteDataContainerCommand>
{
    private readonly IGenericRepository<DataContainer> _dataContainerRepository;
    private readonly IAuditLogService _auditLogService;

    public UpdateDataContainerCommandHandler(IGenericRepository<DataContainer> dataContainerRepository,
        IAuditLogService auditLogService)
    {
        _dataContainerRepository = dataContainerRepository;
        _auditLogService = auditLogService;
    }

    public async Task<Unit> Handle(DeleteDataContainerCommand request, CancellationToken cancellationToken)
    {
        var dataContainer = _dataContainerRepository.GetAll().SingleOrDefault(x => x.Key == request.Key);

        await _dataContainerRepository.DeleteAsync(dataContainer);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DeleteDataContainer",
            SourceApplication = "Content",
            Resource = "DataContainer",
            Details = new Dictionary<string, string>
            {
                {"Key", request.Key },
                {"Value", dataContainer.Value.ToString() }
            }
        });

        return Unit.Value;
    }
}
