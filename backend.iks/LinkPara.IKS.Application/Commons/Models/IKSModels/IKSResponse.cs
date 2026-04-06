

namespace LinkPara.IKS.Application.Commons.Models.IKSModels
{
    public class IKSResponse<T>
    {
        public IKSError Error { get; set; }
        public T Data { get; set; }
        public string StatusCode { get; set; }
        public bool IsSuccess
        {
            get
            {
                return Error == null || string.IsNullOrWhiteSpace(Error.moreInformation);
            }
        }
    }
}
