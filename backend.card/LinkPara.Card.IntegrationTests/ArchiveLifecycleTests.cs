using FluentAssertions;
using LinkPara.Card.Application.Commons.Models.Archive;
using LinkPara.Card.Domain.Entities.Archive;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Archive;
using LinkPara.Card.Infrastructure.Services.Audit;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.DomainEvents.Interfaces;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using NUnit.Framework;

namespace LinkPara.Card.IntegrationTests;

[TestFixture]
public class ArchiveLifecycleTests
{
    [Test]
    public void Evaluator_marks_atomic_items_independently_and_does_not_allow_cleanup_from_file_state_alone()
    {
        var evaluator = new ArchiveEligibilityEvaluator(Options.Create(CreateArchiveOptions(minLastUpdateAgeHours: 72)));
        var snapshot = new ArchiveAggregateSnapshot
        {
            AggregateId = Guid.NewGuid(),
            FileCreateDateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            LastUpdateUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            AggregateLastActivityUtc = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            Lifecycle = new ArchiveFileLifecycleState
            {
                ArchiveRecordWritten = true
            },
            ItemSnapshots =
            [
                new ArchiveAtomicItemSnapshot
                {
                    FileLineId = Guid.NewGuid(),
                    FileLineStatuses = new HashSet<string>(["Success"], StringComparer.OrdinalIgnoreCase),
                    FileLineReconciliationStatuses = new HashSet<string>(["Success"], StringComparer.OrdinalIgnoreCase),
                    EvaluationStatuses = new HashSet<string>(["Completed"], StringComparer.OrdinalIgnoreCase),
                    OperationStatuses = new HashSet<string>(["Completed"], StringComparer.OrdinalIgnoreCase)
                },
                new ArchiveAtomicItemSnapshot
                {
                    FileLineId = Guid.NewGuid(),
                    FileLineStatuses = new HashSet<string>(["Success"], StringComparer.OrdinalIgnoreCase),
                    FileLineReconciliationStatuses = new HashSet<string>(["Success"], StringComparer.OrdinalIgnoreCase),
                    EvaluationStatuses = new HashSet<string>(["Completed"], StringComparer.OrdinalIgnoreCase),
                    OperationStatuses = new HashSet<string>(["Planned"], StringComparer.OrdinalIgnoreCase)
                }
            ]
        };
        snapshot.AtomicItems.TotalCount = snapshot.ItemSnapshots.Count;

        var result = evaluator.Evaluate(snapshot, new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc));

