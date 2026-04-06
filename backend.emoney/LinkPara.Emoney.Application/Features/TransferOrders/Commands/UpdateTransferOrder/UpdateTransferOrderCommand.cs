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

namespace LinkPara.Emoney.Application.Features.TransferOrders.Commands.UpdateTransferOrder;

public class UpdateTransferOrderCommand : IRequest
{
    public Guid TransferOrderId { get; set; }
    public string SenderWalletNumber { get; set; }
    public string SenderNameSurname { get; set; }
    public UserType SenderUserType { get; set; }
    public ReceiverAccountType ReceiverAccountType { get; set; }
    public string ReceiverAccountValue { get; set; }
    public string ReceiverNameSurname { get; set; }
    public string Description { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferDate { get; set; }
    public string ReceiverPhoneCode { get; set; }
}

public class UpdateTransferOrderCommandHandler : IRequestHandler<UpdateTransferOrderCommand>
{
    private readonly IGenericRepository<Wallet> _walletRepository;
    private readonly IGenericRepository<TransferOrder> _transferOrderRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IUserService _userService;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IGenericRepository<Account> _accountRepository;

    public UpdateTransferOrderCommandHandler(
        IGenericRepository<Wallet> walletRepository,
        IGenericRepository<TransferOrder> transferOrderRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IUserService userService,
        IMoneyTransferService moneyTransferService,
        IGenericRepository<AccountUser> accountUserRepository,
        IGenericRepository<Account> accountRepository)
    {
        _walletRepository = walletRepository;
        _transferOrderRepository = transferOrderRepository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _userService = userService;
        _moneyTransferService = moneyTransferService;
        _accountUserRepository = accountUserRepository;
        _accountRepository = accountRepository;
    }

    public async Task<Unit> Handle(UpdateTransferOrderCommand request, CancellationToken cancellationToken)
    {
        var transferOrder = await _transferOrderRepository.GetAll()
            .Where(q => q.Id == request.TransferOrderId
                     && q.TransferOrderStatus == TransferOrderStatus.Pending)
            .FirstOrDefaultAsync(cancellationToken);

        if (transferOrder is null)
        {
            throw new NotFoundException(nameof(TransferOrder), request.TransferOrderId);
        }

        var senderWallet = await CheckWalletNumberAsync(request.SenderWalletNumber, cancellationToken);

        if (transferOrder.UserId != GetUserId())
        {
            throw new ForbiddenAccessException();
        }

        string receiverWalletNumber = null;

        if (request.ReceiverAccountType is not ReceiverAccountType.Iban)
        {
            receiverWalletNumber = request.ReceiverAccountType switch
            {
                ReceiverAccountType.PhoneNumber => await GetReceiverWalletByPhoneAsync(request.ReceiverPhoneCode, request.ReceiverAccountValue),
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

        transferOrder.SenderWalletNumber = request.SenderWalletNumber;
        transferOrder.SenderNameSurname = request.SenderNameSurname;
        transferOrder.SenderUserType = request.SenderUserType;
        transferOrder.TransferDate = request.TransferDate;
        transferOrder.Amount = request.Amount;
        transferOrder.ReceiverAccountValue = request.ReceiverAccountValue;
        transferOrder.ReceiverAccountType = request.ReceiverAccountType;
        transferOrder.ReceiverNameSurname = request.ReceiverNameSurname;
        transferOrder.Description = request.Description;
        transferOrder.CurrencyCode = senderWallet.CurrencyCode;
        transferOrder.ReceiverPhoneCode = request?.ReceiverPhoneCode;
        transferOrder.ReceiverWalletNumber = receiverWalletNumber;

        await _transferOrderRepository.UpdateAsync(transferOrder);

        await TransferOrderAuditLogAsync(true, new Dictionary<string, string>
        {
            {"TransferOrderId",transferOrder.Id.ToString() },
            {"SenderWalletNumber",transferOrder.SenderWalletNumber },
            {"ReceiverNameSurname",transferOrder.ReceiverNameSurname }
        });

        return Unit.Value;
    }

    private async Task<string> GetReceiverWalletByPhoneAsync(string phoneCode, string receiverPhoneNumber)
    {
        UserDto user = new();
        var userName = await GetUserNameHelper.GetUserNameAsync(phoneCode, receiverPhoneNumber);

        var users = await _userService.GetAllUsersAsync(new GetUsersRequest { UserName = userName });

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
              .ToListAsync();

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
              .FirstOrDefaultAsync(q => q.WalletNumber.Equals(walletNumber)
                          && q.RecordStatus == RecordStatus.Active, cancellationToken);

        if (checkedWallet is null)
        {
            throw new NotFoundException(nameof(Wallet), walletNumber);
        }

        return checkedWallet;
    }

    private async Task TransferOrderAuditLogAsync(bool isSuccess, Dictionary<string, string> deatils)
    {
        var userId = GetUserId();
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "UpdateTransferOrder",
                Resource = "TransferOrder",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
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
}