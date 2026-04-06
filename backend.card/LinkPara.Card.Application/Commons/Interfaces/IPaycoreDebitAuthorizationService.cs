using LinkPara.Card.Application.Commons.Models.PaycoreModels.DebitAuthorizationModels;
using LinkPara.Card.Application.Features.PaycoreServices.DebitAuthorizationServices.Commands.DebitAuthorization;

namespace LinkPara.Card.Application.Commons.Interfaces
{
    public interface IPaycoreDebitAuthorizationService
    {
        Task<DebitAuthorizationResponse> DebitAuthAsync(DebitAuthorizationCommand command);
    }
}
