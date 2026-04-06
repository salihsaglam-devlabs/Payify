using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.LinkPayments;
using LinkPara.PF.Application.Features.LinkPayments.Commands.SaveLinkPayment;
using LinkPara.PF.Application.Features.LinkPayments.Queries.GetPaymentDetail;
using LinkPara.PF.Application.Features.MerchantTransactions.Command;
using LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services.PaymentServices;

public class LinkPaymentService : ILinkPaymentService
{
    private readonly PfDbContext _context;
    private readonly IGenericRepository<Link> _linkRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<SubMerchant> _subMerchantRepository;
    private readonly IBasePaymentService _basePaymentService;
    private readonly IPaymentService _paymentService;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICurrencyService _currencyService;
    private readonly ILogger<LinkPaymentService> _logger;
    public LinkPaymentService(PfDbContext context,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<Link> linkRepository,
        IPaymentService paymentService,
        IBasePaymentService basePaymentService,
        IResponseCodeService errorCodeService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IContextProvider contextProvider,
        IApplicationUserService applicationUserService,
        ICurrencyService currencyService,
        ILogger<LinkPaymentService> logger,
        IGenericRepository<SubMerchant> subMerchantRepository)
    {
        _context = context;
        _merchantRepository = merchantRepository;
        _linkRepository = linkRepository;
        _paymentService = paymentService;
        _basePaymentService = basePaymentService;
        _errorCodeService = errorCodeService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _contextProvider = contextProvider;
        _applicationUserService = applicationUserService;
        _currencyService = currencyService;
        _logger = logger;
        _subMerchantRepository = subMerchantRepository;
    }
    public async Task<LinkPaymentResponse> SaveLinkPaymentAsync(SaveLinkPaymentCommand command)
    {
        var response = new LinkPaymentResponse();

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : _applicationUserService.ApplicationUserId;

        var link = await _linkRepository.GetAll()
            .Include(s => s.LinkInstallments)
            .Where(s => s.LinkCode == command.LinkCode
            && s.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (link is null)
        {
            var apiResponse = await _errorCodeService.GetApiResponseCode(ApiErrorCode.LinkNotFound, command.LanguageCode);

            return new LinkPaymentResponse
            {
                IsSucceed = false,
                ErrorCode = ApiErrorCode.LinkNotFound,
                ErrorMessage = apiResponse.DisplayMessage,
                LinkUrlPath = command.LinkCode
            };
        }

        var currency = await _currencyService.GetByNumberAsync(link.Currency);

        var merchant = await _merchantRepository.GetAll()
            .Where(s => s.Number == link.MerchantNumber)
            .FirstOrDefaultAsync();

        var subMerchant = new SubMerchant();
        if (command.SubMerchantId.HasValue && command.SubMerchantId != Guid.Empty)
        {
            subMerchant = await _subMerchantRepository.GetByIdAsync(command.SubMerchantId.Value);
        }

        var parentMerchantFinancialTransaction = true;
        if (merchant?.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }
        
        var validationResponse = await LinkPreValidateAsync(link, merchant, parentMerchantFinancialTransaction, subMerchant, command);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"Link PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            var apiResponse = await _errorCodeService.GetApiResponseCode(validationResponse.Code, command.LanguageCode);

            await InsertValidationLogAsync(command, apiResponse, currency.Name, link.OrderId);

            return new LinkPaymentResponse
            {
                IsSucceed = false,
                ErrorCode = apiResponse.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                LinkUrlPath = command.LinkCode
            };
        }

        var orderNumber = await _basePaymentService.GenerateOrderNumberAsync(merchant.Id, link.OrderId);

        var provisionRequest = new ProvisionCommand()
        {
            CardToken = command.CardToken,
            CardHolderName = command.CardHolderName,
            MerchantId = merchant.Id,
            SubMerchantId = command.SubMerchantId,
            Amount = command.Amount,
            Currency = currency.Code,
            LanguageCode = command.LanguageCode,
            IntegrationMode = command.IntegrationMode,
            ConversationId = orderNumber,
            InstallmentCount = command.InstallmentCount,
            PaymentType = VposPaymentType.Auth,
            ClientIpAddress = command.ClientIpAddress,
            ThreeDSessionId = command.ThreeDSessionId ?? null,
        };

        var paymentResponse = await _paymentService.ProvisionAsync(provisionRequest);

        return paymentResponse.IsSucceed ? await MarkAsCompletedAsync(command, link, paymentResponse, orderNumber, parseUserId.ToString())
            : await MarkAsFailedAsync(command, link, paymentResponse, orderNumber, parseUserId.ToString());
    }
    private async Task<LinkPaymentResponse> MarkAsCompletedAsync(SaveLinkPaymentCommand request, Link link,
    ProvisionResponse provisionResponse, string orderNumber, string parseUserId)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                if (link.LinkType == LinkType.SingleUse)
                {
                    link.UpdateDate = DateTime.Now;
                    link.RecordStatus = RecordStatus.Passive;
                    link.OrderId = orderNumber;
                    link.LinkStatus = ChannelStatus.Passive;
                    link.LinkPaymentStatus = ChannelPaymentStatus.Success;
                    link.CurrentUsageCount++;
                    link.LastModifiedBy = parseUserId.ToString();
                }
                else
                {
                    link.CurrentUsageCount++;
                    link.UpdateDate = DateTime.Now;

                    if (link.MaxUsageCount == link.CurrentUsageCount)
                    {
                        link.UpdateDate = DateTime.Now;
                        link.RecordStatus = RecordStatus.Passive;
                        link.LinkStatus = ChannelStatus.Passive;
                        link.LinkPaymentStatus = ChannelPaymentStatus.Success;
                        link.LastModifiedBy = parseUserId.ToString();
                    }
                }
                _context.Link.Update(link);

