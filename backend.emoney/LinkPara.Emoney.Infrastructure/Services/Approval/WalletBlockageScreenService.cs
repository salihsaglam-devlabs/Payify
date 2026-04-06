using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.WalletBlockages.Commands;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;
public class WalletBlockageScreenService : IApprovalScreenService
{    
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<WalletBlockage> _walletBlockageRepository;

    public WalletBlockageScreenService(IStringLocalizerFactory factory,
        IGenericRepository<WalletBlockage> walletBlockageRepository)
    {        
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
        _walletBlockageRepository = walletBlockageRepository;
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<AddWalletBlockageCommand>(request.Body);

        var blockage = _walletBlockageRepository.GetAll()
            .Where(x => x.WalletNumber == requestBody.WalletNumber)
            .FirstOrDefault();

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, blockage.AccountName},
            { _localizer.GetString("WalletNumber").Value, blockage.WalletNumber},
            { _localizer.GetString("OperationType").Value, blockage.OperationType},
            { _localizer.GetString("CashBlockageAmount").Value, blockage.CashBlockageAmount},
            { _localizer.GetString("CreditBlockageAmount").Value, blockage.CreditBlockageAmount},
            { _localizer.GetString("WalletCurrencyCode").Value, blockage.WalletCurrencyCode},
            { _localizer.GetString("BlockageType").Value, blockage.BlockageType},
            { _localizer.GetString("BlockageDescription").Value, blockage.BlockageDescription},
            { _localizer.GetString("BlockageStartDate").Value, blockage.BlockageStartDate.ToString("dd-MM-yyyy HH:mm:ss")},
            { _localizer.GetString("BlockageEndDate").Value, Convert.ToDateTime(blockage.BlockageEndDate).ToString("dd-MM-yyyy HH:mm:ss")}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "WalletBlockage"
        });
    }
    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
