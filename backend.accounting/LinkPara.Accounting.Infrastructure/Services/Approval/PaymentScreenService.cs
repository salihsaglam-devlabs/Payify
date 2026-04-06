using LinkPara.Accounting.Application.Features.Payments.Commands.PostPayment;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Accounting.Infrastructure.Services.Approval;

public class PaymentScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Payment> _repository;
    private readonly IStringLocalizer _localizer;

    public PaymentScreenService(IStringLocalizerFactory factory, IGenericRepository<Payment> repository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Accounting.API");
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var paymentId))
        {
            throw new InvalidCastException();
        }

        var payment = await _repository.GetAll()
                                       .SingleOrDefaultAsync(x => x.Id == paymentId);
        if (payment is null)
        {
            throw new NotFoundException(nameof(Payment));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), payment.Id.ToString() },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Payments"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<PostPaymentCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("ReferenceId"), requestBody.ReferenceId },
            { _localizer.GetString("OperationType"), requestBody.OperationType.ToString()},
            { _localizer.GetString("Amount"), requestBody.Amount.ToString() },
            { _localizer.GetString("TransactionDate"), requestBody.TransactionDate.ToString() },
            { _localizer.GetString("AccountingTransactionType"), requestBody.AccountingTransactionType.ToString() },
            { _localizer.GetString("AccountingCustomerType"), requestBody.AccountingCustomerType.ToString() },
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Payments"
        });
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
