using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.HostedPayment;
using LinkPara.PF.Application.Commons.Models.Payments.Response;
using LinkPara.PF.Application.Features.HostedPayments;
using LinkPara.PF.Application.Features.HostedPayments.Command.InitHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Command.SaveHostedPayment;
using LinkPara.PF.Application.Features.HostedPayments.Queries.GetHppTransactions;
using LinkPara.PF.Application.Features.Payments.Commands.Provision;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.Security;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Transactions;

namespace LinkPara.PF.Infrastructure.Services.PaymentServices;

public class HostedPaymentService : IHostedPaymentService
{
    private const string GenericErrorCode = "99";
    
    private readonly PfDbContext _context;
    private readonly IGenericRepository<HostedPayment> _hostedPaymentRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<HostedPaymentTransaction> _hppTransactionRepository;
    private readonly IGenericRepository<ThreeDVerification> _threeDVerificationRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly IVaultClient _vaultClient;
    private readonly ILogger<HostedPaymentService> _logger;
    private readonly IResponseCodeService _errorCodeService;
    private readonly IApplicationUserService _applicationUserService;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericRepository<MerchantApiValidationLog> _apiRequestValidationLogRepository;
    private readonly IBasePaymentService _basePaymentService;
    private readonly IPaymentService _paymentService;
    private readonly IBus _bus;
    private readonly IMapper _mapper;
    private readonly IParameterService _parameterService;
    private readonly ICardBinService _binService;
    private readonly ICardTokenService _cardTokenService;
    private readonly ISecureRandomGenerator _randomGenerator;

    public HostedPaymentService(PfDbContext context,
        IGenericRepository<HostedPayment> hostedPaymentRepository,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<HostedPaymentTransaction> hppTransactionRepository,
        IGenericRepository<ThreeDVerification> threeDVerificationRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IAuditLogService auditLogService,
        IVaultClient vaultClient,
        IResponseCodeService errorCodeService,
        IApplicationUserService applicationUserService,
        ILogger<HostedPaymentService> logger,
        ICurrencyService currencyService,
        IGenericRepository<MerchantApiValidationLog> apiRequestValidationLogRepository,
        IBasePaymentService basePaymentService,
        IPaymentService paymentService,
        IParameterService parameterService,
        IBus bus,
        IMapper mapper,
        ICardBinService binService,
        ICardTokenService cardTokenService,
        ISecureRandomGenerator randomGenerator)
    {
        _context = context;
        _hostedPaymentRepository = hostedPaymentRepository;
        _merchantRepository = merchantRepository;
        _hppTransactionRepository = hppTransactionRepository;
        _threeDVerificationRepository = threeDVerificationRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _acquireBankRepository = acquireBankRepository;
        _auditLogService = auditLogService;
        _vaultClient = vaultClient;
        _errorCodeService = errorCodeService;
        _applicationUserService = applicationUserService;
        _logger = logger;
        _currencyService = currencyService;
        _apiRequestValidationLogRepository = apiRequestValidationLogRepository;
        _basePaymentService = basePaymentService;
        _paymentService = paymentService;
        _parameterService = parameterService;
        _bus = bus;
        _mapper = mapper;
        _binService = binService;
        _cardTokenService = cardTokenService;
        _randomGenerator = randomGenerator;
    }