                var linkCustomer = new LinkCustomer()
                {
                    Name = request.CustomerName,
                    Email = request.CustomerEmail,
                    PhoneNumber = request.CustomerPhoneNumber,
                    Address = request.CustomerAddress,
                    Note = request.CustomerNote,
                    CreatedBy = parseUserId.ToString(),
                    RecordStatus = RecordStatus.Active,
                };

                await _context.LinkCustomer.AddAsync(linkCustomer);

                var linkTransaction = new LinkTransaction()
                {
                    MerchantTransactionId = provisionResponse.TransactionId,
                    LinkPaymentStatus = ChannelPaymentStatus.Success,
                    TransactionType = TransactionType.Auth,
                    TransactionDate = DateTime.Now,
                    LinkCode = link.LinkCode,
                    OrderId = orderNumber,
                    LinkType = link.LinkType,
                    CommissionFromCustomer = link.CommissionFromCustomer,
                    CommissionAmount = link.CommissionFromCustomer ? request.Amount - link.Amount : 0,
                    Is3dRequired = link.Is3dRequired,
                    Amount = request.Amount,
                    InstallmentCount = request.InstallmentCount,
                    Currency = link.Currency,
                    ThreeDSessionId = request.ThreeDSessionId,
                    CreatedBy = parseUserId.ToString(),
                    RecordStatus = RecordStatus.Active,
                    CustomerId = linkCustomer.Id,
                };

                linkCustomer.LinkTransactionId = linkTransaction.Id;

                await _context.LinkTransaction.AddAsync(linkTransaction);

