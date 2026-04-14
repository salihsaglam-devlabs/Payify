using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using MediatR;

namespace LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;


public class CardRenewalCommand : IRequest<PaycoreResponse>
{
    public string CardNo { get; set; }
    public string EmbossCode { get; set; }
    public int PersoCenterCode { get; set; }
    public int ExpiryDate { get; set; }
    public string TransferredProductCode { get; set; }
    public bool IsEmbossDigitalCard { get; set; }
    public bool IsExtendExpiryPeriod { get; set; }
    public string EmbossMethod { get; set; }
    public string EmbossName1 { get; set; }
    public string EmbossName2 { get; set; }
    public bool IsBSCSigned { get; set; }
}


public class CardRenewalCommandHandler : IRequestHandler<CardRenewalCommand, PaycoreResponse>
{
    private readonly IPaycoreCardService _paycoreService;
    public CardRenewalCommandHandler(IPaycoreCardService paycoreService)
    {
        _paycoreService = paycoreService;
    }
    public async Task<PaycoreResponse> Handle(CardRenewalCommand request, CancellationToken cancellationToken)
    {
        return await _paycoreService.CardRenewalAsync(request);
    }
}
