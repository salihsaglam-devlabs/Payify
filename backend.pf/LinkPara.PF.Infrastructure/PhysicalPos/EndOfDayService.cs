using System.Transactions;
using AutoMapper;
using ClosedXML.Excel;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces.PhysicalPos;
using LinkPara.PF.Application.Features.MerchantTransactions;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation;
using LinkPara.PF.Application.Features.PhysicalPos.Reconciliation.Queries.GetAllPhysicalPosEndOfDay;
using LinkPara.PF.Application.Features.PhysicalPos.UnacceptableTransaction;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.PF.Infrastructure.PhysicalPos;

public class EndOfDayService : IEndOfDayService
{
    private readonly IGenericRepository<PhysicalPosUnacceptableTransaction> _unacceptableTransactionRepository;
    private readonly IGenericRepository<MerchantTransaction> _merchantTransactionRepository;
    private readonly IGenericRepository<PhysicalPosEndOfDay> _physicalPosEndOfDayRepository;
    private readonly IGenericRepository<PhysicalPosReconciliationTransaction> _reconciliationTransactionRepository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly PfDbContext _dbContext;

    public EndOfDayService(
        IGenericRepository<PhysicalPosUnacceptableTransaction> unacceptableTransactionRepository,
        IGenericRepository<MerchantTransaction> merchantTransactionRepository,
        IGenericRepository<PhysicalPosEndOfDay> physicalPosEndOfDayRepository,
        IGenericRepository<PhysicalPosReconciliationTransaction> reconciliationTransactionRepository,
        IMapper mapper, IAuditLogService auditLogService, PfDbContext dbContext, IContextProvider contextProvider)
    {
        _unacceptableTransactionRepository = unacceptableTransactionRepository;
        _merchantTransactionRepository = merchantTransactionRepository;
        _physicalPosEndOfDayRepository = physicalPosEndOfDayRepository;
        _reconciliationTransactionRepository = reconciliationTransactionRepository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _dbContext = dbContext;
        _contextProvider = contextProvider;
    }

    public async Task<PaginatedList<PhysicalPosEndOfDayDto>> GetAllPhysicalPosEndOfDayAsync(
        GetAllPhysicalPosEndOfDayQuery request)
    {
        var endOfDayList = _physicalPosEndOfDayRepository.GetAll().AsQueryable();

        if (request.MerchantId is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.BatchId is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.BatchId == request.BatchId);
        }

