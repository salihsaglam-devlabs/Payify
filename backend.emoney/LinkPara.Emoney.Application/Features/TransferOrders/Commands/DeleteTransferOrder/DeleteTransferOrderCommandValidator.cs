using FluentValidation;

namespace LinkPara.Emoney.Application.Features.TransferOrders.Commands.DeleteTransferOrder;

public class DeleteTransferOrderCommandValidator : AbstractValidator<DeleteTransferOrderCommand>
{
    public DeleteTransferOrderCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .NotNull();
    }
}
