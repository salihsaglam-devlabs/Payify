using LinkPara.Cache;
using LinkPara.HttpProviders.BusinessParameter.Models;
using Microsoft.AspNetCore.Http;
using System.Text;
using System.Text.Json;

namespace LinkPara.HttpProviders.BusinessParameter;

public class ParameterService : HttpClientBase, IParameterService
{
    private readonly ICacheService _cacheService;
    public ParameterService(HttpClient client, ICacheService cacheService, IHttpContextAccessor httpContextAccessor)
        : base(client, httpContextAccessor)
    {
        _cacheService = cacheService;
    }

    public async Task<ParameterDto> GetParameterAsync(string groupCode, string parameterCode)
    {
        var key = new StringBuilder();
        key.Append(groupCode);
        key.Append('_');
        key.Append(parameterCode);

        var parameter = await _cacheService.GetOrCreateAsync<ParameterDto>(key.ToString(),
        async () =>
        {
            var response = await GetAsync($"v1/Parameters/getByCode?groupCode={groupCode}&parameterCode={parameterCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ParameterDto>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                throw new InvalidOperationException();
            }
        });

        return parameter;
    }

    public async Task<List<ParameterDto>> GetParametersAsync(string groupCode)
    {
        var parameters = await _cacheService.GetOrCreateAsync<List<ParameterDto>>(groupCode,
            async () =>
            {
                var response = await GetAsync($"v1/Parameters/{groupCode}");

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<List<ParameterDto>>(responseString,
                     new JsonSerializerOptions
                     {
                         PropertyNameCaseInsensitive = true
                     });
                }
                else
                {
                    throw new InvalidOperationException();
                }
            });

        return parameters;
    }
    public async Task<ParameterTemplateDto> GetParameterTemplateAsync(string groupCode, string templateCode)
    {
        var key = new StringBuilder();
        key.Append(groupCode);
        key.Append("_");
        key.Append(templateCode);

        var parameterTemplate = await _cacheService.GetOrCreateAsync<ParameterTemplateDto>(key.ToString(),
        async () =>
        {
            var response = await GetAsync($"v1/ParameterTemplates/?groupCode={groupCode}&templateCode={templateCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ParameterTemplateDto>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                throw new InvalidOperationException();
            }
        });

        return parameterTemplate;
    }
    public async Task<ParameterTemplateValueDto> GetParameterTemplateValueAsync(string groupCode, string parameterCode, string templateCode)
    {
        var key = new StringBuilder();
        key.Append(groupCode);
        key.Append("_");
        key.Append(parameterCode);
        key.Append("_");
        key.Append(templateCode);

        var parameterTemplateValue = await _cacheService.GetOrCreateAsync<ParameterTemplateValueDto>(key.ToString(),
        async () =>
        {
            var response = await GetAsync($"v1/ParameterTemplateValues/?groupCode={groupCode}&parameterCode={parameterCode}&templateCode={templateCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<ParameterTemplateValueDto>(responseString,
                 new JsonSerializerOptions
                 {
                     PropertyNameCaseInsensitive = true
                 });
            }
            else
            {
                throw new InvalidOperationException();
            }
        });

        return parameterTemplateValue;
    }

    public async Task<List<ParameterTemplateValueDto>> GetAllParameterTemplateValuesAsync(string groupCode, string parameterCode)
    {
        var key = new StringBuilder();
        key.Append(groupCode);
        key.Append("_");
        key.Append(parameterCode);

        var parameterTemplateValues = await _cacheService.GetOrCreateAsync<List<ParameterTemplateValueDto>>(key.ToString(),
        async () =>
        {
            var response = await GetAsync($"v1/ParameterTemplateValues/getAll?groupCode={groupCode}&parameterCode={parameterCode}");

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<List<ParameterTemplateValueDto>>(responseString,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            }
            else
            {
                throw new InvalidOperationException();
            }
        });

        return parameterTemplateValues;
    }
}
