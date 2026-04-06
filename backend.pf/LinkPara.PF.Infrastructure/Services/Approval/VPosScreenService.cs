using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Helpers;
using LinkPara.PF.Application.Commons.Models.VposModels.Request;
using LinkPara.PF.Application.Features.VirtualPos.Command.SaveVpos;
using LinkPara.PF.Application.Features.VirtualPos.Command.UpdateVpos;
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

public class VPosScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Vpos> _repository;
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<AcquireBank> _acquireBankRepository;
    private readonly IGenericRepository<Bank> _bankRepository;

    public VPosScreenService(IGenericRepository<Vpos> repository,
        IGenericRepository<AcquireBank> acquireBankRepository,
        IGenericRepository<Bank> bankRepository,
        IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _acquireBankRepository = acquireBankRepository;
        _bankRepository = bankRepository;
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
            throw new NotFoundException(nameof(Vpos), id);
        }

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == entity.AcquireBankId);
        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), entity.AcquireBankId);
        }

        var bankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);
        if (bankEntity is null)
        {
            throw new NotFoundException(nameof(Bank), acquireBank.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("VposStatus").Value, _localizer.GetString(entity.VposStatus.ToString()).Value},
            { _localizer.GetString("SecurityType").Value, _localizer.GetString(entity.SecurityType.ToString()).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bankEntity.Name).Value}
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
        if (!Guid.TryParse(url.LastOrDefault(), out var vposId))
        {
            throw new InvalidCastException();
        }

        var entity = await _repository.GetAll().FirstOrDefaultAsync(b => b.Id == vposId);

        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), vposId);
        }

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == entity.AcquireBankId);
        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), entity.AcquireBankId);
        }

        var bankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);
        if (bankEntity is null)
        {
            throw new NotFoundException(nameof(Bank), acquireBank.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(entity.Name).Value},
            { _localizer.GetString("VposStatus").Value, _localizer.GetString(entity.VposStatus.ToString()).Value},
            { _localizer.GetString("SecurityType").Value, _localizer.GetString(entity.SecurityType.ToString()).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bankEntity.Name).Value}
        };

        var settings = new JsonSerializerSettings
        {
            Culture = new CultureInfo("tr-TR")
        };

        var requestBody = JsonConvert.DeserializeObject<JsonPatchDocument<UpdateVposRequest>>(request.Body, settings);

        var updatedFields = ScreenServiceHelper.GetUpdatedFields(requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "AcquireBankId"))
        {
            updatedFields.TryGetValue("AcquireBankId", out Dictionary<string, object> dicObj);
            dicObj.TryGetValue("NewValue", out string acquireBankId);
            updatedFields.Remove("AcquireBankId");

            var newAcquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == Guid.Parse(acquireBankId));
            if (acquireBank is null)
            {
                throw new NotFoundException(nameof(AcquireBank), entity.AcquireBankId);
            }

            var newBankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);
            if (newBankEntity is null)
            {
                throw new NotFoundException(nameof(Bank), acquireBank.BankCode);
            }

            var updatedField = new Dictionary<string, object>
            {
                {"OldValue", bankEntity.Name },
                {"NewValue", newBankEntity.Name }
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
        var requestBody = JsonConvert.DeserializeObject<SaveVposCommand>(request.Body);

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.AcquireBankId);
        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), requestBody.AcquireBankId);
        }

        var bankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);
        if (bankEntity is null)
        {
            throw new NotFoundException(nameof(Bank), acquireBank.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(requestBody.Name).Value},
            { _localizer.GetString("SecurityType").Value, _localizer.GetString(requestBody.SecurityType.ToString()).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bankEntity.Name).Value}
        };


        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateVposCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.Id);
        if (entity is null)
        {
            throw new NotFoundException(nameof(Vpos), requestBody.Id);
        }

        var acquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.AcquireBankId);
        if (acquireBank is null)
        {
            throw new NotFoundException(nameof(AcquireBank), requestBody.AcquireBankId);
        }

        var bankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);
        if (bankEntity is null)
        {
            throw new NotFoundException(nameof(Bank), acquireBank.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("Name").Value, _localizer.GetString(requestBody.Name).Value},
            { _localizer.GetString("SecurityType").Value, _localizer.GetString(requestBody.SecurityType.ToString()).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bankEntity.Name).Value}
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

        if(updatedFields.Any(x => x.Key == "AcquireBankId"))
        {
            updatedFields.Remove("AcquireBankId");

            var newAcquireBank = await _acquireBankRepository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.AcquireBankId);
            if (acquireBank is null)
            {
                throw new NotFoundException(nameof(AcquireBank), requestBody.AcquireBankId);
            }

            var newBankEntity = await _bankRepository.GetAll().FirstOrDefaultAsync(x => x.Code == acquireBank.BankCode);

            var updatedField = new Dictionary<string, object>
            {
                {"OldValue", bankEntity.Name },
                {"NewValue", newBankEntity.Name }
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
}
