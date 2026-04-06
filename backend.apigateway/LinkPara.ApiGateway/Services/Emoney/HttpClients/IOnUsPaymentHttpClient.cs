using LinkPara.ApiGateway.Services.Emoney.Models.Requests;
using LinkPara.HttpProviders.Emoney.Models;
using Microsoft.AspNetCore.Mvc;

namespace LinkPara.ApiGateway.Services.Emoney.HttpClients;

public interface IOnUsPaymentHttpClient
{
    Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsRequest request);
    Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId);
    Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync([FromQuery] OnUsPaymentApproveRequest request);
}