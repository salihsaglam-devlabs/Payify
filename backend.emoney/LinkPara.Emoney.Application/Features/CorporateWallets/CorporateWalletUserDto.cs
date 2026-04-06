using LinkPara.Emoney.Application.Commons.Mappings;
using LinkPara.Emoney.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Emoney.Application.Features.CorporateWallets;

public class CorporateWalletUserDto : IMapFrom<AccountUser>
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
}
