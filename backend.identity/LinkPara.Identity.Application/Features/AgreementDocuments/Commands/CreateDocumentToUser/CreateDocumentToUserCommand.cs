using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocumentToUser;

public class CreateDocumentToUserCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid AgreementDocumentId { get; set; }
}

public class CreateDocumentToUserCommandHandler : IRequestHandler<CreateDocumentToUserCommand>
{
    private readonly UserManager<User> _userManager;
    private readonly IRepository<UserAgreementDocument> _userAgreementRepository;
    private readonly IRepository<AgreementDocumentVersion> _documentVersionRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public CreateDocumentToUserCommandHandler(
        UserManager<User> userManager,
        IRepository<UserAgreementDocument> userAgreementRepository,
        IRepository<AgreementDocumentVersion> documentVersionRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _userManager = userManager;
        _userAgreementRepository = userAgreementRepository;
        _documentVersionRepository = documentVersionRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(CreateDocumentToUserCommand command, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(command.UserId.ToString());

        if (user is null)
        {
            throw new NotFoundException(nameof(User), command.UserId);
        }

        var languageCode = string.IsNullOrEmpty(_contextProvider.CurrentContext.Language)
                            ? "tr"
                            : _contextProvider.CurrentContext.Language.Substring(0, 2);

        var documentVersion = await _documentVersionRepository.GetAll()
          .Include(x => x.AgreementDocument)
          .FirstOrDefaultAsync(q => q.AgreementDocumentId == command.AgreementDocumentId
           && q.IsLatest && q.LanguageCode == languageCode);

        if (documentVersion is null)
        {
            throw new NotFoundException(nameof(AgreementDocumentVersion), command.AgreementDocumentId);
        }

        var isAlreadyAssigned = _userAgreementRepository.GetAll()
            .Include(q => q.AgreementDocumentVersion)
            .Include(q => q.AgreementDocumentVersion.AgreementDocument)
            .Any(q => q.UserId == command.UserId && q.AgreementDocumentVersionId == documentVersion.Id);

        if (isAlreadyAssigned)
        {
            throw new DuplicateRecordException(nameof(AgreementDocument));
        }

        var userAgreementDocument = new UserAgreementDocument
        {
            UserId = user.Id,
            AgreementDocumentVersionId = documentVersion.Id,
            ApprovalChannel = _contextProvider.CurrentContext.Channel,
        };

        await _userAgreementRepository.AddAsync(userAgreementDocument);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateDocumentToUser",
            SourceApplication = "Identity",
            Resource = "UserAgreementDocument",
            Details = new Dictionary<string, string>
            {
                     {"Id", userAgreementDocument.Id.ToString() },
                     {"AgreementDocumentVersionId", userAgreementDocument.AgreementDocumentVersionId.ToString() }
            }
        });
        return Unit.Value;
    }
}

