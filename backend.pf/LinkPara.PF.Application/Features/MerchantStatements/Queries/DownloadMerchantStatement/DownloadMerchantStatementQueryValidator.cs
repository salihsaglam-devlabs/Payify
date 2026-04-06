using FluentValidation;
using LinkPara.PF.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.MerchantStatements.Queries.DownloadMerchantStatement;

public class DownloadMerchantStatementQueryValidator : AbstractValidator<DownloadMerchantStatementQuery>
{
    public DownloadMerchantStatementQueryValidator(IStringLocalizerFactory factory)
    {
        var localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        
        RuleFor(x => x.Id)
            .NotNull().NotEmpty();
        
        RuleFor(x => x.StatementType)
            .IsInEnum()
            .Must(value => value is MerchantStatementType.Excel or MerchantStatementType.PDF)
            .WithMessage(localizer.GetString("InvalidMerchantStatementTypeException").Value);

    }
}
