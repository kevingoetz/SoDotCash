namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class IntraSyncRequest : AbstractSyncRequest {
        
        private AbstractAccount item1Field;
        
        private IntraTransactionRequest[] iNTRATRNRQField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("BANKACCTFROM", typeof(BankAccount), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("CCACCTFROM", typeof(CreditCardAccount), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlElementAttribute("LOANACCTFROM", typeof(LoanAccount), Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AbstractAccount Item1 {
            get {
                return item1Field;
            }
            set {
                item1Field = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("INTRATRNRQ", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public IntraTransactionRequest[] INTRATRNRQ {
            get {
                return iNTRATRNRQField;
            }
            set {
                iNTRATRNRQField = value;
            }
        }
    }
}