using System.Globalization;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.ContextProvider;
using LinkPara.Epin.Application.Commons.Exceptions;
using LinkPara.Epin.Application.Commons.Interfaces;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Requests.Order;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.OrderHistory;
using LinkPara.Epin.Application.Commons.Models.Perdigital.Responses.Product;
using LinkPara.Epin.Application.Features.Orders;
using LinkPara.Epin.Application.Features.Orders.Commands.CancelOrder;
using LinkPara.Epin.Application.Features.Orders.Commands.CreateOrder;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrdersFilter;
using LinkPara.Epin.Application.Features.Orders.Queries.GetOrderSummary;
using LinkPara.Epin.Application.Features.Orders.Queries.GetUserOrdersFilter;
using LinkPara.Epin.Domain.Entities;
using LinkPara.Epin.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.HttpProviders.Emoney.Models;
using LinkPara.HttpProviders.Identity;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Accounting.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.Epin.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly IEpinHttpClient _httpClient;
    private readonly IEmoneyService _eMoneyService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Order> _repository;
    private readonly ILogger<OrderService> _logger;
    private readonly IProductService _productService;
    private readonly IAccountingService _accountingService;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly IBrandService _brandService;
    private readonly IGenericRepository<ReconciliationDetail> _reconciliationDetailRepository;

    public OrderService(IEpinHttpClient httpClient,
        IEmoneyService eMoneyService,
        IContextProvider contextProvider,
        IGenericRepository<Order> repository,
        ILogger<OrderService> logger,
        IProductService productService,
        IAccountingService accountingService,
        IMapper mapper,
        IUserService userService,
        IBrandService brandService,
        IGenericRepository<ReconciliationDetail> reconciliationDetailRepository)
    {
        _httpClient = httpClient;
        _eMoneyService = eMoneyService;
        _contextProvider = contextProvider;
        _repository = repository;
        _logger = logger;
        _productService = productService;
        _accountingService = accountingService;
        _mapper = mapper;
        _userService = userService;
        _brandService = brandService;
        _reconciliationDetailRepository = reconciliationDetailRepository;
    }

    private async Task<bool> CheckStockAysnc(int productId)
    {
        try
        {
            return await _httpClient.CheckStockAsync(productId);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Stock Exception \n{exception}");
            return false;
        }
    }

    private Task<string> GenerateOrderReferenceAsync()
    {
        return Task.FromResult(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString().PadLeft(15, '0'));
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderCommand request)
    {
        string orderReference = await GenerateOrderReferenceAsync();

        var brand = await _brandService.GetBrandByIdAsync(request.BrandId);

        var order = new Order
        {
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            WalletNumber = request.WalletNumber,
            ReferenceId = orderReference,
            OrderStatus = OrderStatus.Pending,
            PublisherId = request.PublisherId,
            BrandId = brand.Id,
            ExternalProductId = request.ProductId
        };

        await _repository.AddAsync(order);

        await UpdateUserInformationAsync(order);

        var product = await UpdateProductInformationAsync(request, order);

        await ValidateAmountAsync(request, order, product);

        if (!await CheckStockAysnc(product.Id))
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "StockError";
            await _repository.UpdateAsync(order);

            throw new StockNotFoundException(request.ProductId);
        }

        await ProvisionPreviewAsync(request, order);

        await UpdateOrderAsync(request, orderReference, order);

        var emoneyTransactionId = await CreateProvisionAsync(request, order, product);

        await _accountingService.PostAccountingPaymentAsync(order, 
            string.IsNullOrWhiteSpace(request.CurrencyCode) 
                ? "TRY" 
                : request.CurrencyCode, OperationType.BuyPin);

        return new CreateOrderResponse
        {
            Amount = product.Unit_Price,
            Pin = order.Pin,
            WalletNumber = order.WalletNumber,
            Image = brand.Image,
            EmoneyTransactionId = emoneyTransactionId
        };
    }

    private async Task<Guid> CreateProvisionAsync(CreateOrderCommand request, Order order, ProductServiceDto product)
    {
        var provisionRequest = new ProvisionRequest
        {
            Amount = product.Unit_Price,
            ClientIpAddress = _contextProvider.CurrentContext.ClientIpAddress,
            ConversationId = Guid.NewGuid().ToString(),
            ProvisionSource = ProvisionSource.Epin,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId),
            WalletNumber = request.WalletNumber,
            Description = $"{request.ProductId}-{request.WalletNumber}",
            CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "TRY" : request.CurrencyCode,
            Tag = order.Equivalent
        };

        ProvisionResponse provision;

        try
        {
            provision = await _eMoneyService.CreateProvisionAsync(provisionRequest);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ProvisionError : {exception}");
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "ProvisionError";
            await _repository.UpdateAsync(order);

            throw;
        }

        if (!provision.IsSucceed)
        {
            _logger.LogError($"ProvisionUnsuccess Message: {provision.ErrorMessage}");
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = $"ProvisionUnsuccess:{provision.ErrorMessage}";
            await _repository.UpdateAsync(order);

            throw new ProvisionErrorException(provision.ErrorMessage);
        }

        order.ProvisionReferenceId = provision.ConversationId;
        order.OrderStatus = OrderStatus.Completed;
        await _repository.UpdateAsync(order);
        return provision.TransactionId;
    }

    private async Task UpdateOrderAsync(CreateOrderCommand request, string orderReference, Order order)
    {
        var orderServiceRequest = new OrderServiceRequest
        {
            product = request.ProductId,
            quantity = 1,
            reference = orderReference
        };
        try
        {
            var orderResponse = await _httpClient.CreateOrderAsync(orderServiceRequest);

            if (orderResponse is null ||
               orderResponse.Order is null ||
               orderResponse.Order.Products is null ||
               orderResponse.Order.Products.Product is null ||
               orderResponse.Order.Products.Product?.Count == 0 ||
               orderResponse.Order.Products.Product?.FirstOrDefault().Pins?.Count == 0)
            {
                order.OrderStatus = OrderStatus.Error;
                await _repository.UpdateAsync(order);

                throw new OrderServiceException();
            }

            order.ExternalId = orderResponse.Order.Id;
            order.Pin = orderResponse.Order.Products.Product.FirstOrDefault().Pins.FirstOrDefault().Pin;
            order.OrderStatus = OrderStatus.PendingProvision;
            order.Total = orderResponse.Order.Total;
            order.TransactionDate = DateTime.Now.Date;

            await _repository.UpdateAsync(order);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Exception On Create Order \n{exception}");

            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = exception.Message;
            await _repository.UpdateAsync(order);

            throw;
        }
        
    }

    private async Task ProvisionPreviewAsync(CreateOrderCommand request, Order order)
    {
        var provisionPreviewRequest = new ProvisionPreviewRequest
        {
            Amount = request.Amount,
            CurrencyCode = string.IsNullOrWhiteSpace(request.CurrencyCode) ? "TRY" : request.CurrencyCode,
            WalletNumber = request.WalletNumber,
            UserId = Guid.Parse(_contextProvider.CurrentContext.UserId)
        };

        ProvisionPreviewResponse provisionPreview;

        try
        {
            provisionPreview = await _eMoneyService.PreviewProvisionAsync(provisionPreviewRequest);
        }
        catch (Exception exception)
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "ProvisionPreviewError";
            await _repository.UpdateAsync(order);
            _logger.LogError($"ProvisionPreviewException : {exception}");
            throw;
        }

        if (!provisionPreview.IsSuccess)
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "ProvisionPreviewIsNotSuccessfull";
            await _repository.UpdateAsync(order);

            _logger.LogError($"ProvisionPreviewUnSuccess Message: {provisionPreview.ErrorMessage}");
            throw new ProvisionErrorException(provisionPreview.ErrorMessage);
        }
    }

    private async Task ValidateAmountAsync(CreateOrderCommand request, Order order, ProductServiceDto product)
    {
        if (product.Unit_Price != request.Amount)
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "ProductPriceAndRequestAmountNotMatch";
            await _repository.UpdateAsync(order);
            throw new PriceDidNotMatchException();
        }
    }

    private async Task<ProductServiceDto> UpdateProductInformationAsync(CreateOrderCommand request, Order order)
    {
        var product = await _productService.GetProductAsync(request.PublisherId, request.BrandId, request.ProductId);

        if (product is null)
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "ProductNotFound";
            await _repository.UpdateAsync(order);

            throw new ProductNotFoundException("ProductIsNotFound");
        }

        order.Price = product.Price;
        order.UnitPrice = product.Unit_Price;
        order.Discount = product.Discount;
        order.Equivalent = product.Equivalent;
        
        return product;
    }

    private async Task UpdateUserInformationAsync(Order order)
    {
        var user = await _userService.GetUserAsync(Guid.Parse(_contextProvider.CurrentContext.UserId));

        if (user is null)
        {
            order.OrderStatus = OrderStatus.Error;
            order.ErrorMessage = "UserNotFound";
            await _repository.UpdateAsync(order);

            throw new NotFoundException(nameof(user));
        }

        order.Email = user.Email;
        order.PhoneNumber = user.PhoneNumber;
        order.UserFullName = $"{user.FirstName} {user.LastName}";
    }

    public async Task<PaginatedList<UserOrderDto>> GetUserOrdersFilterAsync(GetUserOrdersFilterQuery request)
    {
        var userOrders = _repository.GetAll().Where(b => b.RecordStatus == RecordStatus.Active
                                                         && b.UserId == request.UserId
                                                         && b.OrderStatus == OrderStatus.Completed)
                                             .Include(x => x.Brand).AsQueryable();

        if (!string.IsNullOrEmpty(request.Q))
        {
            userOrders = userOrders.Where(b => b.Equivalent.Contains(request.Q));
        }

        return await userOrders
           .PaginatedListWithMappingAsync<Order,UserOrderDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<PaginatedList<OrderDto>> GetOrdersFilterAsync(GetOrdersFilterQuery request)
    {
        var orders = _repository.GetAll().Where(b => b.RecordStatus == RecordStatus.Active)
                                        .Include(b => b.Brand)
                                        .Include(b => b.Publisher).AsQueryable();


        if (!string.IsNullOrEmpty(request.Email))
        {
            orders = orders.Where(b => b.Email.Contains(request.Email));
        }

        if (request.CreateDateStart is not null)
        {
            orders = orders.Where(b => b.CreateDate >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            orders = orders.Where(b => b.CreateDate <= request.CreateDateEnd);
        }

        if (request.OrderStatus is not null)
        {
            orders = orders.Where(b => b.OrderStatus == request.OrderStatus);
        }

        if (request.PublisherId is not null)
        {
            orders = orders.Where(b => b.PublisherId == request.PublisherId);
        }

        if (request.BrandId is not null)
        {
            orders = orders.Where(b => b.BrandId == request.BrandId);
        }

        if (request.ProductId is not null)
        {
            orders = orders.Where(b => b.ExternalProductId == request.ProductId);
        }

        return await orders
           .PaginatedListWithMappingAsync<Order,OrderDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<OrderSummaryDto> GetOrderSummaryAsync(GetOrderSummaryQuery request)
    {
        var order = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == request.OrderId);

        if (order is null)
        {
            throw new NotFoundException(nameof(order));
        }

        return _mapper.Map<OrderSummaryDto>(order);
    }

    public async Task<List<Order>> GetOrdersByDateAsync(DateTime transactionDate)
    {
        var orders = await _repository.GetAll().Where(x => x.TransactionDate == transactionDate.Date).ToListAsync();
        return orders;
    }

    public async Task UpdateReconciliationOrdersAsync(List<Order> orders)
    {
        foreach (var item in orders)
        {
            await _repository.UpdateAsync(item);
        }
    }

    public async Task<Unit> CancelOrderAsync(CancelOrderCommand request)
    {
        var reconciliationDetail = await _reconciliationDetailRepository
            .GetAll()
            .Include(x => x.Order)
            .SingleOrDefaultAsync(x => x.OrderId == request.OrderId);

        if (reconciliationDetail is null)
        {
            throw new NotFoundException(nameof(reconciliationDetail));
        }

        if (reconciliationDetail.Order is null)
        {
            throw new NotFoundException(nameof(reconciliationDetail.Order));
        }

        if (reconciliationDetail.ReconciliationDetailStatus != ReconciliationDetailStatus.HasOnlyInternal
            || reconciliationDetail.Order.OrderStatus != OrderStatus.Completed
            || reconciliationDetail.Order.ExternalId == 0)
        {
            throw new InvalidStatusForCancelException();
        }

        var order = reconciliationDetail.Order;
        
        var cancelProvisionResponse = await _eMoneyService.CancelProvisionAsync(order.ProvisionReferenceId);

        if (!cancelProvisionResponse.IsSucceed)
        {
            _logger.LogError($"ErrorOnCancelProvision ProvisionReferenceId:{order.ProvisionReferenceId}\n errorCode : {cancelProvisionResponse.ErrorCode}\n message: {cancelProvisionResponse.ErrorMessage} ");
            throw new ProvisionErrorException("ErrorCancelProvision");
        }

        await _accountingService.PostAccountingPaymentAsync(order, "TRY", OperationType.CancelPin);

        order.OrderStatus = OrderStatus.Canceled;
        await _repository.UpdateAsync(order);

        return Unit.Value;    
    }
}
