using MediatR;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.ContextProvider;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateWalletAccount;

public class UpdateWalletAccountCommand : IRequest
{
    public string Tag { get; set; }
    public string WalletNumber { get; set; }
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
}

public class UpdateWalletAccountCommandHandler : IRequestHandler<UpdateWalletAccountCommand>
{
    private readonly IGenericRepository<SavedWalletAccount> _savedWalletAccountRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public UpdateWalletAccountCommandHandler(IGenericRepository<SavedWalletAccount> savedWalletAccountRepository,
        IAuditLogService auditLogService,
        IGenericRepository<Wallet> wallet,
              IContextProvider contextProvider)
    {
        _savedWalletAccountRepository = savedWalletAccountRepository;
        _auditLogService = auditLogService;
        _walletRepository = wallet;
        _contextProvider = contextProvider;
    }
    public async Task<Unit> Handle(UpdateWalletAccountCommand request, CancellationToken cancellationToken)
    {
        var walletAccount = await _savedWalletAccountRepository.GetByIdAsync(request.Id);

        if (walletAccount is null)
        {
            throw new NotFoundException(nameof(SavedWalletAccount), request.Id);
        }

        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser || walletAccount.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        var wallet = _walletRepository.GetAll().FirstOrDefault(x => x.WalletNumber == request.WalletNumber);

        if (wallet is null)
        {
            await UpdateSavedWalletAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"WalletNumber",request.WalletNumber.ToString() },
                {"Error","Wallet Is Not Found"},
                {"Tag",request.Tag }
            });
            throw new NotFoundException(nameof(Wallet), request.WalletNumber);
        }

        var duplicateWallet = _savedWalletAccountRepository.GetAll()
         .SingleOrDefault(x => x.UserId == request.UserId &&
                               x.WalletNumber == request.WalletNumber &&
                               x.Id != request.Id &&
                               x.RecordStatus == RecordStatus.Active);

        if (duplicateWallet is not null)
        {
            await UpdateSavedWalletAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"WalletNumber",request.WalletNumber.ToString() },
                {"Error","DuplicateWalletNumber"},
                {"Tag",request.Tag }
            });

            throw new DuplicateRecordException();
        }

        if (walletAccount.UserId != request.UserId)
        {
            await UpdateSavedWalletAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"AccountUserId", walletAccount.UserId.ToString()},
                {"RequestedUserId", request.UserId.ToString()},
                {"SavedAccountId", request.Id.ToString()},
                {"Error", "UnAuthorizedAccess"}
            });
            throw new UnauthorizedAccessException();
        }

        walletAccount.Tag = request.Tag;
        walletAccount.WalletNumber = request.WalletNumber;
        walletAccount.WalletOwnerAccountId = wallet.AccountId;

        await _savedWalletAccountRepository.UpdateAsync(walletAccount);

        await UpdateSavedWalletAccountAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            {"UpdateAccountId", walletAccount.Id.ToString()},
            {"Tag", request.Tag}
        });

        return Unit.Value;
    }

    private async Task UpdateSavedWalletAccountAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "UpdateSavedWalletAccount",
                Resource = "SavedAccount",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
}