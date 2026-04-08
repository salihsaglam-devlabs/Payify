using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class ReconciliationPayloadException : ApiException
{
    public ReconciliationPayloadException(string code, string message)
        : base(code, message)
    {
    }

    public ReconciliationPayloadException(string code, string message, Exception innerException)
        : base(code, AppendInnerException(message, innerException))
    {
    }

    private static string AppendInnerException(string message, Exception innerException)
    {
        if (innerException == null)
            return message;
        
        return $"{message} | Inner: {innerException.GetType().Name} - {innerException.Message}";
    }

    protected ReconciliationPayloadException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

