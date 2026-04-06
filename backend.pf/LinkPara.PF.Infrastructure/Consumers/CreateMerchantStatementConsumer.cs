using System.Globalization;
using LinkPara.HttpProviders.Receipt;
using LinkPara.HttpProviders.Receipt.Models;
using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.MerchantStatement;
using LinkPara.PF.Domain.Entities;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;
using MassTransit;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LinkPara.PF.Infrastructure.Consumers;

public class CreateMerchantStatementConsumer : IConsumer<CreateMerchantStatement>
{
    private readonly ILogger<CreateMerchantStatementConsumer> _logger;
    private readonly IMerchantStatementService _merchantStatementService;
    private readonly IGenericRepository<Merchant> _merchantRepository;
    private readonly IGenericRepository<MerchantStatement> _merchantStatementRepository;
    private readonly IReceiptService _receiptService;
    private readonly IBus _bus;

    public CreateMerchantStatementConsumer(
        ILogger<CreateMerchantStatementConsumer> logger,
        IGenericRepository<Merchant> merchantRepository,
        IGenericRepository<MerchantStatement> merchantStatementRepository,
        IMerchantStatementService merchantStatementService,
        IReceiptService receiptService,
        IBus bus)
    {
        _logger = logger;
        _merchantRepository = merchantRepository;
        _merchantStatementService = merchantStatementService;
        _merchantStatementRepository = merchantStatementRepository;
        _receiptService = receiptService;
        _bus = bus;
    }

    public async Task Consume(ConsumeContext<CreateMerchantStatement> context)
    {
        var merchantStatement = await _merchantStatementRepository.GetAll()
            .FirstOrDefaultAsync(s => s.Id == context.Message.MerchantStatementId);

        try
        {
            var merchant = await _merchantRepository
                .GetAll()
                .Where(b => b.Id == merchantStatement.MerchantId)
                .Include(b => b.MerchantBankAccounts)
                .Include(b => b.MerchantWallets)
                .Include(b => b.Customer)
                .ThenInclude(b => b.AuthorizedPerson)
                .FirstOrDefaultAsync();
            
            var statementDetails = await _merchantStatementService.GetStatementDetailsAsync(merchant, merchantStatement.StatementStartDate, merchantStatement.StatementEndDate);
            
            var receiptDetails = await _receiptService.CreateReceiptAsync(new CreateReceiptRequest
            {
                ReceiptInfo = new ReceiptDto
                {
                    RefTransactionId = merchantStatement.Id,
                    Module = "PF",
                    ReceiptId = 0,
                    TransactionType = "MerchantStatement",
                    TransactionDirection = "MerchantStatement",
                    PaymentMethod = "MerchantStatement",
                    TransactionDate = merchantStatement.StatementEndDate,
                    Amount = statementDetails.StatementSummary.TotalReceivedNetAmount,
                    CommissionAmount = statementDetails.StatementSummary.TotalCommissionAmount,
                    TaxAmount = statementDetails.StatementSummary.BsmvAmount,
                    TotalAmount = statementDetails.StatementSummary.TotalAmount,
                    CurrencyCode = "TRY",
                    DetailInfo = JsonConvert.SerializeObject(statementDetails.MerchantDetails)
                }
            });

            if (!receiptDetails.Success)
            {
                merchantStatement.StatementStatus = MerchantStatementStatus.Failed;
                merchantStatement.Description = receiptDetails.ErrorMessage;
                await _merchantStatementRepository.UpdateAsync(merchantStatement);
                return;
            }

            merchantStatement.ReceiptNumber = receiptDetails.ReceiptNumber;
            statementDetails.ReceiptNumber = receiptDetails.ReceiptNumber;
            
            if (merchantStatement.StatementType is MerchantStatementType.Excel or MerchantStatementType.Both)
            {
                merchantStatement = await _merchantStatementService.CreateMerchantStatementExcelFileAsync(merchant, merchantStatement, statementDetails);
            }
            
            if (merchantStatement.StatementType is MerchantStatementType.PDF or MerchantStatementType.Both)
            {
                merchantStatement = await _merchantStatementService.CreateMerchantStatementPdfFileAsync(merchant, merchantStatement, statementDetails);
            }
            
            if (string.IsNullOrEmpty(merchantStatement.ExcelPath) && string.IsNullOrEmpty(merchantStatement.PdfPath))
            {
                merchantStatement.StatementStatus = MerchantStatementStatus.Failed;
                merchantStatement.Description = "Empty content returned";
                await _merchantStatementRepository.UpdateAsync(merchantStatement);
                return;
            }

            var merchantStatementEvent = new SharedModels.Notification.NotificationModels.PF.MerchantStatement
            {
                StatementMonth = merchantStatement.StatementStartDate.ToString("MMMM", CultureInfo.CreateSpecificCulture("tr")),
                StatementYear = merchantStatement.StatementStartDate.Year.ToString(),
                StartDate = merchantStatement.StatementStartDate.ToString("dd.MM.yyyy"),
                EndDate = merchantStatement.StatementEndDate.ToString("dd.MM.yyyy"),
                MerchantNumber = merchant.Number,
                MerchantName = merchant.Customer.CommercialTitle,
            };
            
            merchantStatementEvent.AddAttachment(await GetAttachmentsAsync(merchantStatement));
            
            await _bus.Publish(merchantStatementEvent);

            merchantStatement.StatementStatus = MerchantStatementStatus.Completed;
            await _merchantStatementRepository.UpdateAsync(merchantStatement);
        }
        catch (Exception exception)
        {
            _logger.LogError($"CreateMerchantStatementConsumerError: {exception}");
            merchantStatement.StatementStatus = MerchantStatementStatus.Failed;
            await _merchantStatementRepository.UpdateAsync(merchantStatement);
        }
    }
    
    private async Task<Dictionary<string, string>> GetFileAttachmentAsync(string filePath, string fileName)
    {
        byte[] fileBytes;
        await using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        using (var ms = new MemoryStream())
        {
            await fs.CopyToAsync(ms);
            fileBytes = ms.ToArray();
        }

        var provider = new FileExtensionContentTypeProvider();
        if (!provider.TryGetContentType(filePath, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return new Dictionary<string, string>
        {
            { "file_size_bytes", fileBytes.Length.ToString() },
            { "mime_type", contentType },
            { "file_name", fileName },
            { "extension", Path.GetExtension(filePath) },
            { "body", Convert.ToBase64String(fileBytes) }
        };
    }

    private async Task<List<Dictionary<string, string>>> GetAttachmentsAsync(MerchantStatement updatedStatement)
    {
        var attachmentList = new List<Dictionary<string, string>>();
        
        if(!string.IsNullOrEmpty(updatedStatement.ExcelPath))
        {
            attachmentList.Add(await GetFileAttachmentAsync(updatedStatement.ExcelPath, $"{updatedStatement.FileName}.xlsx"));
        }
        
        if(!string.IsNullOrEmpty(updatedStatement.PdfPath))
        {
            attachmentList.Add(await GetFileAttachmentAsync(updatedStatement.PdfPath, $"{updatedStatement.FileName}.pdf"));
        }

        return attachmentList;
    }
}