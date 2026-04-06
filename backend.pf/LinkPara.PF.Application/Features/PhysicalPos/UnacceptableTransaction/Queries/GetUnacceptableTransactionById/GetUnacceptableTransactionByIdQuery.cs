using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using MediatR;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction.Queries.GetUnacceptableTransactionById;

public class GetUnacceptableTransactionByIdQuery : IRequest<UnacceptableTransactionDetailResponse>
{
    public Guid Id { get; set; }
}

public class GetUnacceptableTransactionByIdQueryHandler : IRequestHandler<GetUnacceptableTransactionByIdQuery, UnacceptableTransactionDetailResponse>
{
    private readonly IUnacceptableTransactionService _unacceptableTransactionService;
    private readonly ILogger<GetUnacceptableTransactionByIdQuery> _logger;

    public GetUnacceptableTransactionByIdQueryHandler(IUnacceptableTransactionService unacceptableTransactionService, 
        ILogger<GetUnacceptableTransactionByIdQuery> logger)
    {
        _unacceptableTransactionService = unacceptableTransactionService;
        _logger = logger;
    }

    public async Task<UnacceptableTransactionDetailResponse> Handle(GetUnacceptableTransactionByIdQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _unacceptableTransactionService.GetDetailsByIdAsync(request.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetUnacceptableTransactionById Failed: {exception}");
            throw;
        }
        
    }
}
