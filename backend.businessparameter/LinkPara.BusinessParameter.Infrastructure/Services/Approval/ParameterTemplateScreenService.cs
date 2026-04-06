
using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.SaveParameterTemplate;
using LinkPara.BusinessParameter.Application.Features.ParameterTemplates.Command.UpdateParameterTemplate;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.BusinessParameter.Infrastructure.Services.Approval;

public class ParameterTemplateScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<ParameterTemplate> _repository;
    private readonly IStringLocalizer _localizer;

    public ParameterTemplateScreenService(IGenericRepository<ParameterTemplate> repository,
        IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.BusinessParameter.API");
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
            throw new NotFoundException(nameof(ParameterTemplate), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, entity.GroupCode},
            { _localizer.GetString("TemplateCode").Value, entity.TemplateCode},
            { _localizer.GetString("DataType").Value, _localizer.GetString( entity.DataType.ToString()).Value},
            { _localizer.GetString("DataLength").Value, entity.DataLength},
            { _localizer.GetString("Explanation").Value, entity.Explanation},
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
        var requestBody = JsonConvert.DeserializeObject<SaveParameterTemplateCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, requestBody.GroupCode},
            { _localizer.GetString("TemplateCode").Value, requestBody.TemplateCode},
            { _localizer.GetString("DataType").Value, _localizer.GetString( requestBody.DataType.ToString()).Value},
            { _localizer.GetString("DataLength").Value, requestBody.DataLength},
            { _localizer.GetString("Explanation").Value, requestBody.Explanation},
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateParameterTemplateCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id &&
                                                                          x.RecordStatus == RecordStatus.Active);
        if (entity is null)
        {
            throw new NotFoundException(nameof(ParameterTemplate), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, entity.GroupCode},
            { _localizer.GetString("TemplateCode").Value, entity.TemplateCode},
            { _localizer.GetString("DataType").Value, _localizer.GetString( entity.DataType.ToString()).Value},
            { _localizer.GetString("DataLength").Value, entity.DataLength},
            { _localizer.GetString("Explanation").Value, entity.Explanation},
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
