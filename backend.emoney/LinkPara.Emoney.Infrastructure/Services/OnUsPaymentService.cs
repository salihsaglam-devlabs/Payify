using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.Emoney.Application.Commons.Exceptions;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.LimitModels;
using LinkPara.Emoney.Application.Commons.Models.ProvisionModels;
using LinkPara.Emoney.Application.Features.OnUsPayments;
using LinkPara.Emoney.Application.Features.OnUsPayments.Commands;
using LinkPara.Emoney.Application.Features.OnUsPayments.Queries;
using LinkPara.Emoney.Application.Features.Provisions.Queries.ProvisionPreview;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.HttpProviders.PF;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.HttpProviders.PF.Models.Response;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using OnUsPaymentStatus = LinkPara.Emoney.Domain.Enums.OnUsPaymentStatus;
using ProvisionPreviewResponse = LinkPara.Emoney.Application.Commons.Models.ProvisionModels.ProvisionPreviewResponse;

namespace LinkPara.Emoney.Infrastructure.Services;

public class OnUsPaymentService : IOnUsPaymentService
{
    private const string GeneralErrorCode = "500";
    private readonly IGenericRepository<OnUsPaymentRequest> _onUsPaymentRequestRepository;
    private readonly IProvisionService _provisionService;
    private readonly IUserService _userService;
    private readonly ILimitService _limitService;
    private readonly IGenericRepository<AccountUser> _accountUserRepository;
    private readonly IPushNotificationSender _pusNotificationSender;
    private readonly ISearchService _searchService;
    private readonly IParameterService _parameterService;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
    private readonly IPfOnUsService _pfOnUsService;
    private readonly IStringLocalizer _localizer;

    public OnUsPaymentService(IGenericRepository<OnUsPaymentRequest> onUsPaymentRequestRepository,
        IProvisionService provisionService,
        IUserService userService,
        IGenericRepository<AccountUser> accountUserRepository,
        IPushNotificationSender pusNotificationSender,
        ISearchService searchService,
        ILimitService limitService,
        IParameterService parameterService,
        IContextProvider contextProvider,
        IAuditLogService auditLogService,
        IPfOnUsService pfOnUsService,
        IStringLocalizerFactory factory)
    {
        _onUsPaymentRequestRepository = onUsPaymentRequestRepository;
        _provisionService = provisionService;
        _userService = userService;
        _accountUserRepository = accountUserRepository;
        _pusNotificationSender = pusNotificationSender;
        _searchService = searchService;
        _limitService = limitService;
        _parameterService = parameterService;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
        _pfOnUsService = pfOnUsService;
        _localizer = factory.Create("ErrorMessages", "LinkPara.Emoney.API");
        
    }

