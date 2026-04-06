using System.Runtime.Serialization;

namespace LinkPara.LogConsumers.Commons.Models.Enums;


public enum ElasticOperationType
{
    [EnumMember(Value = "index")]
    Index,
    [EnumMember(Value = "create")]
    Create
}