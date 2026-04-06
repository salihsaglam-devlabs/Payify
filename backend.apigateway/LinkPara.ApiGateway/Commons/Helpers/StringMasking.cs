using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkPara.ApiGateway.Commons.Helpers;

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
            .Select(word => word.Length <= 2
                ? word
                : word.Substring(0, 2) + new string('*', word.Length - 2))));
    }
}