        if (request.PosMerchantId is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.PosMerchantId == request.PosMerchantId);
        }

        if (request.PosTerminalId is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.PosTerminalId == request.PosTerminalId);
        }

        if (request.DateStart is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.Date
                                                   >= request.DateStart);
        }

        if (request.DateEnd is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.Date
                                                   <= request.DateEnd);
        }

        if (request.SaleCount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.SaleCount == request.SaleCount);
        }

        if (request.VoidCount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.VoidCount == request.VoidCount);
        }

        if (request.RefundCount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.RefundCount == request.RefundCount);
        }

        if (request.InstallmentSaleCount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.InstallmentSaleCount == request.InstallmentSaleCount);
        }

        if (request.FailedCount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.FailedCount == request.FailedCount);
        }

        if (request.SaleAmount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.SaleAmount == request.SaleAmount);
        }

        if (request.VoidAmount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.VoidAmount == request.VoidAmount);
        }

        if (request.RefundAmount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.RefundAmount == request.RefundAmount);
        }

        if (request.InstallmentSaleAmount is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.InstallmentSaleAmount == request.InstallmentSaleAmount);
        }

        if (request.Currency is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.Currency == request.Currency);
        }

        if (request.InstitutionId is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.InstitutionId == request.InstitutionId);
        }

        if (request.Status is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.Status == request.Status);
        }

        if (request.Vendor is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.Vendor == request.Vendor);
        }

        if (request.SerialNumber is not null)
        {
            endOfDayList = endOfDayList.Where(b => b.SerialNumber == request.SerialNumber);
        }

        return await endOfDayList
            .PaginatedListWithMappingAsync<PhysicalPosEndOfDay, PhysicalPosEndOfDayDto>(
                _mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task<PhysicalPosEndOfDayDetailResponse> GetDetailsByIdAsync(Guid id)
    {
        var endOfDay = await _physicalPosEndOfDayRepository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == id);

        if (endOfDay is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), id);
        }

        var unacceptableTransactions = await _unacceptableTransactionRepository.GetAll()
            .Where(s => s.PhysicalPosEodId == endOfDay.Id).ToListAsync();

        var reconciliationTransactions = await _reconciliationTransactionRepository.GetAll()
            .Where(s => s.PhysicalPosEodId == endOfDay.Id).ToListAsync();

        var merchantTransactions = await _merchantTransactionRepository.GetAll()
            .Where(s => s.PhysicalPosEodId == endOfDay.Id).ToListAsync();

        return new PhysicalPosEndOfDayDetailResponse
        {
            EndOfDay = _mapper.Map<PhysicalPosEndOfDayDto>(endOfDay),
            RelatedUnacceptableTransactions =
                _mapper.Map<List<PhysicalPosUnacceptableTransactionDto>>(unacceptableTransactions),
            RelatedMerchantTransactions = _mapper.Map<List<MerchantTransactionDto>>(merchantTransactions),
            RelatedReconciliationTransactions =
                _mapper.Map<List<ReconciliationTransactionDto>>(reconciliationTransactions)
        };
    }

    public async Task<IActionResult> DownloadEndOfDayDetailByIdAsync(Guid id)
    {
        var model = await GetDetailsByIdAsync(id);

        using var workbook = new XLWorkbook();

        AddSheet(workbook, "End Of Day", new List<PhysicalPosEndOfDayDto> { model.EndOfDay });

        if (model.RelatedUnacceptableTransactions?.Any() == true)
            AddSheet(workbook, "Unacceptable Transactions", model.RelatedUnacceptableTransactions);

        if (model.RelatedMerchantTransactions?.Any() == true)
            AddSheet(workbook, "Merchant Transactions", model.RelatedMerchantTransactions);

        if (model.RelatedReconciliationTransactions?.Any() == true)
            AddSheet(workbook, "Reconciliation Transactions", model.RelatedReconciliationTransactions);

        var stream = new MemoryStream();
        workbook.SaveAs(stream);
        stream.Position = 0;

        return new FileStreamResult(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
        {
            FileDownloadName = "reconciliation-report.xlsx"
        };
    }

    private static void AddSheet<T>(XLWorkbook workbook, string sheetName, List<T> data)
    {
        var worksheet = workbook.Worksheets.Add(sheetName);
        var properties = typeof(T).GetProperties();

        for (var i = 0; i < properties.Length; i++)
            worksheet.Cell(1, i + 1).Value = properties[i].Name;

        for (var row = 0; row < data.Count; row++)
        {
            for (var col = 0; col < properties.Length; col++)
            {
                var value = properties[col].GetValue(data[row]);
                var cell = worksheet.Cell(row + 2, col + 1);
                cell.Value = value switch
                {
                    decimal d => (double)d,
                    DateTime dt => dt,
                    null => "",
                    _ => value.ToString()
                };
            }
        }

        worksheet.Columns().AdjustToContents();
    }

    public async Task BatchManualReconciliationAsync(Guid endOfDayId)
    {
        var endOfDay = await _dbContext.PhysicalPosEndOfDay
            .FirstOrDefaultAsync(b => b.Id == endOfDayId);

        if (endOfDay is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), endOfDayId);
        }

        var unacceptableTransactions = await _dbContext.PhysicalPosUnacceptableTransaction
            .Where(s => s.PhysicalPosEodId == endOfDay.Id && s.CurrentStatus != UnacceptableTransactionStatus.Accepted)
            .CountAsync();
        if (unacceptableTransactions > 0)
        {
            throw new BatchHasUnacceptableTransactionException();
        }

        var merchantTransactions = await _dbContext.MerchantTransaction
            .Where(s => s.PhysicalPosEodId == endOfDay.Id).ToListAsync();
        var merchantTransactionIds = merchantTransactions.Select(s => s.Id).ToList();
        var bankTransactions = await _dbContext.BankTransaction.Where(s =>
                s.PhysicalPosEodId == endOfDay.Id && merchantTransactionIds.Contains(s.MerchantTransactionId))
            .ToListAsync();
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            merchantTransactions.ForEach(s =>
            {
                if (s.EndOfDayStatus == EndOfDayStatus.Suspended) return;
                if (s.BatchStatus == BatchStatus.EodPending)
                    s.BatchStatus = BatchStatus.Pending;
                s.EndOfDayStatus = EndOfDayStatus.ManualReconciliation;
            });

            bankTransactions.ForEach(s =>
            {
                if (s.EndOfDayStatus != EndOfDayStatus.Suspended)
                    s.EndOfDayStatus = EndOfDayStatus.ManualReconciliation;
            });

            endOfDay.Status = EndOfDayStatus.ManualReconciliation;

            _dbContext.MerchantTransaction.UpdateRange(merchantTransactions);
            _dbContext.BankTransaction.UpdateRange(bankTransactions);
            _dbContext.PhysicalPosEndOfDay.Update(endOfDay);

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "EndOfDayBatchManualReconciliation",
                    SourceApplication = "PF",
                    Resource = "PhysicalPos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        { "Id", endOfDayId.ToString() },
                    }
                });

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }

    public async Task SingleManualReconciliationAsync(Guid merchantTransactionId)
    {
        var merchantTransaction = await _dbContext.MerchantTransaction.FirstOrDefaultAsync(s =>
            s.Id == merchantTransactionId && s.PfTransactionSource == PfTransactionSource.PhysicalPos);
        if (merchantTransaction is null)
        {
            throw new NotFoundException(nameof(MerchantTransaction), merchantTransactionId);
        }

        var endOfDay = await _dbContext.PhysicalPosEndOfDay
            .FirstOrDefaultAsync(b => b.Id == merchantTransaction.PhysicalPosEodId);

        if (endOfDay is null)
        {
            throw new NotFoundException(nameof(PhysicalPosEndOfDay), merchantTransaction.PhysicalPosEodId);
        }

        var bankTransaction = await _dbContext.BankTransaction.Where(s =>
            s.PhysicalPosEodId == merchantTransaction.PhysicalPosEodId &&
            s.MerchantTransactionId == merchantTransactionId).FirstOrDefaultAsync();

        var isLastReconciledTransaction = await _dbContext.MerchantTransaction.Where(s =>
            s.PhysicalPosEodId == merchantTransaction.PhysicalPosEodId &&
            s.Id != merchantTransaction.Id &&
            s.EndOfDayStatus != EndOfDayStatus.Completed &&
            s.EndOfDayStatus != EndOfDayStatus.Empty &&
            s.EndOfDayStatus != EndOfDayStatus.ManualReconciliation
        ).AnyAsync();

        var strategy = _dbContext.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            if (merchantTransaction.EndOfDayStatus != EndOfDayStatus.Suspended)
            {
                if (merchantTransaction.BatchStatus == BatchStatus.EodPending)
                    merchantTransaction.BatchStatus = BatchStatus.Pending;
                merchantTransaction.EndOfDayStatus = EndOfDayStatus.ManualReconciliation;
                _dbContext.MerchantTransaction.Update(merchantTransaction);
            }

            if (bankTransaction.EndOfDayStatus != EndOfDayStatus.Suspended)
            {
                bankTransaction.EndOfDayStatus = EndOfDayStatus.ManualReconciliation;
                _dbContext.BankTransaction.Update(bankTransaction);
            }

            if (isLastReconciledTransaction)
            {
                endOfDay.Status = EndOfDayStatus.ManualReconciliation;
                _dbContext.PhysicalPosEndOfDay.Update(endOfDay);
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "EndOfDayManualReconciliation",
                    SourceApplication = "PF",
                    Resource = "PhysicalPos",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        { "MerchantTransactionId", merchantTransactionId.ToString() },
                    }
                });

            await _dbContext.SaveChangesAsync();
            scope.Complete();
        });
    }
}