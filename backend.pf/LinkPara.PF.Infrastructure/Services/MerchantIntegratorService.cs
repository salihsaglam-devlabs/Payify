using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantIntegrators;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.DeleteMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.SaveMerchantIntegrator;
using LinkPara.PF.Application.Features.MerchantIntegrators.Command.UpdateMerchantIntegrator;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static MassTransit.ValidationResultExtensions;

namespace LinkPara.PF.Infrastructure.Services;

public class MerchantIntegratorService : IMerchantIntegratorService
{
    private readonly ILogger<MerchantIntegratorService> _logger;
    private readonly IGenericRepository<MerchantIntegrator> _repository;
    private readonly IMapper _mapper;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;

    public MerchantIntegratorService(ILogger<MerchantIntegratorService> logger,
        IGenericRepository<MerchantIntegrator> repository,
        IMapper mapper,
        IAuditLogService auditLogService,
        IContextProvider contextProvider)
    {
        _logger = logger;
        _repository = repository;
        _mapper = mapper;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
    }

    public async Task DeleteAsync(DeleteMerchantIntegratorCommand request)
    {
        var merchantIntegrator = await _repository.GetByIdAsync(request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (merchantIntegrator is null)
        {
            throw new NotFoundException(nameof(MerchantIntegrator), request.Id);
        }

        try
        {
            merchantIntegrator.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(merchantIntegrator);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteMerchantIntegrator",
                    SourceApplication = "PF",
                    Resource = "MerchantIntegrator",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString()},
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantIntegratorDeleteError : {exception}");
        }
    }

    public async Task<MerchantIntegratorDto> GetByIdAsync(Guid id)
    {
        var merchantIntegrator = await _repository.GetAll()
                            .FirstOrDefaultAsync(b => b.Id == id);

        if (merchantIntegrator is null)
        {
            throw new NotFoundException(nameof(MerchantIntegrator), id);
        }

        return _mapper.Map<MerchantIntegratorDto>(merchantIntegrator);
    }

    public async Task<PaginatedList<MerchantIntegratorDto>> GetListAsync(SearchQueryParams request)
    {
        var merchantIntegratorList = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            merchantIntegratorList = merchantIntegratorList.Where(b => b.Name.Contains(request.Q));
        }

        return await merchantIntegratorList
            .PaginatedListWithMappingAsync<MerchantIntegrator,MerchantIntegratorDto>(_mapper, request.Page, request.Size);
    }

    public async Task SaveAsync(SaveMerchantIntegratorCommand request)
    {
        var activeMerchantIntegrator = await _repository.GetAll()
            .FirstOrDefaultAsync(b=>b.CommissionRate == request.CommissionRate
            && b.Name.Equals(request.Name)
            && b.RecordStatus == RecordStatus.Active);

        if (activeMerchantIntegrator is not null)
        {
            throw new DuplicateRecordException(nameof(MerchantIntegrator), request.CommissionRate);
        }

        var merchantIntegrator = new MerchantIntegrator
        {
            CommissionRate = request.CommissionRate,
            Name = request.Name,
        };

        await _repository.AddAsync(merchantIntegrator);
    }

    public async Task UpdateAsync(UpdateMerchantIntegratorCommand request)
    {
        var merchantIntegrator = await _repository.GetAll()
                           .FirstOrDefaultAsync(b => b.Id == request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (merchantIntegrator is null)
        {
            throw new NotFoundException(nameof(MerchantIntegrator), request.Id);
        }       

        try
        {
            merchantIntegrator.CommissionRate = request.CommissionRate;
            merchantIntegrator.Name = request.Name;

            await _repository.UpdateAsync(merchantIntegrator);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateMerchantIntegrator",
                    SourceApplication = "PF",
                    Resource = "MerchantIntegrator",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", request.Id.ToString()},
                        {"Name", request.Name},
                        {"CommissionRate", request.CommissionRate.ToString()},
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"MerchantIntegratorUpdateError : {exception}");
        }
    }
}
