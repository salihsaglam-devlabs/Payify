using LinkPara.HttpProviders.Emoney.Models;

namespace LinkPara.HttpProviders.Emoney;

public interface IOnUsPaymentService
{
    Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsPaymentRequest request);

    Task OnUsPaymentUpdateStatusAsync(OnUsPaymentUpdateStatusRequest request);
}
