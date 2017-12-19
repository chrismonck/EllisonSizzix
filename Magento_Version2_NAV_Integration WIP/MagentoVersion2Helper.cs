using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Magento.RestApi;
using Magento_Version2_NAV_Integration_Sizzix.MagentoAuth;
using System.Xml.Serialization;
using System.Configuration;
//using Magento_Version2_NAV_Integration.MagentoAuth;

//CM 24/10/16 - TO KEEP TRACK OF CHANGES, I'M PUTTING COMMENTS AT THE TOP TO MATCH NAV
//MOD001 AZ CM 241016 - change to the status of new orders, allowed be chosen in NAV
//MOD002 AZ CM 241016 - commented out the check to download only processing orders, now downloads any order of a given status
//MOD003 AZ CM 241016 - update orders to given status
namespace Magento_Version2_NAV_Integration_Sizzix
{
    public class MagentoVersion2Helper
    {
        public string strSessionID;
        public MagentoVersion2Helper() { }

        //method to create a session to Magento
        public void getSessionID(string MagentoURL, string strUser, string strPass)
        {
            try
            {
                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;
                myBinding.Elements.Add(tmebe);
                myBinding.Elements.Add(new HttpsTransportBindingElement());
                EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "services=integrationAdminTokenServiceV1");
                integrationAdminTokenServiceV1PortTypeClient magAuthClient;
                using (magAuthClient = new integrationAdminTokenServiceV1PortTypeClient(myBinding, endPointAddress))
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

                strSessionID = string.Empty; //this is to stop the system reporting an error back to naav the odd time it timesout
            }

            //return strSessionID;

        }

