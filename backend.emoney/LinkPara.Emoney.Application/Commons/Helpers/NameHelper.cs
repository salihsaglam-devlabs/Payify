using LinkPara.Emoney.Application.Commons.Models;

namespace LinkPara.Emoney.Application.Commons.Helpers;

public static class NameHelper
{
    public static NameLastName ParseName(string name)
    {
        name = name.Trim();

        var names = name.Split(' ').ToList();

        var firstName = string.Empty;
        var lastName = string.Empty;

        if (names.Count == 1)
        {
            firstName = name;
        }
        else
        {
            lastName = names.Last();
            names.RemoveAt(names.Count - 1);
            firstName = string.Join(" ", names.ToArray());
        }

        return new NameLastName
        {
            FirstName = firstName,
            LastName = lastName
        };
    }
}
