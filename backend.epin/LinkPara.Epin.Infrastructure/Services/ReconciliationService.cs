using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Epin.Application.Commons.Exceptions;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Features.Reconciliations;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetFilterReconciliationSummaries;
using LinkPara.Epin.Application.Features.Reconciliations.Queries.GetReconciliationSummaryById;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Epin.Infrastructure.Services;

public class ReconciliationService : IReconciliationService
{
    private readonly IEpinHttpClient _epinHttpClient;
    private readonly IOrderService _orderService;
    private readonly IOrderHistoryService _orderHistoryService;
    private readonly IGenericRepository<ReconciliationSummary> _repository;
    private readonly IGenericRepository<ReconciliationDetail> _detailRepository;
    private readonly ILogger<ReconciliationService> _logger;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IProductService _productService;

    public ReconciliationService(IEpinHttpClient epinHttpClient,
        IOrderService orderService,
        IGenericRepository<ReconciliationSummary> repository,
        ILogger<ReconciliationService> logger,
        IOrderHistoryService orderHistoryService,
        IApplicationUserService applicationUserService,
        IGenericRepository<ReconciliationDetail> detailRepository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IProductService productService)
    {
        _epinHttpClient = epinHttpClient;
        _orderService = orderService;
        _repository = repository;
        _logger = logger;
        _orderHistoryService = orderHistoryService;
        _applicationUserService = applicationUserService;
        _detailRepository = detailRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _productService = productService;
    }

