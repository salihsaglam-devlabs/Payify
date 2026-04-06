using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.HttpProviders.MoneyTransfer;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateBankAccount;

public class UpdateBankAccountCommand : IRequest
{
    public string Tag { get; set; }
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Iban { get; set; }
}

public class UpdateBankAccountCommandHandler : IRequestHandler<UpdateBankAccountCommand>
{
    private readonly IGenericRepository<SavedBankAccount> _savedBankAccountRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IMoneyTransferService _moneyTransferService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly ITierPermissionService _permissionService;
    private readonly IAccountIbanService _accountIbanService;
    private readonly IContextProvider _contextProvider;

    public UpdateBankAccountCommandHandler(IGenericRepository<SavedBankAccount> savedBankAccountRepository,
        IAuditLogService auditLogService,
        IMoneyTransferService moneyTransferService,
        IGenericRepository<AccountUser> accountUserRepository,
        ITierPermissionService permissionService,
        IAccountIbanService accountIbanService,
        IContextProvider contextProvider)
    {
        _savedBankAccountRepository = savedBankAccountRepository;
        _auditLogService = auditLogService;
        _moneyTransferService = moneyTransferService;
        _accountUserRepository = accountUserRepository;
        _permissionService = permissionService;
        _accountIbanService = accountIbanService;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(UpdateBankAccountCommand request, CancellationToken cancellationToken)
    {
        var savedBankAccount = await _savedBankAccountRepository.GetAll()
                                                                .Where(s => s.Id == request.Id)
                                                                .SingleOrDefaultAsync(cancellationToken);

        if (savedBankAccount is null)
        {
            throw new NotFoundException(nameof(SavedBankAccount), request.Id);
        }

        var loggedUser = Guid.Parse(_contextProvider.CurrentContext.UserId);

        if (request.UserId != loggedUser || savedBankAccount.UserId != loggedUser)
        {
            throw new ForbiddenAccessException();
        }

        request.Iban = request.Iban.Trim().Replace(" ", "");

        var duplicateIban = _savedBankAccountRepository.GetAll()
            .SingleOrDefault(x =>
                x.Iban == request.Iban &&
                x.UserId == request.UserId &&
                x.RecordStatus == RecordStatus.Active &&
                x.Id != request.Id);

        if (duplicateIban is not null)
        {
            await UpdateBankAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"Iban", request.Iban},
                {"Tag", request.Tag},
                {"Error", "DuplicateIban" }
            });
            throw new DuplicateRecordException();
        }

        var ibanValidationResult = await _moneyTransferService.CheckIbanAsync(request.Iban);

        if (!ibanValidationResult.IsValidIban)
        {
            await UpdateBankAccountAuditLogAsync(false, request.UserId, new Dictionary<string, string>
            {
                {"Iban", request.Iban},
                {"Tag", request.Tag},
                {"Error", "InvalidIban" }
            });
            throw new InvalidIbanException();
        }

        var accountUser = await _accountUserRepository.GetAll()
            .Include(s => s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        var account = accountUser.Account;

        await ValidateSaveBankAccountPermissionAsync(account, request.Iban);

        savedBankAccount.Iban = request.Iban;
        savedBankAccount.Tag = request.Tag;

        await _savedBankAccountRepository.UpdateAsync(savedBankAccount);

        await UpdateBankAccountAuditLogAsync(true, request.UserId, new Dictionary<string, string>
        {
            {"SavedAccountId", savedBankAccount.Id.ToString()},
            {"Tag", request.Tag}
        });

        return Unit.Value;
    }

    private async Task UpdateBankAccountAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> deatils)
    {
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                Details = deatils,
                LogDate = DateTime.Now,
                Operation = "UpdateBankAccount",
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