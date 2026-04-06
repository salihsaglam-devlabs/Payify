namespace LinkPara.Kkb.Application.Commons.Interfaces
{
    public interface IKkbAuthorizationService
    {
        Task<string> GetTokenAsync();
    }
}