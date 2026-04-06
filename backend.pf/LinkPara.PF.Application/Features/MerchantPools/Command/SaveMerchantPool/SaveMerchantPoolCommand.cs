using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPools.Command.SaveMerchantPool;

public class SaveMerchantPoolCommand : IRequest, IMapFrom<MerchantPool>
{
    public string MerchantName { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid? ParentMerchantId { get; set; }
    public string ParentMerchantName { get; set; }
    public string ParentMerchantNumber { get; set; }
    public bool IsInvoiceCommissionReflected { get; set; }
    public CompanyType CompanyType { get; set; }
    public string CommercialTitle { get; set; }
    public string WebSiteUrl { get; set; }
    public decimal MonthlyTurnover { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public string PhoneCode { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string TradeRegistrationNumber { get; set; }
    public string Iban { get; set; }
    public int? BankCode { get; set; }
    public bool IsPaymentToMainMerchant { get; set; }
    public PostingPaymentChannel PostingPaymentChannel { get; set; }
    public string WalletNumber { get; set; }
    public string RejectReason { get; set; }
    public string Email { get; set; }
    public string CompanyEmail { get; set; }
    public string AuthorizedPersonIdentityNumber { get; set; }
    public string AuthorizedPersonName { get; set; }
    public string AuthorizedPersonSurname { get; set; }
    public DateTime AuthorizedPersonBirthDate { get; set; }
    public string AuthorizedPersonCompanyPhoneNumber { get; set; }
    public string AuthorizedPersonMobilePhoneNumber { get; set; }
    public string AuthorizedPersonMobilePhoneNumberSecond { get; set; }
    public PosType PosType { get; set; }
    public int? MoneyTransferStartHour { get; set; }
    public int? MoneyTransferStartMinute { get; set; }
}

public class SaveMerchantPoolCommandHandler : IRequestHandler<SaveMerchantPoolCommand>
{
    private readonly IMerchantPoolService _merchantPoolService;

    public SaveMerchantPoolCommandHandler(IMerchantPoolService merchantPoolService)
    {
        _merchantPoolService = merchantPoolService;
    }
    public async Task<Unit> Handle(SaveMerchantPoolCommand request, CancellationToken cancellationToken)
    {
        await _merchantPoolService.SaveAsync(request);

        return Unit.Value;
    }
}
