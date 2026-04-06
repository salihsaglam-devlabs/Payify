using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.DeleteSavedAccount;

public class DeleteSavedAccountCommand : IRequest
{
    public Guid UserId { get; set; }
    public Guid Id { get; set; }
}

public class DeleteBankAccountCommandHandler : IRequestHandler<DeleteSavedAccountCommand>
{
    private readonly IGenericRepository<SavedAccount> _savedAccountRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public DeleteBankAccountCommandHandler(IGenericRepository<SavedAccount> savedAccountRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _savedAccountRepository = savedAccountRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(DeleteSavedAccountCommand request, CancellationToken cancellationToken)
    {
        var bankAccount = await _savedAccountRepository.GetByIdAsync(request.Id);

        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser || bankAccount.UserId != loggedUser)
        {
            await DeleteSavedAccountAuditLogAsync(true, request.UserId, new Dictionary<string, string>
            {
                {"SavedAccountId", bankAccount.Id.ToString()},
                {"SavedAccountUserId", bankAccount.UserId.ToString()},
                {"RequestedUserId", request.UserId.ToString()},
                {"Error", "UnAuthorizedAccess"}

            });

            throw new ForbiddenAccessException();
        }

        await _savedAccountRepository.DeleteAsync(bankAccount);

        await DeleteSavedAccountAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            {"SavedAccountId", bankAccount.Id.ToString()}
        });

        return Unit.Value;
    }

    private async Task DeleteSavedAccountAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = details,
                LogDate = DateTime.Now,
                Operation = "DeleteSavedAccount",
                Resource = "SavedAccount",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
}