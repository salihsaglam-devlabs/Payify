using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveWalletAccount;

public class SaveWalletAccountCommand : IRequest
{
    public Guid UserId { get; set; }
    public string WalletNumber { get; set; }
    public string Tag { get; set; }
    public string ReceiverName { get; set; }
}

public class SaveWalletAccountCommandHandler : IRequestHandler<SaveWalletAccountCommand>
{
    private readonly IGenericRepository<SavedWalletAccount> _savedWalletRepository;
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public SaveWalletAccountCommandHandler(IGenericRepository<Wallet> wallet,
        IGenericRepository<SavedWalletAccount> savedWalletRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _walletRepository = wallet;
        _savedWalletRepository = savedWalletRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(SaveWalletAccountCommand request, CancellationToken cancellationToken)
    {
        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        var duplicateWallet = _savedWalletRepository.GetAll()
            .SingleOrDefault(x =>
                x.UserId == request.UserId &&
                x.WalletNumber == request.WalletNumber &&
                x.RecordStatus == RecordStatus.Active);

        if (duplicateWallet is not null)
        {
            await SaveWalletAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"WalletNumber",request.WalletNumber.ToString() },
                {"Error","DuplicateWalletNumber"},
                {"Tag",request.Tag },
                {"ReceiverName",request.ReceiverName }
            });

            throw new DuplicateRecordException();
        }

        var wallet = _walletRepository.GetAll()
            .FirstOrDefault(x => x.WalletNumber == request.WalletNumber);

        if (wallet is null)
        {
            await SaveWalletAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"WalletNumber",request.WalletNumber.ToString() },
                {"Error","Wallet Is Not Found"},
                {"Tag",request.Tag },
                {"ReceiverName",request.ReceiverName }
            });
            throw new NotFoundException(nameof(Wallet), request.WalletNumber);
        }

        var savedWalletAccount = new SavedWalletAccount
        {
            CreateDate = DateTime.Now,
            CreatedBy = request.UserId.ToString(),
            UserId = request.UserId,
            Tag = request.Tag,
            ReceiverName = request.ReceiverName,
            WalletNumber = request.WalletNumber,
            WalletOwnerAccountId = wallet.AccountId
        };

        await _savedWalletRepository.AddAsync(savedWalletAccount);

        await SaveWalletAccountAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            {"WalletOwnerUserId",Guid.Empty.ToString() },
            {"WalletNumber",wallet.WalletNumber },
            {"Tag",request.Tag },
            {"ReceiverName",request.ReceiverName }
        });

        return Unit.Value;
    }

    private async Task SaveWalletAccountAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "SaveWalletAccount",
                Resource = "SavedAccount",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }
}