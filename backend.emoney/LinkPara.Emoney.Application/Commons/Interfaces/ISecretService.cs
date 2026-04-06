using LinkPara.Emoney.Application.Commons.Models.OpenBankingConfiguration;

namespace LinkPara.Emoney.Application.Commons.Interfaces;

public interface ISecretService
{
    OpenBankingHhsSettings OpenBankingHhsSettings { get; }
    OpenBankingYosSettings OpenBankingYosSettings { get; }
}
