using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.CreatePaymentOrder;

public class CreatePaymentOrderCommandValidator : AbstractValidator<CreatePaymentOrderCommand>
{
    public CreatePaymentOrderCommandValidator()
    {
        RuleFor(u => u.Contract).NotNull();
        RuleFor(u => u.Contract.RzBlg).NotNull();
        RuleFor(u => u.Contract.RzBlg.RizaNo).NotNull().NotEmpty();
        RuleFor(u => u.Contract.RzBlg.OlusZmn).NotNull().NotEmpty();
        RuleFor(u => u.Contract.RzBlg.RizaDrm).NotNull().NotEmpty();

        RuleFor(u => u.Contract.KatilimciBlg).NotNull();
        RuleFor(u => u.Contract.KatilimciBlg.HhsCode).NotNull().NotEmpty();
        RuleFor(u => u.Contract.KatilimciBlg.YosCode).NotNull().NotEmpty();

        RuleFor(u => u.Contract.Gkd).NotNull();
        RuleFor(u => u.Contract.Gkd.YetYntm).NotNull().NotEmpty();
        RuleFor(u => u.Contract.Gkd.HhsYonAdr).NotNull().NotEmpty();
        RuleFor(u => u.Contract.Gkd.YonAdr).NotNull().NotEmpty();
        RuleFor(u => u.Contract.Gkd.YetTmmZmn).NotNull().NotEmpty();


        RuleFor(u => u.Contract.OdmBsltm).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.Kmlk).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.Kmlk.KmlkTur).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.Kmlk.KmlkVrs).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.Kmlk.OhkTur).NotNull().NotEmpty();

        RuleFor(u => u.Contract.OdmBsltm.IslTtr).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.IslTtr.Ttr).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.IslTtr.PrBrm).NotNull().NotEmpty();

        RuleFor(u => u.Contract.OdmBsltm.Gon).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.Gon.Unv).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.Gon.HspRef).NotNull().NotEmpty();

        RuleFor(u => u.Contract.OdmBsltm.Alc).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.Alc.Unv).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.Alc.HspRef).NotNull().NotEmpty();

        RuleFor(u => u.Contract.OdmBsltm.OdmAyr).NotNull();
        RuleFor(u => u.Contract.OdmBsltm.OdmAyr.OdmKynk).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.OdmAyr.OdmAmc).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.OdmAyr.OhkMsj).NotNull().NotEmpty();
        RuleFor(u => u.Contract.OdmBsltm.OdmAyr.OdmStm).NotNull().NotEmpty();
    }
}
