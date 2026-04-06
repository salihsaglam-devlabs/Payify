using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Features.MerchantDues.Command.SaveMerchantDue;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantDueScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantDue> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<DueProfile> _dueProfileRepository;
    private readonly IStringLocalizer _localizer;

    public MerchantDueScreenService(IGenericRepository<MerchantDue> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<DueProfile> dueProfileRepository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _merchantRepository = merchantRepository;
        _dueProfileRepository = dueProfileRepository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        if (!(Guid.TryParse(queryParameters[2], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(m => m.Id == id);

        var merchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == entity.MerchantId);

        var dueProfile = await _dueProfileRepository.GetAll().FirstOrDefaultAsync(d => d.Id == entity.DueProfileId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("DueProfileTitle").Value, dueProfile.Title},
            { _localizer.GetString("DueProfileType").Value, dueProfile.DueType.ToString()},
            { _localizer.GetString("LastExecDate").Value, entity.LastExecutionDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("TotalExecCount").Value, entity.TotalExecutionCount.ToString()},
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
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantDueCommand>(request.Body);

        var merchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == requestBody.MerchantId);

        var dueProfile = await _dueProfileRepository.GetAll().FirstOrDefaultAsync(d => d.Id == requestBody.DueProfileId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name},
            { _localizer.GetString("MerchantNumber").Value, merchant.Number},
            { _localizer.GetString("DueProfileTitle").Value, dueProfile.Title},
            { _localizer.GetString("DueProfileType").Value, dueProfile.DueType.ToString()},
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
}
