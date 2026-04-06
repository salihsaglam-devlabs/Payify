using AutoMapper;
using ClosedXML.Excel;
using ClosedXML.Excel.Drawings;
using ClosedXML.Graphics;
using LinkPara.HttpProviders.BusinessParameter;
using LinkPara.MappingExtensions.Mapping;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.MerchantStatements;
using LinkPara.PF.Application.Features.Statements.Queries.GetMerchantStatementDetail;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.PF.Infrastructure.Persistence;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Reflection;
using System.Text;
using LinkPara.PF.Application.Commons.Models.MerchantStatement;
using LinkPara.PF.Infrastructure.Services.Statements.MerchantStatement.PdfServices;
using LinkPara.PF.Infrastructure.Services.Statements.MerchantStatement.PdfServices.Designs;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace LinkPara.PF.Infrastructure.Services.Statements;

public class MerchantStatementService : IMerchantStatementService
{
    private readonly ILogger<MerchantStatementService> _logger;
    private readonly PfDbContext _context;
    private readonly IParameterService _parameterService;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository<Domain.Entities.MerchantStatement> _merchantStatementRepository;
    private readonly IMapper _mapper;

    private const int RowNumberForGrid = 16;
    private const int One = 1;
    private const int Two = 2;
    private const int Hundred = 100;
    private const string MerchantNumberCell = "A17";
    private const string AccountTypeCell = "B17";
    private const string TransactionDateCell = "C17";
    private const string OrderIdCell = "D17";
    private const string TransactionTypeCell = "E17";
    private const string CardNumberCell = "F17";
    private const string TotalAmountCell = "G17";
    private const string CommissionAmountCell = "H17";
    private const string DueAmountCell = "I17";
    private const string ChargebackAmountCell = "J17";
    private const string NetAmountCell = "K17";
    private const string CommissionRateCell = "L17";
    private const string InstallmentCountCell = "M17";
    private const string PointAmountCell = "N17";
    private const string PaymentDateCell = "O17";
    private const string AccountTypeValue = "Sanal Pos";
    private const string ReturnString = "İade";
    private const string ReverseString = "İptal";
    private const string AuthString = "Satış";
    private const string PreAuthString = "Ön Provizyon";
    private const string PostAuthString = "Ön Provizyon Kapama";
    private const string DueString = "Aidat";
    private const string ChargebackString = "Chargeback";
    private const string RejectedChargebackString = "Chargeback İptal";
    private const string SuspiciousString = "Şüpheli";
    private const string RejectedSuspiciousString = "Şüpheli İptal";
    private const string ExcessReturnString = "Devreden İade";
    private const string DueApiString = "Api";
    private const string DueHppString = "Ortak Ödeme Sayfası";
    private const string DueManuelPaymentPageString = "Manuel Ödeme Sayfası";
    private const string DueLinkPaymentPageString = "Linkle Ödeme";

    public MerchantStatementService(PfDbContext context,
        IConfiguration configuration,
        IParameterService parameterService,
        ILogger<MerchantStatementService> logger,
        IGenericRepository<Domain.Entities.MerchantStatement> merchantStatementRepository,
        IMapper mapper)
    {
        _context = context;
        _parameterService = parameterService;
        _configuration = configuration;
        _logger = logger;
        _merchantStatementRepository = merchantStatementRepository;
        _mapper = mapper;
    }

