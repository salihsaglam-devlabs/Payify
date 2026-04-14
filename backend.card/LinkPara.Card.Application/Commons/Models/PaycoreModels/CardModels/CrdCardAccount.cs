using System.Text.Json.Serialization;
namespace LinkPara.Card.Application.Commons.Models.PaycoreModels.CardModels;

public class CrdCardAccount
{
    [JsonInclude]
    public CrdAccount CrdAccount { get; set; }
}

public class CrdAccount
{
    [JsonInclude]
    public CrdAccountCommunication CrdAccountCommunication { get; set; }
}

public class CrdAccountCommunication
{
    public int MobilePhoneIdx { get; set; }
    public string MobilePhone { get; set; }
    public string Email { get; set; }
    public int EmailIdx { get; set; }
}
