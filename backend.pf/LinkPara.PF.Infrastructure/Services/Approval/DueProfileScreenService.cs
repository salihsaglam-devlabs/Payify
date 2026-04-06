using DocumentFormat.OpenXml.VariantTypes;
using DocumentFormat.OpenXml.Vml.Office;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.DueProfiles.Command.CreateDueProfile;
using LinkPara.PF.Application.Features.DueProfiles.Command.UpdateDueProfile;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class DueProfileScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<DueProfile> _repository;
    private readonly IStringLocalizer _localizer;
    private readonly ICurrencyService _currencyService;

    public DueProfileScreenService(IGenericRepository<DueProfile> repository,
        IStringLocalizerFactory factory,
        ICurrencyService currencyService)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _currencyService = currencyService;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (!Guid.TryParse(queryParameters[2], out Guid id))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), id);
        }

        var currency = await _currencyService.GetByNumberAsync(entity.Currency);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("DueProfileTitle").Value, _localizer.GetString(entity.Title).Value},
            { _localizer.GetString("DueProfileType").Value, _localizer.GetString(entity.DueType.ToString()).Value},
            { _localizer.GetString("DueAmount").Value, entity.Amount.ToString()},
            { _localizer.GetString("Currency").Value, currency.Code},
            { _localizer.GetString("IsDefault").Value, _localizer.GetString(entity.IsDefault.ToString()).Value},
            { _localizer.GetString("OccurenceInterval").Value, _localizer.GetString(entity.OccurenceInterval.ToString()).Value}
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

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreateDueProfileCommand>(request.Body);

        var currency = await _currencyService.GetByNumberAsync(requestBody.Currency);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("DueProfileTitle").Value, requestBody.Title},
            { _localizer.GetString("DueProfileType").Value, _localizer.GetString(requestBody.DueType.ToString()).Value},
            { _localizer.GetString("DueAmount").Value, requestBody.Amount.ToString("0.00")},
            { _localizer.GetString("Currency").Value, currency.Code},
            { _localizer.GetString("OccurenceInterval").Value, _localizer.GetString(requestBody.OccurenceInterval.ToString()).Value}
        };

        var response = new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };

        return response;
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateDueProfileCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), requestBody.Id);
        }

        var currency = await _currencyService.GetByNumberAsync(requestBody.Currency);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("DueProfileTitle").Value, requestBody.Title},
            { _localizer.GetString("DueAmount").Value, requestBody.Amount.ToString("0.00")},
            { _localizer.GetString("Currency").Value, currency.Code},
            { _localizer.GetString("OccurenceInterval").Value, requestBody.OccurenceInterval.ToString()}
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
