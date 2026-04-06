using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.PF.Application.Commons.Helpers
{
    public static class ScreenServiceHelper
    {
        public static Dictionary<string, object> GetUpdatedFields<T>(JsonPatchDocument<T> request, IStringLocalizer _localizer) where T : class 
        {

            var dic = new Dictionary<string, object>();
            foreach (var operation in request.Operations)
            {
                var updatedField = new Dictionary<string, object>
                {
                    {"OldValue", CapitalizeFirstLetter(new []{operation.from?.ToString()}, _localizer) },
                    {"NewValue", CapitalizeFirstLetter(new []{operation.value?.ToString()}, _localizer) }
                };
                var split = operation.path.Split('/');
                var field = CapitalizeFirstLetter(split, _localizer);
                dic.Add(field, updatedField);
            }
            return dic;
        }
        private static string CapitalizeFirstLetter(string[] inputStrings, IStringLocalizer _localizer)
        {
            var cultureInfo = new CultureInfo("en-US");
            var result = "";
            foreach (var word  in inputStrings)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    if (!int.TryParse(word, out _))
                    {
                        var firstLetter = char.ToUpper(word[0], cultureInfo);
                        var remainingLetters = word.Substring(1);
                        var localizedString = _localizer.GetString(firstLetter + remainingLetters).Value;
                        result += localizedString + " ";
                    }
                    else
                    {
                        result += word + " ";
                    }
                }
            }

            return result.TrimEnd();

        }
    }
}
