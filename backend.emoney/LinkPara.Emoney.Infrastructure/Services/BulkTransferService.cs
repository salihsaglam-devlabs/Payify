using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.BulkTransfers;
using LinkPara.Emoney.Application.Features.BulkTransfers.Commands.ApproveBulkTransfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.Transfer;
using LinkPara.Emoney.Application.Features.Wallets.Commands.WithdrawRequests;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.DbProvider;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Transactions;
using Entities = LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Infrastructure.Services;

public class BulkTransferService : IBulkTransferService
{
    private readonly IContextProvider _contextProvider;
    private readonly IAccountService _accountService;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkTransferService> _logger;
    private readonly ITransferService _transferService;
    private readonly IIbanBlacklistService _ibanBlacklistService;
    private readonly IGenericRepository<Entities.Transaction> _transactionRepository;
    private readonly IBus _bus;
    private readonly IDatabaseProviderService _databaseProviderService;

    public BulkTransferService(IContextProvider contextProvider,
        IAccountService accountService,
        IServiceScopeFactory scopeFactory,
        ILogger<BulkTransferService> logger,
        ITransferService transferService,
        IIbanBlacklistService ibanBlacklistService,
        IGenericRepository<Entities.Transaction> transactionRepository,
        IBus bus,
        IDatabaseProviderService databaseProviderService)
    {
        _contextProvider = contextProvider;
        _accountService = accountService;
        _scopeFactory = scopeFactory;
        _logger = logger;
        _transferService = transferService;
        _ibanBlacklistService = ibanBlacklistService;
        _transactionRepository = transactionRepository;
        _bus = bus;
        _databaseProviderService = databaseProviderService;
    }

