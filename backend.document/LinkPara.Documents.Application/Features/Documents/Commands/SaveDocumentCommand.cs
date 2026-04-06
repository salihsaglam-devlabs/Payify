using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Documents.Application.Commons.Exceptions;
using LinkPara.Documents.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LinkPara.Documents.Application.Features.Documents.Commands;
public class SaveDocumentCommand : IRequest<DocumentResponse>
{
    public DocumentDto Document { get; set; }
}

public class SaveDocumentCommandHandler : IRequestHandler<SaveDocumentCommand, DocumentResponse>
{
    private readonly IGenericRepository<Document> _documentsRepository;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<SaveDocumentCommandHandler> _logger;

    public SaveDocumentCommandHandler(IGenericRepository<Document> documentsRepository,
        IConfiguration configuration,
        IMapper mapper,
        IAuditLogService auditLogService,
        ILogger<SaveDocumentCommandHandler> logger)
    {
        _documentsRepository = documentsRepository;
        _configuration = configuration;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<DocumentResponse> Handle(SaveDocumentCommand command, CancellationToken cancellationToken)
    {
        if (!_configuration["AcceptedDocumentTypes"].Split(',').Any(x => command.Document.ContentType.Contains(x)))
        {
            throw new InvalidFileTypeException();
        }

        var document = new Document()
        {
            ContentType = command.Document.ContentType,
            OriginalFileName = command.Document.OriginalFileName,
            DocumentTypeId = command.Document.DocumentTypeId,
            MerchantId = command.Document.MerchantId,
            SubMerchantId = command.Document.SubMerchantId,
            UserId = command.Document.UserId,
            AccountId = command.Document.AccountId,
            RecordStatus = SharedModels.Persistence.RecordStatus.Active
        };

        try
        {
            await using var memoryStream = new MemoryStream(command.Document.Bytes);

            var fileInfo = new FileInfo(command.Document.OriginalFileName);
            var date = DateTimeOffset.Now;

            document.FilePath =
                @$"{_configuration["StoragePath"]}/{date.Year}/{date.Month}/{date.Day}/{document.Id}{fileInfo.Extension}";

            Directory.CreateDirectory(Path.GetDirectoryName(document.FilePath));

            await using var fileStream = new FileStream(document.FilePath, FileMode.Create, FileAccess.Write);
            memoryStream.WriteTo(fileStream);

            await _documentsRepository.AddAsync(document);
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
            Operation = "SaveDocument",
            SourceApplication = "Document",
            Resource = "Document",
            Details = new Dictionary<string, string>
            {
                {"Id", document.Id.ToString() },
                {"UserId", command.Document.UserId.ToString() },
                {"DocumentTypeId", command.Document.DocumentTypeId.ToString() },
            }
        });

        return _mapper.Map<DocumentResponse>(document);
    }
}