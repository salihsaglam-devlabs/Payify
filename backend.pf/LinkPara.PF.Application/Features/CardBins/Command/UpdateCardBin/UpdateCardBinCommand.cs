using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;

public class UpdateCardBinCommand : IRequest, IMapFrom<CardBin>
{
    public Guid Id { get; set; }
    public string BinNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardType CardType { get; set; }
    public CardSubType CardSubType { get; set; }
    public CardNetwork? CardNetwork { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public bool IsVirtual { get; set; }
    public int BankCode { get; set; }
}

public class UpdateCardBinCommandHandler : IRequestHandler<UpdateCardBinCommand>
{
    private readonly ICardBinService _cardBinService;

    public UpdateCardBinCommandHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }
    public async Task<Unit> Handle(UpdateCardBinCommand request, CancellationToken cancellationToken)
    {
        await _cardBinService.UpdateAsync(request);

        return Unit.Value;  
    }
}
