using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class RequiredDocumentTypeException : CustomApiException
{
    public RequiredDocumentTypeException(string documentType)
        : base(ApiErrorCode.RequiredDocumentType, documentType) { }
}