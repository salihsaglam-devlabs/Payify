using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.ApiGateway.Services.CustomerManagement.Models
{
    public class CustomerAddressDto
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public int CountryId { get; set; }
        public string Country { get; set; }
        public int CityId { get; set; }
        public string City { get; set; }
        public int DistrictId { get; set; }
        public string District { get; set; }
        public string PostalCode { get; set; }
        public string Address { get; set; }
    }
}
