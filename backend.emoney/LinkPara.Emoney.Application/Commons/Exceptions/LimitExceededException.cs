using System.Runtime.Serialization;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class LimitExceededException: ApiException
{
   public LimitExceededException()
       : base(ApiErrorCode.LimitExceeded, "LimitExceeded")
   {
   }

   public LimitExceededException(LimitOperationType operationType) : 
       base(ApiErrorCode.LimitExceeded, $"Limit Exceeded, Limit Type : {operationType}")
   {

   }
   
   protected LimitExceededException(SerializationInfo info, StreamingContext context) 
       : base(info, context)
   {
   }
}