using System.Text;

namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPayInit3dModelRequest : NestPaymentBase
{
    public string PostUrl { get; set; }
    public int? BlockageCode { get; set; }
    private string BuildRequestCommon()
    {
        var request = new StringBuilder();

        request.AppendLine("<html>");
        request.AppendLine("<head>");
        request.AppendLine("</head>");
        request.AppendLine("<body onload=\"document.ThreeDPostForm.submit()\">");
        request.AppendLine($"<form name=\"ThreeDPostForm\" method=\"post\" action=\"{PostUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"clientId\" value=\"{ClientId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"storeType\" value=\"{StoreType}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"oid\" value=\"{OrderId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"okUrl\" value=\"{OkUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"failUrl\" value=\"{FailUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"lang\" value=\"{LanguageCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"amount\" value=\"{Amount}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"currency\" value=\"{CurrencyCode.ToString()}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"pan\" value=\"{Pan}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Year\" value=\"{ExpireYear}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Month\" value=\"{ExpireMonth}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"islemtipi\" value=\"{Type}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hashAlgorithm\" value=\"{HashAlgorithm}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"taksit\" value=\"{NumberOfInstallments}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNAME\" value=\"{SubMerchantName}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTID\" value=\"{SubMerchantId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTPOSTALCODE\" value=\"{SubMerchantPostalCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCITY\" value=\"{SubMerchantCity}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCOUNTRY\" value=\"{SubMerchantCountry}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTMCC\" value=\"{SubMerchantMcc}\">");

        return request.Replace("\r\n", " ").ToString();
    }

    public string BuildRequest()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        if (NumberOfInstallments <= 1)
        {
            NumberOfInstallments = null;
        }

        if (SubMerchantName.Length > 20)
        {
            SubMerchantName = SubMerchantName.Substring(0, 20);
        }

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            Type,
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            SubMerchantId,
            SubMerchantMcc,
            SubMerchantName,
            SubMerchantGlobalMerchantId,
            SubMerchantPostalCode,
            SubMerchantUrl,
            NumberOfInstallments.ToString(),
        };

        if (IsBlockaged == true)
        {
            hashList = new List<string>
            {
                Amount,
                BlockageCode.ToString(),
                ClientId,
                CurrencyCode.ToString(),
                ExpireMonth,
                ExpireYear,
                FailUrl,
                HashAlgorithm,
                Type,
                LanguageCode,
                OrderId,
                OkUrl,
                Pan,
                Rnd,
                StoreType,
                SubMerchantCity,
                SubMerchantCountry,
                SubMerchantId,
                SubMerchantMcc,
                SubMerchantName,
                SubMerchantGlobalMerchantId,
                SubMerchantPostalCode,
                SubMerchantUrl,
                NumberOfInstallments.ToString(),
            };
        }

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);

        var commonRequestXml = BuildRequestCommon();

        var requestXml = new StringBuilder(commonRequestXml);
        if (IsBlockaged == true)
        {
            requestXml.AppendLine($"<input type=\"hidden\" name=\"bolum\" value=\"{BlockageCode}\">");
        }
        requestXml.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTURL\" value=\"{SubMerchantUrl}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNIN\" value=\"{SubMerchantGlobalMerchantId}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        requestXml.AppendLine("</form>");
        requestXml.AppendLine("</body>");
        requestXml.AppendLine("</html>");

        return requestXml.Replace("\r\n", " ").ToString();
    }
    public string BuildRequestAkbank()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        if (NumberOfInstallments <= 1)
        {
            NumberOfInstallments = null;
        }

        if (SubMerchantName.Length > 20)
        {
            SubMerchantName = SubMerchantName.Substring(0, 20);
        }

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            Type,
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            SubMerchantId,
            SubMerchantMcc,
            SubMerchantName,
            SubMerchantPostalCode,
            NumberOfInstallments.ToString(),
            VisaPfId,
            VisaSubmerchantId
        };

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);
        var commonRequestXml = BuildRequestCommon();

        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<input type=\"hidden\" name=\"VISASUBMERCHANTID\" value=\"{VisaSubmerchantId}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"VISAPFID\" value=\"{VisaPfId}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        requestXml.AppendLine("</form>");
        requestXml.AppendLine("</body>");
        requestXml.AppendLine("</html>");

        return requestXml.Replace("\r\n", " ").ToString();
    }
    public string BuildRequestSekerbank()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        if (NumberOfInstallments <= 1)
        {
            NumberOfInstallments = null;
        }

        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }

        List<string> hashList;

        hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            Type,
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            SubMerchantId,
            SubMerchantMcc,
            SubMerchantName,
            SubMerchantNumber,
            SubMerchantPostalCode,
            NumberOfInstallments.ToString()
        };

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);
        var commonRequestXml = BuildRequestCommon();

        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNUMBER\" value=\"{SubMerchantNumber}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        requestXml.AppendLine("</form>");
        requestXml.AppendLine("</body>");
        requestXml.AppendLine("</html>");

        return requestXml.Replace("\r\n", " ").ToString();
    }
    public string BuildRequestZiraat()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        if (NumberOfInstallments <= 1)
        {
            NumberOfInstallments = null;
        }
        
        if (SubMerchantName?.Length > 14)
        {
            SubMerchantName = SubMerchantName.Substring(0, 14);
        }

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            Type,
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantName,//SubMerchantId
            SubMerchantMcc,
            string.Empty,//SubMerchantName
            SubMerchantId,//SubMerchantNumber
            NumberOfInstallments.ToString(),
        };

        String hashVal = "";
        StoreKey = (StoreKey ?? "").Replace("\\", "\\\\").Replace("|", "\\|");

        foreach (var item in hashList)
        {
            String escapedValue = (item ?? "").Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }

        hashVal += StoreKey;


        Hash = VposHelper.GetShaInit1512(hashVal);

        var request = new StringBuilder();
        request.AppendLine("<html>");
        request.AppendLine("<head>");
        request.AppendLine("</head>");
        request.AppendLine("<body onload=\"document.ThreeDPostForm.submit()\">");
        request.AppendLine($"<form name=\"ThreeDPostForm\" method=\"post\" action=\"{PostUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"clientId\" value=\"{ClientId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"storeType\" value=\"{StoreType}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"oid\" value=\"{OrderId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"okUrl\" value=\"{OkUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"failUrl\" value=\"{FailUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"lang\" value=\"{LanguageCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"amount\" value=\"{Amount}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"currency\" value=\"{CurrencyCode.ToString()}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"pan\" value=\"{Pan}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Year\" value=\"{ExpireYear}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Month\" value=\"{ExpireMonth}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"islemtipi\" value=\"{Type}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hashAlgorithm\" value=\"{HashAlgorithm}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"taksit\" value=\"{NumberOfInstallments}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNUMBER\" value=\"{SubMerchantId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNAME\" value=\"\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTMCC\" value=\"{SubMerchantMcc}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTID\" value=\"{SubMerchantName}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        request.AppendLine("</form>");
        request.AppendLine("</body>");
        request.AppendLine("</html>");

        return request.Replace("\r\n", " ").ToString();
    }
    public string BuildRequestTfkb()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        NumberOfInstallments = (NumberOfInstallments == null || NumberOfInstallments == 1) ? 0 : NumberOfInstallments;

        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            NumberOfInstallments.ToString(),
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            VisaSubmerchantId,
            SubMerchantId,
            SubMerchantMcc,
            SubMerchantPostalCode,
            SubMerchantTaxNumber,
            Type
        };

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);

        var request = new StringBuilder();
        request.AppendLine("<html>");
        request.AppendLine("<head>");
        request.AppendLine("</head>");
        request.AppendLine("<body onload=\"document.ThreeDPostForm.submit()\">");
        request.AppendLine($"<form name=\"ThreeDPostForm\" method=\"post\" action=\"{PostUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"clientid\" value=\"{ClientId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"storetype\" value=\"{StoreType}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"oid\" value=\"{OrderId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"okurl\" value=\"{OkUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"failUrl\" value=\"{FailUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"lang\" value=\"{LanguageCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"amount\" value=\"{Amount}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"currency\" value=\"{CurrencyCode.ToString()}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"pan\" value=\"{Pan}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Year\" value=\"{ExpireYear}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Month\" value=\"{ExpireMonth}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"TranType\" value=\"{Type}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hashAlgorithm\" value=\"{HashAlgorithm}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Installment\" value=\"{NumberOfInstallments}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTID\" value=\"{SubMerchantId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTPOSTALCODE\" value=\"{SubMerchantPostalCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCITY\" value=\"{SubMerchantCity}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCOUNTRY\" value=\"{SubMerchantCountry}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTMCC\" value=\"{SubMerchantMcc}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTFACILITATORID\" value=\"{VisaSubmerchantId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTTCKNVKN\" value=\"{SubMerchantTaxNumber}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        request.AppendLine("</form>");
        request.AppendLine("</body>");
        request.AppendLine("</html>");

        return request.Replace("\r\n", " ").ToString();
    }
    public string BuildRequestHalkbank()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        if (SubMerchantName.Length > 25)
        {
            SubMerchantName = SubMerchantName.Substring(0, 25);
        }

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            Type,
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            SubMerchantId,
            SubMerchantMcc,
            SubMerchantName,
            SubMerchantPostalCode,
            NumberOfInstallments.ToString()
        };

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);
        var commonRequestXml = BuildRequestCommon();
    
        var requestXml = new StringBuilder(commonRequestXml);
        requestXml.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        requestXml.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        requestXml.AppendLine("</form>");
        requestXml.AppendLine("</body>");
        requestXml.AppendLine("</html>");

        return requestXml.Replace("\r\n", " ").ToString();
    }

    public string BuildRequestAnadoluBank()
    {
        Rnd = DateTime.Now.Ticks.ToString();

        var hashList = new List<string>
        {
            Amount,
            ClientId,
            CurrencyCode.ToString(),
            Cvv,
            ExpireMonth,
            ExpireYear,
            FailUrl,
            HashAlgorithm,
            NumberOfInstallments.ToString(),
            LanguageCode,
            OrderId,
            OkUrl,
            Pan,
            Rnd,
            StoreType,
            SubMerchantCity,
            SubMerchantCountry,
            SubMerchantTaxNumber,
            SubMerchantMcc,
            SubMerchantName,
            SubMerchantPostalCode,
            Type
        };

        String hashVal = "";
        StoreKey = StoreKey.Replace("\\", "\\\\").Replace("|", "\\|");
        foreach (var item in hashList)
        {
            String escapedValue = item.Replace("\\", "\\\\").Replace("|", "\\|");
            hashVal += escapedValue + "|";
        }
        hashVal += StoreKey;

        Hash = VposHelper.GetShaInit1512(hashVal);
        
        var request = new StringBuilder();
        request.AppendLine("<html>");
        request.AppendLine("<head>");
        request.AppendLine("</head>");
        request.AppendLine("<body onload=\"document.ThreeDPostForm.submit()\">");
        request.AppendLine($"<form name=\"ThreeDPostForm\" method=\"post\" action=\"{PostUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"clientId\" value=\"{ClientId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"storetype\" value=\"{StoreType}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"oid\" value=\"{OrderId}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"okUrl\" value=\"{OkUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"failUrl\" value=\"{FailUrl}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"lang\" value=\"{LanguageCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"amount\" value=\"{Amount}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"currency\" value=\"{CurrencyCode.ToString()}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"pan\" value=\"{Pan}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"cv2\" value=\"{Cvv}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Year\" value=\"{ExpireYear}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Ecom_Payment_Card_ExpDate_Month\" value=\"{ExpireMonth}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"TranType\" value=\"{Type}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hashAlgorithm\" value=\"{HashAlgorithm}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"Instalment\" value=\"{NumberOfInstallments}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTNAME\" value=\"{SubMerchantName}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTMCC\" value=\"{SubMerchantMcc}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTID\" value=\"{SubMerchantTaxNumber}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTPOSTALCODE\" value=\"{SubMerchantPostalCode}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCITY\" value=\"{SubMerchantCity}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"SUBMERCHANTCOUNTRY\" value=\"{SubMerchantCountry}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"hash\" value=\"{Hash}\">");
        request.AppendLine($"<input type=\"hidden\" name=\"rnd\" value=\"{Rnd}\">");
        request.AppendLine("</form>");
        request.AppendLine("</body>");
        request.AppendLine("</html>");

        return request.ToString();
    }
}
