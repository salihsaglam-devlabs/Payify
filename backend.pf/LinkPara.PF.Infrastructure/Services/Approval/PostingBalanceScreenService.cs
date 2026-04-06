using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.Posting;
using LinkPara.PF.Application.Features.PostingBalances.Commands.RetryPayment;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class PostingBalanceScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<PostingBalance> _postingBalanceRepository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;

    public PostingBalanceScreenService(IGenericRepository<PostingBalance> postingBalanceRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository)
    {
        _postingBalanceRepository = postingBalanceRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _merchantRepository = merchantRepository;
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<RetryPaymentCommand>(request.Body);

        var postingBalance =
            await _postingBalanceRepository.GetAll().Include(s => s.Merchant).FirstOrDefaultAsync(s => s.Id == requestBody.BalanceId);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, postingBalance.Merchant.Name},
            { _localizer.GetString("MerchantIban").Value, postingBalance.Iban },
            { _localizer.GetString("PostingPaymentChannel").Value, postingBalance.PostingPaymentChannel.ToString() },
            { _localizer.GetString("MerchantWalletNumber").Value, postingBalance.WalletNumber },
            { _localizer.GetString("PostingDate").Value, postingBalance.PostingDate.ToString("dd.MM.yyyy") },
            { _localizer.GetString("TotalPayingAmount").Value, postingBalance.TotalPayingAmount },
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
        if (!Guid.TryParse(url.LastOrDefault(), out var postingBalanceId))
        {
            throw new InvalidCastException();
        }

        var entity = await _postingBalanceRepository.GetAll().FirstOrDefaultAsync(p => p.Id == postingBalanceId);

        if (entity is null)
        {
            throw new NotFoundException(nameof(PostingBalance), postingBalanceId);
        }

        var balanceMerchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(m => m.Id == entity.MerchantId);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Merchant), entity.MerchantId);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, balanceMerchant.Name},
            { _localizer.GetString("MerchantNumber").Value, balanceMerchant.Number},
            { _localizer.GetString("MerchantIban").Value, entity.Iban},
            { _localizer.GetString("PostingPaymentChannel").Value, entity.PostingPaymentChannel.ToString() },
            { _localizer.GetString("MerchantWalletNumber").Value, entity.WalletNumber },
            { _localizer.GetString("MoneyTransferBankCode").Value, entity.MoneyTransferBankCode},
            { _localizer.GetString("MoneyTransferBankName").Value, entity.MoneyTransferBankName},
            { _localizer.GetString("PostingDate").Value, entity.PostingDate.ToString("dd.MM.yyyy") },
            { _localizer.GetString("TotalPayingAmount").Value, entity.TotalPayingAmount },
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<PatchPostingBalanceRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }


}