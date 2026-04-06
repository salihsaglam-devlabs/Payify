using DocumentFormat.OpenXml.Spreadsheet;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.SaveMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantBusinessPartnerScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantBusinessPartner> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    public MerchantBusinessPartnerScreenService(IGenericRepository<MerchantBusinessPartner> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _merchantRepository = merchantRepository;
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantBusinessPartnerCommand>(request.Body);

        var merchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == requestBody.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("PartnerName").Value, requestBody.FirstName},
            { _localizer.GetString("PartnerLastName").Value, requestBody.LastName},
            { _localizer.GetString("PartnerEmail").Value, requestBody.Email},
            { _localizer.GetString("PartnerPhone").Value, requestBody.PhoneNumber},
            { _localizer.GetString("PartnerIdentityNumber").Value, requestBody.IdentityNumber},
            { _localizer.GetString("PartnerBirthDate").Value, requestBody.BirthDate.ToString("dd.MM.yyyy")},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateMerchantBusinessPartnerCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(e => e.Id == requestBody.Id);

        var merchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == entity.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("PartnerName").Value, entity.FirstName},
            { _localizer.GetString("PartnerLastName").Value, entity.LastName},
            { _localizer.GetString("PartnerEmail").Value, entity.Email},
            { _localizer.GetString("PartnerPhone").Value, entity.PhoneNumber},
            { _localizer.GetString("PartnerIdentityNumber").Value, entity.IdentityNumber},
            { _localizer.GetString("PartnerBirthDate").Value, entity.BirthDate.ToString("dd.MM.yyyy")},
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "MerchantId"))
        {
            var newMerchant = await _merchantRepository.GetAll().Where(m => m.Id == requestBody.MerchantId).SingleOrDefaultAsync();

            if (newMerchant is not null)
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", merchant.Name },
                        {"NewValue", newMerchant.Name }
                    };
                updatedFields.Add(_localizer.GetString("MerchantName").Value, updatedField);

                var updatedField2 = new Dictionary<string, object>
                    {
                        {"OldValue", merchant.Number },
                        {"NewValue", newMerchant.Number }
                    };
                updatedFields.Add(_localizer.GetString("MerchantNumber").Value, updatedField2);
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
