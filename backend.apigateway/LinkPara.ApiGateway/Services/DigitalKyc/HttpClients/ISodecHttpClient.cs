using LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;
using LinkPara.ApiGateway.Services.DigitalKyc.Models.Responses;

namespace LinkPara.ApiGateway.Services.DigitalKyc.HttpClients;

public interface ISodecHttpClient
{
    Task<SodecCreateSessionResponse> SodecCreateSessionAsync(SodecCreateSessionRequest request);
    Task<SodecCompleteSessionResponse> SodecCompleteSessionAsync(SodecCompleteSessionRequest request);
}
