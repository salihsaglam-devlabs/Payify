using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.CreatePricingCommercial;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.UpdatePricingCommercial;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class CommercialPricingScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<PricingCommercial> _repository;
    private readonly IStringLocalizer _localizer;

    public CommercialPricingScreenService(IStringLocalizerFactory factory, IGenericRepository<PricingCommercial> repository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var commercialPricingId))
        {
            throw new InvalidCastException();
        }

        var commercialPricing = await _repository.GetAll()
                                               .SingleOrDefaultAsync(x => x.Id == commercialPricingId);
        if (commercialPricing is null)
        {
            throw new NotFoundException(nameof(PricingCommercial));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), commercialPricing.Id.ToString() },
            { _localizer.GetString("PricingCommercialType"), commercialPricing.PricingCommercialType.ToString() },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "CommercialPricing"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreatePricingCommercialCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("ActivationDate"), requestBody.ActivationDate.ToString() },
            { _localizer.GetString("CommissionRate"), requestBody.CommissionRate.ToString() },
            { _localizer.GetString("PricingCommercialType"), requestBody.PricingCommercialType.ToString() },
            { _localizer.GetString("CurrencyCode"), requestBody.CurrencyCode },
            { _localizer.GetString("MaxDistinctSenderAmount"), requestBody.MaxDistinctSenderAmount },
            { _localizer.GetString("MaxDistinctSenderCountWithAmount"), requestBody.MaxDistinctSenderCountWithAmount },
            { _localizer.GetString("MaxDistinctSenderCount"), requestBody.MaxDistinctSenderCount }
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "CommercialPricing"
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateCommercialPricingCommand>(request.Body);

        var commercialPricing = await _repository.GetAll()
                                              .SingleOrDefaultAsync(x => x.Id == requestBody.Id);
        if (commercialPricing is null)
        {
            throw new NotFoundException(nameof(PricingCommercial));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), commercialPricing.Id.ToString() },
            { _localizer.GetString("ActivationDate"), commercialPricing.ActivationDate.ToString() },
            { _localizer.GetString("CommissionRate"), commercialPricing.CommissionRate.ToString() },
            { _localizer.GetString("PricingCommercialType"), commercialPricing.PricingCommercialType.ToString() },
            { _localizer.GetString("CurrencyCode"), commercialPricing.CurrencyCode },
            { _localizer.GetString("MaxDistinctSenderAmount"), commercialPricing.MaxDistinctSenderAmount },
            { _localizer.GetString("MaxDistinctSenderCountWithAmount"), commercialPricing.MaxDistinctSenderCountWithAmount },
            { _localizer.GetString("MaxDistinctSenderCount"), commercialPricing.MaxDistinctSenderCount }
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(commercialPricing, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "CommercialPricing",
            UpdatedFields = updatedFields
        };
    }
}
