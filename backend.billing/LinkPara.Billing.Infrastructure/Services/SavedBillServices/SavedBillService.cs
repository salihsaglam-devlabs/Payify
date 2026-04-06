using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Billing.Application.Commons.Interfaces;
using LinkPara.Billing.Application.Features.SavedBills;
using LinkPara.Billing.Application.Features.SavedBills.Commands.CreateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.DeleteSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Commands.UpdateSavedBill;
using LinkPara.Billing.Application.Features.SavedBills.Queries.GetAllSavedBill;
using LinkPara.Billing.Domain.Entities;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Billing.Infrastructure.Services.SavedBillServices;

public class SavedBillService : ISavedBillService
{
    private readonly IGenericRepository<SavedBill> _savedBillRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IAuditLogService _auditLogService;
 
    public SavedBillService(IGenericRepository<SavedBill> savedBillRepository,
        IContextProvider contextProvider,
        IAuditLogService auditLogService)
    {
        _savedBillRepository = savedBillRepository;
        _contextProvider = contextProvider;
        _auditLogService = auditLogService;
    }

    public async Task DeleteAsync(DeleteSavedBillCommand request)
    {
        var savedBill = await _savedBillRepository
           .GetByIdAsync(request.Id);

        if (savedBill is null)
        {
            throw new NotFoundException($"SavedBillNotFound: {request.Id}");
        }

        await _savedBillRepository.DeleteAsync(savedBill);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "DeleteSavedBill",
            SourceApplication = "Billing",
            Resource = "SavedBill",
            Details = new Dictionary<string, string>
            {
                  {"Id", savedBill.Id.ToString()},
                  {"BillName", savedBill.BillName}
            }
        });

    }

    public async Task<PaginatedList<SavedBillDto>> GetAllAsync(GetAllSavedBillQuery request)
    {
        var userId = Guid.Parse(_contextProvider.CurrentContext.UserId);

        var savedBills = _savedBillRepository.GetAll()
            .Where(s => s.UserId == userId
                    && s.RecordStatus == RecordStatus.Active);

        if (request.InstitutionId != Guid.Empty)
        {
            savedBills = savedBills.Where(s => s.InstitutionId == request.InstitutionId);
        }

        if (!string.IsNullOrEmpty(request.SubscriberNumber1))
        {
            savedBills = savedBills.Where(s => s.SubscriberNumber1 == request.SubscriberNumber1);
        }

        if (!string.IsNullOrEmpty(request.SubscriberNumber2))
        {
            savedBills = savedBills.Where(s => s.SubscriberNumber2 == request.SubscriberNumber2);
        }

        if (!string.IsNullOrEmpty(request.SubscriberNumber3))
        {
            savedBills = savedBills.Where(s => s.SubscriberNumber3 == request.SubscriberNumber3);
        }

        if (!string.IsNullOrEmpty(request.BillName))
        {
            savedBills = savedBills.Where(s => s.BillName == request.BillName);
        }

        return await savedBills
            .Include(s => s.Institution)
            .ThenInclude(s => s.Fields)
            .Include(s => s.Institution)
            .ThenInclude(s => s.Sector)
            .Select(s => new SavedBillDto
            {
                Id = s.Id,
                BillName = s.BillName,
                InstitutionId = s.InstitutionId,
                SectorName = s.Institution.Sector.Name,
                InstitutionName = s.Institution.Name,
                SubscriberNumber1 = s.SubscriberNumber1,
                SubscriberNumber2 = s.SubscriberNumber2,
                SubscriberNumber3 = s.SubscriberNumber3,
                Fields = s.Institution.Fields
            })
            .PaginatedListAsync(request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(CreateSavedBillCommand request)
    {
        var userId = Guid.Parse(_contextProvider.CurrentContext.UserId);
        var existingBill = await _savedBillRepository.GetAll()
            .FirstOrDefaultAsync(s => s.UserId == userId
                    && s.SubscriberNumber1 == request.SubscriberNumber1
                    && s.InstitutionId == request.InstitutionId
                    && s.RecordStatus == RecordStatus.Active);
        
        if (existingBill is not null)
        {
            throw new DuplicateRecordException();
        }

        var savedBill = new SavedBill
        {
            UserId = userId,
            InstitutionId = request.InstitutionId,
            SubscriberNumber1 = request.SubscriberNumber1,
            SubscriberNumber2 = request.SubscriberNumber2,
            SubscriberNumber3 = request.SubscriberNumber3,
            BillName = request.BillName,
            CreatedBy = userId.ToString()
        };

        await _savedBillRepository.AddAsync(savedBill);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "SavedBill",
            SourceApplication = "Billing",
            Resource = "SavedBill",
            Details = new Dictionary<string, string>
            {
                  {"Id", savedBill.Id.ToString()},
                  {"BillName", savedBill.BillName}
            }
        });

    }

    public async Task UpdateAsync(UpdateSavedBillCommand request)
    {
        var savedBill = await _savedBillRepository
            .GetByIdAsync(request.Id);

        if (savedBill is null)
        {
            throw new NotFoundException($"SavedBillNotFound: {request.Id}");
        }

        savedBill.BillName = request.BillName;

        await _savedBillRepository.UpdateAsync(savedBill);

        await _auditLogService.AuditLogAsync(
        new AuditLog
        {
            IsSuccess = true,
            LogDate = DateTime.Now,
            Operation = "UpdateSavedBill",
            SourceApplication = "Billing",
            Resource = "SavedBill",
            Details = new Dictionary<string, string>
            {
                 {"Id", savedBill.Id.ToString()},
                 {"BillName", savedBill.BillName}
            }
        });

    }
}
