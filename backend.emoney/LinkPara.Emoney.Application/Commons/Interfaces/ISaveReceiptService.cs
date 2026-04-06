
namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ISaveReceiptService
{
    public Task SendReceiptQueueAsync(Guid id);
}