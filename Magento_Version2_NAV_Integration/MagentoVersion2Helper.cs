using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Magento.RestApi;
using Magento_Version2_NAV_Integration.MagentoAuth;
using System.Xml.Serialization;
//using Magento_Version2_NAV_Integration.MagentoAuth;

//CM 24/10/16 - TO KEEP TRACK OF CHANGES, I'M PUTTING COMMENTS AT THE TOP TO MATCH NAV
//MOD001 AZ CM 241016 - change to the status of new orders, allowed be chosen in NAV
//MOD002 AZ CM 241016 - commented out the check to download only processing orders, now downloads any order of a given status
//MOD003 AZ CM 241016 - update orders to given status
namespace Magento_Version2_NAV_Integration
{
    public class MagentoVersion2Helper
    {
        public string strSessionID;
        public MagentoVersion2Helper(){ }

        public void getSessionID(string MagentoURL,string strUser, string strPass)
        {
            try { 
            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(tmebe);
            myBinding.Elements.Add(new HttpsTransportBindingElement());                       
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL +"services=integrationAdminTokenServiceV1");            
            integrationAdminTokenServiceV1PortTypeClient magAuthClient;
            using (magAuthClient = new integrationAdminTokenServiceV1PortTypeClient(myBinding,endPointAddress))
            {
                IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest toke = new IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest();
                toke.username = strUser; 
                toke.password = strPass;

                IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse authResp = magAuthClient.integrationAdminTokenServiceV1CreateAdminAccessToken(toke);
                strSessionID = authResp.result;
            }
            }
            catch (Exception e)
            {
                strSessionID = string.Empty; //to0 prevent the error being returned to NAV

            }

            //return strSessionID;

        }

        public void getSessionID_B2B(string MagentoURL, string strUser, string strPass)
        {
            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(tmebe);
            myBinding.Elements.Add(new HttpsTransportBindingElement());
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=integrationAdminTokenServiceV1");
            MagentoAuthB2B.integrationAdminTokenServiceV1PortTypeClient magAuthClient;
            using (magAuthClient = new MagentoAuthB2B.integrationAdminTokenServiceV1PortTypeClient(myBinding, endPointAddress))
            {
                MagentoAuthB2B.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest toke = new MagentoAuthB2B.IntegrationAdminTokenServiceV1CreateAdminAccessTokenRequest();
                toke.username = strUser;
                toke.password = strPass;

                MagentoAuthB2B.IntegrationAdminTokenServiceV1CreateAdminAccessTokenResponse authResp = magAuthClient.integrationAdminTokenServiceV1CreateAdminAccessToken(toke);                    
                strSessionID = authResp.result;
            }

            //return strSessionID;

        }


        /// <summary>
        /// method to fetch Pre-Orders from Magento and download into NAV as 'Open' orders
        /// </summary>
        /// <param name="strMagentoURL"></param>
        /// <param name="strUser"></param>
        /// <param name="strPass"></param>
        /// <param name="NAVWebURL"></param>
        /// <param name="strUsername"></param>
        /// <param name="strPassword"></param>
        /// <param name="strDomain"></param>
        public void downloadPreOrdersMagentoToNAV(string strMagentoURL,string strUser, string strPass,string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
             //connect to magento
            getSessionID_B2B(strMagentoURL, strUser, strPass); //this may change but for now point to the b2b
             
            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
                
            myBinding.Elements.Add(new HttpsTransportBindingElement());

            EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=quoteCartRepositoryV1");  
            
            //catalogProductRepositoryV1PortTypeClient
            
            MagentoQuoteCart_B2B.quoteCartRepositoryV1PortTypeClient client;

           // using (client = new MagentoQuoteCart_B2B.quoteCartRepositoryV1PortTypeClient(myBinding, endPointAddress))//(
            using (client = new MagentoQuoteCart_B2B.quoteCartRepositoryV1PortTypeClient())
            {
                BasicHttpsBinding b = new BasicHttpsBinding();
                b.MaxReceivedMessageSize = 9999999;
                client.Endpoint.Binding = b;                      
                HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                try
                {
                    MagentoQuoteCart_B2B.QuoteCartRepositoryV1GetListRequest _PreOrdersCriteria = new MagentoQuoteCart_B2B.QuoteCartRepositoryV1GetListRequest();                    
                    MagentoQuoteCart_B2B.FrameworkSearchCriteria _PreOrderSearch = new MagentoQuoteCart_B2B.FrameworkSearchCriteria();
                    MagentoQuoteCart_B2B.FrameworkSearchFilterGroup _PreOrderFilterGroup = new MagentoQuoteCart_B2B.FrameworkSearchFilterGroup();
                    MagentoQuoteCart_B2B.FrameworkFilter _PreOrderFilterisPreOrder = new MagentoQuoteCart_B2B.FrameworkFilter();
                    MagentoQuoteCart_B2B.FrameworkFilter _PreOrderFilterPreOrderStatus = new MagentoQuoteCart_B2B.FrameworkFilter();

                    MagentoQuoteCart_B2B.FrameworkFilter[] lstFilters = new MagentoQuoteCart_B2B.FrameworkFilter[2];

                    _PreOrderFilterisPreOrder.field = "is_preorder";
                    _PreOrderFilterisPreOrder.value = "1";
                    lstFilters[0] = _PreOrderFilterisPreOrder;

                    _PreOrderFilterPreOrderStatus.field = "preorder_status";
                    _PreOrderFilterPreOrderStatus.value = "pending";
                    lstFilters[1] = _PreOrderFilterPreOrderStatus;

                    _PreOrderFilterGroup.filters = lstFilters;

                    MagentoQuoteCart_B2B.FrameworkSearchFilterGroup[] lstFilterGroup = new MagentoQuoteCart_B2B.FrameworkSearchFilterGroup[1];
                    lstFilterGroup[0] = _PreOrderFilterGroup;

                    _PreOrderSearch.filterGroups = lstFilterGroup;

                    _PreOrdersCriteria.searchCriteria = _PreOrderSearch;
                    _PreOrdersCriteria.searchCriteria.pageSizeSpecified = true;
                    _PreOrdersCriteria.searchCriteria.pageSize = 50;
                    //for now don't filter just download all customers
                    MagentoQuoteCart_B2B.QuoteCartRepositoryV1GetListResponse _PreOrderResponses = client.quoteCartRepositoryV1GetList(_PreOrdersCriteria);
                    foreach (MagentoQuoteCart_B2B.QuoteDataCartInterface _PreOrderData in _PreOrderResponses.result.items)
                    {
                        //need to create a SO in the same way we create a SO for download orders
                        


                        /*
                        MagentoQuoteCart_B2B.SalesDataShippingAssignmentInterface[] salesData = MagentoQuoteCart_B2B.extensionAttributes.shippingAssignments;

                        MagentoSales.SalesDataOrderAddressInterface _ShippingAddress = new MagentoSales.SalesDataOrderAddressInterface();
                        if (salesData.Length > 0)
                        {
                            _ShippingAddress = salesData[0].shipping.address;
                        }
                        */                               

                    }
                    
                }catch (Exception e){

                }
            }

        }

        public MagentoProductSR.CatalogDataProductInterface[] ResyncProdIDsToNAVToolStripMenuItem(string strMagentoURL,string strUser, string strPass, ref int intCount)
        {
            try
            {
                getSessionID(strMagentoURL,strUser, strPass);
                BasicHttpsBinding myBinding = new BasicHttpsBinding();
                MagentoProductSR.catalogProductRepositoryV1PortTypeClient client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient();
                using (client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient())
                {

                    client.Endpoint.Binding = new BasicHttpsBinding();
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

                    MagentoProductSR.CatalogProductRepositoryV1GetListRequest r = new MagentoProductSR.CatalogProductRepositoryV1GetListRequest();
                    r.searchCriteria = new MagentoProductSR.FrameworkSearchCriteriaInterface();
                    //r.searchCriteria.pageSize = 50;
                    //MagentoProductSR.CatalogProductRepositoryV1GetRequest req = new MagentoProductSR.CatalogProductRepositoryV1GetRequest();
                    //req.sku = "Deer Scarf";
                    //MagentoProductSR.CatalogProductRepositoryV1GetResponse resp = client.catalogProductRepositoryV1Get(req);
                    MagentoProductSR.CatalogProductRepositoryV1GetListResponse _LstProducts = client.catalogProductRepositoryV1GetList(r);
                    MagentoProductSR.CatalogDataProductSearchResultsInterface _Products = _LstProducts.result;
                    return _Products.items;
                }
            }
            catch (Exception exc)
            {
                MagentoProductSR.CatalogDataProductInterface[] lstCatalogData = null;
                intCount = 0;
                return lstCatalogData;
            }
          }


        public MagentoProductSR.CatalogDataProductInterface getItem(MagentoProductSR.CatalogDataProductInterface[] lstItems, int itemNo)
        {
            //MagentoAPIv2.catalogProductEntity item = new MagentoAPIv2.catalogProductEntity();
            MagentoProductSR.CatalogDataProductInterface item = new MagentoProductSR.CatalogDataProductInterface();
            try
            {
                item = lstItems[itemNo];
                return item;
            }
            catch (Exception exc)
            {
                return item;
            }

        }



        public void UploadItems(string strMagentoURL,string strUser, string strPass,string MagentoURL, string ItemNo, string ItemDescription,
             decimal ItemUnitPrice, string ItemLastDateModified, string ItemMetaDescription, string ItemMetaTitle, string ItemMetaKeywords,
             decimal ItemNetWeight, string ItemeCommerceID, string ItemVATProdPostingGroup, string ItemEcommerceProductType, decimal decWeight,
             ref int intEcommerceID, ref string strError)
        {
            try
            {
                getSessionID(strMagentoURL,strUser, strPass);
                // Set the Magento API Endpoint
                MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;
                CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(tmebe);
            myBinding.Elements.Add(new HttpsTransportBindingElement());
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=catalogProductRepositoryV1");                        
            using (client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient(myBinding, endPointAddress))
            {

                    client.Endpoint.Binding = new BasicHttpsBinding();
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

                    //Define the new Magento Product Record
                    MagentoProductSR.CatalogProductRepositoryV1SaveRequest SaveProductRecord = new MagentoProductSR.CatalogProductRepositoryV1SaveRequest();
                    MagentoProductSR.CatalogDataProductInterface ProductRecord = new MagentoProductSR.CatalogDataProductInterface();
                    string ProductSKU, ProductID, ProductType, ProductAttrSet, Store;
                    ProductType = "simple";
                    ProductAttrSet = "4";
                    Store = "";

                    ProductID = ItemeCommerceID;
                    //MISSING FIELDS IN MAGENTO 2 +                    
                    //ProductRecord.meta_description = ItemMetaDescription; //This is 255 chars                
                    //ProductRecord.meta_keyword = ItemMetaKeywords;
                    //ProductRecord.meta_title = ItemMetaTitle;
                    //MISSING FIELDS IN MAGENTO 2 -

                    ProductRecord.name = ItemDescription;
                    ProductRecord.price = float.Parse(ItemUnitPrice.ToString());  //deciaml???
                    //If product ID is empty then we are creating a record so put in a description
                    if (string.IsNullOrEmpty(ProductID))
                    {
                        // ProductRecord.description = ItemDescription;
                        ProductRecord.name = ItemDescription; //swopped as a best fit
                        //ProductRecord.short_description = ItemDescription; //no matching field
                    }

                    // v2.01f If it is not a template item, then mark it as a simple product
                    if (ItemEcommerceProductType == "Configurable")
                    {

                        ProductType = "configurable";
                    }


                    ProductRecord.weight = float.Parse(ItemNetWeight.ToString());
                    ProductSKU = ItemNo;
                    ProductRecord.status = 1;
                    ProductRecord.visibility = 4; //4 = Catalog,Search

                    //V2.02c Original VAT treatment didn't work in Magento 1.8.1.0.   Values are 0=None, 1=Invalid, 2=Taxable, 3=Invalid
                    //Changed so that if the VAT Prod Posting group is "NO VAT" then it will set to None.  Otherwise it will
                    //set to taxable
                    //ProductRecord.tax_class_id = "2";  //2=Taxable   // NO MATCHING FIELD  
                    // NO MATCHING FIELD
                    /*
                if (ItemVATProdPostingGroup == "NO VAT")
                {
                    ProductRecord.tax_class_id = "0";  //0=None
                }
                    */
                    ProductRecord.weight = float.Parse(decWeight.ToString());
                    int RecordsUpdated = 0;
                    int RecordsFailed = 0;
                    int RecordsInserted = 0;
                    intEcommerceID = 0;
                    strError = string.Empty;
                    // If NAV Item already has a MagentoID then try to update the record.  
                    // If it doesn't then create a new record and store the ID in NAV
                    //Update the record
                    try
                    {
                        SaveProductRecord.product = ProductRecord;
                        MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                        MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                        intEcommerceID = returnedItem.id;
                        //.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                        RecordsUpdated = RecordsUpdated + 1;
                    }
                    catch (Exception e)
                    {
                        //If it fails, try again because Magento often returns a comms error
                        try
                        {
                            //magev2.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                            SaveProductRecord.product = ProductRecord;                            
                            MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                            MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                            intEcommerceID = returnedItem.id;

                            RecordsUpdated = RecordsUpdated + 1;
                        }
                        catch (Exception ex2)
                        {
                            RecordsFailed = RecordsFailed + 1;
                            strError = strError + "Failed: item no: " + ProductID + " ";
                        }
                    }
                }         
            }
            catch (Exception overallE)
            {
                strError = strError + "issue with connection";
            }
       
        }
        
