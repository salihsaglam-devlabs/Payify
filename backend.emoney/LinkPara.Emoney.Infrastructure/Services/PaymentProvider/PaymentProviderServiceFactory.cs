using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Infrastructure.Services.Masterpass;
using LinkPara.Emoney.Infrastructure.Services.PaymentProvider.PayifyPf;

namespace LinkPara.Emoney.Infrastructure.Services.PaymentProvider;

public class PaymentProviderServiceFactory : IPaymentProviderServiceFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentProviderServiceFactory(IServiceProvider serviceProvider)
        => _serviceProvider = serviceProvider;

    public async Task<IPaymentProviderService> GetPaymentProviderServiceAsync(string paymentType)
    {
        if (string.IsNullOrEmpty(paymentType))
            throw new ArgumentNullException(nameof(paymentType), "Payment type cannot be null or empty");

        var paymentProviderService = paymentType switch
        {
            "PayifyPf" => _serviceProvider.GetService(typeof(PayifyPfService)) as IPaymentProviderService,
            _ => throw new ArgumentException("Invalid payment type")
        };

        return paymentProviderService ?? throw new InvalidOperationException($"Service for payment type '{paymentType}' could not be found");
    }

}
