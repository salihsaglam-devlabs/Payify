using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Identity.Application.Features.SecurityPictures.Queries;

public class SecurityPictureDto : IMapFrom<SecurityPicture>
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public byte[] Bytes { get; set; }
    public string ContentType { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
