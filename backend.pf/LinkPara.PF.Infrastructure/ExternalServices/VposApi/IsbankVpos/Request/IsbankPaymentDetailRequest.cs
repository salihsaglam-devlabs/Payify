using LinkPara.SharedModels.Exceptions;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankPaymentDetailRequest
{
    public string MerchantNumber { get; set; }
    public string RetrievalReferenceNumber { get; set; }
    public string OrderNumber { get; set; }

    public async Task<string> SendQueryRequestAsync(string baseUrl)
    {
        using HttpClient client = new HttpClient();
        
        string queryUrl = baseUrl + "/query/v2/virtualpos/orderhistory";

        string urlWithParams = $"{queryUrl}?merchant_number={Uri.EscapeDataString(MerchantNumber)}&retrieval_reference_number={Uri.EscapeDataString(RetrievalReferenceNumber)}";

        try
        {
            HttpResponseMessage response = await client.GetAsync(urlWithParams);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStringAsync();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"SendToRawDataQueue Error: VposName {VposConsts.IsBankVpos} - Exception {ex}");
        } 
    }
}