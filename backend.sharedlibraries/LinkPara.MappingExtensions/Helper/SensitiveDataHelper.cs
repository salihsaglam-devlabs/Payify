namespace LinkPara.MappingExtensions.Helper;

public static class  SensitiveDataHelper
{
    private const string DefaultMaskedData = "*****";
    public static string MaskSensitiveData(string canSeeSensitiveData, string propertyName, string data)
    {
        if (canSeeSensitiveData == "True")
        {
            return data;
        }

        switch (propertyName)
        {
            case "FirstName":
            case "LastName":
            case "Firstname":
            case "Lastname":
            case "AuthorizedPersonName":
            case "AuthorizedPersonSurname":
                return MaskName(data);
            case "UserFullName":
            case "Name":
            case "FullName":
            case "SenderName":
            case "ReceiverName":
            case "SenderNameSurname":
            case "ReceiverNameSurname":
            case "NameSurname":
            case "PayeeFullName":
            case "MerchantCustomerName":
            case "CardHolderName":
            case "MerchantName":
                return MaskFullName(data);
            case "IdentityNumber":
            case "AuthorizedPersonIdentityNumber":
                return MaskIdentityNumber(data);
            case "SenderTaxNumber":
            case "ReceiverTaxNumber":
                return MaskTaxNumber(data);
            case "PhoneNumber":
            case "PayeeMobile":
            case "CompanyPhoneNumber":
            case "MobilePhoneNumber":
            case "MobilePhoneNumberSecond":
            case "AuthorizedPersonCompanyPhoneNumber":
            case "AuthorizedPersonMobilePhoneNumber":
            case "AuthorizedPersonMobilePhoneNumberSecond":
            case "MerchantCustomerPhoneNumber":
                return MaskPhoneNumber(data);
            case "Email":
            case "PayeeEmail":
            case "CompanyEmail":
                return MaskEmail(data);
            case "Address":
                return MaskAddress(data);
            case "Iban":
            case "ReceiverIbanNumber":
            case "SenderIbanNumber":
            case "IBANNumber":
            case "IbanNumber":
                return MaskIbanNumber(data);
            case "CreditCard":
            case "CardNumber":
                return MaskCreditCard(data);
            case "Wallet":
                return data;
            default:
                return DefaultMaskedData;
        }
    }
    private static string MaskName(string name)
    {
        return string.IsNullOrEmpty(name) ? string.Empty : string.Concat(name.AsSpan(0, 1), new string('*', name.Length - 1));
    }
    
    private static string MaskFullName(string fullName)
    {
        if (string.IsNullOrEmpty(fullName))
        {
            return string.Empty;
        }
        var names = fullName.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        return names.Aggregate("", (current, name) => current + ' ' + MaskName(name));
    }
    
    private static string MaskIdentityNumber(string identityNumber)
    {
        return string.IsNullOrEmpty(identityNumber) || identityNumber.Length < 3 ? string.Empty : string.Concat(identityNumber.AsSpan(0, 3), new string('*', identityNumber.Length - 3));
    }
    
    private static string MaskTaxNumber(string taxNumber)
    {
        return string.IsNullOrEmpty(taxNumber) || taxNumber.Length < 3? string.Empty : string.Concat(taxNumber.AsSpan(0, 3), new string('*', taxNumber.Length - 3));
    }
    
    private static string MaskPhoneNumber(string phoneNumber)
    {
        return string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 5 ? string.Empty : $"{phoneNumber[0]}{new string('*', phoneNumber.Length - 5)}{phoneNumber[^4..]}";
    }
    
    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            return string.Empty;
        }
        
        var atIndex = email.IndexOf('@');
        return atIndex <= 0 ? email : string.Concat(new string('*', atIndex), email[atIndex..]);
    }
    
    private static string MaskAddress(string address)
    {
        return string.IsNullOrEmpty(address) ? string.Empty : new string('*', address.Length);
    }
    
    private static string MaskIbanNumber(string iban)
    {
        return (string.IsNullOrEmpty(iban) || iban.Length <= 9) ? string.Empty : string.Concat(iban.AsSpan(0, 9), new string('*', iban.Length - 9));
    }
    
    private static string MaskCreditCard(string creditCard)
    {
        return string.IsNullOrEmpty(creditCard) || creditCard.Length < 10 ? string.Empty : 
            string.Concat(creditCard[..6],new string('*', creditCard.Length - 10), creditCard[^4..]);
    }
}