using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.CreateCard;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardAuthorizations;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Commands.UpdateCardStatus;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardAuthorization;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardTransactions;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetCardSensitiveData;
using LinkPara.Card.Application.Features.PaycoreServices.CardServices.Queries.GetClearCardNo;

namespace LinkPara.Card.Application.Commons.Interfaces;

public interface IPaycoreCardService
{
    Task<CreateCardResponse> CreateCardAsync(CreateCardCommand command);
    Task<PaycoreResponse> UpdateCardStatusAsync(UpdateCardStatusCommand command);
    Task<GetCardAuthorizationsResponse> GetCardAuthorizationsAsync(GetCardAuthorizationsQuery command);
    Task<GetCardInformationsResponse> GetCardInformationsAsync(GetCardInformationsQuery command);
    Task<PaycoreResponse> UpdateCardAuthorizationsAsync(UpdateCardAuthorizationCommand command);
    Task<GetCardTransactionsResponse> GetCardTransactionsAsync(GetCardTransactionsQuery command);
    Task<GetCardLastCourierActivityResponse> GetCardLastCorierActivityAsync(GetCardLastCourierActivityQuery command);
    Task<GetCardSensitiveDataResponse> GetCardSensitiveDataAsync(GetCardSensitiveDataQuery command);
    Task<GetClearCardNoResponse> GetClearCardNoAsync(GetClearCardNoQuery command);
    Task<PaycoreResponse> AddAdditionalLimitRestrictionAsync(AddAdditionalLimitRestrictionCommand command);
    Task<CardRenewalResponse> CardRenewalAsync(CardRenewalCommand command);
    Task<GetCardStatusResponse> GetCardStatusAsync(GetCardStatusQuery command);

}
