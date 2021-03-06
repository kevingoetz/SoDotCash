namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class PayeeResponse : AbstractPayeeResponse {
        
        private Payee pAYEEField;
        
        private BankAccount bANKACCTTOField;
        
        private ExtendedPayee eXTDPAYEEField;
        
        private string[] pAYACCTField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public Payee PAYEE {
            get {
                return pAYEEField;
            }
            set {
                pAYEEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public BankAccount BANKACCTTO {
            get {
                return bANKACCTTOField;
            }
            set {
                bANKACCTTOField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ExtendedPayee EXTDPAYEE {
            get {
                return eXTDPAYEEField;
            }
            set {
                eXTDPAYEEField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("PAYACCT", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string[] PAYACCT {
            get {
                return pAYACCTField;
            }
            set {
                pAYACCTField = value;
            }
        }
    }
}