using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

using HtmlToOpenXml;

namespace LinkPara.ApiGateway.Merchant.Utils;

public static class Word
{
    public static byte[] CreateWordDocument(string html)
    {
        using (var generatedDocument = new MemoryStream())
        {
            using (var package = WordprocessingDocument.Create(generatedDocument, WordprocessingDocumentType.Document))
            {
                var mainPart = package.MainDocumentPart;
                if (mainPart == null)
                {
                    mainPart = package.AddMainDocumentPart();
                    new Document(new Body()).Save(mainPart);
                }

                var converter = new HtmlConverter(mainPart);
                converter.ParseHtml(html);

                mainPart.Document.Save();
            }

            return generatedDocument.ToArray();
        }
    }
}
