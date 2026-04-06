using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantReturnPools.Command.ActionMerchantReturnPool;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantReturnPoolScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantReturnPool> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    public MerchantReturnPoolScreenService(IGenericRepository<MerchantReturnPool> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _merchantRepository = merchantRepository;
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<ActionMerchantReturnPoolCommand>(request.Body);

        var entity = await _repository.GetByIdAsync(requestBody.MerchantReturnPoolId);

        var merchant = await _merchantRepository.GetByIdAsync(entity.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("ReturnStatus").Value, _localizer.GetString(requestBody.ReturnStatus.ToString()).Value},
            { _localizer.GetString("RejectDesc").Value, _localizer.GetString(requestBody.RejectDescription).Value},
            { _localizer.GetString("RejectReason").Value, _localizer.GetString(requestBody.RejectReason).Value},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
