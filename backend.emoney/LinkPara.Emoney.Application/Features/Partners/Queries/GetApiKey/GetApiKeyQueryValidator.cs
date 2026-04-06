
using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Partners.Queries.GetApiKey
{
    public class GetApiKeyQueryValidator : AbstractValidator<GetApiKeyQuery>
    {
        public GetApiKeyQueryValidator()
        {
            RuleFor(s => s.PublicKeyEncoded)
                .NotNull()
                .NotEmpty();
        }
    }
}
