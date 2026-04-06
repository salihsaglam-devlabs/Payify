using LinkPara.Audit.Models;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.Identity.Application.Common.Interfaces;
using MediatR;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using LinkPara.Audit;
using LinkPara.ContextProvider;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.DeleteDocument;

public class DeleteDocumentCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteDocumentCommandHandler : IRequestHandler<DeleteDocumentCommand>
{
    private readonly IRepository<AgreementDocument> _agreementDocumentRepository;
    private readonly IAgreementDocumentService _agreementDocumentService;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public DeleteDocumentCommandHandler(IRepository<AgreementDocument> agreementDocumentRepository,
        IAgreementDocumentService agreementDocumentService,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _agreementDocumentRepository = agreementDocumentRepository;
        _agreementDocumentService = agreementDocumentService;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider; 
    }

    public async Task<Unit> Handle(DeleteDocumentCommand command, CancellationToken cancellationToken)
    {
        var entity = await _agreementDocumentRepository.GetAll()
       .Include(x => x.Agreements)
       .Where(x => x.Id == command.Id)
       .FirstOrDefaultAsync();

        if (entity is null)
        {
            throw new NotFoundException(nameof(AgreementDocument), command.Id);
        }
        entity.RecordStatus = RecordStatus.Passive;

        entity.Agreements.ForEach(e =>
        {
            e.RecordStatus = RecordStatus.Passive;
            e.IsLatest = false;
        });

        await _agreementDocumentService.UpdateAgreementDocument(entity);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = false,
                LogDate = DateTime.Now,
                Operation = "DeleteAgreementDocument",
                SourceApplication = "Identity",
                Resource = "AgreementDocument",
                UserId = parseUserId,
                Details = new Dictionary<string, string>
                {
                         {"Version", entity.LastVersion },
                         {"Name", entity.Name }
                }
            });

        return Unit.Value;
    }
}
