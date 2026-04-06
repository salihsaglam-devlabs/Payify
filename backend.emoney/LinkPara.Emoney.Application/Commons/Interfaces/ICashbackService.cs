using LinkPara.Emoney.Application.Commons.Models;
using LinkPara.HttpProviders.Cashback.Models;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ICashbackService
{
    public Task SendCashbackQueueAsync(SendCashbackQueueRequest request);
}