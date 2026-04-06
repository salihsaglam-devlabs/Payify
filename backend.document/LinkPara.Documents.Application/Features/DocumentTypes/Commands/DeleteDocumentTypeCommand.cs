using LinkPara.Documents.Domain.Entities;
using MediatR;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Documents.Application.Features.DocumentTypes.Commands;
public class DeleteDocumentTypeCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteDocumentTypeCommandHandler : IRequestHandler<DeleteDocumentTypeCommand>
{
    private readonly IGenericRepository<DocumentType> _documentTypesRepository;
    private readonly IAuditLogService _auditLogService;

    public DeleteDocumentTypeCommandHandler(IGenericRepository<DocumentType> documentTypesRepository,
                                            IAuditLogService auditLogService)
    {
        _documentTypesRepository = documentTypesRepository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(DeleteDocumentTypeCommand command, CancellationToken cancellationToken)
    {
        var type = await _documentTypesRepository.GetByIdAsync(command.Id);
        await _documentTypesRepository.DeleteAsync(type);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DeleteDocumentType",
            SourceApplication = "Document",
            Resource = "DocumentType",
            Details = new Dictionary<string, string>
            {
                {"Id", command.Id.ToString() }
            }
        });

        return Unit.Value;
    }
}