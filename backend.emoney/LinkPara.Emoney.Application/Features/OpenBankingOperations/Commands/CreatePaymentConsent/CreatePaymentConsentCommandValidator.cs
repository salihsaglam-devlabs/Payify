using FluentValidation;
using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentConsent;

public class CreatePaymentConsentCommandValidator : AbstractValidator<CreatePaymentConsentCommand>
{
    public CreatePaymentConsentCommandValidator()
    {

        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
        RuleFor(x => x.YonTipi).NotEmpty().NotNull();
        RuleFor(x => x.OhkTur).NotEmpty().NotNull();
        RuleFor(x => x.DrmKod).NotEmpty().NotNull();
        RuleFor(x => x.IslTtr).NotEmpty().NotNull();
        RuleFor(x => x.IslTtr.PrBrm).NotEmpty().NotNull();
        RuleFor(x => x.IslTtr.Ttr).NotEmpty().NotNull();
        RuleFor(x => x.Alc).NotEmpty().NotNull();
        RuleFor(x => x.Alc.HspNo).NotEmpty().NotNull();
        RuleFor(x => x.Alc.Unv).NotEmpty().NotNull();
        RuleFor(x => x.Gon).NotEmpty().NotNull();
        RuleFor(x => x.Gon.Unv).NotEmpty().NotNull();
        RuleFor(x => x.OdmAyr).NotEmpty().NotNull();
        RuleFor(x => x.OdmAyr.OdmKynk).NotEmpty().NotNull();
        RuleFor(x => x.OdmAyr.OdmAmc).NotEmpty().NotNull();
        RuleFor(x => x.OdmAyr.RefBlg).NotEmpty().NotNull();
        RuleFor(x => x.OdmBsltm).NotEmpty().NotNull();
        RuleFor(x => x.OdmBsltm.Kmlk).NotEmpty().NotNull();
        RuleFor(x => x.OdmBsltm.Kmlk.OhkTur).NotEmpty().NotNull();
        RuleFor(x => x.OdmBsltm.Kmlk.KmlkTur).NotEmpty().NotNull();
        RuleFor(x => x.OdmBsltm.Kmlk.KmlkVrs).NotEmpty().NotNull();
    }
}
