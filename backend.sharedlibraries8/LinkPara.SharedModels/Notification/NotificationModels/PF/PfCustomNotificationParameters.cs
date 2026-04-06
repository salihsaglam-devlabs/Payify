namespace LinkPara.SharedModels.Notification.NotificationModels.PF;

[LocalizedDisplay("PF Özel Parametreleri", "tr")]
[LocalizedDisplay("Merchant Custom Parameters", "en")]
public class PfCustomNotificationParameters : NotificationBase, INotificationCustom
{
    [LocalizedDisplay("ÜİY Adı", "tr")]
    [LocalizedDisplay("Merchant Name", "en")]
    public string Name { get; set; }
    
    [LocalizedDisplay("ÜİY Numarası", "tr")]
    [LocalizedDisplay("Merchant Number", "en")]
    public string Number { get; set; }
    
    [LocalizedDisplay("ÜİY Tipi", "tr")]
    [LocalizedDisplay("Merchant Type", "en")]
    public string MerchantType  { get; set; }
    
    [LocalizedDisplay("Ana ÜİY Adı", "tr")]
    [LocalizedDisplay("Parent Merchant Name", "en")]
    public string ParentMerchantName { get; set; }
    
    [LocalizedDisplay("Ana ÜİY Numarası", "tr")]
    [LocalizedDisplay("Parent Merchant Number", "en")]
    public string ParentMerchantNumber { get; set; }
    
    [LocalizedDisplay("ÜİY Durumu", "tr")]
    [LocalizedDisplay("Merchant Status", "en")]
    public string MerchantStatus { get; set; }
    
    [LocalizedDisplay("Uygulama Kanalı", "tr")]
    [LocalizedDisplay("Application Channel", "en")]
    public string ApplicationChannel { get; set; }
    
    [LocalizedDisplay("Entegrasyon Modu", "tr")]
    [LocalizedDisplay("Integration Mode", "en")]
    public string IntegrationMode { get; set; }
    
    [LocalizedDisplay("MCC Kodu", "tr")]
    [LocalizedDisplay("MCC Code", "en")]
    public string Mcc_Code { get; set; }
    
    [LocalizedDisplay("MCC Adı", "tr")]
    [LocalizedDisplay("MCC Name", "en")]
    public string Mcc_Name { get; set; }
    
    [LocalizedDisplay("Müşteri Durumu", "tr")]
    [LocalizedDisplay("Customer Status", "en")]
    public string Customer_CustomerStatus { get; set; }
    
    [LocalizedDisplay("Müşteri Şirketi Tipi", "tr")]
    [LocalizedDisplay("Customer Company Type", "en")]
    public string Customer_CompanyType { get; set; }
    
    [LocalizedDisplay("Müşteri Ticari Ünvanı", "tr")]
    [LocalizedDisplay("Customer Commercial Title", "en")]
    public string Customer_CommercialTitle { get; set; }
    
    [LocalizedDisplay("Ticari Sicil Numarası", "tr")]
    [LocalizedDisplay("Trade Registration Number", "en")]
    public string Customer_TradeRegistrationNumber { get; set; }
    
    [LocalizedDisplay("Vergi Dairesi", "tr")]
    [LocalizedDisplay("Tax Administration", "en")]
    public string Customer_TaxAdministration { get; set; }
    
    [LocalizedDisplay("Vergi Numarası", "tr")]
    [LocalizedDisplay("Tax Number", "en")]
    public string Customer_TaxNumber { get; set; }
    
    [LocalizedDisplay("Mersis Numarası", "tr")]
    [LocalizedDisplay("Mersis Number", "en")]
    public string Customer_MersisNumber { get; set; }
    
    [LocalizedDisplay("Müşteri Ülke", "tr")]
    [LocalizedDisplay("Customer Country", "en")]
    public string Customer_CountryName { get; set; }

    [LocalizedDisplay("Müşteri Şehir", "tr")]
    [LocalizedDisplay("Customer City", "en")]
    public string Customer_CityName { get; set; }

    [LocalizedDisplay("Müşteri İlçe", "tr")]
    [LocalizedDisplay("Customer District", "en")]
    public string Customer_DistrictName { get; set; }

