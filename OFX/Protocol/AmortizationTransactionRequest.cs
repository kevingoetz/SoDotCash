namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class AmortizationTransactionRequest : AbstractTransactionRequest {
        
        private AmortizationRequest aMRTSTMTRQField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AmortizationRequest AMRTSTMTRQ {
            get {
                return aMRTSTMTRQField;
            }
            set {
                aMRTSTMTRQField = value;
            }
        }
    }
}