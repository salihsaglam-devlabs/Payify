using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantUsers.Command.SaveMerchantUser;
using LinkPara.PF.Application.Features.MerchantUsers.Command.UpdateMerchantUser;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantUserScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantUser> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    public MerchantUserScreenService(IGenericRepository<MerchantUser> repository,
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

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantUserCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantUserName").Value, requestBody.Name},
            { _localizer.GetString("MerchantUserLast").Value, requestBody.Surname},
            { _localizer.GetString("MerchantUserBirthDate").Value, requestBody.BirthDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("MerchantUserEmail").Value, requestBody.Email},
            { _localizer.GetString("MerchantUserPhoneNumber").Value, requestBody.MobilePhoneNumber},
            { _localizer.GetString("MerchantUserRoleName").Value, requestBody.RoleName},
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateMerchantUserCommand>(request.Body);

        var entity = await _repository.GetByIdAsync(requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Merchant), requestBody.Id);
        }

        var merchant = await _merchantRepository.GetByIdAsync(entity.MerchantId);
        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), entity.MerchantId);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantUserName").Value, entity.Name},
            { _localizer.GetString("MerchantUserLast").Value, entity.Surname},
            { _localizer.GetString("MerchantUserBirthDate").Value, entity.BirthDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("MerchantUserEmail").Value, entity.Email},
            { _localizer.GetString("MerchantUserPhoneNumber").Value, entity.MobilePhoneNumber.ToString()},
            { _localizer.GetString("MerchantUserRoleName").Value, entity.RoleName},
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
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
