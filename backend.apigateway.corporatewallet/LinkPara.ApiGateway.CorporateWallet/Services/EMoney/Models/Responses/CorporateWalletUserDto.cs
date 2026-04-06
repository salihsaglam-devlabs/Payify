using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Emoney.Models.Responses;
public class CorporateWalletUserDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Firstname { get; set; }
    public string Lastname { get; set; }
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public DateTime CreateDate { get; set; }
    public string FullName => $"{Firstname} {Lastname}";
}

