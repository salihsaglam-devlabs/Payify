using FluentValidation;

namespace LinkPara.PF.Application.Features.Merchants.Queries.GetApiKeys;

public class GetApiKeysQueryValidator : AbstractValidator<GetApiKeysQuery>
{
    public GetApiKeysQueryValidator()
    {
        RuleFor(s => s.PublicKeyEncoded).NotNull().NotEmpty();
    }
}