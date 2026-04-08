using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class ConsumerRequestException : ApiException
{
    public ConsumerRequestException(string code, string message)
        : base(code, message)
    {
    }

    public ConsumerRequestException(string code, string message, Exception innerException)
        : base(code, AppendInnerException(message, innerException))
    {
    }

    private static string AppendInnerException(string message, Exception innerException)
    {
        if (innerException == null)
            return message;
        
        return $"{message} | Inner: {innerException.GetType().Name} - {innerException.Message}";
    }

    protected ConsumerRequestException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

