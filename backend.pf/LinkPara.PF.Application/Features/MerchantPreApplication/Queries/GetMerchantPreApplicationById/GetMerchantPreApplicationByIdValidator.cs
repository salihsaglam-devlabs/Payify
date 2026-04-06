using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPreApplication.Queries.GetMerchantPreApplicationById;

public class GetMerchantPreApplicationByIdValidator : AbstractValidator<GetMerchantPreApplicationByIdQuery>
{
    public GetMerchantPreApplicationByIdValidator()
    {
        RuleFor(p => p.Id)
            .NotEmpty().WithMessage("Id is required.");
    }
}