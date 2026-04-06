using LinkPara.Billing.Domain.Entities;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IVendorMapper
{
    Task<InstitutionMapping> GetVendorInstitutionByInstitutionIdAsync(Guid institutionId, Guid vendorId);
    Task<Institution> GetInstitutionByVendorInstitutionIdAsync(string vendorInstitutionId, Guid vendorId);
    Task<SectorMapping> GetVendorSectorBySectorIdAsync(Guid sectorId, Guid vendorId);
    Task<Sector> GetSectorByVendorSectorNameAsync(string sectorName, Guid vendorId);

    Task<List<SectorMapping>> GetSectorMappingsByVendorAsync(Guid vendorId);
    Task AddSectorMappingAsync(SectorMapping sectorMapping);
    Task UpdateSectorMappingAsync(SectorMapping sectorMapping);
    Task<List<InstitutionMapping>> GetInstitutionMappingsByVendorAsync(Guid vendorId);
    Task AddInstitutionMappingAsync(InstitutionMapping institutionMapping);
    Task UpdateInstitutionMappingAsync(InstitutionMapping institutionMapping);
}