using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ReceiptModels;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetReceipt;

public class GetReceiptQuery : IRequest<ReceiptResponse>
{
    public Guid TransactionId { get; set; }
    public Guid UserId { get; set; }
    public string CustomerNumber { get; set; }
}

public class GetReceiptQueryQueryHandler : IRequestHandler<GetReceiptQuery, ReceiptResponse>
{
    private readonly ITransactionService _transactionService;
    private readonly IParameterService _parameterService;
    private readonly ILogger<GetReceiptQueryQueryHandler> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<CardTopupRequest> _cardTopupRequestRepository;
    private readonly IBankService _bankService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;

    private const string CompanyInfoGroupCode = "ReceiptCompanyInfo";
    private const string AddressParam = "Address";
    private const string MersisNumberParam = "MersisNumber";
    private const string NameParam = "Name";
    private const string TaxNumberParam = "TaxNumber";
    private const string TaxOfficeParam = "TaxOffice";
    private const string DateFormat = "dd.MM.yyyy HH:mm:ss";

    public GetReceiptQueryQueryHandler(ITransactionService transactionService,
        IParameterService parameterService,
        ILogger<GetReceiptQueryQueryHandler> logger,
        IStringLocalizerFactory localizer,
        IGenericRepository<CardTopupRequest> cardTopupRequestRepository,
        IBankService bankService,
        IGenericRepository<AccountUser> accountUserRepository)
    {
        _transactionService = transactionService;
        _parameterService = parameterService;
        _logger = logger;
        _localizer = localizer.Create("ReceiptTexts", "LinkPara.Emoney.API");
        _cardTopupRequestRepository = cardTopupRequestRepository;
        _bankService = bankService;
        _accountUserRepository = accountUserRepository;
    }

