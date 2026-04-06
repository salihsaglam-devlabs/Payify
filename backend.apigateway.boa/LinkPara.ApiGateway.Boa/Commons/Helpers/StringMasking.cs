namespace LinkPara.ApiGateway.Boa.Commons.Helpers;

public interface IStringMasking
{
    Task<string> MaskStringAsync(string input);
}
public class StringMasking : IStringMasking
{
    public async Task<string> MaskStringAsync(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        return await Task.FromResult(string.Join(" ", input.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(word => char.ToUpper(word[0]) + new string('*', word.Length - 1))));
    }
}
