using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;
using LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantBlockagesScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantBlockage> _repository;
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IStringLocalizer _localizer;
    public MerchantBlockagesScreenService(IGenericRepository<Merchant> merchantRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<MerchantBlockage> repository,
        IGenericRepository<PostingBalance> postingBalanceRepository)
    {
        _merchantRepository = merchantRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _postingBalanceRepository = postingBalanceRepository;
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
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantBlockageCommand>(request.Body);

        var merchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == requestBody.MerchantId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("TotalAmount").Value, requestBody.TotalAmount.ToString("0.00")},
            { _localizer.GetString("BlockageAmount").Value, requestBody.BlockageAmount.ToString("0.00")},
            { _localizer.GetString("RemainingAmount").Value, requestBody.RemainingAmount.ToString("0.00")},
            { _localizer.GetString("MerchantBlockageStatus").Value, _localizer.GetString(requestBody.MerchantBlockageStatus.ToString()).Value},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        if (request.Url == "/v1/MerchantBlockages")
        {
            var requestBody = JsonConvert.DeserializeObject<UpdateMerchantBlockageCommand>(request.Body);

            var entity = await _repository.GetAll()
                .Include(m => m.Merchant).FirstOrDefaultAsync(m => m.Id == requestBody.Id);

            var data = new Dictionary<string, object>
            {
                { _localizer.GetString("MerchantName").Value, entity.Merchant.Name},
                { _localizer.GetString("MerchantNumber").Value, entity.Merchant.Number},
                { _localizer.GetString("TotalAmount").Value, entity.TotalAmount.ToString("0.00")},
                { _localizer.GetString("BlockageAmount").Value, entity.BlockageAmount.ToString("0.00")},
                { _localizer.GetString("RemainingAmount").Value, entity.RemainingAmount.ToString("0.00")},
                { _localizer.GetString("MerchantBlockageStatus").Value, _localizer.GetString(entity.MerchantBlockageStatus.ToString()).Value},
            };

            var updatedFields = new Dictionary<string, object>
            {
                {
                    _localizer.GetString("TotalAmount").Value, new Dictionary<string, object>
                    {
                        { "OldValue", entity.TotalAmount.ToString("0.00") },
                        { "NewValue", requestBody.TotalAmount.ToString("0.00") }
                    }
                }
            };

            return new ApprovalScreenResponse
            {
                DisplayScreenFields = data,
                Resource = request.Resource,
                UpdatedFields = updatedFields
            };
        }
        else if (request.Url == "/v1/MerchantBlockages/payment-date")
        {
            var requestBody = JsonConvert.DeserializeObject<UpdatePaymentDateCommand>(request.Body);

            var entity = await _repository.GetAll()
                .Include(m => m.Merchant).FirstOrDefaultAsync(m => m.Id == requestBody.MerchantBlockageId);

            var postingBalance = await _postingBalanceRepository.GetAll().FirstOrDefaultAsync(p => p.Id == requestBody.PostBalanceId);

            var data = new Dictionary<string, object>
            {
                { _localizer.GetString("MerchantName").Value, entity.Merchant.Name},
                { _localizer.GetString("MerchantNumber").Value, entity.Merchant.Number},
                { _localizer.GetString("PaymentDate").Value, postingBalance.PaymentDate.ToString("dd.MM.yyyy")},
                { _localizer.GetString("TotalAmount").Value, entity.TotalAmount.ToString("0.00")},
                { _localizer.GetString("BlockageAmount").Value, entity.BlockageAmount.ToString("0.00")},
                { _localizer.GetString("RemainingAmount").Value, entity.RemainingAmount.ToString("0.00")},
                { _localizer.GetString("MerchantBlockageStatus").Value, _localizer.GetString(entity.MerchantBlockageStatus.ToString()).Value},
            };

            var updatedFields = new Dictionary<string, object>();

            var updatedField = new Dictionary<string, object>
            {
                {"OldValue", postingBalance.PaymentDate.ToString("dd.MM.yyyy hh.mm.ss")},
                {"NewValue", requestBody.PaymentDate.ToString("dd.MM.yyyy hh.mm.ss") }
            };
            updatedFields.Add(_localizer.GetString("PaymentDate").Value, updatedField);

            var newMerchantBlockage = new MerchantBlockage();
            newMerchantBlockage.RemainingAmount = entity.RemainingAmount + postingBalance.TotalAmount;
            newMerchantBlockage.BlockageAmount = entity.BlockageAmount - postingBalance.TotalAmount;
            newMerchantBlockage.MerchantBlockageStatus = entity.RemainingAmount > 0 ? MerchantBlockageStatus.Incomplete : MerchantBlockageStatus.Complete;

            var updatedField2 = new Dictionary<string, object>
            {
                {"OldValue", entity.RemainingAmount.ToString("0.00")},
                {"NewValue", newMerchantBlockage.RemainingAmount.ToString("0.00")}
            };
            updatedFields.Add(_localizer.GetString("RemainingAmount").Value, updatedField2);

            var updatedField3 = new Dictionary<string, object>
            {
                {"OldValue", entity.BlockageAmount.ToString("0.00")},
                {"NewValue", newMerchantBlockage.BlockageAmount.ToString("0.00")}
            };
            updatedFields.Add(_localizer.GetString("BlockageAmount").Value, updatedField3);

            var updatedField4 = new Dictionary<string, object>
            {
                {"OldValue", _localizer.GetString(entity.MerchantBlockageStatus.ToString()).Value},
                {"NewValue", _localizer.GetString(newMerchantBlockage.MerchantBlockageStatus.ToString()).Value}
            };
            updatedFields.Add(_localizer.GetString("MerchantBlockageStatus").Value, updatedField4);

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
}
