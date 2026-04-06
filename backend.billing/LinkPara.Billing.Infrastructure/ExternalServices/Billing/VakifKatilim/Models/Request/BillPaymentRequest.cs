namespace LinkPara.Billing.Infrastructure.ExternalServices.Billing.VakifKatilim.Models.Request;

public class BillPaymentRequest
{
    public int? InstitutionId { get; set; }
    public string SubscriberNumber { get; set; }
    
    public string BillNumber { get; set; }
    public string NameSurname { get; set; }
    public DateTime? LastPaymentDate { get; set; }
    public decimal? Amount { get; set; }
    
    public string SecondQueryField { get; set; }
    public DateTime? InvoiceDate { get; set; }
    public decimal? InvoiceDelayAmount { get; set; }
    
    public string DebtProcessType { get; set; } // Bu alan kullanılmamaktadır
    public string DebtInfo { get; set; } // Bu alan kullanılmamaktadır
    public string AdditionalInfo { get; set; } // Bu alan kullanılmamaktadır

    #region Optional Fields
    public decimal? DebtAmount { get; set; }
    public string DebtExtension { get; set; }
    public string DebtId { get; set; }
    public string Description { get; set; }
    public string Period { get; set; }
    public string Priority { get; set; }
    public string ReferenceNumber { get; set; }
    public int? Year { get; set; }

    #endregion
}