using System.Globalization;
using System.Text.Json;
using LinkPara.Emoney.Application.Commons.Helpers;
using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Infrastructure.Persistence;
using LinkPara.HttpProviders.CustomerManagement;
using LinkPara.HttpProviders.CustomerManagement.Models.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using LinkPara.SharedModels.BusModels.Commands.BTrans;
using LinkPara.SharedModels.BusModels.Commands.Scheduler;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AccountStatus = LinkPara.SharedModels.BusModels.Commands.BTrans.Enums.AccountStatus;

namespace LinkPara.Emoney.Infrastructure.Consumers.CronJobs;

public class SendBalanceInformationReportConsumer : IConsumer<SendBalanceInformationReport>
{
    private readonly EmoneyDbContext _dbContext;
    private readonly ILogger<SendBalanceInformationReportConsumer> _logger;
    private readonly IBTransService _bTransService;

    public SendBalanceInformationReportConsumer(
        EmoneyDbContext dbContext, 
        ILogger<SendBalanceInformationReportConsumer> logger,
        IBTransService bTransService)
    {
        _dbContext = dbContext;
        _logger = logger;
        _bTransService = bTransService;
    }
    
    public async Task Consume(ConsumeContext<SendBalanceInformationReport> context)
    {
        var yesterday = DateTime.Now.AddDays(-1).Date;
        
        var wallets = await _dbContext.Wallet
            .Where(w => w.OpeningDate.Value.Date == yesterday 
                        || (w.OpeningDate.Value.Date < yesterday && w.LastActivityDate.Date == yesterday)
                        || w.ClosingDate.Value.Date == yesterday
                        || _dbContext.Account.Any(a => a.Id == w.AccountId && a.UpdateDate.Value.Date == yesterday))
            .Include(w => w.Account)
            .ToListAsync();
        
        foreach (var wallet in wallets)
        {
            try
            {
                #region BalanceInformationReport
                var bTransIdentity = _bTransService.GetAccountInformation(wallet.Account);
                var balanceInformationReport = new BalanceInformationReport
                {
                    RecordType = RecordTypeConst.NewRecord,
                    OperationType = BalanceInformationConst.BalanceInformation,
                    IsCorporate = bTransIdentity.IsCorporate,
                    PhoneNumber = bTransIdentity.PhoneNumber,
                    Email = bTransIdentity.Email,
                    WalletNumber = wallet.WalletNumber,
                    CityId = 0,
                    TaxNumber = bTransIdentity.TaxNumber,
                    CommercialTitle = bTransIdentity.CommercialTitle,
                    FirstName = bTransIdentity.FirstName,
                    LastName = bTransIdentity.LastName,
                    IdentityNumber = bTransIdentity.IdentityNumber,
                    CurrencyType = wallet.CurrencyCode,
                    OpeningDate = wallet.OpeningDate ?? wallet.CreateDate,
                    ClosingDate = wallet.ClosingDate,
                    AccountBalance = wallet.CurrentBalanceCash + wallet.CurrentBalanceCredit,
                    BalanceDate = yesterday
                };
                #endregion
                
                #region AccountCustomer
                var customerInformation = await _bTransService.GetCustomerInformationAsync(wallet.Account.CustomerId);
                if (customerInformation.IsSucceed)
                {
                    balanceInformationReport.IsCorporate = customerInformation.IsCorporate;
                    balanceInformationReport.PhoneNumber = customerInformation.PhoneNumber;
                    balanceInformationReport.Email = customerInformation.Email;
                    balanceInformationReport.NationCountryId = customerInformation.NationCountryId;
                    balanceInformationReport.CityId = customerInformation.CityId ?? 0;
                    balanceInformationReport.FullAddress = customerInformation.FullAddress;
                    balanceInformationReport.District = customerInformation.District;
                    balanceInformationReport.PostalCode = customerInformation.PostalCode;
                    balanceInformationReport.City = customerInformation.City;
                    balanceInformationReport.TaxNumber = customerInformation.TaxNumber;
                    balanceInformationReport.CommercialTitle = customerInformation.CommercialTitle;
                    balanceInformationReport.FirstName = customerInformation.FirstName;
                    balanceInformationReport.LastName = customerInformation.LastName;
                    balanceInformationReport.DocumentType = customerInformation.DocumentType;
                    balanceInformationReport.IdentityNumber = customerInformation.IdentityNumber;
                }
                #endregion

                if (wallet.OpeningDate?.Date == yesterday && wallet.OpeningDate?.Date == wallet.ClosingDate?.Date)
                {
                    balanceInformationReport.AccountStatus = AccountStatus.New;
                    await _bTransService.SaveBalanceInformationAsync(balanceInformationReport);
                    balanceInformationReport.AccountStatus = AccountStatus.Close;
                    await _bTransService.SaveBalanceInformationAsync(balanceInformationReport);
                }
                else
                {
                    if (wallet.OpeningDate?.Date == yesterday)
                    {
                        balanceInformationReport.AccountStatus = AccountStatus.New;
                    }
                    else if (wallet.ClosingDate?.Date == yesterday)
                    {
                        balanceInformationReport.AccountStatus = AccountStatus.Close;
                    }
                    else
                    {
                        balanceInformationReport.AccountStatus = AccountStatus.Update;
                    }
                    await _bTransService.SaveBalanceInformationAsync(balanceInformationReport);
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(exception,$"Sending balance of wallet[{wallet.WalletNumber}] to BTrans reporting tool failed  Error: {exception}");
            }
        }

    }
}