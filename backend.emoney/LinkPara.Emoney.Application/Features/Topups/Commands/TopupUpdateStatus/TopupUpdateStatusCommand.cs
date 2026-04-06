using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Topups.Commands.TopupUpdateStatus;

public class TopupUpdateStatusCommand : IRequest
{
    public Guid CardTopupRequestId { get; set; }
    public CardTopupRequestStatus Status { get; set; }
}

public class TopupUpdateStatusCommandHandler : IRequestHandler<TopupUpdateStatusCommand>
{
    private readonly IGenericRepository<CardTopupRequest> _repository;
    private readonly IContextProvider _contextProvider;

    public TopupUpdateStatusCommandHandler(IGenericRepository<CardTopupRequest> repository, IContextProvider contextProvider)
    {
        _repository = repository;
        _contextProvider = contextProvider;
    }

    public async Task<Unit> Handle(TopupUpdateStatusCommand request, CancellationToken cancellationToken)
    {
        var cardTopupRequest = await _repository.GetByIdAsync(request.CardTopupRequestId);

        if (cardTopupRequest is null)
            throw new NotFoundException(nameof(cardTopupRequest));

        cardTopupRequest.Status = request.Status;
        cardTopupRequest.UpdateDate = DateTime.Now;
        cardTopupRequest.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? string.Empty;

        await _repository.UpdateAsync(cardTopupRequest);

        return Unit.Value;
    }
}