    public async Task ReconciliationAsync(DateTime reconciliationDate)
    {
        var previousReconcilation = await GetPreviousReconciliationsAsync(reconciliationDate);

        if (previousReconcilation is not null)
        {
            await DeletePreviousReconciliationsAsync(previousReconcilation);
        }

        var reconciliationSummary = new ReconciliationSummary
        {
            ReconciliationDate = reconciliationDate,
            ReconciliationStatus = ReconciliationStatus.Pending,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Organization = Organization.Perdigital
        };

        await _repository.AddAsync(reconciliationSummary);

        var orderHistories = await _orderHistoryService.GetOrderHistoryByDateFromServiceAsync(reconciliationDate);

        await _orderHistoryService.AddOrderHistoriesAsync(orderHistories, reconciliationDate);

        var orders = await _orderService.GetOrdersByDateAsync(reconciliationDate);

        if (orders is null)
        {
            reconciliationSummary.Message = "OrdersNotFound";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw new OrdersNotFoundException();
        }

        var reconciliationDetails = await CompareOrdersAsync(orderHistories, orders);

        await InsertReconciliationDetailsAsync(reconciliationDetails, reconciliationSummary.Id);

        if (reconciliationDetails.Any(x => x.ReconciliationDetailStatus != ReconciliationDetailStatus.Resolved && x.ExternalOrderId != 0))
        {
            reconciliationSummary.Message = "NotMatchBetweenExternalCountAndInternalCount";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw new OrdersNotMatchException();
        }

        orders = orders.Where(x => x.OrderStatus == OrderStatus.Completed).ToList();

        reconciliationSummary.OrderCount = orders.Count;
        reconciliationSummary.OrderTotal = orders.Sum(x => x.Price);

        await _repository.UpdateAsync(reconciliationSummary);

        if (orders.Count == 0 && orderHistories.Count == 0)
        {
            reconciliationSummary.Message = "Success";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Success;
            reconciliationSummary.ExternalTotal = 0;
            reconciliationSummary.ExternalCount = 0;
            await _repository.UpdateAsync(reconciliationSummary);
            return;
        }

        var reconciliationServiceResponse = await _epinHttpClient.GetReconciliationResultByDateAsync(reconciliationDate);

        if (reconciliationServiceResponse is null
            || reconciliationServiceResponse.reconciliation is null
            || reconciliationServiceResponse.reconciliation?.FirstOrDefault() is null)
        {
            reconciliationSummary.Message = "reconciliationServiceResponseIsNull";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw new NotEmptyReconciliationResultException();
        }

        reconciliationSummary.ExternalTotal = reconciliationServiceResponse.reconciliation.FirstOrDefault().total;
        reconciliationSummary.ExternalCount = reconciliationServiceResponse.reconciliation.FirstOrDefault().quantity;

        await _repository.UpdateAsync(reconciliationSummary);

        if (reconciliationServiceResponse.reconciliation.FirstOrDefault().quantity != orders.Count)
        {
            reconciliationSummary.Message = "NotMatchBetweenExternalCountAndInternalCount";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw new OrdersNotMatchException();
        }

        if (reconciliationServiceResponse.reconciliation.FirstOrDefault().total != orders.Sum(x => x.Price))
        {
            reconciliationSummary.Message = "NotMatchBetweenExternalTotalAndInternalOrderTotalPrice";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw new OrdersNotMatchException();
        }

        try
        {
            await _epinHttpClient.ReconciliationApproveByDateAsync(reconciliationDate);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ErrorReconciliationApproveService exception: \n{exception}");

            reconciliationSummary.Message = "ErrorReconciliationApproveService";
            reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Fail;
            await _repository.UpdateAsync(reconciliationSummary);
            throw;
        }

        reconciliationSummary.Message = "Success";
        reconciliationSummary.ReconciliationStatus = ReconciliationStatus.Success;
        await _repository.UpdateAsync(reconciliationSummary);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "ReconciliationByDate",
            SourceApplication = "Epin",
            Resource = "ReconciliationSummary",
            Details = new Dictionary<string, string>
            {
                  {"ReconciliationDate",reconciliationDate.ToString() },
                  {"ReconciliationStatus", ReconciliationStatus.Pending.ToString() },
            }
        });

    }

    private async Task DeletePreviousReconciliationsAsync(List<ReconciliationSummary> previousReconcilations)
    {
        foreach (var previousReconcilation in previousReconcilations)
        {
            await _repository.DeleteAsync(previousReconcilation);

            foreach (var reconciliationDetail in previousReconcilation.ReconciliationDetails)
            {
                await _detailRepository.DeleteAsync(reconciliationDetail);
            }
        }
    }

    private async Task<List<ReconciliationSummary>> GetPreviousReconciliationsAsync(DateTime reconciliationDate)
    {
        var result = await _repository.GetAll()
            .Include(x => x.ReconciliationDetails)
            .Where(x => x.ReconciliationDate.Date == reconciliationDate.Date && x.RecordStatus == RecordStatus.Active)
            .ToListAsync();
        return result;
    }

    private async Task InsertReconciliationDetailsAsync(List<ReconciliationDetail> reconciliationDetails, Guid reconciliationSummaryId)
    {
        foreach (var item in reconciliationDetails)
        {
            item.ReconciliationSummaryId = reconciliationSummaryId;
            await _detailRepository.AddAsync(item);
        }
    }

    private async Task<List<ReconciliationDetail>> CompareOrdersAsync(List<OrderHistory> orderHistories, List<Order> orders)
    {
        var list = new List<ReconciliationDetail>();

        try
        {

            var products = await _productService.GetProductsAsync();

            orderHistories.ForEach(item =>
            {
                var hasInternalOrder = orders.Any(s =>
                        s.ExternalId == item.ExternalId && s.Price == item.Total
                        && s.OrderStatus == OrderStatus.Completed);

                if (hasInternalOrder)
                {
                    var order = orders.FirstOrDefault(s =>
                        s.ExternalId == item.ExternalId && s.Price == item.Total
                        && s.OrderStatus == OrderStatus.Completed);

                    if (order is null)
                    {
                        return;
                    }

                    list.Add(new ReconciliationDetail
                    {
                        ExternalTotal = item.Total,
                        ExternalOrderId = item.ExternalId,
                        HasExternalOrders = true,
                        HasInternalOrders = hasInternalOrder,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        RecordStatus = RecordStatus.Active,
                        TransactionDate = item.TransactionDate,
                        ReconciliationDetailStatus = ReconciliationDetailStatus.Resolved,
                        OrderId = order.Id,
                        OrderHistoryId = item.Id,
                        ProductName = order.Equivalent
                    });
                }

                if (!hasInternalOrder)
                {
                    Product product = null;

                    if(products is not null)
                    {
                        product = products.FirstOrDefault(x => x.ExternalId == item.ExternalProductId);
                    }
                    
                    list.Add(new ReconciliationDetail
                    {
                        ExternalTotal = item.Total,
                        ExternalOrderId = item.ExternalId,
                        HasExternalOrders = true,
                        HasInternalOrders = hasInternalOrder,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        RecordStatus = RecordStatus.Active,
                        TransactionDate = item.TransactionDate,
                        ReconciliationDetailStatus = ReconciliationDetailStatus.HasOnlyExternal,
                        OrderHistoryId = item.Id,
                        ProductName = product is not null ? product.Equivalent : string.Empty,
                    });
                }

                var errorOrderStatus = new List<OrderStatus>
                    { OrderStatus.Error,OrderStatus.PendingProvision};

                var hasErrorStatus = orders.Any(s =>
                        s.ExternalId == item.ExternalId && s.Price == item.Total
                        && errorOrderStatus.Any(x => x == s.OrderStatus));

                if (hasErrorStatus)
                {
                    var order = orders.FirstOrDefault(s =>
                        s.ExternalId == item.ExternalId && s.Price == item.Total);

                    if (order is null)
                    {
                        return;
                    }

                    list.Add(new ReconciliationDetail
                    {
                        ExternalTotal = item.Total,
                        ExternalOrderId = item.ExternalId,
                        HasExternalOrders = true,
                        HasInternalOrders = hasInternalOrder,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        RecordStatus = RecordStatus.Active,
                        TransactionDate = item.TransactionDate,
                        ReconciliationDetailStatus = ReconciliationDetailStatus.Error,
                        InternalOrderErrorMessage = order.ErrorMessage,
                        OrderId = order.Id,
                        OrderHistoryId = item.Id,
                        ProductName = order.Equivalent
                    });
                }

            });

            orders.ForEach(item =>
            {
                var hasExternalOrder = orderHistories.Any(s =>
                            s.ExternalId == item.ExternalId && s.Total == item.Total);

                if (!hasExternalOrder)
                {
                    list.Add(new ReconciliationDetail
                    {
                        ExternalTotal = item.Total,
                        ExternalOrderId = item.ExternalId,
                        InternalOrderErrorMessage = item.ErrorMessage,
                        HasExternalOrders = false,
                        HasInternalOrders = true,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                        RecordStatus = RecordStatus.Active,
                        TransactionDate = item.TransactionDate,
                        ReconciliationDetailStatus = ReconciliationDetailStatus.HasOnlyInternal,
                        OrderId = item.Id,
                        ProductName = item.Equivalent
                    });
                }
            });
        }
        catch (Exception exception)
        {
            _logger.LogError($"Exception On CompareOrders : {exception}");
        }

        return list;
    }

    public async Task<PaginatedList<ReconciliationSummaryDto>> GetFilterReconciliationSummariesAsync(GetFilterReconciliationSummariesQuery request)
    {
        var reconciliationSummaries = _repository.GetAll()
            .Where(b => b.ReconciliationStatus != ReconciliationStatus.Pending && b.RecordStatus == RecordStatus.Active);
        
        if (request.ReconciliationDateStart is not null)
        {
            reconciliationSummaries = reconciliationSummaries.Where(b => b.ReconciliationDate >= request.ReconciliationDateStart);
        }

        if (request.ReconciliationDateEnd is not null)
        {
            reconciliationSummaries = reconciliationSummaries.Where(b => b.ReconciliationDate <= request.ReconciliationDateEnd);
        }

        if (request.Organization is not null)
        {
            reconciliationSummaries = reconciliationSummaries.Where(b => b.Organization == request.Organization);
        }

        if (request.ReconciliationStatus is not null)
        {
            reconciliationSummaries = reconciliationSummaries.Where(b => b.ReconciliationStatus == request.ReconciliationStatus);
        }
        return await reconciliationSummaries
           .PaginatedListWithMappingAsync<ReconciliationSummary,ReconciliationSummaryDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<ReconciliationSummaryDto> GetReconciliationSummaryByIdAsync(GetReconciliationSummaryByIdQuery request)
    {
        var reconciliationSummary = await _repository.GetByIdAsync(request.ReconciliationSummaryId);

        var unreconciledDetails = await _detailRepository.GetAll()
            .Where(x => x.ReconciliationSummaryId == reconciliationSummary.Id
                    && x.ReconciliationDetailStatus != ReconciliationDetailStatus.Resolved
                    && x.ExternalOrderId != 0)
            .Include(x => x.Order).ThenInclude(x => x.Publisher)
            .Include(x => x.Order).ThenInclude(x => x.Brand)
            .Include(x => x.OrderHistory)
            .ToListAsync();

        var result = _mapper.Map<ReconciliationSummaryDto>(reconciliationSummary);

        var unreconciledDetailsDto = _mapper.Map<List<ReconciliationDetailDto>>(unreconciledDetails);

        result.UnreconciledOrders = unreconciledDetailsDto;

        return result;
    }
}
