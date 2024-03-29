﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by Microsoft.VSDesigner, Version 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace WebService1.PlataNeincasate {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    // CODEGEN: The optional WSDL extension element 'Policy' from namespace 'http://schemas.xmlsoap.org/ws/2004/09/policy' was not handled.
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="ZFG_INSTR_PLATA", Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZFG_INSTR_PLATA : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback ZlistaFacturiOperationCompleted;
        
        private System.Threading.SendOrPostCallback ZtbSaveDocCliringOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public ZFG_INSTR_PLATA() {
            this.Url = global::WebService1.Properties.Settings.Default.WebService1_PlataNeincasate_ZFG_INSTR_PLATA;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event ZlistaFacturiCompletedEventHandler ZlistaFacturiCompleted;
        
        /// <remarks/>
        public event ZtbSaveDocCliringCompletedEventHandler ZtbSaveDocCliringCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:sap-com:document:sap:soap:functions:mc-style:ZFG_INSTR_PLATA:ZlistaFacturiReq" +
            "uest", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("ZlistaFacturiResponse", Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
        public ZlistaFacturiResponse ZlistaFacturi([System.Xml.Serialization.XmlElementAttribute("ZlistaFacturi", Namespace="urn:sap-com:document:sap:soap:functions:mc-style")] ZlistaFacturi ZlistaFacturi1) {
            object[] results = this.Invoke("ZlistaFacturi", new object[] {
                        ZlistaFacturi1});
            return ((ZlistaFacturiResponse)(results[0]));
        }
        
        /// <remarks/>
        public void ZlistaFacturiAsync(ZlistaFacturi ZlistaFacturi1) {
            this.ZlistaFacturiAsync(ZlistaFacturi1, null);
        }
        
        /// <remarks/>
        public void ZlistaFacturiAsync(ZlistaFacturi ZlistaFacturi1, object userState) {
            if ((this.ZlistaFacturiOperationCompleted == null)) {
                this.ZlistaFacturiOperationCompleted = new System.Threading.SendOrPostCallback(this.OnZlistaFacturiOperationCompleted);
            }
            this.InvokeAsync("ZlistaFacturi", new object[] {
                        ZlistaFacturi1}, this.ZlistaFacturiOperationCompleted, userState);
        }
        
        private void OnZlistaFacturiOperationCompleted(object arg) {
            if ((this.ZlistaFacturiCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ZlistaFacturiCompleted(this, new ZlistaFacturiCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("urn:sap-com:document:sap:soap:functions:mc-style:ZFG_INSTR_PLATA:ZtbSaveDocClirin" +
            "gRequest", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Bare)]
        [return: System.Xml.Serialization.XmlElementAttribute("ZtbSaveDocCliringResponse", Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
        public ZtbSaveDocCliringResponse ZtbSaveDocCliring([System.Xml.Serialization.XmlElementAttribute("ZtbSaveDocCliring", Namespace="urn:sap-com:document:sap:soap:functions:mc-style")] ZtbSaveDocCliring ZtbSaveDocCliring1) {
            object[] results = this.Invoke("ZtbSaveDocCliring", new object[] {
                        ZtbSaveDocCliring1});
            return ((ZtbSaveDocCliringResponse)(results[0]));
        }
        
        /// <remarks/>
        public void ZtbSaveDocCliringAsync(ZtbSaveDocCliring ZtbSaveDocCliring1) {
            this.ZtbSaveDocCliringAsync(ZtbSaveDocCliring1, null);
        }
        
        /// <remarks/>
        public void ZtbSaveDocCliringAsync(ZtbSaveDocCliring ZtbSaveDocCliring1, object userState) {
            if ((this.ZtbSaveDocCliringOperationCompleted == null)) {
                this.ZtbSaveDocCliringOperationCompleted = new System.Threading.SendOrPostCallback(this.OnZtbSaveDocCliringOperationCompleted);
            }
            this.InvokeAsync("ZtbSaveDocCliring", new object[] {
                        ZtbSaveDocCliring1}, this.ZtbSaveDocCliringOperationCompleted, userState);
        }
        
        private void OnZtbSaveDocCliringOperationCompleted(object arg) {
            if ((this.ZtbSaveDocCliringCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.ZtbSaveDocCliringCompleted(this, new ZtbSaveDocCliringCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZlistaFacturi {
        
        private Zsneincasate[] itDocsField;
        
        private string piKunnrField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public Zsneincasate[] ItDocs {
            get {
                return this.itDocsField;
            }
            set {
                this.itDocsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PiKunnr {
            get {
                return this.piKunnrField;
            }
            set {
                this.piKunnrField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class Zsneincasate {
        
        private string xblnrField;
        
        private string belnrField;
        
        private string gjahrField;
        
        private decimal amountField;
        
        private string docdateField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Xblnr {
            get {
                return this.xblnrField;
            }
            set {
                this.xblnrField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Belnr {
            get {
                return this.belnrField;
            }
            set {
                this.belnrField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Gjahr {
            get {
                return this.gjahrField;
            }
            set {
                this.gjahrField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal Amount {
            get {
                return this.amountField;
            }
            set {
                this.amountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Docdate {
            get {
                return this.docdateField;
            }
            set {
                this.docdateField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZstDocsClearing {
        
        private string nrDocumentField;
        
        private string anDocumentField;
        
        private decimal sumaDocumentField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string NrDocument {
            get {
                return this.nrDocumentField;
            }
            set {
                this.nrDocumentField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string AnDocument {
            get {
                return this.anDocumentField;
            }
            set {
                this.anDocumentField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal SumaDocument {
            get {
                return this.sumaDocumentField;
            }
            set {
                this.sumaDocumentField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZfiBoCec {
        
        private string stareCambieField;
        
        private string clientField;
        
        private decimal sumaField;
        
        private string serieNumarField;
        
        private string dataScadentaField;
        
        private string dataEmitereField;
        
        private string codAgentField;
        
        private string sgtxtField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string StareCambie {
            get {
                return this.stareCambieField;
            }
            set {
                this.stareCambieField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Client {
            get {
                return this.clientField;
            }
            set {
                this.clientField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal Suma {
            get {
                return this.sumaField;
            }
            set {
                this.sumaField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string SerieNumar {
            get {
                return this.serieNumarField;
            }
            set {
                this.serieNumarField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DataScadenta {
            get {
                return this.dataScadentaField;
            }
            set {
                this.dataScadentaField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string DataEmitere {
            get {
                return this.dataEmitereField;
            }
            set {
                this.dataEmitereField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string CodAgent {
            get {
                return this.codAgentField;
            }
            set {
                this.codAgentField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Sgtxt {
            get {
                return this.sgtxtField;
            }
            set {
                this.sgtxtField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZlistaFacturiResponse {
        
        private Zsneincasate[] itDocsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public Zsneincasate[] ItDocs {
            get {
                return this.itDocsField;
            }
            set {
                this.itDocsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZtbSaveDocCliring {
        
        private ZfiBoCec isBoCecField;
        
        private ZstDocsClearing[] itDocsClearingField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ZfiBoCec IsBoCec {
            get {
                return this.isBoCecField;
            }
            set {
                this.isBoCecField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public ZstDocsClearing[] ItDocsClearing {
            get {
                return this.itDocsClearingField;
            }
            set {
                this.itDocsClearingField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Xml", "4.8.9037.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="urn:sap-com:document:sap:soap:functions:mc-style")]
    public partial class ZtbSaveDocCliringResponse {
        
        private string epMessageField;
        
        private string epSuccesField;
        
        private ZstDocsClearing[] etDocsPlataField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EpMessage {
            get {
                return this.epMessageField;
            }
            set {
                this.epMessageField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string EpSucces {
            get {
                return this.epSuccesField;
            }
            set {
                this.epSuccesField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form=System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("item", Form=System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable=false)]
        public ZstDocsClearing[] EtDocsPlata {
            get {
                return this.etDocsPlataField;
            }
            set {
                this.etDocsPlataField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    public delegate void ZlistaFacturiCompletedEventHandler(object sender, ZlistaFacturiCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ZlistaFacturiCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ZlistaFacturiCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ZlistaFacturiResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ZlistaFacturiResponse)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    public delegate void ZtbSaveDocCliringCompletedEventHandler(object sender, ZtbSaveDocCliringCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.8.9037.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class ZtbSaveDocCliringCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal ZtbSaveDocCliringCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public ZtbSaveDocCliringResponse Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((ZtbSaveDocCliringResponse)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591