    public async Task<OnUsPaymentResponse> InitOnUsPaymentAsync(InitOnUsPaymentCommand request)
    {
        try
        {
            var duplicateOrder = await _onUsPaymentRequestRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.OrderId == request.OrderId 
                                           && x.Status == OnUsPaymentStatus.Pending);

            if (duplicateOrder != null)
            {
                throw new DuplicateRecordException("OrderId : ", request.OrderId);
            }

            var user = await GetUserByPhone(request.PhoneNumber);
            if (user is null)
            {
                throw new NotFoundException("PhoneNumber", request.PhoneNumber);
            }

            await CheckBlackListAsync(user);
            var deviceInfo = GetDeviceInfo(user.Id);
            var account = await GetAccount(user.Id);
            var validateLimit = await ValidateLimitsAsync(account.Id, request);
            if (!validateLimit)
            {
                throw new LimitExceededException();
            }

            var templateData = new Dictionary<string, string>
            {
                { "MerchantName", request.MerchantName },
                { "Amount", request.Amount.ToString() },
                { "CurrencyCode", request.Currency }
            };

            var onUsPaymentRequestitem = new OnUsPaymentRequest
            {
                UserId = user.Id,
                UserName = string.Concat(user.FirstName, " ", user.LastName),
                MerchantName = request.MerchantName,
                MerchantNumber = request.MerchantNumber,
                Amount = request.Amount,
                Currency = request.Currency,
                Status = OnUsPaymentStatus.Pending,
                PhoneCode = request.PhoneCode,
                PhoneNumber = request.PhoneNumber,
                OrderId = request.OrderId,
                ConversationId = request.ConversationId,
                ExpireDate = request.ExpireDate,
                RequestDate = request.RequestDate,
                CreatedBy = user.Id.ToString()
            };
            await _onUsPaymentRequestRepository.AddAsync(onUsPaymentRequestitem);

            await PushSenderNotificationAsync(templateData, account, onUsPaymentRequestitem);
            await AuditLogAsync(request, true);

            return new OnUsPaymentResponse
            {
                IsSuccess = true,
                OnUsPaymentRequestId = onUsPaymentRequestitem.Id.ToString()
            };
        }
        catch (Exception ex)
        {
            await AuditLogAsync(request, false);
            return new OnUsPaymentResponse
            {
                IsSuccess = false,
                ErrorMessage = ex.Message,
                ErrorCode = ex is ApiException apiException ? apiException.Code : GeneralErrorCode
            };
        }
    }

    private async Task<bool> ValidateLimitsAsync(Guid accountId, InitOnUsPaymentCommand command)
    {
        var response = await _limitService.IsLimitExceededAsync(new LimitControlRequest
        {
            Amount = command.Amount,
            CurrencyCode = command.Currency,
            LimitOperationType = LimitOperationType.OnUs,
            AccountId = accountId
        });

        return !response.IsLimitExceeded;
    }

    public async Task<OnUsPaymentRequest> GetOnUsPaymentDetailsAsync(Guid onUsPaymentRequestId)
    {
        var onUsPayment = await _onUsPaymentRequestRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.Id == onUsPaymentRequestId);

        if (onUsPayment == null)
        {
            throw new NotFoundException(nameof(OnUsPaymentRequest), onUsPaymentRequestId.ToString());
        }

        return onUsPayment;
    }

    public async Task<PaginatedList<OnUsPaymentRequest>> GetOnUsPaymentsAsync(GetOnUsPaymentQuery request)
    {
        var list = _onUsPaymentRequestRepository.GetAll()
            .Where(x => x.Status == OnUsPaymentStatus.Success);

        if (request.TransactionId != null)
        {
            list = list.Where(s => s.TransactionId == new Guid(request.TransactionId.ToString()));
        }

        if (!string.IsNullOrEmpty(request.OrderId))
        {
            list = list.Where(s => (s.OrderId).Contains(request.OrderId));
        }

        if (!string.IsNullOrEmpty(request.WalletNumber))
        {
            list = list.Where(s => (s.WalletNumber).Contains(request.WalletNumber));
        }
        
        if (!string.IsNullOrEmpty(request.Status.ToString()))
        {
            list = list.Where(s => s.Status == request.Status);
        }

        if (!string.IsNullOrEmpty(request.MerchantName))
        {
            list = list.Where(s => (s.MerchantName).Contains(request.MerchantName));
        }

        if (!string.IsNullOrEmpty(request.UserName))
        {
            list = list.Where(s => (s.UserName).Contains(request.UserName));
        }

        if (request.TransactionDate != null)
        {
            list = list.Where(s => s.TransactionDate == request.TransactionDate);
        }

        return await list.PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<Unit> OnUsPaymentUpdateStatusAsync(OnUsPaymentUpdateStatusCommand request)
    {
        var onUsPaymentItem = await _onUsPaymentRequestRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.Id == request.OnUsPaymentRequestId);

        onUsPaymentItem.Status = request.Status;
        onUsPaymentItem.LastModifiedBy = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
        onUsPaymentItem.UpdateDate = DateTime.Now;
        await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentItem);

        return Unit.Value;
    }

    public async Task<ProvisionPreviewResponse> ApproveOnUsPaymentAsync(ApproveOnUsPaymentCommand request)
    {
        var provisionPreview = new ProvisionPreviewResponse();
        var onUsPaymentDetails = await _onUsPaymentRequestRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.Id == request.OnUsPaymentRequestId);

        if (onUsPaymentDetails == null)
        {
            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = string.Concat(nameof(OnUsPaymentRequest), " ",
                                                    request.OnUsPaymentRequestId.ToString(), " ", 
                                                    _localizer["NotFound"]);
            provisionPreview.ErrorCode = ErrorCode.NotFound;
            return provisionPreview;
        }

        var user = await _userService.GetUserAsync(onUsPaymentDetails.UserId);

        if (!request.IsVerifiedByUser)
        {
            try
            {
                await _pfOnUsService.VerifyOnUsPaymentAsync(new VerifyOnUsPaymentRequest
                {
                    OrderId = onUsPaymentDetails.OrderId,
                    ConversationId = onUsPaymentDetails.ConversationId,
                    MerchantNumber = onUsPaymentDetails.MerchantNumber,
                    Name = user.FirstName,
                    Surname = user.LastName,
                    Email = user.Email,
                    PhoneCode = onUsPaymentDetails.PhoneCode,
                    PhoneNumber = onUsPaymentDetails.PhoneNumber,
                    WalletNumber = string.Empty,
                    LanguageCode = "TR",
                    IsVerifiedByUser = request.IsVerifiedByUser,
                    ClientIpAddress = _contextProvider.CurrentContext.ClientIpAddress ?? "127.0.0.1"
                });
            }
            catch (Exception ex)
            {
                onUsPaymentDetails.Status = OnUsPaymentStatus.Failed;
                onUsPaymentDetails.ErrorMessage = ex.ToString();
                onUsPaymentDetails.ErrorCode = GeneralErrorCode;
                onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
                onUsPaymentDetails.UpdateDate = DateTime.Now;
                await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);

                provisionPreview.IsSuccess = false;
                provisionPreview.ErrorMessage = ex.Message;
                return provisionPreview;
            }

            onUsPaymentDetails.Status = OnUsPaymentStatus.Rejected;
            onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
            onUsPaymentDetails.UpdateDate = DateTime.Now;
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);

            provisionPreview.IsSuccess = true;
            provisionPreview.ErrorMessage = string.Empty;
            provisionPreview.ErrorCode = string.Empty;
            return provisionPreview;
        }

        if (onUsPaymentDetails.Status != OnUsPaymentStatus.Pending)
        {
            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = _localizer["StatusNotAcceptable"];
            provisionPreview.ErrorCode = ApiErrorCode.StatusNotAcceptable;
            return provisionPreview;
        }

        
        if (onUsPaymentDetails.ExpireDate < DateTime.Now)
        {
            onUsPaymentDetails.Status = OnUsPaymentStatus.Expired;
            onUsPaymentDetails.ErrorMessage = _localizer["TimeOut"];
            onUsPaymentDetails.ErrorCode = ApiErrorCode.Timeout;
            onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
            onUsPaymentDetails.UpdateDate = DateTime.Now;
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);

            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = _localizer["TimeOut"];
            provisionPreview.ErrorCode = ApiErrorCode.Timeout;
            return provisionPreview;
        }
        
        try
        {
            provisionPreview = await _provisionService.ProvisionPreviewAsync(new ProvisionPreviewQuery
            {
                WalletNumber = request.SenderWalletNumber,
                CurrencyCode = onUsPaymentDetails.Currency,
                PartnerId = Guid.Empty,
                Amount = onUsPaymentDetails.Amount,
                UserId = new Guid(onUsPaymentDetails.CreatedBy)
            });
        }
        catch (Exception ex)
        {            
            onUsPaymentDetails.Status = OnUsPaymentStatus.Failed;
            onUsPaymentDetails.ErrorMessage = ex.ToString();
            onUsPaymentDetails.ErrorCode = GeneralErrorCode;
            onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
            onUsPaymentDetails.UpdateDate = DateTime.Now;
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);

            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = ex.Message;
            return provisionPreview;
        }

        if (!provisionPreview.IsSuccess)
        {            
            return provisionPreview;
        }

                
        VerifyOnUsPaymentResponse OnUsPaymentResponse = null;
        try
        {
            OnUsPaymentResponse = await _pfOnUsService.VerifyOnUsPaymentAsync(new VerifyOnUsPaymentRequest
            {
                OrderId = onUsPaymentDetails.OrderId,
                ConversationId = onUsPaymentDetails.ConversationId,
                MerchantNumber = onUsPaymentDetails.MerchantNumber,
                Name = user.FirstName,
                Surname = user.LastName,
                Email = user.Email,
                PhoneCode = onUsPaymentDetails.PhoneCode,
                PhoneNumber = onUsPaymentDetails.PhoneNumber,
                WalletNumber = request.SenderWalletNumber,
                LanguageCode = "TR",
                IsVerifiedByUser = request.IsVerifiedByUser,
                ClientIpAddress = _contextProvider.CurrentContext.ClientIpAddress ?? "127.0.0.1"
            });
        }
        catch(Exception ex)
        {
            onUsPaymentDetails.Status = OnUsPaymentStatus.Failed;
            onUsPaymentDetails.ErrorMessage = ex.ToString();
            onUsPaymentDetails.ErrorCode = GeneralErrorCode;
            onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
            onUsPaymentDetails.UpdateDate = DateTime.Now;
            await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);

            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = ex.Message;
            return provisionPreview;
        }

        onUsPaymentDetails = await _onUsPaymentRequestRepository.GetAll().FirstOrDefaultAsync(x => x.Id == request.OnUsPaymentRequestId);
        onUsPaymentDetails.LastModifiedBy = user.Id.ToString();
        onUsPaymentDetails.UpdateDate = DateTime.Now;
        if (OnUsPaymentResponse.IsSucceed)
        {
            onUsPaymentDetails.MerchantTransactionId = OnUsPaymentResponse.TransactionId.ToString();
            onUsPaymentDetails.Status =  OnUsPaymentStatus.Success;
            provisionPreview.IsSuccess = true;
        }
        else
        {
            onUsPaymentDetails.Status = OnUsPaymentStatus.Failed;
            onUsPaymentDetails.ErrorMessage = OnUsPaymentResponse.ErrorMessage;
            onUsPaymentDetails.ErrorCode = OnUsPaymentResponse.ErrorCode;  

            provisionPreview.IsSuccess = false;
            provisionPreview.ErrorMessage = OnUsPaymentResponse.ErrorMessage; 
            provisionPreview.ErrorCode = OnUsPaymentResponse.ErrorCode;
        }

        await _onUsPaymentRequestRepository.UpdateAsync(onUsPaymentDetails);
        return provisionPreview;
    }

    private async Task<UserDto> GetUserByPhone(string phoneNumber)
    {
        var user = await _userService.GetAllUsersAsync(new GetUsersRequest
        {
            PhoneNumber = phoneNumber
        });

        if (user is null || user.Items.Count == 0)
        {
            return null;
        }

        return user.Items.FirstOrDefault(x => x.UserType == HttpProviders.Identity.Models.Enums.UserType.Individual);
    }

    private async Task<List<UserDeviceInfoDto>> GetDeviceInfo(Guid userId)
    {
        var deviceInfo = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = new List<Guid> { userId }
        });

        if (deviceInfo is null || deviceInfo.Count == 0)
        {
            return null;
        }

        return deviceInfo;
    }

    private async Task<Account> GetAccount(Guid userId)
    {
        var accountUser = await _accountUserRepository
                .GetAll()
                .Include(s => s.Account)
                .FirstOrDefaultAsync(s => s.UserId == userId);

        if (accountUser is null)
        {
            throw new NotFoundException(nameof(AccountUser), userId);
        }

        if (accountUser.Account is null)
        {
            throw new NotFoundException(nameof(Account), accountUser.AccountId);
        }

        return accountUser.Account;
    }

    private async Task PushSenderNotificationAsync(Dictionary<string, string> templateData,
                                                    Account senderAccount,
                                                    OnUsPaymentRequest onUsPaymentRequest)
    {
        var senderUserList = senderAccount.AccountUsers
            .Select(x =>
            {
                return new NotificationUserInfo
                {
                    UserId = x.UserId,
                    FirstName = x.Firstname,
                    LastName = x.Lastname,
                };
            })
            .ToList();

        var senderUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(new GetUserDeviceInfoRequest
        {
            UserIdList = senderUserList.Select(x => x.UserId).ToList(),
        });

        var senderNotificationRequest = new SendPushNotification
        {
            TemplateName = "OnUsPaymentNotification",
            TemplateParameters = templateData,
            Tokens = senderUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
            UserList = senderUserList,
            Data = new Dictionary<string, string>
            {
                { "OnUsPaymentRequestId" , onUsPaymentRequest.Id.ToString() },
                { "ExpireDate" , onUsPaymentRequest.ExpireDate.ToString() },
                { "Status" , onUsPaymentRequest.Status.ToString() }
            }
        };

        await _pusNotificationSender.SendPushNotificationAsync(senderNotificationRequest);
    }

    private async Task CheckBlackListAsync(UserDto user)
    {
        SearchByNameRequest searchRequest = new()
        {
            Name = $"{user.FirstName} {user.LastName}",
            BirthYear = Convert.ToDateTime(user.BirthDate).Year.ToString(),
            SearchType = SearchType.Any,
            FraudChannelType = FraudChannelType.Web
        };
        var blackListControl = await _searchService.GetSearchByName(searchRequest);

        var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");
        if ((blackListControl.MatchStatus == MatchStatus.PotentialMatch || blackListControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListControl.MatchRate >= Convert.ToInt32(matchRate))
        {

            throw new UserInBlacklistException("");
        }
    }

    private async Task AuditLogAsync(InitOnUsPaymentCommand request, bool isSuccess)
    {
        var userId = _contextProvider.CurrentContext.UserId ?? Guid.Empty.ToString();
        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = isSuccess,
                LogDate = DateTime.Now,
                Operation = "OnUsPayment",
                SourceApplication = "Emoney",
                Resource = "Wallet",
                UserId = new Guid(userId),
                Details = new Dictionary<string, string>
                {
                    { "OrderId", request.OrderId },
                    { "MerchantNumber", request.MerchantNumber }
                }
            }
        );
    }

}
