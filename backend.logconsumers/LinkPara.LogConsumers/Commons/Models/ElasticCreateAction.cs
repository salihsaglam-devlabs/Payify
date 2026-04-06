using LinkPara.LogConsumers.Services;
using System.Runtime.Serialization;

namespace LinkPara.LogConsumers.Commons.Models;

public class ElasticCreateAction
{
    public ElasticCreateAction(ElasticActionPayload payload)
    {
        Payload = payload;
    }

    [DataMember(Name = "create")]
    public ElasticActionPayload Payload { get; }
}