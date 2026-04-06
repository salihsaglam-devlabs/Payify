using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Responses;

public class MerchantPreApplicationDto
{
    public Guid Id { get; set; }
    public string FullName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public string ResponsiblePerson { get; set; }
    public PosProductType ProductTypes { get; set; }
    public MonthlyTurnover MonthlyTurnover { get; set; }
    public ApplicationStatus ApplicationStatus { get; set; }
    public string Website { get; set; }
    public bool ConsentConfirmation { get; set; }
    public bool KvkkConfirmation { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }
    public List<MerchantPreApplicationHistory> ApplicationHistories { get; set; }
}

public class MerchantPreApplicationHistory
{
    public Guid Id { get; set; }
    public Guid MerchantPreApplicationId { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public ApplicationOperationType OperationType { get; set; }
    public DateTime OperationDate { get; set; }
    public string OperationNote { get; set; }
}