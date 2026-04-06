using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Command.UpdateUnacceptableTransaction;

public class UpdateUnacceptableTransactionCommand : IRequest<PhysicalPosUnacceptableTransactionDto>
{
    public Guid Id { get; set; }
    public JsonPatchDocument<UpdateUnacceptableTransactionRequest> UnacceptableTransaction { get; set; }
}

public class UpdateUnacceptableTransactionCommandHandler : IRequestHandler<UpdateUnacceptableTransactionCommand, PhysicalPosUnacceptableTransactionDto>
{
    private readonly IUnacceptableTransactionService _unacceptableTransactionService;
    private readonly ILogger<UpdateUnacceptableTransactionCommand> _logger;

    public UpdateUnacceptableTransactionCommandHandler(IUnacceptableTransactionService unacceptableTransactionService, 
        ILogger<UpdateUnacceptableTransactionCommand> logger)
    {
        _unacceptableTransactionService = unacceptableTransactionService;
        _logger = logger;
    }

    public async Task<PhysicalPosUnacceptableTransactionDto> Handle(UpdateUnacceptableTransactionCommand request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unacceptableTransactionService.UpdateStatusAsync(request);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Update Unacceptable Transaction Failed: {exception}");
            throw;
        }
    }
}