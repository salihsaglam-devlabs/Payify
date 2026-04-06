using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;
using LinkPara.PF.Application.Features.Merchants.Command.SaveAnnulment;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;
using System.Web;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Merchant> _repository;
    private readonly IStringLocalizer _localizer;

    public MerchantScreenService(IGenericRepository<Merchant> repository,
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
            throw new NotFoundException(nameof(Merchant), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("CreateDate").Value,  _localizer.GetString(entity.CreateDate.ToString("dd.MM.yyyy")).Value},
            { _localizer.GetString("MerchantStatus").Value,  _localizer.GetString(entity.MerchantStatus.ToString()).Value},
            { _localizer.GetString("RejectReason").Value,  _localizer.GetString(entity.RejectReason.ToString()).Value},
            { _localizer.GetString("MonthlyTurnover").Value,  _localizer.GetString(entity.MonthlyTurnover.ToString()).Value},
            { _localizer.GetString("WebSiteUrl").Value,  _localizer.GetString(entity.WebSiteUrl.ToString()).Value}
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
        if (!Guid.TryParse(url.LastOrDefault(), out var id))
        {
            throw new InvalidCastException();
        }

        var merchant = await GetMerchantByIdAsync(id);
        
        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name },
            { _localizer.GetString("CreateDate").Value,  merchant.CreateDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("MerchantStatus").Value,  merchant.MerchantStatus.ToString()},
            { _localizer.GetString("RejectReason").Value,  merchant.RejectReason},
            { _localizer.GetString("MonthlyTurnover").Value,  merchant.MonthlyTurnover.ToString(CultureInfo.InvariantCulture)},
            { _localizer.GetString("WebSiteUrl").Value,  merchant.WebSiteUrl}
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateMerchantRequest>>(HttpUtility.UrlDecode(request.Body), settings);
        
        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveAnnulmentCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id
                                                                    && x.RecordStatus == RecordStatus.Active);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Merchant), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("AnnulmentCode").Value, _localizer.GetString(requestBody.AnnulmentCode).Value},
            { _localizer.GetString("AnnulmentCodeExplanation").Value, string.IsNullOrEmpty(requestBody.AnnulmentCodeDescription) ? string.Empty : _localizer.GetString(requestBody.AnnulmentCodeDescription).Value},
            { _localizer.GetString("IsCancelCode").Value,  _localizer.GetString(requestBody.IsCancelCode.ToString()).Value},
            { _localizer.GetString("AnnulmentDescription").Value,  _localizer.GetString(requestBody.AnnulmentDescription).Value}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var split = request.Url.Split('/');
        if (split.Length > 1 && split[split.Length -1] == "approve")
        {
            var requestBody = JsonConvert.DeserializeObject<ApproveMerchantCommand>(request.Body);

            var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.MerchantId);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Merchant), requestBody.MerchantId);
            }

            var data = new Dictionary<string, object>
            {
                { _localizer.GetString("MerchantName").Value, _localizer.GetString(entity.Name).Value},
                { _localizer.GetString("CreateDate").Value,  _localizer.GetString(entity.CreateDate.ToString("dd.MM.yyyy")).Value},
                { _localizer.GetString("MerchantStatus").Value,  _localizer.GetString(entity.MerchantStatus.ToString()).Value},
                { _localizer.GetString("MonthlyTurnover").Value,  _localizer.GetString(entity.MonthlyTurnover.ToString()).Value},
                { _localizer.GetString("WebSiteUrl").Value,  _localizer.GetString(entity.WebSiteUrl.ToString()).Value}
            };

            var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

            return new ApprovalScreenResponse
            {
                DisplayScreenFields = data,
                Resource = request.Resource,
                UpdatedFields = updatedFields
            };
        }
        else if (split[split.Length - 1] == "")
        {
            var requestBody = JsonConvert.DeserializeObject<UpdateMerchantRequest>(request.Body);

            var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id);
            if (entity is null)
            {
                throw new NotFoundException(nameof(Merchant), requestBody.Id);
            }

            var data = new Dictionary<string, object>
            {
                { _localizer.GetString("MerchantName").Value, _localizer.GetString(entity.Name).Value},
                { _localizer.GetString("MerchantNumber").Value, _localizer.GetString(entity.Number).Value},
                { _localizer.GetString("MerchantStatus").Value,  _localizer.GetString(entity.MerchantStatus.ToString()).Value}
            };

            var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

            return new ApprovalScreenResponse
            {
                DisplayScreenFields = data,
                Resource = request.Resource,
                UpdatedFields = updatedFields
            };
        }
        else
        {
            throw new NotImplementedException();
        }
    }
    
    private JsonPatchDocument<UpdateMerchantRequest> ReplacePatch(JsonPatchDocument<UpdateMerchantRequest> request)
    {
        foreach (var item in request.Operations)
        {
            var split = item.path.Split('/');
            if (split.Contains("merchantVposList") && split.Contains("recordStatus"))
            {
                continue;
            }

            for (int i = 0; i <= split.Length - 1; i++)
            {
                if (split.Length - 1 == i)
                {
                    item.path = item.path.Replace(split[i], "-");
                }
            }
        }
        return request;
    }
    private async Task<Merchant> GetMerchantByIdAsync(Guid id)
        {
            return await _repository.GetAll()
                .FirstOrDefaultAsync(b => b.Id == id);
        }
    }
