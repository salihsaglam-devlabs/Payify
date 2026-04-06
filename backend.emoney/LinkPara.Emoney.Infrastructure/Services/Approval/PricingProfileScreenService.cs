using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.SavePricingProfile;
using LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfile;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class PricingProfileScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<PricingProfile> _repository;
    private readonly IGenericRepository<PricingProfileItem> _pricingProfileItemRepository;
    private readonly IStringLocalizer _localizer;

    public PricingProfileScreenService(IStringLocalizerFactory factory, IGenericRepository<PricingProfile> repository, IGenericRepository<PricingProfileItem> pricingProfileItemRepository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
        _pricingProfileItemRepository = pricingProfileItemRepository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryString = System.Web.HttpUtility.ParseQueryString(request.QueryParameters);

        if (!Guid.TryParse(queryString["id"], out var id))
        {
            throw new InvalidCastException();
        }

        var pricingProfileItem = await _pricingProfileItemRepository.GetAll()
                                               .Include(x => x.PricingProfile)
                                               .SingleOrDefaultAsync(x => x.Id == id);
        if (pricingProfileItem is null)
        {
            throw new NotFoundException(nameof(TierLevel));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), pricingProfileItem.Id },
            { _localizer.GetString("Name"), pricingProfileItem.PricingProfile.Name },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "EmoneyPricingProfiles"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SavePricingProfileCommand>(request.Body);

        var data = new Dictionary<string, object>()
        {
            { _localizer.GetString("CurrencyCode"),requestBody.CurrencyCode },
            { _localizer.GetString("TransferType"),requestBody.TransferType.ToString() },
            { _localizer.GetString("ActivationDateStart"),requestBody.ActivationDateStart.ToString() }
        };

        return Task.FromResult(new ApprovalScreenResponse()
        {
            DisplayScreenFields = data,
            Resource = "EmoneyPricingProfiles"
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdatePricingProfileCommand>(request.Body);

        var pricingProfile = await _repository.GetAll()
                                              .SingleOrDefaultAsync(x => x.Id == requestBody.Id);
        if (pricingProfile is null)
        {
            throw new NotFoundException(nameof(PricingCommercial));
        }

        var data = new Dictionary<string, object>()
        {
            { _localizer.GetString("Id"),pricingProfile.Id.ToString() },
            { _localizer.GetString("CurrencyCode"),pricingProfile.CurrencyCode },
            { _localizer.GetString("TransferType"),pricingProfile.TransferType.ToString() },
            { _localizer.GetString("ActivationDateStart"),pricingProfile.ActivationDateStart.ToString() }
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(pricingProfile, requestBody, _localizer);

        return new ApprovalScreenResponse()
        {
            DisplayScreenFields = data,
            Resource = "EmoneyPricingProfiles",
            UpdatedFields = updatedFields
        };
    }
}
