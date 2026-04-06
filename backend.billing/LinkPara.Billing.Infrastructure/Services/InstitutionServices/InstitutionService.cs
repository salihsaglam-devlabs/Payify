using AutoMapper;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Institutions.Queries;
using LinkPara.Billing.Application.Features.Institutions;
using LinkPara.Billing.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Pagination;
using Microsoft.EntityFrameworkCore;
using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Features.Institutions.Commands;
using LinkPara.Audit.Models;
using LinkPara.Audit;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Billing.Infrastructure.Services.InstitutionServices;

public class InstitutionService : IInstitutionService
{
    private readonly IGenericRepository<Institution> _institutionRepository;
    private readonly IVendorService _vendorService;
    private readonly IAuditLogService _auditLogService;

    public InstitutionService(IGenericRepository<Institution> institutionRepository,
        IMapper mapper,
        IVendorService vendorService,
        IAuditLogService auditLogService)
    {
        _institutionRepository = institutionRepository;
        _vendorService = vendorService;
        _auditLogService = auditLogService; 
    }

    public async Task<PaginatedList<InstitutionDto>> GetListAsync(GetAllInstitutionQuery request)
    {
        var institutions = _institutionRepository.GetAll()
            .Include(i => i.Sector)
            .Include(i => i.ActiveVendor)
            .Select(i => new InstitutionDto
            {
                Id = i.Id,
                Name = i.Name,
                SectorId = i.SectorId,
                SectorName = i.Sector.Name,
                ActiveVendorId = i.ActiveVendorId,
                ActiveVendorName = i.ActiveVendor.Name,
                FieldRequirementType = i.FieldRequirementType,
                OperationMode = i.OperationMode,
                RecordStatus = i.RecordStatus
            });


        if (request.RecordStatus is not null)
        {
            institutions = institutions.Where(i => i.RecordStatus == request.RecordStatus);
        }

        if (request.OperationMode is not null)
        {
            institutions = institutions.Where(i => i.OperationMode == request.OperationMode);
        }

        if (request.SectorId is not null)
        {
            institutions = institutions.Where(i => i.SectorId == request.SectorId);
        }

        if (request.VendorId is not null)
        {
            institutions = institutions.Where(i => i.ActiveVendorId == request.VendorId);
        }
        
        if(request.Q is not null)
        {
            institutions = institutions.Where(i => i.Name.Contains(request.Q));
        }

        return await institutions
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<Vendor> GetActiveVendorIdByIdAsync(Guid institutionId)
    {
        var institution = await _institutionRepository.GetAll()
            .FirstOrDefaultAsync(i => i.Id == institutionId);
            
        if (institution is null)
        {
            throw new NotFoundException($"InstitutionNotFound: {institutionId}");
        }

        return await _vendorService.GetByIdAsync(institution.ActiveVendorId);
    }

    public async Task UpdateAsync(UpdateInstitutionCommand request)
    {
        var institution = await _institutionRepository.GetAll()
            .FirstOrDefaultAsync(i => i.Id == request.InstitutionId);

        if (institution is null)
        {
            throw new NotFoundException($"InstitutionNotFound: {request.InstitutionId}");
        }

        institution.ActiveVendorId = request.ActiveVendorId;
        institution.RecordStatus = request.RecordStatus;

        await _institutionRepository.UpdateAsync(institution);

        await _auditLogService.AuditLogAsync(
            new AuditLog
            {
                IsSuccess = true,
                LogDate = DateTime.Now,
                Operation = "UpdateInstitution",
                SourceApplication = "Billing",
                Resource = "Institution",
                Details = new Dictionary<string, string>
                {
                      {"InstitutionId", request.InstitutionId.ToString()},
                      { "Name", institution.Name }
                 }
            });
    }

    public async Task<InstitutionDto> GetByIdAsync(Guid institutionId)
    {
        var institution = await _institutionRepository.GetAll()
            .Include(i => i.Sector)
            .Include(i => i.ActiveVendor)
            .Include(i => i.Fields)
            .Select(i => new InstitutionDto
            {
                Id = i.Id,
                Name = i.Name,
                SectorId = i.SectorId,
                SectorName = i.Sector.Name,
                ActiveVendorId = i.ActiveVendorId,
                ActiveVendorName = i.ActiveVendor.Name,
                FieldRequirementType = i.FieldRequirementType,
                OperationMode = i.OperationMode,
                RecordStatus = i.RecordStatus,
                Fields = i.Fields
            })
            .FirstOrDefaultAsync(i => i.Id == institutionId);

        if (institution is null)
        {
            throw new NotFoundException($"InstitutionNotFound: {institutionId}");
        }

        return institution;
    }
}