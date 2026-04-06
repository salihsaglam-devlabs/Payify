using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Infrastructure.Services.Mapping;

public class VendorMapperService : IVendorMapper
{
    private readonly IGenericRepository<InstitutionMapping> _institutionMappingRepository;
    private readonly IGenericRepository<SectorMapping> _sectorMappingRepository;

    public VendorMapperService(IGenericRepository<InstitutionMapping> institutionMappingRepository,
        IGenericRepository<SectorMapping> sectorMappingRepository)
    {
        _institutionMappingRepository = institutionMappingRepository;
        _sectorMappingRepository = sectorMappingRepository;
    }

    public async Task<Institution> GetInstitutionByVendorInstitutionIdAsync(string vendorInstitutionId, Guid vendorId)
    {
        var institutionMapping = await _institutionMappingRepository.GetAll()
            .Include(i => i.Vendor)
            .Include(i => i.Institution)
            .FirstOrDefaultAsync(i => i.VendorInstitutionId == vendorInstitutionId && i.Vendor.Id == vendorId);

        if (institutionMapping == null)
        {
            throw new InvalidExternalInstitutionException(null, vendorInstitutionId);
        }

        return institutionMapping.Institution;
    }

    public async Task<Sector> GetSectorByVendorSectorNameAsync(string sectorName, Guid vendorId)
    {
        var sectorMapping = await _sectorMappingRepository.GetAll()
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.VendorSectorId == sectorName && s.Vendor.Id == vendorId);

        if (sectorMapping == null)
        {
            throw new InvalidExternalSectorException(null, sectorName);
        }

        return sectorMapping.Sector;
    }

    public async Task<List<SectorMapping>> GetSectorMappingsByVendorAsync(Guid vendorId) => await _sectorMappingRepository.GetAll()
        .Where(s => s.VendorId == vendorId)
        .Include(s => s.Sector)
        .ToListAsync();


    public async Task<InstitutionMapping> GetVendorInstitutionByInstitutionIdAsync(Guid institutionId, Guid vendorId)
    {
        var vendorInstitution = await _institutionMappingRepository.GetAll()
            .Include(i => i.Vendor)
            .FirstOrDefaultAsync(i => i.InstitutionId == institutionId && i.Vendor.Id == vendorId);

        if (vendorInstitution == null)
        {
            throw new InvalidInstitutionMappingException(null, institutionId);
        }

        return vendorInstitution;
    }

    public async Task<SectorMapping> GetVendorSectorBySectorIdAsync(Guid sectorId, Guid vendorId)
    {
        var sectorMapping = await _sectorMappingRepository.GetAll()
            .Include(s => s.Vendor)
            .FirstOrDefaultAsync(s => s.SectorId == sectorId && s.Vendor.Id == vendorId);

        if (sectorMapping == null)
        {
            throw new InvalidSectorMappingException(null, sectorId);
        }

        return sectorMapping;
    }

    public async Task AddSectorMappingAsync(SectorMapping sectorMapping) => await _sectorMappingRepository.AddAsync(sectorMapping);

    public async Task UpdateSectorMappingAsync(SectorMapping sectorMapping) => await _sectorMappingRepository.UpdateAsync(sectorMapping);

    public async Task<List<InstitutionMapping>> GetInstitutionMappingsByVendorAsync(Guid vendorId) => await _institutionMappingRepository.GetAll()
        .Where(i => i.VendorId == vendorId)
        .Include(i => i.Institution)
        .ToListAsync();

    public async Task AddInstitutionMappingAsync(InstitutionMapping institutionMapping) => await _institutionMappingRepository.AddAsync(institutionMapping);

    public async Task UpdateInstitutionMappingAsync(InstitutionMapping institutionMapping) => await _institutionMappingRepository.UpdateAsync(institutionMapping);
}