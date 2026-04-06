using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Wallets.Commands.UpdateWallet;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class WalletScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Wallet> _repository;
    private readonly IStringLocalizer _localizer;

    public WalletScreenService(IStringLocalizerFactory factory,
        IGenericRepository<Wallet> repository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateWalletCommand>(request.Body);

        var wallet = await _repository.GetAll()
                                      .SingleOrDefaultAsync(r => r.Id == requestBody.WalletId);

        if (wallet == null)
        {
            throw new NotFoundException(nameof(Wallet));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), wallet.Id.ToString() },
            { _localizer.GetString("WalletNumber"), wallet.WalletNumber },
            { _localizer.GetString("RecordStatus"), wallet.RecordStatus },
            { _localizer.GetString("ClosingDate"), wallet.ClosingDate },
            { _localizer.GetString("OpeningDate"), wallet.OpeningDate },
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(wallet, requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Wallets",
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
