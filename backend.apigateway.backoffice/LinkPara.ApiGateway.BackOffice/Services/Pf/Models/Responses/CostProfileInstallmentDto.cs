namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class CostProfileInstallmentDto
{
    public Guid Id { get; set; }
    public int InstallmentSequence { get; set; }
    public int BlockedDayNumber { get; set; }
}