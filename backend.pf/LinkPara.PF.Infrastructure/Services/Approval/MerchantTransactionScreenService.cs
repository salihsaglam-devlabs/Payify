using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Commons.Models.MerchantTransactions;
using LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;
using LinkPara.PF.Application.Features.Payments.Commands.ManualReturn;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;
using System.Web;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantTransactionScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<MerchantTransaction> _repository;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IStringLocalizer _localizer;

    public MerchantTransactionScreenService(IGenericRepository<MerchantTransaction> repository,
        IStringLocalizerFactory factory,
        IMapper mapper,
        IGenericRepository<Merchant> merchantRepository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _merchantRepository = merchantRepository;
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<ManualReturnCommand>(request.Body);

        var merchantTransaction = await _repository.GetAll()
            .Include(s => s.Merchant)
            .Where(s => s.Id == requestBody.MerchantTransactionId)
            .FirstOrDefaultAsync();
        
        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchantTransaction.Merchant.Name},
            { _localizer.GetString("ConversationId").Value, merchantTransaction.ConversationId},
            { _localizer.GetString("TransactionType").Value, _localizer.GetString(merchantTransaction.TransactionType.ToString()).Value },
            { _localizer.GetString("TransactionDate").Value, merchantTransaction.TransactionDate.ToString("dd.MM.yyyy") },
            { _localizer.GetString("TransactionStatus").Value, _localizer.GetString(merchantTransaction.TransactionStatus.ToString()).Value },
            { _localizer.GetString("Amount").Value, requestBody.Amount}
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

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var id))
        {
            throw new InvalidCastException();
        }

        var entity = await _repository.GetByIdAsync(id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(MerchantTransaction), id);
        }

        var merchant = await _merchantRepository.GetByIdAsync(entity.MerchantId);

        if (merchant is null)
        {
            throw new NotFoundException(nameof(Merchant), entity.MerchantId);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, merchant.Name },
            { _localizer.GetString("TransactionType").Value, _localizer.GetString(entity.TransactionType.ToString()).Value },
            { _localizer.GetString("TransactionDate").Value, entity.TransactionDate.ToString("dd.MM.yyyy") },
            { _localizer.GetString("TransactionStatus").Value, _localizer.GetString(entity.TransactionStatus.ToString()).Value },
            { _localizer.GetString("IsChargeback").Value, _localizer.GetString(entity.IsChargeback.ToString()).Value },
            { _localizer.GetString("IsSuspecious").Value, _localizer.GetString(entity.IsSuspecious.ToString()).Value },
            { _localizer.GetString("SuspeciousDesc").Value, entity.SuspeciousDescription },
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateMerchantTransactionRequest>>(HttpUtility.UrlDecode(request.Body), settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}