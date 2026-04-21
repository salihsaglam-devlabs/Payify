using LinkPara.Card.Application.Commons.Models.PaycoreModels.SecurityModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardPinServices.Commands.SetCardPin;

namespace LinkPara.Card.Application.Commons.Interfaces;
public interface IPaycoreSecurityService
{
    Task<EncDecPinblockResponse> SetCardPinAsync(SetCardPinCommand command);
}