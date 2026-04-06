using AutoMapper;
using LinkPara.Approval.Models;
using LinkPara.Approval;
using LinkPara.Billing.Application.Commons.Interfaces;
using Microsoft.Extensions.Localization;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Approval.Helper;
using LinkPara.SharedModels.Exceptions;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using LinkPara.Billing.Application.Features.Institutions.Commands;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Infrastructure.Services.Approval;

public class InstitutionScreenService : IApprovalScreenService
{
    private readonly IGenericRepository<Institution> _institutionRepository;
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly IStringLocalizer _localizer;

    public InstitutionScreenService(IGenericRepository<Institution> institutionRepository,
        IStringLocalizerFactory factory,
        IGenericRepository<Vendor> vendorRepository)
    {
        _institutionRepository = institutionRepository;
        _localizer = factory.Create("ScreenFields", "LinkPara.Billing.API");
        _vendorRepository = vendorRepository;   
    }

    public Task<ApprovalScreenResponse> DeleteScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PatchScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ApprovalScreenResponse> PostScreenFieldsAsync(ApprovalScreenRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ApprovalScreenResponse> PutScreenFieldsAsync(ApprovalScreenRequest request)
    {
        var requestBody = JsonConvert.DeserializeObject<UpdateInstitutionCommand>(request.Body);

        var institution = await _institutionRepository.GetAll()
            .FirstOrDefaultAsync(i => i.Id == requestBody.InstitutionId);

        if (institution is null)
        {
            throw new NotFoundException(nameof(Institution), requestBody.InstitutionId);
        }

        var oldActiveVendor = await _vendorRepository.GetAll().Where(s => s.Id == institution.ActiveVendorId)
               .SingleOrDefaultAsync();

        var data = new Dictionary<string, object>
        {
            {_localizer.GetString("RecordStatus").Value, _localizer.GetString(institution.RecordStatus.ToString()).Value},
            {_localizer.GetString("Name").Value, institution.Name},
            {_localizer.GetString("OperationMode").Value, _localizer.GetString(institution.OperationMode.ToString()).Value},        
            {_localizer.GetString("ActiveVendor").Value, oldActiveVendor?.Name},        
        };

        var updatedFields = UpdatedFieldsHelper.GetUpdatedFields(institution, requestBody, _localizer);

        if (updatedFields.Any(x => x.Key == "ActiveVendorId"))
        {
            var newActiveVendor = await _vendorRepository.GetAll().Where(s => s.Id == requestBody.ActiveVendorId)
             .SingleOrDefaultAsync();


            updatedFields.Remove("ActiveVendorId");
           
            if (newActiveVendor is not null)
            {
                var updatedField = new Dictionary<string, object>
                    {
                        {"OldValue", oldActiveVendor?.Name },
                        {"NewValue", newActiveVendor.Name }
                    };
                updatedFields.Add(_localizer.GetString("ActiveVendor").Value, updatedField);
            }
        }


        return new ApprovalScreenResponse
        {
            DisplayScreenFields = data,
            Resource = request.Resource,
            UpdatedFields = updatedFields
        };
    }
}
