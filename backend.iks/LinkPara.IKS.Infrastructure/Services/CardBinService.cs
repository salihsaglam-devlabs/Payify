using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using LinkPara.Audit;
using LinkPara.Cache;
using LinkPara.ContextProvider;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Vault;
using LinkPara.IKS.Application.Commons.Interfaces;
using LinkPara.IKS.Application.Commons.Models.Configurations;
using LinkPara.IKS.Application.Commons.Models.IKSModels;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Request;
using LinkPara.IKS.Application.Commons.Models.IKSModels.CardBins.Response;
using LinkPara.IKS.Application.Enums;
using LinkPara.IKS.Domain.Entities;
using LinkPara.IKS.Infrastructure.ExternalService.IKS.Models.Responses;

using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.Extensions.Logging;
using IKSError = LinkPara.IKS.Application.Commons.Models.IKSModels.IKSError;


namespace LinkPara.IKS.Infrastructure.Services
{
	public class CardBinService : ICardBinService
	{
		private readonly ICacheService _cacheService;
		private IKSTokenResponse cachedToken;
		private HttpClient _client;

		private readonly CardBinSettings _settings;
		private readonly ILogger<CardBinService> _logger;
		private readonly IGenericRepository<IKSTransaction> _iksTransactionRepository;
		private readonly IParameterService _parameterService;
		private readonly IBus _bus;
		public const string ParameterGroupCode = "IntegrationLoggerState";
		public const string ParameterCode = "Iks";
		
		private string certificatePath = "/iks/certificate.pfx";
		private string CertificatePath
		{
			get
			{
				if (string.IsNullOrEmpty(certificatePath))
				{
					try
					{
						string baseDirectory = AppContext.BaseDirectory;
						int binIndex = baseDirectory.IndexOf("bin");
						string pathFirstPath = baseDirectory[..binIndex];
						return certificatePath = Path.Combine(pathFirstPath, "Certificates", "certificate.pfx");
					}
					catch (Exception exception)
					{
						_logger.LogError($"CardBin-GetCertificatePath error: {exception}");
						throw;
					}
				}

				return certificatePath;
			}
		}

		public CardBinService(IVaultClient vaultClient,
							  ILogger<CardBinService> logger,
							  ICacheService cacheService,
							  IGenericRepository<IKSTransaction> iksTransactionRepository,
							  IBus bus,
							  IParameterService parameterService)
		{
			_cacheService = cacheService;
			_logger = logger;

			_settings = vaultClient.GetSecretValue<CardBinSettings>("IKSSecrets", "CardBinSettings", null);
			_iksTransactionRepository = iksTransactionRepository;
			_bus = bus;
			_parameterService = parameterService;
		}

		public async Task<IKSResponse<CardBinResponse>> GetCardBinRangeAsync(CardBinRequest request)
		{
			try
			{
				var result = await CardBinExecuteAsync<CardBinResponse>(_settings.CardBinEndPoint, request);

				string requestJson = Newtonsoft.Json.JsonConvert.SerializeObject(request, Newtonsoft.Json.Formatting.Indented);
				string responseJson = Newtonsoft.Json.JsonConvert.SerializeObject(result.Data != null
					? result.Data : result.Error, Newtonsoft.Json.Formatting.Indented);

				var operation = "GetCardBinRanges";
				result.StatusCode = "200";

				await _iksTransactionRepository.AddAsync(
				new IKSTransaction
				{
					CreatedBy = Guid.Empty.ToString(),
					IsSuccess = result.IsSuccess,
					ResponseCode = result.IsSuccess ? "200" : result.Error?.httpCode.ToString(),
					Operation = operation,
					MerchantId = Guid.NewGuid(),
					RequestDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(requestJson),
					ResponseDetails = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(responseJson)
				}
				);
				return result;
			}
			catch (Exception exception)
			{
				_logger.LogError($"CardBin-GetCardBinInfoAsync error: {exception}");
				throw;
			}
		}

		public async Task<IKSResponse<T>> CardBinExecuteAsync<T>(string endPoint, object parameters)
		{
			cachedToken = _cacheService.Get<IKSTokenResponse>("CardBinToken");
			if (cachedToken is null || DateTime.Now > cachedToken.ExpireDate)
			{
				var token = await GetToken();
				token.ExpireDate = DateTime.Now.AddSeconds(token.expires_in);
				_cacheService.Add("CardBinToken", token, 60);
				cachedToken = token;
			}

			_client = CreatePfxFile();
			_client.BaseAddress = new Uri(endPoint);
			_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cachedToken.access_token);

