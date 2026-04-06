using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.RetryUnacceptableTransaction;

public class RetryUnacceptableTransactionCommand : IRequest
{
    public Guid UnacceptableTransactionId { get; set; }
}

public class RetryUnacceptableTransactionCommandHandler : IRequestHandler<RetryUnacceptableTransactionCommand>
{
    private readonly IUnacceptableTransactionService _unacceptableTransactionService;
    private readonly ILogger<RetryUnacceptableTransactionCommand> _logger;

    public RetryUnacceptableTransactionCommandHandler(IUnacceptableTransactionService unacceptableTransactionService, ILogger<RetryUnacceptableTransactionCommand> logger)
    {
        _unacceptableTransactionService = unacceptableTransactionService;
        _logger = logger;
    }
    public async Task<Unit> Handle(RetryUnacceptableTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _unacceptableTransactionService.RetryUnacceptableTransactionAsync(request.UnacceptableTransactionId);
            return Unit.Value;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Retry Unacceptable Transaction Failed: {exception}");
            throw;
        }
    }
}