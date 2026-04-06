using HtmlAgilityPack;
using LinkPara.LogConsumers.Commons.ConfidentialKeyConfiguration;
using LinkPara.LogConsumers.Commons.Interfaces;
using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Web;
using System.Xml;

namespace LinkPara.LogConsumers.Services;

public class ConfidentialService : IConfidentialService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<ConfidentialService> _logger;

    public ConfidentialService(IConfiguration configuration,
        ILogger<ConfidentialService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string MaskData(string value, IntegrationLogDataType? dataType, string name = null)
    {
        if (String.IsNullOrEmpty(value))
        {
            return String.Empty;
        }

        var confidentialSettings = new ConfidentialSettings();

        switch (dataType)
        {
            case IntegrationLogDataType.Json:
                {
                    return MaskJsonData(value, name, confidentialSettings);
                }
            case IntegrationLogDataType.Soap:
                {
                    return MaskSoapData(value, name, confidentialSettings);
                }
            case IntegrationLogDataType.Text:
                {
                    return MaskTextData(ref value, name, confidentialSettings);
                }
            case IntegrationLogDataType.Html:
                {
                    return MaskHtmlData(value, name, confidentialSettings);
                }
            default:
                return string.Empty;
        }
    }

    private string MaskHtmlData(string value, string name, ConfidentialSettings confidentialSettings)
    {
        try
        {
            HtmlDocument document = new();
            document.LoadHtml(value);

            _configuration.GetSection(nameof(ConfidentialSettings)).Bind(confidentialSettings);

            var parameters = (List<string>)confidentialSettings.GetType().GetProperty(name)?.GetValue(confidentialSettings);

            HtmlNode input;
            string htmlValue = string.Empty;
            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    input = document.DocumentNode.SelectSingleNode($"//input[@name='{parameter}']");

                    if (input is not null)
                    {
                        htmlValue = input.GetAttributeValue("value", "");
                        input.SetAttributeValue("value", Obscure(htmlValue, parameter));
                    }
                }
            }
            return document.DocumentNode.OuterHtml;
        }
        catch (Exception exception)
        {
            _logger.LogError("Exception on html value : '{Value}',\n Exception Detail: {Exception} ", Obscure(value, string.Empty), exception);
            return value;
        }
        
    }

    private string MaskTextData(ref string value, string name, ConfidentialSettings confidentialSettings)
    {
        try
        {
            _configuration.GetSection(nameof(ConfidentialSettings)).Bind(confidentialSettings);

            if (value.Contains(";;"))
            {
                value = value.Replace(";;", ";").Replace(";", "&");
            }

            var textParams = HttpUtility.ParseQueryString(value);

            var parameters = (List<string>)confidentialSettings.GetType().GetProperty(name)?.GetValue(confidentialSettings);

            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    if (!string.IsNullOrEmpty(textParams[parameter]))
                    {
                        textParams[parameter] = Obscure(textParams[parameter], parameter);
                    }
                }
            }
            var builder = new StringBuilder();

            foreach (string param in textParams.Keys)
            {
                builder.Append(param);
                builder.Append("= ");
                builder.Append(textParams[param] + "&");
            }

            return builder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception on text value : '{Value}',\n Exception Detail: {Ex} ", Obscure(value, string.Empty), ex);
            return value;
        }
    }

    private string MaskSoapData(string value, string name, ConfidentialSettings confidentialSettings)
    {
        try
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(value);
            _configuration.GetSection(nameof(ConfidentialSettings)).Bind(confidentialSettings);

            var parameters = (List<string>)confidentialSettings.GetType().GetProperty(name)?.GetValue(confidentialSettings);

            if (parameters is not null)
            {
                foreach (var parameter in parameters)
                {
                    if (xmlDocument.GetElementsByTagName(parameter)[0] is not null)
                    {
                        xmlDocument.GetElementsByTagName(parameter)[0].InnerText = Obscure(xmlDocument.GetElementsByTagName(parameter)[0].InnerText, parameter);
                    }
                }
            }
            return ArrangedXml(xmlDocument);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception on xml value : '{Value}',\n Exception Detail: {Ex} ", Obscure(value, string.Empty), ex);
            return value;
        }
    }

    private string MaskJsonData(string value, string name, ConfidentialSettings confidentialSettings)
    {
        _configuration.GetSection(nameof(ConfidentialSettings)).Bind(confidentialSettings);
        JToken jsonValue;
        try
        {
            jsonValue = JToken.Parse(value);
        }
        catch (Exception ex)
        {
            _logger.LogError("Exception on parse json value : '{Value}',\n Exception Detail: {Ex} ", Obscure(value, string.Empty), ex);
            return value;
        }

        List<string> parameters;

        if (!string.IsNullOrEmpty(name))
        {
            parameters = (List<string>)confidentialSettings.GetType().GetProperty(name)?.GetValue(confidentialSettings);

            parameters ??= new List<string>();
        }
        else
        {
            parameters = confidentialSettings.ConfidentialKeywords;
        }

        ObscureMatchingValues(jsonValue, parameters);

        return jsonValue.ToString();
    }

    private string Obscure(string property, string path)
    {
        if (string.IsNullOrEmpty(property))
        {
            return property;
        }

        var settings = new ConfidentialSettings();
        _configuration.GetSection(nameof(ConfidentialSettings)).Bind(settings);

        if (settings.ConfidentialKeywords.Contains(path))
        {
            return "*****";
        }
        else
        {
            int length = property.Length;
            int leftLength = length > 4 ? 1 : 0;
            int rightLength = length > 6 ? Math.Min((length - 6) / 2, 4) : 0;

            return string.Concat(property.AsSpan(0, leftLength),
                                 new string('*', length - leftLength - rightLength),
                                 property.AsSpan(property.Length - rightLength));
        }
    }

    private void ObscureMatchingValues(JToken token, IEnumerable<string> jsonPaths)
    {
        foreach (string path in jsonPaths)
        {
            foreach (JToken match in token.SelectTokens(path))
            {
                match.Replace(new JValue(Obscure(match.ToString(),path)));
            }
        }
    }

    private string ArrangedXml(XmlDocument xmlDocument)
    {
        try
        {
            MemoryStream mStream = new MemoryStream();
            XmlTextWriter writer = new XmlTextWriter(mStream, Encoding.Unicode);
            writer.Formatting = Formatting.Indented;

            xmlDocument.WriteContentTo(writer);
            writer.Flush();
            mStream.Flush();
            mStream.Position = 0;

            StreamReader sReader = new StreamReader(mStream);
            string formattedXml = sReader.ReadToEnd();

            return formattedXml;
        }
        catch (Exception)
        {
            return xmlDocument.OuterXml;
        }
    }
}
