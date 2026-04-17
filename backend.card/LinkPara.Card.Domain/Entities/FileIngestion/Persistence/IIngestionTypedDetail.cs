namespace LinkPara.Card.Domain.Entities.FileIngestion.Persistence;

public interface IIngestionTypedDetail
{
    Guid FileLineId { get; set; }
    IngestionFileLine FileLine { get; set; }
}

