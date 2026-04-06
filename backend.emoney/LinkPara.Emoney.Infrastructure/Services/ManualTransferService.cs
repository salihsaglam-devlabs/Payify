using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.PricingModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.ManualTransfer.Commands;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.BusinessParameter.Models;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.HttpProviders.Documents;
using LinkPara.HttpProviders.Documents.Models;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Npgsql;
using BTransOperationType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.OperationType;
using BTransTransferReason = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferReason;
using BTransTransferType = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.TransferType;

namespace LinkPara.Emoney.Infrastructure.Services;

public class ManualTransferService : IManualTransferService
{
    private readonly ILimitService _limitService;
    private readonly IParameterService _parameterService;
    private readonly IDocumentService _documentService;
    private readonly IContextProvider _contextProvider;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ManualTransferService> _logger;
    private readonly ITierLevelService _tierLevelService;
    private readonly ISaveReceiptService _saveReceiptService;
    private readonly IAccountingService _accountingService;
    private readonly IPricingProfileService _pricingProfileService;
    private readonly IAuditLogService _auditLogService;
    private readonly IBTransService _bTransService;
    private readonly IStringLocalizer _localizer;
    private readonly IDatabaseProviderService _databaseProviderService;

    public ManualTransferService(
        IParameterService parameterService,
        IDocumentService documentService,
        IContextProvider contextProvider,
        IServiceScopeFactory scopeFactory,
        ILogger<ManualTransferService> logger,
        ILimitService limitService,
        ITierLevelService tierLevelService,
        ISaveReceiptService saveReceiptService,
        IAccountingService accountingService,
        IStringLocalizerFactory factory,
        IPricingProfileService pricingProfileService,
        IAuditLogService auditLogService,
        IBTransService bTransService,
        IDatabaseProviderService databaseProviderService)
    {
        _parameterService = parameterService;
        _documentService = documentService;
        _contextProvider = contextProvider;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _limitService = limitService;
        _tierLevelService = tierLevelService;
        _saveReceiptService = saveReceiptService;
        _accountingService = accountingService;
        _pricingProfileService = pricingProfileService;
        _auditLogService = auditLogService;
        _bTransService = bTransService;
        _localizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
        _databaseProviderService = databaseProviderService;
    }

