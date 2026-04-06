using LinkPara.LogConsumers.Services;
using System.Runtime.Serialization;

namespace LinkPara.LogConsumers.Commons.Models;


public class ElasticIndexAction
{
    public ElasticIndexAction(ElasticActionPayload payload)
    {
        Payload = payload;
    }

    [DataMember(Name = "index")]
    public ElasticActionPayload Payload { get; }
}

