using LinkPara.SharedModels.Exceptions;
using System.Runtime.Serialization;

namespace LinkPara.Emoney.Application.Commons.Exceptions;

[Serializable]
public class DuplicateBulkTransferDetailException : ApiException
{
    public DuplicateBulkTransferDetailException()
        : base(ApiErrorCode.DuplicateBulkTransferDetail, "DuplicateBulkTransferDetail")
    {
    }

    protected DuplicateBulkTransferDetailException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}