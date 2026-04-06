using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models.Request;
using LinkPara.ApiGateway.BackOffice.Services.BusinessParameter.Models;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Pagination;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.HttpClients
{
    public class MerchantHistoryHttpClient : HttpClientBase, IMerchantHistoryHttpClient
    {
        public MerchantHistoryHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
        {
        }
        public async Task<PaginatedList<MerchantHistoryDto>> GetFilterListAsync(GetFilterMerchantHistoryRequest request)
        {
            var url = CreateUrlWithParams($"v1/MerchantHistory/filter", request, true);
            var response = await GetAsync(url);
            var merchantHistories = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantHistoryDto>>();

            if (!CanSeeSensitiveData())
            {
                merchantHistories.Items.ForEach(s =>
                {
                    s.MerchantName = SensitiveDataHelper.MaskSensitiveData("Name", s.MerchantName != null ? s.MerchantName : string.Empty);
                });
            }

            return merchantHistories ?? throw new InvalidOperationException();
        }

        public async Task<PaginatedList<MerchantHistoryDto>> GetAllParameterAsync(GetAllMerchantHistoryRequest request)
        {
            var url = CreateUrlWithParams($"v1/MerchantHistory", request, true);
            var response = await GetAsync(url);
            var merchantHistories = await response.Content.ReadFromJsonAsync<PaginatedList<MerchantHistoryDto>>();

            if (!CanSeeSensitiveData())
            {
                merchantHistories.Items.ForEach(s =>
                {
                    s.MerchantName = SensitiveDataHelper.MaskSensitiveData("Name", s.MerchantName != null ? s.MerchantName : string.Empty);
                });
            }

            return merchantHistories ?? throw new InvalidOperationException();
        }
    }
}