    public async Task ActionBulkTransferAsync(ActionBulkTransferCommand request, CancellationToken cancellationToken)
    {
        var updatedUserId = _contextProvider.CurrentContext.UserId;

        var accountUser = await _accountService.GetCorporateAccountUserAsync(Guid.Parse(updatedUserId));

        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var strategy = dbContext.Database.CreateExecutionStrategy();


        var bulkTransfer = await GetBulkTransferWithLockAsync(dbContext, request.BulkTransferId, accountUser.AccountId);

        if (bulkTransfer is null)
        {
            throw new NotFoundException(nameof(BulkTransfer), request.BulkTransferId);
        }

        if (bulkTransfer.BulkTransferStatus != BulkTransferStatus.Waiting)
        {
            throw new AlreadyProcessedException();
        }

        if (!request.IsApprove)
        {

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                bulkTransfer.BulkTransferStatus = BulkTransferStatus.Rejected;

                dbContext.BulkTransfer.Update(bulkTransfer);
                await dbContext.SaveChangesAsync();
                scope.Complete();
            });
            return;
        }
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            bulkTransfer.BulkTransferStatus = BulkTransferStatus.Processing;

            dbContext.BulkTransfer.Update(bulkTransfer);
            await dbContext.SaveChangesAsync();
            scope.Complete();
        });


        await SendBulkTransferRequestAsync(new BulkTransferRequest { BulkTransferId = bulkTransfer.Id, UserId = Guid.Parse(updatedUserId) });

    }

    private async Task InternalTransferAsync(EmoneyDbContext dbContext, BulkTransfer bulkTransfer, string userId, CancellationToken cancellationToken)
    {
        var bulkTransferDetails = bulkTransfer.BulkTransferDetails;

        foreach (var item in bulkTransferDetails)
        {
            try
            {
                var newTransferRequest = new TransferCommand
                {
                    Amount = item.Amount,
                    Description = item.Description,
                    ReceiverWalletNumber = item.Receiver,
                    SenderWalletNumber = bulkTransfer.SenderWalletNumber,
                    UserId = userId
                };

                var transferResponse = await _transferService.TransferAsync(newTransferRequest, cancellationToken);

                if (transferResponse.Success)
                {
                    item.TransactionId = transferResponse.TransactionId;
                    item.BulkTransferDetailStatus = BulkTransferDetailStatus.Success;

                    var transactions = await _transactionRepository
                        .GetAll()
                        .Where(x => x.RelatedTransactionId == item.TransactionId)
                        .ToListAsync();

                    if (transactions.Any(x => x.TransactionType == TransactionType.Tax))
                    {
                        item.BsmvAmount = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Tax).Amount;
                    }

                    if (transactions.Any(x => x.TransactionType == TransactionType.Commission))
                    {
                        item.CommissionAmount = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Commission).Amount;
                    }
                }
                else
                {
                    item.BulkTransferDetailStatus = BulkTransferDetailStatus.Failed;
                    item.ExceptionMessage = transferResponse.ErrorMessage;
                }
            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception on InternalTransfer BulkTransfer : \n{exception}");

                if (exception is ForbiddenAccessException)
                {
                    item.ExceptionMessage = "ForbiddenAccessException";
                }
                else if (exception is InvalidWalletStatusException)
                {
                    item.ExceptionMessage = "InvalidWalletStatusException";
                }
                else if (exception is InvalidTierPermissionException)
                {
                    item.ExceptionMessage = "InvalidTierPermissionException";
                }
                else if (exception is PotentialFraudException)
                {
                    item.ExceptionMessage = "PotentialFraudException";
                }
                else if (exception is NotFoundException)
                {
                    item.ExceptionMessage = "UnknownException";
                }
                else if (exception is CurrencyCodeMismatchException)
                {
                    item.ExceptionMessage = "CurrencyCodeMismatchException";
                }
                else if (exception is InsufficientBalanceException)
                {
                    item.ExceptionMessage = "InsufficientBalanceException";
                }
                else if (exception is CustomApiException && ((CustomApiException)exception).Code == ApiErrorCode.LimitExceeded)
                {
                    item.ExceptionMessage = "LimitExceededException";
                }
                else
                {
                    item.ExceptionMessage = "UnknownException";
                }

                item.BulkTransferDetailStatus = BulkTransferDetailStatus.Failed;
            }
        }

        BulkTransferStatus bulkTransferStatus = CalculateTransferBulkTransferStatus(bulkTransferDetails);
        bulkTransfer.BulkTransferStatus = bulkTransferStatus;

    }

    private async Task<BulkTransfer> GetBulkTransferWithLockAsync(EmoneyDbContext dbContext, Guid bulkTransferId, Guid accountId)
    {
        try
        {
            var databaseProvider = await _databaseProviderService.GetProviderAsync();
            switch (databaseProvider)
            {
                case "MsSql":
                    {
                        return await dbContext.BulkTransfer
                            .FromSqlRaw("SELECT * " +
                                        "FROM Core.BulkTransfer WITH (UPDLOCK, ROWLOCK) " +
                                        "WHERE Id = {0} AND AccountId = {1} " +
                                        "AND RecordStatus = 'Active'", bulkTransferId, accountId)
                            .Include(x => x.BulkTransferDetails)
                            .FirstOrDefaultAsync();
                    }
                default:
                    {
                        return await dbContext.BulkTransfer
                            .FromSqlRaw("SELECT * " +
                                        "FROM core.bulk_transfer " +
                                        "WHERE id = {0} and account_id = {1} " +
                                        "AND record_status = 'Active' FOR UPDATE", bulkTransferId, accountId)
                            .Include(x => x.BulkTransferDetails)
                            .FirstOrDefaultAsync();
                    }
            }
        }
        catch (PostgresException exception)
        {
            _logger.LogError("Record is already in progress. It will be retried! Error: {Exception}", exception);

            throw new EntityLockedException();
        }
    }

    private async Task WithdrawTransferAsync(EmoneyDbContext dbContext, BulkTransfer bulkTransfer, Guid userId, CancellationToken cancellationToken)
    {
        var bulkTransferDetails = bulkTransfer.BulkTransferDetails;

        foreach (var item in bulkTransferDetails)
        {
            try
            {
                var newWithdrawRequest = new WithdrawRequestCommand
                {
                    Amount = item.Amount,
                    Description = item.Description,
                    ReceiverIBAN = item.Receiver,
                    ReceiverName = item.FullName,
                    WalletNumber = bulkTransfer.SenderWalletNumber,
                    UserId = userId
                };

                var ibanBlacklisted = await _ibanBlacklistService.IsBlacklistedAsync(newWithdrawRequest.ReceiverIBAN);

                if (ibanBlacklisted)
                {
                    throw new IbanBlacklistedException(newWithdrawRequest.ReceiverIBAN);
                }

                var withdraw = await _transferService.WithdrawAsync(newWithdrawRequest, cancellationToken);

                if (!withdraw.Success)
                {
                    throw new InitiateWithdrawException();
                }
                item.TransactionId = withdraw.TransactionId;
                item.BulkTransferDetailStatus = BulkTransferDetailStatus.WaitingMoneyTransfer;

                var transactions = await _transactionRepository
                    .GetAll()
                    .Where(x => x.RelatedTransactionId == item.TransactionId)
                    .ToListAsync();

                if (transactions.Any(x => x.TransactionType == TransactionType.Tax))
                {
                    item.BsmvAmount = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Tax).Amount;
                }

                if (transactions.Any(x => x.TransactionType == TransactionType.Commission))
                {
                    item.CommissionAmount = transactions.FirstOrDefault(x => x.TransactionType == TransactionType.Commission).Amount;
                }

            }
            catch (Exception exception)
            {
                _logger.LogError($"Exception on Withdraw BulkTransfer : \n{exception}");

                if (exception is InitiateWithdrawException)
                {
                    item.ExceptionMessage = "InitiateWithdrawException";
                }
                else if (exception is ForbiddenAccessException)
                {
                    item.ExceptionMessage = "ForbiddenAccessException";
                }
                else if (exception is IbanBlacklistedException)
                {
                    item.ExceptionMessage = "IbanBlacklistedException";
                }
                else if (exception is InvalidWalletStatusException)
                {
                    item.ExceptionMessage = "InvalidWalletStatusException";
                }
                else if (exception is InvalidIbanException)
                {
                    item.ExceptionMessage = "InvalidIbanException";
                }
                else if (exception is InvalidTierPermissionException)
                {
                    item.ExceptionMessage = "InvalidTierPermissionException";
                }
                else if (exception is MoneyTransferOutsideEftHoursException)
                {
                    item.ExceptionMessage = "MoneyTransferOutsideEftHoursException";
                }
                else if (exception is PotentialFraudException)
                {
                    item.ExceptionMessage = "PotentialFraudException";
                }
                else if (exception is CustomApiException && ((CustomApiException)exception).Code == ApiErrorCode.LimitExceeded)
                {
                    item.ExceptionMessage = "LimitExceededException";
                }
                else if (exception is NotFoundException)
                {
                    item.ExceptionMessage = "NotFoundException";
                }
                else if (exception is CurrencyCodeMismatchException)
                {
                    item.ExceptionMessage = "CurrencyCodeMismatchException";
                }
                else if (exception is InsufficientBalanceException)
                {
                    item.ExceptionMessage = "InsufficientBalanceException";
                }
                else
                {
                    item.ExceptionMessage = "UnknownException";
                }

                item.BulkTransferDetailStatus = BulkTransferDetailStatus.Failed;
            }
        }

        BulkTransferStatus bulkTransferStatus = CalculateWithdrawBulkTransferStatus(bulkTransferDetails);
        bulkTransfer.BulkTransferStatus = bulkTransferStatus;
    }

    private BulkTransferStatus CalculateWithdrawBulkTransferStatus(List<BulkTransferDetail> bulkTransferDetails)
    {
        if (bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Failed)
            && bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.WaitingMoneyTransfer))
        {
            return BulkTransferStatus.PartialFailed;
        }
        else if (!bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.WaitingMoneyTransfer))
        {
            return BulkTransferStatus.Failed;
        }
        else if (!bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Failed))
        {
            return BulkTransferStatus.WaitingMoneyTransfer;
        }

        throw new InvalidOperationException();
    }

    private BulkTransferStatus CalculateTransferBulkTransferStatus(List<BulkTransferDetail> bulkTransferDetails)
    {
        if (bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Failed
                                      || x.BulkTransferDetailStatus == BulkTransferDetailStatus.Returned)
            && bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Success))
        {
            return BulkTransferStatus.PartialFailed;
        }
        else if (!bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Success))
        {
            return BulkTransferStatus.Failed;
        }
        else if (!bulkTransferDetails.Any(x => x.BulkTransferDetailStatus == BulkTransferDetailStatus.Failed))
        {
            return BulkTransferStatus.Success;
        }

        throw new InvalidOperationException();
    }

    public async Task SendWithdrawBulkTransferAsync(CheckWithdrawBulkTransferRequest request)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.CheckWithdrawBulkTransfer"));
            await endpoint.Send(request, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

    private async Task SendBulkTransferRequestAsync(BulkTransferRequest bulkTransferRequest)
    {
        try
        {
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.BulkTransferRequest"));
            await endpoint.Send(bulkTransferRequest, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

    public async Task CheckWithdrawBulkTransferAsync(CheckWithdrawBulkTransferRequest request)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();

        var bulkTransferDetail = await dbContext.BulkTransferDetail
           .FirstOrDefaultAsync(s => s.TransactionId == request.TransactionId);

        if (bulkTransferDetail is null)
        {
            return;
        }

        dbContext.BulkTransferDetail.Attach(bulkTransferDetail);

        bulkTransferDetail.BulkTransferDetailStatus = request.TransactionResult switch
        {
            TransactionResult.Success => BulkTransferDetailStatus.Success,
            TransactionResult.Failed => BulkTransferDetailStatus.Failed,
            TransactionResult.Returned => BulkTransferDetailStatus.Returned,
            _ => throw new ArgumentOutOfRangeException(nameof(request.TransactionResult))
        };

        dbContext.SaveChanges();

        var strategy = dbContext.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {

            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var bulkTransferDetails = await GetBulkTransferDetailsWithLockAsync(dbContext, bulkTransferDetail.BulkTransferId);

            var bulkTransferStatus = CalculateTransferBulkTransferStatus(bulkTransferDetails);

            var bulkTransfer = bulkTransferDetails.FirstOrDefault().BulkTransfer;

            dbContext.BulkTransfer.Attach(bulkTransfer);

            bulkTransfer.BulkTransferStatus = bulkTransferStatus;

            dbContext.SaveChanges();
            scope.Complete();
        });
    }

    private async Task<List<BulkTransferDetail>> GetBulkTransferDetailsWithLockAsync(EmoneyDbContext dbContext, Guid bulkTransferId)
    {
        try
        {
            var databaseProvider = await _databaseProviderService.GetProviderAsync();
            switch (databaseProvider)
            {
                case "MsSql":
                    {
                        return await dbContext.BulkTransferDetail
                            .FromSqlRaw("SELECT * " +
                                        "FROM Core.BulkTransferDetail WITH (UPDLOCK, ROWLOCK) " +
                                        "WHERE BulkTransferId = {0} " +
                                        "AND RecordStatus = 'Active'", bulkTransferId)
                            .Include(x => x.BulkTransfer)
                            .ToListAsync();
                    }
                default:
                    {
                        return await dbContext.BulkTransferDetail
                            .FromSqlRaw("SELECT * " +
                                        "FROM core.bulk_transfer_detail " +
                                        "WHERE bulk_transfer_id = {0} " +
                                        "AND record_status = 'Active' FOR UPDATE", bulkTransferId)
                            .Include(x => x.BulkTransfer)
                            .ToListAsync();
                    }
            }
        }
        catch (PostgresException exception)
        {
            _logger.LogError($"Record is already in progress. It will be retried! Error: {exception}");
            throw new EntityLockedException();
        }
    }

    public async Task BulkTransferAsync(Guid bulkTransferId, Guid userId, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<EmoneyDbContext>();


        var accountUser = await _accountService.GetCorporateAccountUserAsync(userId);

        var bulkTransfer = await GetBulkTransferWithLockAsync(dbContext, bulkTransferId, accountUser.AccountId);

        if (bulkTransfer is null)
        {
            throw new NotFoundException(nameof(bulkTransfer), bulkTransferId);
        }

        await _transferService.ValidateUserAndSenderUser(bulkTransfer.SenderWalletNumber, userId);

        if (bulkTransfer.BulkTransferType == BulkTransferType.Internal)
        {
            await InternalTransferAsync(dbContext, bulkTransfer, userId.ToString(), cancellationToken);
        }
        else if (bulkTransfer.BulkTransferType == BulkTransferType.Withdraw)
        {
            await WithdrawTransferAsync(dbContext, bulkTransfer, userId, cancellationToken);
        }

        var strategy = dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            dbContext.BulkTransfer.Update(bulkTransfer);
            await dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }
}
