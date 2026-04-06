using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Chargebacks.Commands;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;
public class ChargebackScreenService : IApprovalScreenService
{    
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<OnUsPaymentRequest> _onUsPaymentRequestRepository;

    public ChargebackScreenService(IStringLocalizerFactory factory,
        IGenericRepository<OnUsPaymentRequest> onUsPaymentRequestRepository)
    {        
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
        _onUsPaymentRequestRepository = onUsPaymentRequestRepository;
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<InitChargebackCommand>(request.Body);

        var onUsPayment = _onUsPaymentRequestRepository.GetAll()
            .Where(x => x.TransactionId == requestBody.TransactionId)
            .FirstOrDefault();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, onUsPayment.UserName},
            { _localizer.GetString("MerchantName").Value, onUsPayment.MerchantName},
            { _localizer.GetString("OrderId").Value, onUsPayment.OrderId},
            { _localizer.GetString("Amount").Value, onUsPayment.Amount},
            { _localizer.GetString("CurrencyCode").Value, onUsPayment.Currency},
            { _localizer.GetString("TransactionId").Value, onUsPayment.TransactionId},
            { _localizer.GetString("TransactionDate").Value, onUsPayment.TransactionDate.ToString("dd-MM-yyyy HH:mm:ss")},
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Chargeback"
        });
    }
    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<ApproveChargebackCommand>(request.Body);

        var onUsPayment = _onUsPaymentRequestRepository.GetAll()
            .Where(x => x.TransactionId == requestBody.TransactionId)
            .FirstOrDefault();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, onUsPayment.UserName},
            { _localizer.GetString("MerchantName").Value, onUsPayment.MerchantName},
            { _localizer.GetString("OrderId").Value, onUsPayment.OrderId},
            { _localizer.GetString("Amount").Value, onUsPayment.Amount},
            { _localizer.GetString("CurrencyCode").Value, onUsPayment.Currency},
            { _localizer.GetString("TransactionId").Value, onUsPayment.TransactionId},
            { _localizer.GetString("TransactionDate").Value, onUsPayment.TransactionDate.ToString("dd-MM-yyyy HH:mm:ss")},
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Chargeback"
        });
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
