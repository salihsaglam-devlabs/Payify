using AutoMapper;
using LinkPara.ContextProvider;
using LinkPara.Fraud.Application.Commons.Interfaces;
using LinkPara.Fraud.Application.Commons.Models.Searchs;
using LinkPara.Fraud.Application.Features.Searchs;
using LinkPara.Fraud.Application.Features.Searchs.SearchByNames;
using LinkPara.Fraud.Application.Features.Transactions.Queries.GetAllSearches;
using LinkPara.Fraud.Domain.Entities;
using LinkPara.Fraud.Domain.Enums;
using LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi;
using LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Request;
using LinkPara.Fraud.Infrastructure.ExternalServices.SanctionScannerApi.Response;
using LinkPara.HttpProviders.Vault;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using MassTransit;
using LinkPara.Fraud.Application.Commons.Models;
using LinkPara.SharedModels.Boa.Enums;

namespace LinkPara.Fraud.Infrastructure.Services;

public class SearchService : SanctionScannerBase, ISearchService
{
    private readonly ILogger<SearchService> _logger;
    private readonly IGenericRepository<SearchLog> _logRepository;
    private readonly IGenericRepository<OngoingMonitoring> _ongoingRepository;
    private readonly IContextProvider _contextProvider;
    private readonly IMapper _mapper;
    private readonly IBus _bus;
    private readonly string username;
    private readonly string password;
    private readonly string searchBaseAddress;
    private readonly string operationBaseAddress;
    private readonly string expireDate;
    private readonly string ongoingPeriod;
    private readonly string[] disableChannelType;
    private readonly bool ongoingEnabled;

