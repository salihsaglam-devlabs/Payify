using System.Runtime.Serialization;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class DescriptionLengthException : ApiException
{
   public DescriptionLengthException()
       : base(ApiErrorCode.DescriptionLengthIsShort, "DescriptionLenthIsShort")
   {
   }

   public DescriptionLengthException(int length) : 
       base(ApiErrorCode.DescriptionLengthIsShort, $"DescriptionLengthIsShort : {length}")
   {

   }
   
   protected DescriptionLengthException(SerializationInfo info, StreamingContext context) 
       : base(info, context)
   {
   }
}