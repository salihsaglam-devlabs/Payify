namespace LinkPara.SharedModels.BusModels.Commands.Emoney;
public class CreateIndividualKycUser
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime BirthDate { get; set; }
    public string UserName { get; set; }
    public string IdentityNumber { get; set; }
    public bool IysPermission { get; set; }
}
