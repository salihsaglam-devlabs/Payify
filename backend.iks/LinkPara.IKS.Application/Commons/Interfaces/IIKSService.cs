using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Annulments.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;
using LinkPara.IKS.Application.Features.Annulments.Queries.GetAnnulments;
using LinkPara.IKS.Domain.Entities;
using IKSMerchantResponse = LinkPara.IKS.Application.Commons.Models.IKSModels.Merchants.Response.IKSMerchantResponse;

namespace LinkPara.IKS.Application.Commons.Interfaces
{
    public interface IIKSService
    {
        Task<IKSResponse<IKSMerchantResponse>> SaveMerchantAsync(MerchantRequest request);
        Task<IKSResponse<IKSMerchantResponse>> UpdateMerchantAsync(UpdateMerchantRequest request);
        Task<IKSResponse<IKSMerchantsQueryResponse>> MerchantsQueryAsync(MerchantsQueryRequest request);
        Task<IKSResponse<IKSTerminalResponse>> SaveTerminalAsync(SaveTerminalRequest request);
        Task<IKSResponse<IKSTerminalResponse>> CreateTerminalAsync(CreateTerminalRequest request);
        Task<IKSResponse<IKSTerminalResponse>> UpdateTerminalAsync(UpdateTerminalRequest request);
        Task<IKSResponse<IKSAnnulmentResponse>> SaveAnnulmentAsync(SaveAnnulmentRequest request);
        Task<IKSResponse<IKSAnnulmentResponse>> UpdateAnnulmentAsync(UpdateAnnulmentRequest request);
        Task<IKSResponse<AnnulmentCodesResponse>> GetAnnulmentCodesAsync();
        Task<IKSResponse<IKSAnnulmentsQueryResponse>> GetAnnulmentsQueryAsync(GetAnnulmetsQuery request);
        Task<IKSResponse<IksTerminalQueryResponse>> GetTerminalStatusQueryAsync(GetTerminalStatusRequest request);
        Domain.Entities.IksTerminal UpdateIksTerminalFields(Domain.Entities.IksTerminal iksTerminal, Application.Commons.Models.IKSModels.Terminal.Response.IKSTerminal response);
        Domain.Entities.IksTerminal UpdatePassiveIksTerminalFields(Domain.Entities.IksTerminal iksTerminal, Application.Commons.Models.IKSModels.Terminal.Response.IKSTerminal response);
        Task CreateTerminalHistoryAsync(IksTerminal iksTerminal, IKSTerminal response);
    }
}
