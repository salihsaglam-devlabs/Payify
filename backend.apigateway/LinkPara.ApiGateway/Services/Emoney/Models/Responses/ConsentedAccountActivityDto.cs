namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ConsentedAccountActivityDto
{
    public string HspRef { get; set; }
    public List<ActivitiesDto> Isller { get; set; }
}

public class ActivitiesDto
{
    public ActivityMainInfo IslTml { get; set; }
    public ActivityDetailInfo IslDty { get; set; }
}

public class ActivityMainInfo
{
    public string IslNo { get; set; }
    public string RefNo { get; set; }
    public string IslTtr { get; set; }
    public string PrBrm { get; set; }
    public DateTime IslGrckZaman { get; set; }
    public string BrcAlc { get; set; }
    public string Kanal { get; set; }
    public string IslTur { get; set; }
    public string IslAmc { get; set; }
    public string OdmStmNo { get; set; }
}

public class ActivityDetailInfo
{
    public string IslAcklm { get; set; }
    public OtherPersonInfo KrsTrf { get; set; }
}

public class OtherPersonInfo
{
    public string KrsMskIBAN { get; set; }
    public string KrsMskUnvan { get; set; }
}
