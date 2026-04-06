namespace LinkPara.ApiGateway.Merchant.Commons.Models.AuthorizationModels;

public static class ConstUserRoles
{
    public static readonly string MemberOfBoard = "MemberOfBoard";
    public static readonly string GeneralDirector = "GeneralDirector";
    public static readonly string InternalCheck = "InternalCheck";
    public static readonly string RiskManagement = "RiskManagement";

    public static readonly string ITManager = "ITManager";
    public static readonly string ITSpecialist = "ITSpecialist";

    public static readonly string ProducManagement = "ProducManagement";
    public static readonly string AccountingAndFinanceManager = "AccountingAndFinanceManager";

    public static readonly string ITSecurity = "ITSecurity";

    public static readonly string OperationManagement = "OperationManagement";
    public static readonly string OperationSpecialist = "OperationSpecialist";

    public static readonly string SalesAndSupportManagement = "SalesAndSupportManagement";
    public static readonly string SalesAndSupportSpecialist = "SalesAndSupportSpecialist";

    public static readonly string CustomerRelaitionManagement = "CustomerRelaitionManagement";
    public static readonly string RepresentativeSpecialist = "RepresentativeSpecialist";

    //Default roles if hasn't defined any role.
    public static readonly string DefaultMember = "DefaultMember";
    public static readonly string DefaultInternalMember = "DefaultInternalMember";
}