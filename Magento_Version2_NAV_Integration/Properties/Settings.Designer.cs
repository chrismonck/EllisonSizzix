﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Magento_Version2_NAV_Integration.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "12.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd./Page/Customer")]
        public string Magento_Version2_NAV_Integration_Customer_service_Customer_Service {
            get {
                return ((string)(this["Magento_Version2_NAV_Integration_Customer_service_Customer_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd./Page/Item")]
        public string Magento_Version2_NAV_Integration_Item_Service_Item_Service {
            get {
                return ((string)(this["Magento_Version2_NAV_Integration_Item_Service_Item_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd./Page/ItemVariants")]
        public string Magento_Version2_NAV_Integration_ItemVariant_ItemVariants_Service {
            get {
                return ((string)(this["Magento_Version2_NAV_Integration_ItemVariant_ItemVariants_Service"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://192.168.10.12:7047/DynamicsNAV80/WS/Ellison%20Europe%20Ltd./Codeunit/Magen" +
            "toFunctions")]
        public string Magento_Version2_NAV_Integration_NavMagFunctions_MagentoFunctions {
            get {
                return ((string)(this["Magento_Version2_NAV_Integration_NavMagFunctions_MagentoFunctions"]));
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.SpecialSettingAttribute(global::System.Configuration.SpecialSetting.WebServiceUrl)]
        [global::System.Configuration.DefaultSettingValueAttribute("http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd./Page/SalesOrder")]
        public string Magento_Version2_NAV_Integration_SalesOrder_service_SalesOrder_Service {
            get {
                return ((string)(this["Magento_Version2_NAV_Integration_SalesOrder_service_SalesOrder_Service"]));
            }
        }
    }
}
