namespace LinkPara.HttpProviders.Accounting;

public interface IPaymentService
{
    Task CancelPaymentAsync(Guid clientReferenceId);
}