    public async Task<ReceiptResponse> Handle(GetReceiptQuery request, CancellationToken cancellationToken)
    {        
        var accountUser = await _accountUserRepository.GetAll()
            .Include(s=>s.Account)
            .FirstOrDefaultAsync(s => s.UserId == request.UserId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), request.UserId);
        }       

        var tx = await _transactionService.GetTransactionWithDetailsAsync(request.TransactionId, cancellationToken);

        if(tx.Wallet.AccountId != accountUser.AccountId)
        {
            throw new ForbiddenAccessException();
        }

        if (tx.ReceiptNumber == null)
        {
            return new ReceiptResponse { IsReady = false };
        }

        if (tx.TransactionType is TransactionType.Tax or TransactionType.Commission)
        {
            tx = await _transactionService
                .GetTransactionWithDetailsAsync(tx.RelatedTransactionId!.Value, cancellationToken);
        }

        var receipt = MapBase(tx, request);

        await MapSenderBeneficiary(receipt, tx);

        receipt.CompanyInfo = await GetReceiptCompanyAsync();

        return receipt;
    }

    private async Task<ReceiptCompanyInfo> GetReceiptCompanyAsync()
    {
        var receiptInfo = new ReceiptCompanyInfo();

        try
        {
            var companyInfo = await _parameterService.GetParametersAsync(CompanyInfoGroupCode);

            if (companyInfo == null)
            {
                throw new NotFoundException(CompanyInfoGroupCode);
            }

            receiptInfo.Address = companyInfo.FirstOrDefault(p => p.ParameterCode == AddressParam)?.ParameterValue;
            receiptInfo.MersisNumber = companyInfo.FirstOrDefault(p => p.ParameterCode == MersisNumberParam)?.ParameterValue;
            receiptInfo.Name = companyInfo.FirstOrDefault(p => p.ParameterCode == NameParam)?.ParameterValue;
            receiptInfo.TaxNumber = companyInfo.FirstOrDefault(p => p.ParameterCode == TaxNumberParam)?.ParameterValue;
            receiptInfo.TaxOffice = companyInfo.FirstOrDefault(p => p.ParameterCode == TaxOfficeParam)?.ParameterValue;

        }
        catch (Exception exception)
        {
            _logger.LogError("CompanyInfo Fetch Exception : {Exception}", exception);
        }

        return receiptInfo;
    }

    private ReceiptResponse MapBase(TransactionDto tx, GetReceiptQuery query)
    {
        return new ReceiptResponse
        {
            IsReady = true,
            CustomerNumber = query.CustomerNumber,
            ReceiptNumber = tx.ReceiptNumber,
            Transaction = new ReceiptTransaction
            {
                TransactionId = tx.Id,
                TransactionDate = tx.TransactionDate.ToString(DateFormat),
                TransactionType = tx.TransactionType,
                PaymentMethod = tx.PaymentMethod,
                Direction = tx.TransactionDirection,
                CurrencyCode = tx.CurrencyCode,
                Description = tx.Description,
                PaymentType = tx.PaymentType,
                Tag = tx.Tag,
                ReturnedTransactionId = tx.ReturnedTransactionId,
                Display = ResolveDisplayTypeKey(tx)
            },
            Amounts = new ReceiptAmounts
            {
                Amount = tx.Amount,
                Commission = tx.CommissionAmount,
                Tax = tx.TaxAmount,
                TotalAmount = tx.TotalAmount,
                TotalAmountText = tx.TotalAmountText,
                Kmv = 0m
            },
        };
    }

    private async Task MapSenderBeneficiary(ReceiptResponse receipt, TransactionDto tx)
    {
        var isMoneyIn = tx.TransactionDirection == TransactionDirection.MoneyIn;

        if (tx.PaymentMethod == PaymentMethod.CreditCard)
        {
            var topup = await _cardTopupRequestRepository.GetByIdAsync(tx.CardTopupRequestId);

            receipt.Sender = new ReceiptParty
            {
                FullName = topup.Name,
                MaskedCardNumber = topup.CardNumber,
                BankName = topup.BankName,
            };
            receipt.Receiver = new ReceiptParty
            {
                WalletNumber = isMoneyIn ? tx.WalletNumber : tx.CounterWalletNumber,
                FullName = isMoneyIn ? tx.WalletName : tx.CounterWalletName
            };
        }

        else if (tx.PaymentMethod == PaymentMethod.BankTransfer)
        {
            if (isMoneyIn)
            {
                var bankName = string.Empty;

                if (!string.IsNullOrEmpty(tx.SenderAccountNumber))
                {
                    var bank = await _bankService.ResolveBankFromIbanAsync(tx.SenderAccountNumber);
                    bankName = bank.FirstOrDefault()?.Name;
                }
                
                receipt.Sender = new ReceiptParty
                {
                    FullName = tx.SenderName,
                    Iban = tx.SenderAccountNumber,
                    BankName= bankName,
                };

                receipt.Receiver = new ReceiptParty
                {
                    FullName = tx.WalletName,
                    WalletNumber = tx.WalletNumber
                };
            }
            else
            {
                var bankName = string.Empty;

                if (!string.IsNullOrEmpty(tx.ReceiverIban))
                {
                    var bank = await _bankService.ResolveBankFromIbanAsync(tx.ReceiverIban);
                    bankName = bank.FirstOrDefault()?.Name;
                }

                receipt.Sender = new ReceiptParty
                {
                    FullName = tx.WalletName,
                    WalletNumber = tx.WalletNumber
                };

                receipt.Receiver = new ReceiptParty
                {
                    FullName = tx.ReceiverName,
                    Iban = tx.ReceiverIban,
                    BankName = bankName
                };
            }
        }
        else
        {
            receipt.Sender = new ReceiptParty
            {
                FullName = !isMoneyIn ? tx.WalletName : tx.CounterWalletName,
                WalletNumber = !isMoneyIn ? tx.WalletNumber : tx.CounterWalletNumber
            };

            receipt.Receiver = new ReceiptParty
            {
                FullName = isMoneyIn ? tx.WalletName : tx.CounterWalletName,
                WalletNumber = isMoneyIn ? tx.WalletNumber : tx.CounterWalletNumber
            };
        }
    }

    private string ResolveDisplayTypeKey(TransactionDto tx)
    {
        if (tx.TransactionType == TransactionType.Return || tx.TransactionType == TransactionType.BankReturn)
        {
            return tx.PaymentMethod == PaymentMethod.CreditCard
                ? _localizer.GetString("RETURN-CREDIT-CARD")
                : _localizer.GetString("RETURN");
        }

        if (tx.TransactionType == TransactionType.Billing) return _localizer.GetString("BILLING");
        if (tx.TransactionType == TransactionType.Epin) return _localizer.GetString("EPIN");
        if (tx.TransactionType == TransactionType.Cashback) return _localizer.GetString("CASHBACK");
        if (tx.TransactionType == TransactionType.IWallet) return _localizer.GetString("CAMPAIGN");
        if (tx.TransactionType == TransactionType.Maintenance) return _localizer.GetString("MAINTENANCE");

        if (tx.PaymentMethod == PaymentMethod.Prepaid)
        {
            return tx.TransactionDirection == TransactionDirection.MoneyIn
                ? _localizer.GetString("PREPAID-CARD-IN")
                : _localizer.GetString("PREPAID-CARD-OUT");
        }

        if (tx.PaymentMethod == PaymentMethod.CreditCard)
        {
            return tx.TransactionDirection == TransactionDirection.MoneyIn
            ? _localizer.GetString("CREDIT-CARD-IN")
            : _localizer.GetString("CREDIT-CARD-OUT");
        }

        if (tx.PaymentMethod == PaymentMethod.Transfer)
        {
            return tx.TransactionDirection == TransactionDirection.MoneyIn
            ? _localizer.GetString("EMONEY-IN")
            : _localizer.GetString("EMONEY-OUT");
        }

        if (tx.PaymentMethod == PaymentMethod.BankTransfer)
        {
            return tx.TransactionDirection == TransactionDirection.MoneyIn
            ? _localizer.GetString("TRANSFER-MONEY-IN")
            : _localizer.GetString("TRANSFER-MONEY-OUT");
        }

        return string.Empty;
    }
}
