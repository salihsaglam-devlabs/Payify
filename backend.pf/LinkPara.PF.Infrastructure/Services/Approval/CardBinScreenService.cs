using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.CardBins;
using LinkPara.PF.Application.Features.CardBins.Command.SaveCardBin;
using LinkPara.PF.Application.Features.CardBins.Command.UpdateCardBin;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using MassTransit.Serialization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;
using System.Globalization;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class CardBinScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<CardBin> _repository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IStringLocalizer _localizer;

    public CardBinScreenService(IGenericRepository<CardBin> repository,
        IStringLocalizerFactory factory,
        IGenericRepository<Bank> bankRepository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _bankRepository = bankRepository;
    }

    public async Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var queryParameters = request.QueryParameters.Split('=', StringSplitOptions.RemoveEmptyEntries);

        if (!(queryParameters.Length == 2 && Guid.TryParse(queryParameters[1], out Guid id)))
        {
            throw new InvalidCastException("IdIsNotFound");
        }

        var entity = await _repository.GetAll().Include(x => x.Bank)
                                               .SingleOrDefaultAsync(x => x.Id == id &&
                                                                          x.RecordStatus == RecordStatus.Active);
        if (entity is null)
        {
            throw new NotFoundException(nameof(CardBin), id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BIN").Value,  _localizer.GetString(entity.BinNumber).Value},
            { _localizer.GetString("CardBrand").Value,  _localizer.GetString(entity.CardBrand.ToString()).Value},
            { _localizer.GetString("CardType").Value,  _localizer.GetString(entity.CardType.ToString()).Value},
            { _localizer.GetString("CardSubType").Value,  _localizer.GetString(entity.CardSubType.ToString()).Value},
            { _localizer.GetString("CardNetwork").Value,  _localizer.GetString(entity.CardNetwork.ToString()).Value},
            { _localizer.GetString("IsVirtual").Value,  _localizer.GetString(entity.IsVirtual.ToString()).Value},
            { _localizer.GetString("CountryName").Value,  _localizer.GetString(entity.CountryName).Value},
            { _localizer.GetString("BankName").Value,  _localizer.GetString(entity.Bank?.Name).Value}
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
        if (!Guid.TryParse(url.LastOrDefault(), out var cardBinId))
        {
            throw new InvalidCastException();
        }

        var cardBin = await _repository.GetAll().Include(b => b.Bank)
                                                .FirstOrDefaultAsync(b => b.Id == cardBinId);

        if (cardBin is null)
        {
            throw new NotFoundException(nameof(CardBin), cardBinId);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BIN").Value, cardBin.BinNumber},
            { _localizer.GetString("CardBrand").Value,  _localizer.GetString(cardBin.CardBrand.ToString()).Value},
            { _localizer.GetString("CardType").Value,  _localizer.GetString(cardBin.CardType.ToString()).Value},
            { _localizer.GetString("CardSubType").Value,  _localizer.GetString(cardBin.CardSubType.ToString()).Value},
            { _localizer.GetString("CardNetwork").Value,  _localizer.GetString(cardBin.CardNetwork.ToString()).Value},
            { _localizer.GetString("IsVirtual").Value,  _localizer.GetString(cardBin.IsVirtual.ToString()).Value},
            { _localizer.GetString("CountryName").Value,   _localizer.GetString(cardBin.CountryName).Value},
            { _localizer.GetString("BankName").Value,   _localizer.GetString(cardBin.Bank?.Name).Value}
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateCardBinRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == _localizer.GetString("BankCode").Value))
        {
            updatedFields.TryGetValue(_localizer.GetString("BankCode").Value, out Dictionary<string, object> bankCode);

            var bank = await _bankRepository.GetAll().SingleOrDefaultAsync(x => x.Code == (int)bankCode.GetValueOrDefault("NewValue"));

            if (bank is null)
            {
                throw new NotFoundException(nameof(Bank), (int)bankCode.GetValueOrDefault("NewValue"));
            }

            updatedFields.Remove(_localizer.GetString("BankCode").Value);

            var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", cardBin.Bank.Name },
                        {"NewValue", bank.Name }
                    };
            updatedFields.Add(_localizer.GetString("BankName").Value, updatedField);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }

    public async Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<SaveCardBinCommand>(request.Body);
        
        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BIN").Value, requestBody.BinNumber},
            { _localizer.GetString("CardBrand").Value,  _localizer.GetString(requestBody.CardBrand.ToString()).Value},
            { _localizer.GetString("CardType").Value,  _localizer.GetString(requestBody.CardType.ToString()).Value},
            { _localizer.GetString("CardSubType").Value,  _localizer.GetString(requestBody.CardSubType.ToString()).Value},
            { _localizer.GetString("CardNetwork").Value,  _localizer.GetString(requestBody.CardNetwork.ToString()).Value},
            { _localizer.GetString("IsVirtual").Value,  _localizer.GetString(requestBody.IsVirtual.ToString()).Value},
        };

        var bank = await _bankRepository.GetAll().SingleOrDefaultAsync(x => x.Code == requestBody.BankCode);

        if (bank is not null)
        {
            data.Add(_localizer.GetString("BankName").Value, bank.Name);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateCardBinCommand>(request.Body);

        var entity = await _repository.GetAll().Include(x => x.Bank)
                                               .SingleOrDefaultAsync(x => x.Id == requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(CardBin), requestBody.Id);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("BIN").Value, entity.BinNumber},
            { _localizer.GetString("CardBrand").Value,  _localizer.GetString(requestBody.CardType.ToString()).Value},
            { _localizer.GetString("CardType").Value,  _localizer.GetString(entity.CardType.ToString()).Value},
            { _localizer.GetString("CardSubType").Value,  _localizer.GetString(entity.CardSubType.ToString()).Value},
            { _localizer.GetString("CardNetwork").Value,  _localizer.GetString(entity.CardNetwork.ToString()).Value},
            { _localizer.GetString("IsVirtual").Value,  _localizer.GetString(entity.IsVirtual.ToString()).Value},
            { _localizer.GetString("BankName").Value,  entity.Bank.Name}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        if(updatedFields.Any(x=>x.Key == _localizer.GetString("BankCode").Value))
        {
            var bank = await _bankRepository.GetAll().SingleOrDefaultAsync(x => x.Code == requestBody.BankCode);

            if (bank is null)
            {
                throw new NotFoundException(nameof(Bank), requestBody.BankCode);
            }

            updatedFields.Remove(_localizer.GetString("BankCode").Value);
            
            var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", entity.Bank.Name },
                        {"NewValue", bank.Name }
                    };
            updatedFields.Add(_localizer.GetString("BankName").Value, updatedField);
        }

        if (updatedFields.Any(x => x.Key == _localizer.GetString("Country").Value))
        {
            updatedFields.Remove(_localizer.GetString("Country").Value);
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
