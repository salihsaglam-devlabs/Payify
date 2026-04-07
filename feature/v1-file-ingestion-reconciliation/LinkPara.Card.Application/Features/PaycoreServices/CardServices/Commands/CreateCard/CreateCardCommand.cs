using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;

public class CreateCardCommand : IRequest<PaycoreResponse>
{
    public CardAccount CardAccount { get; set; }
    public string BankingCustomerNo { get; set; }
    public string CardLevel { get; set; }
    public string EmbossName1 { get; set; }
    public string EmbossName2 { get; set; }
    public string ProductCode { get; set; }
    public int BranchCode { get; set; }
    public string EmbossMethod { get; set; }
    public CardDelivery CardDelivery { get; set; }
    //todo limit
}

public class CreateCardCommandHandler : IRequestHandler<CreateCardCommand, PaycoreResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public CreateCardCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<PaycoreResponse> Handle(CreateCardCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.CreateCardAsync(request);
    }
}
