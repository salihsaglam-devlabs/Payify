using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Vendors;
using LinkPara.Billing.Application.Features.Vendors.Commands;
using LinkPara.Billing.Application.Features.Vendors.Queries.GetAllVendor;
using LinkPara.Billing.Domain.Entities;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Infrastructure.Services.VendorServices;

public class VendorService : IVendorService
{
    private readonly IGenericRepository<Vendor> _vendorRepository;
    private readonly IMapper _mapper;
    private readonly IApplicationUserService _applicationUserService;

    public VendorService(IGenericRepository<Vendor> vendorRepository, IMapper mapper, IApplicationUserService applicationUserService)
    {
        _vendorRepository = vendorRepository;
        _mapper = mapper;
        _applicationUserService = applicationUserService;
    }

    public async Task<Vendor> GetByIdAsync(Guid vendorId) => await _vendorRepository.GetByIdAsync(vendorId);

    public async Task<Vendor> GetByNameAsync(string name) => await _vendorRepository.GetAll()
            .FirstOrDefaultAsync(v => v.Name == name);

    public async Task<PaginatedList<VendorDto>> GetAllAsync(GetAllVendorQuery request)
    {
        var vendors = _vendorRepository.GetAll();

        if (request.RecordStatus is not null)
        {
            vendors = vendors.Where(v => v.RecordStatus == request.RecordStatus);
        }

        return await vendors
            .PaginatedListWithMappingAsync<Vendor,VendorDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveVendorCommand request)
    {
        var vendor = await _vendorRepository.GetAll()
            .FirstOrDefaultAsync(v => v.Name == request.Name);

        if (vendor is not null)
        {
            throw new DuplicateRecordException();
        }

        await _vendorRepository.AddAsync(new Vendor
        {
            Name = request.Name,
            RecordStatus = request.RecordStatus,
            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
        });
    }
}