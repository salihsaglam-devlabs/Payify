using LinkPara.Documents.Application.Commons.Exceptions;
using LinkPara.SharedModels.Exceptions;

namespace LinkPara.Documents.Application.Commons.Exceptions
{
    public class InvalidFileTypeException : ApiException
    {
        public InvalidFileTypeException()
            : base(ApiErrorCode.InvalidFileType, "InvalidFileTypeException")
        {
        }
    }
}
