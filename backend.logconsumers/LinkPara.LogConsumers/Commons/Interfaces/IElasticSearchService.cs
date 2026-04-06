
namespace LinkPara.LogConsumers.Commons.Interfaces;

public interface IElasticSearchService
{
    Task CreateIndexAsync<T>(string index) where T : class;
    Task InsertDocumentAsync<T>(string indexName, T model) where T : class;
    Task InsertDocumentsAsync<T>(string indexName, List<T> list) where T : class;
    Task BulkPayloadAsync(string index, List<string> payload);
}
