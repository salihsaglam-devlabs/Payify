using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.LogConsumers.Commons.Interfaces;

public interface IConfidentialService
{
    string MaskData(string value , IntegrationLogDataType? dataType, string name = null);
}
