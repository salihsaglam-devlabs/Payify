using System.Transactions;
using LinkPara.HttpProviders.Vault;
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

public class PostingDeductionTransferBatch : IPostingBatch<PostingDeductionTransfer>
{
    private readonly ILogger<PostingDeductionTransfer> _logger;
    private readonly IGenericRepository<PostingBatchStatus> _postingBatchStatusRepository;
    private readonly IApplicationUserService _applicationUserService;
    private readonly PfDbContext _pfDbContext;
    private readonly IVaultClient _vaultClient;
    private const int DefaultDeductionTransferThresholdDays = 30;

    public PostingDeductionTransferBatch(ILogger<PostingDeductionTransfer> logger,
        IGenericRepository<PostingBatchStatus> postingBatchStatusRepository,
        IApplicationUserService applicationUserService,
        PfDbContext pfDbContext,
        IVaultClient vaultClient)
    {
        _logger = logger;
        _postingBatchStatusRepository = postingBatchStatusRepository;
        _applicationUserService = applicationUserService;
        _pfDbContext = pfDbContext;
        _vaultClient = vaultClient;
    }

    public async Task ExecuteBatchAsync(PostingBatchStatus batchStatus)
    {
        try
        {
            var deductionTransferThresholdDays = DefaultDeductionTransferThresholdDays;
            try
            {
                deductionTransferThresholdDays = await _vaultClient.GetSecretValueAsync<int>("PFSecrets", "PostingSettings", "DeductionTransferThresholdDays");
            }
            catch (Exception exception)
            {
                _logger.LogError($"ErrorOnGetSecretValue Error:{exception}");
            }
            
            var transferableDeductionTypes = new List<DeductionType>
            {
                DeductionType.Chargeback, 
                DeductionType.Suspicious, 
                DeductionType.Due, 
                DeductionType.ExcessReturn
            };

            var pendingDeductions = await _pfDbContext.MerchantDeduction
                .Where(s =>
                    s.DeductionStatus == DeductionStatus.Pending &&
                    transferableDeductionTypes.Contains(s.DeductionType) &&
                    DateTime.Today >= s.ExecutionDate.Date.AddDays(deductionTransferThresholdDays))
                .ToListAsync();
            
            var merchantIds = pendingDeductions.Select(s => s.MerchantId).Distinct().ToList();

            var subMerchants = await _pfDbContext.Merchant
                .Where(s =>
                    s.MerchantType == MerchantType.SubMerchant &&
                    s.ParentMerchantId.HasValue && 
                    s.ParentMerchantId != Guid.Empty &&
                    merchantIds.Contains(s.Id))
                .Select(s => new
                {
                    MerchantId = s.Id,
                    ParentMerchantId = s.ParentMerchantId.Value
                })
                .ToListAsync();
            
            var subMerchantIds = subMerchants.Select(s => s.MerchantId).Distinct().ToList();

            var transferableDeductions = pendingDeductions
                .Where(s => subMerchantIds.Contains(s.MerchantId)).ToList();
            
            var strategy = _pfDbContext.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                foreach (var transferableDeduction in transferableDeductions)
                {
                    var deductionType = transferableDeduction.DeductionType switch
                    {
                        DeductionType.Chargeback => DeductionType.ChargebackTransfer,
                        DeductionType.Suspicious => DeductionType.SuspiciousTransfer,
                        DeductionType.Due => DeductionType.DueTransfer,
                        DeductionType.ExcessReturn => DeductionType.ExcessReturnTransfer,
                        _ => throw new ArgumentException($"UnsupportedDeductionType: {transferableDeduction.DeductionType}")
                    };
                    
                    await _pfDbContext.MerchantDeduction.AddAsync(new MerchantDeduction
                    {
                        TotalDeductionAmount = transferableDeduction.RemainingDeductionAmount,
                        RemainingDeductionAmount = transferableDeduction.RemainingDeductionAmount,
                        Currency = transferableDeduction.Currency,
                        DeductionType = deductionType,
                        DeductionStatus = DeductionStatus.Pending,
                        ExecutionDate = DateTime.Now,
                        MerchantId = subMerchants.FirstOrDefault(s => s.MerchantId == transferableDeduction.MerchantId)?.ParentMerchantId ?? Guid.Empty,
                        MerchantTransactionId = transferableDeduction.MerchantTransactionId,
                        MerchantDueId = Guid.Empty,
                        RecordStatus = RecordStatus.Active,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        PostingBalanceId = Guid.Empty,
                        ConversationId = string.Empty,
                        DeductionAmountWithCommission = transferableDeduction.RemainingDeductionAmount,
                        SubMerchantDeductionId = transferableDeduction.Id
                    });
                    
                    transferableDeduction.DeductionStatus =
                        transferableDeduction.RemainingDeductionAmount == transferableDeduction.TotalDeductionAmount
                            ? DeductionStatus.Transferred
                            : DeductionStatus.PartialTransfer;
                    transferableDeduction.UpdateDate = DateTime.Now;
                    _pfDbContext.MerchantDeduction.Update(transferableDeduction);
                }
                
                _pfDbContext.PostingBatchStatus.Attach(batchStatus);
                batchStatus.BatchStatus = BatchStatus.Completed;
                batchStatus.IsCriticalError = false;
                batchStatus.BatchSummary = "PostingDeductionTransferJobFinishedSuccessfully";
                batchStatus.UpdateDate = DateTime.Now;
                
                await _pfDbContext.SaveChangesAsync();
                
                scope.Complete();
            });
            
        }
        catch (Exception exception)
        {
            _logger.LogError($"PostingDeductionTransferError, {exception}");

            batchStatus.BatchSummary = "PostingDeductionTransferError";
            batchStatus.BatchStatus = BatchStatus.Error;
            batchStatus.UpdateDate = DateTime.Now;
            batchStatus.IsCriticalError = true;

            await _postingBatchStatusRepository.UpdateAsync(batchStatus);
        }
    }
}