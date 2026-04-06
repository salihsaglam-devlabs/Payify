using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.BusinessParameter.Application.Commons.Interfaces;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.SaveParameter;
using LinkPara.BusinessParameter.Application.Features.Parameters.Command.UpdateParameter;
using LinkPara.BusinessParameter.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.BusinessParameter.Infrastructure.Services.Approval;

public class ParameterScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Parameter> _repository;
    private readonly IGenericRepository<ParameterTemplateValue> _parameterTemplateValueRepository;
    private readonly IStringLocalizer _localizer;

    public ParameterScreenService(IGenericRepository<Parameter> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<ParameterTemplateValue> parameterTemplateValueRepository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.BusinessParameter.API");
        _repository = repository;
        _parameterTemplateValueRepository = parameterTemplateValueRepository;
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
            throw new NotFoundException(nameof(Parameter), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, entity.GroupCode},
            { _localizer.GetString("ParameterCode").Value, entity.ParameterCode},
            { _localizer.GetString("ParameterValue").Value, entity.ParameterValue}
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
        var requestBody = JsonConvert.DeserializeObject<SaveParameterCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, requestBody.GroupCode},
            { _localizer.GetString("ParameterCode").Value, requestBody.ParameterCode},
            { _localizer.GetString("ParameterValue").Value, requestBody.ParameterValue}
        };

        if (requestBody.ParameterTemplateValueList.Any())
        {
            foreach (var item in requestBody.ParameterTemplateValueList)
            {
                data.Add($"{_localizer.GetString("TemplateCode").Value}:{item.TemplateCode}", item.TemplateValue);
            }
        }

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateParameterCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id &&
                                                                          x.RecordStatus == RecordStatus.Active);
        if (entity is null)
        {
            throw new NotFoundException(nameof(ParameterGroup), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("GroupCode").Value, entity.GroupCode},
            { _localizer.GetString("ParameterCode").Value, entity.ParameterCode},
            { _localizer.GetString("ParameterValue").Value, entity.ParameterValue}
        };


        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        foreach (var item in requestBody.ParameterTemplateValueList)
        {
            var parameterTemplateValue = await _parameterTemplateValueRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Id == item.Id && b.RecordStatus == RecordStatus.Active);

            if (parameterTemplateValue is null)
            {
                throw new NotFoundException(nameof(ParameterTemplateValue), requestBody.Id);
            }
            

            if (parameterTemplateValue.TemplateValue != item.TemplateValue)
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", parameterTemplateValue.TemplateValue },
                        {"NewValue", item.TemplateValue }
                    };
                updatedFields.Add($"{_localizer.GetString("TemplateCode").Value}:{parameterTemplateValue.TemplateCode}", updatedField);
            }
            
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
