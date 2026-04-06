using Elastic.Apm.Api;
using LinkPara.HttpProviders.Identity;
using LinkPara.HttpProviders.Identity.Models;
using LinkPara.SharedModels.BusModels.Commands.Notification;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SoftOtp.Application.Common.Exceptions;
using LinkPara.SoftOtp.Application.Common.Interfaces;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Request;
using LinkPara.SoftOtp.Application.Common.Models.PowerFactorModels.Response;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.CheckTransactionApproval;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.DeviceActivation;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartClientTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.StartOneTouchTransaction;
using LinkPara.SoftOtp.Application.Features.Auth.Commands.UpdateActivationPINByCustomerIdTransaction;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LinkPara.SoftOtp.Infrastructure.Services
{
    public class MultifactorService : IMultifactorService
    {
        private readonly IPowerFactorAdapter _powerFactorAdapter;
        private readonly ILogger<MultifactorService> _logger;
        private readonly IUserService _userService;
        private readonly IStringLocalizer _localizer;
        private readonly IPushNotificationSender _pushNotificationSender;
        private readonly IIntegrationLogger _integrationLogger;

        public MultifactorService(
            IPowerFactorAdapter powerFactorAdapter,
            ILogger<MultifactorService> logger,
            IUserService userService,
            IStringLocalizerFactory factory,
            IPushNotificationSender pushNotificationSender,
            IIntegrationLogger integrationLogger)
        {
            _powerFactorAdapter = powerFactorAdapter;
            _logger = logger;
            _userService = userService;
            _pushNotificationSender = pushNotificationSender;
            _integrationLogger = integrationLogger;
            _localizer = factory.Create("Messages", "LinkPara.SoftOtp.API");

        }

        public async Task<GenerateActivationOtpResponse> SendActivationOtpAsync(MultifactorAuthCommand request)
        {
            var correlationId = Guid.NewGuid().ToString();
            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "MultifactorActivation",
                Request = JsonSerializer.Serialize(request),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            var otpResponse = await _powerFactorAdapter.GetActivationOtpAsync(new GenerateActivationOtpRequest
            {
                CustomerId = long.Parse(request.PhoneNumber)
            });

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "MultifactorActivation",
                Response = JsonSerializer.Serialize(otpResponse),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            if (!otpResponse.Success)
            {
                foreach (var result in otpResponse.Results)
                {
                    _logger.LogError($"SoftOtpError: Id: {request.PhoneNumber}, Error: {result.ErrorMessage}");
                }

                throw new OtpActivationFailedException();
            }

            return otpResponse;
        }

        public async Task<VerifyLoginOtpResponse> VerifyLoginOtpAsync(VerifyLoginOtpRequest request)
        {
            var correlationId = Guid.NewGuid().ToString();

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "VerifyLoginOtp",
                Request = JsonSerializer.Serialize(request),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            var response = await _powerFactorAdapter.VerifyLoginOtpAsync(request);

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "VerifyLoginOtp",
                Response = JsonSerializer.Serialize(response),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });


            if (!response.Success)
            {
                foreach (var result in response.Results)
                {
                    _logger.LogError($"VerifyLoginOtpAsyncException: {result.ErrorMessage}");
                }

                throw new VerifyLoginFailedException();
            }

            return response;
        }

        public async Task<StartClientTransactionResponse> StartClientTransaction(StartClientTransactionCommand command)
        {
            var correlationId = Guid.NewGuid().ToString();

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "StartClientTransaction",
                Request = JsonSerializer.Serialize(command),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            var response = await _powerFactorAdapter.StartClientTransactionAsync(command);

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "StartClientTransaction",
                Response = JsonSerializer.Serialize(response),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            if (!response.Success)
            {
                foreach (var result in response.Results)
                {
                    _logger.LogError($"StartClientTransactionException: {result.ErrorMessage}");
                }

                throw new StartClientTransactionException();
            }

            return response;
        }

        public async Task<CheckTransactionApprovalResponse> CheckTransactionApproval(CheckTransactionApprovalCommand command)
        {
            var correlationId = Guid.NewGuid().ToString();

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "CheckTransactionApproval",
                Request = JsonSerializer.Serialize(command),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            var response = await _powerFactorAdapter.CheckTransactionApproval(command);

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "CheckTransactionApproval",
                Response = JsonSerializer.Serialize(response),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            if (!response.Success)
            {
                foreach (var result in response.Results)
                {
                    _logger.LogError($"CheckTransactionApprovalException: {result.ErrorMessage}");
                }

                throw new CheckTransactionApprovalException();
            }

            return response;
        }

        public async Task<StartOneTouchTransactionResponse> StartOneTouchTransaction(StartOneTouchTransactionCommand command)
        {
            var correlationId = Guid.NewGuid().ToString();

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "StartOneTouchTransaction",
                Request = JsonSerializer.Serialize(command),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            var response = await _powerFactorAdapter.StartOneTouchTransaction(command);

            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "StartOneTouchTransaction",
                Response = JsonSerializer.Serialize(response),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });

            if (!response.Success)
            {
                foreach (var result in response.Results)
                {
                    _logger.LogError($"StartOneTouchTransactionResponseException: {result.ErrorMessage}");
                }

                throw new StartOneTouchTransactionException();
            }

            await SendApprovementNotificationAstnc(command, response);

            return response;
        }

        private async Task SendApprovementNotificationAstnc(StartOneTouchTransactionCommand command,
            StartOneTouchTransactionResponse response)
        {
            var userList = await _userService
                .GetAllUsersAsync(new GetUsersRequest { UserName = command.UserName });

            if (!userList.Items.Any())
            {
                throw new NotFoundException(nameof(User));
            }

            var user = userList.Items.FirstOrDefault();


            if (user != null)
            {
                var receiverUserIdList = new List<NotificationUserInfo>()
                {
                    new()
                    {
                        UserId = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName
                    }
                };

                var receiverUserDeviceInfoResponse = await _userService.GetUserDeviceInfo(
                    new GetUserDeviceInfoRequest()
                    {
                        UserIdList = new List<Guid>() { user.Id }
                    });
                var receiverNotificationRequest = new SendPushNotification
                {
                    TemplateName = "StartOneTouchTransaction",
                    TemplateParameters = new Dictionary<string, string>
                    {
                        { "PushToken", _localizer.GetString("OneTouchTransactionApprovement") }
                    },
                    Tokens = receiverUserDeviceInfoResponse.Select(x => x.DeviceInfo.RegistrationToken).ToList(),
                    UserList = receiverUserIdList,
                    Data = new Dictionary<string, string>
                    {
                        { "TransactionToken", response.TransactionToken },
                        { "PushToken", response.PushToken }
                    }
                };

                await _pushNotificationSender.SendPushNotificationAsync(receiverNotificationRequest);
            }
        }

        public async Task<UpdateActivationPINByCustomerIdResponse> UpdateActivationPINByCustomerIdAsync(UpdateActivationPINByCustomerIdCommand command)
        {
            command.PIN = HashPin(command.PIN);
            var correlationId = Guid.NewGuid().ToString();
            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "UpdateActivationPINByCustomerIdAsync",
                Request = JsonSerializer.Serialize(command),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });
            var otpResponse = await _powerFactorAdapter.UpdateActivationPINByCustomerId(command);
            await _integrationLogger.QueueLogAsync(new IntegrationLog
            {
                CorrelationId = correlationId,
                Name = "SoftOtp",
                Type = "UpdateActivationPINByCustomerIdAsync",
                Response = JsonSerializer.Serialize(otpResponse),
                Date = DateTime.Now,
                DataType = IntegrationLogDataType.Json
            });
            if (!otpResponse.Success)
            {
                foreach (var result in otpResponse.Results)
                {
                    _logger.LogError($"UpdateActivationPINByCustomerIdException: {result.ErrorMessage}");
                }

                throw new UpdateActivationPINByCustomerIdException();
            }
            return otpResponse;
        }

        private static string HashPin(string pin)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(pin);
                byte[] hashBytes = sha256.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                return sb.ToString();
            }
        }
    }
}