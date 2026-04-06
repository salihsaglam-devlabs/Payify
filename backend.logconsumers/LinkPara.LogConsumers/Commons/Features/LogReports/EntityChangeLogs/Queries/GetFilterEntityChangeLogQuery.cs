using AutoMapper;
using LinkPara.HttpProviders.Vault;
using LinkPara.LogConsumers.Commons.Entities;
using LinkPara.LogConsumers.Commons.Models;
using LinkPara.LogConsumers.Commons.Models.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Nest;

namespace LinkPara.LogConsumers.Commons.Features.LogReports.EntityChangeLogs.Queries;

public class GetFilterEntityChangeLogQuery : SearchQueryParams, MediatR.IRequest<PaginatedList<EntityChangeLogDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string TableName { get; set; }
    public CrudOperationType? CrudOperationType { get; set; }
    public string UserId { get; set; }
    public string UserName { get; set; }
    public ServiceName? ServiceName { get; set; }
    public string CorrelationId { get; set; }
}

public class GetFilterEntityChangeLogQueryHandler : IRequestHandler<GetFilterEntityChangeLogQuery, PaginatedList<EntityChangeLogDto>>
{
    private readonly IVaultClient _vaultClient;

    public GetFilterEntityChangeLogQueryHandler(IVaultClient vaultClient)
    {
        _vaultClient = vaultClient;
    }
    public async Task<PaginatedList<EntityChangeLogDto>> Handle(GetFilterEntityChangeLogQuery request, CancellationToken cancellationToken)
    {
        var host = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Host");
        var port = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Port");
        var username = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Username");
        var password = _vaultClient.GetSecretValue<string>("SharedSecrets", "ElasticSearchSettings", "Password");

        var settings = new ConnectionSettings(new Uri(host + ":" + port))
               .DefaultIndex("entitychange-log-*")
               .BasicAuthentication(username,password)
               .EnableDebugMode();

        var client = new ElasticClient(settings);

        var mustQueries = new List<Func<QueryContainerDescriptor<EntityChangeLogModel>, QueryContainer>>();

        if (request.CreateDateStart.HasValue || request.CreateDateEnd.HasValue)
        {
            mustQueries.Add(bs => bs.DateRange(dr => dr
                .Field(f => f.LogDate)
                .GreaterThanOrEquals(request.CreateDateStart)
                .LessThanOrEquals(request.CreateDateEnd)
            ));
        }

        if (!string.IsNullOrEmpty(request.TableName))
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.TableName.Suffix("keyword")).Value(request.TableName)));
        }

        if (request.CrudOperationType.HasValue)
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.CrudOperationType).Value(request.CrudOperationType)));
        }

        if (!string.IsNullOrEmpty(request.UserId))
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.UserId.Suffix("keyword")).Value(request.UserId)));
        }

        if (request.ServiceName.HasValue)
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.ServiceName.Suffix("keyword")).Value(request.ServiceName.ToString().ToLowerInvariant())));
        }

        if (!string.IsNullOrEmpty(request.CorrelationId))
        {
            mustQueries.Add(bs => bs.Term(t => t.Field(f => f.CorrelationId.Suffix("keyword")).Value(request.CorrelationId)));
        }

        var sortOrder = request.OrderBy == OrderByStatus.Asc ? SortOrder.Ascending : SortOrder.Descending;

        var fieldsRequiringKeyword = new List<string> { "serviceName", "tableName", "userName" };

        var sortField = fieldsRequiringKeyword.Contains(request.SortBy)
            ? request.SortBy + ".keyword"
            : request.SortBy;

        var searchResponse = await client.SearchAsync<EntityChangeLogModel>(s => s
            .From((request.Page - 1) * request.Size)
            .Size(request.Size)
            .Query(q => q
                .Bool(b => b
                    .Must(mustQueries.ToArray())
                    .MustNot(mn => mn
                        .Bool(bn => bn
                            .Should(
                                sh => sh.Bool(bo => bo
                                    .Must(
                                        m => m.Match(ma => ma
                                            .Field(f => f.AffectedColumns)
                                            .Query("UpdateDate")
                                        ),
                                        m => m.Script(sc => sc
                                            .Script(s => s.Source("doc['affectedColumns.keyword'].size() == 1"))
                                        )
                                    )
                                ),
                                sh => sh.Bool(bo => bo
                                    .Must(
                                        m => m.Match(ma => ma
                                            .Field(f => f.AffectedColumns)
                                            .Query("UpdateDate")
                                        ),
                                        m => m.Match(ma => ma
                                            .Field(f => f.AffectedColumns)
                                            .Query("LastModifiedBy")
                                        ),
                                        m => m.Script(sc => sc
                                            .Script(s => s.Source("doc['affectedColumns.keyword'].size() == 2"))
                                        )
                                    )
                                )
                            )
                        )
                    )
                )
            )
            .Sort(s => sortField is not null ?
                s.Field(new Field(sortField), sortOrder) : null
            )
        );

        var logs = searchResponse.Documents;

        var logDtos = logs
            .Select(log => new EntityChangeLogDto
                {
                    ServiceName = MapServiceName(log.ServiceName),
                    TableName = log.TableName,
                    CrudOperationType = log.CrudOperationType,
                    UserId = log.UserId,
                    LogDate = log.LogDate,
                    OldValues = log.OldValues,
                    NewValues = log.NewValues,
                    AffectedColumns = log.AffectedColumns,
                    CorrelationId = log.CorrelationId,
                }).ToList();

        var paginatedList = new PaginatedList<EntityChangeLogDto>(
            logDtos,
            (int)searchResponse.Total,
            request.Page,
            request.Size,
            request.OrderBy,
            request.SortBy
        );

        return paginatedList;
    }

    private ServiceName MapServiceName(string serviceName)
    {
        return serviceName.ToLower() switch
        {
            "approval" => ServiceName.Approval,
            "businessparameter" => ServiceName.BusinessParameter,
            "document" => ServiceName.Document,
            "pf" => ServiceName.PF,
            "identity" => ServiceName.Identity,
            _ => ServiceName.None
        };
    }
}
