namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckNfcRequest
{
    public string SessionId { get; set; }
    public string IdentityNumber { get; set; }
    public string FullName { get; set; }
    public bool IsRead { get; set; }
    public ScSoftVerifyNfcRequest NfcRequest { get; set; }
}
public class ScSoftVerifyNfcRequest
{
    public string Session_guid { get; set; }
    public string Base64_cert { get; set; }
    public DateTime Function_start_time { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Personal_number { get; set; }
    public string Gender { get; set; }
    public string Birth_date { get; set; }
    public string Expiry_date { get; set; }
    public string Serial_number { get; set; }
    public string Nationality { get; set; }
    public string Issuer_authority { get; set; }
    public string Face_image_base64 { get; set; }
    public string Portrait_image_base64 { get; set; }
    public string Signature_base64 { get; set; }
    public string Custody_information { get; set; }
    public string Full_date_of_birth { get; set; }
    public string Name_of_holder { get; set; }
    public string[] Other_names { get; set; }
    public string[] Other_valid_id_numbers { get; set; }
    public string[] Permanent_address { get; set; }
    public string Personal_number_1 { get; set; }
    public string Personal_summary { get; set; }
    public string[] Place_of_birth { get; set; }
    public string Profession { get; set; }
    public string Proof_of_citizenship_base64 { get; set; }
    public string Tag { get; set; }
    public string[] Tag_presence_list { get; set; }
    public string Telephone { get; set; }
    public string Title { get; set; }
    public bool Cert_status { get; set; }
    public string Cert_status_message { get; set; }
}
