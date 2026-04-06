using System.Xml.Serialization;

	[XmlRoot(ElementName="Results", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Base")]
	public class Results {
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName="Success", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Base")]
	public class Success {
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
		
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="InstitutionList", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
	public class InstitutionList {
		[XmlElement(ElementName="AdditionalInfoInputField", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string AdditionalInfoInputField { get; set; }
		
		[XmlElement(ElementName="AdditionalInfoInputFieldMaxLength", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string AdditionalInfoInputFieldMaxLength { get; set; }
		
		[XmlElement(ElementName="InstitutionId", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string InstitutionId { get; set; }
		
		[XmlElement(ElementName="InstitutionLongName", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string InstitutionLongName { get; set; }
		
		[XmlElement(ElementName="InstitutionShortName", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string InstitutionShortName { get; set; }
		
		[XmlElement(ElementName="InstitutionType", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string InstitutionType { get; set; }
		
		[XmlElement(ElementName="IsAllowsPartialPayment", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsAllowsPartialPayment { get; set; }
		
		[XmlElement(ElementName="IsAllowsPayment", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsAllowsPayment { get; set; }
		
		[XmlElement(ElementName="IsAllowsQuery", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsAllowsQuery { get; set; }
		
		[XmlElement(ElementName="IsOnline", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsOnline { get; set; }
		
		[XmlElement(ElementName="IsRequiredAdditionalInfoInputField", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsRequiredAdditionalInfoInputField { get; set; }
		
		[XmlElement(ElementName="IsSubscriberNoNumeric", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string IsSubscriberNoNumeric { get; set; }
		
		[XmlElement(ElementName="SubscriberLabel", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string SubscriberLabel { get; set; }
		
		[XmlElement(ElementName="SubscriberNoHelpText", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string SubscriberNoHelpText { get; set; }
		
		[XmlElement(ElementName="SubscriberNoMaxLength", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public string SubscriberNoMaxLength { get; set; }
	}

	[XmlRoot(ElementName="GetCorporationsResult", Namespace="http://boa.net/BOA.Integration.CollectionManagement/Service")]
	public class GetCorporationsResult {
		[XmlElement(ElementName="Results", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Base")]
		public Results Results { get; set; }
		
		[XmlElement(ElementName="Success", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Base")]
		public Success Success { get; set; }
		
		[XmlArray(ElementName="InstitutionList", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		[XmlArrayItem(ElementName="InstitutionList", Namespace="http://schemas.datacontract.org/2004/07/BOA.Integration.Model.CollectionManagement")]
		public List<InstitutionList> InstitutionList { get; set; }
		
		[XmlAttribute(AttributeName="a", Namespace="http://www.w3.org/2000/xmlns/")]
		public string A { get; set; }
		
		[XmlAttribute(AttributeName="i", Namespace="http://www.w3.org/2000/xmlns/")]
		public string I { get; set; }
	}

	[XmlRoot(ElementName="GetCorporationsResponse", Namespace="http://boa.net/BOA.Integration.CollectionManagement/Service")]
	public class GetCorporationsResponse {
		[XmlElement(ElementName="GetCorporationsResult", Namespace="http://boa.net/BOA.Integration.CollectionManagement/Service")]
		public GetCorporationsResult GetCorporationsResult { get; set; }
		
		[XmlAttribute(AttributeName="xmlns")]
		public string Xmlns { get; set; }
	}

	[XmlRoot(ElementName="Body", Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
	public class Body {
		[XmlElement(ElementName="GetCorporationsResponse", Namespace="http://boa.net/BOA.Integration.CollectionManagement/Service")]
		public GetCorporationsResponse GetCorporationsResponse { get; set; }
	}

	[XmlRoot(ElementName="Envelope", Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
	public class Envelope {
		[XmlElement(ElementName="Body", Namespace="http://schemas.xmlsoap.org/soap/envelope/")]
		public Body Body { get; set; }
		
		[XmlAttribute(AttributeName="s", Namespace="http://www.w3.org/2000/xmlns/")]
		public string S { get; set; }
	}
	