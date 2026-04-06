using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.PF.Application.Features.MerchantLimits.Command.SaveMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Command.UpdateMerchantLimit;
using LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services
{
    public class MerchantLimitService : IMerchantLimitService
    {

        private readonly IGenericRepository<MerchantLimit> _merchantLimitRepository;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<MerchantBusinessPartnerService> _logger;
        private readonly IContextProvider _contextProvider;
        public MerchantLimitService(IGenericRepository<MerchantLimit> merchantLimitRepository,
                                    IMapper mapper,
                                    IAuditLogService auditLogService,
                                     ILogger<MerchantBusinessPartnerService> logger,
                                     IContextProvider contextProvider)
        {
            _merchantLimitRepository = merchantLimitRepository;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _logger = logger;
            _contextProvider = contextProvider;
        }
        public async Task<PaginatedList<MerchantLimitDto>> GetFilterListAsync(GetFilterMerchantLimitsQuery request)
        {
            var merchantLimitList = _merchantLimitRepository.GetAll()
                               .Where(x => x.MerchantId == request.MerchantId
                                      && x.RecordStatus == RecordStatus.Active)
                               .AsQueryable();

            return await merchantLimitList
                .PaginatedListWithMappingAsync<MerchantLimit, MerchantLimitDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }

        public async Task SaveAsync(SaveMerchantLimitCommand request)
        {
            var merchantLimit = await _merchantLimitRepository.GetAll()
                               .Where(x => x.MerchantId == request.MerchantId
                                      && x.RecordStatus == RecordStatus.Active
                                      && x.Period == request.Period
                                      && x.TransactionLimitType == request.TransactionLimitType
                                      && x.LimitType == request.LimitType)
                               .FirstOrDefaultAsync();

            if (merchantLimit is not null)
            {
                throw new DuplicateMerchantLimitException();
            }

            try
            {

                var merchantLimitPeriod = await _merchantLimitRepository.GetAll()
                   .Where(x => x.MerchantId == request.MerchantId
                          && x.RecordStatus == RecordStatus.Active
                          && x.Period != request.Period
                          && x.TransactionLimitType == request.TransactionLimitType
                          && x.LimitType == request.LimitType)
                   .FirstOrDefaultAsync();

                if (merchantLimitPeriod is not null)
                {
                    if (request.Period == Domain.Enums.Period.Daily && (request.MaxAmount > merchantLimitPeriod.MaxAmount
                        || request.MaxPiece> merchantLimitPeriod.MaxPiece))
                    {
                        throw new MerchantLimitDailyMaxAmountExceededException();
                    }
                    if (request.Period == Domain.Enums.Period.Monthly && (request.MaxAmount < merchantLimitPeriod.MaxAmount
                        || request.MaxPiece < merchantLimitPeriod.MaxPiece))
                    {
                        throw new MerchantLimitMonthlyMaxAmountExceededException();
                    }
                }

                var saveMerchantLimit = _mapper.Map<MerchantLimit>(request);
                saveMerchantLimit.RecordStatus = RecordStatus.Active;

                await _merchantLimitRepository.AddAsync(saveMerchantLimit);

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "CreateMerchantLimit",
                    SourceApplication = "PF",
                    Resource = "MerchantLimit",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", saveMerchantLimit.Id.ToString()},
                        {"Period", $"{request.Period.ToString()}"},
                        {"TransactionType", request.TransactionLimitType.ToString()}
                    }
                });
            }
            catch (Exception exception)
            {
                _logger.LogError($"MerchantLimitCreateError : {exception}");
                throw;
            }
        }

        public async Task UpdateAsync(UpdateMerchantLimitCommand request)
        {
            var merchantLimit = await _merchantLimitRepository.GetByIdAsync(request.Id);

            if (merchantLimit is null)
            {
                throw new NotFoundException(nameof(MerchantLimit), request.Id);
            }

            var activeMerchantLimit = await _merchantLimitRepository.GetAll()
                                                         .FirstOrDefaultAsync(x =>
                                                          x.MerchantId == request.MerchantId
                                                          && x.RecordStatus == RecordStatus.Active
                                                          && x.Period == request.Period
                                                          && x.TransactionLimitType == request.TransactionLimitType
                                                          && x.LimitType == request.LimitType
                                                          && x.Id != request.Id);

            if (activeMerchantLimit is not null)
            {
                throw new DuplicateMerchantLimitException();
            }

            var merchantLimitPeriod = await _merchantLimitRepository.GetAll()
                  .Where(x => x.MerchantId == request.MerchantId
                         && x.RecordStatus == RecordStatus.Active
                         && x.Period != request.Period
                         && x.TransactionLimitType == request.TransactionLimitType
                         && x.LimitType == request.LimitType
                         && x.Id != request.Id)
                  .FirstOrDefaultAsync();

            if (merchantLimitPeriod is not null)
            {
                if (request.Period == Domain.Enums.Period.Daily && (request.MaxAmount > merchantLimitPeriod.MaxAmount
                    || request.MaxPiece > merchantLimitPeriod.MaxPiece))
                {
                    throw new MerchantLimitDailyMaxAmountExceededException();
                }
                if (request.Period == Domain.Enums.Period.Monthly && (request.MaxAmount < merchantLimitPeriod.MaxAmount
                    || request.MaxPiece < merchantLimitPeriod.MaxPiece))
                {
                    throw new MerchantLimitMonthlyMaxAmountExceededException();
                }
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            try
            {
                merchantLimit = _mapper.Map(request, merchantLimit);

                await _merchantLimitRepository.UpdateAsync(merchantLimit);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdateMerchantLimit",
                        SourceApplication = "PF",
                        Resource = "MerchantLimit",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                            {"Id", merchantLimit.Id.ToString()},
                            {"Period", $"{request.Period.ToString()}"},
                            {"TransactionType", request.TransactionLimitType.ToString()}
                        }
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError($"MerchantLimitUpdateError : {exception}");
                throw;
            }
        }
    }
}
