using Elastic.Apm.Api;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses
{
    public class IKSResponse<T>
    {
        public IKSError Error { get; set; }
        public T Data { get; set; }
        public bool IsSuccess
        {
            get
            {
                return Error == null || string.IsNullOrWhiteSpace(Error.moreInformation);
            }
        }
    }
}