                await _context.SaveChangesAsync();
                scope.Complete();

            });

            return new LinkPaymentResponse()
            {
                LinkUrlPath = link.LinkCode,
                OrderId = orderNumber,
                ErrorCode = provisionResponse.ErrorCode,
                ErrorMessage = provisionResponse.ErrorMessage,
                IsSucceed = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateLinkTransaction -> ProvisionAsync IsSucceed:True : {ex}");
            throw;
        }
    }
    private async Task<LinkPaymentResponse> MarkAsFailedAsync(SaveLinkPaymentCommand request, Link link,
    ProvisionResponse provisionResponse, string orderNumber, string parseUserId)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                var linkCustomer = new LinkCustomer()
                {
                    Name = request.CustomerName,
                    Email = request.CustomerEmail,
                    PhoneNumber = request.CustomerPhoneNumber,
                    Address = request.CustomerAddress,
                    Note = request.CustomerNote,
                    CreatedBy = parseUserId,
                    RecordStatus = RecordStatus.Active,
                };

                await _context.LinkCustomer.AddAsync(linkCustomer);

                var linkTransaction = new LinkTransaction()
                {
                    MerchantTransactionId = provisionResponse.TransactionId,
                    LinkPaymentStatus = ChannelPaymentStatus.Failed,
                    TransactionType = TransactionType.Auth,
                    TransactionDate = DateTime.Now,
                    LinkCode = link.LinkCode,
                    OrderId = orderNumber,
                    LinkType = link.LinkType,
                    CommissionFromCustomer = link.CommissionFromCustomer,
                    CommissionAmount = link.CommissionFromCustomer ? request.Amount - link.Amount : 0,
                    Is3dRequired = link.Is3dRequired,
                    Amount = request.Amount,
                    InstallmentCount = request.InstallmentCount,
                    Currency = link.Currency,
                    ThreeDSessionId = request.ThreeDSessionId,
                    CreatedBy = parseUserId.ToString(),
                    RecordStatus = RecordStatus.Active,
                    CustomerId = linkCustomer.Id,
                };

                linkCustomer.LinkTransactionId = linkTransaction.Id;

                await _context.LinkTransaction.AddAsync(linkTransaction);

                await _context.SaveChangesAsync();
                scope.Complete();

            });

            return new LinkPaymentResponse()
            {
                LinkUrlPath = link.LinkCode,
                OrderId = orderNumber,
                ErrorCode = provisionResponse.ErrorCode,
                ErrorMessage = provisionResponse.ErrorMessage,
                IsSucceed = false
            };

        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateLinkTransaction -> ProvisionAsync IsSucceed:False : {ex}");
            throw;
        }
    }
    private async Task<ValidationResponse> LinkPreValidateAsync(Link link, Merchant merchant, bool parentMerchantFinancialTransaction, SubMerchant subMerchant, SaveLinkPaymentCommand command)
    {
        if (merchant is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, command.LanguageCode);
        }
        
        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.LinkPaymentPage.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, command.LanguageCode);
        }
        
        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, command.LanguageCode);
        }

        if (!string.IsNullOrEmpty(subMerchant.Name))
        {

            if (!subMerchant.IsLinkPaymentPageAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, command.LanguageCode);
            }

            if (subMerchant.RecordStatus != RecordStatus.Active)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, command.LanguageCode);
            }
        }

     

        if (link.ExpiryDate < DateTime.Now)
        {
            link.UpdateDate = DateTime.Now;
            link.RecordStatus = RecordStatus.Passive;
            link.LinkStatus = ChannelStatus.Expired;

            await _linkRepository.UpdateAsync(link);

            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.LinkExpired, command.LanguageCode);
        }
        
        if (link.LinkInstallments.FirstOrDefault(s => s.Installment == command.InstallmentCount) is null && command.InstallmentCount > 1)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, command.LanguageCode);
        }

        if (link.IsEmailRequired && command.CustomerEmail is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerEmailRequired, command.LanguageCode);
        }

        if (link.IsAddressRequired && command.CustomerAddress is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerAddressRequired, command.LanguageCode);
        }

        if (link.IsNameRequired && command.CustomerName is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerNameRequired, command.LanguageCode);
        }

        if (link.IsNoteRequired && command.CustomerNote is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerNoteRequired, command.LanguageCode);
        }

        if (link.IsPhoneNumberRequired && command.CustomerPhoneNumber is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CustomerPhoneNumberRequired, command.LanguageCode);
        }

        if (link.LinkType == LinkType.MultipleUse && link.CurrentUsageCount == link.MaxUsageCount)
        {

            link.LinkPaymentStatus = ChannelPaymentStatus.Success;
            link.UpdateDate = DateTime.Now;
            link.RecordStatus = RecordStatus.Passive;
            link.LinkStatus = ChannelStatus.Passive;

            await _linkRepository.UpdateAsync(link);

            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.LinkMaxLimitCountExceeded, command.LanguageCode);
        }

        if (link.Is3dRequired && command.ThreeDSessionId is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.LinkThreeDSRequired, command.LanguageCode);
        }

        if (link.CommissionFromCustomer && link.Amount >= command.Amount)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CommissionFromCustomerException, command.LanguageCode);
        }

        return new ValidationResponse { IsValid = true };
    }
    private async Task InsertValidationLogAsync(SaveLinkPaymentCommand request, MerchantResponseCodeDto validationResponse, string currency, string orderId)
    {
        await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = request.Amount,
            Currency = currency,
            MerchantId = request.MerchantId,
            CardToken = request.CardToken,
            ConversationId = orderId,
            InstallmentCount = request.InstallmentCount,
            LanguageCode = request.LanguageCode,
            TransactionType = TransactionType.Auth,
            ClientIpAddress = request.ClientIpAddress,
            ThreeDSessionId = request.ThreeDSessionId,
            ErrorCode = validationResponse.ResponseCode == null ? "Error Code Not Found!" : validationResponse.ResponseCode,
            ErrorMessage = validationResponse.DisplayMessage == null ? "Error Message Not Found!" : validationResponse.DisplayMessage
        });
    }
    public async Task<PaginatedList<LinkPaymentDetailResponse>> GetLinkPaymentDetails(GetPaymentDetailQuery request)
    {
        var linkPaymentDetail = (from linkTransaction in _context.LinkTransaction
                                 join linkCustomer in _context.LinkCustomer on linkTransaction.Id equals linkCustomer.LinkTransactionId
                                 where linkTransaction.LinkCode == request.LinkCode
                                 select new LinkPaymentDetailResponse()
                                 {
                                     OrderId = linkTransaction.OrderId,
                                     TransactionType = linkTransaction.TransactionType,
                                     LinkPaymentStatus = linkTransaction.LinkPaymentStatus,
                                     PaymentDate = linkTransaction.CreateDate,
                                     Amount = linkTransaction.Amount,
                                     InstallmentCount = linkTransaction.InstallmentCount,
                                     CustomerName = linkCustomer.Name,
                                     CustomerEmail = linkCustomer.Email,
                                     CustomerAddress = linkCustomer.Address,
                                     CustomerNote = linkCustomer.Note,
                                     CustomerPhoneNumber = linkCustomer.PhoneNumber
                                 });

        return await linkPaymentDetail.PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }
}