namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Emoney;

public class WithdrawRequestCreated
{
    public Guid WithdrawRequestId { get; set; }

    public WithdrawRequestCreated(Guid withdrawRequestId)
    {
        WithdrawRequestId = withdrawRequestId;
    }
}
