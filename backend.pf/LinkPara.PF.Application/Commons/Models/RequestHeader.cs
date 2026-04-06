using Microsoft.AspNetCore.Mvc;

namespace LinkPara.PF.Application.Commons.Models;


public class RequestHeader
{
    [FromHeader]
    public string MerchantName { get; set; }
    [FromHeader]
    public string ConversationId { get; set; }
    [FromHeader]
    public string TransactionDate { get; set; }
}