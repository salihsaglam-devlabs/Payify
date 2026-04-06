namespace LinkPara.PF.Application.Commons.Models.VposModels.Response;

public class PosVerify3dModelResponse : PosResponseBase
{
    public string CallbackUrl { get; set; }
    public string HashParams { get; set; }
    public string HashParamsVal { get; set; }
    public string Hash { get; set; }
    public string MD { get; set; }
    public string Cavv { get; set; }
    public string Eci { get; set; }
    public string PayerTxnId { get; set; }
    public string MdStatus { get; set; }
    public string MdErrorMessage { get; set; }
    public string TxnStat { get; set; }
    public string ThreeDStatus { get; set; }
    public string BankPacket { get; set; }
}
