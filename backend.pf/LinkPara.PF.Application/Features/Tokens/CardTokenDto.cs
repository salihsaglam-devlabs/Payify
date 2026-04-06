using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;

namespace LinkPara.PF.Application.Features.Tokens;

public class CardTokenDto : IMapFrom<CardToken>
{
    public string CardToken { get; set; }
    public string Signature { get; set; }
    public string ConversationId { get; set; }
}
