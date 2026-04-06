using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Command.DeleteCardBin;

public class DeleteCardBinCommand : IRequest
{
    public Guid Id { get; set; }
}

public class DeleteCardBinCommandHandler : IRequestHandler<DeleteCardBinCommand>
{
    private readonly ICardBinService _cardBinService;

    public DeleteCardBinCommandHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }
    public async Task<Unit> Handle(DeleteCardBinCommand request, CancellationToken cancellationToken)
    {
        await _cardBinService.DeleteAsync(request);

        return Unit.Value;
    }
}
