using System.Text.Json.Serialization;

namespace LinkPara.Emoney.Application.Commons.Models.PricingModels;


public class TodebDeclarationResponse
{
    [JsonPropertyName("ToplamIslemSayisi")]
    public int TotalRecordCount { get; set; }
    [JsonPropertyName("BildirimTarih")]
    public string DeclarationDate { get; set; }
    [JsonPropertyName("Durum")]
    public int Status { get; set; }
    [JsonPropertyName("Aciklama")]
    public string Explanation { get; set; }
    [JsonPropertyName("BasariliIslemler")]
    public List<DeclarationRecord> SuccessfulRecords { get; set; }
    [JsonPropertyName("HataliIslemler")]
    public List<DeclarationRecord> UnsuccessfulRecords { get; set; }
}

public class DeclarationRecord
{
    [JsonPropertyName("Durum")]
    public int Status { get; set; }
    [JsonPropertyName("Aciklama")]
    public string Explanation { get; set; }
    [JsonPropertyName("Tckn")]
    public string IdentityNumber { get; set; }
}