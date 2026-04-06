namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Response;

public class IksTerminalQueryResponse
{
    public List<IKSTerminal> terminals { get; set; }
    public int totalCount { get; set; }
    public int offset { get; set; }
    public int limit { get; set; }
}