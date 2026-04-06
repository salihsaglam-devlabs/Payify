using System;
using System.Runtime.Serialization;

namespace LinkPara.SharedModels.Exceptions
{
    [Serializable]
    public class AuthorizationException : UnauthorizedAccessException
    {
        public string Code { get; }

        public AuthorizationException()
            : base(ErrorCode.Unauthorized)
        {
            Code = ErrorCode.Unauthorized;
        }

        public AuthorizationException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        public AuthorizationException(string message, Exception inner)
            : base(message, inner)
        {
            Code = ErrorCode.Unauthorized;
        }

        protected AuthorizationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Code = info.GetString(nameof(Code));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue(nameof(Code), Code);
        }
    }
}