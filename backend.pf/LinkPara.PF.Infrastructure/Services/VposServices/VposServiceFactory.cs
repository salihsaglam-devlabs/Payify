using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.NestPayInsuranceVpos;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos;
using LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.PosnetVpos.Models;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos;
using LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Models;
using LinkPara.SharedModels.Banking.Enums;
using MassTransit;

namespace LinkPara.PF.Infrastructure.Services.VposServices;

public class VposServiceFactory
{
    private readonly IServiceProvider _serviceProvider; 
    public VposServiceFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IVposApi GetVposServices(Vpos vpos, Guid? merchantId, bool? isInsuranceVpos = false)
    {
        var bankCode = vpos.AcquireBank.BankCode;

        if (isInsuranceVpos == true)
        {
            switch (bankCode)
            {
                case (int)BankCode.Halkbank:
                    {
                        var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                        posInfo.BankCode = (int)BankCode.Halkbank;
                        var service = _serviceProvider.GetService(typeof(NestPayInsuranceVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.VakifBank:
                    {
                        var posInfo = GetApiInfo<VakifInsuranceVposInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(VakifInsuranceVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                default:
                {
                    var posInfo = GetApiInfo<VakifPosInfo>(vpos);
                    var service = _serviceProvider.GetService(typeof(VakifInsuranceVpos)) as IVposApi;
                    service.SetServiceParameters(posInfo);
                    return service;
                };
            }
        }
        else
        {
            switch (bankCode)
            {
                case (int)BankCode.Denizbank:
                    {
                        var posInfo = GetApiInfo<IvpPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(InterVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.VakifBank:
                    {
                        var posInfo = GetApiInfo<VakifPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(VakifVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.IsBank:
                    {
                        var posInfo = GetApiInfo<IsbankPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(IsbankVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.Ziraat:
                    {
                        var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                        posInfo.BankCode = (int)BankCode.Ziraat;
                        var service = _serviceProvider.GetService(typeof(NestPayVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.AnadoluBank:
                {
                    var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                    posInfo.BankCode = (int)BankCode.AnadoluBank;
                    var service = _serviceProvider.GetService(typeof(NestPayVpos)) as IVposApi;
                    service.SetServiceParameters(posInfo);
                    return service;
                }
                case (int)BankCode.Akbank:
                    {
                        var posInfo = GetApiInfo<AkbankPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(AkbankVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.KuveytTurk:
                    {
                        var posInfo = GetApiInfo<KuveytPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(KuveytVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.Halkbank:
                    {
                        var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                        posInfo.BankCode = (int)BankCode.Halkbank;
                        var service = _serviceProvider.GetService(typeof(NestPayVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.Finansbank:
                    {
                        var posInfo = GetApiInfo<FinansPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(FinansVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.SekerBank:
                    {
                        var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                        posInfo.BankCode = (int)BankCode.SekerBank;
                        var service = _serviceProvider.GetService(typeof(NestPayVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.YapiKredi:
                    {
                        var posInfo = GetApiInfo<PosnetPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(PosnetVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.TurkiyeFinansKatilim:
                    {
                        var posInfo = GetApiInfo<NestPayPosInfo>(vpos);
                        posInfo.BankCode = (int)BankCode.TurkiyeFinansKatilim;
                        var service = _serviceProvider.GetService(typeof(NestPayVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                case (int)BankCode.OzanPay:
                    {
                        var posInfo = GetApiInfo<OzanPayPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(OzanPayVpos)) as IVposApi;
                        posInfo.VposId = vpos.Id;
                        service.SetServiceParameters(posInfo, merchantId);
                        return service;
                    }
                case (int)BankCode.PttBank:
                    {
                        var posInfo = GetApiInfo<IvpPosInfo>(vpos);
                        var service = _serviceProvider.GetService(typeof(InterVpos)) as IVposApi;
                        service.SetServiceParameters(posInfo);
                        return service;
                    }
                default: return null;
            }
        }
        return null;
            
    }

    private static T GetApiInfo<T>(Vpos vpos)
        where T : IPosInfo
    {
        var info = (T)Activator.CreateInstance(typeof(T));
        
        if (info == null) 
            return default(T);
        
        var properties = info.GetType().GetProperties();

        foreach (var item in properties)
        {
            var posInfo = vpos.VposBankApiInfos.FirstOrDefault(s => s.Key.Key == item.Name);
            if (posInfo is null) continue;

            item.SetValue(info, posInfo.Value);
        }
        return info;
    }
}
