using LinkPara.Emoney.Application.Commons.Enums;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Requests;
using LinkPara.Emoney.Application.Commons.Models.Masterpass.Responses;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;
using LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TransactionType = LinkPara.Emoney.Application.Commons.Enums.TransactionType;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class TopupProvisionTimeoutConsumer : IConsumer<TopupProvisionTimeout>
{
    private readonly IGenericRepository<CardTopupRequest> _repository;
    private readonly IPaymentProviderServiceFactory _paymentServiceFactory;
    private readonly ILogger<TopupProvisionTimeoutConsumer> _logger;
    private readonly string _paymentProviderType;
    private readonly IApplicationUserService _applicationUserService;
    private readonly IMasterpassService _masterpassService;

    public TopupProvisionTimeoutConsumer(IGenericRepository<CardTopupRequest> repository,
        IPaymentProviderServiceFactory paymentServiceFactory,
        IVaultClient vaultClient,
        ILogger<TopupProvisionTimeoutConsumer> logger,
        IApplicationUserService applicationUserService,
        IMasterpassService masterpassService)
    {
        _repository = repository;
        _paymentServiceFactory = paymentServiceFactory;
        _paymentProviderType = vaultClient.GetSecretValue<string>("SharedSecrets", "PaymentProviderConfigs", "Type");
        _logger = logger;
        _applicationUserService = applicationUserService;
        _masterpassService = masterpassService;
    }

    public async Task Consume(ConsumeContext<TopupProvisionTimeout> context)
    {
        var paymentProviderService = await _paymentServiceFactory.GetPaymentProviderServiceAsync(_paymentProviderType);

        var cardTopupRequests = await _repository.GetAll()
            .Where(x => x.Status == CardTopupRequestStatus.ProvisionTimeout
                        && x.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        foreach (var cardTopupRequest in cardTopupRequests)
        {
            var amount = Math.Round(cardTopupRequest.Amount + cardTopupRequest.Fee + cardTopupRequest.CommissionTotal + cardTopupRequest.BsmvTotal, 2);

            if (cardTopupRequest.PaymentProviderType == PaymentProviderType.PayifyPf)
            {
                var inquireResponse = await paymentProviderService.InquireAsync(
                      new InquireRequestModel
                      {
                          CardTopupRequestId = cardTopupRequest.Id,
                          ConversationId = cardTopupRequest.ConversationId,
                          PaymentConversationId = cardTopupRequest.ConversationId,
                      });

                if (IsValidInquireResponse(inquireResponse))
                {
                    cardTopupRequest.OrderId = inquireResponse.OrderId;
                    await ReturnOrReverseProcessAsync(paymentProviderService, cardTopupRequest, amount);
                }
                else
                {
                    await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
                }
            }
            else if (cardTopupRequest.PaymentProviderType == PaymentProviderType.Masterpass)
            {
                var formattedAmount = _masterpassService.FormatAmount(amount);
                await _masterpassService.RefundOrVoidProcessAsync(cardTopupRequest, CardTopupRequestStatus.Failed, CardTopupRequestStatus.Failed, CardTopupRequestStatus.BankActionRequired, formattedAmount);
            }
        }
    }

    private bool IsValidInquireResponse(InquireResponseModel inquireResponse)
    {
        return inquireResponse != null
            && inquireResponse.IsSucceed
            && !string.IsNullOrEmpty(inquireResponse.OrderId)
            && inquireResponse.TransactionStatus == PfTransactionStatus.Success
            && inquireResponse.TransactionType == TransactionType.Auth;
    }

    private async Task ReturnOrReverseProcessAsync(IPaymentProviderService paymentProviderService, CardTopupRequest cardTopupRequest, decimal amount)
    {
        var reverseResponse = new ReverseResponse();
        try
        {
            reverseResponse = await paymentProviderService.ReverseAsync(new ReverseRequest
            {
                OrderId = cardTopupRequest.OrderId,
                ConversationId = cardTopupRequest.ConversationId
            });
        }
        catch (Exception exception)
        {
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
            _logger.LogError($"TopupProvisionTimeoutConsumer error : {exception}");
        }

        if (!reverseResponse.IsSucceed)
        {
            var returnResponse = new ReturnResponse();
            try
            {
                returnResponse = await paymentProviderService.ReturnAsync(new ReturnRequest
                {
                    OrderId = cardTopupRequest.OrderId,
                    Amount = amount,
                    SignatureDataResponse = reverseResponse?.SignatureDataResponse,
                });
            }
            catch (Exception exception)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
                _logger.LogError($"TopupProvisionTimeoutConsumer error : {exception}");
            }

            if (!returnResponse.IsSucceed)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.BankActionRequired);
            }
            else if (returnResponse.ReturnApprovalStatus != ReturnApprovalStatus.PendingApproval && returnResponse.IsSucceed)
            {
                await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
            }
        }
        else
        {
            await UpdateCardTopupStatusAsync(cardTopupRequest, CardTopupRequestStatus.Failed);
        }
    }

    private async Task UpdateCardTopupStatusAsync(CardTopupRequest request,
        CardTopupRequestStatus status)
    {
        request.Status = status;
        request.UpdateDate = DateTime.Now;
        request.LastModifiedBy = _applicationUserService.ApplicationUserId.ToString() ?? Guid.Empty.ToString();

        await _repository.UpdateAsync(request);
    }
}