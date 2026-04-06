using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Identity.Application.Common.Interfaces;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.CreateDocument;
using LinkPara.Identity.Application.Features.AgreementDocuments.Commands.UpdateDocument;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Identity.Infrastructure.Services.Approval;

public class AgreementDocumentScreenService : IApprovalScreenService
{

    private readonly IStringLocalizer _localizer;
    private readonly IRepository<AgreementDocument> _repository;

    public AgreementDocumentScreenService(IStringLocalizerFactory factory,
        IRepository<AgreementDocument> repository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.Identity.API");
        _repository = repository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll()
            .Include(x => x.Agreements)
            .Where(x => x.Id == id)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            throw new NotFoundException(nameof(AgreementDocument), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("AgreementDocumentName").Value, entity.Name}
        };

        var trDocumentVersion = entity.Agreements.FirstOrDefault(x => x.LanguageCode == "tr");
        var enDocumentVersion = entity.Agreements.FirstOrDefault(x => x.LanguageCode == "en");

        if (trDocumentVersion is not null)
        {
            data.Add("TrAgreementDocumentVersionTitle", trDocumentVersion.Title);
        }

        if (enDocumentVersion is not null)
        {
            data.Add("EnAgreementDocumentVersionTitle", enDocumentVersion.Title);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "AgreementDocuments"
        };
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CreateDocumentCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("AgreementDocumentName").Value, requestBody.Name}
        };

        var trDocumentVersion = requestBody.Agreements.FirstOrDefault(x => x.LanguageCode == "tr");
        var enDocumentVersion = requestBody.Agreements.FirstOrDefault(x => x.LanguageCode == "en");

        if (trDocumentVersion is not null)
        {
            data.Add("TrAgreementDocumentVersionTitle", trDocumentVersion.Title);
        }

        if (enDocumentVersion is not null)
        {
            data.Add("EnAgreementDocumentVersionTitle", enDocumentVersion.Title);
        }

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "AgreementDocuments"
        });

    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateAgreementDocumentCommand>(request.Body);

        var entity = await _repository.GetAll()
            .Include(x => x.Agreements)
            .Where(x => x.Id == requestBody.Id)
            .FirstOrDefaultAsync();

        if (entity is null)
        {
            throw new NotFoundException(nameof(AgreementDocument), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("AgreementDocumentName").Value, entity.Name}
        };
        var updatedFields = new Dictionary<string, object>();

        if (entity.Name != requestBody.Name)
        {
            var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", entity.Name },
                        {"NewValue", requestBody.Name }
                    };
            updatedFields.Add(_localizer.GetString("AgreementDocumentName").Value, updatedField);
        }

        var trDocumentVersion = requestBody.Agreements.FirstOrDefault(x => x.LanguageCode == "tr");
        var enDocumentVersion = requestBody.Agreements.FirstOrDefault(x => x.LanguageCode == "en");

        var dbTrDocumentVersion = entity.Agreements.FirstOrDefault(x => x.LanguageCode == "tr");
        var dbEnDocumentVersion = entity.Agreements.FirstOrDefault(x => x.LanguageCode == "en");

        if (trDocumentVersion is not null)
        {
            var trversionUpdatedFields = UpdatedFieldsHelper.GetUpdatedFields(dbTrDocumentVersion, trDocumentVersion, _localizer);
            
            if (trversionUpdatedFields.Any(x => x.Key == "Content"))
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", "" },
                        {"NewValue", _localizer.GetString("ContentIsChanged").Value }
                    };
                updatedFields.Add(_localizer.GetString("TrDocumentVersionContent").Value, updatedField);
            }

            if (trversionUpdatedFields.Any(x => x.Key == "Title"))
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", dbTrDocumentVersion?.Title },
                        {"NewValue", trDocumentVersion?.Title }
                    };
                updatedFields.Add(_localizer.GetString("TrDocumentVersionTitle").Value, updatedField);
            }

        }

        if (enDocumentVersion is not null)
        {
            var enversionUpdatedFields = UpdatedFieldsHelper.GetUpdatedFields(dbEnDocumentVersion, enDocumentVersion, _localizer);

            if (enversionUpdatedFields.Any(x => x.Key == "Content"))
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", "" },
                        {"NewValue",_localizer.GetString("ContentIsChanged").Value  }
                    };
                updatedFields.Add(_localizer.GetString("EnDocumentVersionContent").Value, updatedField);
            }

            if (enversionUpdatedFields.Any(x => x.Key == "Title"))
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", dbEnDocumentVersion?.Title },
                        {"NewValue", enDocumentVersion?.Title }
                    };
                updatedFields.Add(_localizer.GetString("EnDocumentVersionTitle").Value, updatedField);
            }
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "AgreementDocuments",
            UpdatedFields = updatedFields
        };
    }
}
