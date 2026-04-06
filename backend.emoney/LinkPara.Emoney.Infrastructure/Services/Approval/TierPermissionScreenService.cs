using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.UpdatePricingCommercial;
using LinkPara.Emoney.Application.Features.Limits.Commands.PutTierPermission;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class TierPermissionScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<TierPermission> _repository;
    private readonly IStringLocalizer _localizer;

    public TierPermissionScreenService(IStringLocalizerFactory factory,
        IGenericRepository<TierPermission> repository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
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
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<PutTierPermissionCommand>(request.Body);

        var tierPermission = await _repository.GetAll()
                                              .SingleOrDefaultAsync(x => x.Id == requestBody.Id);
        if (tierPermission is null)
        {
            throw new NotFoundException(nameof(PricingCommercial));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), tierPermission.Id.ToString() },
            { _localizer.GetString("TierLevel"), tierPermission.TierLevel.ToString() },
            { _localizer.GetString("IsEnabled"), tierPermission.IsEnabled.ToString() },
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(tierPermission, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "TierPermissions",
            UpdatedFields = updatedFields
        };
    }
}
