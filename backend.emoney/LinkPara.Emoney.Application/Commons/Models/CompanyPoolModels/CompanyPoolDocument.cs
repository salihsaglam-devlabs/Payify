using Microsoft.AspNetCore.Http;

namespace LinkPara.Emoney.Application.Commons.Models.CompanyPoolModels;

public class CompanyPoolDocument
{
    public IFormFile File { get; set; }
    public Guid DocumentTypeId { get; set; }
}
