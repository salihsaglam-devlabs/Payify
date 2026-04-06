using System.Runtime.Serialization;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class InvalidTransactionStatusException : ApiException
{
    public InvalidTransactionStatusException()
        : base(ApiErrorCode.InvalidTransactionStatus, "InvalidTransactionStatus")
    {
    }

    public InvalidTransactionStatusException(Guid transactionId) : 
        base(ApiErrorCode.InvalidTransactionStatus, $"Invalid Transaction Status : {transactionId}")
    {

    }
    
    protected InvalidTransactionStatusException(SerializationInfo info, StreamingContext context) 
        : base(info, context)
    {
    }
}