        result.IsEligible.Should().BeTrue();
        snapshot.AtomicItems.CompletedCount.Should().Be(1);
        snapshot.AtomicItems.EligibleCount.Should().Be(1);
        snapshot.Lifecycle.ChildrenFullyTransitioned.Should().BeFalse();
        snapshot.Lifecycle.CleanupEligible.Should().BeFalse();
    }

    [Test]
    public async Task Executor_archives_only_completed_atomic_items_and_keeps_live_file_until_cleanup_window()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var context = CreateContext(connection))
        {
            await context.Database.EnsureCreatedAsync();
            await SeedPartialArchiveScenarioAsync(context, seedTime);
        }

        await using (var context = CreateContext(connection))
        {
            var executor = CreateExecutor(context, new FixedAuditStampService(new AuditStamp("archive-user", new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc))), CreateArchiveOptions(minLastUpdateAgeHours: 72));

            var result = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);

            result.Status.Should().Be("Archived");
            result.Message.Should().Contain("ATOMIC_ITEMS_ARCHIVED:1");
        }

        await using (var verificationContext = CreateContext(connection))
        {
            var liveFile = await verificationContext.IngestionFiles.SingleAsync(x => x.Id == TestData.FileId);
            var liveLines = await verificationContext.IngestionFileLines
                .Where(x => x.IngestionFileId == TestData.FileId && x.RecordType == "D")
                .OrderBy(x => x.LineNumber)
                .ToListAsync();

            liveFile.ArchiveRecordWrittenAt.Should().NotBeNull();
            liveFile.ArchiveCleanupEligibleAt.Should().BeNull();
            liveLines[0].ArchiveTransitionedAt.Should().NotBeNull();
            liveLines[1].ArchiveTransitionedAt.Should().BeNull();

            (await verificationContext.ArchiveIngestionFiles.CountAsync(x => x.Id == TestData.FileId)).Should().Be(1);
            (await verificationContext.ArchiveIngestionFileLines.CountAsync(x => x.IngestionFileId == TestData.FileId)).Should().Be(1);
            (await verificationContext.ArchiveReconciliationEvaluations.CountAsync(x => x.FileLineId == TestData.Line1Id)).Should().Be(1);
            (await verificationContext.ArchiveReconciliationOperations.CountAsync(x => x.FileLineId == TestData.Line1Id)).Should().Be(1);
            (await verificationContext.IngestionFiles.CountAsync(x => x.Id == TestData.FileId)).Should().Be(1);
        }
    }

    [Test]
    public async Task Executor_cleans_live_aggregate_only_after_all_items_are_archived_and_wait_period_has_elapsed()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var context = CreateContext(connection))
        {
            await context.Database.EnsureCreatedAsync();
            await SeedFullyCompletedScenarioAsync(context, seedTime);
        }

        await using (var firstRunContext = CreateContext(connection))
        {
            var executor = CreateExecutor(firstRunContext, new FixedAuditStampService(new AuditStamp("archive-user", new DateTime(2026, 4, 1, 10, 0, 0, DateTimeKind.Utc))), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            var firstRun = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);

            firstRun.Status.Should().Be("Archived");
        }

        await using (var betweenRunsContext = CreateContext(connection))
        {
            (await betweenRunsContext.IngestionFiles.CountAsync(x => x.Id == TestData.FileId)).Should().Be(1);
            var file = await betweenRunsContext.IngestionFiles.SingleAsync(x => x.Id == TestData.FileId);
            file.ArchiveCleanupEligibleAt.Should().BeNull();
        }

        await using (var secondRunContext = CreateContext(connection))
        {
            await BackdateLiveActivityAsync(secondRunContext, DateTime.UtcNow.AddDays(-4));

            var executor = CreateExecutor(secondRunContext, new FixedAuditStampService(new AuditStamp("archive-user", DateTime.UtcNow)), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            var secondRun = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);

            secondRun.Status.Should().Be("Archived");
            secondRun.Message.Should().Contain("LIVE_AGGREGATE_CLEANED");
        }

        await using (var verificationContext = CreateContext(connection))
        {
            (await verificationContext.IngestionFiles.CountAsync(x => x.Id == TestData.FileId)).Should().Be(0);
            (await verificationContext.IngestionFileLines.CountAsync(x => x.IngestionFileId == TestData.FileId)).Should().Be(0);

            var archiveFile = await verificationContext.ArchiveIngestionFiles.SingleAsync(x => x.Id == TestData.FileId);
            archiveFile.ArchiveChildrenTransitionedAt.Should().NotBeNull();
            archiveFile.ArchiveCleanupEligibleAt.Should().NotBeNull();
            archiveFile.ArchiveCleanupCompletedAt.Should().NotBeNull();
        }
    }

    [Test]
    public async Task Executor_is_idempotent_for_retries_and_does_not_duplicate_archive_rows()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var context = CreateContext(connection))
        {
            await context.Database.EnsureCreatedAsync();
            await SeedPartialArchiveScenarioAsync(context, seedTime);
        }

        await using (var firstRunContext = CreateContext(connection))
        {
            var executor = CreateExecutor(firstRunContext, new FixedAuditStampService(new AuditStamp("archive-user", new DateTime(2026, 4, 9, 12, 0, 0, DateTimeKind.Utc))), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);
        }

        await using (var secondRunContext = CreateContext(connection))
        {
            var executor = CreateExecutor(secondRunContext, new FixedAuditStampService(new AuditStamp("archive-user", new DateTime(2026, 4, 9, 13, 0, 0, DateTimeKind.Utc))), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            var secondRun = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);
            secondRun.Status.Should().Be("Skipped");
        }

        await using (var verificationContext = CreateContext(connection))
        {
            (await verificationContext.ArchiveIngestionFileLines.CountAsync(x => x.IngestionFileId == TestData.FileId)).Should().Be(1);
            (await verificationContext.ArchiveReconciliationEvaluations.CountAsync(x => x.FileLineId == TestData.Line1Id)).Should().Be(1);
            (await verificationContext.ArchiveReconciliationOperations.CountAsync(x => x.FileLineId == TestData.Line1Id)).Should().Be(1);
        }
    }

    [Test]
    public async Task Executor_backfills_legacy_archive_rows_and_allows_safe_cleanup()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var seedTime = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        await using (var context = CreateContext(connection))
        {
            await context.Database.EnsureCreatedAsync();
            await SeedLegacyArchiveScenarioAsync(context, seedTime);
        }

        await using (var firstRunContext = CreateContext(connection))
        {
            var executor = CreateExecutor(firstRunContext, new FixedAuditStampService(new AuditStamp("archive-user", DateTime.UtcNow)), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            var result = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);

            result.Status.Should().Be("Archived");
            result.Message.Should().Contain("FILE_ARCHIVE_RECORD_READY");
        }

        await using (var cleanupContext = CreateContext(connection))
        {
            await BackdateLiveActivityAsync(cleanupContext, DateTime.UtcNow.AddDays(-4));

            var executor = CreateExecutor(cleanupContext, new FixedAuditStampService(new AuditStamp("archive-user", DateTime.UtcNow)), CreateArchiveOptions(minLastUpdateAgeHours: 72));
            var result = await executor.ExecuteAsync(TestData.FileId, CancellationToken.None);

            result.Status.Should().Be("Archived");
            result.Message.Should().Contain("LIVE_AGGREGATE_CLEANED");
        }

        await using (var verificationContext = CreateContext(connection))
        {
            (await verificationContext.IngestionFiles.CountAsync(x => x.Id == TestData.FileId)).Should().Be(0);
            var archiveFile = await verificationContext.ArchiveIngestionFiles.SingleAsync(x => x.Id == TestData.FileId);
            archiveFile.ArchiveCleanupCompletedAt.Should().NotBeNull();
        }
    }

    [Test]
    public void Archive_options_remain_backward_compatible_when_new_lifecycle_fields_are_missing()
    {
        const string legacyJson = """
        {
          "Enabled": true,
          "Defaults": {
            "PreviewLimit": 100,
            "MaxRunCount": 100,
            "ContinueOnError": false,
            "UseConfiguredBeforeDateOnly": false,
            "DefaultBeforeDateStrategy": "RetentionDays"
          },
          "Rules": {
            "RetentionDays": 90,
            "MinLastUpdateAgeHours": 72,
            "RequireAllReviewsClosed": true,
            "RequireAllAlertsResolved": true,
            "ActiveLeaseEnabled": true
          }
        }
        """;

        var options = System.Text.Json.JsonSerializer.Deserialize<ArchiveOptions>(legacyJson)!;
        options.Normalize();

        options.Rules.UseAggregateLastActivityForMinAge.Should().BeTrue();
        options.Rules.RequireFileArchiveRecordBeforeCleanup.Should().BeTrue();
        options.Rules.RequireAllAtomicItemsArchivedBeforeCleanup.Should().BeTrue();
    }

    private static ArchiveExecutor CreateExecutor(CardDbContext context, IAuditStampService auditStampService, ArchiveOptions options)
    {
        var reader = new ArchiveAggregateReader(context);
        var evaluator = new ArchiveEligibilityEvaluator(Options.Create(options.Normalize()));
        return new ArchiveExecutor(
            context,
            reader,
            evaluator,
            new ArchiveVerifier(context),
            new ArchiveSqlDialect(context),
            auditStampService);
    }

    private static CardDbContext CreateContext(SqliteConnection connection)
    {
        var options = new DbContextOptionsBuilder<CardDbContext>()
            .UseSqlite(connection)
            .Options;

        var contextProvider = new Mock<IContextProvider>();
        contextProvider.Setup(x => x.CurrentContext)
            .Returns(new Context
            {
                UserId = "test-user",
                Language = "en"
            });

        var vault = new Mock<IVaultClient>();
        vault.Setup(x => x.GetSecretValue<string>("SharedSecrets", "DatabaseConfiguration", "Provider"))
            .Returns("Npgsql");

        return new CardDbContext(
            options,
            contextProvider.Object,
            Mock.Of<IDomainEventService>(),
            Mock.Of<IBus>(),
            new ConfigurationBuilder().Build(),
            vault.Object);
    }

    private static ArchiveOptions CreateArchiveOptions(int minLastUpdateAgeHours)
    {
        return new ArchiveOptions
        {
            Rules = new ArchiveRuleOptions
            {
                RetentionDays = 30,
                MinLastUpdateAgeHours = minLastUpdateAgeHours,
                UseAggregateLastActivityForMinAge = true,
                RequireAllReviewsClosed = true,
                RequireAllAlertsResolved = true,
                ActiveLeaseEnabled = true,
                RequireFileArchiveRecordBeforeCleanup = true,
                RequireAllAtomicItemsArchivedBeforeCleanup = true
            },
            Statuses = new ArchiveStatusOptions
            {
                TerminalStatuses = new ArchiveEntityStatusOptions
                {
                    IngestionFile = ["Success", "Failed"],
                    IngestionFileLine = ["Success", "Failed"],
                    IngestionFileLineReconciliation = ["Success", "Failed"],
                    ReconciliationEvaluation = ["Completed", "Failed"],
                    ReconciliationOperation = ["Completed", "Failed", "Cancelled"],
                    ReconciliationReview = ["Approved", "Rejected", "Cancelled"],
                    ReconciliationOperationExecution = ["Completed", "Failed", "Skipped"],
                    ReconciliationAlert = ["Consumed", "Failed", "Ignored"]
                },
                NonTerminalBlockingStatuses = new ArchiveEntityStatusOptions
                {
                    IngestionFile = ["Processing"],
                    IngestionFileLine = ["Processing"],
                    IngestionFileLineReconciliation = ["Ready", "Processing"],
                    ReconciliationEvaluation = ["Pending", "Evaluating", "Planned"],
                    ReconciliationOperation = ["Planned", "Blocked", "Executing"],
                    ReconciliationReview = ["Pending"],
                    ReconciliationOperationExecution = ["Started"],
                    ReconciliationAlert = ["Pending", "Processing"]
                },
                ReviewPendingStatuses = ["Pending"],
                AlertPendingStatuses = ["Pending", "Processing"],
                RetryPendingOperationStatuses = ["Planned", "Blocked", "Executing"]
            }
        }.Normalize();
    }

    private static async Task SeedPartialArchiveScenarioAsync(CardDbContext context, DateTime seedTime)
    {
        context.Add(CreateFile(seedTime));
        context.Add(CreateDetailLine(TestData.Line1Id, 1, seedTime));
        context.Add(CreateDetailLine(TestData.Line2Id, 2, seedTime));
        context.Add(CreateEvaluation(TestData.Evaluation1Id, TestData.Line1Id, seedTime));
        context.Add(CreateEvaluation(TestData.Evaluation2Id, TestData.Line2Id, seedTime));
        context.Add(CreateOperation(TestData.Operation1Id, TestData.Line1Id, TestData.Evaluation1Id, OperationStatus.Completed, seedTime));
        context.Add(CreateOperation(TestData.Operation2Id, TestData.Line2Id, TestData.Evaluation2Id, OperationStatus.Planned, seedTime));
        await context.SaveChangesAsync();
        await BackdateSeedDataAsync(context, seedTime, includeSecondLine: true, includeArchive: false);
    }

    private static async Task SeedFullyCompletedScenarioAsync(CardDbContext context, DateTime seedTime)
    {
        context.Add(CreateFile(seedTime));
        context.Add(CreateDetailLine(TestData.Line1Id, 1, seedTime));
        context.Add(CreateEvaluation(TestData.Evaluation1Id, TestData.Line1Id, seedTime));
        context.Add(CreateOperation(TestData.Operation1Id, TestData.Line1Id, TestData.Evaluation1Id, OperationStatus.Completed, seedTime));
        await context.SaveChangesAsync();
        await BackdateSeedDataAsync(context, seedTime, includeSecondLine: false, includeArchive: false);
    }

    private static async Task SeedLegacyArchiveScenarioAsync(CardDbContext context, DateTime seedTime)
    {
        var file = CreateFile(seedTime);
        var line = CreateDetailLine(TestData.Line1Id, 1, seedTime);
        var evaluation = CreateEvaluation(TestData.Evaluation1Id, TestData.Line1Id, seedTime);
        var operation = CreateOperation(TestData.Operation1Id, TestData.Line1Id, TestData.Evaluation1Id, OperationStatus.Completed, seedTime);

        context.Add(file);
        context.Add(line);
        context.Add(evaluation);
        context.Add(operation);
        context.Add(CloneArchiveFile(file, seedTime));
        context.Add(CloneArchiveLine(line, seedTime));
        context.Add(CloneArchiveEvaluation(evaluation, seedTime));
        context.Add(CloneArchiveOperation(operation, seedTime));
        await context.SaveChangesAsync();
        await BackdateSeedDataAsync(context, seedTime, includeSecondLine: false, includeArchive: true);
    }

    private static async Task BackdateSeedDataAsync(CardDbContext context, DateTime seedTime, bool includeSecondLine, bool includeArchive)
    {
        var lineIds = includeSecondLine ? new[] { TestData.Line1Id, TestData.Line2Id } : new[] { TestData.Line1Id };
        var evaluationIds = includeSecondLine ? new[] { TestData.Evaluation1Id, TestData.Evaluation2Id } : new[] { TestData.Evaluation1Id };
        var operationIds = includeSecondLine ? new[] { TestData.Operation1Id, TestData.Operation2Id } : new[] { TestData.Operation1Id };

        await context.IngestionFiles
            .Where(x => x.Id == TestData.FileId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.IngestionFileLines
            .Where(x => lineIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.ReconciliationEvaluations
            .Where(x => evaluationIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.ReconciliationOperations
            .Where(x => operationIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        if (!includeArchive)
        {
            return;
        }

        await context.ArchiveIngestionFiles
            .Where(x => x.Id == TestData.FileId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user")
                .SetProperty(x => x.ArchivedAt, seedTime));

        await context.ArchiveIngestionFileLines
            .Where(x => lineIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user")
                .SetProperty(x => x.ArchivedAt, seedTime));

        await context.ArchiveReconciliationEvaluations
            .Where(x => evaluationIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user")
                .SetProperty(x => x.ArchivedAt, seedTime));

        await context.ArchiveReconciliationOperations
            .Where(x => operationIds.Contains(x.Id))
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.CreateDate, seedTime)
                .SetProperty(x => x.UpdateDate, seedTime)
                .SetProperty(x => x.CreatedBy, "seed-user")
                .SetProperty(x => x.LastModifiedBy, "seed-user")
                .SetProperty(x => x.ArchivedAt, seedTime));
    }

    private static async Task BackdateLiveActivityAsync(CardDbContext context, DateTime backdatedTime)
    {
        await context.IngestionFiles
            .Where(x => x.Id == TestData.FileId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.UpdateDate, backdatedTime)
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.IngestionFileLines
            .Where(x => x.IngestionFileId == TestData.FileId)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.UpdateDate, backdatedTime)
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.ReconciliationEvaluations
            .Where(x => x.FileLineId == TestData.Line1Id || x.FileLineId == TestData.Line2Id)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.UpdateDate, backdatedTime)
                .SetProperty(x => x.LastModifiedBy, "seed-user"));

        await context.ReconciliationOperations
            .Where(x => x.FileLineId == TestData.Line1Id || x.FileLineId == TestData.Line2Id)
            .ExecuteUpdateAsync(update => update
                .SetProperty(x => x.UpdateDate, backdatedTime)
                .SetProperty(x => x.LastModifiedBy, "seed-user"));
    }

    private static IngestionFile CreateFile(DateTime seedTime)
    {
        return Stamp(new IngestionFile
        {
            Id = TestData.FileId,
            FileKey = "FILE-KEY",
            FileName = "archive-test.txt",
            FullPath = "/tmp/archive-test.txt",
            SourceType = FileSourceType.Local,
            FileType = FileType.Card,
            ContentType = FileContentType.Visa,
            Status = FileStatus.Success,
            Message = "Seeded",
            ExpectedCount = 2,
            TotalCount = 2,
            SuccessCount = 2,
            ErrorCount = 0,
            LastProcessedLineNumber = 2,
            LastProcessedByteOffset = 200,
            IsArchived = true
        }, seedTime);
    }

    private static IngestionFileLine CreateDetailLine(Guid lineId, long lineNumber, DateTime seedTime)
    {
        return Stamp(new IngestionFileLine
        {
            Id = lineId,
            IngestionFileId = TestData.FileId,
            LineNumber = lineNumber,
            ByteOffset = lineNumber * 100,
            ByteLength = 100,
            RecordType = "D",
            RawData = $"RAW-{lineNumber}",
            ParsedData = $"{{\"line\":{lineNumber}}}",
            Status = FileRowStatus.Success,
            Message = "Ready",
            RetryCount = 0,
            CorrelationKey = "TxnId",
            CorrelationValue = lineNumber.ToString(),
            DuplicateDetectionKey = lineNumber.ToString(),
            DuplicateStatus = "Unique",
            ReconciliationStatus = ReconciliationStatus.Success
        }, seedTime);
    }

    private static ReconciliationEvaluation CreateEvaluation(Guid id, Guid lineId, DateTime seedTime)
    {
        return Stamp(new ReconciliationEvaluation
        {
            Id = id,
            FileLineId = lineId,
            GroupId = Guid.NewGuid(),
            Status = EvaluationStatus.Completed,
            Message = "Completed",
            CreatedOperationCount = 1
        }, seedTime);
    }

    private static ReconciliationOperation CreateOperation(Guid id, Guid lineId, Guid evaluationId, OperationStatus status, DateTime seedTime)
    {
        return Stamp(new ReconciliationOperation
        {
            Id = id,
            FileLineId = lineId,
            EvaluationId = evaluationId,
            GroupId = Guid.NewGuid(),
            SequenceIndex = 0,
            Code = "TEST",
            Note = "Seeded",
            Payload = "{}",
            IsManual = false,
            Branch = "Main",
            Status = status,
            RetryCount = 0,
            MaxRetries = 5,
            IdempotencyKey = $"{id:N}"
        }, seedTime);
    }

    private static ArchiveIngestionFile CloneArchiveFile(IngestionFile source, DateTime seedTime)
    {
        return Stamp(new ArchiveIngestionFile
        {
            Id = source.Id,
            FileKey = source.FileKey,
            FileName = source.FileName,
            FullPath = source.FullPath,
            SourceType = source.SourceType,
            FileType = source.FileType,
            ContentType = source.ContentType,
            Status = source.Status,
            Message = source.Message,
            ExpectedCount = source.ExpectedCount,
            TotalCount = source.TotalCount,
            SuccessCount = source.SuccessCount,
            ErrorCount = source.ErrorCount,
            LastProcessedLineNumber = source.LastProcessedLineNumber,
            LastProcessedByteOffset = source.LastProcessedByteOffset,
            IsArchived = source.IsArchived,
            ArchivedAt = seedTime,
            ArchivedBy = "legacy-user",
            ArchiveRunId = Guid.NewGuid()
        }, seedTime);
    }

    private static ArchiveIngestionFileLine CloneArchiveLine(IngestionFileLine source, DateTime seedTime)
    {
        return Stamp(new ArchiveIngestionFileLine
        {
            Id = source.Id,
            IngestionFileId = source.IngestionFileId,
            LineNumber = source.LineNumber,
            ByteOffset = source.ByteOffset,
            ByteLength = source.ByteLength,
            RecordType = source.RecordType,
            RawData = source.RawData,
            ParsedData = source.ParsedData,
            Status = source.Status,
            Message = source.Message,
            RetryCount = source.RetryCount,
            CorrelationKey = source.CorrelationKey,
            CorrelationValue = source.CorrelationValue,
            DuplicateDetectionKey = source.DuplicateDetectionKey,
            DuplicateStatus = source.DuplicateStatus,
            DuplicateGroupId = source.DuplicateGroupId,
            ReconciliationStatus = source.ReconciliationStatus,
            ArchivedAt = seedTime,
            ArchivedBy = "legacy-user",
            ArchiveRunId = Guid.NewGuid()
        }, seedTime);
    }

    private static ArchiveReconciliationEvaluation CloneArchiveEvaluation(ReconciliationEvaluation source, DateTime seedTime)
    {
        return Stamp(new ArchiveReconciliationEvaluation
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            GroupId = source.GroupId,
            Status = source.Status,
            Message = source.Message,
            CreatedOperationCount = source.CreatedOperationCount,
            ArchivedAt = seedTime,
            ArchivedBy = "legacy-user",
            ArchiveRunId = Guid.NewGuid()
        }, seedTime);
    }

    private static ArchiveReconciliationOperation CloneArchiveOperation(ReconciliationOperation source, DateTime seedTime)
    {
        return Stamp(new ArchiveReconciliationOperation
        {
            Id = source.Id,
            FileLineId = source.FileLineId,
            EvaluationId = source.EvaluationId,
            GroupId = source.GroupId,
            SequenceIndex = source.SequenceIndex,
            ParentSequenceIndex = source.ParentSequenceIndex,
            Code = source.Code,
            Note = source.Note,
            Payload = source.Payload,
            IsManual = source.IsManual,
            Branch = source.Branch,
            Status = source.Status,
            LeaseOwner = source.LeaseOwner,
            LeaseExpiresAt = source.LeaseExpiresAt,
            RetryCount = source.RetryCount,
            MaxRetries = source.MaxRetries,
            NextAttemptAt = source.NextAttemptAt,
            IdempotencyKey = source.IdempotencyKey,
            LastError = source.LastError,
            ArchivedAt = seedTime,
            ArchivedBy = "legacy-user",
            ArchiveRunId = Guid.NewGuid()
        }, seedTime);
    }

    private static T Stamp<T>(T entity, DateTime seedTime) where T : AuditEntity
    {
        entity.CreateDate = seedTime;
        entity.UpdateDate = seedTime;
        entity.CreatedBy = "seed-user";
        entity.LastModifiedBy = "seed-user";
        entity.RecordStatus = RecordStatus.Active;
        return entity;
    }

    private sealed class FixedAuditStampService : IAuditStampService
    {
        private readonly AuditStamp _stamp;

        public FixedAuditStampService(AuditStamp stamp)
        {
            _stamp = stamp;
        }

        public AuditStamp CreateStamp() => _stamp;

        public void StampForCreate(AuditEntity entity)
        {
            entity.CreateDate = _stamp.Timestamp;
            entity.CreatedBy = _stamp.UserId;
            entity.RecordStatus = RecordStatus.Active;
        }

        public void StampForCreate(IEnumerable<AuditEntity> entities)
        {
            foreach (var entity in entities)
            {
                StampForCreate(entity);
            }
        }

        public void StampForUpdate(AuditEntity entity)
        {
            entity.UpdateDate = _stamp.Timestamp;
            entity.LastModifiedBy = _stamp.UserId;
        }

        public void StampForUpdate(IEnumerable<AuditEntity> entities)
        {
            foreach (var entity in entities)
            {
                StampForUpdate(entity);
            }
        }
    }

    private static class TestData
    {
        public static readonly Guid FileId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        public static readonly Guid Line1Id = Guid.Parse("22222222-2222-2222-2222-222222222221");
        public static readonly Guid Line2Id = Guid.Parse("22222222-2222-2222-2222-222222222222");
        public static readonly Guid Evaluation1Id = Guid.Parse("33333333-3333-3333-3333-333333333331");
        public static readonly Guid Evaluation2Id = Guid.Parse("33333333-3333-3333-3333-333333333332");
        public static readonly Guid Operation1Id = Guid.Parse("44444444-4444-4444-4444-444444444441");
        public static readonly Guid Operation2Id = Guid.Parse("44444444-4444-4444-4444-444444444442");
    }
}
