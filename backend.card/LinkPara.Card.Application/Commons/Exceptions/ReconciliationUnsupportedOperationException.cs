using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Card.Application.Commons.Exceptions;

[Serializable]
public class ReconciliationUnsupportedOperationException : ApiException
{
    public ReconciliationUnsupportedOperationException(string code, string message)
        : base(code, message)
    {
    }

    public ReconciliationUnsupportedOperationException(string code, string message, Exception innerException)
        : base(code, AppendInnerException(message, innerException))
    {
    }

    private static string AppendInnerException(string message, Exception innerException)
    {
        if (innerException == null)
            return message;
        
        return $"{message} | Inner: {innerException.GetType().Name} - {innerException.Message}";
    }

    protected ReconciliationUnsupportedOperationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}

