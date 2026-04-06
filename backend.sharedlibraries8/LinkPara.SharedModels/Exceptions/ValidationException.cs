using FluentValidation.Results;

namespace LinkPara.SharedModels.Exceptions;

public class ValidationException : ApiException
{
    public ValidationException()
        : base(ErrorCode.ValidationError, "One or more validation errors occurred.")
    {
        Errors = new Dictionary<string, string[]>();
    }

    public ValidationException(IEnumerable<ValidationFailure> failures)
        : this()
    {
        Errors = failures
            .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
            .ToDictionary(failureGroup => failureGroup.Key, failureGroup => failureGroup.ToArray());
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : this()
    {
        Errors = errors;
    }

    public IDictionary<string, string[]> Errors { get; }
}