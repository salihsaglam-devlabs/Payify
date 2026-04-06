using LinkPara.ApiGateway.Commons.Mappings;

namespace LinkPara.ApiGateway.Services.Identity.Models.Requests;

public class RegisterRequest
{
    public string Email { get; set; }
    public string PhoneCode { get; set; }
    public string PhoneNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public DateTime BirthDate { get; set; }
    public bool IysPermission { get; set; }
    public Guid ParentAccountId { get; set; }
    public string IdentityNumber { get; set; }
    public List<Guid> AgreedDocuments { get; set; }
    public Guid? SecurityQuestionId { get; set; }
    public string SecurityAnswer { get; set; }
    public string Profession { get; set; }
    public string RecaptchaToken { get; set; }
}

public class RegisterWithUserName : RegisterRequest, IMapFrom<RegisterRequest>
{
    public string UserName { get; set; }
}