using LinkPara.ApiGateway.CorporateWallet.Commons.Extensions;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Requests;
using LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.HttpClients;

public class CompanyPoolHttpClient : HttpClientBase, ICompanyPoolHttpClient
{
    public CompanyPoolHttpClient(HttpClient client, IHttpContextAccessor httpContextAccessor) : base(client, httpContextAccessor)
    {
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

    public async Task<List<TaxAdministrationsResponse>> GetTaxAdministrationsAsync()
    {
        var response = await GetAsync($"v1/CompanyPools/tax-administrations");
        var responseString = await response.Content.ReadAsStringAsync();
        var taxAdministrations = JsonSerializer.Deserialize<List<TaxAdministrationsResponse>>(responseString, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        return taxAdministrations ?? throw new InvalidOperationException();
    }

    public async Task<SaveCompanyPoolResponse> SaveCompanyPoolAsync(SaveCompanyPoolRequest request)
    {
        request.Channel = CompanyPoolChannel.Web;

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

