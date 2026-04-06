using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Commons.Models.VposModels.Response;
using Microsoft.AspNetCore.Http;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IVposApi
{
    Task<PosPaymentProvisionResponse> PaymentNonSecure(PosPaymentNonSecureRequest request);
    Task<PosInit3DModelResponse> Init3DModel(PosInit3DModelRequest request);
    Task<PosPaymentProvisionResponse> Payment3DModel(PosPayment3DModelRequest request);
    Task<PosVerify3dModelResponse> Verify3DModel(Dictionary<string,string> form);
    Task<PosPaymentProvisionResponse> PostAuth(PosPostAuthRequest request);
    Task<PosVoidResponse> Void(PosVoidRequest request);
    Task<PosRefundResponse> Refund(PosRefundRequest request);
    Task<PosPaymentDetailResponse> PaymentDetail(PosPaymentDetailRequest request);
    Task<PosPointInquiryResponse> PointInquiry(PosPointInquiryRequest request);
    void SetServiceParameters(object serviceParameters, Guid? merchantId = null);
}
