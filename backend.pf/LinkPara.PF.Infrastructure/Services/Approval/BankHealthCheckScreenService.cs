using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.BankHealthChecks.Command.UpdateBankHealthCheck;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class BankHealthCheckScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<BankHealthCheck> _bankHealthRepository;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IStringLocalizer _localizer;
    public BankHealthCheckScreenService(IGenericRepository<BankHealthCheck> bankHealthRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IGenericRepository<Bank> bankRepository)
    {
        _bankHealthRepository = bankHealthRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _acquireBankRepository = acquireBankRepository;
        _bankRepository = bankRepository;
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
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateBankHealthCheckCommand>(request.Body);

        var bankHealthCheck = await _bankHealthRepository.GetAll().FirstOrDefaultAsync(s => s.Id == requestBody.Id);

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(a => a.Id == bankHealthCheck.AcquireBankId);

        var bank = _bankRepository.GetAll().FirstOrDefault(b => b.Code == acquireBank.BankCode);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BankName").Value, bank?.Name},
            { _localizer.GetString("IsHealthCheckAllowed").Value, _localizer.GetString(bankHealthCheck.IsHealthCheckAllowed.ToString()).Value},
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(bankHealthCheck, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