    [LocalizedDisplay("Müşteri Posta Kodu", "tr")]
    [LocalizedDisplay("Customer Postal Code", "en")]
    public string Customer_PostalCode { get; set; }

    [LocalizedDisplay("Müşteri Adres", "tr")]
    [LocalizedDisplay("Customer Address", "en")]
    public string Customer_Address { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Kimlik No", "tr")]
    [LocalizedDisplay("Authorized Person Identity Number", "en")]
    public string Customer_AuthorizedPerson_IdentityNumber { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Ad", "tr")]
    [LocalizedDisplay("Authorized Person-Name", "en")]
    public string Customer_AuthorizedPerson_Name { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Soyad", "tr")]
    [LocalizedDisplay("Authorized Person-Surname", "en")]
    public string Customer_AuthorizedPerson_Surname { get; set; }

    [LocalizedDisplay("Yetkili Kişi - E-posta", "tr")]
    [LocalizedDisplay("Authorized Person-Email", "en")]
    public string Customer_AuthorizedPerson_Email { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Kurumsal E-posta", "tr")]
    [LocalizedDisplay("Authorized Person-Company Email", "en")]
    public string Customer_AuthorizedPerson_CompanyEmail { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Kurumsal Telefon", "tr")]
    [LocalizedDisplay("Authorized Person-Company Phone Number", "en")]
    public string Customer_AuthorizedPerson_CompanyPhoneNumber { get; set; }

    [LocalizedDisplay("Yetkili Kişi - Telefon", "tr")]
    [LocalizedDisplay("Authorized Person-Phone", "en")]
    public string Customer_AuthorizedPerson_MobilePhoneNumber { get; set; }

    [LocalizedDisplay("Yetkili Kişi - İkinci Telefon", "tr")]
    [LocalizedDisplay("Authorized Person-Second Phone", "en")]
    public string Customer_AuthorizedPerson_MobilePhoneNumberSecond { get; set; }

    [LocalizedDisplay("Web Sitesi", "tr")]
    [LocalizedDisplay("Website", "en")]
    public string WebSiteUrl { get; set; }

    [LocalizedDisplay("Aylık Ciro", "tr")]
    [LocalizedDisplay("Monthly Turnover", "en")]
    public string MonthlyTurnover { get; set; }

    [LocalizedDisplay("Telefon Kodu", "tr")]
    [LocalizedDisplay("Phone Code", "en")]
    public string PhoneCode { get; set; }

    [LocalizedDisplay("Sözleşme Tarihi", "tr")]
    [LocalizedDisplay("Agreement Date", "en")]
    public string AgreementDate { get; set; }

    [LocalizedDisplay("Ödeme Günü", "tr")]
    [LocalizedDisplay("Payment Due Day", "en")]
    public string PaymentDueDay { get; set; }

    [LocalizedDisplay("Reddetme Nedeni", "tr")]
    [LocalizedDisplay("Reject Reason", "en")]
    public string RejectReason { get; set; }

    [LocalizedDisplay("Parametre Değeri", "tr")]
    [LocalizedDisplay("Parameter Value", "en")]
    public string ParameterValue { get; set; }

    [LocalizedDisplay("Fiyatlandırma Profil Numarası", "tr")]
    [LocalizedDisplay("Pricing Profile Number", "en")]
    public string PricingProfileNumber { get; set; }

    [LocalizedDisplay("ÜİY Entegratör - Adı", "tr")]
    [LocalizedDisplay("Merchant Integrator-Name", "en")]
    public string MerchantIntegrator_Name { get; set; }

    [LocalizedDisplay("ÜİY Entegratör - Komisyon Oranı", "tr")]
    [LocalizedDisplay("Merchant Integrator-Commission Rate", "en")]
    public string MerchantIntegrator_CommissionRate { get; set; }

    [LocalizedDisplay("Teknik Yetkili - T.C. Kimlik No", "tr")]
    [LocalizedDisplay("Technical Contact-Identity Number", "en")]
    public string TechnicalContact_IdentityNumber { get; set; }

    [LocalizedDisplay("Teknik Yetkili - Ad", "tr")]
    [LocalizedDisplay("Technical Contact-Name", "en")]
    public string TechnicalContact_Name { get; set; }

    [LocalizedDisplay("Teknik Yetkili - Soyad", "tr")]
    [LocalizedDisplay("Technical Contact-Surname", "en")]
    public string TechnicalContact_Surname { get; set; }

    [LocalizedDisplay("Teknik Yetkili - E-posta", "tr")]
    [LocalizedDisplay("Technical Contact-Email", "en")]
    public string TechnicalContact_Email { get; set; }

    [LocalizedDisplay("Teknik Yetkili - Kurumsal E-posta", "tr")]
    [LocalizedDisplay("Technical Contact-Company Email", "en")]
    public string TechnicalContact_CompanyEmail { get; set; }

    [LocalizedDisplay("Teknik Yetkili - Kurumsal Telefon", "tr")]
    [LocalizedDisplay("Technical Contact-Company Phone", "en")]
    public string TechnicalContact_CompanyPhoneNumber { get; set; }

    [LocalizedDisplay("Teknik Yetkili - Telefon", "tr")]
    [LocalizedDisplay("Technical Contact-Phone", "en")]
    public string TechnicalContact_MobilePhoneNumber { get; set; }

    [LocalizedDisplay("Teknik Yetkili - İkinci Telefon", "tr")]
    [LocalizedDisplay("Technical Contact-Second Phone", "en")]
    public string TechnicalContact_MobilePhoneNumberSecond { get; set; }

    [LocalizedDisplay("Fesih Kodu", "tr")]
    [LocalizedDisplay("Annulment Code", "en")]
    public string AnnulmentCode { get; set; }

    [LocalizedDisplay("Fesih Açıklaması", "tr")]
    [LocalizedDisplay("Annulment Description", "en")]
    public string AnnulmentDescription { get; set; }

    [LocalizedDisplay("Fesih Tarihi", "tr")]
    [LocalizedDisplay("Annulment Date", "en")]
    public string AnnulmentDate { get; set; }

    [LocalizedDisplay("Fesih Ek Bilgisi", "tr")]
    [LocalizedDisplay("Annulment Additional Info", "en")]
    public string AnnulmentAdditionalInfo { get; set; }

    [LocalizedDisplay("ÜİY Kullanıcıları", "tr")]
    [LocalizedDisplay("Merchant Users", "en")]
    public string MerchantUsers { get; set; }

    [LocalizedDisplay("ÜİY E-postaları", "tr")]
    [LocalizedDisplay("Merchant Emails", "en")]
    public string MerchantEmails { get; set; }

    [LocalizedDisplay("ÜİY Skorları", "tr")]
    [LocalizedDisplay("Merchant Scores", "en")]
    public string MerchantScores { get; set; }

    [LocalizedDisplay("ÜİY Belgeleri", "tr")]
    [LocalizedDisplay("Merchant Documents", "en")]
    public string MerchantDocuments { get; set; }

    [LocalizedDisplay("ÜİY Banka Hesapları", "tr")]
    [LocalizedDisplay("Merchant Bank Accounts", "en")]
    public string MerchantBankAccounts { get; set; }

    [LocalizedDisplay("ÜİY Limitleri", "tr")]
    [LocalizedDisplay("Merchant Limits", "en")]
    public string MerchantLimits { get; set; }

    [LocalizedDisplay("ÜİY Bloke Listesi", "tr")]
    [LocalizedDisplay("Merchant Blockage List", "en")]
    public string MerchantBlockageList { get; set; }

    [LocalizedDisplay("İş Ortağı", "tr")]
    [LocalizedDisplay("Merchant Business Partner", "en")]
    public string MerchantBusinessPartner { get; set; }

    [LocalizedDisplay("Host Vergi No", "tr")]
    [LocalizedDisplay("Hosting Tax No", "en")]
    public string HostingTaxNo { get; set; }

    [LocalizedDisplay("Alt ÜİY'ler", "tr")]
    [LocalizedDisplay("Sub merchants", "en")]
    public string SubMerchants { get; set; }

    [LocalizedDisplay("Ödeme Kanalı", "tr")]
    [LocalizedDisplay("Posting Payment Channel", "en")]
    public string PostingPaymentChannel { get; set; }

    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.All;
}