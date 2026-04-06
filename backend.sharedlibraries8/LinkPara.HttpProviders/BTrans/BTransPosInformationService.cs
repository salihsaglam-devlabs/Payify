using LinkPara.HttpProviders.BTrans.Models;
using Microsoft.AspNetCore.Http;

namespace LinkPara.HttpProviders.BTrans;

public class BTransPosInformationService : HttpClientBase, IBTransPosInformationService
{
    public BTransPosInformationService(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {

    }

    public async Task CreatePosInformationRecordsAsync(CreatePosInformationRecordsRequest request)
    {
        var response = await PostAsJsonAsync("v1/PosInformation", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }

    public async Task DeletePosInformationRecordAsync(DeletePosInformationRecordRequest request)
    {
        var response = await PostAsJsonAsync("v1/PosInformation/delete-record", request);

        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException();
        }
    }
}