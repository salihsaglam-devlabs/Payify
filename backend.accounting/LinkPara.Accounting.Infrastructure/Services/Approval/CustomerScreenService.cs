using LinkPara.Accounting.Application.Features.Customers.Commands.SaveCustomer;
using LinkPara.Accounting.Domain.Entities;
using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Accounting.Infrastructure.Services.Approval;

public class CustomerScreenService : IApprovalScreenService
{
    private readonly IStringLocalizer _localizer;

    public CustomerScreenService(IStringLocalizerFactory factory, IGenericRepository<Customer> repository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.Accounting.API");
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveCustomerCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("CustomerCode"), requestBody.Code },
            { _localizer.GetString("CustomerTitle"), requestBody.Title}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Customers"
        });
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
