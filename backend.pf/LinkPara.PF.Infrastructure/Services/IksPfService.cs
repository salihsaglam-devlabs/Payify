using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.HttpProviders.IKS;
using LinkPara.HttpProviders.IKS.Models.Enums;
using LinkPara.HttpProviders.IKS.Models.Request;
using LinkPara.HttpProviders.Location;
using LinkPara.HttpProviders.PF.Models.Request;
using LinkPara.PF.Application.Commons.Exceptions;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Entities.PhysicalPos;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Domain.Enums.PhysicalPos;
using LinkPara.SharedModels.Exceptions;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.PF.Infrastructure.Services;

public class IksPfService : IIksPfService
{
    private readonly ILocationService _locationService;
    private readonly IIKSService _iKSService;
    private readonly IGenericRepository<Vpos> _vposRepository;
    private readonly IGenericRepository<CardLoyalty> _cardLoyaltyRepository;
    private readonly IGenericRepository<VposBankApiInfo> _bankApiInfoRepository;
    private readonly IParameterService _parameterService;
    private readonly IGenericRepository<MerchantVpos> _merchantVposRepository;
    private readonly IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> _physicalPosRepository;
    private readonly ILogger<MerchantService> _logger;
    private readonly IStringLocalizer _localizer;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantPhysicalPos> _merchantPhysicalPosRepository;

    public const int LicenseTagTRMinValue = 1;
    public const int LicenseTagTRMaxValue = 81;
    public const int LicenseTagCyprusMinValue = 991;
    public const int LicenseTagCyprusMaxValue = 996;
    public const int LicenseTagDefaultValue = 99;
    public const string TimeoutStatusCode = "1686";
    public const string AnnulmentStatusCode = "1691";
    public const string CorporateCode = "T";
    public const string IndividualCode = "G";
    public const string EnterpriseCode = "T";
    public const string MerchantOwnerPspNo = "8000";

    public IksPfService(ILocationService locationService,
        IIKSService iKSService,
        IGenericRepository<Vpos> vposRepository,
        IGenericRepository<VposBankApiInfo> bankApiInfoRepository,
        IParameterService parameterService,
        IGenericRepository<CardLoyalty> cardLoyaltyRepository,
        IGenericRepository<MerchantVpos> merchantVposRepository,
        ILogger<MerchantService> logger,
        IStringLocalizerFactory factory,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<Domain.Entities.PhysicalPos.PhysicalPos> physicalPosRepository,
        IGenericRepository<MerchantPhysicalPos> merchantPhysicalPosRepository)
    {
        _locationService = locationService;
        _iKSService = iKSService;
        _vposRepository = vposRepository;
        _bankApiInfoRepository = bankApiInfoRepository;
        _parameterService = parameterService;
        _cardLoyaltyRepository = cardLoyaltyRepository;
        _merchantVposRepository = merchantVposRepository;
        _logger = logger;
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");
        _merchantRepository = merchantRepository;
        _physicalPosRepository = physicalPosRepository;
        _merchantPhysicalPosRepository = merchantPhysicalPosRepository;
    }

