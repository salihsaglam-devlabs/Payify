using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;

public class SaveCardBinCommand : IRequest, IMapFrom<CardBin>
{
    public string BinNumber { get; set; }
    public CardBrand CardBrand { get; set; }
    public CardType? CardType { get; set; }
    public CardSubType? CardSubType { get; set; }
    public CardNetwork? CardNetwork { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public bool IsVirtual { get; set; }
    public int BankCode { get; set; }
}
public class SaveCardBinCommandHandler : IRequestHandler<SaveCardBinCommand>
{
    private readonly ICardBinService _cardBinService;

    public SaveCardBinCommandHandler(ICardBinService cardBinService)
    {
        _cardBinService = cardBinService;
    }

    public async Task<Unit> Handle(SaveCardBinCommand request, CancellationToken cancellationToken)
    {
        await _cardBinService.SaveAsync(request);

        return Unit.Value;
    }
}
