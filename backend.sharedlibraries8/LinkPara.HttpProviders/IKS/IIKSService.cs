using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.IKS.Models.Response;

namespace LinkPara.HttpProviders.IKS
{
    public interface IIKSService
    {
        Task<IKSResponse<IKSAnnulmentResponse>> SaveAnnulmentAsync(IKSSaveAnnulmentRequest request);
        Task<IKSResponse<IKSAnnulmentResponse>> UpdateAnnulmentAsync(IKSUpdateAnnulmentRequest request);
        Task<IKSResponse<IKSMerchantResponse>> SaveMerchantAsync(IKSSaveMerchantRequest request);
        Task<IKSResponse<IKSMerchantResponse>> UpdateMerchantAsync(IKSUpdateMerchantRequest request);
        Task<IKSResponse<IKSTerminalResponse>> SaveTerminalAsync(IKSSaveTerminalRequest request);
        Task<IKSResponse<IKSTerminalResponse>> UpdateTerminalAsync(IKSUpdateTerminalRequest request);
        Task<IKSResponse<AnnulmentsQueryResponse>> AnnulmentQueryAsync(IKSAnnulmentsQueryRequest request);
        Task<IKSResponse<IKSTerminalResponse>> CreateTerminalAsync(IKSCreateTerminalRequest request);
        Task<IKSResponse<IKSTerminalResponse>> GetTerminalStatusQueryAsync(IKSGetTerminalStatusRequest request);
	}
}
