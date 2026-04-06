using LinkPara.ApiGateway.BackOffice.Commons.Extensions;
using LinkPara.ApiGateway.BackOffice.Commons.Helpers;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Enums;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Requests;
using LinkPara.ApiGateway.BackOffice.Services.EMoney.Models.Responses;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace LinkPara.ApiGateway.BackOffice.Services.EMoney.HttpClients;

public class CompanyPoolHttpClient : HttpClientBase, ICompanyPoolHttpClient
{
    private readonly IServiceRequestConverter _serviceRequestConverter;

    public CompanyPoolHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor, IServiceRequestConverter serviceRequestConverter) 
        : base(client, httpContextAccessor)
    {
        _serviceRequestConverter = serviceRequestConverter;
    }

    public async Task ApproveCompanyPoolAsync(ApproveCompanyPoolRequest request)
    {
        var serviceRequest = _serviceRequestConverter.Convert<ApproveCompanyPoolRequest,ApproveCompanyPoolServiceRequest>(request);

        var response = await PutAsJsonAsync($"v1/CompanyPools/approve", serviceRequest);

        if (!response.IsSuccessStatusCode)
            throw new InvalidOperationException();
    }

    public async Task<List<CompanyDocumentTypeResponse>> GetCompanyDocumentTypesAsync(GetCompanyDocumentTypesRequest request)
    {
        var queryString = request.GetQueryString();
        var response = await GetAsync($"v1/CompanyPools/document-types{queryString}");
        var responseString = await response.Content.ReadAsStringAsync();
        var documentTypes = JsonSerializer.Deserialize<List<CompanyDocumentTypeResponse>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return documentTypes ?? throw new InvalidOperationException();
    }

    public async Task<CompanyPoolDto> GetCompanyPoolAsync(Guid id)
    {
        var response = await GetAsync($"v1/CompanyPools/{id}");

        if (!response.IsSuccessStatusCode)
        {
            throw new NotFoundException(nameof(CompanyPoolDto), id);
        }

        var result = await response.Content.ReadFromJsonAsync<CompanyPoolDto>();

        return result ?? throw new InvalidOperationException();
    }

    public async Task<PaginatedList<CompanyPoolDto>> GetCompanyPoolsListAsync(GetCompanyPoolListRequest request)
    {
        var url = CreateUrlWithParams("v1/CompanyPools", request, true);
        var response = await GetAsync(url);
        return await response.Content.ReadFromJsonAsync<PaginatedList<CompanyPoolDto>>();
    }

    public async Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync(SaveCompanyPoolRequest request)
    {
        request.Channel = CompanyPoolChannel.Operation;

        var content = await PrepareFormDataContentAsync(request);

        var response = await PostAsync("v1/CompanyPools", content);

        var user = await response.Content.ReadFromJsonAsync<SaveCompanyPoolResponse>();

        return user ?? throw new InvalidOperationException();
    }

    private async Task<MultipartFormDataContent> PrepareFormDataContentAsync(SaveCompanyPoolRequest request)
    {

        var content = new MultipartFormDataContent();
        PropertyInfo[] properties = typeof(SaveCompanyPoolRequest)!.GetProperties();

        foreach (PropertyInfo propertyInfo in properties)
        {
            object value = typeof(SaveCompanyPoolRequest)!.GetProperty(propertyInfo.Name)!.GetValue(request, null);
            PropertyInfo property = typeof(SaveCompanyPoolRequest)!.GetProperty(propertyInfo.Name);


            if ((object)property != null)
            {
                if (property.Name == "Documents" && value is not null)
                {
                    var documents = (List<CompanyPoolDocumentRequest>)propertyInfo.GetValue(request, null);

                    for (var i = 0; i < documents.Count; i++)
                    {
                        var document = documents[i];
                        await using var memoryStream = new MemoryStream();
                        await document.File.CopyToAsync(memoryStream);

                        byte[] fileBytes = memoryStream.ToArray();
                        ByteArrayContent byteArrayContent = new ByteArrayContent(fileBytes);
                        byteArrayContent.Headers.ContentType = MediaTypeHeaderValue.Parse(document.File.ContentType);
                        content.Add(byteArrayContent, $"Documents[{i}].File", document.File.FileName);
                        content.Add(new StringContent(document.DocumentTypeId.ToString()), $"Documents[{i}].DocumentTypeId");
                    }
                }
                else if (value is not null)
                {
                    content.Add(new StringContent(value?.ToString()), propertyInfo.Name);
                }

            }
        }

        return content;
    }
}
