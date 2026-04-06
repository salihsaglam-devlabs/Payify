using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.IKS.Models.Response;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.BackOffice.Services.IKS.HttpClients
{
    public interface IAnnulmentHttpClient
    {
        Task<IKSResponse<AnnulmentCodesResponse>> AnnulmentCodesAsync();
        Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentsQueryAsync(AnnulmentsQueryRequest request);
        Task<IKSResponse<CardBinResponse>> GetCardBinAsync();
    }
}
