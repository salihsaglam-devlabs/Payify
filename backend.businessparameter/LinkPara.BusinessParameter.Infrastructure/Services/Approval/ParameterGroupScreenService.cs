using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.SaveParameterGroup;
using LinkPara.BusinessParameter.Application.Features.ParameterGroups.Command.UpdateParameterGroup;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.BusinessParameter.Infrastructure.Services.Approval;

public class ParameterGroupScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<ParameterGroup> _repository;
    private readonly IStringLocalizer _localizer;

    public ParameterGroupScreenService(IGenericRepository<ParameterGroup> repository,
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
            throw new NotFoundException(nameof(ParameterGroup), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, entity.GroupCode},
            { _localizer.GetString("Explanation").Value, entity.Explanation},
            { _localizer.GetString("ParameterValueType").Value, _localizer.GetString( entity.ParameterValueType.ToString()).Value}
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
        var requestBody = JsonConvert.DeserializeObject<SaveParameterGroupCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, requestBody.GroupCode},
            { _localizer.GetString("Explanation").Value, requestBody.Explanation}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateParameterGroupCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, requestBody.GroupCode},
            { _localizer.GetString("Explanation").Value, requestBody.Explanation}
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
