using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Application.Commons.Models.ReceiptModels;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace LinkPara.Emoney.Infrastructure.Services;

public class SaveReceiptService : ISaveReceiptService
{
    private readonly ILogger<SaveReceiptService> _logger;
    private readonly IBus _bus;

    public SaveReceiptService(
        ILogger<SaveReceiptService> logger, IBus bus)
    {
        _logger = logger;
        _bus = bus;
    }


    public async Task SendReceiptQueueAsync(Guid id)
    {
        try
        {
            var request = new SaveReceiptRequest() { TransactionId = id };
            var tokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var endpoint = await _bus.GetSendEndpoint(new Uri("exchange:Emoney.SaveReceiptRequest"));
            await endpoint.Send(request, tokenSource.Token);
        }
        catch (Exception exception)
        {
            _logger.LogError($"ExceptionOnSendMessage detail:\n{exception}");
        }
    }

}