    public async Task IKSSaveMerchantAsync(Merchant merchant)
    {
        try
        {
            if (merchant.Customer != null)
            {
                var getCities = await _locationService.GetCityByCode(merchant.Customer.Country);
                var getCityIso2 = getCities.FirstOrDefault(x => x.CityCode == merchant.Customer.City);


                var licenseTag = getCityIso2 != null ? Convert.ToInt32(getCityIso2.Iso2) : 0;

                if (!(licenseTag >= LicenseTagTRMinValue && licenseTag <= LicenseTagTRMaxValue))
                {
                    licenseTag = LicenseTagDefaultValue;
                }

                var mainSellerFlag = merchant.MerchantType switch
                {
                    MerchantType.StandartMerchant => MainSellerFlag.StandardMerchant,
                    MerchantType.MainMerchant => MainSellerFlag.MainMerchant,
                    MerchantType.SubMerchant => MainSellerFlag.SubMerchant,
                    _ => MainSellerFlag.StandardMerchant
                };

                string mainSellerTaxNo = null;
                if (mainSellerFlag == MainSellerFlag.SubMerchant)
                {
                    var parentMerchant = await _merchantRepository.GetAll().Include(s => s.Customer)
                        .FirstOrDefaultAsync(s => s.Id == merchant.ParentMerchantId);
                    mainSellerTaxNo = parentMerchant.Customer.TaxNumber;
                }

                if (merchant.MerchantBusinessPartner is not null && merchant.MerchantBusinessPartner.Any())
                {
                    merchant.MerchantBusinessPartner = merchant.MerchantBusinessPartner.Where(m => m.RecordStatus == RecordStatus.Active).ToList();
                }

                if (merchant.GlobalMerchantId is null)
                {
                    var iksMerchantRequest = new IKSSaveMerchantRequest
                    {
                        MerchantId = merchant.Id,
                        PspMerchantId = merchant.Number,
                        TaxNo = merchant.Customer.TaxNumber,
                        TradeName = merchant.Customer.CommercialTitle,
                        MerchantName = merchant.Name,
                        Address = merchant.Customer.Address,
                        District = merchant.Customer.DistrictName,
                        LicenseTag = licenseTag,
                        CountryCode = merchant.Customer?.Country.ToString().PadLeft(3, '0'),
                        Mcc = merchant.MccCode != null ? Convert.ToInt32(merchant.MccCode) : 0,
                        ManagerName = $"{merchant.Customer.AuthorizedPerson.Name} {merchant.Customer.AuthorizedPerson.Surname}",
                        Phone = merchant.Customer.AuthorizedPerson?.CompanyPhoneNumber,
                        ZipCode = merchant.Customer.PostalCode,
                        TaxOfficeName = merchant.Customer.TaxAdministration,
                        MainSellerFlag = mainSellerFlag,
                        MainSellerTaxNo = mainSellerTaxNo,
                        CommercialType = merchant.Customer.CompanyType == CompanyType.Individual ? IndividualCode :
                        merchant.Customer.CompanyType == CompanyType.Corporate ? CorporateCode :
                        merchant.Customer.CompanyType == CompanyType.Enterprise ? EnterpriseCode : null,
                        EstablishmentDate = merchant.EstablishmentDate.ToString("dd.MM.yyy"),
                        BusinessModel = merchant.BusinessModel.ToString(),
                        BusinessActivity = merchant.BusinessActivity,
                        BranchCount = merchant.BranchCount,
                        EmployeeCount = merchant.EmployeeCount,
                        ManagerBirthDate = merchant.Customer.AuthorizedPerson.BirthDate.ToString("dd.MM.yyy"),
                        ExpectedRevenue = merchant.MonthlyTurnover,
                        WebsiteUrl = merchant.WebSiteUrl,
                        MerchantBusinessPartners = new List<MerchantBusinessPartnerRequest>()
                    };

                    if (merchant.MerchantBusinessPartner is null || !merchant.MerchantBusinessPartner.Any())
                    {
                        iksMerchantRequest.MerchantBusinessPartners.Add(new MerchantBusinessPartnerRequest
                        {
                            Type = "G",
                            IdentityNo = merchant.Customer.AuthorizedPerson.IdentityNumber,
                            Name = merchant.Customer.AuthorizedPerson.Name + " " + merchant.Customer.AuthorizedPerson.Surname,
                            BirthDate = merchant.Customer.AuthorizedPerson.BirthDate.ToString("dd.MM.yyy")
                        });
                    }
                    else
                    {
                        if (!merchant.MerchantBusinessPartner.Any())
                        {
                            throw new IKSGeneralException("MerchantBusinessPartner Not Found");
                        }

                        iksMerchantRequest.MerchantBusinessPartners = merchant.MerchantBusinessPartner.Select(x =>
                            new MerchantBusinessPartnerRequest
                            {
                                Type = "G",
                                IdentityNo = x.IdentityNumber,
                                Name = x.FirstName + " " + x.LastName,
                                BirthDate = x.BirthDate.ToString("dd.MM.yyy")
                            }).ToList();
                    }

                    var result = await _iKSService.SaveMerchantAsync(iksMerchantRequest);

                    if (result.IsSuccess)
                    {
                        if (result.StatusCode.Contains(TimeoutStatusCode))
                        {
                            merchant.MerchantStatus = MerchantStatus.PendingIKS;
                        }
                        else
                        {
                            merchant.GlobalMerchantId = result?.Data?.GlobalMerchantId;

                            var annulmentAdditionalInfo = result.Data?.AdditionalInfo?.FirstOrDefault(x => x.Code == AnnulmentStatusCode);
                            if (annulmentAdditionalInfo != null && merchant.GlobalMerchantId != null)
                            {
                                var annulmentMerchant = await _merchantRepository.GetAll().FirstOrDefaultAsync(x => x.GlobalMerchantId == merchant.GlobalMerchantId && x.Id != merchant.Id && x.MerchantStatus == MerchantStatus.Annulment);

                                merchant.AnnulmentAdditionalInfo = _localizer.GetString("CompanyHasAlreadyAnnulmentRecord").Value;
                                merchant.MerchantStatus = annulmentMerchant != null && annulmentMerchant.GlobalMerchantId != null ? MerchantStatus.Annulment : MerchantStatus.Closed;
                            }
                            else if (annulmentAdditionalInfo is null && merchant.GlobalMerchantId != null)
                            {
                                await IKSCreateTerminalAsync(merchant);
                            }
                        }
                    }
                    else
                    {
                        throw new IKSGeneralException(result.Error.moreInformation);
                    }
                }
                else
                {
                    var iksUpdateMerchantRequest = new IKSUpdateMerchantRequest
                    {
                        MerchantId = merchant.Id,
                        GlobalMerchantId = merchant.GlobalMerchantId,
                        PspMerchantId = merchant.Number,
                        TaxNo = merchant.Customer.TaxNumber,
                        TradeName = merchant.Customer.CommercialTitle,
                        MerchantName = merchant.Name,
                        Address = merchant.Customer.Address,
                        District = merchant.Customer.DistrictName,
                        LicenseTag = licenseTag,
                        CountryCode = merchant.Customer?.Country.ToString().PadLeft(3, '0'),
                        Mcc = merchant.MccCode != null ? Convert.ToInt32(merchant.MccCode) : 0,
                        ManagerName = $"{merchant.Customer.AuthorizedPerson.Name} {merchant.Customer.AuthorizedPerson.Surname}",
                        Phone = merchant.Customer.AuthorizedPerson?.CompanyPhoneNumber,
                        ZipCode = merchant.Customer.PostalCode,
                        TaxOfficeName = merchant.Customer.TaxAdministration,
                        MainSellerFlag = mainSellerFlag,
                        MainSellerTaxNo = mainSellerTaxNo,
                        CommercialType = merchant.Customer.CompanyType == CompanyType.Individual ? IndividualCode :
                        merchant.Customer.CompanyType == CompanyType.Corporate ? CorporateCode :
                        merchant.Customer.CompanyType == CompanyType.Enterprise ? EnterpriseCode : null,
                        StatusCode = merchant.MerchantStatus == MerchantStatus.Active ? ((int)StatusCode.ActiveStatusCode).ToString()
                                    : ((int)StatusCode.PassiveStatusCode).ToString(),
                        TerminationCode = merchant.ParameterValue,
                        EstablishmentDate = merchant.EstablishmentDate.ToString("dd.MM.yyy"),
                        BusinessModel = merchant.BusinessModel.ToString(),
                        BusinessActivity = merchant.BusinessActivity,
                        BranchCount = merchant.BranchCount,
                        EmployeeCount = merchant.EmployeeCount,
                        ManagerBirthDate = merchant.Customer.AuthorizedPerson.BirthDate.ToString("dd.MM.yyy"),
                        ExpectedRevenue = merchant.MonthlyTurnover,
                        WebsiteUrl = merchant.WebSiteUrl,
                        MerchantBusinessPartners = new List<MerchantBusinessPartnerRequest>()
                    };

                    if (merchant.MerchantBusinessPartner is null || !merchant.MerchantBusinessPartner.Any())
                    {
                        iksUpdateMerchantRequest.MerchantBusinessPartners.Add(new MerchantBusinessPartnerRequest
                        {
                            Type = "G",
                            IdentityNo = merchant.Customer.AuthorizedPerson.IdentityNumber,
                            Name = merchant.Customer.AuthorizedPerson.Name + " " + merchant.Customer.AuthorizedPerson.Surname,
                            BirthDate = merchant.Customer.AuthorizedPerson.BirthDate.ToString("dd.MM.yyy")
                        });
                    }
                    else
                    {
                        if (!merchant.MerchantBusinessPartner.Any())
                        {
                            throw new IKSGeneralException("MerchantBusinessPartner Not Found");
                        }

                        iksUpdateMerchantRequest.MerchantBusinessPartners = merchant.MerchantBusinessPartner.Select(x =>
                            new MerchantBusinessPartnerRequest
                            {
                                Type = "G",
                                IdentityNo = x.IdentityNumber,
                                Name = x.FirstName + " " + x.LastName,
                                BirthDate = x.BirthDate.ToString("dd.MM.yyy")
                            }).ToList();
                    }

                    var result = await _iKSService.UpdateMerchantAsync(iksUpdateMerchantRequest);

                    if (result.IsSuccess)
                    {
                        if (result.StatusCode.Contains(TimeoutStatusCode))
                        {
                            merchant.MerchantStatus = MerchantStatus.PendingIKS;
                        }
                        else
                        {
                            merchant.GlobalMerchantId = result?.Data?.GlobalMerchantId;
                            await IKSCreateTerminalAsync(merchant);
                        }
                    }
                    else
                    {
                        throw new IKSGeneralException(result.Error.moreInformation);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"IKSMerchantOperationCommand : {exception}");
            throw;
        }
    }

    public async Task IKSSaveTerminalAsync(Merchant merchant)
    {
        try
        {
            var vposList = merchant.MerchantVposList.Where(x => !x.IsTerminalNotification && x.RecordStatus == RecordStatus.Active).ToList();
            foreach (var item in vposList.Where(item => item.SubMerchantCode is not null))
            {
                var vpos = await _vposRepository.GetAll()
                    .Include(x => x.AcquireBank)
                    .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                              && x.Id == item.VposId);

                if (vpos is null)
                {
                    continue;
                }

                var iksTerminalParameters = await _parameterService.GetParametersAsync("IKS_Terminal");

                var ownerPspNo = iksTerminalParameters.FirstOrDefault(x => x.ParameterCode == "OwnerPspNo")?.ParameterValue;
                var serviceProviderPspNo = iksTerminalParameters.FirstOrDefault(x => x.ParameterCode == "ServiceProviderPspNo")?.ParameterValue;
                var paymentGwTaxNo = iksTerminalParameters.FirstOrDefault(x => x.ParameterCode == "PaymentGwTaxNo")?.ParameterValue;

                var iksTerminalRequest = new IKSSaveTerminalRequest
                {
                    MerchantId = merchant.Id,
                    GlobalMerchantId = merchant.GlobalMerchantId,
                    PspMerchantId = merchant.Number,
                    TerminalId = item.SubMerchantCode,
                    BrandSharing = await BrandSharing(vpos.AcquireBank?.BankCode ?? 0),
                    VirtualPosUrl = merchant.WebSiteUrl,
                    OwnerPspNo = ownerPspNo != null ? Convert.ToInt32(ownerPspNo) : 0,
                    PaymentGwTaxNo = paymentGwTaxNo,
                    ServiceProviderPspNo = serviceProviderPspNo != null ? Convert.ToInt32(serviceProviderPspNo) : 0,
                    HostingTaxNo = merchant.HostingTaxNo
                };

                var result = await _iKSService.SaveTerminalAsync(iksTerminalRequest);

                item.IsTerminalNotification = result.IsSuccess;

                var merchantVpos = await _merchantVposRepository.GetAll()
                    .Include(x => x.Vpos)
                    .ThenInclude(x => x.AcquireBank)
                    .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                              && x.Id == item.Id);

                if (merchantVpos is not null)
                {
                    await _merchantVposRepository.UpdateAsync(item);
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveTerminalError : {exception}");
        }
    }

    public async Task IKSUpdateTerminalAsync(Merchant merchant, MerchantVpos merchantVpos)
    {
        try
        {
            var vpos = await _vposRepository.GetAll()
                                    .Include(x => x.AcquireBank)
                                     .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                     && x.IsOnUsVpos != true && x.IsTopUpVpos != true
                                     && x.Id == merchantVpos.VposId);

            if (vpos is not null)
            {
                if (!string.IsNullOrEmpty(merchantVpos.SubMerchantCode))
                {
                    var bankApiInfo = await _bankApiInfoRepository.GetAll().Include(b => b.Key).Where(b => b.VposId == vpos.Id && b.RecordStatus == RecordStatus.Active).ToListAsync();

                    var pfMainMerchantId = bankApiInfo.Where(b => b.Key.IsPfMainMerchantId == true).FirstOrDefault();

                    if (!bankApiInfo.Any() || pfMainMerchantId is null)
                    {
                        throw new NotFoundException(nameof(VposBankApiInfo));
                    }

                    var iksTerminalRequest = new IKSUpdateTerminalRequest
                    {
                        MerchantId = merchant.Id,
                        GlobalMerchantId = merchant.GlobalMerchantId,
                        PspMerchantId = merchant.Number,
                        TerminalId = merchantVpos.SubMerchantCode,
                        BrandSharing = await BrandSharing(vpos?.AcquireBank?.BankCode == null ? 0 : vpos.AcquireBank.BankCode),
                        VirtualPosUrl = merchant.WebSiteUrl,
                        StatusCode = ((int)StatusCode.PassiveStatusCode).ToString(),
                        OwnerPspNo = vpos.AcquireBank.BankCode,
                        PaymentGwTaxNo = vpos.AcquireBank.PaymentGwTaxNo,
                        ServiceProviderPspNo = vpos.AcquireBank.BankCode,
                        HostingTaxNo = merchant.HostingTaxNo,
                        PaymentGwTradeName = vpos.AcquireBank.PaymentGwTradeName,
                        PaymentGwUrl = vpos.AcquireBank.PaymentGwUrl,
                        PfMainMerchantId = pfMainMerchantId.Value,
                        HostingTradeName = merchant.HostingTradeName,
                        HostingUrl = merchant.HostingUrl,
                        ReferenceCode = merchantVpos.BkmReferenceNumber,
                        Type = "S"
                    };

                    var result = await _iKSService.UpdateTerminalAsync(iksTerminalRequest);

                    if (result.IsSuccess)
                    {
                        merchantVpos.IsTerminalNotification = false;
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdateTerminalError : {exception}");
            throw;
        }
    }

    public async Task UpdateMerchantStatus(Merchant merchant)
    {
        var getCities = await _locationService.GetCityByCode(merchant.Customer.Country);
        var getCityIso2 = getCities.FirstOrDefault(x => x.CityCode == merchant.Customer.City);


        var licenseTag = getCityIso2 != null ? Convert.ToInt32(getCityIso2.Iso2) : 0;

        if (!(licenseTag >= LicenseTagTRMinValue && licenseTag <= LicenseTagTRMaxValue)
            && !(LicenseTagCyprusMinValue >= 991 && LicenseTagCyprusMaxValue <= 996))
        {
            licenseTag = LicenseTagDefaultValue;
        }

        var mainSellerFlag = merchant.MerchantType switch
        {
            MerchantType.StandartMerchant => MainSellerFlag.StandardMerchant,
            MerchantType.MainMerchant => MainSellerFlag.MainMerchant,
            MerchantType.SubMerchant => MainSellerFlag.SubMerchant,
            _ => MainSellerFlag.StandardMerchant
        };

        string mainSellerTaxNo = null;
        if (mainSellerFlag == MainSellerFlag.SubMerchant)
        {
            var parentMerchant = await _merchantRepository.GetAll().Include(s => s.Customer)
                .FirstOrDefaultAsync(s => s.Id == merchant.ParentMerchantId);
            mainSellerTaxNo = parentMerchant.Customer.TaxNumber;
        }

        var iksUpdateMerchantRequest = new IKSUpdateMerchantRequest
        {
            MerchantId = merchant.Id,
            GlobalMerchantId = merchant.GlobalMerchantId,
            PspMerchantId = merchant.Number,
            TaxNo = merchant.Customer.TaxNumber,
            TradeName = merchant.Customer.CommercialTitle,
            MerchantName = merchant.Name,
            Address = merchant.Customer.Address,
            District = merchant.Customer.DistrictName,
            LicenseTag = licenseTag,
            CountryCode = merchant.Customer?.Country.ToString().PadLeft(3, '0'),
            Mcc = merchant.MccCode != null ? Convert.ToInt32(merchant.MccCode) : 0,
            ManagerName = $"{merchant.Customer.AuthorizedPerson.Name} {merchant.Customer.AuthorizedPerson.Surname}",
            Phone = merchant.Customer.AuthorizedPerson?.CompanyPhoneNumber,
            ZipCode = merchant.Customer.PostalCode,
            TaxOfficeName = merchant.Customer.TaxAdministration,
            MainSellerFlag = mainSellerFlag,
            MainSellerTaxNo = mainSellerTaxNo,
            CommercialType = merchant.Customer.CompanyType == CompanyType.Individual ? IndividualCode :
                              merchant.Customer.CompanyType == CompanyType.Corporate ? CorporateCode :
                              merchant.Customer.CompanyType == CompanyType.Enterprise ? EnterpriseCode : null,
            StatusCode = ((int)StatusCode.ActiveStatusCode).ToString()
        };

        var updateMerchantStatusResult = await _iKSService.UpdateMerchantAsync(iksUpdateMerchantRequest);

        if (updateMerchantStatusResult.IsSuccess && updateMerchantStatusResult.StatusCode.Contains(TimeoutStatusCode))
        {
            merchant.MerchantStatus = MerchantStatus.PendingIKS;
        }
    }

    public async Task<string> BrandSharing(int bankCode)
    {
        var loyalty = await _cardLoyaltyRepository.GetAll()
                    .FirstOrDefaultAsync(x => x.BankCode == bankCode);

        if (loyalty?.Name is not null)
        {
            if (loyalty.Name.Contains("World"))
            {
                return "W";
            }
            else if (loyalty.Name.Contains("Axess"))
            {
                return "X";
            }
            else if (loyalty.Name.Contains("Bonus"))
            {
                return "B";
            }
            else if (loyalty.Name.Contains("Maksimum"))
            {
                return "M";
            }
            else if (loyalty.Name.Contains("Advantage"))
            {
                return "A";
            }
            else
            {
                return "0";
            }
        }
        return null;

    }

    public async Task IKSCreateTerminalAsync(Merchant merchant)
    {
        try
        {
            var vposList = merchant?.MerchantVposList?.Where(x => !x.IsTerminalNotification && x.RecordStatus == RecordStatus.Active).ToList();
            foreach (var item in vposList.Where(item => item.TerminalStatus == TerminalStatus.PendingRequest))
            {
                var vpos = await _vposRepository.GetAll()
                    .Include(x => x.AcquireBank)
                    .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                              && x.Id == item.VposId);

                if (vpos is null)
                {
                    continue;
                }

                if (vpos is not null && (vpos.IsOnUsVpos == true || vpos.IsTopUpVpos == true))
                {
                    item.TerminalStatus = TerminalStatus.Active;
                    await UpdateMerchantVposAsync(item);
                    continue;
                }

                var bankApiInfo = await _bankApiInfoRepository.GetAll().Include(b => b.Key).Where(b => b.VposId == vpos.Id && b.RecordStatus == RecordStatus.Active).ToListAsync();

                var pfMainMerchantId = bankApiInfo.Where(b => b.Key.IsPfMainMerchantId == true).FirstOrDefault();

                if (!bankApiInfo.Any() || pfMainMerchantId is null)
                {
                    throw new NotFoundException(nameof(VposBankApiInfo));
                }

                var iksTerminalRequest = new IKSCreateTerminalRequest
                {
                    MerchantId = merchant.Id,
                    VposId = vpos.Id,
                    GlobalMerchantId = merchant.GlobalMerchantId,
                    PspMerchantId = merchant.Number,
                    VirtualPosUrl = merchant.WebSiteUrl,
                    OwnerPspNo = vpos.AcquireBank.BankCode,
                    PaymentGwTaxNo = vpos.AcquireBank.PaymentGwTaxNo,
                    ServiceProviderPspNo = vpos.AcquireBank.BankCode,
                    HostingTaxNo = merchant.HostingTaxNo,
                    HostingTradeName = merchant.HostingTradeName,
                    HostingUrl = merchant.HostingUrl,
                    PaymentGwTradeName = vpos.AcquireBank.PaymentGwTradeName,
                    PaymentGwUrl = vpos.AcquireBank.PaymentGwUrl,
                    BrandSharing = await BrandSharing(vpos?.AcquireBank?.BankCode == null ? 0 : vpos.AcquireBank.BankCode),
                    PfMainMerchantId = pfMainMerchantId.Value,
                    Type = "S"
                };

                var result = await _iKSService.CreateTerminalAsync(iksTerminalRequest);

                item.IsTerminalNotification = result.IsSuccess;
                item.BkmReferenceNumber = result.Data.ReferenceCode;
                item.TerminalStatus = TerminalStatus.SentRequest;

                await UpdateMerchantVposAsync(item);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError($"SaveTerminalError : {exception}");
        }
    }

    public async Task IKSCreatePhysicalTerminalAsync(Merchant merchant)
    {

        var merchantPhysicalDeviceList = merchant?.MerchantPhysicalDevices?.Where(b => b.RecordStatus == RecordStatus.Active).ToList();

        foreach (var item in merchantPhysicalDeviceList)
        {
            var physicalPosList = item.MerchantPhysicalPosList.Where(b => b.RecordStatus == RecordStatus.Active).ToList();

            foreach (var physicalPosItem in physicalPosList.Where(item => item.TerminalStatus == TerminalStatus.PendingRequest))
            {
                try
                {
                    var physicalPos = await _physicalPosRepository.GetAll().Include(b => b.AcquireBank).FirstOrDefaultAsync(b => b.Id == physicalPosItem.PhysicalPosId && b.RecordStatus == RecordStatus.Active);

                    if (physicalPos is null)
                    {
                        _logger.LogError($"IKSCreatePhysicalTerminalAsync PhysicalPos Not Found {physicalPosItem.PhysicalPosId}");
                        continue;
                    }

                    var ownerPspNo = MerchantOwnerPspNo;
                    if (item.AssignmentType == AssignmentType.Pf)
                    {
                        var establishmentCode = await _parameterService.GetParameterAsync("PfEstablishmentCode", "EstablishmentCode");
                        ownerPspNo = establishmentCode.ParameterValue;
                    }

                    var conType = item.ConnectionType switch
                    {
                        ConnectionType.GSM => "S",
                        ConnectionType.ADSL => "A",
                        ConnectionType.ISDN => "I",
                        ConnectionType.DialUp => "D",
                        ConnectionType.Wifi => "W",
                        ConnectionType.Ethernet => "E",
                        ConnectionType.GPRS => "G",
                        _ => "W"
                    };

                    var iksTerminalRequest = new IKSCreateTerminalRequest
                    {
                        MerchantId = merchant.Id,
                        PhysicalPosId = physicalPos.Id,
                        GlobalMerchantId = merchant.GlobalMerchantId,
                        PspMerchantId = merchant.Number,
                        OwnerPspNo = Convert.ToInt32(ownerPspNo),
                        OwnerTerminalId = item.OwnerTerminalId,
                        ServiceProviderPspNo = physicalPos.AcquireBank.BankCode,
                        PfMainMerchantId = physicalPos.PfMainMerchantId,
                        Type = "O",
                        TechPos = 1,
                        ConnectionType = conType,
                        Contactless = (int)item.DeviceInventory.ContactlessSeparator,
                        FiscalNo = item.FiscalNo,
                        Model = item.DeviceInventory.DeviceModel.ToString(),
                        PinPad = item.IsPinPad == true ? 0 : 1,
                        SerialNo = item.DeviceInventory.SerialNo,
                        BrandCode = "X"
                    };

                    var result = await _iKSService.CreateTerminalAsync(iksTerminalRequest);

                    physicalPosItem.BkmReferenceNumber = result.Data.ReferenceCode;
                    physicalPosItem.TerminalStatus = TerminalStatus.SentRequest;

                    await UpdateMerchantPhysicalPosAsync(physicalPosItem);
                }
                catch (Exception exception)
                {
                    _logger.LogError($"SavePhysicalTerminalError : {exception}");
                    continue;
                }
            }
        }
    }

    public async Task<bool> IKSUpdatePhysicalTerminalAsync(Merchant merchant, MerchantPhysicalPos merchantPhysicalPos)
    {
        try
        {
            var physicalPos = await _physicalPosRepository.GetAll().Include(x => x.AcquireBank).FirstOrDefaultAsync(b => b.Id == merchantPhysicalPos.PhysicalPosId);


            if (physicalPos is not null)
            {
                var ownerPspNo = MerchantOwnerPspNo;
                if (merchantPhysicalPos.MerchantPhysicalDevice.AssignmentType == Domain.Enums.PhysicalPos.AssignmentType.Pf)
                {
                    var establishmentCode = await _parameterService.GetParameterAsync("PfEstablishmentCode", "EstablishmentCode");
                    ownerPspNo = establishmentCode.ParameterValue;
                }

                var conType = merchantPhysicalPos.MerchantPhysicalDevice.ConnectionType switch
                {
                    ConnectionType.GSM => "S",
                    ConnectionType.ADSL => "A",
                    ConnectionType.ISDN => "I",
                    ConnectionType.DialUp => "D",
                    ConnectionType.Wifi => "W",
                    ConnectionType.Ethernet => "E",
                    ConnectionType.GPRS => "G",
                    _ => "W"
                };

                var iksTerminalRequest = new IKSUpdateTerminalRequest
                {
                    MerchantId = merchant.Id,
                    GlobalMerchantId = merchant.GlobalMerchantId,
                    PspMerchantId = merchant.Number,
                    OwnerPspNo = Convert.ToInt32(ownerPspNo),
                    ServiceProviderPspNo = physicalPos.AcquireBank.BankCode,
                    PfMainMerchantId = physicalPos.PfMainMerchantId,
                    Type = "O",
                    ConnectionType = conType,
                    TechPos = 1,
                    Contactless = (int)merchantPhysicalPos.MerchantPhysicalDevice.DeviceInventory.ContactlessSeparator,
                    FiscalNo = merchantPhysicalPos.MerchantPhysicalDevice.FiscalNo,
                    Model = merchantPhysicalPos.MerchantPhysicalDevice.DeviceInventory.DeviceModel.ToString(),
                    PinPad = merchantPhysicalPos.MerchantPhysicalDevice.IsPinPad == true ? 0 : 1,
                    SerialNo = merchantPhysicalPos.MerchantPhysicalDevice.DeviceInventory.SerialNo,
                    StatusCode = ((int)StatusCode.PassiveStatusCode).ToString(),
                    ReferenceCode = merchantPhysicalPos.BkmReferenceNumber,
                    TerminalId = merchantPhysicalPos.PosTerminalId,
                    OwnerTerminalId = merchantPhysicalPos.MerchantPhysicalDevice.OwnerTerminalId,
                    BrandCode = "X"
                };

                var result = await _iKSService.UpdateTerminalAsync(iksTerminalRequest);

                if (result.IsSuccess)
                {
                    return true;
                }
            }

        }
        catch (Exception exception)
        {
            _logger.LogError($"UpdatePhysicalTerminalError : {exception}");
        }

        return false;
    }

    private async Task UpdateMerchantVposAsync(MerchantVpos merchantVposItem)
    {
        var merchantVpos = await _merchantVposRepository.GetAll()
                    .Include(x => x.Vpos)
                    .ThenInclude(x => x.AcquireBank)
                    .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                              && x.Id == merchantVposItem.Id);

        if (merchantVpos is not null)
        {
            await _merchantVposRepository.UpdateAsync(merchantVposItem);
        }
    }

    private async Task UpdateMerchantPhysicalPosAsync(MerchantPhysicalPos merchantPhysicalPosItem)
    {
        var merchantVpos = await _merchantPhysicalPosRepository.GetAll()
                    .Include(x => x.PhysicalPos)
                    .ThenInclude(x => x.AcquireBank)
                    .FirstOrDefaultAsync(x => x.RecordStatus == RecordStatus.Active
                                              && x.Id == merchantPhysicalPosItem.Id);

        if (merchantVpos is not null)
        {
            await _merchantPhysicalPosRepository.UpdateAsync(merchantPhysicalPosItem);
        }
    }
}
