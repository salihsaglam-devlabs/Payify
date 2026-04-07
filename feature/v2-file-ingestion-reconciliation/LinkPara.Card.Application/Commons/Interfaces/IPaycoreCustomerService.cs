using LinkPara.Card.Application.Commons.Models.PaycoreModels;
using LinkPara.Card.Application.Commons.Models.PaycoreModels.CustomerModels;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.CreateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomer;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerAddress;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerCommunication;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Commands.UpdateCustomerLimit;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerCards;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerInformation;
using LinkPara.Card.Application.Features.PaycoreServices.CustomerServices.Queries.GetCustomerLimitInfo;

namespace LinkPara.Card.Application.Commons.Interfaces;
public interface IPaycoreCustomerService
{
    Task<PaycoreResponse> CreateCustomerAsync(CreateCustomerCommand command);
    Task<GetCustomerInformationResponse> GetCustomerInformationAsync(GetCustomerInformationQuery query);
    Task<List<GetCustomerCardsResponse>> GetCustomerCardsAsync(GetCustomerCardsQuery query);
    Task<List<GetCustomerLimitInfoResponse>> GetCustomerLimitInfoAsync(GetCustomerLimitInfoQuery query);
    Task<PaycoreResponse> UpdateCustomerAsync(UpdateCustomerCommand command);
    Task<PaycoreResponse> UpdateCustomerCommunicationAsync(UpdateCustomerCommunicationCommand command);
    Task<PaycoreResponse> UpdateCustomerAddressAsync(UpdateCustomerAddressCommand command);
    Task<PaycoreResponse> UpdateCustomerLimitAsync(UpdateCustomerLimitCommand command);
}
