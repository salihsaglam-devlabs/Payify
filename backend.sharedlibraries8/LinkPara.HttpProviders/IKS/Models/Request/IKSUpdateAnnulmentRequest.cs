namespace LinkPara.HttpProviders.IKS.Models.Request
{
    public class IKSUpdateAnnulmentRequest : IKSSaveAnnulmentRequest
    {
        public bool IsCancelCode { get; set; }
    }
}
