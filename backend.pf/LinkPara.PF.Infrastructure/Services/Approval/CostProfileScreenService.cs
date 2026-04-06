using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.CostProfiles;
using LinkPara.PF.Application.Features.CostProfiles.Command.SaveCostProfile;
using LinkPara.PF.Application.Features.VirtualPos;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;
using System.Text;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class CostProfileScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<CostProfile> _repository;
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<Vpos> _vposRepository;

    public CostProfileScreenService(IGenericRepository<CostProfile> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<Vpos> vposRepository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _vposRepository = vposRepository;
    }
    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var costProfileId))
        {
            throw new InvalidCastException();
        }

        var entity = await _repository.GetAll()
            .Include(c => c.CostProfileItems
                .OrderByDescending(b => b.CardTransactionType)
                .ThenByDescending(b=> b.ProfileCardType)
                .ThenBy(b=> b.InstallmentNumberEnd))       
            .FirstOrDefaultAsync(c => c.Id == costProfileId);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), costProfileId);
        }

        var vpos = await _vposRepository.GetAll().Include(v => v.AcquireBank)
            .ThenInclude(a => a.Bank).FirstOrDefaultAsync(v => v.Id == entity.VposId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(entity.ActivationDate.ToString()).Value},
            { _localizer.GetString("PointCommission").Value, _localizer.GetString(entity.PointCommission.ToString()).Value},
            { _localizer.GetString("ServiceCommission").Value, _localizer.GetString(entity.ServiceCommission.ToString()).Value},
            { _localizer.GetString("ProfileStatus").Value, _localizer.GetString(entity.ProfileStatus.ToString()).Value},
            { _localizer.GetString("RecordStatus").Value, _localizer.GetString(entity.RecordStatus.ToString()).Value},
            { _localizer.GetString("VposName").Value, _localizer.GetString(vpos.Name).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(vpos.AcquireBank?.Bank?.Name).Value},
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateCostProfileRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "VposId"))
        {
            updatedFields.TryGetValue("VposId", out Dictionary<string, object> dic);

            var newVpos = await _vposRepository.GetByIdAsync((string)dic.GetValueOrDefault("NewValue"));
            updatedFields.Remove("VposId");
            updatedFields.Add(_localizer.GetString("VposName").Value, newVpos.Name);
        }

        List<string> removedKeys = new List<string>();

        var keys = updatedFields.Keys.ToList();

        foreach (var key in keys)
        {
            var split = key.Split(' ');

            var localizedKey = split.Length >= 3 ? string.Join(" ", split.Take(3)) : "";
            
            if (localizedKey.Contains(_localizer.GetString("CostProfileItems").Value))
            {
                int.TryParse(split[3], out var index);

                var costProfileItem = entity.CostProfileItems[index];

                updatedFields.TryGetValue(key, out Dictionary<string, object> dic);

                removedKeys.Add(key);

                var newKey = "";
                if (costProfileItem.InstallmentNumber == costProfileItem.InstallmentNumberEnd)
                {
                    newKey = costProfileItem.InstallmentNumber.ToString() + " "
                                    + _localizer.GetString("Installment").Value + " ";
                }
                else
                {
                    newKey = costProfileItem.InstallmentNumber.ToString() + "-" +
                                   costProfileItem.InstallmentNumberEnd.ToString() + " "
                                   + _localizer.GetString("Installment").Value + " ";
                }

                if (costProfileItem.CardTransactionType == CardTransactionType.NotOnUs)
                {
                    newKey = string.Concat(newKey, _localizer.GetString("NotOnUs").Value);
                }
                
                newKey = newKey.Insert(newKey.Length, 
                                " " + _localizer.GetString(costProfileItem.ProfileCardType.ToString()).Value 
                                + " - " + _localizer.GetString(string.Join(" ", split.Skip(4))).Value);
                updatedFields.Add(newKey, dic);
            }
        }

        foreach (var key in removedKeys)
        {
            updatedFields.Remove(key);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveCostProfileCommand>(request.Body);


        var vpos = await _vposRepository.GetAll().Include(v => v.AcquireBank)
            .ThenInclude(a => a.Bank).FirstOrDefaultAsync(v => v.Id == requestBody.VposId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(requestBody.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(requestBody.ActivationDate.ToString()).Value},
            { _localizer.GetString("PointCommission").Value, _localizer.GetString(requestBody.PointCommission.ToString()).Value},
            { _localizer.GetString("ServiceCommission").Value, _localizer.GetString(requestBody.ServiceCommission.ToString()).Value},
            { _localizer.GetString("PerTransactionFee").Value, _localizer.GetString(requestBody.PointCommission.ToString()).Value},
            { _localizer.GetString("PricingProfileItems").Value, _localizer.GetString(CostProfileItemsToString(requestBody.CostProfileItems)).Value},
            { _localizer.GetString("VposName").Value, _localizer.GetString(vpos.Name).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(vpos.AcquireBank?.Bank?.Name).Value},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    private string CostProfileItemsToString(List<CostProfileItemDto> costProfileItems)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var costProfileItem in costProfileItems)
        {
            sb.AppendLine($"{_localizer.GetString("ProfileCardType").Value} : {_localizer.GetString(costProfileItem.ProfileCardType.ToString()).Value}, " +
                $" {_localizer.GetString("InstallmentNumber").Value} : {costProfileItem.InstallmentNumber}, " +
                $"{_localizer.GetString("InstallmentNumberEnd").Value} : {costProfileItem.InstallmentNumberEnd}, " +
                $"{_localizer.GetString("CommissionRate").Value} : {costProfileItem.CommissionRate}, " +
                $"{_localizer.GetString("BlockedDayNumber").Value} : {costProfileItem.BlockedDayNumber}, " +
                $"{_localizer.GetString("IsActive").Value} : {costProfileItem.IsActive} <br>");

        }
        return sb.ToString();
    }
}
