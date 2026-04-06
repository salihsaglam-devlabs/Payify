using FluentValidation;

namespace LinkPara.Epin.Application.Features.Products.Queries.GetFilterProducts;

public class GetFilterProductsQueryValidator : AbstractValidator<GetFilterProductsQuery>
{
    public GetFilterProductsQueryValidator()
    {
        RuleFor(x => x.BrandId)
            .NotNull().NotEmpty();
        RuleFor(x => x.PublisherId)
            .NotNull().NotEmpty();
    }
}