        public bool UpdateItem(string MagentoURL,string strUser, string strPass, string ItemNo, string intEcommerceID, string strDescription, decimal decWeight)
        {
            try
            {

                getSessionID(MagentoURL,strUser, strPass);
                // Set the Magento API Endpoint                
                CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(tmebe);
            myBinding.Elements.Add(new HttpsTransportBindingElement());
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=catalogProductRepositoryV1");  
            MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;      
            using (client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient(myBinding, endPointAddress))
            {

                    client.Endpoint.Binding = new BasicHttpsBinding();
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;                                            
                    //Define the new Magento Product Record
                    MagentoProductSR.CatalogProductRepositoryV1SaveRequest SaveProductRecord = new MagentoProductSR.CatalogProductRepositoryV1SaveRequest();
                    MagentoProductSR.CatalogDataProductInterface ProductRecord = new MagentoProductSR.CatalogDataProductInterface();
                    string ProductSKU, ProductID, ProductType, ProductAttrSet, Store;
                  

                    ProductID = intEcommerceID;                  
                    ProductRecord.name = strDescription;
                  

                    ProductRecord.weight = float.Parse(decWeight.ToString());
                    ProductSKU = ItemNo;
                    ProductRecord.status = 1;
                    ProductRecord.visibility = 4; //4 = Catalog,Search

                    
                    ProductRecord.weight = float.Parse(decWeight.ToString());
                    int RecordsUpdated = 0;
                    int RecordsFailed = 0;
                    int RecordsInserted = 0;                                        
                    // If NAV Item already has a MagentoID then try to update the record.  
                    // If it doesn't then create a new record and store the ID in NAV
                    //Update the record
                    try
                    {
                        SaveProductRecord.product = ProductRecord;
                        MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                        MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                        intEcommerceID = returnedItem.id.ToString();
                        //.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                        RecordsUpdated = RecordsUpdated + 1;
                    }
                    catch (Exception e)
                    {
                        //If it fails, try again because Magento often returns a comms error
                        try
                        {
                            //magev2.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                            SaveProductRecord.product = ProductRecord;                            
                            MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                            MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                            intEcommerceID = returnedItem.id.ToString();

                            RecordsUpdated = RecordsUpdated + 1;
                        }
                        catch (Exception ex2)
                        {
                            RecordsFailed = RecordsFailed + 1;                            
                        }
                    }                                                                   

                return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public bool downloadCustomers(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try 
            {
                //connect to magento
               getSessionID(strMagentoURL,strUser, strPass);
                // Set the Magento API Endpoint                
                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;
                
                myBinding.Elements.Add(new HttpsTransportBindingElement());
                
                EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=customerCustomerRepositoryV1,customerAddressRepositoryV1");  
                //catalogProductRepositoryV1PortTypeClient
                MagentoCustomer.customerCustomerRepositoryV1PortTypeClient client;
                
                using (client = new MagentoCustomer.customerCustomerRepositoryV1PortTypeClient(myBinding, endPointAddress))//(
                {
                    BasicHttpsBinding b = new BasicHttpsBinding();
                    b.MaxReceivedMessageSize = 9999999;
                    client.Endpoint.Binding = b;
                        //new BasicHttpsBinding();                        
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                    try
                    {
                        //_CustFilter.field = "ID";
                        //_CustFilter.conditionType = ">";
                        //_CustFilter.value = "0";

                        MagentoCustomer.CustomerCustomerRepositoryV1GetListRequest _CustomerCriteria = new MagentoCustomer.CustomerCustomerRepositoryV1GetListRequest();
                        MagentoCustomer.FrameworkSearchCriteriaInterface _CustSearch = new MagentoCustomer.FrameworkSearchCriteriaInterface();
                        MagentoCustomer.FrameworkSearchFilterGroup _CustFilterGroup = new MagentoCustomer.FrameworkSearchFilterGroup();
                        MagentoCustomer.FrameworkFilter _CustFilter = new MagentoCustomer.FrameworkFilter();
                       
                        MagentoCustomer.FrameworkFilter[] lstFilters = new MagentoCustomer.FrameworkFilter[1];
                       // lstFilters[0] = _CustFilter;
                        _CustFilterGroup.filters = lstFilters;
                        MagentoCustomer.FrameworkSearchFilterGroup[] lstFilterGroup = new MagentoCustomer.FrameworkSearchFilterGroup[1];
                        lstFilterGroup[0] = _CustFilterGroup;
                        
                        _CustSearch.filterGroups = lstFilterGroup;

                        _CustomerCriteria.searchCriteria = _CustSearch;
                        _CustomerCriteria.searchCriteria.pageSizeSpecified = true;                        
                        _CustomerCriteria.searchCriteria.pageSize = 1;
                        //for now don't filter just download all customers
                        MagentoCustomer.CustomerCustomerRepositoryV1GetListResponse _CustomerResponses = client.customerCustomerRepositoryV1GetList(_CustomerCriteria);
                       // MagentoCustomer.CustomerDataCustomerInterface _CustomerData = new MagentoCustomer.CustomerDataCustomerInterface();
                        foreach (MagentoCustomer.CustomerDataCustomerInterface _CustomerData in _CustomerResponses.result.items)
                        {
                            //_CustomerData.id
                            //Create Service Reference for customer
                            Customer_service.Customer_Service CustService = new Customer_service.Customer_Service();
                            CustService.Url = NAVWebURL + "/Page/Customer";
                            CustService.UseDefaultCredentials = true;
                            NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
                            CustService.Credentials = netCred;
                            Customer_service.Customer_Filter _CustNAVFilter = new Customer_service.Customer_Filter();
                            _CustNAVFilter.Field = Customer_service.Customer_Fields.eCommerceID;
                            _CustNAVFilter.Criteria = _CustomerData.id.ToString();

                            Customer_service.Customer_Filter[] lstCustomerFilters = new Customer_service.Customer_Filter[] { _CustNAVFilter };
                            Customer_service.Customer[] _Customer = CustService.ReadMultiple(lstCustomerFilters, string.Empty, 1);
                            bool bUpdate = false;
                            Customer_service.Customer _NewCust;
                            if (_Customer.Length >= 1)
                            { //can only ever be 1 or 0 in lenth
                                //update
                                bUpdate = true;
                                _NewCust = _Customer[0];
                            }
                            else
                            {
                                _NewCust = new Customer_service.Customer();
                            }

                            if (_CustomerData.addresses.Length >= 1)
                            {
                                _NewCust.Address = _CustomerData.addresses[0].street[0].ToString();
                                _NewCust.City = _CustomerData.addresses[0].city;
                                _NewCust.County = _CustomerData.addresses[0].region.region.ToString();
                                _NewCust.Country_Region_Code = _CustomerData.addresses[0].countryId; //THIS WILL NEED CHECKING
                                _NewCust.Post_Code = _CustomerData.addresses[0].postcode;
                                _NewCust.Phone_No = _CustomerData.addresses[0].telephone;
                            }

                            if ((_CustomerData.firstname + " " + _CustomerData.lastname).Length > 50)
                            {
                                _NewCust.Name = (_CustomerData.firstname + " " + _CustomerData.lastname).Substring(0, 50);
                            }
                            else
                            {
                                _NewCust.Name = _CustomerData.firstname + " " + _CustomerData.lastname;
                            }

                            _NewCust.E_Mail = _CustomerData.email;
                            _NewCust.eCommerce_Enabled = true;
                            _NewCust.eCommerce_EnabledSpecified = true;
                            _NewCust.eCommerceID = _CustomerData.id.ToString();
                            
                            if (bUpdate){
                                CustService.Update(ref _NewCust);                                                      
                            }else{
                                CustService.Create(ref _NewCust);
                                //this will update _NewCust with NAV NO
                                //update Magento
                            }
                            
                        }
                       
                    }catch(Exception eFailedToSave){
                        return false;
                    }

                }


            }
            catch (Exception e)
            {
                return false;
            }

            return true;
        }


       /* 
       public void UploadCustomers(string MagentoURL,string strUser, string strPass,
           string CustomerNo, string CustomerName, string CustomerAddress, string CustomerAddress2, string CustomerCity,
           string CustomerContact, string CustomerPhoneNo, string CustomerPostCode, string CustomerCounty,
           string CustomerE_Mail, string CustomeFaxNo, string CustomerCountry, string CustomereCommerceID, string CustomereCommerceAddressID,
           ref int intEcommerceID, ref int intEcommerceAddressID, ref string strError)
       {
           try
           {


               getSessionID(MagentoURL, strUser, strPass);
                // Set the Magento API Endpoint
                MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;
                CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(tmebe);
            myBinding.Elements.Add(new HttpsTransportBindingElement());
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=catalogProductRepositoryV1");
            using (client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient(myBinding, endPointAddress))
            {
                client.Endpoint.Binding = new BasicHttpsBinding();
                //BasicHttpBinding();
                HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
            }

               // Set the Magento API Endpoint
               BasicHttpBinding myBinding = new BasicHttpBinding();
               myBinding.Name = "BasicHttpBinding_IService1";
               myBinding.Security.Mode = BasicHttpSecurityMode.None;
               myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
               myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
               myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
               myBinding.MaxReceivedMessageSize = 999999;
               System.TimeSpan timeout = new TimeSpan(0, 5, 0);
               myBinding.SendTimeout = timeout;
               EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
               //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
               MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

               // Define the new Magento Customer Record   
               // StoreID for Moon is 2 but uses default if not specified.
               MagentoAPIv2.customerCustomerEntityToCreate CustomerRecord = new MagentoAPIv2.customerCustomerEntityToCreate();
               MagentoAPIv2.customerAddressEntityCreate CustomerAddressRecord = new MagentoAPIv2.customerAddressEntityCreate();

               string CustomerID, CustomerAddressID;
               int X;
               string NewPassword;
               int RecordsInserted = 0;
               int RecordsUpdated = 0;
               int RecordsFailed = 0;

               //Populate the new Magento Record 
               if ((CustomereCommerceID != "") && !(string.IsNullOrEmpty(CustomereCommerceAddressID)))
               {
                   CustomerID = CustomereCommerceID;
               }
               else
               {
                   CustomerID = string.Empty;
               }

               CustomerAddressID = CustomereCommerceAddressID; //CustomerAddressID = Val(sdr.GetString(13))
               CustomerRecord.email = CustomerE_Mail; //email - can't be a duplicate otherwise createcustomer fails!!

               string[] lstName = CustomerContact.Split(new Char[] { ' ' });
               if (lstName.Length > 1)
               {
                   CustomerRecord.firstname = lstName[0];
                   CustomerRecord.lastname = lstName[1];
               }
               else
               {
                   CustomerRecord.firstname = CustomerContact;
               }


               //check the value in Magento to assig the right value at 'Associate to Website' by exporting a customer in csv
               //MS 03/09/10 commented by now because it causes error
               //           CustomerRecord.store_idSpecified = true;
               //           CustomerRecord.store_id = 1;
               //           CustomerRecord.website_idSpecified = true;
               //           CustomerRecord.website_id = 1;


               // V1.03d Replace old password details with random generated 10 character password
               if (string.IsNullOrEmpty(CustomerID))
               {
                   //New code to generate a random password
                   NewPassword = "";
                   Random ran = new Random();
                   for (int i = 1; i <= 2; i++)
                   {
                       NewPassword = NewPassword + (ran.Next(1, 9999) * 10).ToString();
                   }
                   CustomerRecord.password = "password"; //NewPassword;
               }

               //CustomerRecord.store_id = 2
               //CustomerRecord.website_id = 
               CustomerAddressRecord.city = CustomerCity;
               CustomerAddressRecord.company = CustomerName;
               CustomerAddressRecord.country_id = CustomerCountry;   //Country code
               CustomerAddressRecord.fax = CustomeFaxNo;
               CustomerAddressRecord.firstname = CustomerRecord.firstname;
               CustomerAddressRecord.is_default_billing = true;
               CustomerAddressRecord.is_default_billingSpecified = true;
               CustomerAddressRecord.is_default_shipping = true;
               CustomerAddressRecord.is_default_shippingSpecified = true;
               CustomerAddressRecord.lastname = CustomerRecord.lastname;
               CustomerAddressRecord.postcode = CustomerPostCode;
               CustomerAddressRecord.region = CustomerCounty;
               string[] lstStreet = new string[2];
               lstStreet[0] = CustomerAddress;
               lstStreet[1] = CustomerAddress2;
               CustomerAddressRecord.street = lstStreet;
               CustomerAddressRecord.telephone = CustomerPhoneNo;

               if (CustomerAddressRecord.telephone == "")
               {
                   CustomerAddressRecord.telephone = ".";
               }

               int intCustomerID;
               if (!Int32.TryParse(CustomerID, out intCustomerID))
               {

                   intCustomerID = 0;
               }
               else
               {
                   Int32.TryParse(CustomerID, out intCustomerID);
               }
               //CUSTOMER RECORD
               if (!string.IsNullOrEmpty(CustomerID))
               {
                   //Update CUSTOMER
                   try
                   {   //magev2.customerCustomerUpdate(strSessionID, CustomerID, CustomerRecord);

                       magev2.customerCustomerUpdate(strSessionID, intCustomerID, CustomerRecord);
                       RecordsUpdated = RecordsUpdated + 1;
                   }
                   catch (Exception e)
                   {
                       // If it fails, try again because Magento often returns a comms error
                       try
                       {
                           magev2.customerCustomerUpdate(strSessionID, intCustomerID, CustomerRecord);
                           RecordsUpdated = RecordsUpdated + 1;
                       }
                       catch (Exception ex2)
                       {
                           RecordsFailed = RecordsFailed + 1;
                           strError = strError + "Failed: customer no: " + CustomerID + " ";
                       }
                   }
               }
               else
               {
                   // insert the record and update the NAV customer with the ID
                   try
                   {
                       //CustomerCreateResponse = magev2.customerCustomerCreate(CustomerCreateRequest)
                       intEcommerceID = magev2.customerCustomerCreate(strSessionID, CustomerRecord);
                       RecordsInserted = RecordsInserted + 1;
                       intCustomerID = intEcommerceID;
                   }
                   catch (Exception ex)
                   {
                       // Magento API often returns a communication error so have a 2nd try if it fails
                       try
                       {
                           intEcommerceID = magev2.customerCustomerCreate(strSessionID, CustomerRecord);
                           RecordsInserted = RecordsInserted + 1;
                           intCustomerID = intEcommerceID;
                       }
                       catch (Exception exc2)
                       {
                           //if it fails, try to update the record
                           RecordsFailed = RecordsFailed + 1;
                           strError = strError + " Failed: customer no: " + CustomerID + " Error: " + exc2.Message.ToString();//"Failed: customer no: " + CustomerID + " ";
                       }
                   }
               } //end else

               //CUSTOMER ADDRESS RECORD  The Customer address ID is already set then update the record
               if (!string.IsNullOrEmpty(CustomerAddressID))
               {
                   int intCustomerAddressID;
                   if (Int32.TryParse(CustomerAddressID, out intCustomerAddressID))
                   {

                       intCustomerAddressID = 0;
                   }
                   else
                   {
                       Int32.TryParse(CustomerAddressID, out intCustomerAddressID);
                   }

                   //Update Customer address ID
                   try
                   {
                       magev2.customerAddressUpdate(strSessionID, intCustomerAddressID, CustomerAddressRecord);
                   }
                   catch (Exception e)
                   {
                       // If it fails, try again because Magento often returns a comms error
                       try
                       {
                           magev2.customerAddressUpdate(strSessionID, intCustomerAddressID, CustomerAddressRecord);
                       }
                       catch (Exception ex2)
                       {
                           strError = strError + " Failed: customer no: " + CustomerID + " Error with address ID: " + CustomerAddressID + " ";
                       }
                   }
               }
               else
               {   //Create a new record and update the NAV Customer Record with the ID
                   try
                   {
                       intEcommerceAddressID = magev2.customerAddressCreate(strSessionID, intCustomerID, CustomerAddressRecord);
                   }
                   catch (Exception ex)
                   {
                       // Magento API often returns a communication error so have a 2nd try if it fails
                       try
                       {
                           intEcommerceAddressID = magev2.customerAddressCreate(strSessionID, intCustomerID, CustomerAddressRecord);
                       }
                       catch (Exception exc2)
                       {
                           // if it fails, try to update the record
                           strError = strError + " Failed: customer no: " + CustomerID + " Error with address: " + exc2.Message.ToString(); //"Failed: customer no: " +  CustomerID + " ";
                       }
                   }
               } //end else

           }
           catch (Exception overallE)
           {
               //strError = strError + "issue with connection";
               strError = strError + overallE.Message.ToString();
           }


       }
        */
     
       public string DownloadOrders(string strMagentoURL,string strUser, string strPass,string NAVWebURL, bool DownloadProcessingStatusOrders, string NAVCusAcc, string WHSupplyLocationCode, string strDefaultCustomer, string DiscountGLAccount, string strUsername, string strPassword, string strDomain, bool bUpdateMagentoOrders, string strNewOrders, string strInProgressOrders, bool bLog)
       {         
          // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
           MagentoSales.salesOrderRepositoryV1PortTypeClient client;
           getSessionID(strMagentoURL,strUser, strPass);
                //MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

           CustomBinding myBinding = new CustomBinding();
           myBinding.Name = "CustomBinding2";
           
           
           TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
           tmebe.MessageVersion = MessageVersion.Soap12;
           myBinding.Elements.Add(tmebe);
          // myBinding.Elements.Add(new HttpsTransportBindingElement());

           HttpsTransportBindingElement httpBindingElement = new HttpsTransportBindingElement();
           httpBindingElement.MaxBufferSize = Int32.MaxValue;
           httpBindingElement.MaxReceivedMessageSize = Int32.MaxValue;
           myBinding.Elements.Add(httpBindingElement);
           EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1,salesShipmentCommentRepositoryV1");

           if (bLog)
           {
               Log.WriteInfo("Began Magento Download Check...");
               Log.WriteInfo(System.DateTime.Today.ToShortDateString() + " " + System.DateTime.Now.ToString("h:mm:ss tt"));
           }
           using (client = new MagentoSales.salesOrderRepositoryV1PortTypeClient(myBinding, endPointAddress))
                {

                   // client.Endpoint.Binding = new BasicHttpsBinding();                    
                    System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                    myBinding.SendTimeout = timeout;
                   
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                    client.Endpoint.Address = endPointAddress;
                    
                    
                        //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                    
                

           //----------------------- Create new orders for pending items in Magento ---------------------------------------

           // 2.02b Define a tax rate %
           int tax_rate = 0;

           //v2.01f restrict the filter to orders with a status of Pending
           MagentoSales.SalesOrderRepositoryV1GetListRequest SalesOrderRequest = new MagentoSales.SalesOrderRepositoryV1GetListRequest();
           MagentoSales.FrameworkSearchCriteria SOSearch = new MagentoSales.FrameworkSearchCriteria();
           MagentoSales.FrameworkSearchFilterGroup SOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup();
           MagentoSales.FrameworkFilter SOFilter = new MagentoSales.FrameworkFilter();


           SOFilter.field = "status";
           SOFilter.value = strNewOrders;//strNewOrders;
          // SOFilter.field = "Increment_Id";
           //SOFilter.value = "000411704";// "000401994";



           MagentoSales.FrameworkFilter[] lstFilter = new MagentoSales.FrameworkFilter[1];
           lstFilter[0] = SOFilter;
           SOFilterGroup.filters = lstFilter;
           MagentoSales.FrameworkSearchFilterGroup[] lstSOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup[1];
           lstSOFilterGroup[0] = SOFilterGroup;
           SOSearch.filterGroups = lstSOFilterGroup;            
           SOSearch.pageSize = 20;
           SOSearch.pageSizeSpecified = true;
           
           SalesOrderRequest.searchCriteria = SOSearch;
           
               //new MagentoSales.FrameworkSearchCriteria();
           MagentoSales.SalesDataShippingExtensionInterface salesShip = new MagentoSales.SalesDataShippingExtensionInterface();

  

           var stringwriter = new System.IO.StringWriter();
           var serializer = new XmlSerializer(SalesOrderRequest.GetType());
           serializer.Serialize(stringwriter, SalesOrderRequest);
           string strTestXML = stringwriter.ToString();
                            
           MagentoSales.SalesOrderRepositoryV1GetListResponse SOResponses = client.salesOrderRepositoryV1GetList(SalesOrderRequest);
           //MagentoSales.SalesDataOrderInterface[] soe = client.salesOrderRepositoryV1GetList(SalesOrderRequest).result.items;
           
           
           NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
           NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
           nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
           nmFuncs.UseDefaultCredentials = true;
           nmFuncs.Credentials = netCred;

           MagentoSales.SalesDataOrderInterface[] soe = SOResponses.result.items;
           

           int OrdersInserted = 0;
           int OrdersUpdated = 0;

           string NAVOrderNo;
           if (bLog)
           {
               Log.WriteInfo("Fetched: " + soe.Length.ToString() + " Open Orders");
           }
           if (soe.Length > 0)
           {
               
               foreach (MagentoSales.SalesDataOrderInterface MagentoSalesOrder in soe)
               {                   
                       try
                       {
                           if (MagentoSalesOrder.items.Length > 0)
                           {
                               if (bLog)
                               {
                                   Log.WriteInfo("Downloading Order: " + MagentoSalesOrder.incrementId.ToString());
                               }

                               nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                               nmFuncs.UseDefaultCredentials = true;
                               bool bExists = nmFuncs.CheckMagentoIncrementID(MagentoSalesOrder.incrementId.ToString());
                               if (bExists & bLog)
                               {
                                   Log.WriteInfo("Order already in NAV: " + MagentoSalesOrder.incrementId.ToString());
                               }
                               if (!bExists)
                               {

                                   MagentoSales.SalesDataShippingAssignmentInterface[] salesData = MagentoSalesOrder.extensionAttributes.shippingAssignments;

                                   MagentoSales.SalesDataOrderAddressInterface _ShippingAddress = new MagentoSales.SalesDataOrderAddressInterface();
                                   if (salesData.Length > 0)
                                   {
                                       _ShippingAddress = salesData[0].shipping.address;
                                   }

                                   MagentoSales.SalesOrderRepositoryV1GetListRequest SORequest = new MagentoSales.SalesOrderRepositoryV1GetListRequest();

                                   decimal intTaxAmount = 0;
                                   // v2.01f if the Download to Single Customer Account is set then change the Customer Code to that specified
                                   if (!string.IsNullOrEmpty(NAVCusAcc))
                                   {
                                       MagentoSalesOrder.customerId = int.Parse(NAVCusAcc);
                                   }


                                   //Create Service Reference 
                                   SalesOrder_service.SalesOrder_Service SOService = new SalesOrder_service.SalesOrder_Service();
                                   SOService.Url = NAVWebURL + "/Page/SalesOrder";
                                   //example: "http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd./Page/SalesOrder";                                        
                                   SOService.UseDefaultCredentials = true;
                                   SOService.Credentials = netCred;

                                   //Create Service Reference for customer
                                   Customer_service.Customer_Service CustService = new Customer_service.Customer_Service();
                                   CustService.Url = NAVWebURL + "/Page/Customer";
                                   CustService.UseDefaultCredentials = true;
                                   CustService.Credentials = netCred;

                                   //Create Service Reference for item
                                   Item_Service.Item_Service itemService = new Item_Service.Item_Service();
                                   itemService.Url = NAVWebURL + "/Page/Item";
                                   itemService.UseDefaultCredentials = true;
                                   itemService.Credentials = netCred;

                                   //Create the Order header 
                                   SalesOrder_service.SalesOrder newNAVSOrder = new SalesOrder_service.SalesOrder();
                                   try
                                   {
                                       SOService.Create(ref newNAVSOrder);
                                   }
                                   catch (Exception newSOEx)
                                   {
                                       if (bLog)
                                       {
                                           Log.WriteError("Error: Creating new empty sales order for: " + MagentoSalesOrder.incrementId.ToString());
                                       }
                                       return newSOEx.Message.ToString();//"Error creating a new sales order";
                                   }

                                   // Update the header fields on the order
                                   DateTime OrderDate;
                                   if (DateTime.TryParse(MagentoSalesOrder.updatedAt.ToString(), out OrderDate))
                                   {
                                       newNAVSOrder.Order_Date = OrderDate;
                                   }
                                   else
                                   {
                                       newNAVSOrder.Order_Date = System.DateTime.Now;

                                   }
                                   //2.02g if this field is TRUE then in NAV Sales Header Table will insert a line in Cash Receipt Journal (table Gen Jrnl Line)
                                   newNAVSOrder.NewOrder = true;

                                   decimal decTest;
                                   if (!decimal.TryParse(MagentoSalesOrder.baseGrandTotal.ToString(), out decTest))
                                   {
                                       newNAVSOrder.TotalAmountMagentoOrder = (decimal)0;
                                   }
                                   else
                                   {
                                       newNAVSOrder.TotalAmountMagentoOrder = decTest;
                                   }

                                   newNAVSOrder.Salesperson_Code = "CONS";
                                   newNAVSOrder.External_Document_No = "SZUK" + MagentoSalesOrder.incrementId;
                                   //if (SOI.increment_id == "700002923")
                                   // {
                                   //    string strTest = "test";
                                   // }

                                   //newNAVSOrder.Email = SOI.customer_email; //CR3 this is pushed from magento to the sales header and from there to DPD                               

                                   // v2.02b populate the contact from the Magento Order
                                   if (!string.IsNullOrEmpty(MagentoSalesOrder.customerLastname))
                                   {
                                       newNAVSOrder.Bill_to_Contact = MagentoSalesOrder.customerFirstname + " " + MagentoSalesOrder.customerLastname;
                                   }
                                   else
                                   {
                                       newNAVSOrder.Bill_to_Contact = "";
                                   }
                                   // Only populate current code if not local currency
                                   if (MagentoSalesOrder.orderCurrencyCode.ToString().ToUpper() != "GBP")
                                   {
                                       newNAVSOrder.Currency_Code = MagentoSalesOrder.orderCurrencyCode;
                                   }

                                   // CS 1.02f - If customer_id is blank, use the billing address id.
                                   if (string.IsNullOrEmpty(MagentoSalesOrder.customerId.ToString()))
                                   {
                                       MagentoSalesOrder.customerId = MagentoSalesOrder.billingAddressId;
                                   }

                                   // v2.01f if a fixed customer account is specified then use that or use the one on the magento order
                                   if (!string.IsNullOrEmpty(NAVCusAcc))
                                   {
                                       newNAVSOrder.Sell_to_Customer_No = NAVCusAcc;
                                       newNAVSOrder.Sell_to_Customer_Name = MagentoSalesOrder.customerFirstname + " " + MagentoSalesOrder.customerLastname;
                                       newNAVSOrder.Sell_to_Contact = MagentoSalesOrder.customerFirstname + " " + MagentoSalesOrder.customerLastname;
                                       //TEST
                                       if (MagentoSalesOrder.incrementId == "700002947")
                                       {
                                           string strstTest = "test";
                                       }

                                       if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[0].ToString()))
                                       {
                                           if (MagentoSalesOrder.billingAddress.street[0].Length > 50)
                                           {
                                               newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0].ToString().Substring(0, 50);
                                           }
                                           else
                                           {
                                               newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0];
                                           }
                                       }
                                       else
                                       {
                                           newNAVSOrder.Sell_to_Address = ""; //we want to default it to blank if there isn't anything
                                       }
                                       if (MagentoSalesOrder.billingAddress.street.Length > 1)
                                       {
                                           if (MagentoSalesOrder.billingAddress.street[1].Length > 50)
                                           {
                                               newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1].ToString().Substring(0, 50);
                                           }
                                           else
                                           {
                                               newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1];
                                           }
                                       }
                                       else
                                       {
                                           newNAVSOrder.Sell_to_Address_2 = "";
                                       }
                                       if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.city))
                                       {
                                           if (MagentoSalesOrder.billingAddress.city.Length > 50)
                                           {
                                               newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city.ToString().Substring(0, 50);
                                           }
                                           else
                                           {
                                               newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city;
                                           }
                                       }
                                       else
                                       {
                                           newNAVSOrder.Sell_to_City = "";
                                       }

                                       newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region; //2.02f
                                       newNAVSOrder.Sell_to_Post_Code = MagentoSalesOrder.billingAddress.postcode;
                                       //TEST
                                       if (MagentoSalesOrder.incrementId == "700002947")
                                       {
                                           string strTest = "test";
                                       }

                                       //SHIPPING ADDRESS!!!!!!!!! +

                                       if (!String.IsNullOrEmpty(_ShippingAddress.street[0].ToString()))
                                       {
                                           if (_ShippingAddress.street[0].Length > 50)
                                           {
                                               newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0].Substring(0, 50);
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0];
                                           }
                                       }
                                       else
                                       {
                                           newNAVSOrder.Ship_to_Address = "";
                                       }

                                       //if (!String.IsNullOrEmpty(_ShippingAddress.street[1].ToString()))
                                       if (_ShippingAddress.street.Length > 1)
                                       {
                                           if (_ShippingAddress.street[1].Length > 50)
                                           {
                                               newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1].Substring(0, 50);
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1];
                                           }
                                       }
                                       else
                                       {
                                           newNAVSOrder.Ship_to_Address_2 = "";
                                       }

                                       newNAVSOrder.Ship_to_City = _ShippingAddress.city;
                                       newNAVSOrder.Ship_to_County = _ShippingAddress.region; //2.02f
                                       if (newNAVSOrder.Ship_to_County == "")
                                       {
                                           newNAVSOrder.Ship_to_County = _ShippingAddress.city;
                                       }

                                       newNAVSOrder.Ship_to_Post_Code = _ShippingAddress.postcode;
                                       //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                       //newNAVSOrder.Ship_to_Name = SOI.shipping_firstname + SOI.shipping_lastname;


                                       if (!string.IsNullOrEmpty(_ShippingAddress.telephone))
                                       {
                                           newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                       }
                                       else
                                       {
                                           newNAVSOrder.Ship_To_Phone_No = "";
                                       }


                                       if (!(_ShippingAddress.firstname + " " + _ShippingAddress.lastname).Equals(" "))
                                       {
                                           newNAVSOrder.Ship_to_Name = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                           newNAVSOrder.Ship_to_Contact = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                       }
                                       else
                                       {
                                           //newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                           newNAVSOrder.Ship_to_Name = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                           newNAVSOrder.Ship_to_Contact = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                       }
                                       //SOI.customer_firstname + " " + SOI.customer_lastname;
                                       ////SHIPPING ADDRESS -
                                       if (decimal.TryParse(MagentoSalesOrder.taxAmount.ToString(), out intTaxAmount))
                                       {
                                           newNAVSOrder.Prices_Including_VAT = false;
                                       }
                                       else
                                       {
                                           newNAVSOrder.Prices_Including_VAT = true; //this is exlcuding the VAT
                                       }
                                       // 2.02e Added addresses for the customer though is single customer 
                                       switch (newNAVSOrder.Ship_to_Country_Region_Code)
                                       {
                                           case "GB":
                                               newNAVSOrder.Shipping_Agent_Code = "DPD";                //2.02g depending on country shipment
                                               newNAVSOrder.Shipping_Agent_Service_Code = "1^12";       //2.02g depending on country shipment
                                               break;
                                           default:
                                               newNAVSOrder.Shipping_Agent_Code = "DHL";                    //'2.02g depending on country shipment
                                               newNAVSOrder.Shipping_Agent_Service_Code = "W";           //'2.02g depending on country shipment
                                               break;
                                       }
                                       newNAVSOrder.Shipment_Method_Code = "DDP";                           //2.02g depending on country shipment
                                   }
                                   else
                                   {
                                       // Query the customer table to get the customer code for the Magento CustomerID
                                       if (!string.IsNullOrEmpty(MagentoSalesOrder.customerId.ToString()))
                                       {


                                           nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                           nmFuncs.UseDefaultCredentials = true;
                                           string strCustomerNo = string.Empty; //customerNo.= "9 CJONES" or "9 CJONES01" or "9 CJONES02" if email different 
                                           if (MagentoSalesOrder.billingAddress.lastname.ToString().Length >= 10)
                                           {
                                               strCustomerNo = "9" + MagentoSalesOrder.billingAddress.firstname.Substring(0, 1).ToUpper() + MagentoSalesOrder.billingAddress.lastname.Substring(0, 10).ToUpper().Replace(" ", "");
                                           }
                                           else
                                           {
                                               strCustomerNo = "9" + MagentoSalesOrder.billingAddress.firstname.Substring(0, 1).ToUpper() + MagentoSalesOrder.billingAddress.lastname.ToUpper().Replace(" ", "");
                                           }
                                           //this calls a new function in NAV that returns the customer no. if it exists based on customer email and weird 9 FNLN logic :S
                                           newNAVSOrder.Sell_to_Customer_No = nmFuncs.CheckCustomer(MagentoSalesOrder.customerEmail, strCustomerNo);
                                           //TEST
                                           if (MagentoSalesOrder.incrementId == "700002947")
                                           {
                                               string strTest = "test";
                                           }

                                           //nmFuncs.GetCustomerIdByEcomID(MagentoSalesOrder.customer_id);
                                           //SHIPPING ADDRESS +
                                           if (!String.IsNullOrEmpty(_ShippingAddress.street[0])) //SHIPPING ADDRESS MagentoSalesOrder.shipping_address.street
                                           {
                                               if (_ShippingAddress.street[0].Length > 50)
                                               {
                                                   newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0].Substring(0, 50);
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0];
                                               }
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_Address = "";
                                           }
                                           //    if (!String.IsNullOrEmpty(_ShippingAddress.street[1].ToString())) //SHIPPING ADDRESS MagentoSalesOrder.shipping_address.street
                                           if (_ShippingAddress.street.Length > 1)
                                           {
                                               if (_ShippingAddress.street[1].Length > 50)
                                               {
                                                   newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1].Substring(0, 50);
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1];
                                               }
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_Address_2 = "";
                                           }

                                           //newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                           if (!String.IsNullOrEmpty(_ShippingAddress.city))
                                           {
                                               if (_ShippingAddress.city.Length > 30)
                                               {
                                                   newNAVSOrder.Ship_to_City = _ShippingAddress.city.Substring(0, 30);
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_City = _ShippingAddress.city;
                                               }
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_City = "";
                                           }


                                           newNAVSOrder.Ship_to_County = _ShippingAddress.region; //2.02f
                                           if (!String.IsNullOrEmpty(_ShippingAddress.region))
                                           {
                                               if (_ShippingAddress.region.Length > 30)
                                               {
                                                   newNAVSOrder.Ship_to_County = _ShippingAddress.region.Substring(0, 30);
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_County = _ShippingAddress.region;
                                               }
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_to_County = _ShippingAddress.city; //default to city if it's blank
                                           }
                                           newNAVSOrder.Ship_to_Post_Code = _ShippingAddress.postcode;
                                           //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                           //newNAVSOrder.Ship_to_Name = SOI.shipping_firstname + " " + SOI.shipping_lastname;

                                           if (!string.IsNullOrEmpty(_ShippingAddress.telephone))
                                           {
                                               newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                           }
                                           else
                                           {
                                               newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                           }

                                           if (!(_ShippingAddress.firstname + " " + _ShippingAddress.lastname).Equals(" "))
                                           {
                                               newNAVSOrder.Ship_to_Name = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                               newNAVSOrder.Ship_to_Contact = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                           }
                                           else
                                           {
                                               //newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                               newNAVSOrder.Ship_to_Name = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                               newNAVSOrder.Ship_to_Contact = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                           }

                                           //SHIPPING ADDRESS -
                                           if (string.IsNullOrEmpty(newNAVSOrder.Sell_to_Customer_No))
                                           {

                                               //CS 1.03c removed next line to improve processing
                                               string strAppendingCustNo = nmFuncs.GetLastCustomerNo(strCustomerNo);
                                               strCustomerNo = strCustomerNo + strAppendingCustNo; //this will look like 9 CMONCK01 or 9 CMONCK02 etc.. not clear why we do this??

                                               Customer_service.Customer newCustomer = new Customer_service.Customer();
                                               newCustomer.No = strCustomerNo; //set the customer no
                                               try
                                               {
                                                   CustService.Create(ref newCustomer);
                                                   if (bLog)
                                                   {
                                                       Log.WriteInfo("Creating new customer : " + strCustomerNo);
                                                   }
                                               }
                                               catch (Exception custEx)
                                               {

                                                   return "Error creating new customer with ecomm id: " + MagentoSalesOrder.customerId.ToString();
                                               }


                                               newCustomer.Name = MagentoSalesOrder.billingAddress.company;
                                               newCustomer.Contact = MagentoSalesOrder.billingAddress.firstname + " " + MagentoSalesOrder.billingAddress.lastname;
                                               if (string.IsNullOrEmpty(newCustomer.Name))
                                               {
                                                   newCustomer.Name = newCustomer.Contact;
                                               }
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[0]))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.street[0].Length > 50)
                                                   {
                                                       newCustomer.Address = MagentoSalesOrder.billingAddress.street[0].Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.Address = MagentoSalesOrder.billingAddress.street[0];
                                                   }
                                               }
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.city))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.city.Length > 30)
                                                   {
                                                       newCustomer.City = MagentoSalesOrder.billingAddress.city.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.City = MagentoSalesOrder.billingAddress.city;
                                                   }
                                               }

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.region))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.region.Length > 30)
                                                   {
                                                       newCustomer.County = MagentoSalesOrder.billingAddress.region.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.County = MagentoSalesOrder.billingAddress.region;
                                                   }
                                               }
                                               newCustomer.Post_Code = MagentoSalesOrder.billingAddress.postcode;
                                               newCustomer.Phone_No = MagentoSalesOrder.billingAddress.telephone;

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.customerEmail))
                                               {
                                                   if (MagentoSalesOrder.customerEmail.Length > 80)
                                                   {
                                                       newCustomer.E_Mail = MagentoSalesOrder.customerEmail.Substring(0, 80);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.E_Mail = MagentoSalesOrder.customerEmail;
                                                   }
                                               }

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.countryId))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.countryId.Length > 80)
                                                   {
                                                       newCustomer.Country_Region_Code = MagentoSalesOrder.billingAddress.countryId.Substring(0, 80);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.Country_Region_Code = MagentoSalesOrder.billingAddress.countryId;
                                                   }
                                               }


                                               newCustomer.Location_Code = WHSupplyLocationCode;
                                               newCustomer.Global_Dimension_2_Code = "Consumer"; //2.02g
                                               if (MagentoSalesOrder.orderCurrencyCode.ToString().ToUpper() != "GBP")
                                               {
                                                   newCustomer.Currency_Code = MagentoSalesOrder.orderCurrencyCode;
                                               }
                                               // Determine if the customer is VATABALE by checking the country code

                                               switch (newCustomer.Country_Region_Code)
                                               {
                                                   case "GB":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-UK";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DPD";                 //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "1^12";        //2.02g depending on country shipment
                                                       break;
                                                   case "Jersey - Channel Islands":
                                                   case "JE":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                  //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;

                                                   case "GG":
                                                   case "Guernsey - Channel Islands":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                                       newCustomer.Customer_Posting_Group = "STD";

                                                       newCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";             //2.02g depending on country shipment
                                                       break;
                                                   case "IT":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "IT";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";             //2.02g depending on country shipment
                                                       break;
                                                   case "ES":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "ES";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;
                                                   case "NO":
                                                   case "Norway":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;
                                                   case "CH":
                                                   case "Switzerland":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                  //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment  
                                                       break;
                                                   case "AT":
                                                   case "BG":
                                                   case "BE":
                                                   case "HR":
                                                   case "CZ":
                                                   case "DK":
                                                   case "EE":
                                                   case "FI":
                                                   case "FR":
                                                   case "DE":
                                                   case "HU":
                                                   case "IE":
                                                   case "LV":
                                                   case "LT":
                                                   case "PL":
                                                   case "RO":
                                                   case "SK":
                                                   case "SI":
                                                   case "SE":
                                                   case "NL":
                                                   case "LU":
                                                   case "PT":
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-UK"; //   "GB-EU"; //changed by request of Will Rees //changed again to GB-UK by request of Will & spreadsheet list
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";           //2.02g depending on country shipment
                                                       break;
                                                   default:
                                                       newCustomer.Gen_Bus_Posting_Group = "STD";
                                                       newCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       newCustomer.Customer_Posting_Group = "STD";
                                                       newCustomer.Shipping_Agent_Code = "DHL";                 //2.02g depending on country shipment
                                                       newCustomer.Shipping_Agent_Service_Code = "W";           //2.02g depending on country shipment
                                                       break;
                                               }
                                               newCustomer.Shipment_Method_Code = "DDP";                          //2.02g depending on country shipment                                            

                                               newNAVSOrder.Sell_to_Customer_No = newCustomer.No;
                                               //13/4/12 CS Changed the below so it is consistent
                                               //newOrder.Sell_to_Address = SalesOrderInfoResponse.result.billing_address.street
                                               //newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region

                                               //newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                               //newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[0].ToString()))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.street[0].Length > 50)
                                                   {
                                                       newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0];
                                                   }
                                               }

                                               //if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[1].ToString()))
                                               if (MagentoSalesOrder.billingAddress.street.Length > 1)
                                               {
                                                   if (MagentoSalesOrder.billingAddress.street[1].Length > 50)
                                                   {
                                                       newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1];
                                                   }
                                               }

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.city))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.city.Length > 30)
                                                   {
                                                       newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city;
                                                   }
                                               }

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.region))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.region.Length > 30)
                                                   {
                                                       newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region;
                                                   }
                                               }

                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.customerEmail))
                                               {
                                                   if (MagentoSalesOrder.customerEmail.Length > 80)
                                                   {
                                                       newCustomer.E_Mail = MagentoSalesOrder.customerEmail.Substring(0, 80);
                                                   }
                                                   else
                                                   {
                                                       newCustomer.E_Mail = MagentoSalesOrder.customerEmail;
                                                   }
                                               }

                                               newNAVSOrder.Sell_to_Post_Code = MagentoSalesOrder.billingAddress.postcode;

                                               //SHIPPING DETAILS +
                                               if (!String.IsNullOrEmpty(_ShippingAddress.street[0]))
                                               {
                                                   if (_ShippingAddress.street[0].Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0].Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0];
                                                   }
                                               }
                                               // if (!String.IsNullOrEmpty(_ShippingAddress.street[1].ToString()))
                                               if (_ShippingAddress.street.Length > 1)
                                               {
                                                   if (_ShippingAddress.street[1].Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1].Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1];
                                                   }
                                               }

                                               //newNAVSOrder.Ship_to_City = SOI.shipping_address.city; //replaced by below

                                               if (!String.IsNullOrEmpty(_ShippingAddress.city))
                                               {
                                                   if (_ShippingAddress.city.Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_City = _ShippingAddress.city.Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_City = _ShippingAddress.city;
                                                   }
                                               }


                                               //newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f //replaced by below
                                               if (!String.IsNullOrEmpty(_ShippingAddress.region))
                                               {
                                                   if (_ShippingAddress.region.Length > 30)
                                                   {
                                                       newNAVSOrder.Ship_to_County = _ShippingAddress.region.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_County = _ShippingAddress.region;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_County = _ShippingAddress.city;
                                               }


                                               newNAVSOrder.Ship_to_Post_Code = _ShippingAddress.postcode;


                                               //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                               if (MagentoSalesOrder.incrementId == "400001928")
                                               {
                                                   string strTest = "test";
                                               }

                                               if (!string.IsNullOrEmpty(_ShippingAddress.telephone))
                                               {
                                                   newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                               }
                                               if (!string.IsNullOrEmpty(_ShippingAddress.telephone))
                                               {
                                                   newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_To_Phone_No = _ShippingAddress.telephone;
                                               }

                                               if (!(_ShippingAddress.firstname + " " + _ShippingAddress.lastname).Equals(" "))
                                               {
                                                   newNAVSOrder.Ship_to_Name = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                                   newNAVSOrder.Ship_to_Contact = _ShippingAddress.firstname + " " + _ShippingAddress.lastname;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Name = MagentoSalesOrder.customerFirstname + " " + MagentoSalesOrder.customerLastname;
                                                   newNAVSOrder.Ship_to_Contact = MagentoSalesOrder.customerFirstname + " " + MagentoSalesOrder.customerLastname;
                                               }
                                               //newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id; //replaced by below
                                               if (!String.IsNullOrEmpty(_ShippingAddress.countryId))
                                               {
                                                   if (_ShippingAddress.countryId.Length > 10)
                                                   {
                                                       newNAVSOrder.Ship_to_Country_Region_Code = _ShippingAddress.countryId.Substring(0, 10);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Country_Region_Code = _ShippingAddress.countryId;
                                                   }
                                               }


                                               //SHIPPING DETAILS -
                                               switch (newNAVSOrder.Ship_to_Country_Region_Code)
                                               {
                                                   case "GB":
                                                       newNAVSOrder.Shipping_Agent_Code = "DPD";                 //'2.02g depending on country shipment
                                                       newNAVSOrder.Shipping_Agent_Service_Code = "1^12"; //'2.02g depending on country shipment
                                                       break;
                                                   default:
                                                       newNAVSOrder.Shipping_Agent_Code = "DHL";                  //  '2.02g depending on country shipment
                                                       newNAVSOrder.Shipping_Agent_Service_Code = "W";           //'2.02g depending on country shipment
                                                       break;
                                               }
                                               newNAVSOrder.Shipment_Method_Code = "DDP";                           //'2.02g depending on country shipment

                                               //13/4/12 CS Added following in as requested by Moon.
                                               newNAVSOrder.Requested_Delivery_Date = System.DateTime.Today;

                                               //cs 4/4 Amended to pass down prices including VAT
                                               //cs 23/10 v2.01b - changed to exclude VAT so pricing is correct in NAV for UK and overseas orders. Will now use default from customer record.
                                               //newOrder.Prices_Including_VAT = True
                                               newCustomer.eCommerce_Enabled = true;
                                               newCustomer.eCommerceID = MagentoSalesOrder.customerId.ToString();
                                               CustService.Update(ref newCustomer);
                                               if (bLog)
                                               {
                                                   Log.WriteInfo("Update Customer: " + newCustomer.No + " Magento ID: " + newCustomer.eCommerceID.ToString());
                                               }

                                           }
                                           else
                                           {

                                               //this means we need to update the customer rather than insert a new customer
                                               Customer_service.Customer updateCustomer = new Customer_service.Customer();

                                               try
                                               {
                                                   updateCustomer = CustService.Read(newNAVSOrder.Sell_to_Customer_No);
                                                   if (bLog)
                                                   {
                                                       Log.WriteInfo("Fetched NAV Customer: " + updateCustomer.No);
                                                   }
                                               }
                                               catch (Exception custEx)
                                               {
                                                   if (bLog)
                                                   {
                                                       Log.WriteError("Error fetching NAV Customer: " + newNAVSOrder.Sell_to_Customer_No);
                                                   }
                                                   return "Error reading customer to update with ecomm id: " + MagentoSalesOrder.customerId.ToString();
                                               }



                                               if (!string.IsNullOrEmpty(strDefaultCustomer))
                                               {
                                                   newNAVSOrder.Sell_to_Customer_No = strDefaultCustomer; /// usually customer no. 999
                                               }
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[0].ToString()))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.street[0].Length > 50)
                                                   {
                                                       newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0].ToString().Substring(0, 50);
                                                       updateCustomer.Address = MagentoSalesOrder.billingAddress.street[0].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_Address = MagentoSalesOrder.billingAddress.street[0];
                                                       updateCustomer.Address = MagentoSalesOrder.billingAddress.street[0];
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Sell_to_Address = "";
                                                   updateCustomer.Address = "";
                                               }

                                               //  if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[1].ToString()))
                                               if (MagentoSalesOrder.billingAddress.street.Length > 1)
                                               {
                                                   if (MagentoSalesOrder.billingAddress.street[1].Length > 50)
                                                   {
                                                       newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1].ToString().Substring(0, 50);
                                                       updateCustomer.Address_2 = MagentoSalesOrder.billingAddress.street[1].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_Address_2 = MagentoSalesOrder.billingAddress.street[1];
                                                       updateCustomer.Address_2 = MagentoSalesOrder.billingAddress.street[1];
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Sell_to_Address_2 = "";
                                                   updateCustomer.Address_2 = "";
                                               }

                                               //newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.street[0]))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.city.Length > 30)
                                                   {
                                                       newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city.Substring(0, 30);
                                                       updateCustomer.City = MagentoSalesOrder.billingAddress.city.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_City = MagentoSalesOrder.billingAddress.city;
                                                       updateCustomer.City = MagentoSalesOrder.billingAddress.city;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Sell_to_City = "";
                                                   updateCustomer.City = "";
                                               }
                                               newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region;
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.region))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.region.Length > 30)
                                                   {
                                                       newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region.Substring(0, 30);
                                                       updateCustomer.County = MagentoSalesOrder.billingAddress.region.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Sell_to_County = MagentoSalesOrder.billingAddress.region;
                                                       updateCustomer.County = MagentoSalesOrder.billingAddress.region;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Sell_to_County = "";
                                                   updateCustomer.County = "";
                                               }
                                               newNAVSOrder.Sell_to_Post_Code = MagentoSalesOrder.billingAddress.postcode;
                                               updateCustomer.Post_Code = MagentoSalesOrder.billingAddress.postcode;
                                               if (!String.IsNullOrEmpty(MagentoSalesOrder.billingAddress.countryId))
                                               {
                                                   if (MagentoSalesOrder.billingAddress.countryId.Length > 80)
                                                   {
                                                       updateCustomer.Country_Region_Code = MagentoSalesOrder.billingAddress.countryId.Substring(0, 80);
                                                   }
                                                   else
                                                   {
                                                       updateCustomer.Country_Region_Code = MagentoSalesOrder.billingAddress.countryId;
                                                   }
                                               }
                                               else
                                               {
                                                   updateCustomer.Country_Region_Code = "";
                                               }

                                               if (MagentoSalesOrder.orderCurrencyCode.ToString().ToUpper() != "GBP")
                                               {
                                                   updateCustomer.Currency_Code = MagentoSalesOrder.orderCurrencyCode;
                                               }
                                               // Determine if the customer is VATABALE by checking the country code

                                               switch (updateCustomer.Country_Region_Code)
                                               {
                                                   case "GB":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-UK";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DPD";                 //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "1^12";        //2.02g depending on country shipment
                                                       break;
                                                   case "Jersey - Channel Islands":
                                                   case "JE":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                  //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;

                                                   case "GG":
                                                   case "Guernsey - Channel Islands":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                                       updateCustomer.Customer_Posting_Group = "STD";

                                                       updateCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";             //2.02g depending on country shipment
                                                       break;
                                                   case "IT":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "IT";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";             //2.02g depending on country shipment
                                                       break;
                                                   case "ES":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "ES";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;
                                                   case "NO":
                                                   case "Norway":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                   //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment
                                                       break;
                                                   case "CH":
                                                   case "Switzerland":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                  //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";            //2.02g depending on country shipment  
                                                       break;
                                                   case "AT":
                                                   case "BG":
                                                   case "BE":
                                                   case "HR":
                                                   case "CZ":
                                                   case "DK":
                                                   case "EE":
                                                   case "FI":
                                                   case "FR":
                                                   case "DE":
                                                   case "HU":
                                                   case "IE":
                                                   case "LV":
                                                   case "LT":
                                                   case "PL":
                                                   case "RO":
                                                   case "SK":
                                                   case "SI":
                                                   case "SE":
                                                   case "NL":
                                                   case "LU":
                                                   case "PT":
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-UK"; //   "GB-EU"; //changed by request of Will Rees //changed again to GB-UK by request of Will & spreadsheet list
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";           //2.02g depending on country shipment
                                                       break;
                                                   default:
                                                       updateCustomer.Gen_Bus_Posting_Group = "STD";
                                                       updateCustomer.VAT_Bus_Posting_Group = "GB-ROW";
                                                       updateCustomer.Customer_Posting_Group = "STD";
                                                       updateCustomer.Shipping_Agent_Code = "DHL";                 //2.02g depending on country shipment
                                                       updateCustomer.Shipping_Agent_Service_Code = "W";           //2.02g depending on country shipment
                                                       break;
                                               }

                                               //update the customer
                                               try
                                               {
                                                   CustService.Update(ref updateCustomer);
                                                   if (bLog)
                                                   {
                                                       Log.WriteInfo("Updated NAV Customer: " + updateCustomer.No);
                                                   }
                                               }
                                               catch (Exception e)
                                               {
                                                   if (bLog)
                                                   {
                                                       Log.WriteError("Error Updating NAV Customer: " + updateCustomer.No);
                                                   }
                                                   return ("Unable to update customer...  " + e.Message.ToString());
                                               }

                                               //CS 13/4/12 Added below to deal with ship to addresses.  Also limited the address to 50 characters.

                                               if (!String.IsNullOrEmpty(_ShippingAddress.street[0]))
                                               {
                                                   if (_ShippingAddress.street[0].Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Address = _ShippingAddress.street[0];
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Address = "";
                                               }

                                               //if (!String.IsNullOrEmpty(_ShippingAddress.street[1]))
                                               if (_ShippingAddress.street.Length > 1)
                                               {
                                                   if (_ShippingAddress.street[1].Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1].ToString().Substring(0, 50);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Address_2 = _ShippingAddress.street[1];
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Address_2 = "";
                                               }

                                               //newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                               if (!String.IsNullOrEmpty(_ShippingAddress.city))
                                               {
                                                   if (_ShippingAddress.city.Length > 50)
                                                   {
                                                       newNAVSOrder.Ship_to_City = _ShippingAddress.city.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_City = _ShippingAddress.city;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_City = "";
                                               }
                                               //newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                               if (!String.IsNullOrEmpty(_ShippingAddress.region))
                                               {
                                                   if (_ShippingAddress.region.Length > 30)
                                                   {
                                                       newNAVSOrder.Ship_to_County = _ShippingAddress.region.Substring(0, 30);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_County = _ShippingAddress.region;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_County = _ShippingAddress.city;
                                               }
                                               newNAVSOrder.Ship_to_Post_Code = _ShippingAddress.postcode;
                                               if (!String.IsNullOrEmpty(_ShippingAddress.countryId))
                                               {
                                                   if (_ShippingAddress.countryId.Length > 10)
                                                   {
                                                       newNAVSOrder.Ship_to_Country_Region_Code = _ShippingAddress.countryId.Substring(0, 10);
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.Ship_to_Country_Region_Code = _ShippingAddress.countryId;
                                                   }
                                               }
                                               else
                                               {
                                                   newNAVSOrder.Ship_to_Country_Region_Code = "";
                                               }

                                               switch (newNAVSOrder.Ship_to_Country_Region_Code)
                                               {
                                                   case "GB":
                                                       newNAVSOrder.Shipping_Agent_Code = "DPD";                 //'2.02g depending on country shipment
                                                       newNAVSOrder.Shipping_Agent_Service_Code = "1^12"; //'2.02g depending on country shipment
                                                       break;
                                                   default:
                                                       newNAVSOrder.Shipping_Agent_Code = "DHL";                  //  '2.02g depending on country shipment
                                                       newNAVSOrder.Shipping_Agent_Service_Code = "W";           //'2.02g depending on country shipment
                                                       break;
                                               }
                                               newNAVSOrder.Shipment_Method_Code = "DDP";                           //'2.02g depending on country shipment


                                           }
                                       }
                                   }

                                   //update the VAT Bus posting group bsaed on the ship-to info
                                   switch (_ShippingAddress.countryId)
                                   {
                                       case "GB":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-UK";
                                           break;
                                       case "Jersey - Channel Islands":
                                       case "JE":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                           break;
                                       case "GG":
                                       case "Guernsey - Channel Islands":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-CHANNEL";
                                           break;
                                       case "IT":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "IT";
                                           break;
                                       case "ES":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "ES";
                                           break;
                                       case "NO":
                                       case "Norway":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-ROW";
                                           break;
                                       case "CH":
                                       case "Switzerland":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-ROW";
                                           break;
                                       case "AT":
                                       case "BG":
                                       case "HR":
                                       case "CZ":
                                       case "DK":
                                       case "EE":
                                       case "FI":
                                       case "FR":
                                       case "DE":
                                       case "HU":
                                       case "IE":
                                       case "LV":
                                       case "LT":
                                       case "PL":
                                       case "RO":
                                       case "SK":
                                       case "SI":
                                       case "SE":
                                       case "BE":
                                       case "NL":
                                       case "LU":
                                       case "PT":
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-UK"; //   "GB-EU"; //changed by request of Will Rees
                                           break;

                                       default:
                                           newNAVSOrder.VAT_Bus_Posting_Group = "GB-ROW";
                                           break;
                                   }


                                   //Create the blank order lines


                                   // v2.01f - need to add extra records for discount and shipping
                                   //newOrder.SalesLines = New SalesOrder_Service.Sales_Order_Line(SOI.Length) 
                                   //TEST                               
                                   if (MagentoSalesOrder.incrementId == "400001918")
                                   {
                                       string testIncreId = "test";
                                   }

                                   int NoOfSOLines = MagentoSalesOrder.items.Length;
                                   decimal decDisAmount = 0;
                                   decimal.TryParse(MagentoSalesOrder.discountAmount.ToString(), out decDisAmount);
                                   if (decDisAmount != 0)
                                   {
                                       NoOfSOLines = NoOfSOLines + 1;

                                   }
                                   decimal desShippingAmount = 0;
                                   decimal.TryParse(MagentoSalesOrder.shippingAmount.ToString(), out desShippingAmount);

                                   if (desShippingAmount > 0)
                                   {
                                       NoOfSOLines = NoOfSOLines + 1;
                                   }

                                   newNAVSOrder.SalesLines = new SalesOrder_service.Sales_Order_Line[NoOfSOLines];
                                   //Store the amazon/ebay/magento number                                
                                   //if (bEbay)
                                   //{
                                   //  newNAVSOrder.Ebay_Order_Number = SOI.store_id;
                                   //}
                                   // v2.01a Change this so it adds a line for SHIPPING at end
                                   for (int idx = 0; idx < NoOfSOLines; idx++) //added 2 extra lines in case there is a shipment line and a discount line
                                   {
                                       try
                                       {
                                           newNAVSOrder.SalesLines[idx] = new SalesOrder_service.Sales_Order_Line();
                                       }
                                       catch (Exception slEx)
                                       {
                                           return ("Error inserting blank order lines onto order");
                                       }

                                   }


                                   // Update the order 
                                   try
                                   {
                                       SOService.Update(ref newNAVSOrder);
                                       if (bLog)
                                       {
                                           Log.WriteInfo("Updating NAV Order with lines: " + newNAVSOrder.No);
                                       }
                                   }
                                   catch (Exception soUpdateEx)
                                   {
                                       if (bLog)
                                       {
                                           Log.WriteInfo("Error: Updating NAV Order with lines: " + newNAVSOrder.No);
                                       }
                                       return ("Unable to update order....     " + soUpdateEx.Message.ToString());
                                   }

                                   string LastSKU = string.Empty;
                                   int SalesOrderIDX = 0;
                                   //set a bool to signify if bundles are used
                                   bool bBundle = false;
                                   string strSalesOrderNo = string.Empty;
                                   int intLineNo = 0;
                                   // Populate the order line details
                                   for (int idx = 0; idx < MagentoSalesOrder.items.Length; idx++)
                                   {

                                       if (MagentoSalesOrder.items[idx].productType.ToLower() == "bundle")
                                       {
                                           bBundle = true;
                                       }
                                       // v2.01f if the line has a 0 qty then delete the line
                                       if ((MagentoSalesOrder.items[idx].basePrice == 0) && (MagentoSalesOrder.items[idx].sku == LastSKU))
                                       {
                                           //SOservice.Delete_SalesLines(idx)
                                           LastSKU = "";
                                       }

                                       else if ((MagentoSalesOrder.items[SalesOrderIDX].productType.ToLower() == "bundle") || ((bBundle == false) & (MagentoSalesOrder.items[SalesOrderIDX].productType.ToLower() == "simple")))
                                       {
                                           try
                                           {
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item;
                                               //CS V2.01 Changed this back to use the Price field which excludes VAT because NAV then applies the VAT.                                          
                                               string strItemNo = string.Empty;
                                               string strSku = string.Empty;
                                               if (MagentoSalesOrder.items[idx].sku.Length > 20)
                                               {
                                                   strSku = MagentoSalesOrder.items[idx].sku.Substring(0, 19);
                                               }
                                               else
                                               {
                                                   strSku = MagentoSalesOrder.items[idx].sku;
                                               }
                                               string strName = string.Empty;
                                               if (MagentoSalesOrder.items[idx].name.Length > 50)
                                               {
                                                   strName = MagentoSalesOrder.items[idx].name.Substring(0, 49);
                                               }
                                               else
                                               {
                                                   strName = MagentoSalesOrder.items[idx].name;
                                               }
                                               string strVariant = string.Empty;

                                               if (MagentoSalesOrder.items[idx].sku.Contains("#"))
                                               {
                                                   string[] lstItemDetails = MagentoSalesOrder.items[idx].sku.Split('#');
                                                   if (lstItemDetails.Length > 1)
                                                   {
                                                       strSku = lstItemDetails[0];
                                                       strVariant = lstItemDetails[1];
                                                       if (strSku.Length > 20)
                                                       {
                                                           strSku = strSku.Substring(0, 19);
                                                       }
                                                       if (strVariant.Length > 20)
                                                       {
                                                           strVariant = strVariant.Substring(0, 19);
                                                       }
                                                   }

                                               }
                                               //if (newNAVSOrder.External_Document_No == "100000039-1")
                                               //  {
                                               //     string strTest = "test";
                                               // }
                                               string strItemDescrip2 = string.Empty;

                                               //for testing SKU in Magento 2 is after the hash
                                               if (!string.IsNullOrEmpty(strVariant))
                                               {
                                                   strSku = strVariant;
                                               }

                                               strItemNo = nmFuncs.CheckIfItemExists(strSku, MagentoSalesOrder.items[idx].productId.ToString(), ref strName, bBundle, strVariant, ref strItemDescrip2);
                                               //we need to use thestrItemNo for the sales line
                                               newNAVSOrder.SalesLines[SalesOrderIDX].No = strItemNo;
                                               //newNAVSOrder.SalesLines[SalesOrderIDX].Variant_Code = strVariant;
                                               //newNAVSOrder.SalesLines[SalesOrderIDX].No = SOI.items[idx].sku;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Description = strName;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Description_2 = strItemDescrip2;//strSku;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Location_Code = WHSupplyLocationCode;

                                               // v 2.01f This this is a configurable product, save the SKU so we can delete the following line for the simple product
                                               if (MagentoSalesOrder.items[idx].productType.ToString().ToLower() == "configurable")
                                               {
                                                   LastSKU = MagentoSalesOrder.items[idx].sku;
                                               }

                                               decimal decQty = 0;
                                               if (decimal.TryParse(MagentoSalesOrder.items[idx].qtyOrdered.ToString(), out decQty))
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = decQty;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 0;
                                               }
                                               decimal decPrice = 0;
                                               //if (decimal.TryParse(SOI.items[idx].price, out decPrice))
                                               //TEST
                                               if (MagentoSalesOrder.incrementId == "400001918")
                                               {
                                                   string testIncreId = "test";
                                               }

                                               decimal.TryParse(MagentoSalesOrder.taxAmount.ToString(), out intTaxAmount);

                                               if (intTaxAmount > 0)
                                               {
                                                   if (decimal.TryParse(MagentoSalesOrder.items[idx].basePrice.ToString(), out decPrice))
                                                   {
                                                       newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decPrice;
                                                   }
                                                   else
                                                   {
                                                       newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                                   }
                                               }
                                               else
                                               {
                                                   if ((MagentoSalesOrder.billingAddress.countryId == "Switzerland") ||
                                                       (MagentoSalesOrder.billingAddress.countryId == "CH") ||
                                                       (MagentoSalesOrder.billingAddress.countryId == "JE") ||
                                                       (MagentoSalesOrder.billingAddress.countryId == "GG") ||
                                                       (MagentoSalesOrder.billingAddress.countryId == "NO"))
                                                   {
                                                       if (decimal.TryParse(MagentoSalesOrder.items[idx].basePrice.ToString(), out decPrice))
                                                       {
                                                           newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decPrice;
                                                       }
                                                       else
                                                       {
                                                           newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                                       }
                                                   }
                                                   else
                                                   {
                                                       if (decimal.TryParse(MagentoSalesOrder.items[idx].baseOriginalPrice.ToString(), out decPrice))
                                                       {
                                                           newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decPrice;
                                                       }
                                                       else
                                                       {
                                                           newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                                       }
                                                   }
                                               }


                                               // newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Excl_VATSpecified = false;                                           
                                               //CM 08/02/17 - change to get over the rounding issue +
                                               //newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VATSpecified = false;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VATSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VAT = (decimal)MagentoSalesOrder.items[idx].baseRowTotalInclTax;
                                               //newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_AmountSpecified = false;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_AmountSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_Amount = (decimal)MagentoSalesOrder.items[idx].baseTaxAmount;
                                               //CM 08/02/17 - change to get over the rounding issue -
                                               SOService.Update(ref newNAVSOrder); //we need to update here as we need the assembly no. back
                                               intLineNo = newNAVSOrder.SalesLines[SalesOrderIDX].Line_No;
                                               strSalesOrderNo = newNAVSOrder.SalesLines[SalesOrderIDX].Document_No;
                                           }
                                           catch (Exception ex1)
                                           {
                                               if (bLog)
                                               {
                                                   Log.WriteInfo("Error: Updating NAV Order with line: " + newNAVSOrder.SalesLines[SalesOrderIDX].Line_No.ToString());
                                               }
                                               return (ex1.InnerException.ToString());
                                           }
                                           SalesOrderIDX = SalesOrderIDX + 1;
                                       }
                                       else if (((bBundle == true) & (MagentoSalesOrder.items[SalesOrderIDX].productType.ToLower() == "simple")))
                                       {
                                           //this point we need to create an assembly line
                                           string strAssemblyNo = nmFuncs.GetAssemblyOrderNo(strSalesOrderNo, intLineNo);
                                           if (!string.IsNullOrEmpty(strAssemblyNo))
                                           {
                                               string strSku = string.Empty;
                                               if (MagentoSalesOrder.items[idx].sku.Length > 20)
                                               {
                                                   strSku = MagentoSalesOrder.items[idx].sku.Substring(0, 19);
                                               }
                                               else
                                               {
                                                   strSku = MagentoSalesOrder.items[idx].sku;
                                               }
                                               string strVariant = string.Empty;
                                               if (MagentoSalesOrder.items[idx].sku.Contains("#"))
                                               {
                                                   string[] lstItemDetails = MagentoSalesOrder.items[idx].sku.Split('#');
                                                   if (lstItemDetails.Length > 1)
                                                   {
                                                       strSku = lstItemDetails[1];
                                                       strVariant = lstItemDetails[2];
                                                       if (strSku.Length > 20)
                                                       {
                                                           strSku = strSku.Substring(0, 19);
                                                       }
                                                       if (strVariant.Length > 20)
                                                       {
                                                           strVariant = strVariant.Substring(0, 19);
                                                       }
                                                   }

                                               }

                                               string strName = string.Empty;
                                               if (MagentoSalesOrder.items[idx].name.Length > 50)
                                               {
                                                   strName = MagentoSalesOrder.items[idx].name.Substring(0, 49);
                                               }
                                               else
                                               {
                                                   strName = MagentoSalesOrder.items[idx].name;
                                               }
                                               decimal decQty = 0;
                                               decimal.TryParse(MagentoSalesOrder.items[idx].qtyOrdered.ToString(), out decQty);
                                               //at this point we can add the sales line to the assembly order
                                               nmFuncs.SetAssemblyComponent(strAssemblyNo, strSku, decQty, strName, strSku, strVariant);
                                               if (bLog)
                                               {
                                                   Log.WriteInfo("Updating NAV Order - added items to asembly order: " + newNAVSOrder.No + " Assembly No: " + strAssemblyNo);
                                               }
                                           }
                                           SalesOrderIDX = SalesOrderIDX + 1;
                                       }

                                   }

                                   // v2.01f only add a shipping line if there is a shipping amount on the order
                                   decimal decShipPrice = 0;
                                   if (decimal.TryParse(MagentoSalesOrder.shippingAmount.ToString(), out decShipPrice))
                                   {
                                       if (decShipPrice > 0)
                                       {
                                           // v2.01a Add the SHIPPING Line
                                           try
                                           {
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item; //SalesOrder_Service.Type.G_L_Account                                               
                                               newNAVSOrder.SalesLines[SalesOrderIDX].No = "SHIPPING";
                                               newNAVSOrder.SalesLines[SalesOrderIDX].TypeSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Description = MagentoSalesOrder.shippingDescription;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 1;

                                               // v2.01f Use the correct shipping amount for the order currency                                         
                                               if (decimal.TryParse(MagentoSalesOrder.shippingAmount.ToString(), out decShipPrice))
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decShipPrice;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                               }
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Unit_PriceSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].QuantitySpecified = true;
                                               //CM 08/02/17 - change to get over the rounding issue +                                           
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VATSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VAT = (decimal)MagentoSalesOrder.baseShippingInclTax;
                                               //newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_AmountSpecified = false;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_AmountSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_Amount = (decimal)MagentoSalesOrder.baseShippingTaxAmount;
                                               //CM 08/02/17 - change to get over the rounding issue -
                                           }
                                           catch (Exception slEx)
                                           {
                                               if (bLog)
                                               {
                                                   Log.WriteError("Error inserting shipping line " + newNAVSOrder.No);
                                               }
                                               return ("Error inserting blank order lines onto order");
                                           }
                                           SalesOrderIDX = SalesOrderIDX + 1;
                                       }
                                   }

                                   // v2.01f If there is a discount, add the Discount line to the order
                                   decimal decDiscountAmount = 0;
                                   if (decimal.TryParse(MagentoSalesOrder.discountAmount.ToString(), out decDiscountAmount))
                                   {
                                       if (decDiscountAmount != 0)
                                       {
                                           try
                                           {
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.G_L_Account; //SalesOrder_Service.Type.G_L_Account
                                               newNAVSOrder.SalesLines[SalesOrderIDX].TypeSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].No = DiscountGLAccount;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Location_Code = WHSupplyLocationCode;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 1;

                                               if (decimal.TryParse(MagentoSalesOrder.discountAmount.ToString(), out decDiscountAmount))
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decDiscountAmount;
                                               }
                                               else
                                               {
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                               }

                                               //2.02b if VAT is applied to order, deduct VAT from the discount amount.  We've had to assume if tax is applied then 
                                               // it needs the 20% vat deducting
                                               decimal decTaxAmount = 0;
                                               decimal.TryParse(MagentoSalesOrder.taxAmount.ToString(), out decTaxAmount);
                                               if (decTaxAmount > 0)
                                               {
                                                   decimal decGrandTaxAmount = 0;
                                                   decimal.TryParse(MagentoSalesOrder.grandTotal.ToString(), out decGrandTaxAmount);
                                                   if (decGrandTaxAmount == 0) { decGrandTaxAmount = 1; }
                                                   decimal decTaxRate = 0;
                                                   decTaxRate = decTaxAmount / decGrandTaxAmount;
                                                   newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decDiscountAmount -
                                                       (decDiscountAmount * tax_rate);
                                               }
                                               newNAVSOrder.SalesLines[SalesOrderIDX].Unit_PriceSpecified = true;
                                               newNAVSOrder.SalesLines[SalesOrderIDX].QuantitySpecified = true;

                                           }
                                           catch (Exception slEx)
                                           {
                                               Log.WriteError("Error inserting discount line " + newNAVSOrder.No);
                                               return ("Error inserting blank order lines onto order");
                                           }
                                       }
                                   }


                                   try
                                   {
                                       SOService.Update(ref newNAVSOrder);
                                       if (bLog)
                                       {
                                           Log.WriteInfo("Updated NAV Order: " + newNAVSOrder.No);
                                       }
                                       OrdersInserted = OrdersInserted + 1;
                                       //remove the blank lines
                                       nmFuncs.DeleteBlankLines(newNAVSOrder.No);
                                       nmFuncs.ReleaseSO(newNAVSOrder.No);

                                       // 13/4/12 CS Update the order status to processing
                                       try
                                       {      //need to skip this if in test mode!!!
                                           //v2.02b If we want to download processing orders, then change the filter
                                           if (bUpdateMagentoOrders)
                                           {

                                               UpdateOrderToArrivedInNAV(strMagentoURL, strUser, strPass, MagentoSalesOrder.incrementId, strInProgressOrders, "Order Accepted into NAV");
                                               if (bLog)
                                               {
                                                   Log.WriteInfo("Updated Magento Order: " + MagentoSalesOrder.incrementId.ToString() + " to in progress");
                                               }
                                               #region oldCode
                                               /*
                                           MagentoSales.SalesOrderManagementV1GetCommentsListRequest SOCommentList = new MagentoSales.SalesOrderManagementV1GetCommentsListRequest();
                                           

                                           MagentoSales.SalesOrderManagementV1AddCommentRequest SOComment = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                                           MagentoSales.SalesOrderRepositoryV1SaveRequest soRequest = new MagentoSales.SalesOrderRepositoryV1SaveRequest();
                                           MagentoSalesOrder.status = strInProgressOrders;
                                           MagentoSales.SalesOrderManagementV1AddCommentRequest SOCommentRequest = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                                           MagentoSales.SalesDataOrderStatusHistoryInterface SOCommentInterface = new MagentoSales.SalesDataOrderStatusHistoryInterface();
                                           SOCommentInterface.parentId = MagentoSalesOrder.entityId;
                                           SOCommentInterface.status = "Processing"; //strInProgressOrders;                                           
                                           SOCommentInterface.comment = "Order In NAV";
                                           SOCommentInterface.createdAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                                           

                                          
                                           
                                        
                                           MagentoSales.salesOrderManagementV1PortTypeClient client2;
                                           //myBinding = new CustomBinding();
                                           //myBinding.Name = "CustomBinding2";

                                           //tmebe = new TextMessageEncodingBindingElement();
                                           //tmebe.MessageVersion = MessageVersion.Soap12;
                                           //myBinding.Elements.Add(tmebe);
                                           //myBinding.Elements.Add(new HttpsTransportBindingElement());

                                          // endPointAddress = new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1");                                           
                                           using (client2 = new MagentoSales.salesOrderManagementV1PortTypeClient(myBinding, endPointAddress)){
                                                   // client.Endpoint.Binding = new BasicHttpsBinding();                    
                                                    timeout = new TimeSpan(0, 5, 0);
                                                    myBinding.SendTimeout = timeout;                                                    
                                                    hrmp = new HttpRequestMessageProperty();
                                                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                                                    client2.Endpoint.Address = endPointAddress;                                                        

                                                    contextScope = new OperationContextScope(client2.InnerChannel);
                                                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                                                    SOCommentList.id = MagentoSalesOrder.entityId;
                                                                                                   
                                                    MagentoSales.SalesDataOrderStatusHistoryInterface[] SOHistory = MagentoSalesOrder.statusHistories;
                                                        //SOCommentResponse.result;
                                                    if (SOHistory.Length == 0)
                                                    {
                                                      //  SOCommentRequest.id = 1;
                                                        SOCommentInterface.entityId = 1;
                                                        SOCommentInterface.entityIdSpecified = true;
                                                    }
                                                    else
                                                    {
                                                       // SOCommentRequest.id = SOHistory.totalCount + 1;
                                                        SOCommentInterface.entityId = SOHistory[0].entityId + 1;
                                                        SOCommentInterface.entityIdSpecified = true;
                                                    }
                                                    SOCommentInterface.entityName = "order";
                                                    SOCommentRequest.id = 1;
                                                    SOCommentRequest.statusHistory = SOCommentInterface;
                                                    try
                                                    {
                                                        client2.salesOrderManagementV1AddComment(SOCommentRequest);
                                                    }
                                                    catch (Exception eAddComment) { }

                                           }                                                                                     
                                           //client.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, strInProgressOrders, "", 0);
                                           OrdersUpdated = OrdersUpdated + 1;
                                           //MOD003 AZ CM 241016 - update orders to given status -
                                           
                                           */
                                               #endregion
                                           }
                                       }
                                       catch (Exception exl2)
                                       {
                                           if (bLog)
                                           {
                                               Log.WriteError("Error updating Magento Order: " + MagentoSalesOrder.incrementId.ToString() + " to in progress");
                                           }
                                           return ("Couldn't update order status for Magento orderID " + MagentoSalesOrder.incrementId.ToString() + "     " + exl2.ToString());
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       if (bLog)
                                       {
                                           Log.WriteError("Error inserting Magento Order: " + MagentoSalesOrder.incrementId.ToString() + " to in progress");
                                       }
                                       return ("Can't update order " + newNAVSOrder.No + "               " + ex.ToString());
                                   }


                               }
                           }
                       }
                       catch (Exception e)
                       {
                           //do something
                           if (bLog)
                           {
                               Log.WriteError("Error inserting Magento Order: " + MagentoSalesOrder.incrementId.ToString() + " to in progress");
                           }
                           return "Error processing sales orders: order:  " +  e.Message.ToString();
                       }
                   //} //removed check for Increment id
                   //} //MOD002
               }
           }
           if (bLog)
           {
               Log.WriteInfo("Finished downloading orders: " + System.DateTime.Today.ToShortDateString() + " " + System.DateTime.Now.ToString("h:mm:ss tt"));
           }
           return "Success";
           
       }
       }

       public bool UpdateOrderToShipped(string strMagentoURL,string strUser, string strPass, string strIncrementID, string strShippedStatus, string strComment)
       {
           int intEntityId;
           // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
           MagentoSales.salesOrderManagementV1PortTypeClient client;
           MagentoSales.salesOrderRepositoryV1PortTypeClient client2;
           getSessionID(strMagentoURL, strUser, strPass);
           //MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

           CustomBinding myBinding = new CustomBinding();
           myBinding.Name = "CustomBinding2";


           TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
           tmebe.MessageVersion = MessageVersion.Soap12;
           myBinding.Elements.Add(tmebe);
           // myBinding.Elements.Add(new HttpsTransportBindingElement());

           HttpsTransportBindingElement httpBindingElement = new HttpsTransportBindingElement();
           httpBindingElement.MaxBufferSize = Int32.MaxValue;
           httpBindingElement.MaxReceivedMessageSize = Int32.MaxValue;
           myBinding.Elements.Add(httpBindingElement);
           EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1,salesShipmentCommentRepositoryV1");

           using (client2 = new MagentoSales.salesOrderRepositoryV1PortTypeClient(myBinding, endPointAddress))
           {

               // client.Endpoint.Binding = new BasicHttpsBinding();                    
               System.TimeSpan timeout = new TimeSpan(0, 5, 0);
               myBinding.SendTimeout = timeout;

               //BasicHttpBinding();
               HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
               hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
               client2.Endpoint.Address = endPointAddress;


               //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

               OperationContextScope contextScope = new OperationContextScope(client2.InnerChannel);
               OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;



               //----------------------- Create new orders for pending items in Magento ---------------------------------------
               //v2.01f restrict the filter to orders with a status of Pending
               MagentoSales.SalesOrderRepositoryV1GetListRequest SalesOrderRequest = new MagentoSales.SalesOrderRepositoryV1GetListRequest();
               MagentoSales.FrameworkSearchCriteria SOSearch = new MagentoSales.FrameworkSearchCriteria();
               MagentoSales.FrameworkSearchFilterGroup SOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup();
               MagentoSales.FrameworkFilter SOFilter = new MagentoSales.FrameworkFilter();
               SOFilter.field = "Increment_ID";
               SOFilter.value = strIncrementID;//"400002706";// "000401994";
               MagentoSales.FrameworkFilter[] lstFilter = new MagentoSales.FrameworkFilter[1];
               lstFilter[0] = SOFilter;
               SOFilterGroup.filters = lstFilter;
               MagentoSales.FrameworkSearchFilterGroup[] lstSOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup[1];
               lstSOFilterGroup[0] = SOFilterGroup;
               SOSearch.filterGroups = lstSOFilterGroup;

               SalesOrderRequest.searchCriteria = SOSearch;

               //new MagentoSales.FrameworkSearchCriteria();
               MagentoSales.SalesDataShippingExtensionInterface salesShip = new MagentoSales.SalesDataShippingExtensionInterface();


               MagentoSales.SalesOrderRepositoryV1GetListResponse SOResponses = client2.salesOrderRepositoryV1GetList(SalesOrderRequest);
               try
               {
                   intEntityId = SOResponses.result.items[0].entityId;
               }
               catch (Exception e)
               {
                   return true; //just to get it off NAVs books
               }
           }

           try
           {
               using (client = new MagentoSales.salesOrderManagementV1PortTypeClient(myBinding, endPointAddress))
               {
                   // client.Endpoint.Binding = new BasicHttpsBinding();                    
                   System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                   myBinding.SendTimeout = timeout;

                   //BasicHttpBinding();
                   HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                   hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                   client.Endpoint.Address = endPointAddress;


                   //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                   OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                   OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;


                   MagentoSales.SalesOrderManagementV1GetCommentsListRequest SalesOrderRequest = new MagentoSales.SalesOrderManagementV1GetCommentsListRequest();
                   // SalesOrderRequest.
                   //MagentoSales.FrameworkSearchCriteria SOSearch = new MagentoSales.FrameworkSearchCriteria();
                   //MagentoSales.FrameworkSearchFilterGroup SOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup();
                   //SalesOrderRequest.id = intEntityId;
                   //MagentoSales.SalesOrderManagementV1GetCommentsListResponse commentResponses = client.salesOrderManagementV1GetCommentsList(SalesOrderRequest);



                   MagentoSales.SalesOrderManagementV1AddCommentRequest commentSubmit = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                   commentSubmit.id = intEntityId;
                   MagentoSales.SalesDataOrderStatusHistoryInterface _CommetInterface = new MagentoSales.SalesDataOrderStatusHistoryInterface();
                   _CommetInterface.comment = strComment;//"test";
                   _CommetInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                   _CommetInterface.status = strShippedStatus; //"processing";
                   commentSubmit.statusHistory = _CommetInterface;
                   MagentoSales.SalesOrderManagementV1AddCommentResponse commentResponse = client.salesOrderManagementV1AddComment(commentSubmit);


                   /*
                     SOCommentInterface.parentId = MagentoSalesOrder.entityId;
                                               SOCommentInterface.status = "Processing"; //strInProgressOrders;                                           
                                               SOCommentInterface.comment = "Order In NAV";
                                               SOCommentInterface.createdAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                    */

               }
           }
           catch (Exception e2)
           {
               return false;
           }
                      
           return true;

       }


       public bool UpdateOrderToArrivedInNAV(string strMagentoURL, string strUser, string strPass, string strIncrementID, string strShippedStatus, string strComment)
       {
           int intEntityId;
           // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
           MagentoSales.salesOrderManagementV1PortTypeClient client;
           MagentoSales.salesOrderRepositoryV1PortTypeClient client2;
           //getSessionID(strMagentoURL, strUser, strPass);
           //MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

           CustomBinding myBinding = new CustomBinding();
           myBinding.Name = "CustomBinding2";


           TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
           tmebe.MessageVersion = MessageVersion.Soap12;
           myBinding.Elements.Add(tmebe);
           // myBinding.Elements.Add(new HttpsTransportBindingElement());

           HttpsTransportBindingElement httpBindingElement = new HttpsTransportBindingElement();
           httpBindingElement.MaxBufferSize = Int32.MaxValue;
           httpBindingElement.MaxReceivedMessageSize = Int32.MaxValue;
           myBinding.Elements.Add(httpBindingElement);
           EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1,salesShipmentCommentRepositoryV1");

           using (client2 = new MagentoSales.salesOrderRepositoryV1PortTypeClient(myBinding, endPointAddress))
           {

               // client.Endpoint.Binding = new BasicHttpsBinding();                    
               System.TimeSpan timeout = new TimeSpan(0, 5, 0);
               myBinding.SendTimeout = timeout;

               //BasicHttpBinding();
               HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
               hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
               client2.Endpoint.Address = endPointAddress;


               //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

               OperationContextScope contextScope = new OperationContextScope(client2.InnerChannel);
               OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;



               //----------------------- Create new orders for pending items in Magento ---------------------------------------
               //v2.01f restrict the filter to orders with a status of Pending
               MagentoSales.SalesOrderRepositoryV1GetListRequest SalesOrderRequest = new MagentoSales.SalesOrderRepositoryV1GetListRequest();
               MagentoSales.FrameworkSearchCriteria SOSearch = new MagentoSales.FrameworkSearchCriteria();
               MagentoSales.FrameworkSearchFilterGroup SOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup();
               MagentoSales.FrameworkFilter SOFilter = new MagentoSales.FrameworkFilter();
               SOFilter.field = "Increment_ID";
               SOFilter.value = strIncrementID;//"400002706";// "000401994";
               MagentoSales.FrameworkFilter[] lstFilter = new MagentoSales.FrameworkFilter[1];
               lstFilter[0] = SOFilter;
               SOFilterGroup.filters = lstFilter;
               MagentoSales.FrameworkSearchFilterGroup[] lstSOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup[1];
               lstSOFilterGroup[0] = SOFilterGroup;
               SOSearch.filterGroups = lstSOFilterGroup;

               SalesOrderRequest.searchCriteria = SOSearch;

               //new MagentoSales.FrameworkSearchCriteria();
               MagentoSales.SalesDataShippingExtensionInterface salesShip = new MagentoSales.SalesDataShippingExtensionInterface();


               MagentoSales.SalesOrderRepositoryV1GetListResponse SOResponses = client2.salesOrderRepositoryV1GetList(SalesOrderRequest);
               try
               {
                   intEntityId = SOResponses.result.items[0].entityId;
               }
               catch (Exception e)
               {
                   return true; //just to get it off NAVs books
               }
           }

           try
           {
               using (client = new MagentoSales.salesOrderManagementV1PortTypeClient(myBinding, endPointAddress))
               {
                   // client.Endpoint.Binding = new BasicHttpsBinding();                    
                   System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                   myBinding.SendTimeout = timeout;

                   //BasicHttpBinding();
                   HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                   hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                   client.Endpoint.Address = endPointAddress;


                   //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                   OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                   OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;


                   MagentoSales.SalesOrderManagementV1GetCommentsListRequest SalesOrderRequest = new MagentoSales.SalesOrderManagementV1GetCommentsListRequest();
                   // SalesOrderRequest.
                   //MagentoSales.FrameworkSearchCriteria SOSearch = new MagentoSales.FrameworkSearchCriteria();
                   //MagentoSales.FrameworkSearchFilterGroup SOFilterGroup = new MagentoSales.FrameworkSearchFilterGroup();
                   //SalesOrderRequest.id = intEntityId;
                   //MagentoSales.SalesOrderManagementV1GetCommentsListResponse commentResponses = client.salesOrderManagementV1GetCommentsList(SalesOrderRequest);



                   MagentoSales.SalesOrderManagementV1AddCommentRequest commentSubmit = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                   commentSubmit.id = intEntityId;
                   MagentoSales.SalesDataOrderStatusHistoryInterface _CommetInterface = new MagentoSales.SalesDataOrderStatusHistoryInterface();
                   _CommetInterface.comment = strComment;//"test";
                   _CommetInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                   _CommetInterface.status = strShippedStatus; //"processing";
                   commentSubmit.statusHistory = _CommetInterface;
                   MagentoSales.SalesOrderManagementV1AddCommentResponse commentResponse = client.salesOrderManagementV1AddComment(commentSubmit);


                   /*
                     SOCommentInterface.parentId = MagentoSalesOrder.entityId;
                                               SOCommentInterface.status = "Processing"; //strInProgressOrders;                                           
                                               SOCommentInterface.comment = "Order In NAV";
                                               SOCommentInterface.createdAt = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
                    */

               }
           }
           catch (Exception e2)
           {
               return false;
           }

           return true;

       }
           
       public bool uploadInventoryLevels(string MagentoURL,string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
       {
           try
           {              
              MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;           
              getSessionID(MagentoURL,strUser, strPass);
                
              CustomBinding myBinding = new CustomBinding();
              myBinding.Name = "CustomBinding2";

           TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
           tmebe.MessageVersion = MessageVersion.Soap12;
           myBinding.Elements.Add(tmebe);
           myBinding.Elements.Add(new HttpsTransportBindingElement());
                    
           EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=services=catalogProductRepositoryV1");

           using (client = new MagentoProductSR.catalogProductRepositoryV1PortTypeClient(myBinding, endPointAddress))
           {

               client.Endpoint.Binding = new BasicHttpsBinding();
               //BasicHttpBinding();
               HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
               hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
               client.Endpoint.Address = endPointAddress;
               //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

               OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
               OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

               //Create Service Reference 
               Item_Service.Item_Service itemService = new Item_Service.Item_Service();
               itemService.Url = NAVWebURL + "/Page/Item";
               itemService.UseDefaultCredentials = true;
               itemService.Credentials = setCredentials(strUsername, strPassword, strDomain);
               Item_Service.Item_Filter itemFilter = new Item_Service.Item_Filter();
               itemFilter.Field = Item_Service.Item_Fields.eCommerce_Enabled; ;
               itemFilter.Criteria = "true";
               Item_Service.Item_Filter[] lstItemFilter = new Item_Service.Item_Filter[] { itemFilter };
               Item_Service.Item[] lstItems = itemService.ReadMultiple(lstItemFilter, null, 0);

               //MagentoAPIv2.catalogInventoryStockItemUpdateEntity itemUpdate;
               MagentoProductSR.CatalogProductRepositoryV1GetRequest getItem = new MagentoProductSR.CatalogProductRepositoryV1GetRequest();
               MagentoProductSR.CatalogProductRepositoryV1SaveRequest saveItem = new MagentoProductSR.CatalogProductRepositoryV1SaveRequest();
               //client.catalogProductRepositoryV1Save
               int intResponse = 0;
               if (lstItems.Length > 0)
               {
                   foreach (Item_Service.Item item in lstItems)
                   {

                       //get the item
                       getItem = new MagentoProductSR.CatalogProductRepositoryV1GetRequest();
                       getItem.sku = item.No;
                       MagentoProductSR.CatalogProductRepositoryV1GetResponse returnedItem = client.catalogProductRepositoryV1Get(getItem);
                       MagentoProductSR.CatalogDataProductInterface magItem = returnedItem.result;                       

                       /// then update and save the item


                       magItem.extensionAttributes.stockItem.qty = float.Parse(item.Inventory.ToString());
                       

                       if (item.Inventory > 0)
                       {
                           magItem.extensionAttributes.stockItem.isInStock = true;
                           magItem.extensionAttributes.stockItem.stockIdSpecified = true;                           
                       }
                       else
                       {
                           magItem.extensionAttributes.stockItem.isInStock = true;
                           magItem.extensionAttributes.stockItem.stockIdSpecified = true;
                           magItem.extensionAttributes.stockItem.qty = 0;                           
                       }

                       if (!String.IsNullOrEmpty(item.eCommerceID))
                       {
                           saveItem = new MagentoProductSR.CatalogProductRepositoryV1SaveRequest();
                           saveItem.product = magItem;
                           MagentoProductSR.CatalogProductRepositoryV1SaveResponse magItemResponse = client.catalogProductRepositoryV1Save(saveItem);
                           
                       }
                       //int intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, "150", itemUpdate);

                   }

               }

           }
              
              
               
               return true;
           }
           catch (Exception ex)
           {
               return false;
           }
       }

        private NetworkCredential setCredentials(string strUser, string strPassword, string strDomain)
        {
            System.Configuration.AppSettingsReader asr = new System.Configuration.AppSettingsReader();
            return new NetworkCredential(strUser, strPassword, strDomain);
        }

    }

}










