﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.36366
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Magento_Version2_NAV_Integration_Sizzix.MagentoAuth {
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.36366")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1")]
    public partial class GenericFault : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string traceField;
        
        private GenericFaultParameter[] parametersField;
        
        private WrappedError[] wrappedErrorsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string Trace {
            get {
                return this.traceField;
            }
            set {
                this.traceField = value;
                this.RaisePropertyChanged("Trace");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public GenericFaultParameter[] Parameters {
            get {
                return this.parametersField;
            }
            set {
                this.parametersField = value;
                this.RaisePropertyChanged("Parameters");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=2)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public WrappedError[] WrappedErrors {
            get {
                return this.wrappedErrorsField;
            }
            set {
                this.wrappedErrorsField = value;
                this.RaisePropertyChanged("WrappedErrors");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.36366")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1")]
    public partial class GenericFaultParameter : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string keyField;
        
        private string valueField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string key {
            get {
                return this.keyField;
            }
            set {
                this.keyField = value;
                this.RaisePropertyChanged("key");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        public string value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
                this.RaisePropertyChanged("value");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.36366")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1")]
    public partial class WrappedError : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string messageField;
        
        private GenericFaultParameter[] parametersField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string message {
            get {
                return this.messageField;
            }
            set {
                this.messageField = value;
                this.RaisePropertyChanged("message");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public GenericFaultParameter[] parameters {
            get {
                return this.parametersField;
            }
            set {
                this.parametersField = value;
                this.RaisePropertyChanged("parameters");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.36366")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1")]
    public partial class IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string resultField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string result {
            get {
                return this.resultField;
            }
            set {
                this.resultField = value;
                this.RaisePropertyChanged("result");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.0.30319.36366")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1")]
    public partial class IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest : object, System.ComponentModel.INotifyPropertyChanged {
        
        private string usernameField;
        
        private string passwordField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=0)]
        public string username {
            get {
                return this.usernameField;
            }
            set {
                this.usernameField = value;
                this.RaisePropertyChanged("username");
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified, Order=1)]
        public string password {
            get {
                return this.passwordField;
            }
            set {
                this.passwordField = value;
                this.RaisePropertyChanged("password");
            }
        }
        
        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
        
        protected void RaisePropertyChanged(string propertyName) {
            System.ComponentModel.PropertyChangedEventHandler propertyChanged = this.PropertyChanged;
            if ((propertyChanged != null)) {
                propertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1", ConfigurationName="MagentoAuth.integrationAdminTokenServiceV1PortType")]
    public interface integrationAdminTokenServiceV1PortType {
        
        // CODEGEN: Generating message contract since the operation integrationAdminTokenServiceV1CreateAdminAccessToken is neither RPC nor document wrapped.
        [System.ServiceModel.OperationContractAttribute(Action="integrationAdminTokenServiceV1CreateAdminAccessToken", ReplyAction="*")]
        [System.ServiceModel.FaultContractAttribute(typeof(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.GenericFault), Action="integrationAdminTokenServiceV1CreateAdminAccessToken", Name="GenericFault")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1 integrationAdminTokenServiceV1CreateAdminAccessToken(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 request);
        
        [System.ServiceModel.OperationContractAttribute(Action="integrationAdminTokenServiceV1CreateAdminAccessToken", ReplyAction="*")]
        System.Threading.Tasks.Task<Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1> integrationAdminTokenServiceV1CreateAdminAccessTokenAsync(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 request);
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1", Order=0)]
        public Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest integrationAdminTokenServiceV1CreateAdminAccessTokenRequest;
        
        public integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1() {
        }
        
        public integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest integrationAdminTokenServiceV1CreateAdminAccessTokenRequest) {
            this.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest = integrationAdminTokenServiceV1CreateAdminAccessTokenRequest;
        }
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
    [System.ServiceModel.MessageContractAttribute(IsWrapped=false)]
    public partial class integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1 {
        
        [System.ServiceModel.MessageBodyMemberAttribute(Namespace="https://www.sizzix.co.uk/soap/b2c_uk?services=integrationAdminTokenServiceV1", Order=0)]
        public Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse integrationAdminTokenServiceV1CreateAdminAccessTokenResponse;
        
        public integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1() {
        }
        
        public integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse integrationAdminTokenServiceV1CreateAdminAccessTokenResponse) {
            this.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse = integrationAdminTokenServiceV1CreateAdminAccessTokenResponse;
        }
    }
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface integrationAdminTokenServiceV1PortTypeChannel : Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType, System.ServiceModel.IClientChannel {
    }
    
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class integrationAdminTokenServiceV1PortTypeClient : System.ServiceModel.ClientBase<Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType>, Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType {
        
        public integrationAdminTokenServiceV1PortTypeClient() {
        }
        
        public integrationAdminTokenServiceV1PortTypeClient(string endpointConfigurationName) : 
                base(endpointConfigurationName) {
        }
        
        public integrationAdminTokenServiceV1PortTypeClient(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public integrationAdminTokenServiceV1PortTypeClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress) {
        }
        
        public integrationAdminTokenServiceV1PortTypeClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress) {
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1 Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType.integrationAdminTokenServiceV1CreateAdminAccessToken(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 request) {
            return base.Channel.integrationAdminTokenServiceV1CreateAdminAccessToken(request);
        }
        
        public Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse integrationAdminTokenServiceV1CreateAdminAccessToken(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest integrationAdminTokenServiceV1CreateAdminAccessTokenRequest) {
            Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 inValue = new Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1();
            inValue.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest = integrationAdminTokenServiceV1CreateAdminAccessTokenRequest;
            Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1 retVal = ((Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType)(this)).integrationAdminTokenServiceV1CreateAdminAccessToken(inValue);
            return retVal.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse;
        }
        
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Advanced)]
        System.Threading.Tasks.Task<Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1> Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType.integrationAdminTokenServiceV1CreateAdminAccessTokenAsync(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 request) {
            return base.Channel.integrationAdminTokenServiceV1CreateAdminAccessTokenAsync(request);
        }
        
        public System.Threading.Tasks.Task<Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenResponse1> integrationAdminTokenServiceV1CreateAdminAccessTokenAsync(Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest integrationAdminTokenServiceV1CreateAdminAccessTokenRequest) {
            Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1 inValue = new Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest1();
            inValue.integrationAdminTokenServiceV1CreateAdminAccessTokenRequest = integrationAdminTokenServiceV1CreateAdminAccessTokenRequest;
            return ((Magento_Version2_NAV_Integration_Sizzix.MagentoAuth.integrationAdminTokenServiceV1PortType)(this)).integrationAdminTokenServiceV1CreateAdminAccessTokenAsync(inValue);
        }
    }
}
