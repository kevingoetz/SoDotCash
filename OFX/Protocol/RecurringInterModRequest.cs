namespace OFX.Protocol
{
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.81.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute()]
    public partial class RecurringInterModRequest : AbstractRecurringInterRequest {
        
        private string rECSRVRTIDField;
        
        private RecurringInstructions rECURRINSTField;
        
        private InterRequest iNTERRQField;
        
        private BooleanType mODPENDINGField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string RECSRVRTID {
            get {
                return rECSRVRTIDField;
            }
            set {
                rECSRVRTIDField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public RecurringInstructions RECURRINST {
            get {
                return rECURRINSTField;
            }
            set {
                rECURRINSTField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public InterRequest INTERRQ {
            get {
                return iNTERRQField;
            }
            set {
                iNTERRQField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public BooleanType MODPENDING {
            get {
                return mODPENDINGField;
            }
            set {
                mODPENDINGField = value;
            }
        }
    }
}