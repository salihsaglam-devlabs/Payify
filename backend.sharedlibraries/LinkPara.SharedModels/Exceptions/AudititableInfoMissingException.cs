namespace LinkPara.SharedModels.Exceptions;

public class AudititableInfoMissingException : GenericException
{
    public AudititableInfoMissingException() : base(ErrorCode.AuditableMissingInfo, "AuditableInfoMissing")
    {
    }
}
