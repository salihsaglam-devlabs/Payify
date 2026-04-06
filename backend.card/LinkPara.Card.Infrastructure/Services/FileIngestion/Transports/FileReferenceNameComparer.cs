using System.Text.RegularExpressions;

namespace LinkPara.Card.Infrastructure.Services.FileIngestion.Transports;

public sealed class FileReferenceNameComparer : IComparer<string>
{
    public static FileReferenceNameComparer Instance { get; } = new();

    private static readonly Regex SequenceSuffixRegex = new(
        "^(?<prefix>.*?)(?:_(?<sequence>\\d+))?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private FileReferenceNameComparer()
    {
    }

    public int Compare(string? x, string? y)
    {
        if (ReferenceEquals(x, y))
            return 0;

        if (x is null)
            return -1;

        if (y is null)
            return 1;

        var xName = Path.GetFileNameWithoutExtension(x);
        var yName = Path.GetFileNameWithoutExtension(y);
        var xExtension = Path.GetExtension(x);
        var yExtension = Path.GetExtension(y);

        var xKey = CreateKey(xName, xExtension);
        var yKey = CreateKey(yName, yExtension);

        var prefixComparison = StringComparer.OrdinalIgnoreCase.Compare(xKey.Prefix, yKey.Prefix);
        if (prefixComparison != 0)
            return prefixComparison;

        var extensionComparison = StringComparer.OrdinalIgnoreCase.Compare(xKey.Extension, yKey.Extension);
        if (extensionComparison != 0)
            return extensionComparison;

        if (xKey.Sequence.HasValue && yKey.Sequence.HasValue)
        {
            var sequenceComparison = xKey.Sequence.Value.CompareTo(yKey.Sequence.Value);
            if (sequenceComparison != 0)
                return sequenceComparison;
        }

        return StringComparer.OrdinalIgnoreCase.Compare(x, y);
    }

    private static FileNameSortKey CreateKey(string fileNameWithoutExtension, string extension)
    {
        var match = SequenceSuffixRegex.Match(fileNameWithoutExtension);
        if (!match.Success)
            return new FileNameSortKey(fileNameWithoutExtension, extension, null);

        var prefix = match.Groups["prefix"].Value;
        var sequenceGroup = match.Groups["sequence"];
        int? sequence = sequenceGroup.Success
            ? int.Parse(sequenceGroup.Value, System.Globalization.CultureInfo.InvariantCulture)
            : null;

        return new FileNameSortKey(prefix, extension, sequence);
    }

    private sealed record FileNameSortKey(string Prefix, string Extension, int? Sequence);
}
