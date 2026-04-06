using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IntegrationLogger
{
    public class SoapInspectorBehavior : IEndpointBehavior
    {
        private readonly SoapMessageInspector _inspector;
        public SoapInspectorBehavior(SoapMessageInspector inspector)
        {
            _inspector = inspector;
        }

        public string LastRequestXml => _inspector.LastRequestXml;
        public string LastResponseXml => _inspector.LastResponseXml;
        public string Name { get; set; }
        public string Type { get; set; }
        public string CorrelationId { get; set; }

        public void AddBindingParameters(ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            throw new NotSupportedException();
        }

        public void ApplyDispatchBehavior(ServiceEndpoint endpoint, EndpointDispatcher endpointDispatcher)
        {
            throw new NotSupportedException();
        }

        public void Validate(ServiceEndpoint endpoint)
        {
            throw new NotSupportedException();
        }

        public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {
            _inspector.Name = Name;
            _inspector.Type = Type;
            _inspector.CorrelationId = CorrelationId;
            clientRuntime.ClientMessageInspectors.Add(_inspector);
        }
    }
}
