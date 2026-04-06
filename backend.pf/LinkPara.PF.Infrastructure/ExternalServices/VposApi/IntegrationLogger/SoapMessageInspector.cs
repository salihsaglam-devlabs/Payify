using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IntegrationLogger
{
    public class SoapMessageInspector : IClientMessageInspector
    {
        private readonly IBus _bus;
        private readonly ILogger<SoapMessageInspector> _logger;
        public SoapMessageInspector(IBus bus,
            ILogger<SoapMessageInspector> logger)
        {
            _bus = bus;
            _logger = logger;
        }
        public string LastRequestXml { get; private set; }
        public string LastResponseXml { get; private set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string CorrelationId { get; set; }
        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            LastRequestXml = request.ToString();
            CorrelationId = Guid.NewGuid().ToString();
            Task.Run(async () => await SendRequest(LastRequestXml)).Wait();
            return request;
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            LastResponseXml = reply.ToString();

            HttpResponseMessageProperty responseProperty = null;

            if (reply.Properties.ContainsKey(HttpResponseMessageProperty.Name))
            {
                responseProperty = (HttpResponseMessageProperty)reply.Properties[HttpResponseMessageProperty.Name];
            }

            Task.Run(async () => await SendResponse(LastResponseXml, responseProperty)).Wait();
        }

        private async Task SendRequest(string request)
        {
            try
            {
                var log = new IntegrationLog()
                {
                    CorrelationId = CorrelationId,
                    Name = Name,
                    Type = Type,
                    Date = DateTime.Now,
                    Request = request,
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"SendToRawDataQueue Error: BankName {Name} - Exception {exception}");
            }
        }
        private async Task SendResponse(string response, HttpResponseMessageProperty responseProperty)
        {
            try
            {
                var log = new IntegrationLog()
                {
                    CorrelationId = CorrelationId,
                    Name = Name,
                    Type = Type,
                    Date = DateTime.Now,
                    Response = response,
                    HttpCode = responseProperty is null ? string.Empty : ((int)responseProperty.StatusCode).ToString(),
                    ErrorCode = responseProperty is null ? string.Empty : responseProperty.StatusCode.ToString(),
                    ErrorMessage = responseProperty is null ? string.Empty : responseProperty.StatusCode.ToString(),
                    DataType = IntegrationLogDataType.Soap
                };

                using var cancellationToken = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Log.IntegrationLog"));
                await endpoint.Send(log, cancellationToken.Token);
            }
            catch (Exception exception)
            {
                _logger.LogError($"SendToRawDataQueue Error: BankName {Name} - Exception {exception}");
            }
        }
    }
}
