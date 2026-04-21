using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Audit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

public interface IClearingArrivalRequeueService
{
    Task<int> RequeueAwaitingCardRowsAsync(Guid clearingFileId, CancellationToken cancellationToken = default);
}

internal sealed class ClearingArrivalRequeueService : IClearingArrivalRequeueService
{
    private const int CorrelationBatchSize = 5_000;

    private readonly CardDbContext _dbContext;
    private readonly IAuditStampService _auditStampService;
    private readonly IStringLocalizer _localizer;

    public ClearingArrivalRequeueService(
        CardDbContext dbContext,
        IAuditStampService auditStampService,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _auditStampService = auditStampService;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
    }

    public async Task<int> RequeueAwaitingCardRowsAsync(
        Guid clearingFileId,
        CancellationToken cancellationToken = default)
    {
        var clearingFile = await _dbContext.IngestionFiles
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == clearingFileId, cancellationToken);

        if (clearingFile is null || clearingFile.FileType != FileType.Clearing)
        {
            return 0;
        }
        
        if (clearingFile.Status != FileStatus.Success)
        {
            return 0;
        }
        
        var correlationPairs = await _dbContext.IngestionFileLines
            .AsNoTracking()
            .Where(x => x.FileId == clearingFileId
                        && x.LineType == "D"
                        && x.CorrelationKey != null
                        && x.CorrelationValue != null)
            .Select(x => new { x.CorrelationKey, x.CorrelationValue })
            .Distinct()
            .ToListAsync(cancellationToken);

        if (correlationPairs.Count == 0)
        {
            return 0;
        }

        var totalRequeued = 0;
        var auditStamp = _auditStampService.CreateStamp();
        var requeueMessage = _localizer.Get("Reconciliation.RequeuedByClearingArrival", clearingFileId.ToString("N"));
        
        foreach (var group in correlationPairs.GroupBy(x => x.CorrelationKey!))
        {
            var key = group.Key;
            var values = group.Select(x => x.CorrelationValue!).ToArray();

            for (var offset = 0; offset < values.Length; offset += CorrelationBatchSize)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var batch = values.Skip(offset).Take(CorrelationBatchSize).ToArray();

                var updated = await _dbContext.IngestionFileLines
                    .Where(x => x.ReconciliationStatus == ReconciliationStatus.AwaitingClearing)
                    .Where(x => x.LineType == "D")
                    .Where(x => x.CorrelationKey == key && batch.Contains(x.CorrelationValue))
                    .Where(x => x.IngestionFile.FileType != FileType.Clearing)
                    .ExecuteUpdateAsync(update => update
                        .SetProperty(x => x.ReconciliationStatus, ReconciliationStatus.Ready)
                        .SetProperty(x => x.Message, requeueMessage)
                        .SetProperty(x => x.UpdateDate, auditStamp.Timestamp)
                        .SetProperty(x => x.LastModifiedBy, auditStamp.UserId),
                        cancellationToken);

                totalRequeued += updated;
            }
        }

        return totalRequeued;
    }
}

