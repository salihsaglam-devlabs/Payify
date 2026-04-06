namespace LinkPara.SharedModels.Exceptions;

public class NotFoundException : GenericException
{
    public NotFoundException()
        : base(ErrorCode.NotFound)
    {
        
    }
    public NotFoundException(string message)
        : base(ErrorCode.NotFound, message)
    {
    }

    public NotFoundException(string name, object key)
        : base(ErrorCode.NotFound, $"Entity {name} ({key}) was not found.")
    {
    }
}