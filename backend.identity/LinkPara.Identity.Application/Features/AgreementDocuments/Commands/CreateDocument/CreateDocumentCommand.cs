using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using MediatR;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Audit;
using LinkPara.Identity.Domain.Enums;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocument;

public class CreateDocumentCommand : IRequest
{
    public RecordStatus RecordStatus { get; set; }
    public string Name { get; set; }
    public List<DocumentVersionDto> Agreements { get; set; }
    public ProductType ProductType { get; set; }
}

public class CreateDocumentCommandHandler : IRequestHandler<CreateDocumentCommand>
{
    private readonly IRepository<AgreementDocument> _repository;
    private readonly IRepository<AgreementDocumentVersion> _documentVersionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public CreateDocumentCommandHandler(IRepository<AgreementDocument> repository,
        IRepository<AgreementDocumentVersion> documentVersionRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider
        )
    {
        _documentVersionRepository = documentVersionRepository;
        _repository = repository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(CreateDocumentCommand command, CancellationToken cancellationToken)
    {
        var agreementDocument = new AgreementDocument
        {
            Name = command.Name,
            LastVersion = "v.1",
            LanguageCode = command.Agreements?.FirstOrDefault()?.LanguageCode,
            RecordStatus = command.RecordStatus,
            ProductType = command.ProductType
        };

        await _repository.AddAsync(agreementDocument);

        foreach (var item in command.Agreements)
        {
            var agreementDocumentVersion = new AgreementDocumentVersion
            {
                Content = item.Content,
                RecordStatus = command.RecordStatus,
                AgreementDocumentId = agreementDocument.Id,
                Title=item.Title,
                LanguageCode=item.LanguageCode,
                IsLatest = true,
                Version = "v.1",
                IsOptional = item.IsOptional
            };

            await _documentVersionRepository.AddAsync(agreementDocumentVersion);
        }

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateDocument",
                SourceApplication = "Identity",
                Resource = "Agreement",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                      {"Version", "v1.0" },
                      {"Name", command.Name }
                }
            });

        return Unit.Value;
    }
}

