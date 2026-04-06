using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.Order;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.Stock;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Order;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Balance;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Brand;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Publisher;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Stock;
using LinkPara.Epin.Infrastructure.Services.Secrets;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LinkPara.Epin.Application.Commons.Exceptions;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.Reconciliation;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Reconciliation;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.OrderHistory;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.OrderHistory;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses;
using System.Globalization;
using System.Text.Json.Serialization;

namespace LinkPara.Epin.Infrastructure.Services.EpinHttpClients;

public class PerdigitalHttpClient : IEpinHttpClient
{
    private readonly HttpClient _client;

    private readonly JsonSerializerOptions jsonSerializerOption = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
    };

    public PerdigitalHttpClient(HttpClient client
        , SecretService secrets)
    {
        _client = client;
        _client.BaseAddress = new Uri(secrets.PerdigitalApiSettings.Url);
        _client.DefaultRequestHeaders.Add("username", secrets.PerdigitalApiSettings.UserName);
        _client.DefaultRequestHeaders.Add("apikey", secrets.PerdigitalApiSettings.ApiKey);
    }

    protected async Task<HttpResponseMessage> GetAsync(string requestUri)
    {
        var response = await _client.GetAsync(requestUri);
        return response;
    }

    protected async Task<HttpResponseMessage> PostAsJsonAsync<T>(string requestUri, T value)
    {
        var response = await _client.PostAsJsonAsync(requestUri, value);
        return response;
    }

    public async Task<PublisherServiceResponse> GetPublishersAsync()
    {
        var response = await GetAsync($"publishers");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<PublisherServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    public async Task<BrandServiceResponse> GetBrandsAsync(int publisherId)
    {
        var response = await GetAsync($"publisher/{publisherId}/brands");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<BrandServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    public async Task<ProductServiceResponse> GetProductsAsync(int publisherId, int brandId)
    {
        var response = await GetAsync($"publisher/{publisherId}/brand/{brandId}/products");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<ProductServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    public async Task<BalanceServiceResponse> GetBalanceAsync()
    {
        var response = await GetAsync($"balance");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<BalanceServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    public async Task<bool> CheckStockAsync(int productId)
    {
        var request = new List<StockServiceRequest>
        {
            new StockServiceRequest
            {
                product = productId,
                quantity = 1
            }
        };
        string jsonString = JsonSerializer.Serialize(request);

        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");


        var formContent = new MultipartFormDataContent
        {
            {new StringContent(jsonString),"data"}
        };
        var response = await _client.PostAsync("stock", formContent);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<StockServiceResponse>(serviceResult, jsonSerializerOption);
        return result.Error == 0;
    }

    public async Task<OrderServiceResponse> CreateOrderAsync(OrderServiceRequest request)
    {

        string jsonString = JsonSerializer.Serialize(new List<OrderServiceRequest> { request });

        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");


        var formContent = new MultipartFormDataContent
        {
            {new StringContent(jsonString),"data"}
        };
        var response = await _client.PostAsync("order", formContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<OrderServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    public async Task<ReconciliationServiceResponse> GetReconciliationResultByDateAsync(DateTime date)
    {
        var requestedDay = date.ToString("yyyy-MM-dd");

        var reconciliationServiceRequest = new ReconciliationServiceRequest { startDate = requestedDay, endDate = requestedDay };

        string jsonString = JsonSerializer.Serialize(reconciliationServiceRequest);

        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");


        var formContent = new MultipartFormDataContent
        {
            {new StringContent(jsonString),"data"}
        };
        var response = await _client.PostAsync("reconciliation", formContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }
        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<ReconciliationServiceResponse>(serviceResult, jsonSerializerOption);
        return result;
    }

    private void HandleError(int errorCode, string errorMessage)
    {
        switch (errorCode)
        {
            case -1:
                throw new AuthorizationFailedException(errorMessage);
            case -2:
                throw new PublisherListException(errorMessage);
            case -3:
                throw new BrandListException(errorMessage);
            case -4:
                throw new ProductListException(errorMessage);
            case -5:
                throw new SendDataException(errorMessage);
            case -6:
                throw new InsufficientBalanceException(errorMessage);
            case -7:
                throw new ProductNotFoundException(errorMessage);
            case -8:
                throw new InsufficientStockException(errorMessage);
            case -9:
                throw new OrderNotFoundException(errorMessage);
            case -10:
                throw new AuthorizationErrorException(errorMessage);
            case -11:
                throw new JoygameUserNotFoundException(errorMessage);
            case -13:
                throw new AlreadyExistsReferenceException(errorMessage);
            case -99:
                throw new PerdigitalSystemException(errorMessage);
            default: break;
        }
    }

    public async Task ReconciliationApproveByDateAsync(DateTime reconciliationDate)
    {
        var requestedDay = reconciliationDate.ToString("yyyy-MM-dd");

        var reconciliationServiceRequest = new ReconciliationServiceRequest { startDate = requestedDay, endDate = requestedDay };

        string jsonString = JsonSerializer.Serialize(reconciliationServiceRequest);

        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");


        var formContent = new MultipartFormDataContent
        {
            {new StringContent(jsonString),"data"}
        };
        var response = await _client.PostAsync("reconciliation_approved", formContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }

        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<ReconciliationApprovedServiceResponse>(serviceResult, jsonSerializerOption);
    }

    public async Task<OrderHistoryServiceResponse> GetOrderHistoryByDateAsync(DateTime date)
    {
        var requestedDay = date.ToString("yyyy-MM-dd");

        var orderHistoryServiceRequest = new OrderHistoryServiceRequest { startDate = requestedDay, endDate = requestedDay };

        string jsonString = JsonSerializer.Serialize(orderHistoryServiceRequest);

        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "multipart/form-data");


        var formContent = new MultipartFormDataContent
        {
            {new StringContent(jsonString),"data"}
        };
        var response = await _client.PostAsync("order_history", formContent);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpFailedException(response.StatusCode.ToString());
        }

        var serviceResult = await response.Content.ReadAsStringAsync();

        var baseMessage = JsonSerializer.Deserialize<BaseServiceResponse>(serviceResult, jsonSerializerOption);
        HandleError(baseMessage.Error, baseMessage.Message);

        var result = JsonSerializer.Deserialize<OrderHistoryServiceResponse>(serviceResult, jsonSerializerOption);
        return result;

    }
}