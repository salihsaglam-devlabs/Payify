namespace LinkPara.IWallet.ApiGateway.Models.Responses
{
    public class BaseServiceResponse<T> : BaseResponse
    {
        public BaseServiceResponse(BaseResponse response)
        {
            IsSuccess = response.IsSuccess;
            ErrorCode = response.ErrorCode;
            Message = response.Message;
        }

        public T Data { get; set; }
    }
}
