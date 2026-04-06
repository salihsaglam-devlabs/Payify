using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Application.Features.Wallets;

public class WalletPartnerDto
{
    public string WalletId { get; set; } 
    public string Msisdn { get; set; } 
    public CustomerPartnerInfo Customer { get; set; } 
    public DateTime CreatedDate { get; set; } 
    public DateTime? LastStatusDate { get; set; } 
    public string AccountType { get; set; } 
    public bool IsBlocked { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public BalancePartnerInfo Balances { get; set; }
    public Guid AccountId { get; set; }
    public AccountType SegmentType { get; set; }
    public WalletType WalletType { get; set; }
}

public class CustomerPartnerInfo
{
    public CustomerPartnerInfo(string name, string surname, string citizenshipNumber, string jobName, DateTime birthDate)
    {
        Name = name;
        Surname = surname;
        CitizenshipNumber = citizenshipNumber;
        JobName = jobName;
        BirthDate = birthDate;
    }

    public string Name { get; set; } 
    public string Surname { get; set; } 
    public string CitizenshipNumber { get; set; }
    public string JobName { get; set; }
    public DateTime BirthDate { get; set; }
}

public class BalancePartnerInfo
{
    public BalancePartnerInfo(string currencyCodeDesc, decimal currentBalanceCredit, decimal currentBalanceCash, decimal blockedBalance)
    {
        CurrencyCodeDesc = currencyCodeDesc;
        CurrentBalanceCredit = currentBalanceCredit;
        CurrentBalanceCash = currentBalanceCash;
        BlockedBalance = blockedBalance;
    }
    public string CurrencyCodeDesc { get; set; } 
    public decimal CurrentBalanceCredit { get; set; }
    public decimal CurrentBalanceCash { get; set; } 
    public decimal BlockedBalance { get; set; } 
}
