using LinkPara.PF.Application.Features.Tokens;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface ICardTokenService
{
    Task<CardToken> GetByToken(string token);
    Task<CardInfoDto> GetCardDetailsAsync(CardToken token);
    Task DeleteTokenAsync(CardToken token);
}