    public async Task<InitHostedPaymentResponse> InitHostedPaymentAsync(InitHostedPaymentCommand request)
    {
        try
        {
            var merchant = await _context.Merchant.FindAsync(request.MerchantId);
            
            var parentMerchantFinancialTransaction = true;
            if (merchant?.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
            {
                var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
                if (parentMerchant is not null)
                {
                    parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
                }
            }
            
            var validationResponse = await ValidateInitAsync(merchant, parentMerchantFinancialTransaction, request);
            
            if (!validationResponse.IsValid)
            {
                _logger.LogError($"Init hosted payment failed with code : {validationResponse.Code}, " +
                                 $"Message: {validationResponse.Message}");

                return new InitHostedPaymentResponse
                {
                    IsSucceed = false,
                    ErrorCode = validationResponse.Code,
                    ErrorMessage = validationResponse.Message,
                    ConversationId = request.ConversationId
                };
            }

            var hppExpireInMinutes = await _parameterService.GetParameterAsync("PFPaymentParameters", "HppExpireInMinutes");

            var hostedPayment = PopulateHostedPayment(merchant, request, Convert.ToInt32(hppExpireInMinutes.ParameterValue));
            
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                if (request.Installments is not null)
                {
                    foreach (var installment in request.Installments)
                    {
                        await _context.HostedPaymentInstallment.AddAsync(new HostedPaymentInstallment
                        {
                            HostedPaymentId = hostedPayment.Id,
                            Installment = installment.Installment,
                            CardNetwork = installment.CardNetwork,
                            Amount = installment.Amount,
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                            RecordStatus = RecordStatus.Active
                        });
                    }
                }
                hostedPayment.TrackingId = GenerateTrackingId();
                await _context.HostedPayment.AddAsync(hostedPayment);
                await _context.SaveChangesAsync();
                scope.Complete();
            });
            
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "InitHostedPayment",
                    SourceApplication = "PF",
                    Resource = "HostedPayment",
                    Details = new Dictionary<string, string>
                    {
                        {"Id", hostedPayment.Id.ToString() }
                    }
                });

            var baseUrl = _vaultClient.GetSecretValue<string>("SharedSecrets", "ServiceUrls", "HostedPaymentBaseUrl");

            return new InitHostedPaymentResponse
            {
                IsSucceed = true,
                Status = ChannelStatus.Active,
                PaymentUrl = baseUrl + "/" + hostedPayment.TrackingId + "?pageView=" + hostedPayment.PageViewType,
                TrackingId = hostedPayment.TrackingId,
                ExpiryDate = hostedPayment.ExpiryDate,
                PageViewType = hostedPayment.PageViewType,
                LanguageCode = hostedPayment.LanguageCode,
                ConversationId = request.ConversationId
            };
        }
        catch (Exception exception)
        {
            _logger.LogError($"Init hosted payment failed. Error: {exception}");
            
            var apiResponse = await _errorCodeService.GetApiResponseCode(
                exception is ApiException apiException
                ? apiException.Code : GenericErrorCode, 
                request.LanguageCode);

            return new InitHostedPaymentResponse
            {
                IsSucceed = false,
                ErrorCode = apiResponse.ResponseCode,
                ErrorMessage = apiResponse.DisplayMessage,
                ConversationId = request.ConversationId
            };
        }
    }
    
    private async Task<ValidationResponse> ValidateInitAsync(Merchant merchant, bool parentMerchantFinancialTransaction, InitHostedPaymentCommand request)
    {
        if (merchant is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }
        
        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }
        var duplicateOrderId = await _hostedPaymentRepository.GetAll()
            .AnyAsync(x => x.OrderId == request.OrderId && x.MerchantId == request.MerchantId);

        if (duplicateOrderId)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.DuplicateOrderId, request.LanguageCode);
        }

        if (merchant.IsHostedPayment3dRequired && !request.Is3dRequired)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.NonSecureNotAllowed, request.LanguageCode);
        }

        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.Hpp.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, request.LanguageCode);
        }

        if (!request.EnableInstallments && request.CommissionFromCustomer)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CannotTakeCommissionOnAdvance, request.LanguageCode);
        }
        
        if (!request.EnableInstallments && !request.CommissionFromCustomer && request.Installments.Count > 0)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InstallmentsMustBeEmpty, request.LanguageCode);
        }

        if (request.EnableInstallments && request.CommissionFromCustomer &&
            request.Installments.Any(s => s.Amount != null))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.CannotSetAmountOnCommissionFromCustomer, request.LanguageCode);
        }

        if (request.Installments.FirstOrDefault(s => s.Amount < request.Amount) is not null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, request.LanguageCode);
        }

        return new ValidationResponse { IsValid = true };
    }
    
    private string GenerateTrackingId()
    {
        var any = false;
        var url = string.Empty;
        do
        {
            url = GetUniqueKey();
            any = _hostedPaymentRepository.GetAll().Any(s => s.TrackingId == url);
        }
        while (any);
        return url;
    }

    private HostedPayment PopulateHostedPayment(Merchant merchant, InitHostedPaymentCommand request, int expireInMinutes)
    {
        return new HostedPayment
        {
            HppStatus = ChannelStatus.Active,
            HppPaymentStatus = ChannelPaymentStatus.Pending,
            WebhookStatus = WebhookStatus.Pending,
            OrderId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            CommissionFromCustomer = request.CommissionFromCustomer,
            Is3dRequired = request.Is3dRequired,
            CallbackUrl = request.CallbackUrl,
            ReturnUrl = request.ReturnUrl,
            Name = request.Name,
            Surname = request.Surname,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            ClientIpAddress = request.ClientIpAddress,
            LanguageCode = request.LanguageCode,
            ExpiryDate = DateTime.Now.AddMinutes(expireInMinutes),
            MerchantId = merchant.Id,
            MerchantName = merchant.Name,
            MerchantNumber = merchant.Number,
            WebhookRetryCount = 0,
            PageViewType = request.PageViewType,
            EnableInstallments = request.EnableInstallments,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            RecordStatus = RecordStatus.Active
        };
    }
    
    private string GetUniqueKey()
    {
        int maxSize = 24;
        char[] chars = new char[52];
        string a = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        chars = a.ToCharArray();

        int size = maxSize;

        StringBuilder result = new StringBuilder(size);
        for (int i = 0; i < size; i++)
        {
            result.Append(chars[_randomGenerator.GenerateSecureRandomNumber(0,chars.Length)]);
        }

        return result.ToString();
    }

    public async Task<HostedPaymentResponse> SaveHostedPaymentAsync(SaveHostedPaymentCommand request)
    {
        var hpp = await _hostedPaymentRepository.GetAll()
            .Include(s => s.Installments)
            .Where(s => s.TrackingId == request.TrackingId
            && s.RecordStatus == RecordStatus.Active)
            .FirstOrDefaultAsync();

        if (hpp is null)
        {
            var apiResponse = await _errorCodeService.GetApiResponseCode(ApiErrorCode.HppNotFound, request.LanguageCode);

            return new HostedPaymentResponse
            {
                IsSucceed = false,
                ErrorCode = ApiErrorCode.HppNotFound,
                ErrorMessage = apiResponse.DisplayMessage,
                TrackingId = request.TrackingId,
                ConversationId = request.ConversationId
            };
        }

        var currency = await _currencyService.GetByNumberAsync(hpp.Currency);

        var merchant = await _merchantRepository.GetAll()
            .Where(s => s.Id == request.MerchantId)
            .FirstOrDefaultAsync();
        
        var parentMerchantFinancialTransaction = true;
        if (merchant?.ParentMerchantId is not null && merchant.ParentMerchantId != Guid.Empty)
        {
            var parentMerchant = await _merchantRepository.GetByIdAsync(merchant.ParentMerchantId);
            if (parentMerchant is not null)
            {
                parentMerchantFinancialTransaction = parentMerchant.FinancialTransactionAllowed;
            }
        }

        var validationResponse = await HostedPaymentPreValidateAsync(hpp, merchant, parentMerchantFinancialTransaction, request);

        if (!validationResponse.IsValid)
        {
            _logger.LogError($"HostedPayment PreValidation failed with code : {validationResponse.Code}, " +
                             $"Message: {validationResponse.Message}");

            await InsertValidationLogAsync(request, validationResponse, hpp, currency.Name);
            
            hpp.UpdateDate = DateTime.Now;
            hpp.HppStatus = ChannelStatus.Passive;
            hpp.RecordStatus = RecordStatus.Passive;
            hpp.HppPaymentStatus = ChannelPaymentStatus.Failed;
            hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                
            _context.HostedPayment.Update(hpp);
            await _context.SaveChangesAsync();
            
            await TriggerHppWebhookAsync(hpp.TrackingId);

            return new HostedPaymentResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ErrorMessage = validationResponse.Message,
                TrackingId = request.TrackingId,
                ConversationId = request.ConversationId
            };
        }

        var orderNumber = await _basePaymentService.GenerateOrderNumberAsync(merchant.Id, hpp.OrderId);

        var provisionRequest = new ProvisionCommand
        {
            CardToken = request.CardToken,
            CardHolderName = request.CardHolderName,
            MerchantId = merchant.Id,
            Amount = request.Amount,
            Currency = currency.Code,
            LanguageCode = request.LanguageCode,
            IntegrationMode = IntegrationMode.Hpp,
            ConversationId = orderNumber,
            InstallmentCount = request.InstallmentCount,
            PaymentType = VposPaymentType.Auth,
            ClientIpAddress = request.ClientIpAddress,
            ThreeDSessionId = request.ThreeDSessionId
        };

        var paymentResponse = await _paymentService.ProvisionAsync(provisionRequest);

        var returnResponse = paymentResponse.IsSucceed ? await MarkAsCompletedAsync(request, hpp, paymentResponse, orderNumber)
            : await MarkAsFailedAsync(request, hpp, paymentResponse, orderNumber);

        await TriggerHppWebhookAsync(hpp.TrackingId);
        
        return returnResponse;
    }
    
    private async Task<HostedPaymentResponse> MarkAsCompletedAsync(SaveHostedPaymentCommand request, HostedPayment hpp,
                                        ProvisionResponse provisionResponse, string orderNumber)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

                hpp.UpdateDate = DateTime.Now;
                hpp.HppStatus = ChannelStatus.Passive;
                hpp.RecordStatus = RecordStatus.Passive;
                hpp.HppPaymentStatus = ChannelPaymentStatus.Success;
                hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                
                _context.HostedPayment.Update(hpp);

                var hppTransaction = new HostedPaymentTransaction
                {
                    MerchantTransactionId = provisionResponse.TransactionId,
                    TrackingId = request.TrackingId,
                    TransactionType = TransactionType.Auth,
                    TransactionDate = DateTime.Now,
                    HppPaymentStatus = ChannelPaymentStatus.Success,
                    OrderId = orderNumber,
                    Amount = request.Amount,
                    InstallmentCount = request.InstallmentCount,
                    Currency = hpp.Currency,
                    Is3dRequired = hpp.Is3dRequired,
                    ThreeDSessionId = request.ThreeDSessionId,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    RecordStatus = RecordStatus.Active
                };

                await _context.HostedPaymentTransaction.AddAsync(hppTransaction);

                await _context.SaveChangesAsync();
                scope.Complete();

            });

            return new HostedPaymentResponse
            {
                TrackingId = hpp.TrackingId,
                OrderId = orderNumber,
                OriginalOrderId = hpp.OrderId,
                ErrorCode = provisionResponse.ErrorCode,
                ErrorMessage = provisionResponse.ErrorMessage,
                IsSucceed = true,
                ConversationId = request.ConversationId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateHostedPaymentTransaction -> ProvisionAsync IsSucceed:True : {ex}");
            throw;
        }
    }
    private async Task<HostedPaymentResponse> MarkAsFailedAsync(SaveHostedPaymentCommand request, HostedPayment hpp,
                                            ProvisionResponse provisionResponse, string orderNumber)
    {
        try
        {
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
                
                hpp.UpdateDate = DateTime.Now;
                hpp.HppStatus = ChannelStatus.Passive;
                hpp.RecordStatus = RecordStatus.Passive;
                hpp.HppPaymentStatus = ChannelPaymentStatus.Failed;
                hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
                
                _context.HostedPayment.Update(hpp);
                
                var hppTransaction = new HostedPaymentTransaction
                {
                    MerchantTransactionId = provisionResponse.TransactionId,
                    TrackingId = request.TrackingId,
                    TransactionType = TransactionType.Auth,
                    TransactionDate = DateTime.Now,
                    HppPaymentStatus = ChannelPaymentStatus.Failed,
                    OrderId = orderNumber,
                    Amount = request.Amount,
                    InstallmentCount = request.InstallmentCount,
                    Currency = hpp.Currency,
                    Is3dRequired = hpp.Is3dRequired,
                    ThreeDSessionId = request.ThreeDSessionId,
                    CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
                    RecordStatus = RecordStatus.Active
                };

                await _context.HostedPaymentTransaction.AddAsync(hppTransaction);
                await _context.SaveChangesAsync();
                scope.Complete();

            });
            
            return new HostedPaymentResponse
            {
                TrackingId = hpp.TrackingId,
                OrderId = orderNumber,
                OriginalOrderId = hpp.OrderId,
                ErrorCode = provisionResponse.ErrorCode,
                ErrorMessage = provisionResponse.ErrorMessage,
                IsSucceed = false,
                ConversationId = request.ConversationId
            };

        }
        catch (Exception ex)
        {
            _logger.LogError($"CreateHostedPaymentTransaction -> ProvisionAsync IsSucceed:False : {ex}");
            throw;
        }
    }
    private async Task<ValidationResponse> HostedPaymentPreValidateAsync(HostedPayment hpp, Merchant merchant, bool parentMerchantFinancialTransaction, SaveHostedPaymentCommand request)
    {
        if (!request.Gateway.Contains(Gateway.PFPageGateway.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidGateway, request.LanguageCode);
        }

        if (merchant is null || merchant.Id != hpp.MerchantId)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchant, request.LanguageCode);
        }

        if (hpp.ExpiryDate < DateTime.Now)
        {
            hpp.UpdateDate = DateTime.Now;
            hpp.RecordStatus = RecordStatus.Passive;
            hpp.HppStatus = ChannelStatus.Expired;
            hpp.HppPaymentStatus = ChannelPaymentStatus.Expired;
            hpp.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString();
            await _hostedPaymentRepository.UpdateAsync(hpp);
        
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.HppExpired, request.LanguageCode);
        }
        
        if (hpp.Is3dRequired && request.ThreeDSessionId is null)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.HppThreeDSRequired, request.LanguageCode);
        }
        
        if (merchant.MerchantStatus != MerchantStatus.Active || !merchant.FinancialTransactionAllowed || !parentMerchantFinancialTransaction)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidMerchantStatus, request.LanguageCode);
        }

        if (!merchant.IntegrationMode.ToString().Contains(IntegrationMode.Hpp.ToString()))
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.IntegrationModeNotAllowed, request.LanguageCode);
        }
        
        if (!hpp.EnableInstallments && request.InstallmentCount > 1)
        {
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, request.LanguageCode);
        }
        
        try
        {
            var cardToken = await _cardTokenService.GetByToken(request.CardToken);
            var cardInfo = await _cardTokenService.GetCardDetailsAsync(cardToken);
            var bin = await _binService.GetByNumberAsync(cardInfo.CardNumber);

            if (bin is null && !merchant.InternationalCardAllowed)
            {
                return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InternationalCardNotAllowed, request.LanguageCode);
            }

            if (bin is not null && hpp.Installments.Any() && request.InstallmentCount > 1)
            {
                if (hpp.Installments.FirstOrDefault(s => s.CardNetwork == bin.CardNetwork) is not null)
                {
                    if (hpp.Installments.FirstOrDefault(s =>
                            s.CardNetwork == bin.CardNetwork && s.Installment == request.InstallmentCount) is null)
                    {
                        return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, request.LanguageCode);
                    }
                }
                else
                {
                    if (hpp.Installments.FirstOrDefault(s =>
                            s.CardNetwork == CardNetwork.Unknown && s.Installment == request.InstallmentCount) is null)
                    {
                        return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidInstallment, request.LanguageCode);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"InvalidCardInfoException: {exception}");
            return await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.InvalidCardInfo, request.LanguageCode);
        }

        return new ValidationResponse { IsValid = true };
    }
    
    private async Task InsertValidationLogAsync(SaveHostedPaymentCommand request, ValidationResponse validationResponse, HostedPayment hpp, string currency)
    {
        await _apiRequestValidationLogRepository.AddAsync(new MerchantApiValidationLog
        {
            CreatedBy = _applicationUserService.ApplicationUserId.ToString(),
            Amount = hpp.Amount,
            Currency = currency,
            MerchantId = hpp.MerchantId,
            CardToken = request.CardToken,
            ConversationId = hpp.OrderId,
            InstallmentCount = request.InstallmentCount,
            LanguageCode = request.LanguageCode,
            TransactionType = TransactionType.Auth,
            ClientIpAddress = request.ClientIpAddress,
            ThreeDSessionId = request.ThreeDSessionId,
            ErrorCode = validationResponse.Code ?? "Error Code Not Found!",
            ErrorMessage = validationResponse.Message ?? "Error Message Not Found!"
        });
    }

    public async Task<HppTransactionResponse> GetHppTransactionAsync(string trackingId, Guid merchantId)
    {
        var merchant = await _merchantRepository.GetByIdAsync(merchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), merchantId);
        }
        
        var hpp = await _hostedPaymentRepository.GetAll()
            .Include(s => s.Installments)
            .FirstOrDefaultAsync(s => s.TrackingId == trackingId && s.MerchantId == merchantId);

        if (hpp is null)
        {
            var validationResponse = await _basePaymentService.GetValidationResponseAsync(ApiErrorCode.HppNotFound, merchant.Language);
            return new HppTransactionResponse
            {
                IsSucceed = false,
                ErrorCode = validationResponse.Code,
                ErrorMessage = validationResponse.Message,
                TrackingId = trackingId
            };
        }

        var hppTransactionResponse = PopulateInitialTransactionResponse(hpp);

        var hppTransaction = await _hppTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.TrackingId == hpp.TrackingId);

        if (hppTransaction is null)
        {
            return hppTransactionResponse;
        }

        hppTransactionResponse = AddHppTransactionToResponse(hppTransactionResponse, hppTransaction);

        if (hppTransaction.MerchantTransactionId == Guid.Empty)
        {
            if (hppTransaction.ThreeDSessionId is not null)
            {
                var verification = await _threeDVerificationRepository.GetAll()
                    .FirstOrDefaultAsync(s => s.Id == Guid.Parse(hppTransaction.ThreeDSessionId));
                if (verification is not null)
                {
                    hppTransactionResponse.ErrorCode = verification.MdStatus;
                    hppTransactionResponse.ErrorMessage = verification.MdErrorMessage;
                }
            }
            return hppTransactionResponse;
        }

        var merchantTransaction = await _merchantTransactionRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == hppTransaction.MerchantTransactionId);

        if (merchantTransaction is not null)
        {
            var acquireBank = await _acquireBankRepository.GetAll()
                .FirstOrDefaultAsync(s => s.BankCode == merchantTransaction.IssuerBankCode);

            var cardNetwork = acquireBank?.CardNetwork ?? CardNetwork.Unknown;
            hppTransactionResponse = AddMerchantTransactionToResponse(hppTransactionResponse, merchantTransaction, cardNetwork);
        }

        return hppTransactionResponse;
    }

    public async Task TriggerHppWebhookAsync(string trackingId)
    {
        try
        {
            var hpp = await _hostedPaymentRepository
                .GetAll()
                .Where(s => 
                    s.HppStatus != ChannelStatus.Active && 
                    s.WebhookStatus == WebhookStatus.Pending &&
                    s.TrackingId == trackingId)
                .FirstOrDefaultAsync();
    
            if (hpp is null)
            {
                throw new NotFoundException(nameof(HostedPayment), trackingId);
            }

            using var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:PF.HppWebhookNotification"));
            await endpoint.Send(new HppWebhookNotification { TrackingId = hpp.TrackingId, MerchantId = hpp.MerchantId}, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"TriggerHppWebhook Failed ! - TrackingId({trackingId}) Error : {exception}");
        }
    }

    public async Task<PaginatedList<HostedPaymentDto>> GetFilterListAsync(GetHppTransactionsQuery request)
    {
        var hppList = _hostedPaymentRepository.GetAll();
        
        if (request.MerchantId != Guid.Empty)
        {
            hppList = hppList.Where(b => b.MerchantId == request.MerchantId);
        }

        if (!string.IsNullOrEmpty(request.TrackingId))
        {
            hppList = hppList.Where(b => b.TrackingId == request.TrackingId);
        }

        if (!string.IsNullOrEmpty(request.OrderId))
        {
            hppList = hppList.Where(b => b.OrderId == request.OrderId);
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            hppList = hppList.Where(b => b.Name.ToLower().Contains(request.Name.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(request.Surname))
        {
            hppList = hppList.Where(b => b.Surname.ToLower().Contains(request.Surname.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(request.Email))
        {
            hppList = hppList.Where(b => b.Email.ToLower().Contains(request.Email.ToLower()));
        }
        
        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            hppList = hppList.Where(b => b.PhoneNumber.ToLower().Contains(request.PhoneNumber.ToLower()));
        }
        
        if (request.HppStatus is not null)
        {
            hppList = hppList.Where(b => b.HppStatus == request.HppStatus);
        }
        
        if (request.HppPaymentStatus is not null)
        {
            hppList = hppList.Where(b => b.HppPaymentStatus == request.HppPaymentStatus);
        }
        
        if (request.WebhookStatus is not null)
        {
            hppList = hppList.Where(b => b.WebhookStatus == request.WebhookStatus);
        }
        
        if (request.PageViewType is not null)
        {
            hppList = hppList.Where(b => b.PageViewType == request.PageViewType);
        }
        
        if (request.ExpiryDateStart is not null)
        {
            hppList = hppList.Where(b => b.ExpiryDate >= request.ExpiryDateStart);
        }

        if (request.ExpiryDateEnd is not null)
        {
            hppList = hppList.Where(b => b.ExpiryDate <= request.ExpiryDateEnd);
        }

        if (request.CreateDateStart is not null)
        {
            hppList = hppList.Where(b => b.CreateDate >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            hppList = hppList.Where(b => b.CreateDate <= request.CreateDateEnd);
        }
        
        return await hppList
            .PaginatedListWithMappingAsync<HostedPayment,HostedPaymentDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    private HppTransactionResponse PopulateInitialTransactionResponse(HostedPayment hpp)
    {
        return new HppTransactionResponse
        {
            IsSucceed = true,
            TrackingId = hpp.TrackingId,
            HppStatus = hpp.HppStatus,
            HppPaymentStatus = hpp.HppPaymentStatus,
            WebhookStatus = hpp.WebhookStatus,
            OriginalOrderId = hpp.OrderId,
            Amount = hpp.Amount,
            Currency = hpp.Currency,
            Is3dRequired = hpp.Is3dRequired,
            CommissionFromCustomer = hpp.CommissionFromCustomer,
            CallbackUrl = hpp.CallbackUrl,
            ReturnUrl = hpp.ReturnUrl,
            Name = hpp.Name,
            Surname = hpp.Surname,
            Email = hpp.Email,
            PhoneNumber = hpp.PhoneNumber,
            ClientIpAddress = hpp.ClientIpAddress,
            LanguageCode = hpp.LanguageCode,
            ExpiryDate = hpp.ExpiryDate,
            MerchantId = hpp.MerchantId,
            MerchantName = hpp.MerchantName,
            MerchantNumber = hpp.MerchantNumber,
            WebhookRetryCount = hpp.WebhookRetryCount,
            PageViewType = hpp.PageViewType,
            EnableInstallments = hpp.EnableInstallments,
            MerchantInstallments = _mapper.Map<List<HostedPaymentInstallmentDto>>(hpp.Installments)
        };
    }

    private static HppTransactionResponse AddHppTransactionToResponse(HppTransactionResponse response,
        HostedPaymentTransaction hppTransaction)
    {
        response.TransactionType = hppTransaction.TransactionType;
        response.TransactionDate = hppTransaction.TransactionDate;
        response.OrderId = hppTransaction.OrderId;
        response.InstallmentCount = hppTransaction.InstallmentCount;
        return response;
    }
    
    private static HppTransactionResponse AddMerchantTransactionToResponse(HppTransactionResponse response,
        MerchantTransaction merchantTransaction, CardNetwork cardNetwork)
    {
        response.PaymentConversationId = merchantTransaction.ConversationId;
        response.TransactionStatus = merchantTransaction.TransactionStatus;
        response.CardNumber = merchantTransaction.CardNumber;
        response.CardType = merchantTransaction.CardType;
        response.CardBrand = string.IsNullOrEmpty(merchantTransaction.CardNumber)
            ? CardBrand.Undefined
            : CardHelper.GetCardBrand(merchantTransaction.CardNumber);
        response.CardNetwork = cardNetwork;
        response.Is3ds = merchantTransaction.Is3ds;
        response.IssuerBankCode = merchantTransaction.IssuerBankCode;

        if (response.HppPaymentStatus == ChannelPaymentStatus.Failed)
        {
            response.ErrorCode = merchantTransaction.ResponseCode;
            response.ErrorMessage = merchantTransaction.ResponseDescription;
        }
        
        return response;
    }
}