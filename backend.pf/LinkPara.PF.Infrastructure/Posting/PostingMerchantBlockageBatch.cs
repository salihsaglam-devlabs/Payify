using System.Transactions;
using LinkPara.Cache;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Calendar;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Posting;

public class PostingMerchantBlockageBatch : IPostingBatch<PostingMerchantBlockage>
{
    private readonly ILogger<PostingMerchantBlockageBatch> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantBlockage> _merchantBlockageRepository;
    private readonly IGenericRepository<PostingTransaction> _postingTransactionRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    
    private decimal _blockageThresholdRate;
    private int _blockagePeriodMonths;

    private const int TransactionPerBatch = 1000;
    private const int BlockagePeriodMonths = 6;
    private const decimal BlockageThresholdRate = 0.1m;

    public PostingMerchantBlockageBatch(
        ILogger<PostingMerchantBlockageBatch> logger,
        PfDbContext dbContext,
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IApplicationUserService applicationUserService,
        IParameterService parameterService,
        IGenericRepository<MerchantBlockage> merchantBlockageRepository,
        IGenericRepository<PostingTransaction> postingTransactionRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository
    )
    {
        _logger = logger;
        _applicationUserService = applicationUserService;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _dbContext = dbContext;
        _parameterService = parameterService;
        _merchantBlockageRepository = merchantBlockageRepository;
        _postingTransactionRepository = postingTransactionRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            await SetBlockageParamsAsync();

            var transactionQuery = _postingTransactionRepository
                .GetAll()
                .Where(w =>
                    w.PostingDate.Date == DateTime.Now.Date
                    && w.RecordStatus == RecordStatus.Active
                    && w.BatchStatus == BatchStatus.Pending
                    && w.PfTransactionSource == PfTransactionSource.VirtualPos
                    && (w.TransactionType == TransactionType.Auth || w.TransactionType == TransactionType.PostAuth)
                );

            var postingMerchants = await transactionQuery
                .Select(w => w.MerchantId)
                .Distinct()
                .ToListAsync();
            
            var merchantBlockages = await _merchantBlockageRepository
                .GetAll()
                .Where(s => 
                    postingMerchants.Contains(s.MerchantId) &&
                    s.RemainingAmount > 0 &&
                    s.RecordStatus == RecordStatus.Active &&
                    s.MerchantBlockageStatus == MerchantBlockageStatus.Incomplete)
                .ToListAsync();

            foreach (var merchantBlockage in merchantBlockages)
            {
                var merchantBlockageThresholdAmount = merchantBlockage.TotalAmount * _blockageThresholdRate;
                var currentPage = 0;
                
                while (merchantBlockage.RemainingAmount > 0)
                {
                    var pageResults = await transactionQuery
                        .Where(s => s.MerchantId == merchantBlockage.MerchantId)
                        .OrderBy(x => x.Id)
                        .Skip(currentPage * TransactionPerBatch)
                        .Take(TransactionPerBatch)
                        .ToListAsync();

                    if (pageResults.Count == 0)
                    {
                        break;
                    }

                    foreach (var postingTransaction in pageResults)
                    {
                        if (postingTransaction.Amount > merchantBlockage.TotalAmount +
                            merchantBlockageThresholdAmount - merchantBlockage.BlockageAmount)
                        {
                            continue;
                        }

                        var merchantTransaction = await _merchantTransactionRepository.GetAll()
                            .Where(s => s.Id == postingTransaction.MerchantTransactionId).FirstOrDefaultAsync();
                        
                        merchantBlockage.BlockageAmount += postingTransaction.Amount;
                        merchantBlockage.RemainingAmount -= postingTransaction.Amount;
                        merchantBlockage.MerchantBlockageStatus =
                            merchantBlockage.RemainingAmount <= merchantBlockageThresholdAmount
                                ? MerchantBlockageStatus.Complete
                                : MerchantBlockageStatus.Incomplete;

                        if (merchantBlockage.RemainingAmount < 0)
                        {
                            merchantBlockage.RemainingAmount = 0;
                        }
                        
                        var blockagePaymentDate = postingTransaction.PaymentDate.AddMonths(_blockagePeriodMonths);
                        blockagePaymentDate = blockagePaymentDate.Date >= DateTime.Now.Date ? blockagePaymentDate : DateTime.Now.Date;
                        
                        postingTransaction.OldPaymentDate = postingTransaction.PaymentDate;
                        postingTransaction.PaymentDate = blockagePaymentDate;
                        postingTransaction.BlockageStatus = BlockageStatus.Blocked;

                        merchantTransaction.BlockageStatus = BlockageStatus.Blocked;
                        merchantTransaction.PfPaymentDate = blockagePaymentDate;
                        
                        var merchantBlockageDetail = new MerchantBlockageDetail
                        {
                            MerchantBlockageId = merchantBlockage.Id,
                            MerchantId = postingTransaction.MerchantId,
                            TotalAmount = merchantBlockage.TotalAmount,
                            BlockageAmount = postingTransaction.Amount,
                            RemainingAmount = merchantBlockage.RemainingAmount,
                            BlockageStatus = BlockageStatus.Blocked,
                            PostingDate = DateTime.Now.Date,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                        };
                        
                        var strategy = _dbContext.Database.CreateExecutionStrategy();
                        await strategy.ExecuteAsync(async () =>
                        {
                            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                            await _dbContext.MerchantBlockageDetail.AddAsync(merchantBlockageDetail);
                            _dbContext.MerchantBlockage.Update(merchantBlockage);
                            _dbContext.PostingTransaction.Update(postingTransaction);
                            _dbContext.MerchantTransaction.Update(merchantTransaction);
                            await _dbContext.SaveChangesAsync();
                            scope.Complete();
                        });

                        if (merchantBlockage.RemainingAmount <= 0)
                        {
                            break;
                        }
                    }

                    currentPage++;
                }
            }
            
            batchStatus.BatchSummary = "PostingMerchantBlockageBatchFinishedSuccessfully";
            batchStatus.BatchStatus = BatchStatus.Completed;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = false;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingMerchantBlockageBatchError {exception}");

            batchStatus.BatchSummary = "PostingMerchantBlockageBatchError";
            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = true;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }
    
    private async Task SetBlockageParamsAsync()
    {
        var errorMessage = $"ErrorGettingMerchantBlockageParamsContinueWithDefaultValues!";

        try
        {
            var postingParameters =
                await _parameterService.GetParametersAsync("PostingParams");

            var parse = int.TryParse(postingParameters.FirstOrDefault(p => p.ParameterCode == "BlockagePeriodMonth")
                    ?.ParameterValue,
                out _blockagePeriodMonths);

            if (!parse)
            {
                _logger.LogError(errorMessage);
                _blockagePeriodMonths = BlockagePeriodMonths;
            }

            if (decimal.TryParse(postingParameters.FirstOrDefault(p => p.ParameterCode == "BlockageThresholdRate")
                        ?.ParameterValue,
                    out _blockageThresholdRate))
            {
                _blockageThresholdRate /= 100;
            }
            else
            {
                _blockageThresholdRate = BlockageThresholdRate;
            }
        }
        catch (Exception)
        {
            _blockageThresholdRate = BlockageThresholdRate;
            _blockagePeriodMonths = BlockagePeriodMonths;

            _logger.LogError(errorMessage);
        }
    }
}