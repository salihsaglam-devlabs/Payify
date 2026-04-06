using System.Globalization;

namespace LinkPara.Identity.Application.Common.Helpers
{
    public static class PascalCaseHelper
    {
        public static string CapitalFirstLetter(string word)
		{
			try
			{
                return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(word.ToLower());
            }
			catch (Exception)
			{

				throw;
			}           
        }
    }
}
