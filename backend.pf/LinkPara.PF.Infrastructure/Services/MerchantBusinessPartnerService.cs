using AutoMapper;
using LinkPara.Audit;
using LinkPara.Audit.Models;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Fraud;
using LinkPara.HttpProviders.Fraud.Models;
using LinkPara.HttpProviders.Fraud.Models.Enums;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantBusinessPartners;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.SaveMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Command.UpdateMerchantBusinessPartner;
using LinkPara.PF.Application.Features.MerchantBusinessPartners.Queries.GetAllMerchantBusinessPartner;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Boa.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.DirectoryServices.Protocols;

namespace LinkPara.PF.Infrastructure.Services
{
    public class MerchantBusinessPartnerService : IMerchantBusinessPartnerService
    {
        private readonly IGenericRepository<MerchantBusinessPartner> _merchantBusinessPartnerRepository;
        private readonly IGenericRepository<Merchant> _merchantRepository;
        private readonly IContextProvider _contextProvider;
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<MerchantBusinessPartnerService> _logger;
        private readonly IMapper _mapper;
        private readonly ISearchService _searchService;
        private readonly IParameterService _parameterService;
        private readonly IStringLocalizer _localizer;
        private readonly IVaultClient _vaultClient;
        public MerchantBusinessPartnerService(IGenericRepository<MerchantBusinessPartner> merchantBusinessPartnerRepository,
                                              IMapper mapper,
                                              IGenericRepository<Merchant> merchantRepository,
                                              IContextProvider contextProvider,
                                              IAuditLogService auditLogService,
                                              ILogger<MerchantBusinessPartnerService> logger,
                                              ISearchService searchService,
                                              IParameterService parameterService,
                                              IStringLocalizerFactory factory,
                                              IVaultClient vaultClient)
        {
            _merchantBusinessPartnerRepository = merchantBusinessPartnerRepository;
            _mapper = mapper;
            _merchantRepository = merchantRepository;
            _contextProvider = contextProvider;
            _auditLogService = auditLogService;
            _logger = logger;
            _searchService = searchService;
            _parameterService = parameterService;
            _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
            _vaultClient = vaultClient;
        }

        public async Task<PaginatedList<MerchantBusinessPartnerDto>> GetAllAsync(GetAllMerchantBusinessPartnerQuery request)
        {
            var merchantBusinessPartners = _merchantBusinessPartnerRepository.GetAll()
                                                                              .Where(x => x.RecordStatus == RecordStatus.Active);

            if (request.MerchantId is not null)
            {
                merchantBusinessPartners = merchantBusinessPartners.Where(b => b.MerchantId == request.MerchantId);
            }

            return await merchantBusinessPartners
              .PaginatedListWithMappingAsync<MerchantBusinessPartner, MerchantBusinessPartnerDto>(_mapper, request.Page, request.Size, request.OrderBy, request.SortBy);
        }

        public async Task<MerchantBusinessPartnerDto> GetByIdAsync(Guid id)
        {
            var merchantBusinessPartner = await _merchantBusinessPartnerRepository.GetByIdAsync(id);

            if (merchantBusinessPartner is null)
            {
                throw new NotFoundException(nameof(MerchantBusinessPartner), id);
            }

            var map = _mapper.Map<MerchantBusinessPartnerDto>(merchantBusinessPartner);

            return map;
        }

