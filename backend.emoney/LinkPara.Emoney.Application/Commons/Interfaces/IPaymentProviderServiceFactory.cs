namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface IPaymentProviderServiceFactory
{
    public Task<IPaymentProviderService> GetPaymentProviderServiceAsync(string paymentType);
}
