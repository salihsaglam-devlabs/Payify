using FluentValidation;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Queries.GetTransferOrderById;

public class GetTransferOrderByIdQueryValidator : AbstractValidator<GetTransferOrderByIdQuery>
{
    public GetTransferOrderByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
    }
}

