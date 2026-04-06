using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.KKB;
using LinkPara.HttpProviders.KKB.Models;
using LinkPara.HttpProviders.Vault;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using LinkPara.SharedModels.Notification.NotificationModels.PF;
using MassTransit;

namespace LinkPara.PF.Application.Features.Merchants.Command.PutMerchantPanel;

public class PutMerchantPanelCommand : IRequest
{
    public Guid Id { get; set; }
    public UpdateMerchantPanelDto MerchantPanel { get; set; }
}

public class PutMerchantPanelCommandHandler : IRequestHandler<PutMerchantPanelCommand>
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly IGenericRepository<MerchantUser> _merchantUserRepository;
    private readonly IGenericRepository<MerchantBankAccount> _merchantBankRepository;
    private readonly IGenericRepository<Customer> _customerRepository;
    private readonly IGenericRepository<ContactPerson> _contactPersonRepository;
    private readonly ILogger<PutMerchantPanelCommandHandler> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IKKBService _kkbService;
    private readonly IVaultClient _vaultClient;
    private readonly IBus _bus;
    public PutMerchantPanelCommandHandler(
        IGenericRepository<Merchant> repository,
        IGenericRepository<MerchantBankAccount> merchantBankRepository,
        IGenericRepository<Customer> customerRepository,
        IGenericRepository<ContactPerson> contactPersonRepository,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        ILogger<PutMerchantPanelCommandHandler> logger,
        IGenericRepository<MerchantUser> merchantUserRepository,
        IKKBService kkbService,
        IVaultClient vaultClient,
        IBus bus)
    {
        _repository = repository;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _logger = logger;
        _merchantBankRepository = merchantBankRepository;
        _customerRepository = customerRepository;
        _contactPersonRepository = contactPersonRepository;
        _merchantUserRepository = merchantUserRepository;
        _kkbService = kkbService;
        _vaultClient = vaultClient;
        _bus = bus;
    }

    public async Task<Unit> Handle(PutMerchantPanelCommand command, CancellationToken cancellationToken)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (userId is null)
        {
            throw new NotFoundException(nameof(Merchant), command.Id);
        }

        var merchantUser = await _merchantUserRepository.GetAll()
             .FirstOrDefaultAsync(x => x.UserId == Guid.Parse(userId), cancellationToken);

        if (merchantUser.MerchantId != command.Id)
        {
            throw new UnauthorizedAccessException();
        }

        var merchant = await _repository.GetAll()
                    .Include(m => m.Customer)
                    .ThenInclude(m => m.AuthorizedPerson)
                    .Where(x => x.Id == command.Id)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), command.Id);
        }

        var merchantBankAccount = await _merchantBankRepository.GetAll()
            .Where(m => m.MerchantId == command.Id
            && m.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync(cancellationToken: cancellationToken);

        var customer = await _customerRepository.GetByIdAsync(merchant.CustomerId);

        var contactPerson = await _contactPersonRepository.GetByIdAsync(customer.ContactPersonId);

        try
        {
            if (!string.IsNullOrWhiteSpace(command.MerchantPanel.WebSiteUrl) && command.MerchantPanel.WebSiteUrl != merchant.WebSiteUrl)
            {
                merchant.WebSiteUrl = command.MerchantPanel.WebSiteUrl;
                await _repository.UpdateAsync(merchant);
            }
            if (!string.IsNullOrWhiteSpace(command.MerchantPanel.CompanyEmail) && command.MerchantPanel.CompanyEmail != contactPerson.CompanyEmail)
            {
                contactPerson.CompanyEmail = command.MerchantPanel.CompanyEmail;
                await _contactPersonRepository.UpdateAsync(contactPerson);
            }
            if (command.MerchantPanel.Iban != merchantBankAccount.Iban)
            {
                var isKkbEnabled = await _vaultClient.GetSecretValueAsync<bool>("SharedSecrets", "ServiceState", "KkbEnabled");

                if (isKkbEnabled)
                {
                    var validateIban = new ValidateIbanRequest()
                    {
                        Iban = command.MerchantPanel.Iban,
                        IdentityNo = merchant.Customer.CompanyType == CompanyType.Individual
                    ? merchant.Customer.AuthorizedPerson.IdentityNumber
                    : merchant.Customer.TaxNumber,
                    };

                    var kkb = await _kkbService.ValidateIban(validateIban);

                    if (!kkb.IsValid)
                    {
                        _logger.LogError("PutMerchantPanelCommandError: IBAN is not valid : {Iban}", command.MerchantPanel.Iban);
                        throw new IbanValidationFailedException();
                    }
                }

                merchantBankAccount.RecordStatus = RecordStatus.Passive;

                await _merchantBankRepository.UpdateAsync(merchantBankAccount);

                command.MerchantPanel.Iban = command.MerchantPanel.Iban.Trim().Replace(" ", string.Empty);

                var newMerchantBankAccount = new MerchantBankAccount
                {
                    Iban = command.MerchantPanel.Iban,
                    BankCode = command.MerchantPanel.BankCode,
                    MerchantId = merchantBankAccount.MerchantId,
                    RecordStatus = RecordStatus.Active
                };
                
                try
                {
                    await _merchantBankRepository.AddAsync(newMerchantBankAccount);
                    
                    await _bus.Publish(new MerchantIbanChanged
                    {
                        MerchantName = merchant.Name,
                        MerchantNumber = merchant.Number,
                        OldIban = merchantBankAccount.Iban,
                        NewIban = command.MerchantPanel.Iban
                    }, cancellationToken);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "PutMerchantPanelCommand: {Exception}", exception);
                }
            }

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "PutMerchantPanel",
                    SourceApplication = "PF",
                    Resource = "Merchant",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString() }
                    }
                });
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "PutMerchantPanelError : {Exception}", exception);
            throw;
        }
    }
}
