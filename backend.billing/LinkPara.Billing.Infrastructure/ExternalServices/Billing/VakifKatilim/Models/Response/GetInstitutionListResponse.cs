namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Response;

public class GetInstitutionListResponse
{
    public int InstitutionId { get; set; }
    public int InstitutionType { get; set; }
    public string InstitutionShortName { get; set; }
    public string InstitutionLongName { get; set; }
    public string SubscriberLabel { get; set; }
    public string SubscriberNoHelpText { get; set; }
    public int SubscriberNoMaxLength { get; set; }
    public bool IsSubscriberNoNumeric { get; set; }
    public string AdditionalInfoInputField { get; set; }
    public int AdditionalInfoInputFieldMaxLength { get; set; }
    public bool IsRequiredAdditionalInfoInputField { get; set; }
    public bool IsAllowsPartialPayment { get; set; }
    public bool IsOnline { get; set; }
}