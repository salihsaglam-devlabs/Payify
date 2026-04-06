using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.DueProfiles;
using LinkPara.PF.Application.Features.DueProfiles.Command.DeleteDueProfile;
using LinkPara.PF.Application.Features.DueProfiles.Command.UpdateDueProfile;
using LinkPara.PF.Application.Features.DueProfiles.Queries.GetFilterDueProfile;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Infrastructure.Services
{
    public class DueProfileService : IDueProfileService
    {
        private readonly IGenericRepository<DueProfile> _repository;
        private readonly IGenericRepository<MerchantDue> _merchantDueRepository;
        private readonly IMapper _mapper;
        private readonly IAuditLogService _auditLogService;
        private readonly IContextProvider _contextProvider;
        private readonly ILogger<DueProfileService> _logger;

        public DueProfileService(
            IGenericRepository<DueProfile> repository,
            IMapper mapper,
            IAuditLogService auditLogService,
            IContextProvider contextProvider,
            ILogger<DueProfileService> logger,
            IGenericRepository<MerchantDue> merchantDueRepository)
        {
            _repository = repository;
            _mapper = mapper;
            _auditLogService = auditLogService;
            _contextProvider = contextProvider;
            _logger = logger;
            _merchantDueRepository = merchantDueRepository;
        }
        public async Task<PaginatedList<DueProfileDto>> GetFilterListAsync(GetFilterDueProfileQuery request)
        {
            var dueProfileList = _repository.GetAll()
                .AsQueryable();

            if (request.RecordStatus is not null)
            {
                dueProfileList = dueProfileList.Where(b => b.RecordStatus == request.RecordStatus);
            }

            if (!string.IsNullOrEmpty(request.Title))
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.Title.ToLower()
                                    .Contains(request.Title.ToLower()));
            }

            if (request.DueType is not null)
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.DueType == request.DueType);
            }

            if (request.AmountBiggerThan is not null)
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.Amount >= request.AmountBiggerThan);
            }
            if (request.AmountSmallerThan is not null)
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.Amount <= request.AmountSmallerThan);
            }

            if (request.Currency is not null)
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.Currency == request.Currency);
            }

            if (request.OccurenceInterval is not null)
            {
                dueProfileList = dueProfileList
                                    .Where(d => d.OccurenceInterval == request.OccurenceInterval);
            }

            if (request.IsDefault is not null)
            {
                dueProfileList = dueProfileList
                                       .Where(d => d.IsDefault == (bool)request.IsDefault);
            }

            return await dueProfileList
            .PaginatedListWithMappingAsync<DueProfile, DueProfileDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }
        public async Task<DueProfileDto> GetByIdAsync(Guid id)
        {
            var dueProfile = await _repository.GetAll()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (dueProfile is null)
            {
                throw new NotFoundException(nameof(DueProfile), id);
            }

            var dueProfileDto = _mapper.Map<DueProfileDto>(dueProfile);


            return dueProfileDto;
        }
        public async Task UpdateAsync(UpdateDueProfileCommand command)
        {
            var dueProfile = await _repository.GetByIdAsync(command.Id);

            if (dueProfile is null)
            {
                throw new NotFoundException(nameof(DueProfile));
            }

            try
            {
                dueProfile.Title = command.Title;
                dueProfile.Currency = command.Currency;
                dueProfile.OccurenceInterval = command.OccurenceInterval;
                dueProfile.Amount = command.Amount;

                await _repository.UpdateAsync(dueProfile);

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                await _auditLogService.AuditLogAsync(
                  new AuditLog
                  {
                      IsSuccess = true,
                      LogDate = DateTime.Now,
                      Operation = "UpdateDueProfile",
                      SourceApplication = "PF",
                      Resource = "DueProfile",
                      UserId = parseUserId,
                      Details = new Dictionary<string, string>
                      {
                        {"Id", command.Id.ToString() },
                        {"Title", command.Title.ToString() },
                        {"Currency", command.Currency.ToString() },
                        {"Amount", command.Amount.ToString() },
                        {"OccuranceInterval", command.OccurenceInterval.ToString() }
                      }
                  });
            }
            catch (Exception exception)
            {
                _logger.LogError($"DueProfileUpdateError: {exception}");
            }
        }
        public async Task DeleteAsync(DeleteDueProfileCommand command)
        {
            var dueProfile = await _repository.GetAll()
            .FirstOrDefaultAsync(b => b.Id == command.Id);

            if (dueProfile is null)
            {
                throw new NotFoundException(nameof(DueProfile), command.Id);
            }
            var isAnyActiveDueMerchant = await _merchantDueRepository.GetAll()
                .AnyAsync(s => s.DueProfileId == dueProfile.Id && s.RecordStatus == RecordStatus.Active);

            if (isAnyActiveDueMerchant)
            {
                throw new DueInUseException();
            }
            try
            {
                if (dueProfile.IsDefault)
                {
                    throw new CannotDeleteDefaultDueProfileException();
                }
                dueProfile.RecordStatus = RecordStatus.Passive;

                await _repository.UpdateAsync(dueProfile);

                var userId = _contextProvider.CurrentContext.UserId;
                var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "DeleteDueProfile",
                        SourceApplication = "PF",
                        Resource = "DueProfile",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                            {"Id", command.Id.ToString() },
                        }
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError($"DeleteDueProfileError : {exception}");
                throw;
            }
        }
    }
}
