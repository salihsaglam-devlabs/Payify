using DocumentFormat.OpenXml.Vml.Office;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.PricingProfiles;
using LinkPara.PF.Application.Features.AcquireBanks.Command.DeleteAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.SaveAcquireBank;
using LinkPara.PF.Application.Features.AcquireBanks.Command.UpdateAcquireBank;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class AcquireBankScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IStringLocalizer _localizer;

    public AcquireBankScreenService(IGenericRepository<Bank> bankRepository,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _bankRepository = bankRepository;
        _acquireBankRepository = acquireBankRepository;
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveAcquireBankCommand>(request.Body);

        var bank = await _bankRepository.GetAll().Where(s => s.Code == requestBody.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("EndOfDay").Value, FormatTime(requestBody.EndOfDayHour, requestBody.EndOfDayMinute) },
            { _localizer.GetString("AcceptAmex").Value, _localizer.GetString(requestBody.AcceptAmex.ToString()).Value },
            { _localizer.GetString("CardNetwork").Value, _localizer.GetString(requestBody.CardNetwork.ToString()).Value},
            { _localizer.GetString("HasSubmerchantIntegration").Value, _localizer.GetString(requestBody.HasSubmerchantIntegration.ToString()).Value },
            { _localizer.GetString("RestrictOwnCardNotOnUs").Value, _localizer.GetString(requestBody.RestrictOwnCardNotOnUs.ToString()).Value },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateAcquireBankCommand>(request.Body);

        var acquireBank = await _acquireBankRepository.GetAll().Where(s => s.Id == requestBody.Id).SingleOrDefaultAsync();

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(acquireBank, requestBody, _localizer);

        var bank = await _bankRepository.GetAll().Where(s => s.Code == acquireBank.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("EndOfDay").Value, FormatTime(acquireBank.EndOfDayHour,acquireBank.EndOfDayMinute)},
            { _localizer.GetString("CardNetwork").Value, _localizer.GetString(acquireBank.CardNetwork.ToString()).Value},
            { _localizer.GetString("AcceptAmex").Value, _localizer.GetString(acquireBank.AcceptAmex.ToString()).Value },
            { _localizer.GetString("HasSubmerchantIntegration").Value, _localizer.GetString(acquireBank.HasSubmerchantIntegration.ToString()).Value },
            { _localizer.GetString("RestrictOwnCardNotOnUs").Value, _localizer.GetString(acquireBank.RestrictOwnCardNotOnUs.ToString()).Value },
        };

        if (updatedFields.Any(x => x.Key == "BankCode"))
        {
            updatedFields.Remove("BankCode");
            var newBank = await _bankRepository.GetAll().Where(s => s.Code == requestBody.BankCode).SingleOrDefaultAsync();

            if(newBank is not null)
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", bank.Name },
                        {"NewValue", newBank.Name }
                    };
                updatedFields.Add(_localizer.GetString("BankName").Value, updatedField);
            }           
        }

        if (updatedFields.Any(x => x.Key == "EndOfDayMinute") || updatedFields.Any(x => x.Key == "EndOfDayHour"))
        {
            updatedFields.Remove("EndOfDayMinute");
            updatedFields.Remove("EndOfDayHour");
            var updatedField = new Dictionary<string, object>
            {
                {"OldValue", FormatTime(acquireBank.EndOfDayHour, acquireBank.EndOfDayMinute) },
                {"NewValue", FormatTime(requestBody.EndOfDayHour, requestBody.EndOfDayMinute) }
            };

            updatedFields.Add(_localizer.GetString("EndOfDay").Value, updatedField);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
    private string FormatTime(int hour, int minute)
    {
        var time = $"{(hour.ToString().Length == 1 ? $"0{hour}" : hour.ToString())}:{(minute.ToString().Length == 1 ? $"0{minute}" : minute.ToString())}";
        return time;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var acquireBank = await _acquireBankRepository.GetAll().Where(s => s.Id == id).SingleOrDefaultAsync();

        var bank = await _bankRepository.GetAll().Where(s => s.Code == acquireBank.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("EndOfDay").Value, FormatTime(acquireBank.EndOfDayHour,acquireBank.EndOfDayMinute) },
            { _localizer.GetString("CardNetwork").Value, _localizer.GetString(acquireBank.CardNetwork.ToString()).Value},
            { _localizer.GetString("AcceptAmex").Value, _localizer.GetString(acquireBank.AcceptAmex.ToString()).Value },
            { _localizer.GetString("HasSubmerchantIntegration").Value, _localizer.GetString(acquireBank.HasSubmerchantIntegration.ToString()).Value },
            { _localizer.GetString("RestrictOwnCardNotOnUs").Value, _localizer.GetString(acquireBank.RestrictOwnCardNotOnUs.ToString()).Value },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var id))
        {
            throw new InvalidCastException();
        }

        var entity = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(b => b.Id == id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), id);
        }

        var bank = await _bankRepository.GetAll().Where(s => s.Code == entity.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("EndOfDay").Value, FormatTime(entity.EndOfDayHour,entity.EndOfDayMinute) },
            { _localizer.GetString("CardNetwork").Value, _localizer.GetString(entity.CardNetwork.ToString()).Value},
            { _localizer.GetString("AcceptAmex").Value, _localizer.GetString(entity.AcceptAmex.ToString()).Value },
            { _localizer.GetString("HasSubmerchantIntegration").Value, _localizer.GetString(entity.HasSubmerchantIntegration.ToString()).Value },
            { _localizer.GetString("RestrictOwnCardNotOnUs").Value, _localizer.GetString(entity.RestrictOwnCardNotOnUs.ToString()).Value },
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdatePricingProfileRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}