using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantCategoryCodeScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Mcc> _repository;
    private readonly IStringLocalizer _localizer;

    public MerchantCategoryCodeScreenService(IGenericRepository<Mcc> repository,
        IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Mcc), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Code").Value, entity.Code },
            { _localizer.GetString("Name").Value, entity.Name },
            { _localizer.GetString("Description").Value,  entity.Description },
            { _localizer.GetString("MaxIndividualInstallmentCount").Value,  entity.MaxIndividualInstallmentCount.ToString() },
            { _localizer.GetString("MaxCorporateInstallmentCount").Value, entity.MaxCorporateInstallmentCount.ToString() }
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var mccId))
        {
            throw new InvalidCastException();
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(b => b.Id == mccId);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Mcc), mccId);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Code").Value, entity.Code },
            { _localizer.GetString("Name").Value, entity.Name },
            { _localizer.GetString("Description").Value,  entity.Description},
            { _localizer.GetString("MaxIndividualInstallmentCount").Value,  entity.MaxIndividualInstallmentCount.ToString() },
            { _localizer.GetString("MaxCorporateInstallmentCount").Value,  entity.MaxCorporateInstallmentCount.ToString() }
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateMccRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveMccCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Code").Value, requestBody.Code},
            { _localizer.GetString("Name").Value, requestBody.Name},
            { _localizer.GetString("Description").Value,  requestBody.Description},
            { _localizer.GetString("MaxIndividualInstallmentCount").Value,  requestBody.MaxIndividualInstallmentCount.ToString()},
            { _localizer.GetString("MaxCorporateInstallmentCount").Value,  requestBody.MaxCorporateInstallmentCount.ToString()}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateMccCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Mcc), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Code").Value, requestBody.Code},
            { _localizer.GetString("Name").Value, requestBody.Name},
            { _localizer.GetString("Description").Value,  requestBody.Description},
            { _localizer.GetString("MaxIndividualInstallmentCount").Value,  requestBody.MaxIndividualInstallmentCount.ToString()},
            { _localizer.GetString("MaxCorporateInstallmentCount").Value,  requestBody.MaxCorporateInstallmentCount.ToString()}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);
        
        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
