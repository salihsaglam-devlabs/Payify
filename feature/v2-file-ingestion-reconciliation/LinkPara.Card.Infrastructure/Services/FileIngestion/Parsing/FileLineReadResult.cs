namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Parsing;

public class FileLineReadResult
{
    public long LineNumber { get; init; }
    public long ByteOffset { get; init; }
    public int ByteLength { get; init; }
    public int ConsumedByteLength { get; init; }
    public long NextByteOffset => ByteOffset + ConsumedByteLength;
    public string Line { get; init; } = string.Empty;
}
