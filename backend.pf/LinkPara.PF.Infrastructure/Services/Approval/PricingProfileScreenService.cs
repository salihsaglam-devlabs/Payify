using AutoMapper;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles;
using LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Text;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class PricingProfileScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<PricingProfile> _repository;
    private readonly IStringLocalizer _localizer;
    private readonly IMapper _mapper;

    public PricingProfileScreenService(IGenericRepository<PricingProfile> repository,
        IStringLocalizerFactory factory,
        IMapper mapper)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(PricingProfile), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(entity.ActivationDate.ToString("dd.MM.yyyy")).Value},
            { _localizer.GetString("ProfileStatus").Value, _localizer.GetString(entity.ProfileStatus.ToString()).Value},
            { _localizer.GetString("CurrencyCode").Value, _localizer.GetString(entity.CurrencyCode).Value},
            { _localizer.GetString("PerTransactionFee").Value, _localizer.GetString(entity.PerTransactionFee.ToString()).Value}
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
        if (!Guid.TryParse(url.LastOrDefault(), out var id))
        {
            throw new InvalidCastException();
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(b => b.Id == id);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(entity.ActivationDate.ToString("dd.MM.yyyy")).Value},
            { _localizer.GetString("ProfileStatus").Value, _localizer.GetString(entity.ProfileStatus.ToString()).Value},
            { _localizer.GetString("CurrencyCode").Value, _localizer.GetString(entity.CurrencyCode).Value},
            { _localizer.GetString("PerTransactionFee").Value, _localizer.GetString(entity.PerTransactionFee.ToString()).Value},
            { _localizer.GetString("PricingProfileItems").Value, _localizer.GetString(PricingProfileItemsToString(entity.PricingProfileItems)).Value}
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdatePricingProfileRequest>>(request.Body);

        var entityMap = _mapper.Map<UpdatePricingProfileRequest>(entity);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        requestBody.ApplyTo(entityMap);

        if (PricingProfileItemsToString(entity.PricingProfileItems) != PricingProfileItemsToString(entityMap.PricingProfileItems))
        {
            var updatedField = new Dictionary<string, object>
            {
                {"OldValue", PricingProfileItemsToString(entity.PricingProfileItems) },
                {"NewValue", PricingProfileItemsToString(entityMap.PricingProfileItems) }
            };

            updatedFields.Add(_localizer.GetString("PricingProfileItems").Value, updatedField);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SavePricingProfileCommand>(request.Body);

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(requestBody.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(requestBody.ActivationDate.ToString("dd.MM.yyyy")).Value},
            { _localizer.GetString("PerTransactionFee").Value, _localizer.GetString(requestBody.PerTransactionFee.ToString()).Value},
            { _localizer.GetString("PricingProfileItems").Value, _localizer.GetString(PricingProfileItemsToString(requestBody.PricingProfileItems)).Value}
        };

        return Task.FromResult(new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        });
    }    

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdatePricingProfileCommand>(request.Body);

        var entity = await _repository.GetAll()
            .Include(x => x.PricingProfileItems
                .OrderBy(b => b.InstallmentNumberEnd))
            .FirstOrDefaultAsync(x => x.Id == requestBody.Id);
        
        if (entity is null)
        {
            throw new NotFoundException(nameof(PricingProfile), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("ActivationDate").Value, _localizer.GetString(entity.ActivationDate.ToString("dd.MM.yyyy")).Value},
            { _localizer.GetString("ProfileStatus").Value, _localizer.GetString(entity.ProfileStatus.ToString()).Value}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        updatedFields.Remove(_localizer.GetString("PricingProfileItems").Value);
        
        for (int i = 0; i < entity.PricingProfileItems.Count; i++)
        {
            if (entity.PricingProfileItems[i].CommissionRate != requestBody.PricingProfileItems[i].CommissionRate)
            {
                var updatedField = new Dictionary<string, object>
                {
                    { "OldValue", entity.PricingProfileItems[i].CommissionRate.ToString("0.00") },
                    { "NewValue", requestBody.PricingProfileItems[i].CommissionRate.ToString("0.00") }
                };

                var localizedName =
                    requestBody.PricingProfileItems[i].InstallmentNumber ==
                    requestBody.PricingProfileItems[i].InstallmentNumberEnd
                        ? requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + " "
                        + _localizer.GetString("Installment").Value
                        : requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + "-" +
                          requestBody.PricingProfileItems[i].InstallmentNumberEnd.ToString() + " " +
                          _localizer.GetString("Installment").Value;

                localizedName = requestBody.PricingProfileItems[i].InstallmentNumber == 0
                    ? localizedName + " " + _localizer.GetString(requestBody.PricingProfileItems[i].ProfileCardType.ToString()).Value 
                    : localizedName;
                localizedName = localizedName + " " + _localizer.GetString("CommissionRate").Value;
                updatedFields.Add(localizedName, updatedField);
            }

            if (entity.PricingProfileItems[i].BlockedDayNumber != requestBody.PricingProfileItems[i].BlockedDayNumber)
            {
                var updatedField = new Dictionary<string, object>
                {
                    { "OldValue", entity.PricingProfileItems[i].BlockedDayNumber.ToString() },
                    { "NewValue", requestBody.PricingProfileItems[i].BlockedDayNumber.ToString() }
                };

                var localizedName =
                    requestBody.PricingProfileItems[i].InstallmentNumber ==
                    requestBody.PricingProfileItems[i].InstallmentNumberEnd
                        ? requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + " "
                        + _localizer.GetString("Installment").Value
                        : requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + "-" +
                          requestBody.PricingProfileItems[i].InstallmentNumberEnd.ToString() + " " +
                          _localizer.GetString("Installment").Value;
                
                localizedName = requestBody.PricingProfileItems[i].InstallmentNumber == 0
                    ? localizedName + " " + _localizer.GetString(requestBody.PricingProfileItems[i].ProfileCardType.ToString()).Value 
                    : localizedName;
                
                localizedName = localizedName + " " + _localizer.GetString("BlockedDayNumber").Value;
                
                updatedFields.Add(localizedName, updatedField);
            }

            if (entity.PricingProfileItems[i].IsActive != requestBody.PricingProfileItems[i].IsActive)
            {
                var updatedField = new Dictionary<string, object>
                {
                    { "OldValue", entity.PricingProfileItems[i].IsActive.ToString() },
                    { "NewValue", requestBody.PricingProfileItems[i].IsActive.ToString() }
                };

                var localizedName =
                    requestBody.PricingProfileItems[i].InstallmentNumber ==
                    requestBody.PricingProfileItems[i].InstallmentNumberEnd
                        ? requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + " "
                        + _localizer.GetString("Installment").Value
                        : requestBody.PricingProfileItems[i].InstallmentNumber.ToString() + "-" +
                          requestBody.PricingProfileItems[i].InstallmentNumberEnd.ToString() + " " +
                          _localizer.GetString("Installment").Value;
                
                localizedName = requestBody.PricingProfileItems[i].InstallmentNumber == 0
                    ? localizedName + " " + _localizer.GetString(requestBody.PricingProfileItems[i].ProfileCardType.ToString()).Value 
                    : localizedName;
                
                localizedName = localizedName + " " + _localizer.GetString("IsActive").Value;
                
                updatedFields.Add(localizedName, updatedField);
            }
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    private string PricingProfileItemsToString(List<PricingProfileItem> pricingProfileItems)
    {
        if (pricingProfileItems is not null)
        {
            var pricingProfileItemsDto = pricingProfileItems.Select(x => _mapper.Map<PricingProfileItemDto>(x)).ToList();

            return PricingProfileItemsToString(pricingProfileItemsDto);
        }

        return string.Empty;
    }

    private string PricingProfileItemsToString(List<PricingProfileItemDto> pricingProfileItems)
    {
        StringBuilder sb = new StringBuilder();

        foreach (var pricingProfileItem in pricingProfileItems)
        {
            sb.AppendLine($"{_localizer.GetString("ProfileCardType").Value} : {_localizer.GetString(pricingProfileItem.ProfileCardType.ToString()).Value}, " +
                $" { _localizer.GetString("InstallmentNumber").Value} : { pricingProfileItem.InstallmentNumber.ToString()}, " +
                $"{ _localizer.GetString("InstallmentNumberEnd").Value} : { pricingProfileItem.InstallmentNumberEnd.ToString()}, " +
                $"{ _localizer.GetString("CommissionRate").Value} : { pricingProfileItem.CommissionRate.ToString()}, " +
                $"{ _localizer.GetString("BlockedDayNumber").Value} : { pricingProfileItem.BlockedDayNumber.ToString()}, " +
                $"{ _localizer.GetString("IsActive").Value} : { _localizer.GetString(pricingProfileItem.IsActive.ToString()).Value} <br>");

        }
        return sb.ToString();
    }
}