			HttpResponseMessage response = await _client.GetAsync(endPoint);
			var responseString = await response.Content.ReadAsStringAsync();
			string requestString = JsonSerializer.Serialize(parameters);

			try
			{

				var correlationId = Guid.NewGuid();
				var log = new IntegrationLog()
				{
					CorrelationId = correlationId.ToString(),
					Name = "BkmCardBin",
					Type = IksOperationType.GetCardBins.ToString(),
					Date = DateTime.Now,
					Request = requestString,
					Response = responseString,
					DataType = IntegrationLogDataType.Text
				};
				var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
				var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
				await endpoint.Send(log, cancellationToken.Token);

			}
			catch (Exception ex)
			{
				_logger.LogError($"IntegrationLog Error: CardBin-CardBinInfo Exception: {ex}");
				throw;
			}

			var result = JsonSerializer.Deserialize<T>(responseString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			if (!response.IsSuccessStatusCode)
			{
				var errorResult = JsonSerializer.Deserialize<IKSError>(responseString);

				return new IKSResponse<T>
				{
					Error = new IKSError
					{
						httpCode = (int)errorResult.httpCode,
						httpMessage = errorResult.httpMessage,
						type = errorResult.type,
						path = errorResult.path,
						moreInformation = errorResult.moreInformation,
						errors = errorResult.errors,
						timestamp = errorResult.timestamp,
					}
				};
			}
			return new IKSResponse<T>()
			{
				Data = result
			};
		}

		private async Task<IKSTokenResponse> GetToken()
		{
			string requestString = string.Empty;

			try
			{
				_client = CreatePfxFile();

				var values = new Dictionary<string, string>
				{
					{ "grant_type", _settings.Grant_Type },
					{ "scope", _settings.Scope},
					{ "client_secret", _settings.Client_Secret },
					{ "client_id", _settings.Client_ID }
				};
				requestString = JsonSerializer.Serialize(values);
				var content = new FormUrlEncodedContent(values);
				var response = await _client.PostAsync(_settings.APIEndPoint + _settings.TokenEndPoint, content);
				string responseString = await response.Content.ReadAsStringAsync();

				try
				{
					bool isLogEnable;
					try
					{
						var param = await _parameterService.GetParameterAsync(ParameterGroupCode, ParameterCode);
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
							Name = ParameterCode,
							Type = "GetToken",
							Date = DateTime.Now,
							Request = requestString,
							Response = responseString,
							DataType = IntegrationLogDataType.Text
						};
						var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
						var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
						await endpoint.Send(log, cancellationToken.Token);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError($"IntegrationLog Error: CardBin Exception: {ex}");
				}

				return JsonSerializer.Deserialize<IKSTokenResponse>(responseString);
			}
			catch (Exception exception)
			{
				_logger.LogError($"CardBin-GetToken Error : {exception}");
				var correlationId = Guid.NewGuid();
				var log = new IntegrationLog()
				{
					CorrelationId = correlationId.ToString(),
					Name = ParameterCode,
					Type = "GetToken",
					Date = DateTime.Now,
					Request = requestString,
					ErrorMessage = exception.ToString(),
					DataType = IntegrationLogDataType.Text
				};
				var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
				var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
				await endpoint.Send(log, cancellationToken.Token);
				throw;
			}
		}

		private HttpClient CreatePfxFile()
		{
			try
			{
				X509Certificate2 pfxCertWithKey = new X509Certificate2(CertificatePath, _settings.CsrFilePassword);

				HttpClientHandler handler = new HttpClientHandler();
				handler.ClientCertificateOptions = ClientCertificateOption.Manual;
				handler.ServerCertificateCustomValidationCallback = (a, b, c, d) => { return true; };
				handler.ClientCertificates.Add(pfxCertWithKey);
				handler.SslProtocols = SslProtocols.Tls12;

				_client = new HttpClient(handler);

				return _client;
			}
			catch (Exception exception)
			{
				_logger.LogError($"CardBin-CreatePfxFile Error : {exception}");
				throw;
			}
		}
	}
}