    public SearchService(
        ILogger<SearchService> logger,
        IVaultClient vaultClient,
        IGenericRepository<SearchLog> logRepository,
        IContextProvider contextProvider, IMapper mapper, IBus bus, IGenericRepository<OngoingMonitoring> ongoingRepository) : base()
    {
        _logger = logger;
        _logRepository = logRepository;
        _contextProvider = contextProvider;
        _mapper = mapper;
        username = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "SearchUsername");
        password = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "SearchPassword");
        expireDate = vaultClient.GetSecretValue<string>("FraudSecrets", "SearchConfigs", "ExpireDate");
        ongoingPeriod = vaultClient.GetSecretValue<string>("FraudSecrets", "SearchConfigs", "OngoingPeriod");
        ongoingEnabled = vaultClient.GetSecretValue<bool>("SharedSecrets", "ServiceState", "OngoingEnabled");
        disableChannelType = vaultClient.GetSecretValue<string[]>("FraudSecrets", "SearchConfigs", "DisableChannelType");
        searchBaseAddress = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "SearchBaseAddress");
        operationBaseAddress = vaultClient.GetSecretValue<string>("FraudSecrets", "SanctionScannerCredentials", "OperationBaseAddress");
        _bus = bus;
        _ongoingRepository = ongoingRepository;
    }

    public async Task<SearchResponse> GetSearchByNameAsync(SearchByNameQuery request)
    {
        try
        {
            SearchByNameRequest searchModel = new()
            {
                Name = request.Name,
                SearchType = Convert.ToInt32(request.SearchType),
                BirthYear = request.BirthYear
            };

            var url = CreateUrlWithParams($"{searchBaseAddress}{SanctionScannerInfo.SearchByName}", searchModel);
            var sendRequest = await GetAsync(url, username, password);

            SearchByNameResponse response = JsonConvert.DeserializeObject<SearchByNameResponse>(sendRequest);

            var postClientIp = await PostClientIpAddressAsync(response.Result.ReferenceNumber);

            if (ongoingEnabled)
            {
                var isOngoing = await AddOngoingAsync(response.Result.ReferenceNumber);

                if (isOngoing)
                {
                    await AddOngoingMonitoringAsync(request, response.Result.ReferenceNumber);
                }
            }            

            var searchLog = await _logRepository.GetAll()
                .OrderByDescending(l => l.CreateDate)
                .FirstOrDefaultAsync();

            var searchResponse = new SearchResponse();

            if (searchLog?.ExpireDate == null ||
                searchLog?.ExpireDate < DateTime.Now.AddDays(Double.Parse(expireDate)))
            {
                var result = await AddSearchLogAsync(response, request);
                searchResponse.MatchStatus = (MatchStatus)response.Result.MatchStatusId;
                searchResponse.MatchRate = result;
                searchResponse.ReferenceNumber = response.Result.ReferenceNumber;
            }
            else
            {
                searchResponse.MatchStatus = searchLog.MatchStatus;
                searchResponse.MatchRate = searchLog.MatchRate;
                searchResponse.ReferenceNumber = searchLog.ReferenceNumber;
            }

            if (disableChannelType.Contains(request.FraudChannelType.ToString()))
            {
                searchResponse.MatchStatus = MatchStatus.NoMatch;
            }

            return searchResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetSearcByNameError : {exception}");
            throw;
        }
    }
    private async Task<bool> PostClientIpAddressAsync(string referenceNumber)
    {
        AddMemoNotesToSearchRequest request = new()
        {
            Memo = _contextProvider.CurrentContext.ClientIpAddress,
            ScanId = referenceNumber
        };

        var response = await PostAsync($"{operationBaseAddress}{SanctionScannerInfo.AddMemoToSearch}", request, username, password);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<BaseApiResponse>(responseContent);

        return apiResponse.IsSuccess;
    }

    private async Task<bool> AddOngoingAsync(string referenceNumber)
    {
        var validPeriod = ongoingPeriod;
        if (!string.IsNullOrWhiteSpace(ongoingPeriod) && !new[] { "1", "2", "3", "4", "5", "6" }.Contains(ongoingPeriod))
        {
            validPeriod = "1";
        }

        OngoigRequest request = new()
        {
            PeriodId = validPeriod,
            ScanId = referenceNumber
        };

        var response = await PostAsync($"{operationBaseAddress}{SanctionScannerInfo.MonitoringEnable}", request, username, password);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<BaseApiResponse>(responseContent);

        var correlationId = Guid.NewGuid;
        var log = new LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.IntegrationLog()
        {
            CorrelationId = correlationId.ToString(),
            Name = "Fraud.Ongoing",
            Type = nameof(IntegrationLogType.Fraud),
            Date = DateTime.Now,
            Request = JsonConvert.SerializeObject(request),
            Response = responseContent,
            DataType = IntegrationLogDataType.Json
        };

        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
        await endpoint.Send(log, cancellationToken.Token);

        return apiResponse.IsSuccess;
    }

    private async Task AddOngoingMonitoringAsync(SearchByNameQuery request, string referenceNumber)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        var validPeriod = ongoingPeriod;
        if (!string.IsNullOrWhiteSpace(ongoingPeriod) && !new[] { "1", "2", "3", "4", "5", "6" }.Contains(ongoingPeriod))
        {
            validPeriod = "1";
        }

        var ongoigRequest = new OngoingMonitoring
        {
            ScanId = referenceNumber,
            SearchName = request.Name,
            SearchType = request.SearchType,
            CreatedBy = parseUserId.ToString(),
            IsOngoingList = true,
            Period = validPeriod switch
            {
                "1" => OngoingPeriod.Daily,
                "2" => OngoingPeriod.Weekly,
                "3" => OngoingPeriod.Monthly,
                "4" => OngoingPeriod.Quarterly,
                "5" => OngoingPeriod.HalfEarly,
                "6" => OngoingPeriod.Yearly,
                _ => OngoingPeriod.Unknown
            },
        };

        await _ongoingRepository.AddAsync(ongoigRequest);
    }
    public async Task<BaseResponse> RemoveOngoingAsync(string referenceNumber)
    {      
        OngoigDisableRequest request = new()
        {
            ScanId = referenceNumber
        };

        var response = await PostAsync($"{operationBaseAddress}{SanctionScannerInfo.MonitoringDisable}", request, username, password);

        var responseContent = await response.Content.ReadAsStringAsync();
        var apiResponse = JsonConvert.DeserializeObject<BaseApiResponse>(responseContent);

        var baseResponse = new BaseResponse
        {
            IsSuccess = true,
            HttpStatusCode = apiResponse.HttpStatusCode
        };

        if (apiResponse.IsSuccess)
        {
            var ongoing = await _ongoingRepository.GetAll().Where(b => b.ScanId == referenceNumber).FirstOrDefaultAsync();
            if (ongoing != null)
            {
                var userId = _contextProvider.CurrentContext.UserId;

                ongoing.IsOngoingList = false;
                ongoing.LastModifiedBy = userId;
                ongoing.UpdateDate = DateTime.Now;

                await _ongoingRepository.UpdateAsync(ongoing);
            }
        }
        else
        {
            baseResponse.ErrorMessage = apiResponse.ErrorMessage;
            baseResponse.ErrorCode = apiResponse.ErrorCode;
            baseResponse.IsSuccess = false;
        }

            var correlationId = Guid.NewGuid;
        var log = new LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.IntegrationLog()
        {
            CorrelationId = correlationId.ToString(),
            Name = "Fraud.Ongoing",
            Type = nameof(IntegrationLogType.Fraud),
            Date = DateTime.Now,
            Request = JsonConvert.SerializeObject(request),
            Response = responseContent,
            DataType = IntegrationLogDataType.Json
        };

        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
        await endpoint.Send(log, cancellationToken.Token);

        return baseResponse;
    }

    public async Task<SearchResponse> GetSearchByIdentityAsync(string id)
    {
        try
        {
            var sendRequest = await GetAsync($"{searchBaseAddress}{SanctionScannerInfo.SearchByIdentity}?id={id}", username, password);

            SearchByNameResponse response = JsonConvert.DeserializeObject<SearchByNameResponse>(sendRequest);

            SearchResponse searchResponse = new()
            {
                MatchStatus = (MatchStatus)response.Result.MatchStatusId
            };

            await AddSearchLogAsync(response, new SearchByNameQuery
            {
                Name = id,
                SearchType = SearchType.Individual,
                FraudChannelType = FraudChannelType.Web
            });

            return searchResponse;
        }
        catch (Exception exception)
        {
            _logger.LogError($"GetSearcByIdentityError : {exception}");
            throw;
        }
    }

    private async Task<int> AddSearchLogAsync(SearchByNameResponse response, SearchByNameQuery request)
    {
        var userId = _contextProvider.CurrentContext.UserId;
        var parseUserId = userId != null ? Guid.Parse(userId) : Guid.Empty;

        decimal maxResult = 0;
        var blackListResult = string.Empty;
        if (response.Result.Results.Length > 0)
        {
            maxResult = response.Result.Results.Max(b => b.MatchRate);
            blackListResult = response.Result.Results.FirstOrDefault(b => b.MatchRate == maxResult)?.BlacklistName;
        }

        var searchLog = new SearchLog
        {
            SearchName = request.Name,
            BirthYear = request.BirthYear,
            SearchType = request.SearchType,
            MatchRate = Convert.ToInt32(maxResult),
            IsBlackList = !string.IsNullOrEmpty(blackListResult) ? true : false,
            BlacklistName = blackListResult,
            CreatedBy = parseUserId.ToString(),
            ExpireDate = DateTime.Now.AddDays(Double.Parse(expireDate)),
            ClientIpAddress = _contextProvider.CurrentContext?.ClientIpAddress,
            ReferenceNumber = response.Result.ReferenceNumber,
            ChannelType = request.FraudChannelType.ToString(),
            MatchStatus = response.Result.MatchStatusId switch
            {
                0 => MatchStatus.Unknown,
                1 => MatchStatus.NoMatch,
                2 => MatchStatus.PotentialMatch,
                3 => MatchStatus.FalsePositive,
                4 => MatchStatus.TruePositive,
                5 => MatchStatus.TruePositiveApprove,
                6 => MatchStatus.TruePositiveReject,
                _ => MatchStatus.Unknown
            }
        };

        await _logRepository.AddAsync(searchLog);

        var correlationId = Guid.NewGuid;
        var log = new LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.IntegrationLog()
        {
            CorrelationId = correlationId.ToString(),
            Name = "Fraud.Search",
            Type = nameof(IntegrationLogType.Fraud),
            Date = DateTime.Now,
            Request = JsonConvert.SerializeObject(request),
            Response = JsonConvert.SerializeObject(response),
            DataType = IntegrationLogDataType.Json
        };

        using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
        await endpoint.Send(log, cancellationToken.Token);

        return Convert.ToInt32(maxResult);
    }

    public Task<PaginatedList<SearchLogDto>> GetListAsync(GetAllSearchesQuery query)
    {
        var searchLogList = _logRepository.GetAll();

        if (!string.IsNullOrEmpty(query.SearchName))
        {
            searchLogList = searchLogList.Where(b => b.SearchName.ToLower()
                                         .Contains(query.SearchName.ToLower()));
        }
        if (!string.IsNullOrEmpty(query.Q))
        {
            searchLogList = searchLogList.Where(b => b.ReferenceNumber.ToLower()
                                         .Contains(query.Q.ToLower()));
        }
        if (query.SearchType is not null)
        {
            searchLogList = searchLogList.Where(b => b.SearchType == query.SearchType);
        }
        if (query.MatchStatus is not null)
        {
            searchLogList = searchLogList.Where(b => b.MatchStatus == query.MatchStatus);
        }
        if (query.IsBlackList is not null)
        {
            searchLogList = searchLogList.Where(b => b.IsBlackList == query.IsBlackList);
        }
        if (query.DateStart is not null)
        {
            searchLogList = searchLogList.Where(b => b.CreateDate
                               >= query.DateStart);
        }
        if (query.DateEnd is not null)
        {
            searchLogList = searchLogList.Where(b => b.CreateDate
                                                     <= query.DateEnd);
        }
        return searchLogList
            .PaginatedListWithMappingAsync<SearchLog, SearchLogDto>(_mapper, query.Page, query.Size, query.OrderBy, query.SortBy);
    }
}
