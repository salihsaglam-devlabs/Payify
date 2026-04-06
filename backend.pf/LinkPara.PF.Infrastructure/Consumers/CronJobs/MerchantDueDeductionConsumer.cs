using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Consumers.CronJobs;

public class MerchantDueDeductionConsumer : IConsumer<MerchantDueDeduction>
{
    private readonly IGenericRepository<MerchantDue> _repository;
    private readonly ILogger<MerchantDueDeductionConsumer> _logger;
    private readonly PfDbContext _dbContext;
    private readonly IApplicationUserService _applicationUserService;

    public MerchantDueDeductionConsumer(IGenericRepository<MerchantDue> repository,
        ILogger<MerchantDueDeductionConsumer> logger,
        PfDbContext dbContext,
        IApplicationUserService applicationUserService)
    {
        _repository = repository;
        _logger = logger;
        _dbContext = dbContext;
        _applicationUserService = applicationUserService;
    }

    public async Task Consume(ConsumeContext<MerchantDueDeduction> context)
    {
        var deductionDues = await _repository.GetAll()
            .Include(d => d.DueProfile)
            .Where(d => 
            d.RecordStatus == RecordStatus.Active && 
            (
                (d.DueProfile.OccurenceInterval == TimeInterval.Monthly && d.LastExecutionDate.AddMonths(1) <= DateTime.Today) || 
                (d.DueProfile.OccurenceInterval == TimeInterval.Annually && d.LastExecutionDate.AddYears(1) <= DateTime.Today)
            ))
            .ToListAsync();

        foreach (var due in deductionDues)
        {
            try
            {
                var strategy = _dbContext.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                    var deduction = new MerchantDeduction()
                    {
                        MerchantTransactionId = Guid.Empty,
                        MerchantId = due.MerchantId,
                        Currency = due.DueProfile.Currency,
                        TotalDeductionAmount = due.DueProfile.Amount,
                        RemainingDeductionAmount = due.DueProfile.Amount,
                        ExecutionDate = DateTime.Now,
                        DeductionType = DeductionType.Due,
                        DeductionStatus = DeductionStatus.Pending,
                        MerchantDueId = due.Id,
                        PostingBalanceId = Guid.Empty,
                        ConversationId = string.Empty,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        DeductionAmountWithCommission = due.DueProfile.Amount,
                        SubMerchantDeductionId = Guid.Empty
                    };

                    await _dbContext.MerchantDeduction.AddAsync( deduction );

                    due.LastExecutionDate = DateTime.Today;
                    due.TotalExecutionCount += 1;

                    _dbContext.MerchantDue.Update(due);

                    await _dbContext.SaveChangesAsync();

                    scope.Complete();
                });
            }
            catch (Exception exception)
            {

                _logger.LogError($"MerchantDueDeductionConsumerException : {exception}");
            }
        }


    }
}