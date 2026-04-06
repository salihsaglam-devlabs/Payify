using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantIntegratorsScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantIntegrator> _repository;
    private readonly IStringLocalizer _localizer;
    public MerchantIntegratorsScreenService(IGenericRepository<MerchantIntegrator> repository,
        IStringLocalizerFactory factory)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetByIdAsync(id);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("IntegratorName").Value, entity.Name},
            { _localizer.GetString("IntegratorCommissionRate").Value, entity.CommissionRate.ToString()},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantIntegratorCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("IntegratorName").Value, requestBody.Name},
            { _localizer.GetString("IntegratorCommissionRate").Value, requestBody.CommissionRate.ToString()},
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
