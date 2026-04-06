using AutoMapper;
using AutoMapper.QueryableExtensions;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantCategoryCodes;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.DeleteMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.SaveMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Command.UpdateMcc;
using LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetAllMcc;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static MassTransit.ValidationResultExtensions;

namespace LinkPara.PF.Infrastructure.Services;

public class MccService : IMccService
{
    private readonly IGenericRepository<Mcc> _repository;
    private readonly ICacheService _cacheService;
    private readonly IMapper _mapper;
    private readonly ILogger<MccService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IContextProvider _contextProvider;
    private readonly IGenericRepository<Merchant> _merchantRepository;

    public MccService(IGenericRepository<Mcc> repository, ICacheService cacheService,
        IMapper mapper,
        ILogger<MccService> logger,
        IAuditLogService auditLogService,
        IContextProvider contextProvider,
        IGenericRepository<Merchant> merchantRepository)
    {
        _repository = repository;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
        _auditLogService = auditLogService;
        _contextProvider = contextProvider;
        _merchantRepository = merchantRepository;
    }

    public async Task DeleteAsync(DeleteMccCommand command)
    {
        var mcc = await _repository.GetByIdAsync(command.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (mcc is null)
        {
            throw new NotFoundException(nameof(Mcc), command.Id);
        }

        var merchant = await _merchantRepository.GetAll()
            .FirstOrDefaultAsync(b => b.MccCode == mcc.Code
            && b.RecordStatus == RecordStatus.Active);

        if (merchant is not null)
        {
            throw new AlreadyInUseException(nameof(Merchant));
        }

        try
        {
            mcc.RecordStatus = RecordStatus.Passive;

            await _repository.UpdateAsync(mcc);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "DeleteMcc",
                    SourceApplication = "PF",
                    Resource = "Mcc",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", command.Id.ToString() },
                    }
                });

        }
        catch (Exception exception)
        {
            _logger.LogError($"MccDeleteError : {exception}");
        }
    }

    public async Task<MccDto> GetByCodeAsync(string mccCode)
    {
        var mcc = await _cacheService.GetOrCreateAsync(mccCode,
          async () =>
          {
              return await _repository
              .GetAll()
              .FirstOrDefaultAsync(b => b.Code == mccCode);
          });

        return _mapper.Map<MccDto>(mcc);
    }

    public async Task<MccDto> GetByIdAsync(Guid id)
    {
        var mcc = await _repository.GetAll()
                           .FirstOrDefaultAsync(b => b.Id == id);

        if (mcc is null)
        {
            throw new NotFoundException(nameof(Mcc), id);
        }

        return _mapper.Map<MccDto>(mcc);
    }

    public async Task<PaginatedList<MccDto>> GetListAsync(GetAllMccQuery request)
    {
        var mccList = _repository.GetAll();

        if (!string.IsNullOrEmpty(request.Q))
        {
            mccList = mccList.Where(b => b.Code.Contains(request.Q));
        }

        if (!string.IsNullOrEmpty(request.Name))
        {
            mccList = mccList.Where(b => b.Name.ToLower()
                             .Contains(request.Name.ToLower()));
        }

        if (request.CreateDateStart is not null)
        {
            mccList = mccList.Where(b => b.CreateDate
                               >= request.CreateDateStart);
        }

        if (request.CreateDateEnd is not null)
        {
            mccList = mccList.Where(b => b.CreateDate
                               <= request.CreateDateEnd);
        }

        if (request.RecordStatus is not null)
        {
            mccList = mccList.Where(b => b.RecordStatus
                               == request.RecordStatus);
        }

        return await mccList.OrderBy(b=>b.Code)
            .PaginatedListWithMappingAsync<Mcc,MccDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
    }

    public async Task SaveAsync(SaveMccCommand request)
    {
        var activeMcc = await _repository.GetAll().FirstOrDefaultAsync(
                               b => b.Code == request.Code);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;
        try
        {
            if (activeMcc is not null)
            {
                if (activeMcc.RecordStatus == RecordStatus.Active)
                {
                    throw new DuplicateRecordException();
                }
                else
                {
                    activeMcc = _mapper.Map(request, activeMcc);
                    activeMcc.RecordStatus = RecordStatus.Active;

                    await _repository.UpdateAsync(activeMcc);

                    await _auditLogService.AuditLogAsync(
                   new AuditLog
                   {
                       IsSuccess = true,
                       LogDate = DateTime.Now,
                       Operation = "UpdateMcc",
                       SourceApplication = "PF",
                       Resource = "Mcc",
                       UserId = parseUserId,
                       Details = new Dictionary<string, string>
                       {
                    {"Id", activeMcc.Id.ToString()},
                    {"Name", request.Name},
                    {"Code", request.Code},
                       }
                   });
                }
            }
            else
            {
                var mcc = _mapper.Map<Mcc>(request);
                mcc.Id = Guid.Empty;
                mcc.RecordStatus = RecordStatus.Active;

                await _repository.AddAsync(mcc);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "SaveMcc",
                        SourceApplication = "PF",
                        Resource = "Mcc",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                    {"Name", request.Name},
                    {"Code", request.Code},
                        }
                    });
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"MccCreateError : {exception}");
            throw;
        }
    }

    public async Task UpdateAsync(UpdateMccCommand request)
    {
        var mcc = await _repository.GetAll().FirstOrDefaultAsync(
                           b => b.Id == request.Id);

        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        if (mcc is null)
        {
            throw new NotFoundException(nameof(Mcc));
        }        

        var activeMcc = await _repository.GetAll().FirstOrDefaultAsync(
                               b => b.Code == request.Code
                               && b.Id != request.Id);

        if (activeMcc is not null)
        {
            throw new DuplicateRecordException();
        }

        try
        {
            mcc = _mapper.Map(request, mcc);

            await _repository.UpdateAsync(mcc);

            await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "UpdateMcc",
                    SourceApplication = "PF",
                    Resource = "Mcc",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                    {"Id", request.Id.ToString()},
                    {"Name", request.Name},
                    {"Code", request.Code},
                    }
                });
        }
        catch (Exception exception)
        {
            _logger.LogError($"MccUpdateError : {exception}");
            throw;
        }
    }
}
