using System.ComponentModel.DataAnnotations;

namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveErrorDetail
{
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [MaxLength(2000)]
    public string Message { get; set; } = string.Empty;

    [MaxLength(4000)]
    public string? Detail { get; set; }

    [MaxLength(200)]
    public string? Step { get; set; }

    public Guid? AggregateId { get; set; }

    [MaxLength(20)]
    public string Severity { get; set; } = "Error";
}

