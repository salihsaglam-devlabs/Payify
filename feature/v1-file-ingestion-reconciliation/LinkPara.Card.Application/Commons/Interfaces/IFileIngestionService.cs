using LinkPara.Card.Application.Commons.Enums;
using LinkPara.Card.Application.Commons.Models.FileIngestion;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IFileIngestionService
{
    Task<IReadOnlyCollection<FileIngestionResult>> ImportCardTransactionsFromLocalDirectoryAsync(
        string directoryPath,
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FileIngestionResult>> ImportClearingFromLocalDirectoryAsync(
        string directoryPath,
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FileIngestionResult>> ImportCardTransactionsFromFtpAsync(
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<FileIngestionResult>> ImportClearingFromFtpAsync(
        bool resetExisting = false,
        FileIngestionResetScope resetScope = FileIngestionResetScope.Hash,
        CancellationToken cancellationToken = default);
}
