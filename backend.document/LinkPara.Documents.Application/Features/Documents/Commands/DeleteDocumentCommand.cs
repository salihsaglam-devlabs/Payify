using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Documents.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Logging;
using System;

namespace LinkPara.Documents.Application.Features.Documents.Commands;

public class DeleteDocumentCommand : IRequest
{
    public Guid Id { get; set; }
}


public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IGenericRepository<Document> _documentRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SaveDocumentCommandHandler> _logger;

    public DeleteDocumentCommandHandler(IGenericRepository<Document> documentRepository,
        IAuditLogService auditLogService,
        ILogger<SaveDocumentCommandHandler> logger)
    {
        _documentRepository = documentRepository;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Unit> Handle(DeleteDocumentCommand request, CancellationToken cancellationToken)
    {

        var document = await _documentRepository.GetByIdAsync(request.Id);

        try
        {

            var fileName = Path.GetFileName(document.FilePath);
            var deletedFilePath = document.FilePath.Replace(fileName, $"deleted_{fileName}");

            File.Move(document.FilePath, deletedFilePath);

            document.FilePath = deletedFilePath;

            await _documentRepository.DeleteAsync(document);
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveDocumentError : {exception}");
            throw;
        }
        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DeleteDocument",
            SourceApplication = "Document",
            Resource = "Document",
            Details = new Dictionary<string, string>
            {
                {"Id", document.Id.ToString() },
            }
        });

        return Unit.Value;
    }
}
