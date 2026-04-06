namespace LinkPara.PF.Application.Commons.Models.Returns;

public class ReturnSettingsModel
{
    public bool IsManuelReturnAllowed { get; set; }
    public bool IsManuelExcessReturnAllowed { get; set; }
    public string ReturnBankAccountIban { get; set; }
}
