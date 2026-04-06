using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantLimitScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantLimit> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;
    public MerchantLimitScreenService(IGenericRepository<MerchantLimit> repository,
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
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantLimitCommand>(request.Body);

        var merchant = await _merchantRepository.GetByIdAsync(requestBody.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("TransactionLimitType").Value, _localizer.GetString(requestBody.TransactionLimitType.ToString()).Value},
            { _localizer.GetString("TransactionPeriod").Value, _localizer.GetString(requestBody.Period.ToString()).Value},
            { _localizer.GetString("LimitType").Value, _localizer.GetString(requestBody.LimitType.ToString()).Value},
            { _localizer.GetString("MaxPiece").Value, requestBody.MaxPiece?.ToString()},
            { _localizer.GetString("MaxAmount").Value, requestBody.MaxAmount?.ToString()},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateMerchantLimitCommand>(request.Body);

        var entity = await _repository.GetByIdAsync(requestBody.Id);

        var merchant = await _merchantRepository.GetByIdAsync(entity.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("TransactionLimitType").Value, _localizer.GetString(requestBody.TransactionLimitType.ToString()).Value},
            { _localizer.GetString("TransactionPeriod").Value, _localizer.GetString(requestBody.Period.ToString()).Value},
            { _localizer.GetString("LimitType").Value, _localizer.GetString(requestBody.LimitType.ToString()).Value},
            { _localizer.GetString("MaxPiece").Value, entity.MaxPiece?.ToString()},
            { _localizer.GetString("MaxAmount").Value, entity.MaxAmount?.ToString()},
            { _localizer.GetString("RecordStatus").Value, _localizer.GetString(entity.RecordStatus.ToString()).Value},
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "MerchantId"))
        {
            var newMerchant = await _merchantRepository.GetByIdAsync(requestBody.MerchantId);

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
