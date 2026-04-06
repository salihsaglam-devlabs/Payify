using AutoMapper;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Nest;

namespace LinkPara.LogConsumers.Commons.Features.LogReports.RequestResponseLogs.Queries;

public class GetRequestResponseLogQuery : SearchQueryParams, MediatR.IRequest<PaginatedList<RequestResponseLogCreated>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string Path { get; set; }
    public int? StatusCode { get; set; }
}

public class GetRequestResponseLogQueryHandler : IRequestHandler<GetRequestResponseLogQuery, PaginatedList<RequestResponseLogCreated>>
{
    private readonly IVaultClient _vaultClient;

    public GetRequestResponseLogQueryHandler(
    IMapper mapper,
    IVaultClient vaultClient)
    {
        _vaultClient = vaultClient;
    }
    public async Task<PaginatedList<RequestResponseLogCreated>> Handle(GetRequestResponseLogQuery request, CancellationToken cancellationToken)
    {
        var host = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Host");
        var port = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Port");

        var settings = new ConnectionSettings(new Uri(host + ":" + port))
               .DefaultIndex("request-response-log-*")
               .EnableDebugMode();

        var client = new ElasticClient(settings);

        var mustQueries = new List<Func<QueryContainerDescriptor<RequestResponseLogCreated>, QueryContainer>>();

        mustQueries.Add(bs => bs.Term(t => t.Field(f => f.Request.Suffix("host.keyword")).Value("10.222.21.16:5007")));

        if (request.CreateDateStart.HasValue || request.CreateDateEnd.HasValue)
        {
            mustQueries.Add(bs => bs.DateRange(dr => dr
                .Field(f => f.CreatedDate)
                .GreaterThanOrEquals(request.CreateDateStart)
                .LessThanOrEquals(request.CreateDateEnd)
            ));
        }

        if (!string.IsNullOrEmpty(request.Path))
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.Request.Suffix("path.keyword")).Value(request.Path)));
        }

        if (request.StatusCode is not null)
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.Response.Suffix("statusCode")).Value(request.StatusCode)));
        }

        var sortOrder = request.OrderBy == OrderByStatus.Asc ? SortOrder.Ascending : SortOrder.Descending;

        var fieldsRequiringKeyword = new List<string> { "serviceName", "tableName", "userName" };

        var sortField = fieldsRequiringKeyword.Contains(request.SortBy)
            ? request.SortBy + ".keyword"
            : request.SortBy;

        var searchResponse = await client.SearchAsync<RequestResponseLogCreated>(s => s
            .From((request.Page - 1) * request.Size)
            .Size(request.Size)
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray())
                )
            )
            .Sort(s => sortField is not null ?
                s.Field(new Field(sortField), sortOrder) : null
            )
        );

        var logs = searchResponse.Documents;

        var paginatedList = new PaginatedList<RequestResponseLogCreated>(
            logs.ToList(),
            (int)searchResponse.Total,
            request.Page,
            request.Size,
            request.OrderBy,
            request.SortBy
        );

        return paginatedList;
    }
}
