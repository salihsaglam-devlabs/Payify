using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Billing.Application.Commons.Exceptions;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.Commissions;
using LinkPara.Billing.Application.Features.Commissions.Commands.CreateCommission;
using LinkPara.Billing.Application.Features.Commissions.Commands.SaveCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetAllCommission;
using LinkPara.Billing.Application.Features.Commissions.Queries.GetByDetail;
using LinkPara.Billing.Domain.Entities;
using LinkPara.Billing.Domain.Enums;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Infrastructure.Services.CommissionServices;

public class CommissionService : ICommissionService
{
    private readonly IGenericRepository<Commission> _commissionRepository;
    private readonly IInstitutionService _institutionService;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IParameterService _parameterService;
    private readonly IContextProvider _contextProvider;

    public CommissionService(IGenericRepository<Commission> commissionRepository,
        IInstitutionService institutionService,
        IMapper mapper,
        IAuditLogService auditLogService,
        IApplicationUserService applicationUserService,
        IParameterService parameterService,
        IContextProvider contextProvider)
    {
        _commissionRepository = commissionRepository;
        _institutionService = institutionService;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _parameterService = parameterService;
        _contextProvider = contextProvider;
    }

    public async Task AddAsync(CreateCommissionQuery request)
    {
        var existedCommissions = await _commissionRepository
            .GetAll()
            .Where(c =>
                c.InstitutionId == request.InstitutionId &&
                c.VendorId == request.VendorId &&
                c.PaymentType == request.PaymentType &&
                c.RecordStatus == RecordStatus.Active
            ).ToListAsync();

        CheckIfCommissionExists(existedCommissions, request);

        var commission = new Commission
        {
            VendorId = request.VendorId,
            InstitutionId = request.InstitutionId,
            Fee = request.Fee,
            Rate = request.Rate,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            PaymentType = request.PaymentType,
            RecordStatus = RecordStatus.Active,
            CreatedBy = _contextProvider.CurrentContext.UserId
        };

        await _commissionRepository.AddAsync(commission);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "CreateCommission",
            SourceApplication = "Billing",
            Resource = "Commission",
            Details = new Dictionary<string, string>
            {
                   { "VendorId", request.VendorId.ToString()},
                   { "InstitutionId", request.InstitutionId.ToString() },
                   { "UserId", _contextProvider.CurrentContext.UserId}
            }
        });
    }

    private void CheckIfCommissionExists(List<Commission> existedCommissions, CreateCommissionQuery request)
    {
        foreach (var item in existedCommissions)
        {
            var isOverlap =
                (request.MinValue >= item.MinValue && request.MinValue <= item.MaxValue) ||
                (request.MaxValue <= item.MaxValue && request.MaxValue >= item.MinValue) ||
                (request.MinValue >= item.MinValue && request.MaxValue <= item.MaxValue) ||
                (request.MinValue <= item.MinValue && request.MaxValue >= item.MaxValue);

            if (isOverlap && request.PaymentType == item.PaymentType)
            {
                throw new AmountIntervalIsAlreadyInUseException();
            }
        }
    }

    public async Task<CommissionWithAmountDetailDto> CalculateCommissionWithAmountDetailAsync(Guid institutionId, decimal amount, PaymentSource paymentSource)
    {
        var commission = await GetByInstitutionAsync(new GetByDetailQuery
        {
            InstitutionId = institutionId,
            Amount = amount,
            PaymentSource = paymentSource
        });

        var commissionAmount = (amount * commission.Rate / 100.00m) + commission.Fee;
        var bsmvAmount = 0.0m;

        decimal bsmvRate;
        try
        {
            var bsmvRateParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
            if (bsmvRateParameter is null || !decimal.TryParse(bsmvRateParameter.ParameterValue, out bsmvRate))
            {
                bsmvRate = 5.0m;
            }
        }
        catch (Exception)
        {
            bsmvRate = 5.0m;
        }

        if (commissionAmount > 0)
        {
            bsmvAmount = commissionAmount * (bsmvRate / 100.0m);
        }

        return new CommissionWithAmountDetailDto
        {
            Amount = amount,
            BsmvAmount = Math.Round(bsmvAmount, 4),
            CommissionAmount = Math.Round(commissionAmount, 4)
        };
    }

    public async Task DeleteAsync(Guid commissionId)
    {
        var commission = await _commissionRepository.GetByIdAsync(commissionId);

        if (commission is null)
        {
            throw new NotFoundException($"CommissionNotFound: {commissionId}");
        }

        commission.UpdateDate = DateTime.Now;
        commission.LastModifiedBy = _contextProvider.CurrentContext.UserId;
        commission.RecordStatus = RecordStatus.Passive;

        await _commissionRepository.UpdateAsync(commission);
    }

    public Task<CommissionDto> GetByIdAsync(Guid commissionId)
    {
        return _commissionRepository.GetAll()
            .Where(c =>
                c.Id == commissionId &&
                c.RecordStatus == RecordStatus.Active)
            .Select(c => new CommissionDto
            {
                Id = c.Id,
                InstitutionId = c.InstitutionId,
                InstitutionName = c.Institution.Name,
                VendorId = c.VendorId,
                VendorName = c.Vendor.Name,
                PaymentType = c.PaymentType,
                Fee = c.Fee,
                Rate = c.Rate,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue
            })
            .FirstOrDefaultAsync();
    }

    public async Task<PaginatedList<CommissionDto>> GetAllAsync(GetAllCommissionQuery request)
    {
        var commissions = _commissionRepository.GetAll()
            .Include(c => c.Institution)
                .ThenInclude(i => i.Sector)
            .Include(c => c.Vendor)
            .Select(c => new CommissionDto
            {
                Id = c.Id,
                InstitutionId = c.InstitutionId,
                InstitutionName = c.Institution.Name,
                VendorId = c.VendorId,
                VendorName = c.Vendor.Name,
                PaymentType = c.PaymentType,
                Fee = c.Fee,
                Rate = c.Rate,
                MinValue = c.MinValue,
                MaxValue = c.MaxValue,
                SectorName = c.Institution.Sector.Name,
                RecordStatus = c.RecordStatus
            });

        if (request.InstitutionId is not null)
        {
            commissions = commissions.Where(c => c.InstitutionId == request.InstitutionId);
        }

        if (request.VendorId is not null)
        {
            commissions = commissions.Where(c => c.VendorId == request.VendorId);
        }

        if (request.PaymentType is not null)
        {
            commissions = commissions.Where(c => c.PaymentType == request.PaymentType);
        }

        if (request.Q is not null)
        {
            commissions = commissions.Where(c => c.InstitutionName.Contains(request.Q) || c.VendorName.Contains(request.Q));
        }

        return await commissions
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<CommissionDto> GetByInstitutionAsync(GetByDetailQuery request)
    {
        var activeVendor = await _institutionService.GetActiveVendorIdByIdAsync(request.InstitutionId);

        var commissions = await _commissionRepository.GetAll()
            .Where(c => c.VendorId == activeVendor.Id
                        && c.PaymentType == request.PaymentSource
                        && c.MinValue <= request.Amount
                        && c.MaxValue >= request.Amount
                        && c.PaymentType == request.PaymentSource
                        && c.RecordStatus == RecordStatus.Active)
            .ToListAsync();

        var activeCommission = commissions.Find(c => c.InstitutionId == request.InstitutionId);

        if (activeCommission == null)
        {
            activeCommission = commissions.Find(c => c.InstitutionId == Guid.Empty);
        }

        if (activeCommission == null)
        {
            activeCommission = new Commission
            {
                VendorId = activeVendor.Id,
                InstitutionId = request.InstitutionId
            };
        }

        return _mapper.Map<CommissionDto>(activeCommission);
    }

    public async Task UpdateAsync(SaveCommissionCommand request)
    {
        var commission = await _commissionRepository.GetAll()
            .FirstOrDefaultAsync(c => c.Id == request.CommissionId);

        if (commission is null)
        {
            throw new NotFoundException($"CommissionNotFound: {request.CommissionId}");
        }

        commission.Fee = request.Fee;
        commission.Rate = request.Rate;
        commission.MinValue = request.MinValue;
        commission.MaxValue = request.MaxValue;
        commission.RecordStatus = request.RecordStatus;
        commission.PaymentType = request.PaymentType;

        if (request.InstitutionId != Guid.Empty)
        {
            commission.InstitutionId = request.InstitutionId;
        }

        await _commissionRepository.UpdateAsync(commission);
    }
}
