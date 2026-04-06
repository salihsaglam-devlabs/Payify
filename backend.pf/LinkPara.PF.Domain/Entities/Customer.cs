using System.ComponentModel.DataAnnotations.Schema;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class Customer : AuditEntity
{
    public CustomerStatus CustomerStatus { get; set; }
    public CompanyType CompanyType { get; set; }
    public string CommercialTitle { get; set; }
    public string TradeRegistrationNumber { get; set; }
    public string TaxAdministration { get; set; }
    public string TaxNumber { get; set; }
    public string MersisNumber { get; set; }
    public int Country { get; set; }
    public string CountryName { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
    public int District { get; set; }
    public string DistrictName { get; set; }
    public string PostalCode { get; set; }
    public string Address { get; set; }
    public Guid ContactPersonId { get; set; }
    public Guid CustomerId { get; set; }
    public int CustomerNumber { get; set; }
    public ContactPerson AuthorizedPerson { get; set; }
    public List<Merchant> Merchants { get; set; }
    public Guid ExternalCustomerId { get; set; }
}