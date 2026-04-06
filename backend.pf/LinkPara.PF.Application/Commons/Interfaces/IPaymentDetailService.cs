using LinkPara.PF.Application.Commons.Models.VposModels.Response;

namespace LinkPara.PF.Application.Commons.Interfaces;

public interface IPaymentDetailService
{
    Task<PosPaymentDetailResponse> GetPaymentDetailAsync(string orderId);
}
