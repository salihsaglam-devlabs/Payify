using LinkPara.Billing.Application.Features.Vendors;
using LinkPara.Billing.Application.Features.Vendors.Commands;
using LinkPara.Billing.Application.Features.Vendors.Queries.GetAllVendor;
using LinkPara.Billing.Domain.Entities;
using LinkPara.SharedModels.Pagination;

namespace LinkPara.Billing.Application.Commons.Interfaces;

public interface IVendorService
{
    Task<PaginatedList<VendorDto>> GetAllAsync(GetAllVendorQuery request);
    Task SaveAsync(SaveVendorCommand request);
    Task<Vendor> GetByIdAsync(Guid vendorId);
    Task<Vendor> GetByNameAsync(string name);
}