using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;

public class TierLevelScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<TierLevel> _repository;
    private readonly IStringLocalizer _localizer;
    private readonly IMapper _mapper;

    public TierLevelScreenService(IStringLocalizerFactory factory,
        IGenericRepository<TierLevel> repository,
        IMapper mapper)
    {
        _repository = repository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
        _mapper = mapper;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var tierLevelId))
        {
            throw new InvalidCastException();
        }

        var tierLevel = await _repository.GetAll()
                                               .SingleOrDefaultAsync(x => x.Id == tierLevelId);
        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), tierLevel.Id.ToString() },
            { _localizer.GetString("Name"), tierLevel.Name },
            { _localizer.GetString("TierLevelType"), tierLevel.TierLevelType.ToString() },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "TierLevels"
        };
    }

    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var tierLevelId))
        {
            throw new InvalidCastException();
        }

        var tierLevel = await _repository.GetAll()
                                               .SingleOrDefaultAsync(x => x.Id == tierLevelId);

        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel));
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Id"), tierLevel.Id.ToString() },
            { _localizer.GetString("Name"), tierLevel.Name },
            { _localizer.GetString("AccountStatus"), tierLevel.TierLevelType.ToString() },
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<CustomTierLevelDto>>(request.Body);

        var requestTierLevelDto = _mapper.Map<CustomTierLevelDto>(tierLevel);

        requestBody.ApplyTo(requestTierLevelDto);

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(tierLevel, requestTierLevelDto, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "TierLevels",
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CustomTierLevelDto>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name"), requestBody.Name },
            { _localizer.GetString("RecordStatus"), requestBody.RecordStatus.ToString() },
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "TierLevels"
        });
    }

    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
