using System.Text;

namespace LinkPara.Card.Application.Commons.Helpers;

public static class ExceptionDetailHelper
{
    public static string BuildDetailMessage(Exception ex)
    {
        var sb = new StringBuilder();
        var current = ex;
        while (current != null)
        {
            if (sb.Length > 0)
                sb.Append(" => ");

            sb.Append(current.GetType().Name);
            sb.Append(": ");
            sb.Append(current.Message);
            current = current.InnerException;
        }

        return sb.ToString();
    }
    
    public static string BuildDetailMessage(Exception ex, int maxLength)
    {
        var message = BuildDetailMessage(ex);
        return message.Length <= maxLength ? message : message[..maxLength];
    }
}

