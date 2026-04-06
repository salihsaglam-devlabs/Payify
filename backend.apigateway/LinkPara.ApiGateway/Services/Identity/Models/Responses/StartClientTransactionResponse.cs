using LinkPara.HttpProviders.MultiFactor.Models.PowerFactorModels;

namespace LinkPara.ApiGateway.Services.Identity.Models.Responses;

public class StartClientTransactionResponse: PowerFactorResponseBase
{
    public string TransactionToken { get; set; }
}