namespace LinkPara.SharedModels.Notification.NotificationModels.Identity;

[LocalizedDisplay("Kullanıcı Özel Parametreleri", "tr")]
[LocalizedDisplay("User Custom Parameters", "en")]
public class IdentityCustomNotificationParameters : NotificationBase, INotificationCustom
{
    [LocalizedDisplay("Kullanıcı Adı", "tr")]
    [LocalizedDisplay("User Name", "en")]
    public string FirstName { get; set; }
    
    [LocalizedDisplay("Kullanıcı Soyadı", "tr")]
    [LocalizedDisplay("User LastName", "en")]
    public string LastName { get; set; }
    
    [LocalizedDisplay("Kullanıcı Kimlik Numarası", "tr")]
    [LocalizedDisplay("User Identity Number", "en")]
    public string IdentityNumber { get; set; }
    
    [LocalizedDisplay("Kullanıcı Doğum Tarihi", "tr")]
    [LocalizedDisplay("User Birth Date", "en")]
    public string BirthDate { get; set; }
    
    [LocalizedDisplay("Kullanıcı Tipi", "tr")]
    [LocalizedDisplay("User Type", "en")]
    public string UserType { get; set; }
    
    [LocalizedDisplay("Kullanıcı Durumu", "tr")]
    [LocalizedDisplay("User Status", "en")]
    public string UserStatus { get; set; }
    
    [LocalizedDisplay("Kullanıcı Telefon Kodu", "tr")]
    [LocalizedDisplay("User Phone Code", "en")]
    public string PhoneCode { get; set; }
    
    [LocalizedDisplay("Kullanıcı Telefon Numarası", "tr")]
    [LocalizedDisplay("User Phone Number", "en")]
    public string PhoneNumber { get; set; }
    
    [LocalizedDisplay("Kullanıcı Oluşturulma Tarihi", "tr")]
    [LocalizedDisplay("User Create Date", "en")]
    public string CreateDate { get; set; }
    
    [LocalizedDisplay("Kullanıcı Şifre Değiştirme Tarihi", "tr")]
    [LocalizedDisplay("User Password Modify Date", "en")]
    public string PasswordModifiedDate { get; set; }
    
    [LocalizedDisplay("Son Başarılı Giriş", "tr")]
    [LocalizedDisplay("Last SucceededLoginDate", "en")]
    public string LoginLastActivity_LastSucceededLogin { get; set; }
    
    [LocalizedDisplay("Son Başarısız Giriş", "tr")]
    [LocalizedDisplay("Last Failed Login", "en")]
    public string LoginLastActivity_LastFailedLogin { get; set; }
    
    [LocalizedDisplay("Son Kilitlenen Giriş", "tr")]
    [LocalizedDisplay("Last Locked Login", "en")]
    public string LoginLastActivity_LastLockedLogin { get; set; }
    
    [LocalizedDisplay("Giriş Sonucu", "tr")]
    [LocalizedDisplay("Login Result", "en")]
    public string LoginLastActivity_LoginResult { get; set; }
    
    [LocalizedDisplay("Kullanıcı Rolleri", "tr")]
    [LocalizedDisplay("User Roles", "en")]
    public string Roles { get; set; }
    
    [LocalizedDisplay("Giriş Aktiviteleri", "tr")]
    [LocalizedDisplay("Login Activities", "en")]
    public string LoginActivity { get; set; }

    public EventNotificationField EventNotificationField { get; set; } = EventNotificationField.All;
}