using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using LinkPara.Emoney.Application.Commons.Attributes;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveBankAccount;

public class SaveBankAccountCommand : IRequest
{
    [Audit]
    public string Iban { get; set; }
    public Guid UserId { get; set; }
    [Audit]
    public string Tag { get; set; }
    [Audit]
    public string ReceiverName { get; set; }
}

public class SaveBankAccountCommandHandler : IRequestHandler<SaveBankAccountCommand>
{
    private readonly IGenericRepository<SavedBankAccount> _savedBankAccountRepository;
    private readonly IBankService _bankService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IAccountIbanService _accountIbanService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly ITierPermissionService _permissionService;
    private readonly IContextProvider _contextProvider;

    public SaveBankAccountCommandHandler(IGenericRepository<SavedBankAccount> savedBankAccountRepository,
        IBankService bankService,
        IAuditLogService auditLogService,
        IMoneyTransferService moneyTransferService,
        IAccountIbanService accountIbanService,
        IGenericRepository<AccountUser> accountUserRepository,
        ITierPermissionService permissionService,
        IContextProvider contextProvider)
    {
        _savedBankAccountRepository = savedBankAccountRepository;
        _bankService = bankService;
        _auditLogService = auditLogService;
        _moneyTransferService = moneyTransferService;
        _accountIbanService = accountIbanService;
        _accountUserRepository = accountUserRepository;
        _permissionService = permissionService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(SaveBankAccountCommand request, CancellationToken cancellationToken)
    {
        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        request.Iban = request.Iban.Trim().Replace(" ", "");

        var duplicateIban = _savedBankAccountRepository
            .GetAll()
            .SingleOrDefault(x =>
                x.Iban == request.Iban &&
                x.UserId == request.UserId &&
                x.RecordStatus == RecordStatus.Active);

        if (duplicateIban is not null)
        {
            await SaveBankAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"Iban", request.Iban},
                {"Tag", request.Tag},
                {"ReceiverName", request.ReceiverName},
                { "Error", "DuplicateIban" }
            });
            throw new DuplicateRecordException();
        }

        var ibanValidationResult = await _moneyTransferService.CheckIbanAsync(request.Iban);

        if (!ibanValidationResult.IsValidIban)
        {
            await SaveBankAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"Iban", request.Iban},
                {"Tag", request.Tag},
                {"ReceiverName", request.ReceiverName},
                { "Error", "InvalidIban" }
            });
            throw new InvalidIbanException();
        }

        var bank = await _bankService.GetBankAsync(ibanValidationResult.BankCode.ToString());

        if (bank is null)
        {
            throw new InvalidIbanException();
        }

        var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        var account = accountUser.Account;

        await ValidateSaveBankAccountPermissionAsync(account, request.Iban);

        var bankAccount = new SavedBankAccount
        {
            Tag = request.Tag,
            Iban = request.Iban,
            ReceiverName = request.ReceiverName,
            UserId = request.UserId,
            BankId = bank.Id,
            RecordStatus = RecordStatus.Active
        };

        await _savedBankAccountRepository.AddAsync(bankAccount);

        return Unit.Value;
    }

    private async Task SaveBankAccountAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "SaveBankAccount",
                Resource = "SavedAccount",
                SourceApplication = "Emoney",
                UserId = userId
            }
        );
    }

    private async Task ValidateSaveBankAccountPermissionAsync(Account account, string iban)
    {
        var kkbResult = await _accountIbanService.ValidateIbanAsync(account.IdentityNumber, iban);

        if (kkbResult)
        {
            await _permissionService.ValidatePermissionAsync(account.AccountKycLevel, TierPermissionType.SaveOwnIban);
        }
        else
        {
            await _permissionService.ValidatePermissionAsync(account.AccountKycLevel, TierPermissionType.SaveOtherIban);
        }
    }
}