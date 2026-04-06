using LinkPara.Documents.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.SharedModels.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Audit.Models;
using System.Reflection.Metadata;
using LinkPara.Audit;

namespace LinkPara.Documents.Application.Features.DocumentTypes.Commands;
public class CreateDocumentTypeCommand : IRequest
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class CreateDocumentTypeCommandHandler : IRequestHandler<CreateDocumentTypeCommand>
{
    private readonly IGenericRepository<DocumentType> _documentTypesRepository;
    private readonly IAuditLogService _auditLogService;

    public CreateDocumentTypeCommandHandler(IGenericRepository<DocumentType> documentTypesRepository,
                                            IAuditLogService auditLogService)
    {
        _documentTypesRepository = documentTypesRepository;
        _auditLogService = auditLogService; 
    }

    public async Task<Unit> Handle(CreateDocumentTypeCommand command, CancellationToken cancellationToken)
    {
        var activeDocumentType = await _documentTypesRepository.GetAll()
            .FirstOrDefaultAsync(b=>b.Name.Equals(command.Name) 
            && b.RecordStatus == RecordStatus.Active);

        if (activeDocumentType is not null)
        {
            throw new DuplicateRecordException(nameof(DocumentType), command.Name);
        }

        await _documentTypesRepository.AddAsync(new()
        {
            Name = command.Name,
            Description = command.Description
        });

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateDocumentType",
            SourceApplication = "Document",
            Resource = "DocumentType",
            Details = new Dictionary<string, string>
            {
                {"Name", command.Name },
                {"Description", command.Description }
            }
        });

        return Unit.Value;
    }
}