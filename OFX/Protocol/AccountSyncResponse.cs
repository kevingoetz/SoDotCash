namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class AccountSyncResponse : AbstractSyncResponse {
        
        private AccountTransactionResponse[] aCCTTRNRSField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("ACCTTRNRS", Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public AccountTransactionResponse[] ACCTTRNRS {
            get {
                return aCCTTRNRSField;
            }
            set {
                aCCTTRNRSField = value;
            }
        }
    }
}