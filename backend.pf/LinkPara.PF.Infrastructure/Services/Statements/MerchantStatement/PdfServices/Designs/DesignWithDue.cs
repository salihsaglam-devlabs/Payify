using System.Globalization;
using LinkPara.PF.Application.Commons.Models.MerchantStatement;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace LinkPara.PF.Infrastructure.Services.Statements.MerchantStatement.PdfServices.Designs;

public class DesignWithDue : IDocument
{
    public StatementDetails StatementModel { get; }

    public DesignWithDue(StatementDetails statementDetails)
    {
        StatementModel = statementDetails;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;
    public DocumentSettings GetSettings() => DocumentSettings.Default;

    public void Compose(IDocumentContainer container)
    {
        container
            .Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);
            
                page.Header().Element(ComposeHeader);
                
                page.Content().Element(ComposeContent);

                page.Footer().AlignCenter().Text(x =>
                {
                    x.CurrentPageNumber();
                    x.Span(" / ");
                    x.TotalPages();
                });
            });
    }
    
    void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem(1).AlignLeft().Element(containerElement =>
            {
                var logoPath = StatementModel.TenantDetails.Logo;

                if (!string.IsNullOrEmpty(logoPath) && File.Exists(logoPath))
                {
                    var fileExtension = Path.GetExtension(logoPath).ToLower();

                    if (fileExtension == ".svg")
                        containerElement.Width(100).Height(50).Svg(File.ReadAllText(logoPath));
                    else
                        containerElement.Width(100).Height(50).Image(logoPath)
                            .WithCompressionQuality(ImageCompressionQuality.Best);
                }
                else
                {
                    container.Width(100).Height(50).Placeholder("Logo");
                }
            });
            
            row.RelativeItem(1).AlignTop().Column(centerColumn =>
            {
                centerColumn.Item().AlignCenter().AlignMiddle().Text(text =>
                {
                    text.Span("DEKONT (Ödeme Kuruluşu)").FontSize(16).Bold();
                });
                
                centerColumn.Item().AlignCenter().AlignMiddle().Text(text =>
                {
                    text.Span("Hesap Bildirim Cetveli (Ekstre)").FontSize(8).SemiBold();
                });
            });
            
            row.RelativeItem(0.4f).AlignTop().Column(column =>
            {
                
            });
            
            row.RelativeItem(0.6f).AlignLeft().AlignMiddle().Column(column =>
            {
                column.Item().Text(text =>
                {
                    text.Span("Dekont No: ").SemiBold().FontSize(8);
                    text.Span(StatementModel.ReceiptNumber).FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("Dekont Tarihi: ").SemiBold().FontSize(8);
                    text.Span(StatementModel.StatementPeriod.EndDate).FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("Dönem: ").SemiBold().FontSize(8);
                    text.Span(StatementModel.StatementPeriod.Period).FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("İşlem Tarih Aralığı: ").SemiBold().FontSize(8);
                    text.Span($"{StatementModel.StatementPeriod.StartDate} - {StatementModel.StatementPeriod.EndDate}")
                        .FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("Düzenlenme Tarihi: ").SemiBold().FontSize(8);
                    text.Span(StatementModel.StatementPeriod.Date).FontSize(8);
                });
            });
        });
    }
    
    void ComposeContent(IContainer container)
    {
        container.PaddingVertical(20).Column(column =>
        {
            column.Spacing(5);
            
            column.Item().Row(row =>
            {
                row.RelativeItem(1).AlignLeft().Column(leftColumn =>
                {
                    leftColumn.Item().Text(text =>
                    {
                        text.Span(StatementModel.TenantDetails.CommercialTitle).FontSize(10).Bold();
                    });

                    leftColumn.Item().Text(text =>
                    {
                        text.Span(StatementModel.TenantDetails.Address).FontSize(8);
                    });

                    leftColumn.Item().Text(text =>
                    {
                        text.Span($"VKN: {StatementModel.TenantDetails.TaxNumber}").FontSize(8);
                    });
                });

                row.RelativeItem(1.4f).AlignCenter().AlignMiddle().Column(centerColumn =>
                {
                    centerColumn.Item().Text($"{StatementModel.MerchantDetails.MerchantName}").FontSize(10).Bold();
                    centerColumn.Item().Text($"{StatementModel.MerchantDetails.MerchantAddress}").FontSize(8);
                    centerColumn.Item().Text($"VD ve No: {StatementModel.MerchantDetails.TaxAdministration} / {StatementModel.MerchantDetails.TaxNumber}").FontSize(8);
                });

                row.RelativeItem(0.6f).AlignLeft().AlignMiddle().Column(rightColumn =>
                {
                    rightColumn.Item().Text($"Müşteri No: {StatementModel.MerchantDetails.MerchantNumber}").FontSize(8);
                    rightColumn.Item().Text($"Cüzdan No: " + (!string.IsNullOrEmpty(StatementModel.MerchantDetails.MerchantWalletNumber) ? StatementModel.MerchantDetails.MerchantWalletNumber : "-")).FontSize(8);
                    rightColumn.Item().Text($"Hesap No: {StatementModel.MerchantDetails.MerchantAccountNumber}").FontSize(8);
                });
            });
            
            column.Item().Height(10);
            column.Item().Element(ComposeTable);
            
            if (!string.IsNullOrEmpty(StatementModel.Comments))
                column.Item().PaddingTop(25).Element(ComposeComments);
            
            column.Item().PaddingTop(25).AlignRight().Height(100)
                .Image(StatementModel.TenantDetails.SignatureCircular)
                .WithCompressionQuality(ImageCompressionQuality.Best)
                .FitArea();
            
        });
    }

    void ComposeTable(IContainer container)
    {
        container.Column(column =>
        {
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    for (int i = 0; i < 13; i++)
                        columns.RelativeColumn();
                });
        
                table.Header(header =>
                {
                    string[] headers = {
                        "İşlem Tarihi", 
                        "Sipariş No", 
                        "İşlem Türü", 
                        "Kart No", 
                        "Brüt Tutar", 
                        "Komisyon Tutarı", 
                        "Aidat", 
                        "Kesintiler", 
                        "Net Tutar", 
                        "Komisyon Oranı %", 
                        "Taksit Sayısı", 
                        "Puan Tutarı", 
                        "Ödeme Tarihi"
                    };
        
                    foreach (var title in headers)
                    {
                        header.Cell().Element(CellStyle).AlignMiddle().Padding(3).Text(title).FontSize(7);
                    }
                });
                
                table.ExtendLastCellsToTableBottom();
                
                foreach (var item in StatementModel.Transactions)
                {
                    
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.TransactionDate).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.ConversationId).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.TransactionType).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.CardNumber).FontSize(6);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.TotalAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.CommissionAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.DueAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.ChargebackAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.NetAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.CommissionRate.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.InstallmentCount.ToString(CultureInfo.InvariantCulture)).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.PointAmount.ToString("F2")).FontSize(7);
                    table.Cell().Element(CellStyle).Padding(3).AlignMiddle().Text(item.PaymentDate).FontSize(7);
                }
            });
            
            column.Item().PageBreak();
    
            column.Item().Table(summaryTable =>
            {
                summaryTable.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });
    
                summaryTable.Header(header =>
                {
                    header.Cell().ColumnSpan(2).Element(SummaryHeaderStyle).AlignCenter().Text("İşlem Özeti").FontSize(10);
                });
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Toplam Brüt").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Toplam Komisyon").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalCommissionAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Toplam Aidat").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalDueAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Toplam Kesintiler").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalDeductionAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Toplam Net Tutar").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalNetAmount.ToString("N2")).FontSize(9);
    
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Lehe Alınan Tutar").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalReceivedAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("BSMV").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.BsmvAmount.ToString("N2")).FontSize(9);
                
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryLabelStyle).Text("Lehe Alınan Net Tutar").FontSize(9);
                summaryTable.Cell().BorderColor(Colors.Grey.Lighten2).BorderBottom(0.5f).Element(SummaryValueStyle).Text(StatementModel.StatementSummary.TotalReceivedNetAmount.ToString("N2")).FontSize(9);
            });
        });
    }

    
    static IContainer SummaryHeaderStyle(IContainer container)
    {
        return container
            .DefaultTextStyle(x => x.Bold())
            .Background(Colors.Grey.Lighten3)
            .PaddingVertical(5)
            .PaddingHorizontal(10);
    }
    
    static IContainer SummaryLabelStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .PaddingHorizontal(5)
            .AlignLeft();
    }
    
    static IContainer SummaryValueStyle(IContainer container)
    {
        return container
            .PaddingVertical(3)
            .PaddingHorizontal(5)
            .AlignRight();
    }
    
    static IContainer CellStyle(IContainer container)
    {
        return container
            // .PaddingVertical(3)
            // .PaddingHorizontal(5) 
            // .ScaleToFit()
            .ShowEntire()
            .BorderColor(Colors.Grey.Lighten2)
            .Border(0.5f);
    }

    void ComposeComments(IContainer container)
    {
        container.Background(Colors.Grey.Lighten3).Padding(10).Column(column =>
        {
            column.Spacing(5);
            column.Item().Text(StatementModel.Comments).FontSize(10);
        });
    }
}