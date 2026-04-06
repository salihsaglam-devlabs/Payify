using DocumentFormat.OpenXml.Vml.Office;
using LinkPara.Approval;
using LinkPara.Approval.Helper;
using LinkPara.Approval.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantPools.Command.ApproveMerchantPool;
using LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.Services.Approval;

public class MerchantPoolScreenService : IApprovalScreenService
{

    private readonly IGenericRepository<MerchantPool> _repository;
    private readonly IGenericRepository<Bank> _bankRepository;
    private readonly IStringLocalizer _localizer;

    public MerchantPoolScreenService(IGenericRepository<MerchantPool> repository,
    IStringLocalizerFactory factory,
    IGenericRepository<Bank> bankRepository)
    {
        _localizer = factory.Create("ScreenFields", "LinkPara.PF.API");
        _repository = repository;
        _bankRepository = bankRepository;
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
        var requestBody = JsonConvert.DeserializeObject<SaveMerchantPoolCommand>(request.Body);

        var bank = await _bankRepository.GetAll().Where(b => b.Code == requestBody.BankCode).SingleOrDefaultAsync();
        if (bank is null)
        {
            throw new NotFoundException(nameof(Bank), requestBody.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, _localizer.GetString(requestBody.MerchantName).Value},
            { _localizer.GetString("CompanyType").Value, _localizer.GetString(requestBody.CompanyType.ToString()).Value},
            { _localizer.GetString("CommercialTitle").Value, _localizer.GetString(requestBody.CommercialTitle).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bank.Name).Value},
            { _localizer.GetString("WebSiteUrl").Value, _localizer.GetString(requestBody.WebSiteUrl).Value},
            { _localizer.GetString("MonthlyTurnover").Value, _localizer.GetString(requestBody.MonthlyTurnover.ToString("0.00")).Value},
            { _localizer.GetString("PostalCode").Value, requestBody.PostalCode},
            { _localizer.GetString("Address").Value, requestBody.Address},
            { _localizer.GetString("PhoneCode").Value, requestBody.PhoneCode},
            { _localizer.GetString("CountryName").Value, requestBody.CountryName},
            { _localizer.GetString("CityName").Value, requestBody.CityName},
            { _localizer.GetString("DistrictName").Value, requestBody.DistrictName},
            { _localizer.GetString("TaxAdministration").Value, requestBody.TaxAdministration},
            { _localizer.GetString("TaxNumber").Value, requestBody.CompanyType == CompanyType.Individual 
                                                                ? requestBody.AuthorizedPersonIdentityNumber : requestBody.TaxNumber},
            { _localizer.GetString("TradeRegistrationNumber").Value, requestBody.TradeRegistrationNumber},
            { _localizer.GetString("Iban").Value, requestBody.Iban},
            { _localizer.GetString("Email").Value, requestBody.Email},
            { _localizer.GetString("CompanyEmail").Value, requestBody.CompanyEmail},
            { _localizer.GetString("AuthorizedPersonIdentityNumber").Value, requestBody.AuthorizedPersonIdentityNumber},
            { _localizer.GetString("AuthorizedPersonName").Value, requestBody.AuthorizedPersonName},
            { _localizer.GetString("AuthorizedPersonSurname").Value, requestBody.AuthorizedPersonSurname},
            { _localizer.GetString("AuthorizedPersonBirthDate").Value, requestBody.AuthorizedPersonBirthDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("AuthorizedPersonCompanyPhoneNumber").Value, requestBody.AuthorizedPersonCompanyPhoneNumber},
            { _localizer.GetString("AuthorizedPersonMobilePhoneNumber").Value, requestBody.AuthorizedPersonMobilePhoneNumber},
            { _localizer.GetString("AuthorizedPersonMobilePhoneNumberSecond").Value, requestBody.AuthorizedPersonMobilePhoneNumberSecond},
            { _localizer.GetString("MerchantPoolStatus").Value, _localizer.GetString(MerchantPoolStatus.Waiting.ToString()).Value},
        };

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource
        };

    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<ApproveMerchantPoolCommand>(request.Body);

        var entity = await _repository.GetAll().FirstOrDefaultAsync(x => x.Id == requestBody.MerchantPoolId &&
                                                                            x.RecordStatus == RecordStatus.Active);
        if (entity is null)
        {
            throw new NotFoundException(nameof(MerchantPool), requestBody.MerchantPoolId);
        }

        var bank = await _bankRepository.GetAll().Where(b => b.Code == entity.BankCode).SingleOrDefaultAsync();
        if (bank is null)
        {
            throw new NotFoundException(nameof(Bank), entity.BankCode);
        }

        var data = new Dictionary<string, object>
        {
            { _localizer.GetString("MerchantName").Value, _localizer.GetString(entity.MerchantName).Value},
            { _localizer.GetString("CompanyType").Value, _localizer.GetString(entity.CompanyType.ToString()).Value},
            { _localizer.GetString("CommercialTitle").Value, _localizer.GetString(entity.CommercialTitle).Value},
            { _localizer.GetString("BankName").Value, _localizer.GetString(bank.Name).Value},
            { _localizer.GetString("WebSiteUrl").Value, entity.WebSiteUrl},
            { _localizer.GetString("MonthlyTurnover").Value, _localizer.GetString(entity.MonthlyTurnover.ToString("0.00")).Value},
            { _localizer.GetString("PostalCode").Value, entity.PostalCode},
            { _localizer.GetString("Address").Value, entity.Address},
            { _localizer.GetString("PhoneCode").Value, entity.PhoneCode},
            { _localizer.GetString("CountryName").Value, entity.CountryName},
            { _localizer.GetString("CityName").Value, entity.CityName},
            { _localizer.GetString("DistrictName").Value, entity.DistrictName},
            { _localizer.GetString("TaxAdministration").Value, entity.TaxAdministration},
            { _localizer.GetString("TaxNumber").Value, entity.CompanyType == CompanyType.Individual ? entity.AuthorizedPersonIdentityNumber : entity.TaxNumber},
            { _localizer.GetString("TradeRegistrationNumber").Value, entity.TradeRegistrationNumber},
            { _localizer.GetString("Iban").Value, entity.Iban},
            { _localizer.GetString("Email").Value, entity.Email},
            { _localizer.GetString("CompanyEmail").Value, entity.CompanyEmail},
            { _localizer.GetString("AuthorizedPersonIdentityNumber").Value, entity.AuthorizedPersonIdentityNumber},
            { _localizer.GetString("AuthorizedPersonName").Value, entity.AuthorizedPersonName},
            { _localizer.GetString("AuthorizedPersonSurname").Value, entity.AuthorizedPersonSurname},
            { _localizer.GetString("AuthorizedPersonBirthDate").Value, entity.AuthorizedPersonBirthDate.ToString("dd.MM.yyyy")},
            { _localizer.GetString("AuthorizedPersonCompanyPhoneNumber").Value, entity.AuthorizedPersonCompanyPhoneNumber},
            { _localizer.GetString("AuthorizedPersonMobilePhoneNumber").Value, entity.AuthorizedPersonMobilePhoneNumber},
            { _localizer.GetString("AuthorizedPersonMobilePhoneNumberSecond").Value, entity.AuthorizedPersonMobilePhoneNumberSecond},
            { _localizer.GetString("MerchantPoolStatus").Value, _localizer.GetString(MerchantPoolStatus.Waiting.ToString()).Value},
        };

        var updatedFields = new Dictionary<string, object>();

        if (requestBody.IsApprove)
        {
            var updatedFieldApprove = new Dictionary<string, object>
            {
                {"OldValue", _localizer.GetString(false.ToString()).Value },
                {"NewValue", _localizer.GetString(true.ToString()).Value }
            };

            updatedFields.Add(_localizer.GetString("IsApprove").Value, updatedFieldApprove);

            var updatedFieldMerchantPoolStatus = new Dictionary<string, object>
            {
                {"OldValue", _localizer.GetString(entity.MerchantPoolStatus.ToString()).Value },
                {"NewValue",  _localizer.GetString(MerchantPoolStatus.Completed.ToString()).Value }
            };

            updatedFields.Add(_localizer.GetString("MerchantPoolStatus").Value, updatedFieldMerchantPoolStatus);
        }
        else
        {
            updatedFields = UpdatedFieldsHelper.GetUpdatedFields(entity, requestBody, _localizer);

            var updatedFieldMerchantPoolStatus = new Dictionary<string, object>
            {
                {"OldValue", _localizer.GetString(entity.MerchantPoolStatus.ToString()).Value },
                {"NewValue",  _localizer.GetString(MerchantPoolStatus.Rejected.ToString()).Value }
            };

            updatedFields.Add(_localizer.GetString("MerchantPoolStatus").Value, updatedFieldMerchantPoolStatus);
        }

        if (updatedFields.ContainsKey("ParameterValue"))
        {
            updatedFields.Remove("ParameterValue");
        }

        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
