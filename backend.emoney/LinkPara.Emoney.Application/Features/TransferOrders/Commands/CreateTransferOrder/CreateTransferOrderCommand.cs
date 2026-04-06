using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.Identity.Models.Enums;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Commands.CreateTransferOrder;

public class CreateTransferOrderCommand : IRequest
{
    public Guid UserId { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType SenderUserType { get; set; }
    public ReceiverAccountType ReceiverAccountType { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string ReceiverPhoneCode { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string PaymentType { get; set; }
}

public class CreateTransferOrderCommandHandler : IRequestHandler<CreateTransferOrderCommand>
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<TransferOrder> _transferOrderRepository;
    private readonly IUserService _userService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Account> _accountRepository;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<CreateTransferOrderCommand> _logger;

    public CreateTransferOrderCommandHandler(
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<TransferOrder> transferOrderRepository,
        IUserService userService,
        IAuditLogService auditLogService,
        IMoneyTransferService moneyTransferService,
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Account> accountRepository,
        IContextProvider contextProvider,
        ILogger<CreateTransferOrderCommand> logger)
    {
        _walletRepository = walletRepository;
        _transferOrderRepository = transferOrderRepository;
        _userService = userService;
        _auditLogService = auditLogService;
        _moneyTransferService = moneyTransferService;
        _accountUserRepository = accountUserRepository;
        _accountRepository = accountRepository;
        _contextProvider = contextProvider;
        _logger = logger;
    }

    public async Task<Unit> Handle(CreateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var senderWallet = await CheckWalletNumberAsync(request.SenderWalletNumber, cancellationToken);

        if (CheckSenderOwnWalletNumberAsync(request.SenderWalletNumber))
        {
            throw new ForbiddenAccessException();
        }

        string receiverWalletNumber = null;

        if (request.ReceiverAccountType is not ReceiverAccountType.Iban)
        {
            receiverWalletNumber = request.ReceiverAccountType switch
            {
                ReceiverAccountType.PhoneNumber => await GetReceiverWalletByPhoneAsync(request.ReceiverPhoneCode, request.ReceiverAccountValue, cancellationToken),
                _ => request.ReceiverAccountValue
            };

            var wallet = await CheckWalletNumberAsync(receiverWalletNumber, cancellationToken);

            request.ReceiverNameSurname = await GetAccountNameByIdAsync(wallet.AccountId);
        }
        else
        {
            var ibanValidationResult = await _moneyTransferService.CheckIbanAsync(request.ReceiverAccountValue);

            if (!ibanValidationResult.IsValidIban)
            {
                throw new InvalidIbanException();
            }
        }

        var newTransferOrder = new TransferOrder
        {
            UserId = request.UserId,
            SenderWalletNumber = request.SenderWalletNumber,
            SenderNameSurname = request.SenderNameSurname,
            SenderUserType = request.SenderUserType,
            TransferDate = request.TransferDate,
            Amount = request.Amount,
            ReceiverAccountValue = request.ReceiverAccountValue,
            ReceiverAccountType = request.ReceiverAccountType,
            ReceiverNameSurname = request.ReceiverNameSurname,
            Description = request.Description,
            CurrencyCode = senderWallet.CurrencyCode,
            TransferOrderStatus = TransferOrderStatus.Pending,
            ReceiverPhoneCode = request?.ReceiverPhoneCode,
            ReceiverWalletNumber = receiverWalletNumber,
            RecordStatus = RecordStatus.Active,
            CreatedBy = request.UserId.ToString(),
            PaymentType = request.PaymentType
        };

        await _transferOrderRepository.AddAsync(newTransferOrder);

        await TransferOrderAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            {"TransferOrderId",newTransferOrder.Id.ToString() },
            {"SenderWalletNumber",newTransferOrder.SenderWalletNumber },
            {"ReceiverNameSurname",newTransferOrder.ReceiverNameSurname }
        });

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "CreateTransferOrder",
                SourceApplication = "Emoney",
                Resource = "TransferOrder",
                Details = new Dictionary<string, string>
                {
                        {"Id", newTransferOrder.Id.ToString() },
                        {"SenderWalletNumber", newTransferOrder.SenderWalletNumber },
                        {"ReceiverWalletNumber", newTransferOrder.ReceiverWalletNumber },
                }
            });

        return Unit.Value;
    }

    private bool CheckSenderOwnWalletNumberAsync(string senderWalletNumber)
    {
        var userId = GetUserId();
        var wallet = _walletRepository.GetAll()
                                .Include(q => q.Account)
                                .ThenInclude(q => q.AccountUsers)
                                .FirstOrDefault(q => q.WalletNumber == senderWalletNumber && q.RecordStatus == RecordStatus.Active);

        NullControlHelper.CheckAndThrowIfNull(wallet, senderWalletNumber, _logger);

        return wallet.Account.AccountUsers.Any(q => q.UserId != userId);
    }

    private async Task TransferOrderAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "CreateTransferOrder",
                Resource = "TransferOrder",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }

    private async Task<string> GetReceiverWalletByPhoneAsync(string phoneCode, string receiverPhoneNumber, CancellationToken cancellationToken)
    {
        UserDto user = new();
        var userName = await GetUserNameHelper.GetUserNameAsync(phoneCode, receiverPhoneNumber);

        var users = await _userService.GetAllUsersAsync(new GetUsersRequest { UserName = userName, RecordStatus = RecordStatus.Active});
        
        if (users.Items.Count != 1)
        {
            throw new NotFoundException(nameof(Wallet));
        }
        else if (users.Items.Count == 0)
        {
            throw new NotFoundException(receiverPhoneNumber);
        }
        else
        {
            user = users.Items[0];
        }

        var accountUser = await _accountUserRepository.GetAll()
            .FirstOrDefaultAsync(s => s.UserId == user.Id);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser));
        }

        var wallets = await _walletRepository.GetAll()
              .Where(q =>
                    q.AccountId == accountUser.AccountId &&
                    q.RecordStatus == RecordStatus.Active)
              .ToListAsync(cancellationToken);

        var wallet = wallets.FirstOrDefault(q => q.IsMainWallet) ?? wallets.FirstOrDefault();

        if (wallet is null)
        {
            throw new NotFoundException(nameof(Wallet));
        }

        return wallet.WalletNumber;
    }

    private async Task<Wallet> CheckWalletNumberAsync(string walletNumber, CancellationToken cancellationToken)
    {
        var checkedWallet = await _walletRepository.GetAll()
                                                   .FirstOrDefaultAsync(q => q.WalletNumber == walletNumber &&
                                                                             q.RecordStatus == RecordStatus.Active, cancellationToken);

        if (checkedWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), walletNumber);
        }

        return checkedWallet;
    }

    private async Task<string> GetAccountNameByIdAsync(Guid accountId)
    {
        var account = await _accountRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == accountId && s.RecordStatus == RecordStatus.Active);

        if (account is null)
        {
            throw new NotFoundException(nameof(Account), accountId);
        }

        return account.Name;
    }

    private Guid GetUserId()
    {
        if (!Guid.TryParse(_contextProvider.CurrentContext.UserId, out Guid userId))
        {
            //UnknownUser
            userId = Guid.Empty;
        }
        return userId;
    }
}