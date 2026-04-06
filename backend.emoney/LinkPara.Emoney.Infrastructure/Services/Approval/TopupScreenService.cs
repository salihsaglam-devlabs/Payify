using LinkPara.Approval;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.CommercialPricing.Commands.CreatePricingCommercial;
using LinkPara.Emoney.Application.Features.Topups.Commands.TopupCancel;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class TopupScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<CardTopupRequest> _repository;
    private readonly IStringLocalizer _localizer;

    public TopupScreenService(IStringLocalizerFactory factory,
        IGenericRepository<CardTopupRequest> repository)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
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
        var requestBody = JsonConvert.DeserializeObject<TopupCancelCommand>(request.Body);

        var cardTopupRequest = await _repository.GetAll()
                                              .SingleOrDefaultAsync(x => x.Id == requestBody.BaseRequest.CardTopupRequestId);
        
        if (cardTopupRequest is null)
        {
            throw new NotFoundException(nameof(CardTopupRequest));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), cardTopupRequest.Id.ToString() },
            { _localizer.GetString("Description"), cardTopupRequest.CancelDescription},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Topups"
        };
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
