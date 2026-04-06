using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Persistence;
using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Enums;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.UpdateDocument;

public class UpdateAgreementDocumentCommand : IRequest, IMapFrom<AgreementDocument>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public ProductType ProductType { get; set; }
    public List<DocumentVersionDto> Agreements { get; set; }
}

public class UpdateDocumentCommandHandler : IRequestHandler<UpdateAgreementDocumentCommand>
{
    private readonly IRepository<AgreementDocument> _agreementDocumentRepository;
    private readonly IRepository<AgreementDocumentVersion> _docVersionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IAgreementDocumentService _agreementDocumentService;
    private readonly ILogger<UpdateDocumentCommandHandler> _logger;

    public UpdateDocumentCommandHandler(IRepository<AgreementDocument> agreementDocumentRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IRepository<AgreementDocumentVersion> docVersionRepository,
        IAgreementDocumentService agreementDocumentService,
        ILogger<UpdateDocumentCommandHandler> logger)
    {
        _agreementDocumentRepository = agreementDocumentRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _docVersionRepository = docVersionRepository;
        _agreementDocumentService = agreementDocumentService;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateAgreementDocumentCommand request, CancellationToken cancellationToken)
    {
        var entity = await _agreementDocumentRepository.GetAll()
            .Include(x => x.Agreements)
            .Where(x => x.Id == request.Id)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            throw new NotFoundException(nameof(AgreementDocument), request.Id);
        }

        try
        {
            var version = Convert.ToInt32(entity.LastVersion.Substring(2)) + 1;
            string displayableVersion = $"v.{version}";

            entity.Name = request.Name;
            entity.LastVersion = displayableVersion;
            entity.ProductType = request.ProductType;

            entity.Agreements.ForEach(e =>
            {
                e.RecordStatus = RecordStatus.Passive;
                e.IsLatest = false;
            });

            await _agreementDocumentService.UpdateAgreementDocument(entity);

            foreach (var item in request.Agreements)
            {
                var agreementDocumentVersion = new AgreementDocumentVersion
                {
                    Content = item.Content,
                    AgreementDocumentId = entity.Id,
                    Title = item.Title,
                    LanguageCode = item.LanguageCode,
                    IsLatest = true,
                    RecordStatus = request.RecordStatus,
                    Version = displayableVersion,
                    IsOptional = item.IsOptional
                };

                await _docVersionRepository.AddAsync(agreementDocumentVersion);
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateDocument",
                    SourceApplication = "Identity",
                    Resource = "AgreementDocument",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                     {
                        {"Id", entity.Id.ToString()},
                        {"Name", entity.Name },
                     }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"AgreementDocumentPatchError : {exception}");
            throw;
        }

        return Unit.Value;
    }

}


