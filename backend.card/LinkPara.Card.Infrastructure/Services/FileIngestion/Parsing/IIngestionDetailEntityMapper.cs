using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public interface IIngestionDetailEntityMapper
{
    void AttachTypedDetail(IngestionFileLine row, string profileKey, object parsedDataModel);
}

