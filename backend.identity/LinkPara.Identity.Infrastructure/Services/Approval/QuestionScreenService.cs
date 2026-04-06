using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.SecurityQuestions.Commands.CreateSecurityQuestion;
using LinkPara.Identity.Application.Features.SecurityQuestions.Commands.UpdateSecurityQuestion;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Web;

namespace LinkPara.Identity.Infrastructure.Services.Approval;

public class QuestionScreenService : IApprovalScreenService
{
    private readonly IStringLocalizer _localizer;
    private readonly IRepository<SecurityQuestion> _repository;

    public QuestionScreenService(IStringLocalizerFactory factory,
        IRepository<SecurityQuestion> repository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.Identity.API");
        _repository = repository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if(!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1],out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var securityQuestion = await _repository.GetAll().SingleOrDefaultAsync(x => x.Id == id);

        if (securityQuestion is null)
        {
            throw new NotFoundException(nameof(SecurityQuestion));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Question").Value, securityQuestion.Question},
            { _localizer.GetString("LanguageCode").Value,  _localizer.GetString(securityQuestion.LanguageCode ).Value}
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Questions"
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreateSecurityQuestionCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Question").Value, requestBody.Question},
            { _localizer.GetString("LanguageCode").Value,  _localizer.GetString(requestBody.LanguageCode ).Value}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Questions"
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateSecurityQuestionCommand>(request.Body);
        
        var securityQuestion = await _repository.GetAll().SingleOrDefaultAsync(x => x.Id == requestBody.Id
                                                                                && x.RecordStatus == RecordStatus.Active);

        if (securityQuestion is null)
        {
            throw new NotFoundException(nameof(SecurityQuestion));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Question").Value, securityQuestion.Question},
            { _localizer.GetString("LanguageCode").Value,  _localizer.GetString(securityQuestion.LanguageCode ).Value}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(securityQuestion, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Questions",
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
