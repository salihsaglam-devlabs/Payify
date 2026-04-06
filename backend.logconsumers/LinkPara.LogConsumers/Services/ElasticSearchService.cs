using Elasticsearch.Net;
using LinkPara.HttpProviders.Vault;
using LinkPara.LogConsumers.Commons.ElasticSearchConfiguration;
using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.LogConsumers.Commons.Models;
using LinkPara.LogConsumers.Commons.Models.Enums;
using Nest;

namespace LinkPara.LogConsumers.Services;

public class ElasticSearchService : IElasticSearchService
{
    private readonly IElasticClient _client;
    private readonly ILogger<ElasticSearchService> _logger;
    private readonly IVaultClient _vaultClient;
    private readonly IElasticLowLevelClient _lowLevelClient;

    public ElasticSearchService(ILogger<ElasticSearchService> logger,
        IVaultClient vaultClient)
    {
        _logger = logger;
        _vaultClient = vaultClient;
        _client = CreateInstance();
        _lowLevelClient = CreateLowLevelClient();
    }

    private IElasticLowLevelClient CreateLowLevelClient()
    {
        var elasticSearchSettings = _vaultClient.GetSecretValue<ElasticSearchSettings>("LogConsumerSecrets", "ElasticSearchSettings");

        var settings = new ConnectionSettings(new Uri($"{elasticSearchSettings.Host}:{elasticSearchSettings.Port}"))
            .PrettyJson()
            .EnableApiVersioningHeader()
            .RequestTimeout(TimeSpan.FromMinutes(1));

        if (!string.IsNullOrWhiteSpace(elasticSearchSettings.Username))
        {
            settings.BasicAuthentication(elasticSearchSettings.Username, elasticSearchSettings.Password);
        }

        return new ElasticLowLevelClient(settings);

    }

    public ElasticClient CreateInstance()
    {
        var elasticSearchSettings = _vaultClient.GetSecretValue<ElasticSearchSettings>("LogConsumerSecrets", "ElasticSearchSettings");

        var settings = new ConnectionSettings(new Uri($"{elasticSearchSettings.Host}:{elasticSearchSettings.Port}"))
            .PrettyJson()
            .EnableApiVersioningHeader()
            .RequestTimeout(TimeSpan.FromMinutes(1));

        if (!string.IsNullOrWhiteSpace(elasticSearchSettings.Username))
        {
            settings.BasicAuthentication(elasticSearchSettings.Username, elasticSearchSettings.Password);
        }

        return new ElasticClient(settings);
    }

    public async Task InsertDocumentAsync<T>(string indexName, T model) where T : class
    {
        var response = await _client.IndexAsync(model, q => q.Index(indexName));

        if (!response.IsValid)
        {
            _logger.LogError($"InsertDocumentError : {(response.ServerError is null ? response.OriginalException : response.ServerError)}");
        }
    }

    public async Task InsertDocumentsAsync<T>(string indexName, List<T> list) where T : class
    {
        var response = await _client.IndexManyAsync(list, index: indexName);

        if (!response.IsValid)
        {
            _logger.LogError($"InsertDocumentError : {(response.ServerError is null ? response.OriginalException : response.ServerError)}");
        }
    }

    public async Task CreateIndexAsync<T>(string index) where T : class
    {
        var indice = await _client.Indices.ExistsAsync(index);

        if (!indice.Exists)
        {
            var response = await _client.Indices.CreateAsync(index,
            ci => ci
                .Index(index)
                .Map<T>(m => m.AutoMap()));

            if (!response.Acknowledged)
            {
                _logger.LogError("CreateIndexError: {ServerError} Exception : {OriginalException}", response.ServerError, response.OriginalException);
            }
        }
    }

    public async Task BulkPayloadAsync(string index, List<string> payload)
    {
        var indice = await _client.Indices.ExistsAsync(index);
        var operationType = ElasticOperationType.Index;
        if (!indice.Exists)
        {
            var createIndexResponse = await _client.Indices.CreateAsync(index,
            ci => ci
                .Index(index)
                .Map(m => m.AutoMap()));

            if (!createIndexResponse.Acknowledged)
            {
                throw new Exception($"CreateIndexError {createIndexResponse.ServerError} Exception : {createIndexResponse.OriginalException}");
            }
        }

        List<string> payloadForElasticSearch = new List<string>();
        foreach (var item in payload)
        {
            var action = CreateElasticAction(
            opType: operationType,
            indexName: index,
            pipelineName: null,
            mappingType: "_doc");

            payloadForElasticSearch.Add(LowLevelRequestResponseSerializer.Instance.SerializeToString(action));
            payloadForElasticSearch.Add(item);
            operationType = ElasticOperationType.Index;
        }

        var response = await _lowLevelClient.BulkAsync<DynamicResponse>(PostData.MultiJson(payloadForElasticSearch));

        if (!response.Success)
        {
            throw new Exception($"Received failed ElasticSearch shipping result {response.HttpStatusCode}: {response.OriginalException}");
        }
    }

    internal static object CreateElasticAction(ElasticOperationType opType, string indexName, string pipelineName = null, string id = null, string mappingType = null)
    {
        var actionPayload = new ElasticActionPayload(
            indexName: indexName,
            pipeline: string.IsNullOrWhiteSpace(pipelineName) ? null : pipelineName,
            id: id,
            mappingType: mappingType
        );

        var action = opType == ElasticOperationType.Create
            ? (object)new ElasticCreateAction(actionPayload)
            : new ElasticIndexAction(actionPayload);
        return action;
    }

}