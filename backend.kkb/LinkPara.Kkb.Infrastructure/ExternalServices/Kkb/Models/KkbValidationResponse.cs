using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace LinkPara.Kkb.Infrastructure.ExternalServices.Kkb.Models;

public class KkbValidationResponse
{

    // NOTE: Generated code may require at least .NET Framework 4.5 or .NET Core/Standard 2.0.
    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://schemas.xmlsoap.org/soap/envelope/", IsNullable = false)]
    public partial class Envelope
    {

        private object headerField;

        private EnvelopeBody bodyField;

        /// <remarks/>
        public object Header
        {
            get
            {
                return this.headerField;
            }
            set
            {
                this.headerField = value;
            }
        }

        /// <remarks/>
        public EnvelopeBody Body
        {
            get
            {
                return this.bodyField;
            }
            set
            {
                this.bodyField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://schemas.xmlsoap.org/soap/envelope/")]
    public partial class EnvelopeBody
    {

        private yeniBsgIbanDogrulamaResponse yeniBsgIbanDogrulamaResponseField;
        private bsgIbanDogrulamaResponse bsgIbanDogrulamaResponseField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://wsv2.ers.kkb.com.tr/")]
        public yeniBsgIbanDogrulamaResponse yeniBsgIbanDogrulamaResponse
        {
            get
            {
                return this.yeniBsgIbanDogrulamaResponseField;
            }
            set
            {
                this.yeniBsgIbanDogrulamaResponseField = value;
            }
        }        

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "http://wsv2.ers.kkb.com.tr/")]
        public bsgIbanDogrulamaResponse bsgIbanDogrulamaResponse
        {
            get
            {
                return this.bsgIbanDogrulamaResponseField;
            }
            set
            {
                this.bsgIbanDogrulamaResponseField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wsv2.ers.kkb.com.tr/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://wsv2.ers.kkb.com.tr/", IsNullable = false)]
    public partial class bsgIbanDogrulamaResponse
    {

        private @return returnField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public @return @return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "http://wsv2.ers.kkb.com.tr/")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "http://wsv2.ers.kkb.com.tr/", IsNullable = false)]
    public partial class yeniBsgIbanDogrulamaResponse
    {

        private @return returnField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Namespace = "")]
        public @return @return
        {
            get
            {
                return this.returnField;
            }
            set
            {
                this.returnField = value;
            }
        }
    }

    /// <remarks/>
    [System.SerializableAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class @return
    {

        private string hataKoduField;

        private string hataMesajiField;

        private bool islemSonucuField;

        private bool ibanKimlikNoDogrulamaSonucuField;

        private int kalanToplamUrunAdediField;

        private string kullandirimYapilanPaketAdiField;

        private string hesapDovizCinsiField;

        private int kullanilmisToplamUrunAdediField;

        private int toplamUrunAdediField;

        /// <remarks/>
        public string hataKodu
        {
            get
            {
                return this.hataKoduField;
            }
            set
            {
                this.hataKoduField = value;
            }
        }

        /// <remarks/>
        public string hataMesaji
        {
            get
            {
                return this.hataMesajiField;
            }
            set
            {
                this.hataMesajiField = value;
            }
        }

        /// <remarks/>
        public bool islemSonucu
        {
            get
            {
                return this.islemSonucuField;
            }
            set
            {
                this.islemSonucuField = value;
            }
        }

        /// <remarks/>
        public bool ibanKimlikNoDogrulamaSonucu
        {
            get
            {
                return this.ibanKimlikNoDogrulamaSonucuField;
            }
            set
            {
                this.ibanKimlikNoDogrulamaSonucuField = value;
            }
        }

        /// <remarks/>
        public int kalanToplamUrunAdedi
        {
            get
            {
                return this.kalanToplamUrunAdediField;
            }
            set
            {
                this.kalanToplamUrunAdediField = value;
            }
        }

        /// <remarks/>
        public string kullandirimYapilanPaketAdi
        {
            get
            {
                return this.kullandirimYapilanPaketAdiField;
            }
            set
            {
                this.kullandirimYapilanPaketAdiField = value;
            }
        }

        /// <remarks/>
        public string hesapDovizCinsi
        {
            get
            {
                return this.hesapDovizCinsiField;
            }
            set
            {
                this.hesapDovizCinsiField = value;
            }
        }

        /// <remarks/>
        public int kullanilmisToplamUrunAdedi
        {
            get
            {
                return this.kullanilmisToplamUrunAdediField;
            }
            set
            {
                this.kullanilmisToplamUrunAdediField = value;
            }
        }

        /// <remarks/>
        public int toplamUrunAdedi
        {
            get
            {
                return this.toplamUrunAdediField;
            }
            set
            {
                this.toplamUrunAdediField = value;
            }
        }
    }


}