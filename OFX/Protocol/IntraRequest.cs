namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class IntraRequest : AbstractIntraRequest {
        
        private TransferInfo xFERINFOField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public TransferInfo XFERINFO {
            get {
                return xFERINFOField;
            }
            set {
                xFERINFOField = value;
            }
        }
    }
}