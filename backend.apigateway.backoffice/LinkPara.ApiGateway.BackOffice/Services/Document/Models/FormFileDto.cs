using System.ComponentModel.DataAnnotations;

namespace LinkPara.ApiGateway.BackOffice.Services.Document.Models;

public class FormFileDto
{
    [Required]
    public IFormFile File { get; set; }
    [Required]
    public Guid DocumentTypeId { get; set; }
}