    public async Task CreateManualTransferAsync(CreateManualTransferCommand request, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
        var strategy = new NoRetryExecutionStrategy(dbContext);
        var receiverTransaction = new Transaction();
        var senderTransaction = new Transaction();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transactionScope = await dbContext.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var customerWallet = await GetCustomerWalletAsync(dbContext, request, cancellationToken);

                var companyWallet = await GetCompanyWalletAsync(dbContext, cancellationToken);

                Wallet senderWallet, receiverWallet;

                if (request.TransactionType == TransactionType.ManualTransferDeposit)
                {
                    senderWallet = companyWallet;
                    receiverWallet = customerWallet;
                }
                else if (request.TransactionType == TransactionType.ManualTransferWithdraw)
                {
                    senderWallet = customerWallet;
                    receiverWallet = companyWallet;
                }
                else
                {
                    throw new InvalidOperationException(request.TransactionType.ToString());
                }

                if (request.TransferRequestFile == Guid.Empty)
                {
                    throw new RequiredDocumentTypeException(_localizer.GetString("TransferRequestFileRequired"));
                }

                var documentTypeList = await _documentService.GetDocumentTypesAsync(new GetDocumentTypesRequest());

                var manualTransferAmountLimit = await _parameterService.GetParameterAsync("ManualTransfer", "ManualTransferAmountLimit");

                if (manualTransferAmountLimit is null)
                {
                    throw new NotFoundException(nameof(ParameterDto), "ManualTransferAmountLimit");
                }

                if (request.Amount > decimal.Parse(manualTransferAmountLimit.ParameterValue))
                {
                    if (request.TransferApprovalFile == Guid.Empty || request.TransferApprovalFile == null)
                    {
                        throw new RequiredDocumentTypeException(_localizer.GetString("TransferApprovalFileRequired"));
                    }
                }

                var documentIdList = new List<CreateDocumentResponse>
                {
                    new CreateDocumentResponse
                    {
                        Id = request.TransferRequestFile,
                        DocumentTypeId = documentTypeList.FirstOrDefault(x=>x.Name.Equals(DocumentType.ManualTransferRequest.ToString()))?.Id ?? Guid.Empty
                    }
                };

                if (request.TransferApprovalFile.HasValue && request.TransferApprovalFile != Guid.Empty)
                {
                    documentIdList.Add(new CreateDocumentResponse
                    {
                        Id = request.TransferApprovalFile.Value,
                        DocumentTypeId = documentTypeList.FirstOrDefault(x => x.Name.Equals(DocumentType.ManualTransferApproval.ToString()))?.Id ?? Guid.Empty
                    });
                }


                receiverTransaction = await DoReceiverTransfer(request, cancellationToken, senderWallet, receiverWallet, dbContext);

                senderTransaction = await DoSenderTransfer(request, cancellationToken, senderWallet, receiverWallet, dbContext, documentIdList, documentTypeList);

                var transactionId = request.TransactionType == TransactionType.ManualTransferWithdraw ? senderTransaction.Id : receiverTransaction.Id;

                await AddTransactionDocumentsAsync(dbContext, documentIdList, transactionId, documentTypeList, request.ApprovalId, cancellationToken);

                await dbContext.SaveChangesAsync(cancellationToken);

                await transactionScope.CommitAsync(cancellationToken);

                var pricing = await CalculatePricingAsync(senderWallet, receiverWallet, request.Amount);

                await SendAccountingQueueAsync(senderWallet, receiverWallet, pricing, receiverTransaction, pricing, senderTransaction.Id);

                var details = new Dictionary<string, string>
                          {
                              {"SenderWalletNumber", senderWallet.WalletNumber.ToString() },
                              {"ReceiverWalletNumber", receiverWallet.WalletNumber.ToString() },
                              {"TransactionId" ,receiverTransaction.Id.ToString() },
                              {"Amount", request.Amount.ToString() },
                              {"TransferType", request.TransactionType.ToString() }
                          };

                await SendP2PMoneyTransferAuditLogAsync(true, Guid.Parse(_contextProvider.CurrentContext.UserId), details);

                _ = Task.Run(() =>
                    SendTransferBTransQueueAsync(senderWallet, senderWallet.Account, receiverWallet,
                        receiverWallet.Account, pricing, senderTransaction));

            }
            catch (Exception exception)
            {
                _logger.LogError("Error occurred while creating manual transfer: {Exception}", exception);

                await transactionScope.RollbackAsync(cancellationToken);
                throw;
            }
        });

        await _saveReceiptService.SendReceiptQueueAsync(receiverTransaction.Id);
        await _saveReceiptService.SendReceiptQueueAsync(senderTransaction.Id);

    }

    private async Task SendP2PMoneyTransferAuditLogAsync(bool isSuccess, Guid userId, Dictionary<string, string> details)
    {
        await _auditLogService.AuditLogAsync(
               new AuditLog
               {
                   IsSuccess = isSuccess,
                   LogDate = DateTime.Now,
                   Operation = "P2PManualTransfer",
                   SourceApplication = "Emoney",
                   Resource = "MoneyTransfer",
                   UserId = userId,
                   Details = details
               }
           );
    }

    private async Task<CalculatePricingResponse> CalculatePricingAsync(Wallet senderWallet, Wallet receiverWallet, Decimal amount)
    {
        return new CalculatePricingResponse
        {
            Amount = amount,
            BsmvRate = 0,
            BsmvTotal = 0,
            CommissionAmount = 0,
            CommissionRate = 0,
            Fee = 0
        };
    }

    private async Task SendAccountingQueueAsync(Wallet senderWallet, Wallet receiverWallet,
                                                CalculatePricingResponse pricing,
                                                Transaction receiverTransaction,
                                                CalculatePricingResponse receiverPricing,
                                                Guid senderTransactionId)
    {
        AccountingPayment payment = new AccountingPayment
        {
            Amount = receiverTransaction.Amount,
            BsmvAmount = pricing.BsmvTotal,
            CommissionAmount = pricing.PricingAmount,
            CurrencyCode = senderWallet.CurrencyCode,
            Destination = $"WA-{receiverWallet.WalletNumber}",
            HasCommission = pricing.PricingAmount > 0,
            OperationType = OperationType.EmoneyTransfer,
            Source = $"WA-{senderWallet.WalletNumber}",
            TransactionDate = receiverTransaction.TransactionDate,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            AccountingCustomerType = AccountingCustomerType.Emoney,
            AccountingTransactionType = AccountingTransactionType.Emoney,
            TransactionId = senderTransactionId
        };

        if (receiverPricing is not null && receiverPricing.CommissionAmount > 0)
        {
            payment.ReceiverCommissionAmount = receiverPricing.CommissionAmount;
            payment.ReceiverBsmvAmount = receiverPricing.BsmvTotal;
        }

        await _accountingService.SavePaymentAsync(payment);
    }

    private async Task SendTransferBTransQueueAsync(Wallet senderWallet, Account senderAccount,
                                                    Wallet receiverWallet, Account receiverAccount,
                                                    CalculatePricingResponse pricing, Transaction transaction)
    {
        try
        {
            var totalPricingAmount = pricing.PricingAmount + pricing.BsmvTotal + transaction.Amount;

            #region MoneyTransferReport
            var senderBTransIdentity = _bTransService.GetAccountInformation(senderAccount);
            var receiverBTransIdentity = _bTransService.GetAccountInformation(receiverAccount);
            var moneyTransfer = new MoneyTransferReport
            {
                RecordType = RecordTypeConst.NewRecord,
                OperationType = BTransOperationType.AccountToAccount,
                TransferType = BTransTransferType.AccountToAccount,

                //SenderBlock
                IsSenderCustomer = true,
                IsSenderCorporate = senderBTransIdentity.IsCorporate,
                SenderPhoneNumber = senderBTransIdentity.PhoneNumber,
                SenderEmail = senderBTransIdentity.Email,
                SenderWalletNumber = senderWallet.WalletNumber,
                SenderCityId = 0,
                SenderTaxNumber = senderBTransIdentity.TaxNumber,
                SenderCommercialTitle = senderBTransIdentity.CommercialTitle,
                SenderFirstName = senderBTransIdentity.FirstName,
                SenderLastName = senderBTransIdentity.LastName,
                SenderIdentityNumber = senderBTransIdentity.IdentityNumber,

                //ReceiverBlock
                IsReceiverCustomer = true,
                IsReceiverCorporate = receiverBTransIdentity.IsCorporate,
                ReceiverPhoneNumber = receiverBTransIdentity.PhoneNumber,
                ReceiverEmail = receiverBTransIdentity.Email,
                ReceiverWalletNumber = receiverWallet.WalletNumber,
                ReceiverCityId = 0,
                ReceiverTaxNumber = receiverBTransIdentity.TaxNumber,
                ReceiverCommercialTitle = receiverBTransIdentity.CommercialTitle,
                ReceiverFirstName = receiverBTransIdentity.FirstName,
                ReceiverLastName = receiverBTransIdentity.LastName,
                ReceiverIdentityNumber = receiverBTransIdentity.IdentityNumber,

                //TransactionBlock
                RelatedTransactionId = transaction.Id,
                TransactionDate = transaction.TransactionDate,
                PaymentDate = transaction.TransactionDate,
                Amount = transaction.Amount,
                ConvertedAmount = transaction.Amount,
                CurrencyCode = transaction.CurrencyCode,
                TotalPricingAmount = totalPricingAmount,
                TransferReason = BTransTransferReason.Other,
                IpAddress = _contextProvider.CurrentContext.ClientIpAddress,
                CustomerDescription = transaction.Description,
            };
            #endregion

            #region SenderCustomer
            var senderCustomerInformation = await _bTransService.GetCustomerInformationAsync(senderAccount.CustomerId);
            if (senderCustomerInformation.IsSucceed)
            {
                moneyTransfer.IsSenderCorporate = senderCustomerInformation.IsCorporate;
                moneyTransfer.SenderPhoneNumber = senderCustomerInformation.PhoneNumber;
                moneyTransfer.SenderEmail = senderCustomerInformation.Email;
                moneyTransfer.SenderNationCountryId = senderCustomerInformation.NationCountryId;
                moneyTransfer.SenderCityId = senderCustomerInformation.CityId ?? 0;
                moneyTransfer.SenderFullAddress = senderCustomerInformation.FullAddress;
                moneyTransfer.SenderDistrict = senderCustomerInformation.District;
                moneyTransfer.SenderPostalCode = senderCustomerInformation.PostalCode;
                moneyTransfer.SenderCity = senderCustomerInformation.City;
                moneyTransfer.SenderTaxNumber = senderCustomerInformation.TaxNumber;
                moneyTransfer.SenderCommercialTitle = senderCustomerInformation.CommercialTitle;
                moneyTransfer.SenderFirstName = senderCustomerInformation.FirstName;
                moneyTransfer.SenderLastName = senderCustomerInformation.LastName;
                moneyTransfer.SenderDocumentType = senderCustomerInformation.DocumentType;
                moneyTransfer.SenderIdentityNumber = senderCustomerInformation.IdentityNumber;
            }
            #endregion

            #region ReceiverCustomer
            var receiverCustomerInformation = await _bTransService.GetCustomerInformationAsync(receiverAccount.CustomerId);
            if (receiverCustomerInformation.IsSucceed)
            {
                moneyTransfer.IsReceiverCorporate = receiverCustomerInformation.IsCorporate;
                moneyTransfer.ReceiverPhoneNumber = receiverCustomerInformation.PhoneNumber;
                moneyTransfer.ReceiverEmail = receiverCustomerInformation.Email;
                moneyTransfer.ReceiverNationCountryId = receiverCustomerInformation.NationCountryId;
                moneyTransfer.ReceiverCityId = receiverCustomerInformation.CityId ?? 0;
                moneyTransfer.ReceiverFullAddress = receiverCustomerInformation.FullAddress;
                moneyTransfer.ReceiverDistrict = receiverCustomerInformation.District;
                moneyTransfer.ReceiverPostalCode = receiverCustomerInformation.PostalCode;
                moneyTransfer.ReceiverCity = receiverCustomerInformation.City;
                moneyTransfer.ReceiverTaxNumber = receiverCustomerInformation.TaxNumber;
                moneyTransfer.ReceiverCommercialTitle = receiverCustomerInformation.CommercialTitle;
                moneyTransfer.ReceiverFirstName = receiverCustomerInformation.FirstName;
                moneyTransfer.ReceiverLastName = receiverCustomerInformation.LastName;
                moneyTransfer.ReceiverDocumentType = receiverCustomerInformation.DocumentType;
                moneyTransfer.ReceiverIdentityNumber = receiverCustomerInformation.IdentityNumber;
            }
            #endregion

            await _bTransService.SaveMoneyTransferAsync(moneyTransfer);
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, $"Failed to send transfer transaction [{transaction.Id}] to BTrans reporting tool  Error : {exception}");
        }
    }

    private async Task<Transaction> DoSenderTransfer(CreateManualTransferCommand request, CancellationToken cancellationToken,
        Wallet senderWallet, Wallet receiverWallet, EmoneyDbContext dbContext, List<CreateDocumentResponse> documentIdList, List<DocumentTypeDto> documentTypeList)
    {
        var senderTransaction = PopulateSenderTransaction(request, senderWallet, receiverWallet);
        await dbContext.Transaction.AddAsync(senderTransaction, cancellationToken);

        UpdateSenderWalletBalance(senderWallet, request.Amount);
        dbContext.Wallet.Update(senderWallet);

        await UpdateAccountLimitAsync(dbContext, receiverWallet, request.Amount);

        return senderTransaction;
    }

    private async Task<Transaction> DoReceiverTransfer(CreateManualTransferCommand request, CancellationToken cancellationToken,
        Wallet senderWallet, Wallet receiverWallet, EmoneyDbContext dbContext)
    {
        var receiverTransaction = PopulateReceiverTransaction(request, senderWallet, receiverWallet);
        await dbContext.Transaction.AddAsync(receiverTransaction, cancellationToken);

        UpdateReceiverWalletBalance(receiverWallet, request.Amount);
        dbContext.Wallet.Update(receiverWallet);

        await UpdateAccountLimitAsync(dbContext, senderWallet, request.Amount);

        return receiverTransaction;
    }


    private async Task UpdateAccountLimitAsync(EmoneyDbContext dbContext, Wallet senderWallet, decimal amount)
    {
        var existingLevel = await dbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == senderWallet.AccountId
                                       && x.CurrencyCode == senderWallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(senderWallet.CurrencyCode, senderWallet.AccountId, Guid.Parse(_contextProvider.CurrentContext.UserId));
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.InternalTransfer,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType
            }, level);
            dbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = senderWallet.AccountId,
                LimitOperationType = LimitOperationType.InternalTransfer,
                Amount = amount,
                CurrencyCode = senderWallet.CurrencyCode,
                WalletType = senderWallet.WalletType
            }, existingLevel);
            dbContext.Update(existingLevel);
        }
    }

    private async Task AddTransactionDocumentsAsync(EmoneyDbContext dbContext, List<CreateDocumentResponse> documentIdList, Guid transactionId, List<DocumentTypeDto> documentTypeDtoList, Guid approvalId, CancellationToken cancellationToken)
    {
        foreach (var document in documentIdList)
        {
            var transactionDocument = new ManualTransferReference
            {
                ApprovalRequestId = approvalId,
                CreateDate = DateTime.Now,
                CreatedBy = _contextProvider.CurrentContext.UserId,
                RecordStatus = RecordStatus.Active,
                DocumentId = document.Id,
                TransactionId = transactionId,
                DocumentTypeId = document.DocumentTypeId,
                DocumentType = Enum.Parse<DocumentType>(documentTypeDtoList.First(s => s.Id == document.DocumentTypeId).Name)
            };

            await dbContext.ManualTransferReference.AddAsync(transactionDocument, cancellationToken);
        }
    }

    private void UpdateReceiverWalletBalance(Wallet receiverWallet, decimal requestAmount)
    {
        receiverWallet.CurrentBalanceCash += requestAmount;
    }

    private void UpdateSenderWalletBalance(Wallet senderWallet, decimal requestAmount)
    {
        var newBalanceCash = senderWallet.CurrentBalanceCash - requestAmount;
        if (newBalanceCash - senderWallet.BlockedBalance < 0)
        {
            throw new InvalidOperationException(_localizer.GetString("InsufficientBalance"));
        }
        senderWallet.CurrentBalanceCash = newBalanceCash;
    }

    private async Task<Wallet> GetCompanyWalletAsync(EmoneyDbContext dbContext, CancellationToken cancellationToken)
    {
        var companyWalletNumber = await _parameterService.GetParameterAsync("ManualTransfer", "ManualTransferWalletNumber");

        if (companyWalletNumber == null)
        {
            throw new NotFoundException(nameof(ParameterDto), $"Company Account Parameter not found: GroupCode: CompanyContactInformation, ParameterCode: ManualTransferWalletNumber");
        }

        var wallet = await GetWalletWithLockAsync(dbContext, companyWalletNumber.ParameterValue, cancellationToken);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Account), $"Company Account Parameter not found: GroupCode: CompanyContactInformation, ParameterCode: ManualTransferWalletNumber");
        }

        return wallet;
    }

    private async Task<Wallet> GetCustomerWalletAsync(EmoneyDbContext dbContext, CreateManualTransferCommand request, CancellationToken cancellationToken)
    {
        var customerWallet = await GetWalletWithLockAsync(dbContext, request.CustomerWalletNumber, cancellationToken);

        if (customerWallet == null)
        {
            throw new NotFoundException(nameof(Account), $"Customer Account not found: WalletNumber: {request.CustomerWalletNumber}");
        }

        return customerWallet;
    }

    private Transaction PopulateReceiverTransaction(CreateManualTransferCommand command, Wallet senderWallet, Wallet receiverWallet)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = TransactionType.ManualTransferDeposit,
            TransactionStatus = TransactionStatus.Completed,
            Tag = senderWallet.Account.Name,
            TagTitle = TransactionType.Deposit.ToString(),
            Amount = command.Amount,
            CurrencyCode = receiverWallet.CurrencyCode,
            Description = command.Description,
            WalletId = receiverWallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = receiverWallet.AvailableBalance + command.Amount,
            PreBalance = receiverWallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Transfer,
            RecordStatus = RecordStatus.Active,
            SenderName = senderWallet.Account.Name,
            ReceiverName = receiverWallet.Account.Name,
            CounterWalletId = senderWallet.Id,
            Channel = _contextProvider.CurrentContext?.Channel
        };
    }

    private Transaction PopulateSenderTransaction(CreateManualTransferCommand command, Wallet senderWallet, Wallet receiverWallet)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.ManualTransferWithdraw,
            TransactionStatus = TransactionStatus.Completed,
            Tag = receiverWallet.Account.Name,
            TagTitle = TransactionType.Withdraw.ToString(),
            Amount = command.Amount,
            CurrencyCode = senderWallet.CurrencyCode,
            Description = command.Description,
            WalletId = senderWallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = senderWallet.AvailableBalance - command.Amount,
            PreBalance = senderWallet.AvailableBalance,
            TransactionDate = DateTime.Now,
            PaymentMethod = PaymentMethod.Transfer,
            RecordStatus = RecordStatus.Active,
            ReceiverName = receiverWallet.Account.Name,
            SenderName = senderWallet.Account.Name,
            CounterWalletId = receiverWallet.Id,
            Channel = _contextProvider.CurrentContext.Channel,
        };
    }

    private async Task<Wallet> GetWalletWithLockAsync(EmoneyDbContext dbContext, string walletNumber, CancellationToken cancellationToken)
    {
        try
        {
            var databaseProvider = await _databaseProviderService.GetProviderAsync();
            switch (databaseProvider)
            {
                case "MsSql":
                    {
                        return await dbContext.Wallet
                            .FromSqlRaw("SELECT * " +
                                        "FROM Core.Wallet WITH(ROWLOCK, UPDLOCK) " +
                                        "WHERE WalletNumber = {0} " +
                                        "AND RecordStatus = 'Active'", walletNumber)
                            .Include(s => s.Account)
                            .FirstOrDefaultAsync(cancellationToken);
                    }
                default:
                    {
                        return await dbContext.Wallet
                            .FromSqlRaw("SELECT * " +
                                        "FROM core.wallet " +
                                        "WHERE wallet_number = {0} " +
                                        "AND record_status = 'Active' FOR UPDATE", walletNumber)
                            .Include(s => s.Account)
                            .FirstOrDefaultAsync(cancellationToken);
                    }
            }
        }
        catch (PostgresException exception)
        {
            _logger.LogError("Record is already in progress. It will be retried! Error: {Exception}", exception);

            throw new EntityLockedException();
        }
    }
}