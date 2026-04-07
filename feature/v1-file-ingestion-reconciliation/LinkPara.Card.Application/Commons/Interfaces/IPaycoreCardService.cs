using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPaycoreCardService
{
    Task<PaycoreResponse> CreateCardAsync(CreateCardCommand command);
    Task<UpdateCardStatusResponse> UpdateCardStatusAsync(UpdateCardStatusCommand command);
    Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery command);
    Task<GetCardInformationsResponse> GetCardInformationsAsync(GetCardInformationsQuery command);
    Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationCommand command);
    Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery command);
    // Task<PaycoreResponse> GetClearCardNoAsync(GetClearCardNoQuery query);
    //Task<PaycoreResponse> GetCommunicationInformationAsync(GetCommunicationInformationQuery query);
}
