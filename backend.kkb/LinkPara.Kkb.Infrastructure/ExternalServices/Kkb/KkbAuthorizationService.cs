using System.Net.Http.Json;
using LinkPara.Kkb.Application.Commons.Interfaces;
using LinkPara.Kkb.Application.Commons.Models.KkbSettings;
using LinkPara.Kkb.Domain.Entities;
using LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using LinkPara.SystemUser;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using LinkPara.HttpProviders.BusinessParameter;
using MassTransit;
using Microsoft.Extensions.Logging;
using LinkPara.Kkb.Infrastructure.Persistence;

namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb
{
    public class KkbAuthorizationService : IKkbAuthorizationService
    {
        private readonly KkbDbContext _context;
        private readonly HttpClient _client;
        private readonly KkbApiAuthorizationSettings _settings;
        private readonly IGenericRepository<KkbAuthorizationToken> _kkbAuthorizationTokenRepository;
        private readonly IApplicationUserService _applicationUserService;
        private readonly ILogger<KkbValidationService> _logger;
        private readonly IParameterService _parameterService;
        private readonly IBus _bus;
        public const string ParameterGroupCode = "IntegrationLoggerState";
        public const string ParameterCode = "Kkb";

        private string AccessToken;
        private string RefreshToken;
        private DateTime? TokenExpireDate;

        public KkbAuthorizationService(HttpClient client,
            KkbApiAuthorizationSettings settings,
            IGenericRepository<KkbAuthorizationToken> kkbAuthorizationTokenRepository,
            IApplicationUserService applicationUserService,
            ILogger<KkbValidationService> logger,
            IParameterService parameterService,
            IBus bus,
            KkbDbContext context)
        {
            _client = client;
            _settings = settings;
            _kkbAuthorizationTokenRepository = kkbAuthorizationTokenRepository;
            _applicationUserService = applicationUserService;
            _logger = logger;
            _parameterService = parameterService;
            _bus = bus;
            _context = context;
        }

        public async Task<string> GetTokenAsync()
        {
            await GetNewTokenAsync();
            return string.Concat("Bearer ", AccessToken);
        }

        private async Task GetNewTokenAsync()
        {
            string requestXml = string.Empty;
            string responseBody = string.Empty;
            var now = DateTime.Now;

            var kkbAuthorizationTokenRepo = await _context.KkbAuthorizationToken
                .Where(s => s.ExpiresDate >= now && s.IsSuccess)
                .FirstOrDefaultAsync();

            if (kkbAuthorizationTokenRepo is null)
            {
                var body = new Dictionary<string, string>
                {
                    { "state", "init" },
                    { "grant_type", "password" },
                    { "scope", "findeks" },
                    { "client_id", _settings.ClientId },
                    { "client_secret", _settings.ClientSecret },
                    { "username", _settings.UserName },
                    { "password", _settings.Password }
                };

                try
                {
                    _client.BaseAddress = new Uri(_settings.ApiUrl);
                    var request = new HttpRequestMessage(HttpMethod.Post, "auth/oauth/v2/token") { Content = new FormUrlEncodedContent(body) };
                    requestXml = JsonConvert.SerializeObject(body);

                    var response = await _client.SendAsync(request);
                    responseBody = await response.Content.ReadAsStringAsync();


                    bool isLogEnable;
                    try
                    {
                        var param = await _parameterService.GetParameterAsync(ParameterGroupCode, "Kkb");
                        isLogEnable = Convert.ToBoolean(param.ParameterValue);
                    }
                    catch
                    {
                        isLogEnable = true;
                    }

                    if (isLogEnable)
                    {
                        var correlationId = Guid.NewGuid();
                        var log = new IntegrationLog()
                        {
                            CorrelationId = correlationId.ToString(),
                            Name = "Kkb",
                            Type = "KkbAuthorizationToken",
                            Date = DateTime.Now,
                            Request = requestXml,
                            Response = responseBody,
                            DataType = IntegrationLogDataType.Json
                        };
                        var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                        var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                        await endpoint.Send(log, cancellationToken.Token);
                    }

                    if (!response.IsSuccessStatusCode)
                    {
                        await _kkbAuthorizationTokenRepository.AddAsync(new KkbAuthorizationToken
                        {
                            AccessToken = null,
                            RefreshToken = null,
                            TokenType = null,
                            ExpiresDate = DateTime.MinValue,
                            IsSuccess = false,
                            Error = response.StatusCode.ToString(),
                            ErrorDescription = response.ToString(),
                            CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                        });
                        throw new AuthorizationException(response.StatusCode.ToString(), nameof(this.GetNewTokenAsync));
                    }

                    await DeleteLoginSessionAsync();

                    var kkbToken = await response.Content.ReadFromJsonAsync<KkbTokenResponse>();
                    AccessToken = kkbToken.access_token;
                    RefreshToken = kkbToken.refresh_token;
                    TokenExpireDate = DateTime.Now.AddSeconds(kkbToken.expires_in - 100);

                    var tokenExpireDate = DateTime.Now.AddSeconds(kkbToken.expires_in - 100);

                    await _kkbAuthorizationTokenRepository.AddAsync(new KkbAuthorizationToken
                    {
                        AccessToken = AccessToken,
                        RefreshToken = RefreshToken,
                        TokenType = kkbToken.token_type,
                        ExpiresDate = tokenExpireDate,
                        IsSuccess = true,
                        CreatedBy = _applicationUserService.ApplicationUserId.ToString()
                    });
                }
                catch (Exception ex)
                {
                    var correlationId = Guid.NewGuid();
                    var log = new IntegrationLog()
                    {
                        CorrelationId = correlationId.ToString(),
                        Name = "Kkb",
                        Type = "KkbAuthorizationToken",
                        Date = DateTime.Now,
                        Request = requestXml,
                        Response = responseBody,
                        DataType = IntegrationLogDataType.Text
                    };
                    var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                    await endpoint.Send(log, cancellationToken.Token);

                    _logger.LogError("IntegrationLog Error: Kkb - Exception {Exception}", ex);
                }                
            }
            else
            {
                AccessToken = kkbAuthorizationTokenRepo.AccessToken;
                RefreshToken = kkbAuthorizationTokenRepo.RefreshToken;
                TokenExpireDate = kkbAuthorizationTokenRepo.ExpiresDate;
            }
        }

        private async Task GetNewTokenWithRefreshTokenAsync()
        {
            var body = new Dictionary<string, string>
            {
                { "state", "init" },
                { "grant_type", "refresh_token" },
                { "scope", "findeks" },
                { "client_id", _settings.ClientId },
                { "client_secret", _settings.ClientSecret },
                { "refresh_token", RefreshToken }
            };

            _client.BaseAddress = new Uri(_settings.ApiUrl);

            var request = new HttpRequestMessage(HttpMethod.Post, "auth/oauth/v2/token") { Content = new FormUrlEncodedContent(body) };

            var response = await _client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                throw new AuthorizationException(response.StatusCode.ToString(), nameof(this.GetNewTokenWithRefreshTokenAsync));
            }

            var kkbToken = await response.Content.ReadFromJsonAsync<KkbTokenResponse>();

            AccessToken = kkbToken.access_token;
            RefreshToken = kkbToken.refresh_token;
            TokenExpireDate = DateTime.Now.AddSeconds(kkbToken.expires_in - 100);
        }

        private async Task DeleteLoginSessionAsync()
        {
            var now = DateTime.Now;

            var sessions = await _context.KkbAuthorizationToken
                    .Where(b => b.ExpiresDate < now)
                    .ToListAsync();

            if (sessions.Any())
            {
                try
                {
                    _context.RemoveRange(sessions);

                    await _context.SaveChangesAsync();
                }
                catch (Exception exception)
                {
                    _logger.LogError("KKB Sessions Delete Error {Exception}", exception);
                }
            }
        }
    }
}
