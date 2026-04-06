namespace LinkPara.ApiGateway.BackOffice.Services.Epin.Models.Responses;

public class BrandDto
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Image { get; set; }
    public string Summary { get; set; }
    public string Description { get; set; }
    public Guid PublisherId { get; set; }
}
