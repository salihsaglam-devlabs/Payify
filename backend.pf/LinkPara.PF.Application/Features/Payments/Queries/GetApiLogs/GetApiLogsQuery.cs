using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.BusModels.Commands.Scheduler.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Application.Features.Payments.Queries.GetApiLogs;

public class GetApiLogsQuery : SearchQueryParams, IRequest<PaginatedList<ApiLogModel>>
{
    public Guid? MerchantId { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public PaymentLogType? PaymentType { get; set; }
    public string ApiLogRequest { get; set; }
    public string ApiLogResponse { get; set; }
}
public class GetApiLogsQueryHandler : IRequestHandler<GetApiLogsQuery, PaginatedList<ApiLogModel>>
{
    private readonly IGenericRepository<MerchantApiLog> _repository;
    private readonly IMapper _mapper;

    public GetApiLogsQueryHandler(IGenericRepository<MerchantApiLog> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }
    public async Task<PaginatedList<ApiLogModel>> Handle(GetApiLogsQuery request, CancellationToken cancellationToken)
    {
        var apiLogs = _repository.GetAll().Include(b => b.Merchant).AsQueryable();

        if (request.MerchantId is not null)
        {
            apiLogs = apiLogs.Where(b => b.MerchantId
                               == request.MerchantId);
        }

        if (request.PaymentType is not null)
        {
            apiLogs = apiLogs.Where(b => b.PaymentType
                               == request.PaymentType);
        }

        if (request.CreateDateStart is not null)
        {
            apiLogs = apiLogs.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            apiLogs = apiLogs.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (!string.IsNullOrEmpty(request.ApiLogResponse))
        {
            apiLogs = apiLogs.Where(b => b.Response.ToLower().Contains(request.ApiLogResponse.ToLower()));
        }

        if (!string.IsNullOrEmpty(request.ApiLogRequest))
        {
            apiLogs = apiLogs.Where(b => b.Request.ToLower().Contains(request.ApiLogRequest.ToLower()));
        }

        return await apiLogs
           .PaginatedListWithMappingAsync<MerchantApiLog,ApiLogModel>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}
