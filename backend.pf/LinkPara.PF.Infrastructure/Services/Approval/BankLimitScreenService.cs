using DocumentFormat.OpenXml.Spreadsheet;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.BankLimits.Command.SaveBankLimit;
using LinkPara.PF.Application.Features.BankLimits.Command.UpdateBankLimit;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class BankLimitScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<BankLimit> _bankLimitRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IStringLocalizer _localizer;
    public BankLimitScreenService(IGenericRepository<BankLimit> bankLimitRepository, 
        IGenericRepository<AcquireBank> acquireBankRepository, 
        IGenericRepository<Bank> bankRepository,
        IStringLocalizerFactory factory)
    {
        _bankLimitRepository = bankLimitRepository;
        _acquireBankRepository = acquireBankRepository;
        _bankRepository = bankRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var bankLimit = await _bankLimitRepository.GetAll().FirstOrDefaultAsync(b => b.Id == id);

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(a => a.Id == bankLimit.AcquireBankId);

        var bank = await _bankRepository.GetAll().Where(s => s.Code == acquireBank.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("BankLimitType").Value, _localizer.GetString(bankLimit.BankLimitType.ToString()).Value},
            { _localizer.GetString("LastValidDate").Value, bankLimit.LastValidDate.ToString("dd.MM.yy")},
            { _localizer.GetString("MonthlyLimitAmount").Value, bankLimit.MonthlyLimitAmount.ToString()},
            { _localizer.GetString("MarginRatio").Value, bankLimit.MarginRatio.ToString()},
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

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveBankLimitCommand>(request.Body);

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(a => a.Id == requestBody.AcquireBankId);

        var bank = await _bankRepository.GetAll().Where(s => s.Code == acquireBank.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("BankLimitType").Value, _localizer.GetString(requestBody.BankLimitType.ToString()).Value},
            { _localizer.GetString("LastValidDate").Value, requestBody.LastValidDate.ToString("dd.MM.yy")},
            { _localizer.GetString("MonthlyLimitAmount").Value, requestBody.MonthlyLimitAmount.ToString()},
            { _localizer.GetString("MarginRatio").Value, requestBody.MarginRatio.ToString()},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateBankLimitCommand>(request.Body);

        var bankLimit = await _bankLimitRepository.GetAll().FirstOrDefaultAsync(b => b.Id == requestBody.Id);

        var acquireBank = await _acquireBankRepository.GetAll().Where(s => s.Id == bankLimit.AcquireBankId).SingleOrDefaultAsync();

        var bank = await _bankRepository.GetAll().Where(s => s.Code == acquireBank.BankCode).SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank.Name},
            { _localizer.GetString("BankLimitType").Value, _localizer.GetString(bankLimit.BankLimitType.ToString()).Value},
            { _localizer.GetString("LastValidDate").Value, bankLimit.LastValidDate.ToString("dd.MM.yy")},
            { _localizer.GetString("MonthlyLimitAmount").Value, bankLimit.MonthlyLimitAmount.ToString()},
            { _localizer.GetString("MarginRatio").Value, bankLimit.MarginRatio.ToString()},
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(bankLimit, requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "AcquireBankId"))
        {
            var newAcquireBank = await _acquireBankRepository.GetAll().Where(s => s.Id == requestBody.AcquireBankId).SingleOrDefaultAsync();
            var newBank = await _bankRepository.GetAll().Where(s => s.Code == newAcquireBank.BankCode).SingleOrDefaultAsync();

            if (newBank is not null)
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", bank.Name },
                        {"NewValue", newBank.Name }
                    };
                updatedFields.Add(_localizer.GetString("BankName").Value, updatedField);
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
