namespace LinkPara.SharedModels.BusModels.IntegrationEvents.DigitalKyc;

public class UpdateIntegrationState
{
    public Guid UserId { get; set; }
    public string IdentityNumber { get; set; }
}
