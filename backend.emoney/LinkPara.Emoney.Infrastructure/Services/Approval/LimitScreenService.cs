using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.Emoney.Application.Features.Limits;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.Emoney.Infrastructure.Services.Approval;
public class LimitScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<TierLevel> _tierLevelRepository;
    private readonly IMapper _mapper;
    private readonly IStringLocalizer _localizer;

    public LimitScreenService(IGenericRepository<TierLevel> tierLevelRepository,
        IMapper mapper,
        IStringLocalizerFactory factory)
    {
        _tierLevelRepository = tierLevelRepository;
        _mapper = mapper;
        _localizer = factory.Create("ScreenFields", "LinkPara.Emoney.API");
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<CustomTierLevelDto>(request.Body);

        var data = new Dictionary<string, object>
        {
            { "Name", requestBody.Name},
            { "TierLevelType", TierLevelType.Custom.ToString() },
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Limits"
        });
    }
    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var tierLevelId))
        {
            throw new InvalidCastException();
        }

        var tierLevel = await _tierLevelRepository.GetAll()
                                               .SingleOrDefaultAsync(x => x.Id == tierLevelId);
        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel));
        }

        var data = new Dictionary<string, object>
        {
            {"Id", tierLevel.Id},
            {"Name", tierLevel.Name},
            { "TierLevelType", TierLevelType.Custom.ToString() },
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = "Limits"
        };
    }
    public async Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        ///v1/Limits/894b23e6-985d-4eb5-89a0-08da9ff646bb
        var url = request.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (!Guid.TryParse(url.LastOrDefault(), out var limitId))
        {
            throw new InvalidCastException();
        }

        var tierLevel = await _tierLevelRepository.GetByIdAsync(limitId);

        if (tierLevel is null)
        {
            throw new NotFoundException(nameof(TierLevel), limitId!);
        }

        var displayScreenFields = new Dictionary<string, object>
        {
            { "Name", tierLevel.Name}
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<CustomTierLevelDto>>(request.Body);

        var requestTierLevelDto = _mapper.Map<CustomTierLevelDto>(tierLevel);

        requestBody.ApplyTo(requestTierLevelDto);

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(tierLevel, requestTierLevelDto, _localizer);

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = displayScreenFields,
            Resource = "Limits",
            UpdatedFields = updatedFields
        };
    }
    public Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }
}
