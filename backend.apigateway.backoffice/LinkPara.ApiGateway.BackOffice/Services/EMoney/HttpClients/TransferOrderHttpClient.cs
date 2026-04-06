using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class TransferOrderHttpClient : HttpClientBase, ITransferOrderHttpClient
{

    public TransferOrderHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
    }

    public async Task<TransferOrderDto> GetTransferOrderByIdAsync(Guid transferOrderId)
    {
        var response = await GetAsync($"v1/TransferOrders/{transferOrderId}");
        var transferOrder = await response.Content.ReadFromJsonAsync<TransferOrderDto>();
        if (!CanSeeSensitiveData())
        {
            transferOrder.SenderNameSurname = SensitiveDataHelper.MaskSensitiveData("FullName",transferOrder.SenderNameSurname);
            transferOrder.ReceiverNameSurname = SensitiveDataHelper.MaskSensitiveData("FullName",transferOrder.ReceiverNameSurname);
            transferOrder.ReceiverAccountValue = SensitiveDataHelper.MaskSensitiveData(transferOrder.ReceiverAccountType.ToString(),transferOrder.SenderNameSurname);
        }
        return transferOrder ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<TransferOrderDto>> GetTransferOrdersByUserIdAsync(GetTransferOrdersRequest query)
    {
        var url = CreateUrlWithParams($"v1/TransferOrders", query, true);
        var response = await GetAsync(url);
        var transferOrders = await response.Content.ReadFromJsonAsync<PaginatedList<TransferOrderDto>>();
        if (!CanSeeSensitiveData())
        {
            transferOrders.Items.ForEach(s =>
            {
                s.SenderNameSurname = SensitiveDataHelper.MaskSensitiveData("FullName",s.SenderNameSurname);
                s.ReceiverNameSurname = SensitiveDataHelper.MaskSensitiveData("FullName",s.ReceiverNameSurname);
                s.ReceiverAccountValue = SensitiveDataHelper.MaskSensitiveData(s.ReceiverAccountType.ToString(),s.SenderNameSurname);
            });
        }
        return transferOrders ?? throw new InvalidOperationException();
    }
}
