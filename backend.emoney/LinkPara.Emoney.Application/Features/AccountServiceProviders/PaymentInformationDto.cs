namespace LinkPara.Emoney.Application.Features.AccountServiceProviders;

public class PaymentInformationDto
{
    /// <summary>
    /// Gets or sets the identity information.
    /// </summary>
    public IdentityInfoDto Kmlk { get; set; }

    /// <summary>
    /// Gets or sets the price information.
    /// </summary>
    public PriceInfo IslTtr { get; set; }

    /// <summary>
    /// Gets or sets the price information
    /// </summary>
    public PriceInfo GrckIslTtr { get; set; }

    /// <summary>
    /// Gets or sets the person information for "Gon".
    /// </summary>
    public PersonInfo Gon { get; set; }

    /// <summary>
    /// Gets or sets the person information for "Alc".
    /// </summary>
    public PersonInfo Alc { get; set; }

    /// <summary>
    /// Gets or sets the QR code information.
    /// </summary>
    public QrCodeInfo Kkod { get; set; }

    /// <summary>
    /// Gets or sets the payment detail.
    /// </summary>
    public PaymentDetail OdmAyr { get; set; }
}

public class PriceInfo
{
    /// <summary>
    /// Gets or sets the price branch.
    /// </summary>
    public string PrBrm { get; set; }

    /// <summary>
    /// Gets or sets the transaction type.
    /// </summary>
    public string Ttr { get; set; }
}

public class PersonInfo
{
    /// <summary>
    /// Gets or sets the person's title.
    /// </summary>
    public string Unv { get; set; }

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string HspNo { get; set; }

    /// <summary>
    /// Gets or sets the account reference.
    /// </summary>
    public string HspRef { get; set; }

    /// <summary>
    /// Gets or sets the Kolas information.
    /// </summary>
    public KolasInfoType Kolas { get; set; }
}

public class KolasInfoType
{
    /// <summary>
    /// Gets or sets the Kolas type.
    /// </summary>
    public string KolasTur { get; set; }

    /// <summary>
    /// Gets or sets the Kolas degree.
    /// </summary>
    public string KolasDgr { get; set; }

    /// <summary>
    /// Gets or sets the Kolas reference number.
    /// </summary>
    public string KolasRefNo { get; set; }

    /// <summary>
    /// Gets or sets the Kolas account type.
    /// </summary>
    public string KolasHspTur { get; set; }
}

public class QrCodeInfo
{
    /// <summary>
    /// Gets or sets the QR code type.
    /// //Enum 01-02-03
    /// </summary>
    public string AksTur { get; set; } 

    /// <summary>
    /// Gets or sets the QR code reference.
    /// </summary>
    public string KkodRef { get; set; }

    /// <summary>
    /// Gets or sets the QR code URTC code.
    /// </summary>
    public string KkodUrtcKod { get; set; }
}

public class PaymentDetail
{
    /// <summary>
    /// Gets or sets the payment confirmation.
    /// //Enum I-A-T-K-S-M-D-O
    /// </summary>
    public string OdmKynk { get; set; }

    /// <summary>
    /// Gets or sets the payment code.
    /// //01 to 11
    /// </summary>
    public string OdmAmc { get; set; } 

    /// <summary>
    /// Gets or sets the reference information.
    /// </summary>
    public string RefBlg { get; set; }

    /// <summary>
    /// Gets or sets the payment acknowledgement.
    /// </summary>
    public string OdmAcklm { get; set; }

    /// <summary>
    /// Gets or sets the OHK message.
    /// </summary>
    public string OhkMsj { get; set; }

    /// <summary>
    /// Gets or sets the payment method.
    /// // H:Havale - F:Fast - E:Eft(PÖS)
    /// </summary>
    public string OdmStm { get; set; } 

    /// <summary>
    /// Gets or sets the payment method number for inquiry.
    /// </summary>
    public string OdmStmNo { get; set; }

    /// <summary>
    /// Gets or sets the expected payment time for PÖS payment system outside of PÖS transaction hours.
    /// </summary>
    public string BekOdmZmn { get; set; }    

    /// <summary>
    /// Gets or sets the expected payment Date .
    /// </summary>
    public DateTime TlmtTrh { get; set; }

    /// <summary>
    /// Gets or sets the payment Date .
    /// </summary>
    public string GrckOdmZmn { get; set; }
    
    /// <summary>
    /// Gets or sets the status of the Payment.
    /// </summary>
    public string OdmDrm { get; set; }
}
