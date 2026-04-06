namespace LinkPara.ContextProvider;

public interface IContextProvider
{
    Context CurrentContext { get; }
}