    public async Task<Domain.Entities.MerchantStatement> CreateMerchantStatementExcelFileAsync(Merchant merchant, Domain.Entities.MerchantStatement merchantStatement, StatementDetails statementDetails)
    {
        try
        {
            if (!statementDetails.Transactions.Any())
            {
                return merchantStatement;
            }

            byte[] workbookBytes;
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location).ToString();
            var filename = _configuration.GetValue<string>("MerchantStatement:TemplateName");
            var path = string.Concat(directory, $"/Services/Statements/MerchantStatement/{filename}");

            var loadOptions = new LoadOptions
            {
                GraphicEngine = NoGraphicsEngine.Instance
            };

            var wbook = new XLWorkbook(path, loadOptions);
            var workSheet = wbook.Worksheet(One);
            var sheetRange = workSheet.RangeUsed();
            
            foreach (var cell in sheetRange.CellsUsed())
            {
                if (!cell.Value.ToString().Contains("{TenantHeader}")) 
                    continue;
                var richText = cell.CreateRichText();
                richText
                    .AddText(statementDetails.TenantDetails.CommercialTitle).SetFontSize(17)
                    .AddNewLine()
                    .AddText(statementDetails.TenantDetails.Address).SetFontSize(13)
                    .AddNewLine()
                    .AddText($"{statementDetails.TenantDetails.TaxAdministration} / {statementDetails.TenantDetails.TaxNumber}").SetFontSize(13);
                
                richText.ElementAt(0).Text = string.Empty;
                
                break;
            }
            
            sheetRange.Search("{MerchantNumber}").Value = statementDetails.MerchantDetails.MerchantNumber;
            sheetRange.Search("{MerchantAccountNumber}").Value = statementDetails.MerchantDetails.MerchantAccountNumber;
            sheetRange.Search("{MerchantWalletNumber}").Value = statementDetails.MerchantDetails.MerchantWalletNumber;
            sheetRange.Search("{StatementDate}").Value = statementDetails.StatementPeriod.Date;
            sheetRange.Search("{StatementPeriod}").Value = statementDetails.StatementPeriod.Period;
            sheetRange.Search("{StatementStartDate}").Value = statementDetails.StatementPeriod.StartDate;
            sheetRange.Search("{StatementEndDate}").Value = statementDetails.StatementPeriod.EndDate;
            sheetRange.Search("{MerchantName}").Value = statementDetails.MerchantDetails.MerchantName;
            sheetRange.Search("{MerchantAddress}").Value = statementDetails.MerchantDetails.MerchantAddress;
            sheetRange.Search("{MerchantTaxNo}").Value = string.Concat(statementDetails.MerchantDetails.TaxAdministration, " / ", statementDetails.MerchantDetails.TaxNumber);
            sheetRange.Search("{TotalDeductionAmount}").Value = statementDetails.StatementSummary.TotalDeductionAmount;
            sheetRange.Search("{TotalDueAmount}").Value = statementDetails.StatementSummary.TotalDueAmount;
            sheetRange.Search("{TotalAmount}").Value = statementDetails.StatementSummary.TotalAmount;
            sheetRange.Search("{TotalCommissionAmount}").Value = statementDetails.StatementSummary.TotalCommissionAmount;
            sheetRange.Search("{TotalNetAmount}").Value = statementDetails.StatementSummary.TotalNetAmount;
            sheetRange.Search("{TotalReceivedAmount}").Value = statementDetails.StatementSummary.TotalReceivedAmount;
            sheetRange.Search("{BsmvAmount}").Value = statementDetails.StatementSummary.BsmvAmount;
            sheetRange.Search("{TotalReceivedNetAmount}").Value = statementDetails.StatementSummary.TotalReceivedNetAmount;

            foreach (var transaction in statementDetails.Transactions)
            {
                workSheet.Row(RowNumberForGrid).InsertRowsBelow(One);
                workSheet.Cell(MerchantNumberCell).Value = transaction.MerchantNumber;
                workSheet.Cell(AccountTypeCell).Value = transaction.AccountType;
                workSheet.Cell(TransactionDateCell).Value = transaction.TransactionDate;
                workSheet.Cell(OrderIdCell).Value = transaction.ConversationId;
                workSheet.Cell(TransactionTypeCell).Value = transaction.TransactionType;
                workSheet.Cell(CardNumberCell).Value = transaction.CardNumber;
                workSheet.Cell(TotalAmountCell).Value = transaction.TotalAmount;
                workSheet.Cell(CommissionAmountCell).Value = transaction.CommissionAmount;
                workSheet.Cell(DueAmountCell).Value = transaction.DueAmount;
                workSheet.Cell(ChargebackAmountCell).Value = transaction.ChargebackAmount;
                workSheet.Cell(NetAmountCell).Value = transaction.NetAmount;
                workSheet.Cell(CommissionRateCell).Value = transaction.CommissionRate;
                workSheet.Cell(InstallmentCountCell).Value = transaction.InstallmentCount;
                workSheet.Cell(PointAmountCell).Value = transaction.PointAmount;
                workSheet.Cell(PaymentDateCell).Value = transaction.PaymentDate;
            }

            workSheet.Row(RowNumberForGrid).Delete();
            using (var ms = new MemoryStream())
            {
                wbook.SaveAs(ms);
                workbookBytes = ms.ToArray();
            }

            if (workbookBytes.Any())
            {
                var period = merchantStatement.StatementStartDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("tr"));
                var saveDocumentPath = Path.Combine(
                    _configuration.GetValue<string>("MerchantStatement:StatementFilePath"),
                    merchantStatement.StatementStartDate.Year.ToString(), merchantStatement.StatementStartDate.ToString(format: "MM"));
                var saveDocumentName = string.Concat(merchant.Number, "_", period,
                    merchantStatement.StatementStartDate.Year.ToString());
            
                if (!Directory.Exists(saveDocumentPath))
                {
                    Directory.CreateDirectory(saveDocumentPath);
                }

                var fullPath = Path.Combine(saveDocumentPath, $"{saveDocumentName}.xlsx");
                await File.WriteAllBytesAsync(fullPath, workbookBytes.ToArray());

                merchantStatement.ExcelPath = fullPath;
                merchantStatement.FileName = saveDocumentName;
            }
            return merchantStatement;
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateMerchantStatementExcelFile: {exception}");
            return merchantStatement;
        }
    }
    
    public async Task<Domain.Entities.MerchantStatement> CreateMerchantStatementPdfFileAsync(Merchant merchant, Domain.Entities.MerchantStatement merchantStatement, StatementDetails statementDetails)
    {
        IDocument document = statementDetails.StatementDesign switch
        {
            "1" => new DesignWithDue(statementDetails),
            "2" => new DesignWithoutDue(statementDetails),
            _ => new DesignWithDue(statementDetails)
        };
        
        var period = merchantStatement.StatementStartDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("tr"));
        var saveDocumentPath = Path.Combine(
            _configuration.GetValue<string>("MerchantStatement:StatementFilePath"),
            merchantStatement.StatementStartDate.Year.ToString(), merchantStatement.StatementStartDate.ToString(format: "MM"));
        var saveDocumentName = string.Concat(merchant.Number, "_", period,
            merchantStatement.StatementStartDate.Year.ToString());
            
        if (!Directory.Exists(saveDocumentPath))
        {
            Directory.CreateDirectory(saveDocumentPath);
        }
        var fullPath = Path.Combine(saveDocumentPath, $"{saveDocumentName}.pdf");
        
        document.GeneratePdf(fullPath);
        
        merchantStatement.PdfPath = fullPath;
        merchantStatement.FileName = saveDocumentName;
        return merchantStatement;
    }

    internal class NoGraphicsEngine : IXLGraphicEngine
    {
        private NoGraphicsEngine()
        {
        }

        public static NoGraphicsEngine Instance { get; } = new();
        public double GetDescent(IXLFontBase font, double dpiY) => default;
        public GlyphBox GetGlyphBox(ReadOnlySpan<int> graphemeCluster, IXLFontBase font, Dpi dpi) => default;
        public double GetMaxDigitWidth(IXLFontBase font, double dpiX) => default;
        public XLPictureInfo GetPictureInfo(Stream imageStream, XLPictureFormat expectedFormat) => default;
        public double GetTextHeight(IXLFontBase font, double dpiY) => default;
        public double GetTextWidth(string text, IXLFontBase font, double dpiX) => default;
    }

    public async Task<PaginatedList<MerchantStatementDto>> GetPaginatedMerchantStatementsAsync(
        GetMerchantStatementDetailQuery request)
    {
        var merchantStatements = _merchantStatementRepository.GetAll();

        if (request.MerchantId is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.MerchantId == request.MerchantId);
        }

        if (request.MailAddress is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.MailAddress.Contains(request.MailAddress));
        }

        if (request.ReceiptNumber is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.ReceiptNumber.Contains(request.ReceiptNumber));
        }

        if (request.StatementStatus is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementStatus == request.StatementStatus);
        }

        if (request.StatementType is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementType == request.StatementType);
        }

        if (request.StatementStartDate is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementEndDate
                                                               >= request.StatementStartDate);
        }

        if (request.StatementEndDate is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementEndDate
                                                               <= request.StatementEndDate);
        }

        if (request.StatementMonth is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementMonth
                                                               == request.StatementMonth);
        }

        if (request.StatementYear is not null)
        {
            merchantStatements = merchantStatements.Where(b => b.StatementYear
                                                               == request.StatementYear);
        }

        return await merchantStatements.OrderBy(b => b.CreateDate)
            .PaginatedListWithMappingAsync<Domain.Entities.MerchantStatement, MerchantStatementDto>(_mapper, request.Page, request.Size,
                request.OrderBy, request.SortBy);
    }

    public async Task<StatementDetails> GetStatementDetailsAsync(Merchant merchant, DateTime startDate, DateTime endDate)
    {
        var tenantCompanyInfo =
            await _parameterService.GetParametersAsync("CompanyContactInformation");
        
        var logoPath = await DownloadImageAndReturnPathAsync(
            tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "LogoPngUrl")?.ParameterValue, 
            "tenant-logo");

        var signatureCircularPath = await DownloadImageAndReturnPathAsync(
            tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "SignatureCircularPngUrl")?.ParameterValue,
            "tenant-signature-circular");
        
        var statementDesign = await _parameterService.GetParameterAsync("PFParameters", "MerchantStatementDesign");
        
        var statementDetails = new StatementDetails
        {
            TenantDetails = new StatementTenantDetails
            {
                CommercialTitle = tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "CompanyCommercialTitle")?.ParameterValue,
                Address = tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "Address")?.ParameterValue,
                TaxAdministration = tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "TaxAdministration")?.ParameterValue,
                TaxNumber = tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "TaxNumber")?.ParameterValue,
                Email = tenantCompanyInfo.FirstOrDefault(w => w.ParameterCode == "CompanyEmail")?.ParameterValue,
                Logo = logoPath,
                SignatureCircular = signatureCircularPath
            },
            MerchantDetails = new StatementMerchantDetails
            {
                MerchantNumber = merchant.Number,
                MerchantAccountNumber = merchant.MerchantBankAccounts?.FirstOrDefault(m => m.RecordStatus == RecordStatus.Active)?.Iban,
                MerchantWalletNumber = merchant.MerchantWallets?.FirstOrDefault(m => m.RecordStatus == RecordStatus.Active)?.WalletNumber,
                MerchantName = merchant.Customer.CommercialTitle,
                MerchantAddress = string.Concat(merchant.Customer.DistrictName, " / ", merchant.Customer.CityName),
                TaxAdministration = merchant.Customer.TaxAdministration,
                TaxNumber = merchant.Customer.TaxNumber
            },
            StatementPeriod = new StatementPeriod
            {
                Date = DateTime.Now.ToString("dd.MM.yyyy"),
                Period = string.Concat(startDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("tr")), " ", startDate.Year.ToString()),
                StartDate = startDate.ToString("dd.MM.yyyy"),
                EndDate = endDate.ToString("dd.MM.yyyy")
            },
            Transactions = new List<StatementTransaction>(),
            Comments = "Bu dekont, ekstre işlemi sırasında gerçekleşen komisyon kesintilerinin ve diğer kesintilerin aylık toplam tutarını gösteren belge olarak düzenlenmiştir. Hesabınızdan ayrıca başka bir kesinti yapılmamaktadır. 11 Mart 2017 tarihli ve 30004 sayılı Resmî Gazete'de yayımlanan \"91 Seri No.lu Gider Vergileri Genel Tebliği\" kapsamında Banka ve Sigorta Muameleleri Vergisi (BSMV) mükellefi olmamız nedeniyle bu dekont, fatura yerine düzenlenmiştir. Belgeyi gider kaydı olarak muhasebeleştirebilirsiniz.",
            StatementDesign = statementDesign is not null ? statementDesign.ParameterValue : "1"
        };
        
        var postingTransactions = await (from postingTransaction in _context.PostingTransaction
            join merchantTransaction in _context.MerchantTransaction on postingTransaction.MerchantTransactionId
                equals merchantTransaction.Id
            where postingTransaction.MerchantId == merchant.Id
                  && postingTransaction.TransactionDate >= startDate
                  && postingTransaction.TransactionDate <= endDate
                  && postingTransaction.BatchStatus == BatchStatus.Completed
            orderby postingTransaction.TransactionDate descending
            select new
            {
                OrderId = postingTransaction.OrderId,
                PfCommissionAmount =
                    postingTransaction.TransactionType == TransactionType.Return || postingTransaction.TransactionType == TransactionType.Reverse
                        ? -1 * postingTransaction.PfCommissionAmount
                        : postingTransaction.PfCommissionAmount,
                TransactionDate = postingTransaction.TransactionDate,
                TransactionType = postingTransaction.TransactionType,
                CardNumber = postingTransaction.CardNumber,
                Amount = postingTransaction.TransactionType == TransactionType.Return || postingTransaction.TransactionType == TransactionType.Reverse
                    ? -1 * postingTransaction.Amount
                    : postingTransaction.Amount,
                AmountWithoutCommissions =
                    postingTransaction.TransactionType == TransactionType.Return || postingTransaction.TransactionType == TransactionType.Reverse
                        ? -1 * postingTransaction.AmountWithoutCommissions
                        : postingTransaction.AmountWithoutCommissions,
                PfCommissionRate = postingTransaction.PfCommissionRate,
                InstallmentCount = postingTransaction.InstallmentCount,
                PointAmount =
                    postingTransaction.TransactionType == TransactionType.Return || postingTransaction.TransactionType == TransactionType.Reverse
                        ? -1 * postingTransaction.PointAmount
                        : postingTransaction.PointAmount,
                PaymentDate = postingTransaction.PaymentDate,
                MerchantOrderId = merchantTransaction.ConversationId,
                MerchantTransactionId = merchantTransaction.Id
            }).ToListAsync();

        var deductions = await (from deductionTransaction in _context.DeductionTransaction
            join deduction in _context.MerchantDeduction on deductionTransaction.MerchantDeductionId equals
                deduction.Id
            where deductionTransaction.MerchantId == merchant.Id
                  && deductionTransaction.CreateDate >= startDate
                  && deductionTransaction.CreateDate.Date <= endDate.Date
            orderby deduction.CreateDate descending
            select new
            {
                DeductionId = deduction.Id,
                MerchantTransactionId = deduction.MerchantTransactionId,
                MerchantDueId = deduction.MerchantDueId,
                PostingBalanceId = deduction.PostingBalanceId,
                DeductionType = deductionTransaction.DeductionType,
                CreateDate = deduction.CreateDate,
                TransactionCreateDate = deductionTransaction.CreateDate,
                Amount = deductionTransaction.DeductionType == DeductionType.RejectedChargeback
                    || deductionTransaction.DeductionType == DeductionType.RejectedSuspicious
                    ? deductionTransaction.Amount
                    : -1 * deductionTransaction.Amount
            }).ToListAsync();
        
        var totalAmount = 0m;
        var totalDeductionAmount = 0m;
        var totalCommissionAmount = 0m;
        
        var unlistedDeductions = deductions.Where(s =>
            s.DeductionType is not DeductionType.Due && s.DeductionType is not DeductionType.ExcessReturn && !postingTransactions
                .Select(a => a.MerchantTransactionId).ToList().Contains(s.MerchantTransactionId)).ToList();

        var oldMerchantTransactions = await _context.MerchantTransaction.Where(s =>
            unlistedDeductions.Select(a => a.MerchantTransactionId).ToList().Contains(s.Id)).ToListAsync();

        foreach (var deduction in unlistedDeductions
                                                                .DistinctBy(s => s.MerchantTransactionId))
        {
            var merchantTransaction =
                oldMerchantTransactions.FirstOrDefault(s => s.Id == deduction.MerchantTransactionId);
            var deductionAmount = Math.Round(
                unlistedDeductions
                    .Where(s => s.MerchantTransactionId == deduction.MerchantTransactionId)
                    .Sum(s => s.Amount), Two);
            
            if (deductionAmount == 0)
            {
                continue;
            }
            
            var transactionType = merchantTransaction?.TransactionType switch
            {
                TransactionType.Return => ReturnString,
                TransactionType.Reverse => ReverseString,
                TransactionType.PostAuth => PostAuthString,
                TransactionType.PreAuth => PreAuthString,
                _ => AuthString
            };
            
            var deductionType = deduction.DeductionType switch
            {
                DeductionType.Chargeback => ChargebackString,
                DeductionType.Suspicious => SuspiciousString,
                DeductionType.RejectedChargeback => RejectedChargebackString,
                _ => RejectedSuspiciousString
            };
            
            statementDetails.Transactions.Add(new StatementTransaction
            {
                MerchantNumber = merchant.Number,
                AccountType = AccountTypeValue,
                TransactionDate = merchantTransaction.TransactionDate.ToString("dd.MM.yyyy"),
                ConversationId = merchantTransaction.ConversationId,
                TransactionType = $"{transactionType} ({deductionType})",
                CardNumber = string.Empty,
                TotalAmount = Math.Round(0m, Two),
                CommissionAmount = Math.Round(0m, Two),
                DueAmount = Math.Round(0m, Two),
                ChargebackAmount = deductionAmount,
                NetAmount = Math.Round(0m, Two),
                CommissionRate = Math.Round(0m, Two),
                InstallmentCount = 0,
                PointAmount = Math.Round(0m, Two),
                PaymentDate = deduction.CreateDate.ToString("dd.MM.yyyy")
            });

            totalDeductionAmount += deductionAmount;
        }
        
        var totalDueAmount = 0m;
        var merchantDueDeductions = deductions
            .Where(s => s.DeductionType == DeductionType.Due).ToList();
        var merchantDues = await _context.MerchantDue.Include(s => s.DueProfile).Where(s => s.MerchantId == merchant.Id).ToListAsync();

        foreach (var merchantDueDeduction in merchantDueDeductions.DistinctBy(s => s.DeductionId))
        {
            var merchantDue = merchantDues.FirstOrDefault(s => s.Id == merchantDueDeduction.MerchantDueId);
            var dueDeductionAmount = Math.Round(
                merchantDueDeductions
                    .Where(s => s.DeductionId == merchantDueDeduction.DeductionId)
                    .Sum(s => s.Amount), Two);
            var lastTransactionDate = merchantDueDeductions
                .Where(s => s.DeductionId == merchantDueDeduction.DeductionId)
                .OrderByDescending(s => s.TransactionCreateDate)
                .Select(s => s.TransactionCreateDate)
                .FirstOrDefault();
            
            var dueType = merchantDue.DueProfile.DueType switch
            {
                DueType.Api => DueApiString,
                DueType.Hpp => DueHppString,
                DueType.ManuelPaymentPage => DueManuelPaymentPageString,
                DueType.LinkPaymentPage => DueLinkPaymentPageString,
                _ => string.Empty
            };
            
            statementDetails.Transactions.Add(new StatementTransaction
            {
                MerchantNumber = merchant.Number,
                AccountType = AccountTypeValue,
                TransactionDate = merchantDueDeduction.CreateDate.ToString("dd.MM.yyyy"),
                ConversationId = string.Empty,
                TransactionType = $"{dueType} {merchantDueDeduction.CreateDate.ToString("MMMM", new CultureInfo("tr-TR"))} {DueString}",
                CardNumber = string.Empty,
                TotalAmount = Math.Round(0m, Two),
                CommissionAmount = Math.Round(0m, Two),
                DueAmount = dueDeductionAmount,
                ChargebackAmount = Math.Round(0m, Two),
                NetAmount = Math.Round(0m, Two),
                CommissionRate = Math.Round(0m, Two),
                InstallmentCount = 0,
                PointAmount = Math.Round(0m, Two),
                PaymentDate = lastTransactionDate.ToString("dd.MM.yyyy")
            });
            
            totalDueAmount += dueDeductionAmount;
        }

        var totalExcessReturnAmount = 0m;
        var excessReturnDeductions = deductions
            .Where(s => 
                    s.DeductionType == DeductionType.ExcessReturn &&
                    (s.CreateDate < startDate || s.CreateDate > endDate)
                ).ToList();
        
        foreach (var excessReturnDeduction in excessReturnDeductions.DistinctBy(s => s.DeductionId))
        {
            var excessReturnDeductionAmount = Math.Round(
                excessReturnDeductions
                    .Where(s => s.DeductionId == excessReturnDeduction.DeductionId)
                    .Sum(s => s.Amount), Two);
            
            statementDetails.Transactions.Add(new StatementTransaction
            {
                MerchantNumber = merchant.Number,
                AccountType = AccountTypeValue,
                TransactionDate = excessReturnDeduction.CreateDate.ToString("dd.MM.yyyy"),
                ConversationId = string.Empty,
                TransactionType = ExcessReturnString,
                CardNumber = string.Empty,
                TotalAmount = Math.Round(0m, Two),
                CommissionAmount = Math.Round(0m, Two),
                DueAmount = Math.Round(0m, Two),
                ChargebackAmount = excessReturnDeductionAmount,
                NetAmount = Math.Round(0m, Two),
                CommissionRate = Math.Round(0m, Two),
                InstallmentCount = 0,
                PointAmount = Math.Round(0m, Two),
                PaymentDate = excessReturnDeduction.CreateDate.ToString("dd.MM.yyyy")
            });
            
            totalExcessReturnAmount += excessReturnDeductionAmount;
        }

        foreach (var statement in postingTransactions)
        {
            var transactionType = statement.TransactionType switch
            {
                TransactionType.Return => ReturnString,
                TransactionType.Reverse => ReverseString,
                TransactionType.PostAuth => PostAuthString,
                TransactionType.PreAuth => PreAuthString,
                _ => AuthString
            };

            var statementDeductionAmount = Math.Round(
                deductions.Any(s => s.MerchantTransactionId == statement.MerchantTransactionId)
                    ? deductions.Where(s => s.MerchantTransactionId == statement.MerchantTransactionId)
                        .Sum(s => s.Amount)
                    : 0, Two);
            var statementAmount = Math.Round(statement.Amount, Two);
            var statementCommissionAmount = Math.Round(statement.PfCommissionAmount, Two);
            var statementNetAmount = Math.Round(statement.AmountWithoutCommissions, Two);
            var statementCommissionRate = Math.Round(statement.PfCommissionRate, Two);
            var statementPointAmount = Math.Round(statement.PointAmount, Two);
            
            statementDetails.Transactions.Add(new StatementTransaction
            {
                MerchantNumber = merchant.Number,
                AccountType = AccountTypeValue,
                TransactionDate = statement.TransactionDate.ToString("dd.MM.yyyy"),
                ConversationId = statement.MerchantOrderId,
                TransactionType = transactionType,
                CardNumber = statement.CardNumber,
                TotalAmount = statementAmount,
                CommissionAmount = statementCommissionAmount,
                DueAmount = 0,
                ChargebackAmount = statementDeductionAmount,
                NetAmount = statementNetAmount,
                CommissionRate = statementCommissionRate,
                InstallmentCount = statement.InstallmentCount,
                PointAmount = statementPointAmount,
                PaymentDate = statement.PaymentDate.ToString("dd.MM.yyyy")
            });

            totalAmount += statementAmount;
            totalDeductionAmount += statementDeductionAmount;
            totalCommissionAmount += statementCommissionAmount;
        }

        totalDueAmount = totalDueAmount < 0 ? -1 * totalDueAmount : totalDueAmount;
        totalDeductionAmount = totalDeductionAmount < 0 ? -1 * totalDeductionAmount : totalDeductionAmount;
        totalExcessReturnAmount = totalExcessReturnAmount < 0 ? -1 * totalExcessReturnAmount : totalExcessReturnAmount;
        totalDeductionAmount += totalExcessReturnAmount;

        var totalReceivedAmount = totalCommissionAmount + totalDueAmount;
        
        var bsmvParameter = await _parameterService.GetParameterAsync("Comission", "BsmvRate");
        
        var bsmvAmount =
            Math.Round(
                totalReceivedAmount - (totalReceivedAmount /
                                       (One + Convert.ToDecimal(bsmvParameter.ParameterValue) / Hundred)), Two);

        statementDetails.StatementSummary = new StatementSummary
        {
            TotalAmount = Math.Round(totalAmount, Two),
            TotalCommissionAmount = Math.Round(totalCommissionAmount, Two),
            TotalDueAmount = Math.Round(totalDueAmount, Two),
            TotalDeductionAmount = Math.Round(totalDeductionAmount, Two),
            TotalNetAmount = Math.Round((totalAmount - totalReceivedAmount - totalDeductionAmount), Two),
            TotalReceivedAmount = Math.Round(totalReceivedAmount, Two),
            BsmvAmount = Math.Round(bsmvAmount, Two),
            TotalReceivedNetAmount = Math.Round((totalReceivedAmount - bsmvAmount), Two)
        };

        return statementDetails;
    }

    private async Task<string> DownloadImageAndReturnPathAsync(string url, string fileName)
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(30);
            
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            
            var imageBytes = await response.Content.ReadAsByteArrayAsync();
            var extension = Path.GetExtension(new Uri(url).AbsolutePath).ToLower();
            
            var location = Assembly.GetExecutingAssembly().Location;
            var directory = Path.GetDirectoryName(location).ToString();
            var filePath = string.Concat(directory, $"/Services/Statements/MerchantStatement/{fileName}{extension}");
            
            var text = Encoding.UTF8.GetString(imageBytes);
            if (text.TrimStart().StartsWith("<svg", StringComparison.OrdinalIgnoreCase))
            {
                var svgContent = Encoding.UTF8.GetString(imageBytes);
                await File.WriteAllTextAsync(filePath, svgContent, Encoding.UTF8);
            }
            else
            {
                await File.WriteAllBytesAsync($"{filePath}", imageBytes);
            }
            return filePath;
        }
        catch (Exception exception)
        {
            _logger.LogError($"Error occured while fetching {fileName}. Exception:{exception}");
            return string.Empty;
        }
        
    }
}