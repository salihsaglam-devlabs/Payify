namespace LinkPara.SharedModels.Exceptions;

public class DuplicateRecordException : GenericException
{
    public DuplicateRecordException() 
        : base(ErrorCode.DuplicateRecord, "Duplicate Record")
    {
    }

    public DuplicateRecordException(string name)
        : base(ErrorCode.DuplicateRecord, $"Entity \"{name}\" already created.")
    {
    }
    
    public DuplicateRecordException(string name, object key)
        : base(ErrorCode.DuplicateRecord, $"Entity \"{name}\" ({key}) already created.")
    {
    }
}