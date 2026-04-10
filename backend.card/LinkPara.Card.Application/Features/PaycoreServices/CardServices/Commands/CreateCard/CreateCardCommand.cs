using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;

public class CreateCardCommand : IRequest<CreateCardResponse>
{
    public CardAccount CardAccount { get; set; }
    public string EmbossName1 { get; set; }
    public string ProductCode { get; set; }
    public CardDelivery CardDelivery { get; set; }
    public string WalletNumber { get; set; }
    public string CardName { get; set; }
}

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, CreateCardResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public CreateCardCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<CreateCardResponse> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.CreateCardAsync(request);
    }
}