        public async Task SaveAsync(SaveMerchantBusinessPartnerCommand request)
        {
            var merchant = await _merchantRepository.GetByIdAsync(request.MerchantId);

            if (merchant is null)
            {
                throw new NotFoundException(nameof(Merchant), request.MerchantId);
            }

            var maxCount = await _merchantBusinessPartnerRepository.GetAll()
                                             .Where(b => b.MerchantId == request.MerchantId
                                              && b.RecordStatus == RecordStatus.Active)
                                             .ToListAsync();

            if (maxCount.Count >= 5)
            {
                throw new MerchantBusinessPartnerMaxCountException();
            }

            var activeUser = await _merchantBusinessPartnerRepository.GetAll()
                                                                     .FirstOrDefaultAsync(b =>
                                                                     ((b.PhoneNumber == request.PhoneNumber
                                                                      || b.Email == request.Email
                                                                      || b.IdentityNumber == request.IdentityNumber)
                                                                      && b.MerchantId == request.MerchantId
                                                                      && b.RecordStatus == RecordStatus.Active));

            if (activeUser is not null)
            {
                throw new DuplicateRecordException();
            }

            var IsBlacklistCheckEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");
            string amlReferenceNumber = null;
            if (IsBlacklistCheckEnabled)
            {
                SearchByNameRequest searchRequest = new()
                {
                    Name = $"{request.FirstName} {request.LastName}",
                    BirthYear = request.BirthDate.Year.ToString(),
                    SearchType = SearchType.Corporate,
                    FraudChannelType = FraudChannelType.Backoffice
                };

                var res = await UserBlacklistControlAsync(searchRequest);
                amlReferenceNumber = res;
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            try
            {
                var merchantBusinessPartner = _mapper.Map<MerchantBusinessPartner>(request);
                merchantBusinessPartner.RecordStatus = RecordStatus.Active;
                merchantBusinessPartner.AmlReferenceNumber = amlReferenceNumber;

                await _merchantBusinessPartnerRepository.AddAsync(merchantBusinessPartner);

                await _auditLogService.AuditLogAsync(
                new AuditLog
                {
                    IsSuccess = true,
                    LogDate = DateTime.Now,
                    Operation = "CreateMerchantBusinessPartner",
                    SourceApplication = "PF",
                    Resource = "MerchantBusinessPartner",
                    UserId = parseUserId,
                    Details = new Dictionary<string, string>
                    {
                        {"Id", merchantBusinessPartner.Id.ToString()},
                        {"FullName", $"{request.FirstName} {request.LastName}"},
                        {"PhoneNumber", request.PhoneNumber},
                    }
                });
            }
            catch (Exception exception)
            {
                _logger.LogError($"MerchantBusinessPartnerCreateError : {exception}");
                throw;
            }
        }

        public async Task UpdateAsync(UpdateMerchantBusinessPartnerCommand request)
        {
            var merchantBusinessPartner = await _merchantBusinessPartnerRepository.GetByIdAsync(request.Id);

            if (merchantBusinessPartner is null)
            {
                throw new NotFoundException(nameof(MerchantBusinessPartner), request.Id);
            }

            var activeUser = await _merchantBusinessPartnerRepository.GetAll()
                                                         .FirstOrDefaultAsync(b =>
                                                         ((b.PhoneNumber == request.PhoneNumber
                                                          || b.Email == request.Email
                                                          || b.IdentityNumber == request.IdentityNumber)
                                                          && b.MerchantId == request.MerchantId
                                                          && b.RecordStatus == RecordStatus.Active)
                                                          && b.Id != request.Id);

            if (activeUser is not null)
            {
                throw new DuplicateRecordException();
            }

            var IsBlacklistCheckEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "BlacklistEnabled");
            string amlReferenceNumber = merchantBusinessPartner.AmlReferenceNumber;
            if (IsBlacklistCheckEnabled)
            {
                var oldMerchantBusinessPartner = $"{merchantBusinessPartner.FirstName}{merchantBusinessPartner.LastName}";

                if (!oldMerchantBusinessPartner.Equals($"{request.FirstName}{request.LastName}"))
                {
                    SearchByNameRequest searchRequest = new()
                    {
                        Name = $"{request.FirstName} {request.LastName}",
                        BirthYear = request.BirthDate.Year.ToString(),
                        SearchType = SearchType.Corporate,
                        FraudChannelType = FraudChannelType.Backoffice
                    };

                    amlReferenceNumber = await UserBlacklistControlAsync(searchRequest);
                }
            }

            var IsOngoingEnabled = _vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "OngoingEnabled");
            if (request.RecordStatus == RecordStatus.Passive && merchantBusinessPartner.RecordStatus == RecordStatus.Active && IsOngoingEnabled)
            {
                var removeOngoing = await _searchService.RemoveOngoingMonitoringAsync(merchantBusinessPartner.AmlReferenceNumber);
                if (!removeOngoing.IsSuccess)
                {
                    _logger.LogError($"RemoveOngoingMonitoringError : {removeOngoing.ErrorCode + removeOngoing.ErrorMessage}");
                }
            }

            var userId = _contextProvider.CurrentContext.UserId;
            var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

            try
            {
                merchantBusinessPartner = _mapper.Map(request, merchantBusinessPartner);
                merchantBusinessPartner.AmlReferenceNumber = amlReferenceNumber;

                await _merchantBusinessPartnerRepository.UpdateAsync(merchantBusinessPartner);

                await _auditLogService.AuditLogAsync(
                    new AuditLog
                    {
                        IsSuccess = true,
                        LogDate = DateTime.Now,
                        Operation = "UpdateMerchantBusinessPartner",
                        SourceApplication = "PF",
                        Resource = "MerchantBusinessPartner",
                        UserId = parseUserId,
                        Details = new Dictionary<string, string>
                        {
                            {"Id", merchantBusinessPartner.Id.ToString()},
                            {"FullName", $"{request.FirstName} {request.LastName}"},
                            {"PhoneNumber", request.PhoneNumber},
                        }
                    });
            }
            catch (Exception exception)
            {
                _logger.LogError($"MerchantBusinessPartnerUpdateError : {exception}");
                throw;
            }
        }
        private async Task<string> UserBlacklistControlAsync(SearchByNameRequest searchRequest)
        {
            var matchRate = await _parameterService.GetParameterAsync("FraudParameters", "MatchRate");

            var blackListTitleControl = await _searchService.GetSearchByName(searchRequest);

            if ((blackListTitleControl.MatchStatus == MatchStatus.PotentialMatch || blackListTitleControl.MatchStatus == MatchStatus.TruePositiveReject) && blackListTitleControl.MatchRate >= Convert.ToInt32(matchRate.ParameterValue))
            {
                var informationMail = await _parameterService.GetParameterAsync("FraudParameters", "InfoEmail");

                var exceptionMessage = _localizer.GetString("UserInBlacklistException");

                throw new UserInBlacklistException(exceptionMessage.Value.Replace("@@informationMail", informationMail.ParameterValue));
            }

            return blackListTitleControl.ReferenceNumber;
        }
    }
}