        //Method to create a session to sizzix b2b magento site
        public void getSessionID_B2B(string MagentoURL, string strUser, string strPass)
        {
            try
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
            }
            catch (Exception e)
            {
                strSessionID = string.Empty;
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
        public void downloadPreOrdersMagentoToNAV(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain, string NAVCustAcc, string WHSupplyLocationCode, string strDefaultCustomer)
        {
            //connect to magento
            getSessionID_B2B(strMagentoURL, strUser, strPass); //this may change but for now point to the b2b

            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";
            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;
            myBinding.Elements.Add(new HttpsTransportBindingElement());
            EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=quoteCartRepositoryV1");
            MagentoQuoteCart_B2B.quoteCartRepositoryV1PortTypeClient client;

            using (client = new MagentoQuoteCart_B2B.quoteCartRepositoryV1PortTypeClient(myBinding, endPointAddress))
            {

                BasicHttpsBinding b = new BasicHttpsBinding();
                b.MaxReceivedMessageSize = 99999999;
                b.ReaderQuotas.MaxArrayLength = 999999;
                b.ReaderQuotas.MaxBytesPerRead = 999999;
                b.ReaderQuotas.MaxDepth = 999999;
                b.ReaderQuotas.MaxStringContentLength = 999999;
                b.ReaderQuotas.MaxNameTableCharCount = 999999;
                client.Endpoint.Binding = b;
                HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                try
                {
                    MagentoQuoteCart_B2B.quoteCartRepositoryV1GetListRequest1 _PreOrdersCriteria = new MagentoQuoteCart_B2B.quoteCartRepositoryV1GetListRequest1();

                    MagentoQuoteCart_B2B.FrameworkSearchCriteria _PreOrderSearch = new MagentoQuoteCart_B2B.FrameworkSearchCriteria();
                    MagentoQuoteCart_B2B.FrameworkSearchFilterGroup _PreOrderFilterGroup = new MagentoQuoteCart_B2B.FrameworkSearchFilterGroup();
                    MagentoQuoteCart_B2B.FrameworkSearchFilterGroup _PreOrderFilterGroup2 = new MagentoQuoteCart_B2B.FrameworkSearchFilterGroup();
                    MagentoQuoteCart_B2B.FrameworkFilter _PreOrderFilterisPreOrder = new MagentoQuoteCart_B2B.FrameworkFilter();
                    MagentoQuoteCart_B2B.FrameworkFilter _PreOrderFilterPreOrderStatus = new MagentoQuoteCart_B2B.FrameworkFilter();
                    MagentoQuoteCart_B2B.FrameworkFilter _CreateDateFilter = new MagentoQuoteCart_B2B.FrameworkFilter();

                    MagentoQuoteCart_B2B.FrameworkFilter[] lstFilters = new MagentoQuoteCart_B2B.FrameworkFilter[2];
                    MagentoQuoteCart_B2B.FrameworkFilter[] lstFilters2 = new MagentoQuoteCart_B2B.FrameworkFilter[1];

                    _PreOrderFilterisPreOrder.field = "is_preorder";
                    _PreOrderFilterisPreOrder.value = "1";
                    lstFilters[0] = _PreOrderFilterisPreOrder;

                    _PreOrderFilterPreOrderStatus.field = "preorder_status";
                    _PreOrderFilterPreOrderStatus.value = "ready";
                    lstFilters[1] = _PreOrderFilterPreOrderStatus;

                    _CreateDateFilter.field = "created_at";
                    _CreateDateFilter.conditionType = "gt";
                    _CreateDateFilter.value = System.DateTime.Now.AddDays(-100).ToString("yyyy-MM-ddTHH:mm:ss");
                    lstFilters2[0] = _CreateDateFilter;

                    _PreOrderFilterGroup.filters = lstFilters;
                    _PreOrderFilterGroup2.filters = lstFilters2;

                    MagentoQuoteCart_B2B.FrameworkSearchFilterGroup[] lstFilterGroup = new MagentoQuoteCart_B2B.FrameworkSearchFilterGroup[2];

                    lstFilterGroup[0] = _PreOrderFilterGroup;
                    lstFilterGroup[1] = _PreOrderFilterGroup2;

                    _PreOrderSearch.filterGroups = lstFilterGroup;
                    MagentoQuoteCart_B2B.QuoteCartRepositoryV1GetListRequest _PreOrderRequest = new MagentoQuoteCart_B2B.QuoteCartRepositoryV1GetListRequest();
                    //left in for testing
                    // _PreOrderSearch.pageSize = 10;
                    //  _PreOrderSearch.pageSizeSpecified = true;
                    _PreOrderRequest.searchCriteria = _PreOrderSearch;

                    _PreOrdersCriteria.quoteCartRepositoryV1GetListRequest = _PreOrderRequest;

                    MagentoQuoteCart_B2B.quoteCartRepositoryV1GetListResponse1 _PreOrderResponses = client.quoteCartRepositoryV1GetList(_PreOrdersCriteria);

                    NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
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

                    NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
                    nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                    nmFuncs.UseDefaultCredentials = true;
                    nmFuncs.Credentials = netCred;


                    foreach (MagentoQuoteCart_B2B.QuoteDataCartInterface _PreOrderData in _PreOrderResponses.quoteCartRepositoryV1GetListResponse.result.items)
                    {
                        //need to create a SO in the same way we create a SO for download orders

                        //check to order doesn't exist
                        SalesOrder_service.SalesOrder_Filter _CheckSOFilter = new SalesOrder_service.SalesOrder_Filter();
                        _CheckSOFilter.Field = SalesOrder_service.SalesOrder_Fields.External_Document_No;
                        _CheckSOFilter.Criteria = "SZUK" + _PreOrderData.id;
                        SalesOrder_service.SalesOrder_Filter[] _lstSOFilters = new SalesOrder_service.SalesOrder_Filter[] { _CheckSOFilter };
                        SalesOrder_service.SalesOrder[] _lstOrders = SOService.ReadMultiple(_lstSOFilters, "", 1);

                        if (_lstOrders.Count() == 0)
                        {

                            //Create the Order header 
                            SalesOrder_service.SalesOrder newNAVSOrder = new SalesOrder_service.SalesOrder();
                            try
                            {
                                SOService.Create(ref newNAVSOrder);
                            }
                            catch (Exception newSOEx)
                            {
                                throw new System.ArgumentException(newSOEx.Message.ToString());
                            }

                            // Update the header fields on the order
                            DateTime OrderDate;
                            if (DateTime.TryParse(_PreOrderData.updatedAt.ToString(), out OrderDate))
                            {
                                newNAVSOrder.Order_Date = OrderDate;
                            }
                            else
                            {
                                newNAVSOrder.Order_Date = System.DateTime.Now;

                            }
                            //2.02g if this field is TRUE then in NAV Sales Header Table will insert a line in Cash Receipt Journal (table Gen Jrnl Line)
                            newNAVSOrder.NewOrder = true;
                            newNAVSOrder.Salesperson_Code = "CONS";
                            newNAVSOrder.External_Document_No = "SZUK" + _PreOrderData.id;

                            // v2.02b populate the contact from the Magento Order
                            if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.lastname))
                            {
                                newNAVSOrder.Bill_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                            }
                            else
                            {
                                newNAVSOrder.Bill_to_Contact = "";
                            }
                            // Only populate current code if not local currency
                            if (_PreOrderData.currency.baseCurrencyCode.ToString().ToUpper() != "GBP")
                            {
                                newNAVSOrder.Currency_Code = _PreOrderData.currency.baseCurrencyCode;
                            }

                            //copied code from the downloadOrders with time we'll make this a common method but for now

                            // v2.01f if a fixed customer account is specified then use that or use the one on the magento order
                            if (!string.IsNullOrEmpty(NAVCustAcc))
                            {
                                newNAVSOrder.Sell_to_Customer_No = NAVCustAcc;
                                newNAVSOrder.Sell_to_Customer_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                newNAVSOrder.Sell_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;


                                if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0].ToString()))
                                {
                                    if (_PreOrderData.billingAddress.street[0].Length > 50)
                                    {
                                        newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0].ToString().Substring(0, 50);
                                    }
                                    else
                                    {
                                        newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0];
                                    }
                                }
                                else
                                {
                                    newNAVSOrder.Sell_to_Address = ""; //we want to default it to blank if there isn't anything
                                }
                                if (_PreOrderData.billingAddress.street.Length > 1)
                                {
                                    if (_PreOrderData.billingAddress.street[1].Length > 50)
                                    {
                                        newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1].ToString().Substring(0, 50);
                                    }
                                    else
                                    {
                                        newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                    }
                                }
                                else
                                {
                                    newNAVSOrder.Sell_to_Address_2 = "";
                                }
                                if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                {
                                    if (_PreOrderData.billingAddress.city.Length > 50)
                                    {
                                        newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city.ToString().Substring(0, 50);
                                    }
                                    else
                                    {
                                        newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city;
                                    }
                                }
                                else
                                {
                                    newNAVSOrder.Sell_to_City = "";
                                }

                                newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region; //2.02f
                                newNAVSOrder.Sell_to_Post_Code = _PreOrderData.billingAddress.postcode;


                                //SHIPPING ADDRESS!!!!!!!!! +

                                if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0].ToString()))
                                {
                                    if (_PreOrderData.billingAddress.street[0].Length > 50)
                                    {
                                        newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0].Substring(0, 50);
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0];
                                    }
                                }
                                else
                                {
                                    newNAVSOrder.Ship_to_Address = "";
                                }

                                if (_PreOrderData.billingAddress.street.Length > 1)
                                {
                                    if (_PreOrderData.billingAddress.street[1].Length > 50)
                                    {
                                        newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1].Substring(0, 50);
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                    }
                                }
                                else
                                {
                                    newNAVSOrder.Ship_to_Address_2 = "";
                                }

                                newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city;
                                newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region; //2.02f
                                if (newNAVSOrder.Ship_to_County == "")
                                {
                                    newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.city;
                                }

                                newNAVSOrder.Ship_to_Post_Code = _PreOrderData.billingAddress.postcode;

                                if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.telephone))
                                {
                                    newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                }
                                else
                                {
                                    newNAVSOrder.Ship_To_Phone_No = "";
                                }


                                if (!(_PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname).Equals(" "))
                                {
                                    newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                    newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                }
                                else
                                {
                                    newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                    newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                }
                                ////SHIPPING ADDRESS -

                                //for now we'll set the price doesn't include tax and then we can change it based on the lines
                                newNAVSOrder.Prices_Including_VAT = false;

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
                                if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.id.ToString()))
                                {


                                    nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                    nmFuncs.UseDefaultCredentials = true;
                                    string strCustomerNo = string.Empty; //customerNo.= "9 CJONES" or "9 CJONES01" or "9 CJONES02" if email different 
                                    if (_PreOrderData.billingAddress.lastname.ToString().Length >= 10)
                                    {
                                        strCustomerNo = "9" + _PreOrderData.billingAddress.firstname.Substring(0, 1).ToUpper() + _PreOrderData.billingAddress.lastname.Substring(0, 10).ToUpper().Replace(" ", "");
                                    }
                                    else
                                    {
                                        strCustomerNo = "9" + _PreOrderData.billingAddress.firstname.Substring(0, 1).ToUpper() + _PreOrderData.billingAddress.lastname.ToUpper().Replace(" ", "");
                                    }
                                    //this calls a new function in NAV that returns the customer no. if it exists based on customer email and weird 9 FNLN logic :S
                                    newNAVSOrder.Sell_to_Customer_No = nmFuncs.CheckCustomer(_PreOrderData.billingAddress.email, strCustomerNo);


                                    //nmFuncs.GetCustomerIdByEcomID(MagentoSalesOrder.customer_id);
                                    //SHIPPING ADDRESS +
                                    if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0])) //SHIPPING ADDRESS MagentoSalesOrder.shipping_address.street
                                    {
                                        if (_PreOrderData.billingAddress.street[0].Length > 50)
                                        {
                                            newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0].Substring(0, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0];
                                        }
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_Address = "";
                                    }

                                    if (_PreOrderData.billingAddress.street.Length > 1)
                                    {
                                        if (_PreOrderData.billingAddress.street[1].Length > 50)
                                        {
                                            newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1].Substring(0, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                        }
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_Address_2 = "";
                                    }


                                    if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                    {
                                        if (_PreOrderData.billingAddress.city.Length > 30)
                                        {
                                            newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city;
                                        }
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_City = "";
                                    }


                                    newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region; //2.02f
                                    if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                    {
                                        if (_PreOrderData.billingAddress.region.Length > 30)
                                        {
                                            newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region;
                                        }
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.city; //default to city if it's blank
                                    }
                                    newNAVSOrder.Ship_to_Post_Code = _PreOrderData.billingAddress.postcode;

                                    if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.telephone))
                                    {
                                        newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                    }

                                    if (!(_PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname).Equals(" "))
                                    {
                                        newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                        newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                        newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
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
                                        }
                                        catch (Exception custEx)
                                        {

                                            throw new System.ArgumentException("Error creating new customer with ecomm id: " + _PreOrderData.billingAddress.id.ToString());
                                        }


                                        newCustomer.Name = _PreOrderData.billingAddress.company;
                                        newCustomer.Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                        if (string.IsNullOrEmpty(newCustomer.Name))
                                        {
                                            newCustomer.Name = newCustomer.Contact;
                                        }
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0]))
                                        {
                                            if (_PreOrderData.billingAddress.street[0].Length > 50)
                                            {
                                                newCustomer.Address = _PreOrderData.billingAddress.street[0].Substring(0, 50);
                                            }
                                            else
                                            {
                                                newCustomer.Address = _PreOrderData.billingAddress.street[0];
                                            }
                                        }
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                        {
                                            if (_PreOrderData.billingAddress.city.Length > 30)
                                            {
                                                newCustomer.City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newCustomer.City = _PreOrderData.billingAddress.city;
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                        {
                                            if (_PreOrderData.billingAddress.region.Length > 30)
                                            {
                                                newCustomer.County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newCustomer.County = _PreOrderData.billingAddress.region;
                                            }
                                        }
                                        newCustomer.Post_Code = _PreOrderData.billingAddress.postcode;
                                        newCustomer.Phone_No = _PreOrderData.billingAddress.telephone;

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.email))
                                        {
                                            if (_PreOrderData.billingAddress.email.Length > 80)
                                            {
                                                newCustomer.E_Mail = _PreOrderData.billingAddress.email.Substring(0, 80);
                                            }
                                            else
                                            {
                                                newCustomer.E_Mail = _PreOrderData.billingAddress.email;
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.countryId))
                                        {
                                            if (_PreOrderData.billingAddress.countryId.Length > 80)
                                            {
                                                newCustomer.Country_Region_Code = _PreOrderData.billingAddress.countryId.Substring(0, 80);
                                            }
                                            else
                                            {
                                                newCustomer.Country_Region_Code = _PreOrderData.billingAddress.countryId;
                                            }
                                        }


                                        newCustomer.Location_Code = WHSupplyLocationCode;
                                        newCustomer.Global_Dimension_2_Code = "Consumer"; //2.02g
                                        if (_PreOrderData.currency.baseCurrencyCode.ToString().ToUpper() != "GBP")
                                        {
                                            newCustomer.Currency_Code = _PreOrderData.currency.baseCurrencyCode.ToString();
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

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0].ToString()))
                                        {
                                            if (_PreOrderData.billingAddress.street[0].Length > 50)
                                            {
                                                newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0];
                                            }
                                        }


                                        if (_PreOrderData.billingAddress.street.Length > 1)
                                        {
                                            if (_PreOrderData.billingAddress.street[1].Length > 50)
                                            {
                                                newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                        {
                                            if (_PreOrderData.billingAddress.city.Length > 30)
                                            {
                                                newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city;
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                        {
                                            if (_PreOrderData.billingAddress.region.Length > 30)
                                            {
                                                newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region;
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.email))
                                        {
                                            if (_PreOrderData.billingAddress.email.Length > 80)
                                            {
                                                newCustomer.E_Mail = _PreOrderData.billingAddress.email.Substring(0, 80);
                                            }
                                            else
                                            {
                                                newCustomer.E_Mail = _PreOrderData.billingAddress.email;
                                            }
                                        }

                                        newNAVSOrder.Sell_to_Post_Code = _PreOrderData.billingAddress.postcode;

                                        //SHIPPING DETAILS +
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0]))
                                        {
                                            if (_PreOrderData.billingAddress.street[0].Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0].Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0];
                                            }
                                        }
                                        if (_PreOrderData.billingAddress.street.Length > 1)
                                        {
                                            if (_PreOrderData.billingAddress.street[1].Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1].Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                        {
                                            if (_PreOrderData.billingAddress.city.Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city.Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city;
                                            }
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                        {
                                            if (_PreOrderData.billingAddress.region.Length > 30)
                                            {
                                                newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region;
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.city;
                                        }


                                        newNAVSOrder.Ship_to_Post_Code = _PreOrderData.billingAddress.postcode;




                                        if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.telephone))
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                        }
                                        if (!string.IsNullOrEmpty(_PreOrderData.billingAddress.telephone))
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = _PreOrderData.billingAddress.telephone;
                                        }

                                        if (!(_PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname).Equals(" "))
                                        {
                                            newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                            newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Name = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                            newNAVSOrder.Ship_to_Contact = _PreOrderData.billingAddress.firstname + " " + _PreOrderData.billingAddress.lastname;
                                        }
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.countryId))
                                        {
                                            if (_PreOrderData.billingAddress.countryId.Length > 10)
                                            {
                                                newNAVSOrder.Ship_to_Country_Region_Code = _PreOrderData.billingAddress.countryId.Substring(0, 10);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Country_Region_Code = _PreOrderData.billingAddress.countryId;
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
                                        newCustomer.eCommerce_Enabled = true;
                                        newCustomer.eCommerceID = _PreOrderData.billingAddress.id.ToString();
                                        CustService.Update(ref newCustomer);

                                    }
                                    else
                                    {

                                        //this means we need to update the customer rather than insert a new customer
                                        Customer_service.Customer updateCustomer = new Customer_service.Customer();

                                        try
                                        {
                                            updateCustomer = CustService.Read(newNAVSOrder.Sell_to_Customer_No);
                                        }
                                        catch (Exception custEx)
                                        {

                                            throw new System.ArgumentException("Error reading customer to update with ecomm id: " + _PreOrderData.billingAddress.id.ToString());
                                        }



                                        if (!string.IsNullOrEmpty(strDefaultCustomer))
                                        {
                                            newNAVSOrder.Sell_to_Customer_No = strDefaultCustomer; /// usually customer no. 999
                                        }
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0].ToString()))
                                        {
                                            if (_PreOrderData.billingAddress.street[0].Length > 50)
                                            {
                                                newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0].ToString().Substring(0, 50);
                                                updateCustomer.Address = _PreOrderData.billingAddress.street[0].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_Address = _PreOrderData.billingAddress.street[0];
                                                updateCustomer.Address = _PreOrderData.billingAddress.street[0];
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_Address = "";
                                            updateCustomer.Address = "";
                                        }

                                        if (_PreOrderData.billingAddress.street.Length > 1)
                                        {
                                            if (_PreOrderData.billingAddress.street[1].Length > 50)
                                            {
                                                newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1].ToString().Substring(0, 50);
                                                updateCustomer.Address_2 = _PreOrderData.billingAddress.street[1].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                                updateCustomer.Address_2 = _PreOrderData.billingAddress.street[1];
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_Address_2 = "";
                                            updateCustomer.Address_2 = "";
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0]))
                                        {
                                            if (_PreOrderData.billingAddress.city.Length > 30)
                                            {
                                                newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                                updateCustomer.City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_City = _PreOrderData.billingAddress.city;
                                                updateCustomer.City = _PreOrderData.billingAddress.city;
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_City = "";
                                            updateCustomer.City = "";
                                        }
                                        newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region;
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                        {
                                            if (_PreOrderData.billingAddress.region.Length > 30)
                                            {
                                                newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                                updateCustomer.County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Sell_to_County = _PreOrderData.billingAddress.region;
                                                updateCustomer.County = _PreOrderData.billingAddress.region;
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_County = "";
                                            updateCustomer.County = "";
                                        }
                                        newNAVSOrder.Sell_to_Post_Code = _PreOrderData.billingAddress.postcode;
                                        updateCustomer.Post_Code = _PreOrderData.billingAddress.postcode;
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.countryId))
                                        {
                                            if (_PreOrderData.billingAddress.countryId.Length > 80)
                                            {
                                                updateCustomer.Country_Region_Code = _PreOrderData.billingAddress.countryId.Substring(0, 80);
                                            }
                                            else
                                            {
                                                updateCustomer.Country_Region_Code = _PreOrderData.billingAddress.countryId;
                                            }
                                        }
                                        else
                                        {
                                            updateCustomer.Country_Region_Code = "";
                                        }

                                        if (_PreOrderData.currency.baseCurrencyCode.ToString().ToUpper() != "GBP")
                                        {
                                            updateCustomer.Currency_Code = _PreOrderData.currency.baseCurrencyCode;
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
                                        }
                                        catch (Exception e)
                                        {
                                            throw new System.ArgumentException("Unable to update customer...  " + e.Message.ToString());
                                        }

                                        //CS 13/4/12 Added below to deal with ship to addresses.  Also limited the address to 50 characters.

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.street[0]))
                                        {
                                            if (_PreOrderData.billingAddress.street[0].Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address = _PreOrderData.billingAddress.street[0];
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address = "";
                                        }

                                        if (_PreOrderData.billingAddress.street.Length > 1)
                                        {
                                            if (_PreOrderData.billingAddress.street[1].Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1].ToString().Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address_2 = _PreOrderData.billingAddress.street[1];
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address_2 = "";
                                        }

                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.city))
                                        {
                                            if (_PreOrderData.billingAddress.city.Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_City = _PreOrderData.billingAddress.city;
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_City = "";
                                        }
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.region))
                                        {
                                            if (_PreOrderData.billingAddress.region.Length > 30)
                                            {
                                                newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.region;
                                            }
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_County = _PreOrderData.billingAddress.city;
                                        }
                                        newNAVSOrder.Ship_to_Post_Code = _PreOrderData.billingAddress.postcode;
                                        if (!String.IsNullOrEmpty(_PreOrderData.billingAddress.countryId))
                                        {
                                            if (_PreOrderData.billingAddress.countryId.Length > 10)
                                            {
                                                newNAVSOrder.Ship_to_Country_Region_Code = _PreOrderData.billingAddress.countryId.Substring(0, 10);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Country_Region_Code = _PreOrderData.billingAddress.countryId;
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


                                    //update the VAT Bus posting group bsaed on the ship-to info
                                    switch (_PreOrderData.billingAddress.countryId)
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
                                    MagentoPreOrderItems.sizzixB2bPreOrdersPreOrdersServiceV1PortTypeClient linesClient;
                                    CustomBinding myBindingLines = new CustomBinding();
                                    myBinding.Name = "CustomBinding1";
                                    TextMessageEncodingBindingElement tmebeLines = new TextMessageEncodingBindingElement();
                                    tmebeLines.MessageVersion = MessageVersion.Soap12;
                                    myBinding.Elements.Add(new HttpsTransportBindingElement());
                                    EndpointAddress endPointAddressLines = new EndpointAddress(strMagentoURL + "services=sizzixB2bPreOrdersPreOrdersServiceV1");

                                    using (linesClient = new MagentoPreOrderItems.sizzixB2bPreOrdersPreOrdersServiceV1PortTypeClient(myBindingLines, endPointAddressLines))
                                    {

                                        BasicHttpsBinding bLineClient = new BasicHttpsBinding();
                                        bLineClient.MaxReceivedMessageSize = 99999999;
                                        bLineClient.ReaderQuotas.MaxArrayLength = 999999;
                                        bLineClient.ReaderQuotas.MaxBytesPerRead = 999999;
                                        bLineClient.ReaderQuotas.MaxDepth = 999999;
                                        bLineClient.ReaderQuotas.MaxStringContentLength = 999999;
                                        bLineClient.ReaderQuotas.MaxNameTableCharCount = 999999;
                                        linesClient.Endpoint.Binding = bLineClient;
                                        HttpRequestMessageProperty hrmpLines = new HttpRequestMessageProperty();
                                        hrmpLines.Headers.Add("Authorization", "Bearer " + strSessionID);

                                        contextScope = new OperationContextScope(linesClient.InnerChannel);
                                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmpLines;
                                        try
                                        {
                                            MagentoPreOrderItems.SizzixB2bPreOrdersPreOrdersServiceV1GetListRequest _LineRequest = new MagentoPreOrderItems.SizzixB2bPreOrdersPreOrdersServiceV1GetListRequest();
                                            _LineRequest.cartId = _PreOrderData.id;
                                            MagentoPreOrderItems.SizzixB2bPreOrdersPreOrdersServiceV1GetListResponse _LinesResponse = linesClient.sizzixB2bPreOrdersPreOrdersServiceV1GetList(_LineRequest);
                                            MagentoPreOrderItems.QuoteDataCartItemInterface[] _Lines = _LinesResponse.result;
                                            int NoOfSOLines = _Lines.Count();
                                            newNAVSOrder.SalesLines = new SalesOrder_service.Sales_Order_Line[NoOfSOLines];

                                            // v2.01a Change this so it adds a line for SHIPPING at end
                                            for (int idx = 0; idx < NoOfSOLines; idx++) //added 2 extra lines in case there is a shipment line and a discount line
                                            {
                                                try
                                                {
                                                    newNAVSOrder.SalesLines[idx] = new SalesOrder_service.Sales_Order_Line();
                                                }
                                                catch (Exception slEx)
                                                {
                                                    throw new System.ArgumentException("Error inserting blank order lines onto order");
                                                }

                                            }
                                            try
                                            {
                                                SOService.Update(ref newNAVSOrder);
                                            }
                                            catch (Exception soUpdateEx)
                                            {
                                                throw new System.ArgumentException("Unable to update order....     " + soUpdateEx.Message.ToString());
                                            }

                                            //now insert the lines                                            
                                            int intLineNo = 0;
                                            if (NoOfSOLines > 0)
                                            {
                                                foreach (MagentoPreOrderItems.QuoteDataCartItemInterface _item in _Lines)
                                                {

                                                    newNAVSOrder.SalesLines[intLineNo].Type = SalesOrder_service.Type.Item;
                                                    //CS V2.01 Changed this back to use the Price field which excludes VAT because NAV then applies the VAT.                                          
                                                    string strItemNo = string.Empty;
                                                    string strSku = string.Empty;
                                                    if (_item.sku.Length > 20)
                                                    {
                                                        strSku = _item.sku.Substring(0, 19);
                                                    }
                                                    else
                                                    {
                                                        strSku = _item.sku;
                                                    }
                                                    string strName = string.Empty;
                                                    if (_item.name.Length > 50)
                                                    {
                                                        strName = _item.name.Substring(0, 49);
                                                    }
                                                    else
                                                    {
                                                        strName = _item.name;
                                                    }
                                                    string strVariant = string.Empty;

                                                    if (_item.sku.Contains("#"))
                                                    {
                                                        string[] lstItemDetails = _item.sku.Split('#');
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

                                                    string strItemDescrip2 = string.Empty;

                                                    //for testing SKU in Magento 2 is after the hash
                                                    if (!string.IsNullOrEmpty(strVariant))
                                                    {
                                                        strSku = strVariant;
                                                    }

                                                    strItemNo = nmFuncs.CheckIfItemExists(strSku, _item.itemId.ToString(), ref strName, false, strVariant, ref strItemDescrip2);
                                                    //we need to use thestrItemNo for the sales line
                                                    newNAVSOrder.SalesLines[intLineNo].No = strItemNo;
                                                    newNAVSOrder.SalesLines[intLineNo].Description = strName;
                                                    newNAVSOrder.SalesLines[intLineNo].Description_2 = strItemDescrip2;//strSku;
                                                    newNAVSOrder.SalesLines[intLineNo].Location_Code = WHSupplyLocationCode;

                                                    decimal decQty = 0;
                                                    if (decimal.TryParse(_item.qty.ToString(), out decQty))
                                                    {
                                                        newNAVSOrder.SalesLines[intLineNo].Quantity = decQty;
                                                    }
                                                    else
                                                    {
                                                        newNAVSOrder.SalesLines[intLineNo].Quantity = 0;
                                                    }
                                                    decimal decPrice = 0;

                                                    //CM 08/02/17 - change to get over the rounding issue +

                                                    newNAVSOrder.SalesLines[intLineNo].Total_Amount_Incl_VATSpecified = false;
                                                    newNAVSOrder.SalesLines[intLineNo].Total_VAT_AmountSpecified = false;

                                                    //CM 08/02/17 - change to get over the rounding issue -
                                                    SOService.Update(ref newNAVSOrder); //we need to update here as we need the assembly no. back
                                                    intLineNo++;
                                                }
                                            }



                                        }
                                        catch (Exception lines)
                                        {
                                        }

                                        newNAVSOrder.Status = SalesOrder_service.Status.Open;
                                        newNAVSOrder.Magento_Pre_Order = true;
                                        newNAVSOrder.Magento_Pre_OrderSpecified = true;
                                        // Update the order 
                                        try
                                        {
                                            SOService.Update(ref newNAVSOrder);
                                        }
                                        catch (Exception soUpdateEx)
                                        {
                                            throw new System.ArgumentException("Unable to update order....     " + soUpdateEx.Message.ToString());
                                        }
                                    }



                                    MagentoPreOrderItems.SizzixB2bPreOrdersPreOrdersServiceV1SetPreOrderStatusRequest _updateStatusRequest = new MagentoPreOrderItems.SizzixB2bPreOrdersPreOrdersServiceV1SetPreOrderStatusRequest();
                                    _updateStatusRequest.cartId = _PreOrderData.id.ToString();
                                    _updateStatusRequest.status = "synced";
                                    linesClient.sizzixB2bPreOrdersPreOrdersServiceV1SetPreOrderStatus(_updateStatusRequest);

                                }
                            }


                            /*
                            //need to update the preorder
                            MagentoQuoteCart_B2B.QuoteDataCartExtensionInterface _PreOrderExtensions = _PreOrderData.extensionAttributes;
                            //_PreOrderData.entit

                            //preorder_status

                            MagentoQuoteCart_B2B.quoteCartRepositoryV1Save _saveRequest = new MagentoQuoteCart_B2B.quoteCartRepositoryV1Save();
                            _saveRequest.quoteCartRepositoryV1SaveRequest.quote = _PreOrderData;
                            client.quoteCartRepositoryV1Save(_saveRequest);
                             * */
                        }
                    }

                }
                catch (Exception e)
                {

                }
            }

        }

        //method to download messages from Magento and store them as interactions in NAV
        //but needs work as no detials on how to connect to magento?
        public string SyncInteractions(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try
            {
                // this is temporary whilst waiting for C3 to set up Magento side, simple test to make sure interaction can be insert into NAV
                NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
                //Create Service Reference for customer
                Interactions_service.MagInteraction_Service IntService = new Interactions_service.MagInteraction_Service();
                IntService.Url = NAVWebURL + "/Page/MagInteraction";
                IntService.UseDefaultCredentials = true;

                Interactions_service.MagInteraction _Interaction = new Interactions_service.MagInteraction();
                _Interaction.Magento_Customer_ID = "XXXXXX";
                _Interaction.Description = "THIS IS THE MESSAGE!!!!";
                _Interaction.Initiated_By = Interactions_service.Initiated_By.Them;
                _Interaction.Salesperson_Code = "ADMIN"; //this is the account coordinator
                IntService.Create(ref _Interaction);
                return _Interaction.Entry_No.ToString();
            }
            catch (Exception e)
            {
                return "Failed to insert";
            }


        }

        public bool submitInteractionToMagento(string strMagentoURL, string strUser, string strPass, string strReceipient, string strSender, string strSubject, string strBody, string strAttachment, string strAttachmentFN, string stFileExtension)
        {
            try
            {
                getSessionID_B2B(strMagentoURL, strUser, strPass);

                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;
                myBinding.Elements.Add(tmebe);
                myBinding.Elements.Add(new HttpsTransportBindingElement());

                EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=sizzixB2bAccountMessagesMessageServiceV1");
                MagentoAccountMessages.sizzixB2bAccountMessagesMessageServiceV1PortTypeClient client;
                using (client = new MagentoAccountMessages.sizzixB2bAccountMessagesMessageServiceV1PortTypeClient(myBinding, endPointAddress))
                {
                    //strSessionID = "aarrt5ni3iorj4mpqtquiepcfhsik8w2";
                    client.Endpoint.Binding = new BasicHttpsBinding();
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                    MagentoAccountMessages.SizzixB2bAccountMessagesMessageServiceV1CreateRequest _Request = new MagentoAccountMessages.SizzixB2bAccountMessagesMessageServiceV1CreateRequest();
                    List<MagentoAccountMessages.SizzixB2bAccountMessagesDataMessageInterface> _lstMessages = new List<MagentoAccountMessages.SizzixB2bAccountMessagesDataMessageInterface>();
                    MagentoAccountMessages.SizzixB2bAccountMessagesDataMessageInterface _Message = new MagentoAccountMessages.SizzixB2bAccountMessagesDataMessageInterface();
                    _Message.body = strBody;
                    _Message.createdAt = System.DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                    _Message.recipient = strReceipient;
                    _Message.sender = strSender;
                    _Message.subject = strSubject;
                    List<MagentoAccountMessages.SizzixB2bAccountMessagesDataAttachmentDataInterface> _LstAttachments = new List<MagentoAccountMessages.SizzixB2bAccountMessagesDataAttachmentDataInterface>();
                    MagentoAccountMessages.SizzixB2bAccountMessagesDataAttachmentDataInterface _Attachment = new MagentoAccountMessages.SizzixB2bAccountMessagesDataAttachmentDataInterface();
                    _Attachment.base64ImageString = strAttachment;
                    _Attachment.fileName = strAttachmentFN;
                    _Attachment.fileExtension = stFileExtension;
                    _LstAttachments.Add(_Attachment);
                    _Message.base64Attachments = _LstAttachments.ToArray();
                    _lstMessages.Add(_Message);
                    _Request.messages = _lstMessages.ToArray();
                    MagentoAccountMessages.SizzixB2bAccountMessagesMessageServiceV1CreateResponse _Response = client.sizzixB2bAccountMessagesMessageServiceV1Create(_Request);



                }
            }
            catch (Exception e)
            {
                return false; // just so NAV doesn't mark as sent and it can be resent
            }

            return true;
        }


        public MagentoProductSR.CatalogDataProductInterface[] ResyncProdIDsToNAVToolStripMenuItem(string strMagentoURL, string strUser, string strPass, ref int intCount)
        {

            try
            {
                getSessionID(strMagentoURL, strUser, strPass);
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


        public bool uploadSalesDiscounts(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try
            {
                getSessionID_B2B(strMagentoURL, strUser, strPass);
                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;
                myBinding.Elements.Add(tmebe);
                myBinding.Elements.Add(new HttpsTransportBindingElement());

                EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=sizzixB2bCustomerPricingSaleDiscountsServiceV1");

                MagentoCustomerPriceGroups.sizzixB2bCustomerPricingSaleDiscountsServiceV1PortTypeClient client;
                using (client = new MagentoCustomerPriceGroups.sizzixB2bCustomerPricingSaleDiscountsServiceV1PortTypeClient(myBinding, endPointAddress))
                {
                    //strSessionID = "aarrt5ni3iorj4mpqtquiepcfhsik8w2";
                    client.Endpoint.Binding = new BasicHttpsBinding();
                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                    OperationContextScope contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

                    MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1ClearDownRulesRequest _ClearDown = new MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1ClearDownRulesRequest();

                    // **** SKIPPED OVER AS ITS GOING TO CHANGE AT MAGENTO END ****                    
                    //  client.sizzixB2bCustomerPricingSaleDiscountsServiceV1ClearDownRules(_ClearDown); //this should clear all rules

                    //now apply the customer specific rules
                    MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1CreateRequest _CreateRequest = new MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1CreateRequest();
                    List<MagentoCustomerPriceGroups.SizzixB2bCustomerPricingDataDiscountInterface> _lstDiscounts = new List<MagentoCustomerPriceGroups.SizzixB2bCustomerPricingDataDiscountInterface>();

                    //fetch discounts from NAV
                    SalesPrices.SalesLineDiscounts_Service _SalesService = new SalesPrices.SalesLineDiscounts_Service();
                    _SalesService.Url = NAVWebURL + "/Page/SalesLineDiscounts";
                    _SalesService.UseDefaultCredentials = true;
                    _SalesService.Credentials = setCredentials(strUsername, strPassword, strDomain); ;
                    SalesPrices.SalesLineDiscounts_Filter _SalesServiceFilter = new SalesPrices.SalesLineDiscounts_Filter();
                    _SalesServiceFilter.Field = SalesPrices.SalesLineDiscounts_Fields.Sales_Type;
                    _SalesServiceFilter.Criteria = "Customer Disc. Group";
                    SalesPrices.SalesLineDiscounts_Filter[] lstServiceFilters = new SalesPrices.SalesLineDiscounts_Filter[] { _SalesServiceFilter };
                    SalesPrices.SalesLineDiscounts[] _Discounts = _SalesService.ReadMultiple(lstServiceFilters, string.Empty, 999999);
                    int intCount = 0;
                    //loop around all price discounts and add to the list
                    foreach (SalesPrices.SalesLineDiscounts _disc in _Discounts)
                    {
                        MagentoCustomerPriceGroups.SizzixB2bCustomerPricingDataDiscountInterface _MagDisc = new MagentoCustomerPriceGroups.SizzixB2bCustomerPricingDataDiscountInterface();
                        _MagDisc.amount = _disc.Line_Discount_Percent.ToString();
                        _MagDisc.custGroup = "Trade " + _disc.Sales_Code;
                        if (_disc.Ending_Date >= new DateTime(0005, 01, 01))
                        {
                            _MagDisc.endDate = _disc.Ending_Date.ToString("yyyy-MM-dd HH:mm:ss");
                        }
                        else
                        {
                            _disc.Ending_Date = new DateTime(2190, 1, 1);
                        }
                        _MagDisc.sku = _disc.Code;
                        _MagDisc.startDate = _disc.Starting_Date.ToString("yyyy-MM-dd HH:mm:ss");
                        _MagDisc.type = "percentage";
                        _lstDiscounts.Add(_MagDisc);
                        intCount++;
                        if (intCount >= 1)
                        {
                            _CreateRequest.discounts = _lstDiscounts.ToArray();
                            MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1CreateResponse _CreateResponse = client.sizzixB2bCustomerPricingSaleDiscountsServiceV1Create(_CreateRequest);
                            intCount = 0;
                            _lstDiscounts.Clear();

                        }

                    }



                    // _CreateRequest.discounts = _lstDiscounts.ToArray();                    
                    //MagentoCustomerPriceGroups.SizzixB2bCustomerPricingSaleDiscountsServiceV1CreateResponse _CreateResponse = client.sizzixB2bCustomerPricingSaleDiscountsServiceV1Create(_CreateRequest);
                    //return _CreateResponse.result;
                    return true;

                }
            }
            catch (Exception exc)
            {
                return false;
            }

        }

        //NEW ITEMS
        public void UploadItems(
            string strMagentoURL,
            string strUser,
            string strPass,
            string ItemNo, //SKU 
            string ItemDescription, //desc1 + desc 2
            decimal ItemNetWeight, //product weight
            decimal decGrossWeight, //product + package weight
            //dimensions
            string strDimenions,
            string strTheme, //Theme
            string strTechnology, //technology
            string strSubTechnology, //sub-technology
            string strDesigner, //designer
            //design dimensions
            string strDesignDimensions,
            string strServiceItemGroup, //service item group
            string strPlanogram, //planogram??
            string strItemDiscountGroup,
            string strVirtualWarehouse, //need to discuss what we're doing with this in NAV??
            decimal decBoxQuantity, //this will be from UOM in NAV
            decimal decPalletQuantity, //same as above from UOM in NAV
            string strChapter, //Need to discuss what this is in NAV?
            DateTime dtReleaseDate, //new field in NAV?
            string strProductType, //new field in NAV
            string strBrand,
            float ftPrice,
            decimal decMinQty,
            //return values            
            ref int intEcommerceID,
            ref string strError)
        {
            try
            {
                getSessionID_B2B(strMagentoURL, strUser, strPass);
                // Set the Magento API Endpoint
                MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;
                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;
                myBinding.Elements.Add(tmebe);
                myBinding.Elements.Add(new HttpsTransportBindingElement());
                EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=catalogProductRepositoryV1");
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

                    
                    List<MagentoProductSR.FrameworkAttributeInterface> _lstProductCustomAttributes = new List<MagentoProductSR.FrameworkAttributeInterface>(); //we'll narrow it down when we know                    
                    MagentoProductAttributesB2B.catalogProductAttributeRepositoryV1PortTypeClient attributeClient;
                    EndpointAddress AttributeEndPointAddress = new EndpointAddress(strMagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1,catalogProductAttributeRepositoryV1");
                    using (attributeClient = new MagentoProductAttributesB2B.catalogProductAttributeRepositoryV1PortTypeClient(myBinding, AttributeEndPointAddress))
                    {
                        BasicHttpsBinding b = new BasicHttpsBinding();
                        b.MaxReceivedMessageSize = 99999999;
                        attributeClient.Endpoint.Binding = b;

                        //BasicHttpBinding();
                        HttpRequestMessageProperty attributeHrmp = new HttpRequestMessageProperty();
                        attributeHrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                        OperationContextScope attributeContextScope = new OperationContextScope(attributeClient.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = attributeHrmp;
                        MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListRequest _SearchProductAttributes = new MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListRequest();
                        _SearchProductAttributes.searchCriteria = new MagentoProductAttributesB2B.FrameworkSearchCriteriaInterface();
                        MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListResponse _ResponseProductAttributes = attributeClient.catalogProductAttributeRepositoryV1GetList(_SearchProductAttributes);
                        MagentoProductAttributesB2B.CatalogDataProductAttributeInterface[] _ProductAttributes = _ResponseProductAttributes.result.items;
                        // we now how all the attributes going with the values if has any
                        //search for the product_weight
                        var query = (from p in _ProductAttributes
                                     where p.attributeCode == "product_weight"
                                     select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _ProductWeightAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _ProductWeightAttribute.attributeCode = query.attributeCode;
                        _ProductWeightAttribute.value = ItemNetWeight.ToString();
                        _lstProductCustomAttributes.Add(_ProductWeightAttribute);

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "packaged_weight"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _PackageWeightAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _PackageWeightAttribute.attributeCode = query.attributeCode;
                        _PackageWeightAttribute.value = decGrossWeight.ToString();
                        _lstProductCustomAttributes.Add(_PackageWeightAttribute);

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "description"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _descriptionAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _descriptionAttribute.attributeCode = query.attributeCode;
                        _descriptionAttribute.value = ItemDescription; ;
                        _lstProductCustomAttributes.Add(_descriptionAttribute);


                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "min_qty"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _minOrderAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _minOrderAttribute.attributeCode = query.attributeCode;
                        _minOrderAttribute.value = decMinQty.ToString();
                        _lstProductCustomAttributes.Add(_minOrderAttribute);


                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "machine_dimensions"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _DimensionsAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        if (query != null)
                        {
                            _DimensionsAttribute.attributeCode = query.attributeCode;
                            if (strDimenions == "")
                            {
                                strDimenions = "0";
                            }
                            _DimensionsAttribute.value = strDimenions;
                            _lstProductCustomAttributes.Add(_DimensionsAttribute);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _Theme = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_theme"
                                 select p).FirstOrDefault();


                        _Theme.attributeCode = query.attributeCode;
                        var themeQuery = (from theme in query.options
                                          where theme.label.ToLower() == strTheme.ToLower()
                                          select theme.value.ToString()).FirstOrDefault();
                        string[] lstThemes = new string[1];
                        if (themeQuery != null)
                        {

                            lstThemes[0] = themeQuery;
                            _Theme.value = themeQuery;
                            //lstThemes;                            
                            _lstProductCustomAttributes.Add(_Theme);
                        }

                        //theme raw - new field 19/12/17 
                        /*
                        MagentoProductSR.FrameworkAttributeInterface _ThemeRaw = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "theme_raw"
                                 select p).FirstOrDefault();
                        if (query != null)
                        {
                            _ThemeRaw.attributeCode = query.attributeCode;
                            _ThemeRaw.value = strTheme;
                        }

                        _lstProductCustomAttributes.Add(_ThemeRaw);
                         * */

                        //packaged_weight

                        MagentoProductSR.FrameworkAttributeInterface _Technology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_product_line"
                                 select p).FirstOrDefault();


                        _Technology.attributeCode = query.attributeCode;
                        var techQuery = (from tech in query.options
                                         where tech.label.ToLower() == strTechnology.ToLower()
                                         select tech.value.ToString()).FirstOrDefault();

                        if (techQuery != null)
                        {
                            _Technology.value = techQuery;
                            _lstProductCustomAttributes.Add(_Technology);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _SubTechnology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_subcategory"
                                 select p).FirstOrDefault();



                        _SubTechnology.attributeCode = query.attributeCode;
                        var subtechQuery = (from subtech in query.options
                                            where subtech.label.ToLower() == strSubTechnology.ToLower()
                                            select subtech.value.ToString()).FirstOrDefault();
                        if (subtechQuery != null)
                        {
                            lstThemes = new string[1];
                            lstThemes[0] = subtechQuery;
                            _SubTechnology.value = subtechQuery;
                            _lstProductCustomAttributes.Add(_SubTechnology);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _DesignerTechnology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_designer"
                                 select p).FirstOrDefault();


                        _DesignerTechnology.attributeCode = query.attributeCode;
                        var designerQuery = (from subtech in query.options
                                             where subtech.label.ToLower() == strDesigner.ToLower()
                                             select subtech.value.ToString()).FirstOrDefault();

                        if (subtechQuery != null)
                        {
                            lstThemes = new string[1];
                            lstThemes[0] = designerQuery;
                            _DesignerTechnology.value = designerQuery;
                            _lstProductCustomAttributes.Add(_DesignerTechnology);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _DesignerDimension = new MagentoProductSR.FrameworkAttributeInterface();


                        _DesignerDimension.attributeCode = "design_dimension";
                        if (strDesignDimensions == "") { strDesignDimensions = "0"; }
                        _DesignerDimension.value = strDesignDimensions;
                        _lstProductCustomAttributes.Add(_DesignerDimension);

                        MagentoProductSR.FrameworkAttributeInterface _DisplyStart = new MagentoProductSR.FrameworkAttributeInterface();
                        _DisplyStart.attributeCode = "display_start_date";
                        _DisplyStart.value = String.Format("{0:s}", System.DateTime.Now);  //0000-00-00 00:00:00                        
                        _lstProductCustomAttributes.Add(_DisplyStart);

                        MagentoProductSR.FrameworkAttributeInterface _ProductType = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "product_type"
                                 select p).FirstOrDefault();
                        var productType = (from subtech in query.options
                                           where subtech.label.ToLower() == strProductType.ToLower()
                                           select subtech.value.ToString()).FirstOrDefault();

                        if (productType != null)
                        {
                            _ProductType.attributeCode = query.attributeCode;
                            int intProductType = 0;
                            Int32.TryParse(productType, out intProductType);
                            _ProductType.value = intProductType;
                            _lstProductCustomAttributes.Add(_ProductType);
                        }



                        
                        //removed by request of C3
                        MagentoProductSR.FrameworkAttributeInterface _Brand = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "brand"
                                 select p).FirstOrDefault();
                        var brand = (from subtech in query.options
                                     where subtech.label.ToLower() == strBrand.ToLower()
                                     select subtech.value.ToString()).FirstOrDefault();

                        if (brand != null)
                        {
                            _Brand.attributeCode = query.attributeCode;

                            int intBrand = 0;
                            Int32.TryParse(brand, out intBrand);
                            _Brand.value = intBrand;
                            _lstProductCustomAttributes.Add(_Brand);
                        }
                        else
                        {
                            brand = (from subtech in query.options
                                     where subtech.label.ToLower() == "ellison"  //default to ellison 
                                     select subtech.value.ToString()).FirstOrDefault();
                            int intBrand = 0;
                            Int32.TryParse(brand, out intBrand);
                            _Brand.attributeCode = query.attributeCode;
                            _Brand.value = intBrand;
                            _lstProductCustomAttributes.Add(_Brand);
                        }
                         

                        MagentoProductSR.FrameworkAttributeInterface _PlanogramAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _PlanogramAttribute.attributeCode = "planogram";
                        _PlanogramAttribute.value = strPlanogram;

                        if (strPlanogram != "")
                        {
                            _lstProductCustomAttributes.Add(_PlanogramAttribute);
                        }
                       

                        //life cycle +
                        //search for the live cycle
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "life_cycle"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _LifeCycle = new MagentoProductSR.FrameworkAttributeInterface();

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "life_cycle"
                                 select p).FirstOrDefault();
                        var lifeCycle = (from lcQuery in query.options
                                         where lcQuery.label.ToLower() == strServiceItemGroup.ToLower()
                                         select lcQuery.value.ToString()).FirstOrDefault();

                        if (lifeCycle != null)
                        {
                            _LifeCycle.attributeCode = query.attributeCode;

                            _LifeCycle.attributeCode = query.attributeCode;
                            _LifeCycle.value = Int32.Parse(lifeCycle);
                            _lstProductCustomAttributes.Add(_LifeCycle);
                        }

                        //live cycle -

                        MagentoProductSR.FrameworkAttributeInterface _Chapter = new MagentoProductSR.FrameworkAttributeInterface();
                        _Chapter.attributeCode = "chapter";
                        _Chapter.value = strChapter;
                        if (strChapter != "")
                        {
                            _lstProductCustomAttributes.Add(_Chapter);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _ItemDiscountGroup = new MagentoProductSR.FrameworkAttributeInterface();
                        _ItemDiscountGroup.attributeCode = "item_discount_group";
                        _ItemDiscountGroup.value = strItemDiscountGroup;
                        if (strItemDiscountGroup != "")
                        {
                            _lstProductCustomAttributes.Add(_ItemDiscountGroup);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _VirtualWarehouse = new MagentoProductSR.FrameworkAttributeInterface();

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "product_virtual_warehouse"
                                 select p).FirstOrDefault();

                        var queryVirtualWarehouse = (from subtech in query.options
                                                     where subtech.label.ToLower() == strVirtualWarehouse.ToLower()
                                                     select subtech.value.ToString()).FirstOrDefault();

                        if (queryVirtualWarehouse != null)
                        {
                            _VirtualWarehouse.attributeCode = query.attributeCode;                            
                            _VirtualWarehouse.value = queryVirtualWarehouse;
                            _lstProductCustomAttributes.Add(_VirtualWarehouse);
                        }
                    }

                   
                   


                    int RecordsUpdated = 0;
                    intEcommerceID = 0;
                    strError = string.Empty;

                    try
                    {
                        hrmp = new HttpRequestMessageProperty();
                        hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                        contextScope = new OperationContextScope(client.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

                        SaveProductRecord.product = ProductRecord;                                                
                        SaveProductRecord.product.name = ItemDescription;                        
                        //All this below is defaulted based on trial and error with the API - neeed to discuss with Jamie C3
                        SaveProductRecord.product.attributeSetId = 9; //product = 9 and default = 4 // ProductRecord.attributeSetId;
                        SaveProductRecord.product.attributeSetIdSpecified = true;
                        SaveProductRecord.product.visibility = ProductRecord.visibility;
                        SaveProductRecord.product.visibilitySpecified = true;
                        SaveProductRecord.product.price = ftPrice;// ProductRecord.price; /// this has been left in during testing but needs revisitng
                        SaveProductRecord.product.priceSpecified = true;                        
                        SaveProductRecord.product.typeId = ProductRecord.typeId;
                        SaveProductRecord.product.weightSpecified = true;
                        SaveProductRecord.product.weight = float.Parse(decGrossWeight.ToString());
                        SaveProductRecord.product.sku = ItemNo;
                        SaveProductRecord.product.customAttributes = _lstProductCustomAttributes.ToArray();

                                     
                        MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                        MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                        intEcommerceID = returnedItem.id;
                        RecordsUpdated = RecordsUpdated + 1;
                    }
                    catch (Exception e)
                    {
                        throw new System.ArgumentException(e.Message.ToString());
                    }
                }
            }
            catch (Exception overallE)
            {
                throw new System.ArgumentException(overallE.Message.ToString());                
            }

        }

        //Update Items
        public bool UpdateItem(string MagentoURL, string strUser, string strPass, string ItemNo, string intEcommerceID,

             string ItemDescription, //desc1 + desc 2
             decimal ItemNetWeight, //product weight
             decimal decGrossWeight, //product + package weight
            //dimensions
             string strDimenions,
             string strTheme, //Theme
             string strTechnology, //technology
             string strSubTechnology, //sub-technology
             string strDesigner, //designer
             string strDesignDimensions,
             string strServiceItemGroup, //service item group
             string strPlanogram, //planogram??
             string strItemDiscountGroup,
             string strVirtualWarehouse, //need to discuss what we're doing with this in NAV??
             decimal decBoxQuantity, //this will be from UOM in NAV
             decimal decPalletQuantity, //same as above from UOM in NAV
             string strChapter, //Need to discuss what this is in NAV?
             DateTime dtReleaseDate, //new field in NAV?
             string strProductType, //new field in NAV
             string strBrand,
                float ftPrice,
            decimal decMinQty,
             ref string strError
            )
        {
            try
            {
                getSessionID_B2B(MagentoURL, strUser, strPass);
                //getSessionID
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

                    MagentoProductSR.FrameworkAttributeInterface[] _ProductCustomAttributes = new MagentoProductSR.FrameworkAttributeInterface[13]; //we'll narrow it down when we know
                    List<MagentoProductSR.FrameworkAttributeInterface> _lstProductCustomAttributes = new List<MagentoProductSR.FrameworkAttributeInterface>(); //we'll narrow it down when we know


                    #region fetch dropdownvalues

                    Log.WriteInfo("Start Updating Item: " + ItemNo + " " + intEcommerceID.ToString() + " to in progress");

                    MagentoProductAttributesB2B.catalogProductAttributeOptionManagementV1PortTypeClient optionClient;
                    EndpointAddress AttributeEndPointAddress = new EndpointAddress(MagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1,catalogProductAttributeRepositoryV1");
                    using (optionClient = new MagentoProductAttributesB2B.catalogProductAttributeOptionManagementV1PortTypeClient(myBinding, AttributeEndPointAddress))
                    {
                        BasicHttpsBinding b = new BasicHttpsBinding();
                        b.MaxReceivedMessageSize = 9999999;
                        optionClient.Endpoint.Binding = b;

                        //BasicHttpBinding();
                        HttpRequestMessageProperty attributeHrmp = new HttpRequestMessageProperty();
                        attributeHrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                        OperationContextScope attributeContextScope = new OperationContextScope(optionClient.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = attributeHrmp;

                        //product attribute options
                        MagentoProductAttributesB2B.CatalogProductAttributeOptionManagementV1GetItemsRequest _getOptionRequest = new MagentoProductAttributesB2B.CatalogProductAttributeOptionManagementV1GetItemsRequest();
                        _getOptionRequest.attributeCode = "life_cycle";
                        MagentoProductAttributesB2B.CatalogProductAttributeOptionManagementV1GetItemsResponse _OptionResponse = optionClient.catalogProductAttributeOptionManagementV1GetItems(_getOptionRequest);

                        Log.WriteInfo("Service Item Group: " + ItemNo + " " + strServiceItemGroup);

                        var optionQuery = (from p in _OptionResponse.result
                                           where p.label.ToLower() == strServiceItemGroup.ToLower()
                                           select p).FirstOrDefault();
                        strServiceItemGroup = optionQuery.value.ToString();

                        Log.WriteInfo("Service Item Group: " + ItemNo + " " + strServiceItemGroup);

                    }
                    #endregion


                    #region updateitem - using section

                    MagentoProductAttributesB2B.catalogProductAttributeRepositoryV1PortTypeClient attributeClient;
                    AttributeEndPointAddress = new EndpointAddress(MagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1,catalogProductAttributeRepositoryV1");
                    using (attributeClient = new MagentoProductAttributesB2B.catalogProductAttributeRepositoryV1PortTypeClient(myBinding, AttributeEndPointAddress))
                    {
                        BasicHttpsBinding b = new BasicHttpsBinding();
                        b.MaxReceivedMessageSize = 9999999;
                        attributeClient.Endpoint.Binding = b;

                        //BasicHttpBinding();
                        HttpRequestMessageProperty attributeHrmp = new HttpRequestMessageProperty();
                        attributeHrmp.Headers.Add("Authorization", "Bearer " + strSessionID);

                        OperationContextScope attributeContextScope = new OperationContextScope(attributeClient.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = attributeHrmp;
                        MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListRequest _SearchProductAttributes = new MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListRequest();
                        _SearchProductAttributes.searchCriteria = new MagentoProductAttributesB2B.FrameworkSearchCriteriaInterface();
                        MagentoProductAttributesB2B.CatalogProductAttributeRepositoryV1GetListResponse _ResponseProductAttributes = attributeClient.catalogProductAttributeRepositoryV1GetList(_SearchProductAttributes);
                        MagentoProductAttributesB2B.CatalogDataProductAttributeInterface[] _ProductAttributes = _ResponseProductAttributes.result.items;
                        // we now how all the attributes going with the values if has any

                        var query = (from p in _ProductAttributes
                                     where p.attributeCode == "product_weight"
                                     select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _ProductWeightAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _ProductWeightAttribute.attributeCode = query.attributeCode;
                        _ProductWeightAttribute.value = ItemNetWeight.ToString();
                        //_ProductCustomAttributes[0] = _ProductWeightAttribute;
                        _lstProductCustomAttributes.Add(_ProductWeightAttribute);
                       


                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "packaged_weight"
                                 select p).FirstOrDefault();


                        MagentoProductSR.FrameworkAttributeInterface _minOrderAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _minOrderAttribute.attributeCode = query.attributeCode;
                        _minOrderAttribute.value = decMinQty.ToString();
                        _lstProductCustomAttributes.Add(_minOrderAttribute);

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "description"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _descriptionAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _descriptionAttribute.attributeCode = query.attributeCode;
                        _descriptionAttribute.value = ItemDescription; ;
                        _lstProductCustomAttributes.Add(_descriptionAttribute);


                        MagentoProductSR.FrameworkAttributeInterface _PackageWeightAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _PackageWeightAttribute.attributeCode = query.attributeCode;
                        _PackageWeightAttribute.value = decGrossWeight.ToString();
                        _lstProductCustomAttributes.Add(_PackageWeightAttribute);

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "machine_dimensions"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _DimensionsAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        if (query != null)
                        {
                            _DimensionsAttribute.attributeCode = query.attributeCode;
                            if (strDimenions == "")
                            {
                                strDimenions = "0";
                            }
                            _DimensionsAttribute.value = strDimenions;
                            _lstProductCustomAttributes.Add(_DimensionsAttribute);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _Theme = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_theme"
                                 select p).FirstOrDefault();


                        _Theme.attributeCode = query.attributeCode;
                        var themeQuery = (from theme in query.options
                                          where theme.label.ToLower() == strTheme.ToLower()
                                          select theme.value.ToString()).FirstOrDefault();
                        string[] lstThemes = new string[1];
                        if (themeQuery != null)
                        {

                            lstThemes[0] = themeQuery;
                            _Theme.value = themeQuery;
                            _lstProductCustomAttributes.Add(_Theme);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _Technology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_product_line"
                                 select p).FirstOrDefault();


                        _Technology.attributeCode = query.attributeCode;
                        var techQuery = (from tech in query.options
                                         where tech.label.ToLower() == strTechnology.ToLower()
                                         select tech.value.ToString()).FirstOrDefault();

                        if (techQuery != null)
                        {
                            _Technology.value = techQuery; //lstThemes;
                            _lstProductCustomAttributes.Add(_Technology);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _SubTechnology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_subcategory"
                                 select p).FirstOrDefault();



                        _SubTechnology.attributeCode = query.attributeCode;
                        var subtechQuery = (from subtech in query.options
                                            where subtech.label.ToLower() == strSubTechnology.ToLower()
                                            select subtech.value.ToString()).FirstOrDefault();
                        if (subtechQuery != null)
                        {
                            lstThemes = new string[1];
                            lstThemes[0] = subtechQuery;
                            _SubTechnology.value = subtechQuery; //lstThemes;
                            //_ProductCustomAttributes[5] = _SubTechnology;
                            _lstProductCustomAttributes.Add(_SubTechnology);
                        }
                        MagentoProductSR.FrameworkAttributeInterface _DesignerTechnology = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "tag_designer"
                                 select p).FirstOrDefault();


                        _DesignerTechnology.attributeCode = query.attributeCode;
                        var designerQuery = (from subtech in query.options
                                             where subtech.label.ToLower() == strDesigner.ToLower()
                                             select subtech.value.ToString()).FirstOrDefault();

                        if (subtechQuery != null)
                        {
                            lstThemes = new string[1];
                            lstThemes[0] = designerQuery;
                            _DesignerTechnology.value = designerQuery; //lstThemes;
                            //_ProductCustomAttributes[6] = _DesignerTechnology;
                            _lstProductCustomAttributes.Add(_DesignerTechnology);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _DesignerDimension = new MagentoProductSR.FrameworkAttributeInterface();


                        _DesignerDimension.attributeCode = "design_dimension";
                        if (strDesignDimensions == "") { strDesignDimensions = "0"; }
                        _DesignerDimension.value = strDesignDimensions;// "Width: " + decDesWidth.ToString() + " Height: " + decDesHeight.ToString() + " Depth: " + decDesDepth.ToString();                                                     
                        //_ProductCustomAttributes[7] = _DesignerDimension;
                        _lstProductCustomAttributes.Add(_DesignerDimension);

                        MagentoProductSR.FrameworkAttributeInterface _DisplyStart = new MagentoProductSR.FrameworkAttributeInterface();
                        _DisplyStart.attributeCode = "display_start_date";
                        _DisplyStart.value = String.Format("{0:s}", System.DateTime.Now);  //0000-00-00 00:00:00
                        //_ProductCustomAttributes[8] = _DisplyStart;
                        _lstProductCustomAttributes.Add(_DisplyStart);

                        MagentoProductSR.FrameworkAttributeInterface _ProductType = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "product_type"
                                 select p).FirstOrDefault();
                        var productType = (from subtech in query.options
                                           where subtech.label.ToLower() == strProductType.ToLower()
                                           select subtech.value.ToString()).FirstOrDefault();

                        if (productType != null)
                        {
                            _ProductType.attributeCode = query.attributeCode;
                            int intProductType = 0;
                            Int32.TryParse(productType, out intProductType);
                            _ProductType.value = intProductType;
                            //_ProductCustomAttributes[9] = _ProductType;
                            _lstProductCustomAttributes.Add(_ProductType);
                        }

                        
                         //removed by request of C3
                        MagentoProductSR.FrameworkAttributeInterface _Brand = new MagentoProductSR.FrameworkAttributeInterface();
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "brand"
                                 select p).FirstOrDefault();
                        var brand = (from subtech in query.options
                                     where subtech.label.ToLower() == strBrand.ToLower()
                                     select subtech.value.ToString()).FirstOrDefault();

                        if (brand != null)
                        {
                            _Brand.attributeCode = query.attributeCode;

                            int intBrand = 0;
                            Int32.TryParse(brand, out intBrand);
                            _Brand.value = intBrand;
                            // _ProductCustomAttributes[10] = _Brand;
                            _lstProductCustomAttributes.Add(_Brand);
                        }
                        else
                        {
                            brand = (from subtech in query.options
                                     where subtech.label.ToLower() == "ellison"  //default to ellison 
                                     select subtech.value.ToString()).FirstOrDefault();
                            int intBrand = 0;
                            Int32.TryParse(brand, out intBrand);
                            _Brand.attributeCode = query.attributeCode;
                            _Brand.value = intBrand;
                            //_ProductCustomAttributes[10] = _Brand;
                            _lstProductCustomAttributes.Add(_Brand);
                        }
                        



                        MagentoProductSR.FrameworkAttributeInterface _PlanogramAttribute = new MagentoProductSR.FrameworkAttributeInterface();
                        _PlanogramAttribute.attributeCode = "planogram";
                        _PlanogramAttribute.value = strPlanogram;
                        //_ProductCustomAttributes[11] = _PlanogramAttribute;
                        if (strPlanogram != "")
                        {
                            _lstProductCustomAttributes.Add(_PlanogramAttribute);
                        }





                        //Service Item group
                        //search for the live cycle
                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "life_cycle"
                                 select p).FirstOrDefault();

                        MagentoProductSR.FrameworkAttributeInterface _LifeCycle = new MagentoProductSR.FrameworkAttributeInterface();
                        if (query != null)
                        {

                            _LifeCycle.attributeCode = query.attributeCode;
                            _LifeCycle.value = Int32.Parse(strServiceItemGroup);
                            //_ProductCustomAttributes[12] = _LifeCycle;
                            _lstProductCustomAttributes.Add(_LifeCycle);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _Chapter = new MagentoProductSR.FrameworkAttributeInterface();
                        _Chapter.attributeCode = "chapter";
                        _Chapter.value = strChapter;
                        if (strChapter != "")
                        {
                            //_ProductCustomAttributes[13] = _Chapter;
                            _lstProductCustomAttributes.Add(_Chapter);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _ItemDiscountGroup = new MagentoProductSR.FrameworkAttributeInterface();
                        _ItemDiscountGroup.attributeCode = "item_discount_group";
                        _ItemDiscountGroup.value = strItemDiscountGroup;
                        if (strItemDiscountGroup != "")
                        {
                            //_ProductCustomAttributes[14] = _ItemDiscountGroup;
                            _lstProductCustomAttributes.Add(_ItemDiscountGroup);
                        }

                        MagentoProductSR.FrameworkAttributeInterface _VirtualWarehouse = new MagentoProductSR.FrameworkAttributeInterface();

                        query = (from p in _ProductAttributes
                                 where p.attributeCode == "product_virtual_warehouse"
                                 select p).FirstOrDefault();

                        var queryVirtualWarehouse = (from subtech in query.options
                                                     where subtech.label.ToLower() == strVirtualWarehouse.ToLower()
                                                     select subtech.value.ToString()).FirstOrDefault();

                        if (queryVirtualWarehouse != null)
                        {
                            _VirtualWarehouse.attributeCode = query.attributeCode;

                            int intVirtualWarehouse = 0;
                            _VirtualWarehouse.value = queryVirtualWarehouse;
                            _lstProductCustomAttributes.Add(_VirtualWarehouse);
                        }



                    }
                    #endregion
                    MagentoProductSR.CatalogDataProductInterface ProductRecord = new MagentoProductSR.CatalogDataProductInterface();




                    string ProductSKU, ProductID;
                    MagentoProductSR.CatalogProductRepositoryV1GetRequest _getItem = new MagentoProductSR.CatalogProductRepositoryV1GetRequest();
                    _getItem.sku = ItemNo;
                    
                    contextScope = new OperationContextScope(client.InnerChannel);
                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                    MagentoProductSR.CatalogProductRepositoryV1GetResponse _getItemResponse = client.catalogProductRepositoryV1Get(_getItem);

                    ProductRecord = _getItemResponse.result;
                    ProductID = intEcommerceID;
                    //ProductRecord.name = ItemDescription;
                    
                    ProductRecord.customAttributes = _lstProductCustomAttributes.ToArray();
                    //_ProductCustomAttributes;
                   //MagentoProductSR.CatalogDataProductTierPriceInterface _tierPrices = new MagentoProductSR.CatalogDataProductTierPriceInterface();                
                   //ProductRecord.tierPrices 

                    ProductRecord.weight = float.Parse(decGrossWeight.ToString());
                    ProductSKU = ItemNo;
                    ProductRecord.status = 1;
                    ProductRecord.visibility = 4; //4 = Catalog,Search


                    ProductRecord.weight = float.Parse(decGrossWeight.ToString());
                    int RecordsUpdated = 0;
                    int RecordsFailed = 0;
                    int RecordsInserted = 0;
                    // If NAV Item already has a MagentoID then try to update the record.  
                    // If it doesn't then create a new record and store the ID in NAV
                    //Update the record
                    try
                    {
                        SaveProductRecord.product = ProductRecord;
                        SaveProductRecord.product.id = Int32.Parse(intEcommerceID);
                        SaveProductRecord.product.name = ItemDescription;
                        SaveProductRecord.product.attributeSetId = ProductRecord.attributeSetId; //product = 9 and default = 4
                        SaveProductRecord.product.attributeSetIdSpecified = true;
                        SaveProductRecord.product.visibility = ProductRecord.visibility;
                        SaveProductRecord.product.visibilitySpecified = true;
                        SaveProductRecord.product.price = ftPrice;//(float)100;// ProductRecord.price; This has been left in during testing and needs revisiting
                        SaveProductRecord.product.priceSpecified = true;
                        SaveProductRecord.product.typeId = ProductRecord.typeId;
                        SaveProductRecord.product.weightSpecified = true;
                        SaveProductRecord.product.weight = float.Parse(decGrossWeight.ToString());
                        SaveProductRecord.product.customAttributes = _lstProductCustomAttributes.ToArray();
                        //_ProductCustomAttributes;

                        MagentoProductSR.CatalogProductRepositoryV1SaveResponse respItem = client.catalogProductRepositoryV1Save(SaveProductRecord);
                        MagentoProductSR.CatalogDataProductInterface returnedItem = respItem.result;
                        intEcommerceID = returnedItem.id.ToString();
                        //.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                        RecordsUpdated = RecordsUpdated + 1;
                    }
                    catch (Exception e)
                    {

                        Log.WriteError("Errored: " + ItemNo + " " + e.Message.ToString());
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
                Log.WriteError("Errored: " + ItemNo + " " + ex.Message.ToString());
                return false;
            }

        }

        public bool downloadCustomers(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try
            {
                string strNavCustId = string.Empty;
                //connect to magento
                getSessionID_B2B(strMagentoURL, strUser, strPass);
                // Set the Magento API Endpoint                
                CustomBinding myBinding = new CustomBinding();
                myBinding.Name = "CustomBinding1";

                TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
                tmebe.MessageVersion = MessageVersion.Soap12;

                myBinding.Elements.Add(new HttpsTransportBindingElement());

                EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=customerCustomerRepositoryV1,customerAddressRepositoryV1");
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


                        MagentoCustomer.CustomerCustomerRepositoryV1GetListRequest _CustomerCriteria = new MagentoCustomer.CustomerCustomerRepositoryV1GetListRequest();
                        MagentoCustomer.FrameworkSearchCriteriaInterface _CustSearch = new MagentoCustomer.FrameworkSearchCriteriaInterface();
                        MagentoCustomer.FrameworkSearchFilterGroup _CustFilterGroup = new MagentoCustomer.FrameworkSearchFilterGroup();
                        MagentoCustomer.FrameworkFilter _CustFilter = new MagentoCustomer.FrameworkFilter();
                        _CustFilter.field = "approval_status";
                        _CustFilter.value = "new";

                        MagentoCustomer.FrameworkFilter[] lstFilters = new MagentoCustomer.FrameworkFilter[1];
                        lstFilters[0] = _CustFilter;
                        _CustFilterGroup.filters = lstFilters;
                        MagentoCustomer.FrameworkSearchFilterGroup[] lstFilterGroup = new MagentoCustomer.FrameworkSearchFilterGroup[1];
                        lstFilterGroup[0] = _CustFilterGroup;

                        _CustSearch.filterGroups = lstFilterGroup;

                        _CustomerCriteria.searchCriteria = _CustSearch;

                        NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);



                        MagentoCustomer.CustomerCustomerRepositoryV1GetListResponse _CustomerResponses = client.customerCustomerRepositoryV1GetList(_CustomerCriteria);
                        //  Log.WriteInfo("Start updating customers no of records to update Magento: " + _CustomerResponses.result.totalCount);         

                        foreach (MagentoCustomer.CustomerDataCustomerInterface _CustomerData in _CustomerResponses.result.items)
                        {


                            if (_CustomerData.customAttributes != null)
                            {
                                var queryNewCustomers = (from ca in _CustomerData.customAttributes
                                                         where ca.attributeCode == "approval_status"
                                                         select ca).FirstOrDefault();
                                if (queryNewCustomers != null)
                                {
                                    System.Xml.XmlNode[] xmlQueryNewCustomer = (System.Xml.XmlNode[])queryNewCustomers.value;

                                    string strStatus = string.Empty;
                                    if (xmlQueryNewCustomer[0].Value != null)
                                    {
                                        strStatus = xmlQueryNewCustomer[0].Value.ToString();
                                    }


                                    if (strStatus.ToString().ToLower() == "new")
                                    {

                                        Log.WriteInfo("Start updating customers no: " + _CustomerData.id); ;
                                        strNavCustId = string.Empty;
                                        NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
                                        nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                        nmFuncs.UseDefaultCredentials = true;
                                        nmFuncs.Credentials = netCred;

                                        //Create Service Reference for customer
                                        Customer_service.Customer_Service CustService = new Customer_service.Customer_Service();
                                        CustService.Url = NAVWebURL + "/Page/Customer";
                                        CustService.UseDefaultCredentials = true;

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

                                        if (bUpdate == false)
                                        {
                                            nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                            nmFuncs.UseDefaultCredentials = true;
                                            string strCustomerNo = string.Empty; //customerNo.= "9 CJONES" or "9 CJONES01" or "9 CJONES02" if email different 
                                            if (_CustomerData.lastname.ToString().Length >= 10)
                                            {
                                                strCustomerNo = "9" + _CustomerData.firstname.Substring(0, 1).ToUpper() + _CustomerData.lastname.Substring(0, 10).ToUpper().Replace(" ", "");
                                            }
                                            else
                                            {
                                                strCustomerNo = "9" + _CustomerData.firstname.Substring(0, 1).ToUpper() + _CustomerData.lastname.ToUpper().Replace(" ", "");
                                            }
                                            //this calls a new function in NAV that returns the customer no. if it exists based on customer email and weird 9 FNLN logic :S
                                            string strExists = nmFuncs.CheckCustomer(_CustomerData.email, strCustomerNo);
                                            if (!string.IsNullOrEmpty(strExists))
                                            {
                                                string strAppendingCustNo = nmFuncs.GetLastCustomerNo(strCustomerNo);
                                                strCustomerNo = strCustomerNo + strAppendingCustNo; //this will look like 9 CMONCK01 or 9 CMONCK02 etc.. not clear why we do this??

                                            }
                                            _NewCust.No = strCustomerNo;

                                        }


                                        _NewCust.E_Mail = _CustomerData.email;
                                        _NewCust.eCommerce_Enabled = true;
                                        _NewCust.eCommerce_EnabledSpecified = true;
                                        _NewCust.eCommerceID = _CustomerData.id.ToString();
                                        _NewCust.Type_of_Supply_Code = "Pending";
                                        _NewCust.Magento_Store_ID = _CustomerData.storeId;

                                        MagentoCustomer.FrameworkAttributeInterface[] _CustAttributes = _CustomerData.customAttributes;

                                        MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient attributesClient;

                                        EndpointAddress endAttributePointAddress = new EndpointAddress(strMagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1");
                                        using (attributesClient = new MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient(myBinding, endAttributePointAddress))
                                        {

                                            BasicHttpsBinding bAttribute = new BasicHttpsBinding();
                                            bAttribute.MaxReceivedMessageSize = 9999999;
                                            attributesClient.Endpoint.Binding = bAttribute;
                                            //new BasicHttpsBinding();                        
                                            HttpRequestMessageProperty hrmpAttribute = new HttpRequestMessageProperty();
                                            hrmpAttribute.Headers.Add("Authorization", "Bearer " + strSessionID);

                                            OperationContextScope contextScopeAttribute = new OperationContextScope(attributesClient.InnerChannel);
                                            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmpAttribute;
                                            try
                                            {
                                                MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest _AttributeReq = new MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest();

                                                //Payment Term Code??
                                                System.Xml.XmlNode[] xmlQuery;
                                                _AttributeReq.attributeCode = "payment_term_code";
                                                MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsResponse _AttributeResponse =
                                                attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);
                                                //search for the Payment term code
                                                var query = (from p in _CustAttributes
                                                             where p.attributeCode == "nav_id"
                                                             select p).FirstOrDefault();
                                                string attributeQuery = string.Empty;
                                                if (query != null)
                                                {
                                                    xmlQuery = (System.Xml.XmlNode[])query.value;

                                                    strNavCustId = xmlQuery[0].Value.ToString();
                                                }




                                                //search for the Payment term code
                                                query = (from p in _CustAttributes
                                                         where p.attributeCode == "payment_term_code"
                                                         select p).FirstOrDefault();
                                                attributeQuery = string.Empty;
                                                if (query != null)
                                                {
                                                    xmlQuery = (System.Xml.XmlNode[])query.value;

                                                    attributeQuery = (from att in _AttributeResponse.result
                                                                      where att.value == xmlQuery[0].Value.ToString()
                                                                      select att.label).FirstOrDefault();

                                                    _NewCust.Payment_Terms_Code = attributeQuery;
                                                }


                                                //search the results back of the customer attributes for credit limit
                                                query = (from p in _CustAttributes
                                                         where p.attributeCode == "credit_limit"
                                                         select p).FirstOrDefault();

                                                decimal decCreditLimit;
                                                if (query != null)
                                                {
                                                    xmlQuery = (System.Xml.XmlNode[])query.value;
                                                    decimal.TryParse(xmlQuery[0].Value, out decCreditLimit);
                                                    _NewCust.Credit_Limit_ACY = decCreditLimit;
                                                }



                                                _AttributeReq.attributeCode = "blocked";
                                                _AttributeResponse =
                                                attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);
                                                //search for the account coordinator
                                                query = (from p in _CustAttributes
                                                         where p.attributeCode == "blocked"
                                                         select p).FirstOrDefault();
                                                if (query != null)
                                                {
                                                    xmlQuery = (System.Xml.XmlNode[])query.value;
                                                    attributeQuery = (from att in _AttributeResponse.result
                                                                      where att.value == xmlQuery[0].Value.ToString()
                                                                      select att.label).FirstOrDefault();
                                                }
                                                switch (attributeQuery)
                                                {
                                                    case "":
                                                        _NewCust.Blocked = Customer_service.Blocked._blank_;
                                                        break;
                                                    case "No":
                                                        _NewCust.Blocked = Customer_service.Blocked._blank_;
                                                        break;
                                                    case "All":
                                                        _NewCust.Blocked = Customer_service.Blocked.All;
                                                        break;
                                                    case "Invoice":
                                                        _NewCust.Blocked = Customer_service.Blocked.Invoice;
                                                        break;
                                                    case "Ship":
                                                        _NewCust.Blocked = Customer_service.Blocked.Ship;
                                                        break;
                                                    case "Yes":
                                                        _NewCust.Blocked = Customer_service.Blocked.All;
                                                        break;
                                                }





                                            }


                                            catch (Exception exceptAttribute)
                                            {

                                            }
                                        }



                                        if (bUpdate)
                                        {
                                            Log.WriteInfo("updatingNAV customer : " + _NewCust.No);
                                            CustService.Update(ref _NewCust);
                                            if (strNavCustId != _NewCust.No)
                                            {
                                                Log.WriteInfo("updating magento customer : " + _NewCust.eCommerceID);
                                                updateTargetedMagentoCustomer(strMagentoURL, strUser, strPass, _NewCust.No, _NewCust.eCommerceID);
                                            }
                                        }
                                        else
                                        {
                                            Log.WriteInfo("creating nav customer : " + _NewCust.No);
                                            CustService.Create(ref _NewCust);
                                            //we need to update the magento item with the customer no. only if its a create though
                                            updateTargetedMagentoCustomer(strMagentoURL, strUser, strPass, _NewCust.No, _NewCust.eCommerceID);
                                        }

                                    }
                                }
                            }
                        }

                    }
                    catch (Exception eFailedToSave)
                    {
                        Log.WriteInfo("failed on customer update : " + eFailedToSave.Message);
                        return false;
                    }

                }


            }
            catch (Exception e)
            {
                Log.WriteInfo("failed on customer update: " + e.Message);
                return false;
            }

            return true;
        }

        public bool updateTargetedMagentoCustomer(string strMagentoURL, string strUser, string strPass, string CustNo, string strMagCustNo)
        {
            //connect to magento
            getSessionID_B2B(strMagentoURL, strUser, strPass);
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

                    MagentoCustomer.CustomerCustomerRepositoryV1GetByIdRequest _CustByIDRequest = new MagentoCustomer.CustomerCustomerRepositoryV1GetByIdRequest();
                    int intCustID;
                    if (Int32.TryParse(strMagCustNo, out intCustID))
                    {
                        _CustByIDRequest.customerId = intCustID;



                        MagentoCustomer.CustomerCustomerRepositoryV1GetByIdResponse _CustResponse = client.customerCustomerRepositoryV1GetById(_CustByIDRequest);
                        MagentoCustomer.CustomerDataCustomerInterface _MagCustomer = _CustResponse.result;


                        //this is guess work
                        MagentoCustomer.FrameworkAttributeInterface[] _lstAttributes = _MagCustomer.customAttributes;
                        List<MagentoCustomer.FrameworkAttributeInterface> _newAttributes = new List<MagentoCustomer.FrameworkAttributeInterface>();
                        int intAttributeNo = 0;

                        MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient attributesClient;
                        MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsResponse _AttributeResponse;
                        EndpointAddress endAttributePointAddress = new EndpointAddress(strMagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1");
                        MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest _AttributeReq = new MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest();
                        using (attributesClient = new MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient(myBinding, endAttributePointAddress))
                        {

                            BasicHttpsBinding bAttribute = new BasicHttpsBinding();
                            bAttribute.MaxReceivedMessageSize = 9999999;
                            attributesClient.Endpoint.Binding = bAttribute;
                            HttpRequestMessageProperty hrmpAttribute = new HttpRequestMessageProperty();
                            hrmpAttribute.Headers.Add("Authorization", "Bearer " + strSessionID);

                            OperationContextScope contextScopeAttribute = new OperationContextScope(attributesClient.InnerChannel);
                            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmpAttribute;

                            try
                            {
                                bool bFound = false;
                                foreach (MagentoCustomer.FrameworkAttributeInterface _Attribute in _lstAttributes)
                                {
                                    //Credit Available
                                    if (_Attribute.attributeCode.ToString().ToLower() == "nav_id")
                                    {
                                        _Attribute.value = CustNo;
                                        bFound = true;
                                    }

                                    _newAttributes.Add(_Attribute);

                                    //approval status
                                    if (_Attribute.attributeCode.ToString().ToLower() == "approval_status")
                                    {
                                        _AttributeReq.attributeCode = "approval_status";
                                        _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                        var attributeQuery = (from att in _AttributeResponse.result
                                                              where att.label.ToLower() == "pending"
                                                              select att.value).FirstOrDefault();

                                        if (!string.IsNullOrEmpty(attributeQuery))
                                        {
                                            System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;

                                            _blockedNode[0].Value = attributeQuery;

                                            _Attribute.value = _blockedNode; //_Attribute;
                                        }


                                        bFound = true;
                                    }

                                    _newAttributes.Add(_Attribute);
                                    intAttributeNo++;
                                }
                                if (!bFound)
                                {
                                    MagentoCustomer.FrameworkAttributeInterface _AttributeCustID = new MagentoCustomer.FrameworkAttributeInterface();
                                    _AttributeCustID.attributeCode = "nav_id";
                                    _AttributeCustID.value = CustNo;
                                    _newAttributes.Add(_AttributeCustID);
                                }

                            }
                            catch (Exception exceptAttribute)
                            {
                                Log.WriteInfo("error updating magento customer : " + CustNo + " " + exceptAttribute.Message);
                            }
                        }


                        _MagCustomer.customAttributes = _newAttributes.ToArray(); //replace the old with the new


                        //update magento
                        contextScope = new OperationContextScope(client.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                        MagentoCustomer.CustomerCustomerRepositoryV1SaveRequest _MagCustSave = new MagentoCustomer.CustomerCustomerRepositoryV1SaveRequest();



                        _MagCustSave.customer = _MagCustomer;
                        client.customerCustomerRepositoryV1Save(_MagCustSave);
                    }
                }

                catch (Exception e)
                {
                    Log.WriteInfo("error updating magento customer : " + CustNo + " " + e.Message);
                }



            }

            return true;
        }

        public bool updateMagentoCustomer(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            //connect to magento
            getSessionID_B2B(strMagentoURL, strUser, strPass);
            // Set the Magento API Endpoint                
            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;

            myBinding.Elements.Add(new HttpsTransportBindingElement());

            EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=customerCustomerRepositoryV1,customerAddressRepositoryV1");
            MagentoCustomer.customerCustomerRepositoryV1PortTypeClient client;

            NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);

            Customer_service.Customer_Service CustService = new Customer_service.Customer_Service();
            CustService.Url = NAVWebURL + "/Page/Customer";
            CustService.UseDefaultCredentials = true;

            CustService.Credentials = netCred;
            Customer_service.Customer_Filter _CustNAVFilter = new Customer_service.Customer_Filter();
            _CustNAVFilter.Field = Customer_service.Customer_Fields.Update_Magento;
            _CustNAVFilter.Criteria = "1";

            Customer_service.Customer_Filter[] lstCustomerFilters = new Customer_service.Customer_Filter[] { _CustNAVFilter };
            Customer_service.Customer[] _Customer = CustService.ReadMultiple(lstCustomerFilters, string.Empty, 99999);

            NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
            nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
            nmFuncs.UseDefaultCredentials = true;
            nmFuncs.Credentials = netCred;
            Log.WriteInfo("Updating magento customers :");
            foreach (Customer_service.Customer _Cust in _Customer)
            {

                using (client = new MagentoCustomer.customerCustomerRepositoryV1PortTypeClient(myBinding, endPointAddress))//(
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

                        MagentoCustomer.CustomerCustomerRepositoryV1GetByIdRequest _CustByIDRequest = new MagentoCustomer.CustomerCustomerRepositoryV1GetByIdRequest();
                        int intCustID;
                        if (Int32.TryParse(_Cust.eCommerceID.ToString(), out intCustID))
                        {
                            _CustByIDRequest.customerId = intCustID;


                            try
                            {

                                MagentoCustomer.CustomerCustomerRepositoryV1GetByIdResponse _CustResponse = client.customerCustomerRepositoryV1GetById(_CustByIDRequest);
                                MagentoCustomer.CustomerDataCustomerInterface _MagCustomer = _CustResponse.result;


                                //now we have the magento customer we can update the key fields
                                //need to know where the fields are

                                //this is guess work
                                MagentoCustomer.FrameworkAttributeInterface[] _lstAttributesFromMag = _MagCustomer.customAttributes;

                                int intAttributeNo = 0;

                                MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient attributesClient;
                                MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsResponse _AttributeResponse;
                                EndpointAddress endAttributePointAddress = new EndpointAddress(strMagentoURL + "services=catalogProductAttributeOptionManagementV1,sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1,catalogCategoryAttributeOptionManagementV1");
                                MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest _AttributeReq = new MagentoAttributes.SizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItemsRequest();
                                bool bRegion = false;
                                bool bShipInstructions = false;
                                using (attributesClient = new MagentoAttributes.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1PortTypeClient(myBinding, endAttributePointAddress))
                                {

                                    BasicHttpsBinding bAttribute = new BasicHttpsBinding();
                                    bAttribute.MaxReceivedMessageSize = 9999999;
                                    attributesClient.Endpoint.Binding = bAttribute;
                                    //new BasicHttpsBinding();                        
                                    HttpRequestMessageProperty hrmpAttribute = new HttpRequestMessageProperty();
                                    hrmpAttribute.Headers.Add("Authorization", "Bearer " + strSessionID);

                                    OperationContextScope contextScopeAttribute = new OperationContextScope(attributesClient.InnerChannel);
                                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmpAttribute;
                                    MagentoCustomer.FrameworkAttributeInterface[] _lstAttributes = CheckAllAttributes(_lstAttributesFromMag, _Cust);
                                    MagentoCustomer.FrameworkAttributeInterface[] _newAttributes = new MagentoCustomer.FrameworkAttributeInterface[_lstAttributes.Length];
                                    List<MagentoCustomer.FrameworkAttributeInterface> _lstNewAttributes = new List<MagentoCustomer.FrameworkAttributeInterface>();

                                    try
                                    {
                                        bool bAdded = false;
                                        foreach (MagentoCustomer.FrameworkAttributeInterface _Attribute in _lstAttributes)
                                        {
                                            bAdded = false;
                                            //Credit Available
                                            if (_Attribute.attributeCode.ToString().ToLower() == "credit_spend")
                                            {
                                                _Attribute.value = nmFuncs.CheckCustomerCreditLimit(_Cust.No).ToString();
                                                bAdded = true;
                                            }
                                            //Credit Limit
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "credit_limit")
                                            {
                                                _Attribute.value = _Cust.Credit_Limit_ACY.ToString();
                                                bAdded = true;
                                            }
                                            //approval status
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "approval_status")
                                            {
                                                if (!string.IsNullOrEmpty(_Cust.Type_of_Supply_Code))
                                                {
                                                    _AttributeReq.attributeCode = "approval_status";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);
                                                    string strTypeOfSupply = _Cust.Type_of_Supply_Code.ToLower();
                                                    if (strTypeOfSupply.ToLower() == "newly appr")
                                                    {
                                                        strTypeOfSupply = "newly approved";
                                                    }
                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.label.ToLower() == strTypeOfSupply
                                                                          select att.value).FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    {
                                                        System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;

                                                        _blockedNode[0].Value = attributeQuery;

                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                    }

                                                }
                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "customer_virtual_warehouse")
                                            {
                                                //this is a select field                                        
                                                if (!string.IsNullOrEmpty(_Cust.Location_Code))
                                                {
                                                    //virtual warehouse??

                                                    _AttributeReq.attributeCode = "customer_virtual_warehouse";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.value.ToLower() == _Cust.Location_Code.ToLower()
                                                                          select att.value).FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    {
                                                        System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                                                        System.Xml.XmlNode[] _CVWNode = new System.Xml.XmlNode[1];
                                                        _CVWNode[0] = _doc.CreateTextNode(_Cust.Location_Code);
                                                        _CVWNode[0].InnerText = _Cust.Location_Code;
                                                        _CVWNode[0].Value = attributeQuery;
                                                        _Attribute.value = _CVWNode; //_Attribute;
                                                        bAdded = true;

                                                        bAdded = true;
                                                    }

                                                }
                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "vat_status")
                                            {
                                                //this is a select field                                        
                                                if (!string.IsNullOrEmpty(_Cust.VAT_Bus_Posting_Group))
                                                {
                                                    //vat_status??
                                                    _AttributeReq.attributeCode = "vat_status";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.label == _Cust.VAT_Bus_Posting_Group.ToString()
                                                                          select att.value).FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    {
                                                        System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                                                        System.Xml.XmlNode[] _blockedNode = new System.Xml.XmlNode[1];
                                                        _blockedNode[0] = _doc.CreateTextNode(_Cust.VAT_Bus_Posting_Group.ToString());
                                                        _blockedNode[0].InnerText = _Cust.VAT_Bus_Posting_Group.ToString();
                                                        _blockedNode[0].Value = attributeQuery;
                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                    }

                                                }

                                            }
                                            /*
                                             //removed by request of Jamie - field does not exist? - 31/10/17
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "customer_price_group")
                                            {
                                                //this is a select field                                        
                                                if (!string.IsNullOrEmpty(_Cust.Customer_Price_Group))
                                                {
                                                    //customer_price_group??
                                                    _AttributeReq.attributeCode = "customer_price_group";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.label == _Cust.Customer_Price_Group.ToString()
                                                                          select att.value).FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    {
                                                        System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;

                                                        _blockedNode[0].Value = attributeQuery;

                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                    }
                                                }

                                            }
                                             * */


                                            else if (_Attribute.attributeCode.ToString().ToLower() == "account_coordinator")
                                            {
                                                //this is a select field                                        
                                                if (!string.IsNullOrEmpty(_Cust.Salesperson_Code))
                                                {

                                                    //_Attribute.value = _Cust.Salesperson_Code;
                                                    // not needed for the account coordinator

                                                    //account coordinator??
                                                    _AttributeReq.attributeCode = "account_coordinator";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);
                                                    string strLookup = "(NAV ID: " + _Cust.Salesperson_Code.ToString() + ")";
                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.value == _Cust.Salesperson_Code.ToString()
                                                                          //where att.label.Contains(strLookup)
                                                                          //where SqlMethods.Like(att.label ,"%(NAV ID: " + _Cust.Salesperson_Code.ToString() + ")")
                                                                          select att.value).FirstOrDefault();

                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    {
                                                        /*
                                                        System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;
                                                        _blockedNode[0].Value = attributeQuery;
                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                         * */
                                                        System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                                                        System.Xml.XmlNode[] _blockedNode = new System.Xml.XmlNode[1];
                                                        _blockedNode[0] = _doc.CreateTextNode(attributeQuery);
                                                        _blockedNode[0].InnerText = attributeQuery;
                                                        _blockedNode[0].Value = attributeQuery;
                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                    }
                                                    else
                                                    {
                                                        attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.label == _Attribute.value.ToString()
                                                                          select att.value).FirstOrDefault();

                                                        System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;

                                                        _blockedNode[0].Value = attributeQuery;

                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                    }



                                                }

                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "blocked")
                                            {
                                                //this is a select field                                        

                                                //account coordinator??
                                                _AttributeReq.attributeCode = "blocked";
                                                _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                                string strBlocked = string.Empty;
                                                if (_Cust.Blocked == Customer_service.Blocked.All)
                                                {
                                                    strBlocked = "Yes";
                                                }
                                                else
                                                {
                                                    strBlocked = "No";
                                                }
                                                var attributeQueryBlocked = (from att in _AttributeResponse.result
                                                                             where att.label == strBlocked
                                                                             select att.value).FirstOrDefault();
                                                if (!string.IsNullOrEmpty(attributeQueryBlocked))
                                                {
                                                    // System.Xml.XmlNode[] _blockedNode = (System.Xml.XmlNode[])_Attribute.value;
                                                    System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                                                    System.Xml.XmlNode[] _blockedNode = new System.Xml.XmlNode[1];
                                                    _blockedNode[0] = _doc.CreateTextNode(strBlocked);
                                                    _blockedNode[0].InnerText = strBlocked;
                                                    _blockedNode[0].Value = attributeQueryBlocked;
                                                    _Attribute.value = _blockedNode; //_Attribute;
                                                    bAdded = true;

                                                    //_blockedNode[0].Value = attributeQuery;

                                                    //_Attribute.value = _blockedNode; //_Attribute;
                                                }

                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "payment_term_code")
                                            {
                                                //this is a select field                                        
                                                if (!string.IsNullOrEmpty(_Cust.Payment_Terms_Code))
                                                {
                                                    //account coordinator??
                                                    _AttributeReq.attributeCode = "payment_term_code";
                                                    _AttributeResponse = attributesClient.sizzixB2bCustomerAttributeOptionsApiCustomerAttributeOptionManagementV1GetItems(_AttributeReq);

                                                    var attributeQuery = (from att in _AttributeResponse.result
                                                                          where att.label == _Cust.Payment_Terms_Code.ToString()
                                                                          select att.value).FirstOrDefault();


                                                    if (!string.IsNullOrEmpty(attributeQuery))
                                                    //catch all to prevent errors
                                                    {
                                                        //   var attributeXML = (from att in _AttributeResponse.result
                                                        //                         where att.label == "30D"
                                                        //                         select att).FirstOrDefault();
                                                        System.Xml.XmlDocument _doc = new System.Xml.XmlDocument();
                                                        System.Xml.XmlNode[] _blockedNode = new System.Xml.XmlNode[1];
                                                        _blockedNode[0] = _doc.CreateTextNode(_Cust.Payment_Terms_Code.ToString());
                                                        _blockedNode[0].InnerText = _Cust.Payment_Terms_Code.ToString();
                                                        _blockedNode[0].Value = attributeQuery;
                                                        _Attribute.value = _blockedNode; //_Attribute;
                                                        bAdded = true;
                                                    }
                                                }

                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "nav_id")
                                            {
                                                _Attribute.value = _Cust.No; ;
                                                bAdded = true;

                                            }
                                            else if (_Attribute.attributeCode.ToString().ToLower() == "region")
                                            {
                                                _Attribute.value = _Cust.Magento_Region.ToString();
                                                bAdded = true;
                                                bRegion = true;
                                            }
                                            //else if (_Attribute.attributeCode.ToString().ToLower() == "shipping_instructions")
                                            //{
                                            //    if (_Attribute.value == "")
                                            //    {
                                            //        _Attribute.value = "Ship Instructions";
                                            //        bAdded = true;
                                            //        bShipInstructions = true;
                                            //    }
                                            //}


                                            if (bAdded)
                                            {
                                                _lstNewAttributes.Add(_Attribute);
                                            }
                                        }

                                    }
                                    catch (Exception exceptAttribute)
                                    {
                                        Log.WriteInfo("Error updating magento customers :" + exceptAttribute.Message);
                                    }




                                    if (bRegion == false)
                                    {
                                        MagentoCustomer.FrameworkAttributeInterface[] _newAttributesRegion = new MagentoCustomer.FrameworkAttributeInterface[_lstAttributes.Length + 1];
                                        MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                                        _att.attributeCode = "region";
                                        _att.value = _Cust.Magento_Region.ToString();
                                        _lstNewAttributes.Add(_att);

                                    }


                                    try
                                    {
                                        MagentoCustGroupRespository.customerGroupRepositoryV1PortTypeClient clientResp;

                                        EndpointAddress endPointAddressResp = new EndpointAddress(strMagentoURL + "services=customerGroupRepositoryV1");
                                        using (clientResp = new MagentoCustGroupRespository.customerGroupRepositoryV1PortTypeClient(myBinding, endPointAddressResp))
                                        {
                                            BasicHttpsBinding b2 = new BasicHttpsBinding();
                                            b2.MaxReceivedMessageSize = 9999999;
                                            clientResp.Endpoint.Binding = b2;

                                            clientResp.Endpoint.Address = endPointAddressResp;

                                            contextScope = new OperationContextScope(clientResp.InnerChannel);
                                            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                                            MagentoCustGroupRespository.CustomerGroupRepositoryV1GetListRequest _RequestGroup = new MagentoCustGroupRespository.CustomerGroupRepositoryV1GetListRequest();

                                            _RequestGroup.searchCriteria = new MagentoCustGroupRespository.FrameworkSearchCriteriaInterface();
                                            // MagentoCustGroupRespository.FrameworkSearchCriteriaInterface _searchGroup = new MagentoCustGroupRespository.FrameworkSearchCriteriaInterface();
                                            // MagentoCustGroupRespository.FrameworkSearchFilterGroup _groupidFilterGroup = new MagentoCustGroupRespository.FrameworkSearchFilterGroup();
                                            //MagentoCustGroupRespository.FrameworkFilter _groupidFilter = new MagentoCustGroupRespository.FrameworkFilter();
                                            //_groupidFilter.field = "code";
                                            //_groupidFilter.value = _Cust.Customer_Price_Group + " " + _Cust.Customer_Disc_Group;                                        
                                            //_groupidFilter.filters =
                                            //_searchGroup.filterGroups = 
                                            MagentoCustGroupRespository.CustomerGroupRepositoryV1GetListResponse _ResponseGroup = clientResp.customerGroupRepositoryV1GetList(_RequestGroup);
                                            foreach (MagentoCustGroupRespository.CustomerDataGroupInterface _int in _ResponseGroup.result.items)
                                            {
                                                if ((_Cust.Customer_Price_Group + " " + _Cust.Customer_Disc_Group).ToLower() == _int.code.ToLower())
                                                {
                                                    //then we update the customer in Magento with _int id
                                                    _MagCustomer.groupId = _int.id;

                                                }
                                            }




                                        }
                                    }
                                    catch (Exception e2)
                                    {
                                        Log.WriteInfo("Error updating magento customers :" + e2.Message);
                                        return false;
                                    }

                                    contextScope = new OperationContextScope(client.InnerChannel);
                                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;



                                    _MagCustomer.customAttributes = _lstNewAttributes.ToArray();
                                    _MagCustomer.storeId = _Cust.Magento_Store_ID;
                                    _MagCustomer.email = _Cust.E_Mail;

                                    List<MagentoCustomer.CustomerDataAddressInterface> _lstorgCustAddress = _MagCustomer.addresses.ToList();
                                    MagentoCustomer.CustomerDataAddressInterface _custAddress = new MagentoCustomer.CustomerDataAddressInterface();
                                    List<string> _street = new List<string>();
                                    _street.Add(_Cust.Address);
                                    _custAddress.street = _street.ToArray();
                                    _custAddress.city = _Cust.City;
                                    _custAddress.postcode = _Cust.Post_Code;
                                    MagentoCustomer.CustomerDataRegionInterface _region = new MagentoCustomer.CustomerDataRegionInterface();
                                    _region.regionCode = _Cust.County;
                                    _region.region = _Cust.County;
                                    _custAddress.region = _region;
                                    _custAddress.countryId = _Cust.Country_Region_Code;
                                    if (_lstorgCustAddress.Count > 0)
                                    {
                                        _custAddress.firstname = _lstorgCustAddress[0].firstname;
                                        _custAddress.lastname = _lstorgCustAddress[0].lastname;
                                    }

                                    _custAddress.customerId = intCustID;
                                    List<MagentoCustomer.CustomerDataAddressInterface> _listAddresses = new List<MagentoCustomer.CustomerDataAddressInterface>();
                                    _listAddresses.Add(_custAddress);


                                    //_MagCustomer.addresses = _listAddresses.ToArray(); //COMMENTED OUT UNTIL WE DISCUSS



                                    //update magento
                                    contextScope = new OperationContextScope(client.InnerChannel);
                                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;
                                    MagentoCustomer.CustomerCustomerRepositoryV1SaveRequest _MagCustSave = new MagentoCustomer.CustomerCustomerRepositoryV1SaveRequest();
                                    _MagCustSave.customer = _MagCustomer;
                                    MagentoCustomer.CustomerCustomerRepositoryV1SaveResponse _Response = client.customerCustomerRepositoryV1Save(_MagCustSave);

                                    //update NAV to say we've picked up the change                        
                                    Customer_service.Customer _Cust2Update = _Cust;
                                    _Cust2Update.Update_Magento = false;
                                    CustService.Update(ref _Cust2Update);
                                }
                            }
                            catch (Exception eloadingMagCust)
                            {
                                Log.WriteInfo("Error updating magento customers :" + _Cust.No + " - Mag ID: " + _Cust.eCommerceID + " - " + eloadingMagCust.Message);
                                //we just skip tthe customer as it has a wrong id or something that isn't valid
                            }
                        }
                    }

                    catch (Exception e)
                    {
                        Log.WriteInfo("Error updating magento customers :" + e.Message);
                        return false;
                    }



                }

            }
            return true;
        }


        public MagentoCustomer.FrameworkAttributeInterface[] CheckAllAttributes(MagentoCustomer.FrameworkAttributeInterface[] _lstAttributes, Customer_service.Customer _Cust)
        {
            List<MagentoCustomer.FrameworkAttributeInterface> _return = new List<MagentoCustomer.FrameworkAttributeInterface>();
            if (_lstAttributes != null)
            {
                _return = _lstAttributes.ToList();

                var attributeQuery = (from att in _lstAttributes
                                      where att.attributeCode == "credit_spend"
                                      select att).FirstOrDefault();


                if (attributeQuery == null)
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "credit_spend";
                    _att.value = 0;
                    _return.Add(_att);
                }

                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "credit_limit"
                                  select att).FirstOrDefault();


                if (attributeQuery == null)
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "credit_limit";
                    _att.value = 0;
                    _return.Add(_att);
                }

                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "approval_status"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.Type_of_Supply_Code != ""))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "approval_status";
                    //_att.value = attributeQuery.value;
                    _return.Add(_att);
                }
                /*
                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "customer_virtual_warehouse"
                                  select att).FirstOrDefault();


                if (attributeQuery == null)
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "virtual_warehouse";
                    _return.Add(_att);
                }
                 */

                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "vat_status"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.VAT_Bus_Posting_Group != null))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "vat_status";
                    //   _att.value = attributeQuery.value;
                    _return.Add(_att);
                }
                /*
                 // removed by request of Jamie - field does not exist 31/10/17 
                if (_Cust.Customer_Price_Group != null)
                {
                    attributeQuery = (from att in _lstAttributes
                                      where att.attributeCode == "customer_price_group"
                                      select att).FirstOrDefault();


                    if (attributeQuery == null)
                    {
                        //then we need to add the attribute to the list
                        MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                        _att.attributeCode = "customer_price_group";
                        _return.Add(_att);
                    }
                }
                 * */

                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "account_coordinator"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.Salesperson_Code != ""))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "account_coordinator";
                    // _att.value = attributeQuery.value;
                    _return.Add(_att);
                }

                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "blocked"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.Blocked != Customer_service.Blocked._blank_))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "blocked";
                    // _att.value = attributeQuery.value;
                    _return.Add(_att);
                }
                if ((_Cust.Payment_Terms_Code != null))
                {
                    attributeQuery = (from att in _lstAttributes
                                      where att.attributeCode == "payment_term_code"
                                      select att).FirstOrDefault();


                    if (attributeQuery == null)
                    {
                        //then we need to add the attribute to the list
                        MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                        _att.attributeCode = "payment_term_code";
                        // _att.value = attributeQuery.value;
                        _return.Add(_att);
                    }
                }
                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "nav_id"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.No != ""))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "nav_id";
                    // _att.value = attributeQuery.value;
                    _return.Add(_att);
                }
                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "region"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.Magento_Region != ""))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "region";
                    //  _att.value = attributeQuery.value;
                    _return.Add(_att);
                }


                attributeQuery = (from att in _lstAttributes
                                  where att.attributeCode == "customer_virtual_warehouse"
                                  select att).FirstOrDefault();


                if ((attributeQuery == null) && (_Cust.Location_Code != ""))
                {
                    //then we need to add the attribute to the list
                    MagentoCustomer.FrameworkAttributeInterface _att = new MagentoCustomer.FrameworkAttributeInterface();
                    _att.attributeCode = "customer_virtual_warehouse";
                    //  _att.value = attributeQuery.value;
                    _return.Add(_att);
                }
            }

            return _return.ToArray();
        }

        public bool UploadAccountCoordinator(string strMagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {

            //connect to magento
            getSessionID_B2B(strMagentoURL, strUser, strPass);
            // Set the Magento API Endpoint                
            CustomBinding myBinding = new CustomBinding();
            myBinding.Name = "CustomBinding1";

            TextMessageEncodingBindingElement tmebe = new TextMessageEncodingBindingElement();
            tmebe.MessageVersion = MessageVersion.Soap12;

            myBinding.Elements.Add(new HttpsTransportBindingElement());

            EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=sizzixB2bAccountMessagesAccountCoordinatorServiceV1");
            Log.WriteInfo("Updating magento Account Coordinators");
            MagentoAccountCoordinatorB2B.sizzixB2bAccountMessagesAccountCoordinatorServiceV1PortTypeClient client;
            using (client = new MagentoAccountCoordinatorB2B.sizzixB2bAccountMessagesAccountCoordinatorServiceV1PortTypeClient(myBinding, endPointAddress))//(
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
                    //loop around all salespersons that aren't uploaded
                    string strImage = string.Empty;
                    NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
                    NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
                    nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                    nmFuncs.UseDefaultCredentials = true;
                    nmFuncs.Credentials = netCred;

                    Salespersons.Salespersons_Service _SalesPersons = new Salespersons.Salespersons_Service();
                    _SalesPersons.Url = NAVWebURL + "/Page/Salespersons";
                    _SalesPersons.UseDefaultCredentials = true;
                    _SalesPersons.Credentials = netCred;

                    Salespersons.Salespersons_Filter _SalespersonFilter = new Salespersons.Salespersons_Filter();
                    _SalespersonFilter.Criteria = "Yes";
                    _SalespersonFilter.Field = Salespersons.Salespersons_Fields.Update_Magento;
                    Salespersons.Salespersons_Filter[] _LstSalespersonFilter = new Salespersons.Salespersons_Filter[] { _SalespersonFilter };
                    Salespersons.Salespersons[] _LstSalesperons = _SalesPersons.ReadMultiple(_LstSalespersonFilter, "", 0);
                    bool bAllInMagento = true;
                    if (_LstSalesperons != null)
                    {

                        foreach (Salespersons.Salespersons sp in _LstSalesperons)
                        {
                            //create in Magento


                            MagentoAccountCoordinatorB2B.SizzixB2bAccountMessagesAccountCoordinatorServiceV1SaveRequest _saveAccountCoordinator = new MagentoAccountCoordinatorB2B.SizzixB2bAccountMessagesAccountCoordinatorServiceV1SaveRequest();
                            MagentoAccountCoordinatorB2B.SizzixB2bAccountMessagesDataAccountCoordinatorInterface _AccountInterface = new MagentoAccountCoordinatorB2B.SizzixB2bAccountMessagesDataAccountCoordinatorInterface();



                            _AccountInterface.name = sp.Name;
                            _AccountInterface.navId = sp.Code;
                            nmFuncs.GetBase64SalespersonImage(sp.Code, ref strImage);
                            if (!string.IsNullOrEmpty(strImage))
                            {
                                _AccountInterface.base64ImageString = strImage;
                            }
                            else
                            {
                                _AccountInterface.base64ImageString = ConfigurationManager.AppSettings["DefaultImage"].ToString();
                            }


                            _saveAccountCoordinator.accountCoordinator = _AccountInterface;
                            MagentoAccountCoordinatorB2B.SizzixB2bAccountMessagesAccountCoordinatorServiceV1SaveResponse _Response = client.sizzixB2bAccountMessagesAccountCoordinatorServiceV1Save(_saveAccountCoordinator);
                            if (!_Response.result)
                            {
                                bAllInMagento = false;
                            }
                            strImage = string.Empty;

                        }


                        if (bAllInMagento)
                        {
                            nmFuncs.MarkAllSalespersonsInMagento();
                        }
                    }

                }
                catch (Exception e)
                {
                    Log.WriteInfo("Error Updating magento Account Coordinators: " + e.Message
                        );
                    return false; //just to let NAV know its not uploaded
                }
            }
            return true;
        }

        public string DownloadOrders(string strMagentoURL, string strUser, string strPass, string NAVWebURL, bool DownloadProcessingStatusOrders, string NAVCusAcc, string WHSupplyLocationCode, string strDefaultCustomer, string DiscountGLAccount, string strUsername, string strPassword, string strDomain, bool bUpdateMagentoOrders, string strNewOrders, string strInProgressOrders)
        {
            // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);           
            MagentoSales.salesOrderRepositoryV1PortTypeClient client;
            getSessionID_B2B(strMagentoURL, strUser, strPass);
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
                MagentoSales.SalesDataOrderItemExtensionInterface _salesOrderItemExtensionInterface = new MagentoSales.SalesDataOrderItemExtensionInterface();



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

                if (soe.Length > 0)
                {
                    Log.WriteInfo("Downloading Orders: " + soe.Length.ToString());
                    foreach (MagentoSales.SalesDataOrderInterface MagentoSalesOrder in soe)
                    {
                        try
                        {

                            MagentoSales.SalesOrderRepositoryV1GetRequest _getSORequest = new MagentoSales.SalesOrderRepositoryV1GetRequest();
                            _getSORequest.id = MagentoSalesOrder.entityId;

                            MagentoSales.SalesOrderRepositoryV1GetResponse _SOResponse = client.salesOrderRepositoryV1Get(_getSORequest);


                            if (MagentoSalesOrder.items.Length > 0)
                            {

                                MagentoSales.SalesDataShippingAssignmentInterface[] salesData = MagentoSalesOrder.extensionAttributes.shippingAssignments;


                                //string jsonString = Newtonsoft.Json.JsonConvert.SerializeObject(MagentoSalesOrder.extensionAttributes);

                                //string json = new Newtonsoft.Json.JsonConvert.SerializeObject().Serialize(MagentoSalesOrder.extensionAttributes);


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
                                    Log.WriteInfo("Error downloading orders: " + newSOEx.Message);
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
                                newNAVSOrder.External_Document_No = "Sizzix:" + MagentoSalesOrder.incrementId;
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
                                        //newNAVSOrder.Sell_to_Customer_No = nmFuncs.CheckCustomer(MagentoSalesOrder.customerEmail, strCustomerNo);
                                        //changed how NAV checks if the customer exists, it checks the eCommcerceID for the magento id


                                        newNAVSOrder.Sell_to_Customer_No = nmFuncs.CheckSizzixCustomer(MagentoSalesOrder.customerEmail, MagentoSalesOrder.customerId.ToString());

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
                                            }
                                            catch (Exception custEx)
                                            {
                                                Log.WriteInfo("Error downloading orders, creating new customer: " + custEx.Message);
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
                                                    //MagentoSalesOrder.billingAddress.street[0].Substring(0, 50);
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

                                        }
                                        else
                                        {

                                            //this means we need to update the customer rather than insert a new customer
                                            Customer_service.Customer updateCustomer = new Customer_service.Customer();

                                            try
                                            {
                                                updateCustomer = CustService.Read(newNAVSOrder.Sell_to_Customer_No);
                                            }
                                            catch (Exception custEx)
                                            {
                                                Log.WriteInfo("Error downloading orders, reading customers : " + custEx.Message);
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
                                            }
                                            catch (Exception e)
                                            {
                                                Log.WriteInfo("Error downloading orders, unable to update customer: " + e.Message);
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
                                        Log.WriteInfo("Error downloading orders, inserting blank order lines: " + slEx.Message);
                                        return ("Error inserting blank order lines onto order");
                                    }

                                }
                                MagentoSales.SalesDataOrderExtensionInterface _MagentoSOAttributes = MagentoSalesOrder.extensionAttributes;
                                //get shipping instructions and PO
                                //limit the character length for shipping instructions
                                if (MagentoSalesOrder.extensionAttributes.shippingInstructions.Length > 50)
                                {
                                    newNAVSOrder.Magento_Shipping_Instructions = MagentoSalesOrder.extensionAttributes.shippingInstructions.Substring(0, 49);
                                }
                                else
                                {

                                    newNAVSOrder.Magento_Shipping_Instructions = MagentoSalesOrder.extensionAttributes.shippingInstructions;
                                }

                                if (MagentoSalesOrder.extensionAttributes.shippingInstructions.Length > 250)
                                {
                                    newNAVSOrder.Shipping_Instructions = MagentoSalesOrder.extensionAttributes.shippingInstructions.Substring(0, 249);
                                }
                                else
                                {

                                    newNAVSOrder.Shipping_Instructions = MagentoSalesOrder.extensionAttributes.shippingInstructions;
                                }

                                newNAVSOrder.Magento_Purchase_Order = MagentoSalesOrder.extensionAttributes.purchaseOrderNumber;



                                // Update the order 
                                try
                                {
                                    SOService.Update(ref newNAVSOrder);
                                }
                                catch (Exception soUpdateEx)
                                {
                                    Log.WriteInfo("Error downloading orders, unable to update nav order: " + newNAVSOrder.No + " - " + soUpdateEx.Message);
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
                                    ///MagentoSalesOrder.items[idx].extensionAttributes
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
                                            _salesOrderItemExtensionInterface = MagentoSalesOrder.items[idx].extensionAttributes;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Magento_Pre_Orders = _salesOrderItemExtensionInterface.preorderRef;
                                            //CM 31/10/17 - pre_order_ref

                                            //newNAVSOrder.SalesLines[SalesOrderIDX].Comments = soe[1].items[1].pre
                                            //   MagentoSalesOrder.items[idx].pre


                                            SOService.Update(ref newNAVSOrder); //we need to update here as we need the assembly no. back
                                            intLineNo = newNAVSOrder.SalesLines[SalesOrderIDX].Line_No;
                                            strSalesOrderNo = newNAVSOrder.SalesLines[SalesOrderIDX].Document_No;
                                        }
                                        catch (Exception ex1)
                                        {
                                            Log.WriteInfo("Error downloading orders: " + ex1.Message);
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
                                            Log.WriteInfo("Error downloading orders, error inserting blank lines onto  order: " + slEx.Message);

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
                                            Log.WriteInfo("Error downloading orders, inserting blank lines onto order: " + slEx.Message);

                                            return ("Error inserting blank order lines onto order");
                                        }
                                    }
                                }


                                try
                                {
                                    SOService.Update(ref newNAVSOrder);


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

                                            //  UpdateOrderToArrivedInNAV(strMagentoURL, strUser, strPass, MagentoSalesOrder.incrementId, strInProgressOrders, "Order Accepted into NAV", MagentoSalesOrder.entityId);
                                            try
                                            {
                                                MagentoSales.salesOrderManagementV1PortTypeClient client3;
                                                using (client3 = new MagentoSales.salesOrderManagementV1PortTypeClient(myBinding, endPointAddress))
                                                {
                                                    // client.Endpoint.Binding = new BasicHttpsBinding();                    
                                                    timeout = new TimeSpan(0, 5, 0);
                                                    myBinding.SendTimeout = timeout;

                                                    //BasicHttpBinding();
                                                    hrmp = new HttpRequestMessageProperty();
                                                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                                                    client3.Endpoint.Address = endPointAddress;


                                                    //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                                                    contextScope = new OperationContextScope(client3.InnerChannel);
                                                    OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

                                                    MagentoSales.SalesOrderManagementV1AddCommentRequest commentSubmit = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                                                    commentSubmit.id = MagentoSalesOrder.entityId;
                                                    MagentoSales.SalesDataOrderStatusHistoryInterface _CommetInterface = new MagentoSales.SalesDataOrderStatusHistoryInterface();
                                                    _CommetInterface.comment = "Order Accepted into NAV";//"test";
                                                    _CommetInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                                                    _CommetInterface.status = strInProgressOrders; //"processing";
                                                    commentSubmit.statusHistory = _CommetInterface;
                                                    MagentoSales.SalesOrderManagementV1AddCommentResponse commentResponse = client3.salesOrderManagementV1AddComment(commentSubmit);


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
                                                Log.WriteInfo("Error downloading orders, updating Magento order status: " + e2.Message);

                                                return ("Errored during updating Magento Order Status");
                                            }

                                            contextScope = new OperationContextScope(client.InnerChannel);
                                            OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;

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
                                        Log.WriteInfo("Error downloading orders: " + exl2.Message);

                                        return ("Couldn't update order status for Magento orderID " + MagentoSalesOrder.incrementId.ToString() + "     " + exl2.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Log.WriteInfo("Error downloading orders: " + ex.Message);

                                    return ("Can't update order " + newNAVSOrder.No + "               " + ex.ToString());
                                }


                            }
                        }
                        catch (Exception e)
                        {
                            //do something
                            Log.WriteInfo("Error downloading orders: " + e.Message);

                            return "Error processing sales orders: order:  " + e.Message.ToString();
                        }
                        //} //removed check for Increment id
                        //} //MOD002
                    }
                }
                return "Success";
            }
        }

        public bool UpdateOrderToShipped(string strMagentoURL, string strUser, string strPass, string strIncrementID, string strShippedStatus, string strComment, string strTrackingNumber, string strCarrierCode)
        {
            int intEntityId;
            int intCustId;
            int intBillingId;
            int intOrderId;

            MagentoSales.SalesDataOrderItemInterface[] _items;

            // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
            MagentoSales.salesOrderManagementV1PortTypeClient client;
            MagentoSales.salesOrderRepositoryV1PortTypeClient client2;

            getSessionID_B2B(strMagentoURL, strUser, strPass);
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
                MagentoSales.SalesDataOrderInterface soe = SOResponses.result.items[0];

                try
                {
                    intEntityId = SOResponses.result.items[0].entityId;
                    intBillingId = SOResponses.result.items[0].billingAddressId;
                    intCustId = SOResponses.result.items[0].customerId;
                    _items = SOResponses.result.items[0].items;
                    MagentoSales.SalesDataOrderItemInterface[] _SDOIT = soe.items;

                    intOrderId = _SDOIT[0].orderId;


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


                    MagentoSales.SalesOrderManagementV1AddCommentRequest commentSubmit = new MagentoSales.SalesOrderManagementV1AddCommentRequest();
                    commentSubmit.id = intEntityId;
                    MagentoSales.SalesDataOrderStatusHistoryInterface _CommetInterface = new MagentoSales.SalesDataOrderStatusHistoryInterface();
                    _CommetInterface.comment = strComment;//"test";
                    _CommetInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                    _CommetInterface.status = strShippedStatus; //"processing";
                    commentSubmit.statusHistory = _CommetInterface;
                    MagentoSales.SalesOrderManagementV1AddCommentResponse commentResponse = client.salesOrderManagementV1AddComment(commentSubmit);


                    UpdateOrderTracking(strMagentoURL, strUser, strPass, strTrackingNumber, strCarrierCode, strComment, intBillingId, intCustId, strIncrementID, _items, intOrderId);

                }
            }
            catch (Exception e2)
            {
                return false;
            }

            return true;

        }

        public bool UpdateOrderTracking(string strMagentoURL, string strUser, string strPass, string strTrackingNumber, string strCarrierCode, string strComment, int intBillingID, int intCustID, string strIncrementID, MagentoSales.SalesDataOrderItemInterface[] _items, int intOrderId)
        {

            int intEntityId;
            int parentId;
            MagentoSalesShipmentTracking.salesShipmentRepositoryV1PortTypeClient client1;
            MagentoSalesShipmentTracking.salesShipmentTrackRepositoryV1PortTypeClient client2;

            getSessionID_B2B(strMagentoURL, strUser, strPass);
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
            EndpointAddress endPointAddress = new EndpointAddress(strMagentoURL + "services=salesShipmentRepositoryV1,salesShipmentManagementV1,salesShipmentTrackRepositoryV1");

            using (client1 = new MagentoSalesShipmentTracking.salesShipmentRepositoryV1PortTypeClient(myBinding, endPointAddress))
            {

                // client.Endpoint.Binding = new BasicHttpsBinding();                    
                System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                myBinding.SendTimeout = timeout;

                //BasicHttpBinding();
                HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                client1.Endpoint.Address = endPointAddress;


                //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                OperationContextScope contextScope = new OperationContextScope(client1.InnerChannel);
                OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;



                //----------------------- Create new orders for pending items in Magento ---------------------------------------
                //v2.01f restrict the filter to orders with a status of Pending   
                //  MagentoSales.salesSH
                //  MagentoSalesShipmentTracking.SalesShipmentTrackRepositoryV1Saveequest salesShipmentRequest = new MagentoSalesShipmentTracking.SalesShipmentTrackRepositoryV1SaveRequest();
                MagentoSalesShipmentTracking.SalesShipmentRepositoryV1SaveRequest _SalesShipmentSaveRequest = new MagentoSalesShipmentTracking.SalesShipmentRepositoryV1SaveRequest();
                MagentoSalesShipmentTracking.SalesDataShipmentInterface _SalesDataInterface = new MagentoSalesShipmentTracking.SalesDataShipmentInterface();
                _SalesDataInterface.billingAddressId = intBillingID;
                _SalesDataInterface.billingAddressIdSpecified = true;
                // _SalesDataInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                _SalesDataInterface.customerId = intCustID;
                _SalesDataInterface.customerIdSpecified = true;
                //_SalesDataInterface.incrementId = strIncrementID;
                List<MagentoSalesShipmentTracking.SalesDataShipmentItemInterface> lstitems = new List<MagentoSalesShipmentTracking.SalesDataShipmentItemInterface>();
                MagentoSalesShipmentTracking.SalesDataShipmentItemInterface _shipLine = new MagentoSalesShipmentTracking.SalesDataShipmentItemInterface();
                foreach (MagentoSales.SalesDataOrderItemInterface _item in _items)
                {
                    _shipLine = new MagentoSalesShipmentTracking.SalesDataShipmentItemInterface();
                    _shipLine.description = _item.description;
                    _shipLine.name = _item.name;
                    _shipLine.qty = _item.qtyOrdered;
                    _shipLine.sku = _item.sku;
                    _shipLine.orderItemId = _item.itemId;


                    lstitems.Add(_shipLine);

                }
                _SalesDataInterface.items = lstitems.ToArray();
                _SalesDataInterface.shippingAddressId = intBillingID;
                _SalesDataInterface.shippingAddressIdSpecified = true;
                _SalesDataInterface.orderId = intOrderId;

                _SalesShipmentSaveRequest.entity = _SalesDataInterface;





                MagentoSales.SalesDataShippingExtensionInterface salesShip = new MagentoSales.SalesDataShippingExtensionInterface();

                //
                MagentoSalesShipmentTracking.SalesShipmentRepositoryV1SaveResponse _saveResponse = client1.salesShipmentRepositoryV1Save(_SalesShipmentSaveRequest);
                try
                {

                    //intEntityId = _saveResponse.result.entityId;
                    //parentId = _saveResponse.result.orderId;
                    parentId = _saveResponse.result.entityId;

                    using (client2 = new MagentoSalesShipmentTracking.salesShipmentTrackRepositoryV1PortTypeClient(myBinding, endPointAddress))
                    {

                        // client.Endpoint.Binding = new BasicHttpsBinding();                    
                        timeout = new TimeSpan(0, 5, 0);
                        myBinding.SendTimeout = timeout;

                        //BasicHttpBinding();
                        hrmp = new HttpRequestMessageProperty();
                        hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                        client2.Endpoint.Address = endPointAddress;


                        //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                        contextScope = new OperationContextScope(client2.InnerChannel);
                        OperationContext.Current.OutgoingMessageProperties[HttpRequestMessageProperty.Name] = hrmp;



                        //----------------------- Create new orders for pending items in Magento ---------------------------------------
                        //v2.01f restrict the filter to orders with a status of Pending               
                        MagentoSalesShipmentTracking.SalesShipmentTrackRepositoryV1SaveRequest salesShipmentRequest = new MagentoSalesShipmentTracking.SalesShipmentTrackRepositoryV1SaveRequest();
                        MagentoSalesShipmentTracking.SalesDataShipmentTrackInterface _SalesDataTrackInterface = new MagentoSalesShipmentTracking.SalesDataShipmentTrackInterface();
                        _SalesDataTrackInterface.carrierCode = strCarrierCode;
                        _SalesDataTrackInterface.createdAt = System.DateTime.Now.ToString("MM/dd/yyyy HH:mm:ss");
                        _SalesDataTrackInterface.description = strComment;
                        //_SalesDataTrackInterface.entityId = intEntityId;
                        _SalesDataTrackInterface.entityIdSpecified = true;
                        _SalesDataTrackInterface.orderId = intOrderId;
                        _SalesDataTrackInterface.trackNumber = strTrackingNumber;
                        _SalesDataTrackInterface.weight = 1;
                        _SalesDataTrackInterface.qty = 1;
                        _SalesDataTrackInterface.title = strCarrierCode + " " + strTrackingNumber;
                        _SalesDataTrackInterface.parentId = parentId;



                        salesShipmentRequest.entity = _SalesDataTrackInterface;

                        //
                        MagentoSalesShipmentTracking.SalesShipmentTrackRepositoryV1SaveResponse _saveResponseTrack = client2.salesShipmentTrackRepositoryV1Save(salesShipmentRequest);
                        try
                        {

                            intEntityId = _saveResponse.result.entityId;

                        }
                        catch (Exception e)
                        {
                            return true; //just to get it off NAVs books
                        }
                    }

                }
                catch (Exception e)
                {
                    return true; //just to get it off NAVs books
                }
            }


            return true;

        }

        public bool UpdateOrderToArrivedInNAV(string strMagentoURL, string strUser, string strPass, string strIncrementID, string strShippedStatus, string strComment, int intEntityId)
        {
            MagentoSales.salesOrderManagementV1PortTypeClient client3;
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

            /*
            // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
            MagentoSales.salesOrderManagementV1PortTypeClient client3;
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
             * */

            try
            {
                using (client3 = new MagentoSales.salesOrderManagementV1PortTypeClient(myBinding, endPointAddress))
                {
                    // client.Endpoint.Binding = new BasicHttpsBinding();                    
                    System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                    myBinding.SendTimeout = timeout;

                    //BasicHttpBinding();
                    HttpRequestMessageProperty hrmp = new HttpRequestMessageProperty();
                    hrmp.Headers.Add("Authorization", "Bearer " + strSessionID);
                    client3.Endpoint.Address = endPointAddress;


                    //new EndpointAddress(strMagentoURL + "services=salesOrderManagementV1,salesOrderRepositoryV1,salesOrderManagementV1");

                    OperationContextScope contextScope = new OperationContextScope(client3.InnerChannel);
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
                    MagentoSales.SalesOrderManagementV1AddCommentResponse commentResponse = client3.salesOrderManagementV1AddComment(commentSubmit);


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

        public bool uploadInventoryLevels(string MagentoURL, string strUser, string strPass, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try
            {
                MagentoProductSR.catalogProductRepositoryV1PortTypeClient client;
                getSessionID(MagentoURL, strUser, strPass);

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










