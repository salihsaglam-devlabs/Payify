using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.WalletModels;
using LinkPara.Emoney.Application.Commons.Strategies;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateBalance;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;
public class UpdateBalanceService : IUpdateBalanceService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDatabaseProviderService _databaseProviderService;
    private readonly ILimitService _limitService;
    private readonly IStringLocalizer _localizer;
    private readonly IContextProvider _contextProvider;
    private readonly ILogger<UpdateBalanceService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly ITierLevelService _tierLevelService;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<Wallet> _walletRepository;
    public const string SuccessCode = ExceptionPrefix.Emoney + "000";
    public const string SuccessReasonCode = "Successful";
    public UpdateBalanceService(
        IServiceScopeFactory scopeFactory,
        IDatabaseProviderService databaseProviderService,
        ILimitService limitService,
        IStringLocalizerFactory stringLocalizerFactory,
        IContextProvider contextProvider,
        ILogger<UpdateBalanceService> logger,
        IAuditLogService auditLogService,
        ITierLevelService tierLevelService,
        IParameterService parameterService,
        IGenericRepository<Wallet> walletRepository)
    {
        _scopeFactory = scopeFactory;
        _databaseProviderService = databaseProviderService;
        _limitService = limitService;
        _localizer = stringLocalizerFactory.Create("Exceptions", "LinkPara.Emoney.API");
        _contextProvider = contextProvider;
        _logger = logger;
        _auditLogService = auditLogService;
        _tierLevelService = tierLevelService;
        _parameterService = parameterService;
        _walletRepository = walletRepository;
    }
    public async Task<UpdateBalanceResponse> MaintenanceAsync(UpdateBalanceCommand command)
    {
        Wallet wallet;
        try
        {
            wallet = await GetWalletAsync(command.WalletNumber);

            var currencyCodeNumber = await GetCurrencyCodeAsync(wallet.CurrencyCode);
            ValidateStatus(wallet, command.CurrencyCode, currencyCodeNumber);
        }
        catch (Exception exception)
        {
            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
        return await ExecuteMaintenanceAsync(command, wallet);
    }
    public async Task<UpdateBalanceResponse> MoneyInAsync(UpdateBalanceCommand command)
    {
        Wallet wallet;
        try
        {
            wallet = await GetWalletAsync(command.WalletNumber);

            var currencyCodeNumber = await GetCurrencyCodeAsync(wallet.CurrencyCode);
            ValidateStatus(wallet, command.CurrencyCode, currencyCodeNumber);
        }
        catch (Exception exception)
        {
            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
        return await ExecuteMoneyInAsync(command, wallet);
    }
    public Task<UpdateBalanceResponse> ReturnAsync(UpdateBalanceCommand command)
    {
        throw new NotImplementedException();
    }
    public async Task<UpdateBalanceResponse> MoneyOutAsync(UpdateBalanceCommand command)
    {
        Wallet wallet;
        try
        {
            wallet = await GetWalletAsync(command.WalletNumber);

            var currencyCodeNumber = await GetCurrencyCodeAsync(wallet.CurrencyCode);
            ValidateStatus(wallet, command.CurrencyCode, currencyCodeNumber);
        }
        catch (Exception exception)
        {
            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
        if (command.TransactionDirection == TransactionDirection.MoneyIn)
        {
            return await ExecuteMoneyOutCancelAsync(command, wallet);
        }
        return await ExecuteMoneyOutAsync(command, wallet);
    }
    private async Task<Wallet> GetWalletAsync(string walletNumber)
    {
        var wallet = await _walletRepository
            .GetAll()
            .SingleOrDefaultAsync(x => x.WalletNumber == walletNumber);
        if (wallet is null)
        {
            throw new CustomizedWalletNotFoundException();
        }
        return wallet;
    }
    private async Task<string> GetCurrencyCodeAsync(string currencyCode)
    {
        var parameterTemplateValue = await _parameterService
            .GetAllParameterTemplateValuesAsync("Currencies", currencyCode);
        return parameterTemplateValue?.FirstOrDefault(b => b.TemplateCode == "Number")?.TemplateValue;
    }
    private static void ValidateStatus(Wallet wallet, string requestCurrencyCode, string walletCurrencyCode)
    {
        if (requestCurrencyCode != walletCurrencyCode)
        {
            throw new CurrencyCodeMismatchException();
        }
        if (wallet.RecordStatus == RecordStatus.Passive)
        {
            throw new InvalidWalletStatusException();
        }
        if (wallet.IsBlocked)
        {
            throw new WalletBlockedException();
        }
    }
    private async Task<UpdateBalanceResponse> ExecuteMoneyOutAsync(UpdateBalanceCommand command, Wallet wallet)
    {
        try
        {
            var transactionId = Guid.Empty;
            using var scope = _scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            var strategy = new NoRetryExecutionStrategy(_dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var dbWallet = await GetWalletWithLockAsync(_dbContext, wallet.Id);
                    _dbContext.Attach(dbWallet);

                    var bsmvAmount = await CalculateBsmvAmount(command.CommissionAmount, command.FeeAmount);
                    var totalAmount = command.Amount + command.CommissionAmount + command.FeeAmount + bsmvAmount;

                    ValidateBalance(dbWallet, totalAmount, command.TransactionType);
                    await ValidateLimitAsync(dbWallet, command.Amount, command.TransactionType);

                    var transaction = PopulateMoneyOutTransaction(dbWallet, command);
                    var withdrawResponse = Withdraw(dbWallet, command.Amount, command.TransactionType);
                    transaction.UsedCreditAmount = withdrawResponse.UsedCreditAmount;
                    _dbContext.Transaction.Add(transaction);

                    if (bsmvAmount > 0)
                    {
                        var commissionTransaction = PopulateCommissionTransaction(dbWallet, command, transaction.Id);
                        var commissionResponse = Withdraw(dbWallet, command.CommissionAmount + command.FeeAmount, command.TransactionType);
                        commissionTransaction.UsedCreditAmount = commissionResponse.UsedCreditAmount;
                        _dbContext.Transaction.Add(commissionTransaction);


                        var bsmvTransaction = PopulateBsmvTransaction(dbWallet, command, transaction.Id, bsmvAmount);
                        var bsmvResponse = Withdraw(dbWallet, bsmvAmount, command.TransactionType);
                        bsmvTransaction.UsedCreditAmount = bsmvResponse.UsedCreditAmount;
                        _dbContext.Transaction.Add(bsmvTransaction);
                    }

                    await IncreaseLimitUsageAsync(_dbContext, wallet, command.Amount, command.TransactionType);

                    await _dbContext.SaveChangesAsync();
                    transactionId = transaction.Id;
                    await transactionScope.CommitAsync();
                }
                catch
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "ExecuteMoneyOutAsync",
                        SourceApplication = "Emoney",
                        Resource = "Transaction",
                        Details = new Dictionary<string, string>
                        {
                            {"Id", transactionId.ToString() },
                            {"WalletNumber", command.WalletNumber },
                            {"Amount", command.Amount.ToString() }
                        }
                    });
            });
            return new UpdateBalanceResponse
            {
                ResponseCode = SuccessCode,
                ResponseReasonCode = SuccessReasonCode,
                Utid = command.Utid,
                TransactionId = transactionId.ToString()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError("MoneyOut Transaction Error: {Exception}", exception);

            await _auditLogService.AuditLogAsync(
                 new AuditLog
                 {
                     IsSuccess = false,
                     LogDate = DateTime.Now,
                     Operation = "ExecuteMoneyOutAsync",
                     SourceApplication = "Emoney",
                     Resource = "Transaction",
                     Details = new Dictionary<string, string>
                     {
                          {"Utid", command.Utid },
                          {"WalletNumber", command.WalletNumber },
                          {"Amount", command.Amount.ToString() },
                          {"Message", exception.Message }
                     }
                 });

            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
    }
    private async Task<UpdateBalanceResponse> ExecuteMoneyInAsync(UpdateBalanceCommand command, Wallet wallet)
    {
        try
        {
            var transactionId = Guid.Empty;
            using var scope = _scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            var strategy = new NoRetryExecutionStrategy(_dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var dbWallet = await GetWalletWithLockAsync(_dbContext, wallet.Id);
                    _dbContext.Attach(dbWallet);

                    var bsmvAmount = await CalculateBsmvAmount(command.CommissionAmount, command.FeeAmount);

                    await ValidateLimitAsync(dbWallet, command.Amount, command.TransactionType);

                    var transaction = PopulateMoneyInTransaction(dbWallet, command);
                    _dbContext.Transaction.Add(transaction);

                    Deposit(dbWallet, command.Amount);

                    if (bsmvAmount > 0)
                    {
                        var commissionTransaction = PopulateCommissionTransaction(dbWallet, command, transaction.Id);
                        _dbContext.Transaction.Add(commissionTransaction);

                        WithdrawDepositFee(dbWallet, command.CommissionAmount + command.FeeAmount);

                        var bsmvTransaction = PopulateBsmvTransaction(dbWallet, command, transaction.Id, bsmvAmount);
                        _dbContext.Transaction.Add(bsmvTransaction);

                        WithdrawDepositFee(dbWallet, bsmvAmount);
                    }

                    await IncreaseLimitUsageAsync(_dbContext, wallet, command.Amount, command.TransactionType);

                    await _dbContext.SaveChangesAsync();
                    transactionId = transaction.Id;
                    await transactionScope.CommitAsync();
                }
                catch
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "ExecuteMoneyInAsync",
                        SourceApplication = "Emoney",
                        Resource = "Transaction",
                        Details = new Dictionary<string, string>
                        {
                            {"Id", transactionId.ToString() },
                            {"WalletNumber", command.WalletNumber },
                            {"Amount", command.Amount.ToString() }
                        }
                    });
            });
            return new UpdateBalanceResponse
            {
                ResponseCode = SuccessCode,
                ResponseReasonCode = SuccessReasonCode,
                Utid = command.Utid,
                TransactionId = transactionId.ToString()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError("MoneyIn Transaction Error: {Exception}", exception);

            await _auditLogService.AuditLogAsync(
                 new AuditLog
                 {
                     IsSuccess = false,
                     LogDate = DateTime.Now,
                     Operation = "ExecuteMoneyInAsync",
                     SourceApplication = "Emoney",
                     Resource = "Transaction",
                     Details = new Dictionary<string, string>
                     {
                          {"Utid", command.Utid },
                          {"WalletNumber", command.WalletNumber },
                          {"Amount", command.Amount.ToString() },
                          {"Message", exception.Message }
                     }
                 });

            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
    }
    private async Task<UpdateBalanceResponse> ExecuteMaintenanceAsync(UpdateBalanceCommand command, Wallet wallet)
    {
        try
        {
            var transactionId = Guid.Empty;
            using var scope = _scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            var strategy = new NoRetryExecutionStrategy(_dbContext);
            var isCommissionFee = command.Amount == 0;

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var dbWallet = await GetWalletWithLockAsync(_dbContext, wallet.Id);
                    _dbContext.Attach(dbWallet);

                    var bsmvAmount = await CalculateBsmvAmount(command.CommissionAmount, command.FeeAmount);
                    Transaction transaction;

                    if (command.TransactionDirection == TransactionDirection.MoneyIn)
                    {
                        transaction = DepositMaintenance(command, _dbContext, isCommissionFee, dbWallet, bsmvAmount);
                    }
                    else
                    {
                        var totalAmount = command.Amount + command.CommissionAmount + command.FeeAmount + bsmvAmount;
                        ValidateBalance(dbWallet, totalAmount, TransactionType.Withdraw);
                        transaction = WithdrawMaintenance(command, _dbContext, isCommissionFee, dbWallet, bsmvAmount);
                    }

                    await _dbContext.SaveChangesAsync();
                    transactionId = transaction.Id;
                    await transactionScope.CommitAsync();
                }
                catch
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "ExecuteMaintenanceAsync",
                        SourceApplication = "Emoney",
                        Resource = "Transaction",
                        Details = new Dictionary<string, string>
                        {
                            {"Id", transactionId.ToString() },
                            {"WalletNumber", command.WalletNumber },
                            {"Amount", command.Amount.ToString() }
                        }
                    });
            });
            return new UpdateBalanceResponse
            {
                ResponseCode = SuccessCode,
                ResponseReasonCode = SuccessReasonCode,
                Utid = command.Utid,
                TransactionId = transactionId.ToString()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError("Maintenance Transaction Error: {Exception}", exception);

            await _auditLogService.AuditLogAsync(
                 new AuditLog
                 {
                     IsSuccess = false,
                     LogDate = DateTime.Now,
                     Operation = "ExecuteMaintenanceAsync",
                     SourceApplication = "Emoney",
                     Resource = "Transaction",
                     Details = new Dictionary<string, string>
                     {
                          {"Utid", command.Utid },
                          {"WalletNumber", command.WalletNumber },
                          {"Amount", command.Amount.ToString() },
                          {"Message", exception.Message }
                     }
                 });

            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
    }
    private async Task<UpdateBalanceResponse> ExecuteMoneyOutCancelAsync(UpdateBalanceCommand command, Wallet wallet)
    {
        try
        {
            var transactionId = Guid.Empty;
            using var scope = _scopeFactory.CreateScope();
            var _dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();
            var strategy = new NoRetryExecutionStrategy(_dbContext);

            await strategy.ExecuteAsync(async () =>
            {
                await using var transactionScope = await _dbContext.Database.BeginTransactionAsync();
                try
                {
                    var dbWallet = await GetWalletWithLockAsync(_dbContext, wallet.Id);
                    _dbContext.Attach(dbWallet);

                    var mainTransactions = _dbContext.Transaction
                    .Where(x => x.CustomerTransactionId == command.Utid)
                    .ToList();

                    if (mainTransactions.Count == 0)
                    {
                        throw new NotFoundException($"ExecuteMoneyOutCancelAsyncException Utid = {command.Utid}");
                    }
                    if (mainTransactions.Any(x => x.IsCancelled == true))
                    {
                        throw new AlreadyProcessedException();
                    }

                    mainTransactions.ForEach(x => x.IsCancelled = true);
                    _dbContext.UpdateRange(mainTransactions);

                    var totalAmount = ReturnWithdrawAmount(mainTransactions, dbWallet, command.TransactionType);
                    command.Amount = totalAmount;
                    var transaction = PopulateMoneyInTransaction(wallet, command, TransactionType.Cancel);
                    _dbContext.Transaction.Add(transaction);

                    await DecreaseLimitUsageAsync(_dbContext, wallet, totalAmount, command.TransactionType);

                    await _dbContext.SaveChangesAsync();
                    transactionId = transaction.Id;
                    await transactionScope.CommitAsync();
                }
                catch
                {
                    await transactionScope.RollbackAsync();
                    throw;
                }

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "ExecuteMoneyOutCancelAsync",
                        SourceApplication = "Emoney",
                        Resource = "Transaction",
                        Details = new Dictionary<string, string>
                        {
                            {"Id", transactionId.ToString() },
                            {"WalletNumber", command.WalletNumber },
                            {"Amount", command.Amount.ToString() }
                        }
                    });
            });
            return new UpdateBalanceResponse
            {
                ResponseCode = SuccessCode,
                ResponseReasonCode = SuccessReasonCode,
                Utid = command.Utid,
                TransactionId = transactionId.ToString()
            };
        }
        catch (Exception exception)
        {
            _logger.LogError("MoneyOut Cancel Transaction Error: {Exception}", exception);

            await _auditLogService.AuditLogAsync(
                 new AuditLog
                 {
                     IsSuccess = false,
                     LogDate = DateTime.Now,
                     Operation = "ExecuteMoneyOutCancelAsync",
                     SourceApplication = "Emoney",
                     Resource = "Transaction",
                     Details = new Dictionary<string, string>
                     {
                          {"Utid", command.Utid },
                          {"WalletNumber", command.WalletNumber },
                          {"Amount", command.Amount.ToString() },
                          {"Message", exception.Message }
                     }
                 });

            if (exception is ApiException apiException)
            {
                return new UpdateBalanceResponse
                {
                    ResponseCode = apiException.Code,
                    ResponseReasonCode = apiException.Message,
                    Utid = command.Utid
                };
            }
            throw;
        }
    }
    private Transaction WithdrawMaintenance(UpdateBalanceCommand command, EmoneyDbContext _dbContext, bool isCommissionFee, Wallet dbWallet, decimal bsmvAmount)
    {
        Transaction transaction;
        if (isCommissionFee)
        {
            command.Amount = command.CommissionAmount + command.FeeAmount + bsmvAmount;
        }

        transaction = PopulateMoneyOutTransaction(dbWallet, command);
        Withdraw(dbWallet, command.Amount, TransactionType.Withdraw);
        _dbContext.Transaction.Add(transaction);

        if (bsmvAmount > 0 && !isCommissionFee)
        {
            var commissionTransaction = PopulateCommissionTransaction(dbWallet, command, transaction.Id);
            Withdraw(dbWallet, command.CommissionAmount + command.FeeAmount, command.TransactionType);
            _dbContext.Transaction.Add(commissionTransaction);


            var bsmvTransaction = PopulateBsmvTransaction(dbWallet, command, transaction.Id, bsmvAmount);
            Withdraw(dbWallet, bsmvAmount, command.TransactionType);
            _dbContext.Transaction.Add(bsmvTransaction);
        }
        return transaction;
    }
    private Transaction DepositMaintenance(UpdateBalanceCommand command, EmoneyDbContext _dbContext, bool isCommissionFee, Wallet dbWallet, decimal bsmvAmount)
    {
        Transaction transaction;
        if (isCommissionFee)
        {
            command.Amount = command.CommissionAmount + command.FeeAmount + bsmvAmount;
        }

        transaction = PopulateMoneyInTransaction(dbWallet, command);
        _dbContext.Transaction.Add(transaction);
        Deposit(dbWallet, command.Amount);

        if (bsmvAmount > 0 && !isCommissionFee)
        {
            var commissionTransaction = PopulateCommissionTransaction(dbWallet, command, transaction.Id);
            _dbContext.Transaction.Add(commissionTransaction);
            WithdrawDepositFee(dbWallet, command.CommissionAmount + command.FeeAmount);

            var bsmvTransaction = PopulateBsmvTransaction(dbWallet, command, transaction.Id, bsmvAmount);
            _dbContext.Transaction.Add(bsmvTransaction);
            WithdrawDepositFee(dbWallet, bsmvAmount);
        }
        return transaction;
    }
    private async Task<decimal> CalculateBsmvAmount(decimal commission, decimal fee)
    {
        if (commission + fee > 0)
        {
            var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
            var bsmvRate = Convert.ToDecimal(bsmvRateParameter?.ParameterValue);
            var bsmvAmount = ((fee + commission) * (bsmvRate / 100m)).ToDecimal2();
            return bsmvAmount > 0 ? bsmvAmount : 0.01m;
        }
        return 0;
    }
    private Transaction PopulateBsmvTransaction(Wallet wallet, UpdateBalanceCommand command, Guid transactionId, decimal bsmvAmount)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Tax,
            TransactionStatus = TransactionStatus.Completed,
            Tag = command.Channel,
            TagTitle = TransactionType.Tax.ToString(),
            Amount = bsmvAmount,
            CurrencyCode = wallet.CurrencyCode,
            Description = command.Description,
            WalletId = wallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = wallet.AvailableBalance - bsmvAmount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = command.TransactionDate,
            ExternalTransactionDate = command.TransactionDate,
            PaymentMethod = PaymentMethod.Prepaid,
            RecordStatus = RecordStatus.Active,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            CustomerTransactionId = command.Utid,
            PaymentChannel = command.Channel,
            RelatedTransactionId = transactionId
        };
    }
    private Transaction PopulateCommissionTransaction(Wallet wallet, UpdateBalanceCommand command, Guid transactionId)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = TransactionType.Commission,
            TransactionStatus = TransactionStatus.Completed,
            Tag = command.Channel,
            TagTitle = TransactionType.Commission.ToString(),
            Amount = command.CommissionAmount + command.FeeAmount,
            CurrencyCode = wallet.CurrencyCode,
            Description = command.Description,
            WalletId = wallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = wallet.AvailableBalance - (command.CommissionAmount + command.FeeAmount),
            PreBalance = wallet.AvailableBalance,
            TransactionDate = command.TransactionDate,
            ExternalTransactionDate = command.TransactionDate,
            PaymentMethod = PaymentMethod.Prepaid,
            RecordStatus = RecordStatus.Active,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            CustomerTransactionId = command.Utid,
            PaymentChannel = command.Channel,
            RelatedTransactionId = transactionId
        };
    }
    private Transaction PopulateMoneyOutTransaction(Wallet wallet, UpdateBalanceCommand command)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyOut,
            TransactionType = command.TransactionType,
            TransactionStatus = TransactionStatus.Completed,
            Tag = command.Channel,
            TagTitle = command.TransactionType.ToString(),
            Amount = command.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = command.Description,
            WalletId = wallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = wallet.AvailableBalance - command.Amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = command.TransactionDate,
            ExternalTransactionDate = command.TransactionDate,
            PaymentMethod = PaymentMethod.Prepaid,
            RecordStatus = RecordStatus.Active,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            CustomerTransactionId = command.Utid,
            PaymentChannel = command.Channel
        };
    }
    private Transaction PopulateMoneyInTransaction(Wallet wallet, UpdateBalanceCommand command, TransactionType? transactionType = null)
    {
        return new Transaction
        {
            TransactionDirection = TransactionDirection.MoneyIn,
            TransactionType = transactionType ?? command.TransactionType,
            TransactionStatus = TransactionStatus.Completed,
            Tag = command.Channel,
            TagTitle = command.TransactionType.ToString(),
            Amount = command.Amount,
            CurrencyCode = wallet.CurrencyCode,
            Description = command.Description,
            WalletId = wallet.Id,
            CreatedBy = _contextProvider.CurrentContext.UserId,
            CurrentBalance = wallet.AvailableBalance + command.Amount,
            PreBalance = wallet.AvailableBalance,
            TransactionDate = command.TransactionDate,
            ExternalTransactionDate = command.TransactionDate,
            PaymentMethod = PaymentMethod.Prepaid,
            RecordStatus = RecordStatus.Active,
            Channel = _contextProvider.CurrentContext?.Channel,
            IpAddress = _contextProvider.CurrentContext?.ClientIpAddress ?? string.Empty,
            CustomerTransactionId = command.Utid,
            PaymentChannel = command.Channel
        };
    }
    private static void ValidateBalance(Wallet dbWallet, decimal totalAmount, TransactionType transactionType)
    {
        if (transactionType is TransactionType.Withdraw)
        {
            if (dbWallet.CurrentBalanceCash < totalAmount)
            {
                throw new InsufficientBalanceException();
            }
        }
        else
        {
            if (dbWallet.AvailableBalance < totalAmount)
            {
                throw new InsufficientBalanceException();
            }
        }
    }
    private WithdrawResponse Withdraw(Wallet wallet, decimal amount, TransactionType transactionType)
    {
        wallet.LastActivityDate = DateTime.Now;
        if (transactionType is TransactionType.Withdraw)
        {
            wallet.CurrentBalanceCash -= amount;
            return new WithdrawResponse { UsedCreditAmount = 0 };
        }
        else
        {
            if (amount <= wallet.CurrentBalanceCredit)
            {
                wallet.CurrentBalanceCredit -= amount;
                return new WithdrawResponse { UsedCreditAmount = amount };
            }
            else
            {
                var difference = amount - wallet.CurrentBalanceCredit;
                wallet.CurrentBalanceCredit = 0;
                wallet.CurrentBalanceCash -= difference;
                return new WithdrawResponse { UsedCreditAmount = wallet.CurrentBalanceCredit };
            }
        }
    }
    private static void Deposit(Wallet wallet, decimal amount)
    {
        wallet.CurrentBalanceCash += amount;
        wallet.LastActivityDate = DateTime.Now;
    }
    private static void WithdrawDepositFee(Wallet wallet, decimal amount)
    {
        wallet.CurrentBalanceCash -= amount;
        wallet.LastActivityDate = DateTime.Now;
    }
    private decimal ReturnWithdrawAmount(List<Transaction> transactions, Wallet wallet, TransactionType transactionType)
    {
        wallet.LastActivityDate = DateTime.Now;
        var totalAmount = transactions.Sum(x => x.Amount);
        if (transactionType == TransactionType.Sale)
        {
            var creditAmount = transactions.Sum(x => x.UsedCreditAmount);
            wallet.CurrentBalanceCredit += creditAmount;
            if (totalAmount > creditAmount)
            {
                wallet.CurrentBalanceCash += totalAmount - creditAmount;
            }
        }
        else
        {
            wallet.CurrentBalanceCash += totalAmount;
        }
        return totalAmount;
    }
    private async Task<Wallet> GetWalletWithLockAsync(EmoneyDbContext dbContext, Guid senderWalletId)
    {
        var databaseProvider = await _databaseProviderService.GetProviderAsync();
        switch (databaseProvider)
        {
            case "MsSql":
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM Core.Wallet WITH(ROWLOCK, UPDLOCK) " +
                                    "WHERE Id = {0} " +
                                    "AND RecordStatus = 'Active'", senderWalletId)
                        .FirstOrDefaultAsync();
                }
            default:
                {
                    return await dbContext.Wallet
                        .FromSqlRaw("SELECT * " +
                                    "FROM core.wallet " +
                                    "WHERE id = {0} " +
                                    "AND record_status = 'Active' FOR UPDATE", senderWalletId)
                        .FirstOrDefaultAsync();
                }
        }
    }
    private async Task ValidateLimitAsync(Wallet wallet, decimal requestAmount, TransactionType transactionType)
    {
        var limitControlRequest = new LimitControlRequest
        {
            WalletNumber = wallet.WalletNumber,
            CurrencyCode = wallet.CurrencyCode,
            Amount = requestAmount,
            LimitOperationType = GetLimitOperationType(transactionType),
            AccountId = wallet.AccountId
        };
        await CheckLimitAsync(limitControlRequest);
    }
    private async Task CheckLimitAsync(LimitControlRequest limitControlRequest)
    {
        var limitResponse = await _limitService.IsLimitExceededAsync(limitControlRequest);

        if (limitResponse.IsLimitExceeded)
        {
            throw new CustomApiException(ApiErrorCode.LimitExceeded,
                _localizer.GetString($"{limitResponse.LimitOperationType}LimitExceededException"));
        }
    }
    private LimitOperationType GetLimitOperationType(TransactionType transactionType)
    {
        return transactionType switch
        {
            TransactionType.Deposit => LimitOperationType.Deposit,
            TransactionType.Sale => LimitOperationType.Withdrawal,
            TransactionType.Withdraw => LimitOperationType.Withdrawal,
            _ => throw new ArgumentOutOfRangeException(nameof(transactionType)),
        };
    }
    private async Task IncreaseLimitUsageAsync(
    EmoneyDbContext emoneyDbContext, Wallet wallet, decimal amount, TransactionType transactionType)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == wallet.AccountId
                                       && x.CurrencyCode == wallet.CurrencyCode);

        if (existingLevel is null)
        {
            var level = await _tierLevelService.PopulateInitialLevelAsync(
                wallet.CurrencyCode,
                wallet.AccountId,
                Guid.Parse(_contextProvider.CurrentContext.UserId));

            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = wallet.AccountId,
                LimitOperationType = GetLimitOperationType(transactionType),
                Amount = amount,
                CurrencyCode = wallet.CurrencyCode,
                WalletType = wallet.WalletType
            }, level);
            emoneyDbContext.Add(level);
        }
        else
        {
            await _limitService.IncreaseUsageAsync(new AccountLimitUpdateRequest
            {
                AccountId = wallet.AccountId,
                LimitOperationType = GetLimitOperationType(transactionType),
                Amount = amount,
                CurrencyCode = wallet.CurrencyCode,
                WalletType = wallet.WalletType
            }, existingLevel);
            emoneyDbContext.Update(existingLevel);
        }
    }
    private async Task DecreaseLimitUsageAsync(EmoneyDbContext emoneyDbContext, Wallet wallet, decimal amount, TransactionType transactionType)
    {
        var existingLevel = await emoneyDbContext.AccountCurrentLevel
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == wallet.AccountId &&
            x.CurrencyCode == wallet.CurrencyCode);

        await _limitService.DecreaseUsageAsync(new AccountLimitUpdateRequest
        {
            AccountId = wallet.AccountId,
            LimitOperationType = GetLimitOperationType(transactionType),
            Amount = amount,
            CurrencyCode = wallet.CurrencyCode,
            WalletType = wallet.WalletType
        }, existingLevel);
    }
}

