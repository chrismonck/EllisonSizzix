using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
//CM 24/10/16 - TO KEEP TRACK OF CHANGES, I'M PUTTING COMMENTS AT THE TOP TO MATCH NAV
//MOD001 AZ CM 241016 - change to the status of new orders, allowed be chosen in NAV
//MOD002 AZ CM 241016 - commented out the check to download only processing orders, now downloads any order of a given status
//MOD003 AZ CM 241016 - update orders to given status
namespace Magento_NAV_Integration
{
    public class MagentoHelper
    {
        public MagentoHelper()
        {


        }

        public string getSessionID(string MagentoURL, string MagentoAPIUser, string MagentoAPIKey)
        {
            string mLogin = string.Empty;
            /*
             <endpoint address="http://www.moonclimbing.com/index.php/api/v2_soap/index/"
                binding="basicHttpBinding" bindingConfiguration="Mage_Api_Model_Server_Wsi_HandlerBinding"
                contract="MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortType"
                name="Mage_Api_Model_Server_Wsi_HandlerPort" />
            */
            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.Name = "BasicHttpBinding_IService1";
            myBinding.Security.Mode = BasicHttpSecurityMode.None;
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            //EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
            //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
            MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

            MagentoAPIv2.loginResponse loginResponse = new MagentoAPIv2.loginResponse();



            // 2.01d CS AMended so it uses the Magento API details stored in Registry
            //Dim loginRequest As New MagentoAPIv2.loginRequest("NAVConnect", "NAVConnect")
            MagentoAPIv2.loginRequest loginRequest = new MagentoAPIv2.loginRequest(MagentoAPIUser, MagentoAPIKey);
            try
            {
                mLogin = magev2.login(MagentoAPIUser, MagentoAPIKey);

            }
            catch (Exception exc)
            {

                mLogin = exc.InnerException.ToString();
            }
            return mLogin;



        }

        public MagentoAPIv2.catalogProductEntity[] ResyncProdIDsToNAVToolStripMenuItem(string strSessionID, string MagentoURL, ref int intCount)
        {
            try
            {
                // Set the Magento API Endpoint
                //Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
                BasicHttpBinding myBinding = new BasicHttpBinding();
                myBinding.Name = "BasicHttpBinding_IService1";
                myBinding.Security.Mode = BasicHttpSecurityMode.None;
                myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                myBinding.MaxReceivedMessageSize = 999999;
                EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
                //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
                MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);


                MagentoAPIv2.filters mf = new MagentoAPIv2.filters();
                MagentoAPIv2.catalogProductListRequest ProductListRequest = new MagentoAPIv2.catalogProductListRequest(strSessionID, mf, "");
                MagentoAPIv2.catalogProductEntity[] ItemEntityList = magev2.catalogProductList(strSessionID, mf, "");
                intCount = ItemEntityList.Length;
                return ItemEntityList;
            }
            catch (Exception exc)
            {
                MagentoAPIv2.catalogProductEntity[] ItemEntityList = null;
                intCount = 0;
                return ItemEntityList;
            }
        }
        public MagentoAPIv2.catalogProductEntity getItem(MagentoAPIv2.catalogProductEntity[] lstItems, int itemNo)
        {
            MagentoAPIv2.catalogProductEntity item = new MagentoAPIv2.catalogProductEntity();
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

        public void UploadItems(string strSessionID, string MagentoURL, string ItemNo, string ItemDescription,
            decimal ItemUnitPrice, string ItemLastDateModified, string ItemMetaDescription, string ItemMetaTitle, string ItemMetaKeywords,
            decimal ItemNetWeight, string ItemeCommerceID, string ItemVATProdPostingGroup, string ItemEcommerceProductType, decimal decWeight,
            ref int intEcommerceID, ref string strError)
        {
            try
            {   // Set the Magento API Endpoint
                BasicHttpBinding myBinding = new BasicHttpBinding();
                myBinding.Name = "BasicHttpBinding_IService1";
                myBinding.Security.Mode = BasicHttpSecurityMode.None;
                myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                myBinding.MaxReceivedMessageSize = 999999;
                EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
                //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
                MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

                //Define the new Magento Product Record
                MagentoAPIv2.catalogProductCreateEntity ProductRecord = new MagentoAPIv2.catalogProductCreateEntity();
                string ProductSKU, ProductID, ProductType, ProductAttrSet, Store;
                ProductType = "simple";
                ProductAttrSet = "4";
                Store = "";

                ProductID = ItemeCommerceID;
                ProductRecord.meta_description = ItemMetaDescription; //This is 255 chars
                ProductRecord.meta_keyword = ItemMetaKeywords;
                ProductRecord.meta_title = ItemMetaTitle;
                ProductRecord.name = ItemDescription;
                ProductRecord.price = ItemUnitPrice.ToString();  //deciaml???
                //If product ID is empty then we are creating a record so put in a description
                if (string.IsNullOrEmpty(ProductID))
                {
                    ProductRecord.description = ItemDescription;
                    ProductRecord.short_description = ItemDescription;
                }

                // v2.01f If it is not a template item, then mark it as a simple product

                //if (sdr.GetInt32(10) = 2) {
                //    ProductType = "configurable";
                //}
                if (ItemEcommerceProductType == "Configurable")
                {

                    ProductType = "configurable";
                }


                ProductRecord.weight = ItemNetWeight.ToString(); //decimal???
                ProductSKU = ItemNo;
                ProductRecord.status = "1";
                ProductRecord.visibility = "4"; //4 = Catalog,Search

                //V2.02c Original VAT treatment didn't work in Magento 1.8.1.0.   Values are 0=None, 1=Invalid, 2=Taxable, 3=Invalid
                //Changed so that if the VAT Prod Posting group is "NO VAT" then it will set to None.  Otherwise it will
                //set to taxable
                ProductRecord.tax_class_id = "2";  //2=Taxable  
                if (ItemVATProdPostingGroup == "NO VAT")
                {
                    ProductRecord.tax_class_id = "0";  //0=None
                }

                ProductRecord.weight = decWeight.ToString();
                //MagentoAPIv2.catalogProductCreateRequest ProductCreateRequest = new MagentoAPIv2.catalogProductCreateRequest(strSessionID, ProductType, ProductAttrSet, ProductSKU, ProductRecord, Store);

                // MagentoAPIv2.catalogProductCreateResponse ProductCreateResponse = new MagentoAPIv2.catalogProductCreateResponse();
                // MagentoAPIv2.catalogProductUpdateRequest ProductUpdateRequest = new MagentoAPIv2.catalogProductUpdateRequest(strSessionID, ProductID, ProductRecord, "", "");
                // MagentoAPIv2.catalogProductUpdateResponse ProductUpdateResponse = new MagentoAPIv2.catalogProductUpdateResponse();
                int RecordsUpdated = 0;
                int RecordsFailed = 0;
                int RecordsInserted = 0;
                intEcommerceID = 0;
                strError = string.Empty;
                // If NAV Item already has a MagentoID then try to update the record.  
                // If it doesn't then create a new record and store the ID in NAV
                if (!string.IsNullOrEmpty(ProductID))
                {
                    //Update the record
                    try
                    {
                        magev2.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");
                        RecordsUpdated = RecordsUpdated + 1;
                    }
                    catch (Exception e)
                    {
                        //If it fails, try again because Magento often returns a comms error
                        try
                        {
                            magev2.catalogProductUpdate(strSessionID, ProductID, ProductRecord, "", "");

                            RecordsUpdated = RecordsUpdated + 1;
                        }
                        catch (Exception ex2)
                        {
                            RecordsFailed = RecordsFailed + 1;
                            strError = strError + "Failed: item no: " + ProductID + " ";
                        }
                    }
                }
                else
                {
                    // insert the record and update the NAV item with the ID
                    try
                    {
                        //ProductCreateResponse = magev2.catalogProductCreate(ProductCreateRequest)
                        intEcommerceID = magev2.catalogProductCreate(strSessionID, ProductType, ProductAttrSet, ProductSKU, ProductRecord, Store);
                        RecordsInserted = RecordsInserted + 1;
                    }
                    catch (Exception ex)
                    {
                        //Magento API often returns a communication error so have a 2nd try if it fails
                        try
                        {
                            intEcommerceID = magev2.catalogProductCreate(strSessionID, ProductType, ProductAttrSet, ProductSKU, ProductRecord, Store);
                            RecordsInserted = RecordsInserted + 1;
                        }
                        catch (Exception exc2)
                        {
                            // if it fails, try to update the record
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


        public bool UpdateItem(string strSessionID, string MagentoURL, string ItemNo, string intEcommerceID, string strDescription, decimal decWeight)
        {
            try
            {
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
                MagentoAPIv2.catalogProductCreateEntity product = new MagentoAPIv2.catalogProductCreateEntity();
                product.meta_description = strDescription;
                product.description = strDescription;
                product.short_description = strDescription;
                product.name = strDescription;
                product.weight = decWeight.ToString();
                magev2.catalogProductUpdate(strSessionID, intEcommerceID.ToString(), product, "", "");

                //int intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, "150", itemUpdate);                    
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }

        }

        public void UploadCustomers(string strSessionID, string MagentoURL,
            string CustomerNo, string CustomerName, string CustomerAddress, string CustomerAddress2, string CustomerCity,
            string CustomerContact, string CustomerPhoneNo, string CustomerPostCode, string CustomerCounty,
            string CustomerE_Mail, string CustomeFaxNo, string CustomerCountry, string CustomereCommerceID, string CustomereCommerceAddressID,
            ref int intEcommerceID, ref int intEcommerceAddressID, ref string strError)
        {
            try
            {   // Set the Magento API Endpoint
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

        #region olddownloadorders
        /*
        public string DownloadOrders(string strSessionID, string MagentoURL, string NAVWebURL, bool DownloadProcessingStatusOrders, string NAVCusAcc, string WHSupplyLocationCode, string strDefaultCustomer, string DiscountGLAccount, string strUsername, string strPassword, string strDomain)
        {

            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.Name = "BasicHttpBinding_IService1";
            myBinding.Security.Mode = BasicHttpSecurityMode.None;
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            myBinding.MaxReceivedMessageSize = 999999;
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
            MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);



            //----------------------- Create new orders for pending items in Magento ---------------------------------------

            // 2.02b Define a tax rate %
            int tax_rate = 0;

            //v2.01f restrict the filter to orders with a status of Pending
            MagentoAPIv2.filters mf = new MagentoAPIv2.filters();
            MagentoAPIv2.complexFilter complexFilter = new MagentoAPIv2.complexFilter();
            complexFilter.key = "Status";
            MagentoAPIv2.associativeEntity assEnt = new MagentoAPIv2.associativeEntity();
            assEnt.key = "status";
            assEnt.value = "pending";


            //v2.02b If we want to download processing orders, then change the filter
            if (DownloadProcessingStatusOrders == true)
            {
                assEnt.value = "processing";
            }

            complexFilter.value = assEnt;
            MagentoAPIv2.complexFilter[] lstComplexFilter = new MagentoAPIv2.complexFilter[1];
            lstComplexFilter[0] = complexFilter;
            mf.complex_filter = lstComplexFilter;

            NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
            NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
            nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
            nmFuncs.UseDefaultCredentials = true;
            nmFuncs.Credentials = netCred;

            MagentoAPIv2.salesOrderListRequest SalesOrderListRequest = new MagentoAPIv2.salesOrderListRequest(strSessionID, mf);
            MagentoAPIv2.salesOrderListResponse SalesOrderListResponse = new MagentoAPIv2.salesOrderListResponse();
            MagentoAPIv2.salesOrderListEntity[] soe = magev2.salesOrderList(strSessionID, mf);

            //SalesOrderListResponse = magev2.salesOrderList(SalesOrderListRequest)
            //Dim soe As MagentoAPIv2.salesOrderListEntity() = SalesOrderListResponse.result

            int OrdersInserted = 0;
            int OrdersUpdated = 0;

            string NAVOrderNo;

            if (soe.Length > 0)
            {
                foreach (MagentoAPIv2.salesOrderListEntity MagentoSalesOrder in soe)
                {
                    //If msoe.increment_id > LastSalesOrderID.Text And msoe.status = "pending" Then
                    //If msoe.status = "pending" Then
                    if ((MagentoSalesOrder.status.ToString().ToLower().Equals("pending")) || (MagentoSalesOrder.status.ToString().ToLower().Equals("processing") && DownloadProcessingStatusOrders))
                    {
                        //"pending" Or (msoe.status = "processing" And DownloadProcessingStatusOrders = True) Then
                        try
                        {
                            MagentoAPIv2.salesOrderEntity SOI = magev2.salesOrderInfo(strSessionID, MagentoSalesOrder.increment_id);


                            if (SOI.items.Length > 0)
                            {



                                // v2.01f if the Download to Single Customer Account is set then change the Customer Code to that specified
                                if (!string.IsNullOrEmpty(NAVCusAcc))
                                {
                                    MagentoSalesOrder.customer_id = NAVCusAcc;
                                }


                                //Create Service Reference 
                                SalesOrder_service.SalesOrder_Service SOService = new SalesOrder_service.SalesOrder_Service();
                                SOService.Url = NAVWebURL + "/Page/SalesOrder";
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
                                    return "Error creating a new sales order";
                                }

                                // Update the header fields on the order
                                DateTime OrderDate;
                                if (DateTime.TryParse(MagentoSalesOrder.updated_at.ToString(), out OrderDate))
                                {
                                    newNAVSOrder.Order_Date = OrderDate;
                                }
                                else
                                {
                                    newNAVSOrder.Order_Date = System.DateTime.Now;

                                }

                                newNAVSOrder.External_Document_No = SOI.increment_id;

                                // v2.02b populate the contact from the Magento Order
                                if (!string.IsNullOrEmpty(MagentoSalesOrder.customer_lastname.ToString()))
                                {
                                    newNAVSOrder.Bill_to_Contact = MagentoSalesOrder.customer_firstname + " " + MagentoSalesOrder.customer_lastname;
                                }
                                // Only populate current code if not local currency
                                if (MagentoSalesOrder.order_currency_code.ToString().ToUpper() != "GBP")
                                {
                                    newNAVSOrder.Currency_Code = MagentoSalesOrder.order_currency_code;
                                }

                                // CS 1.02f - If customer_id is blank, use the billing address id.
                                if (string.IsNullOrEmpty(MagentoSalesOrder.customer_id))
                                {
                                    MagentoSalesOrder.customer_id = MagentoSalesOrder.billing_address_id;
                                }

                                // v2.01f if a fixed customer account is specified then use that or use the one on the magento order
                                if (!string.IsNullOrEmpty(NAVCusAcc))
                                {
                                    newNAVSOrder.Sell_to_Customer_No = NAVCusAcc;
                                    newNAVSOrder.Sell_to_Customer_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    newNAVSOrder.Sell_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                    {
                                        if (SOI.billing_address.street.Length > 50)
                                        {
                                            newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(1, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                        }
                                    }
                                    if (!String.IsNullOrEmpty(SOI.billing_address.city))
                                    {
                                        if (SOI.billing_address.city.Length > 50)
                                        {
                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city.ToString().Substring(1, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                        }
                                    }

                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region; //2.02f
                                    newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                    if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                    {
                                        if (SOI.shipping_address.street.Length > 50)
                                        {
                                            newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(1, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                        }
                                    }

                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                    newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                    newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    newNAVSOrder.Prices_Including_VAT = true;
                                    // 2.02e Added addresses for the customer though is single customer 
                                }
                                else
                                {
                                    // Query the customer table to get the customer code for the Magento CustomerID
                                    if (!string.IsNullOrEmpty(MagentoSalesOrder.customer_id))
                                    {


                                        nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                        nmFuncs.UseDefaultCredentials = true;
                                        newNAVSOrder.Sell_to_Customer_No = nmFuncs.GetCustomerIdByEcomID(MagentoSalesOrder.customer_id);
                                        if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                        {
                                            if (SOI.shipping_address.street.Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(1, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                            }
                                        }

                                        newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                        newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                        newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                        newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                        newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                        if (string.IsNullOrEmpty(newNAVSOrder.Sell_to_Customer_No))
                                        {

                                            //CS 1.03c removed next line to improve processing
                                            Customer_service.Customer newCustomer = new Customer_service.Customer();
                                            try
                                            {
                                                CustService.Create(ref newCustomer);
                                            }
                                            catch (Exception custEx)
                                            {

                                                return "Error creating new customer with ecomm id: " + MagentoSalesOrder.customer_id.ToString();
                                            }


                                            newCustomer.Name = SOI.billing_address.company;
                                            newCustomer.Contact = SOI.billing_address.firstname + " " + SOI.billing_address.lastname;
                                            if (string.IsNullOrEmpty(newCustomer.Name))
                                            {
                                                newCustomer.Name = newCustomer.Contact;
                                            }
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newCustomer.Address = SOI.billing_address.street.Substring(1, 50);
                                                }
                                                else
                                                {
                                                    newCustomer.Address = SOI.billing_address.street;
                                                }
                                            }

                                            newCustomer.City = SOI.billing_address.city;
                                            newCustomer.County = SOI.billing_address.region; //2.02f
                                            newCustomer.Post_Code = SOI.billing_address.postcode;
                                            newCustomer.Phone_No = SOI.billing_address.telephone;
                                            newCustomer.E_Mail = SOI.customer_email;
                                            newCustomer.Country_Region_Code = SOI.billing_address.country_id;
                                            newCustomer.Location_Code = WHSupplyLocationCode;
                                            if (SOI.order_currency_code.ToString().ToUpper() != "GBP")
                                            {
                                                newCustomer.Currency_Code = SOI.order_currency_code;
                                            }
                                            // Determine if the customer is VATABALE by checking the country code

                                            switch (newCustomer.Country_Region_Code)
                                            {
                                                case "GB":
                                                    newCustomer.Gen_Bus_Posting_Group = "DOMESTIC";
                                                    newCustomer.VAT_Bus_Posting_Group = "DOMESTIC";
                                                    newCustomer.Customer_Posting_Group = "DOMESTIC";
                                                    break;
                                                case "AT":
                                                case "BE":
                                                case "BG":
                                                case "CY":
                                                case "CZ":
                                                case "DK":
                                                case "EE":
                                                case "FI":
                                                case "FR":
                                                case "DE":
                                                case "EL":
                                                case "HE":
                                                case "IE":
                                                case "IT":
                                                case "LV":
                                                case "LT":
                                                case "LU":
                                                case "MT":
                                                case "NL":
                                                case "PL":
                                                case "PT":
                                                case "SO":
                                                case "SK":
                                                case "SI":
                                                case "ES":
                                                case "SE":
                                                    newCustomer.Gen_Bus_Posting_Group = "EU";
                                                    newCustomer.VAT_Bus_Posting_Group = "EUINCVAT";
                                                    newCustomer.Customer_Posting_Group = "EU";
                                                    break;
                                                default:
                                                    newCustomer.Gen_Bus_Posting_Group = "FOREIGN";
                                                    newCustomer.VAT_Bus_Posting_Group = "FOREIGN";
                                                    newCustomer.Customer_Posting_Group = "FOREIGN";
                                                    break;
                                            }




                                            newNAVSOrder.Sell_to_Customer_No = newCustomer.No;
                                            //13/4/12 CS Changed the below so it is consistent
                                            //newOrder.Sell_to_Address = SalesOrderInfoResponse.result.billing_address.street
                                            //newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                            //newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                            //newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(1, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                                }
                                            }

                                            //newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                            newNAVSOrder.Sell_to_County = SOI.billing_address.region; //2.02f
                                            newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.shipping_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(1, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                                }
                                            }

                                            newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                            newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                            newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                            newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                            newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;

                                            //13/4/12 CS Added following in as requested by Moon.
                                            newNAVSOrder.Requested_Delivery_Date = System.DateTime.Today;

                                            //cs 4/4 Amended to pass down prices including VAT
                                            //cs 23/10 v2.01b - changed to exclude VAT so pricing is correct in NAV for UK and overseas orders. Will now use default from customer record.
                                            //newOrder.Prices_Including_VAT = True
                                            newCustomer.eCommerce_Enabled = true;
                                            newCustomer.eCommerceID = SOI.customer_id;
                                            CustService.Update(ref newCustomer);

                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_Customer_No = strDefaultCustomer; /// usually customer no. 999
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(1, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                                }
                                            }

                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                            newNAVSOrder.Sell_to_County = SOI.billing_address.region;
                                            newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                            //CS 13/4/12 Added below to deal with ship to addresses.  Also limited the address to 50 characters.
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.shipping_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.ToString().Substring(1, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                                }
                                            }

                                            newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                            newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                            newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;


                                        }
                                    }
                                }

                                //Create the blank order lines


                                // v2.01f - need to add extra records for discount and shipping
                                //newOrder.SalesLines = New SalesOrder_Service.Sales_Order_Line(SOI.Length) 
                                int NoOfSOLines = SOI.items.Length;
                                decimal decDisAmount = 0;
                                decimal.TryParse(SOI.discount_amount, out decDisAmount);
                                if (decDisAmount != 0)
                                {
                                    NoOfSOLines = NoOfSOLines + 1;

                                }
                                decimal desShippingAmount = 0;
                                decimal.TryParse(SOI.shipping_amount, out desShippingAmount);

                                if (desShippingAmount > 0)
                                {
                                    NoOfSOLines = NoOfSOLines + 1;
                                }

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
                                        return ("Error inserting blank order lines onto order");
                                    }

                                }
                                // Update the order 
                                try
                                {
                                    SOService.Update(ref newNAVSOrder);
                                }
                                catch (Exception soUpdateEx)
                                {
                                    return ("Unable to update order....     " + soUpdateEx.ToString());
                                }

                                string LastSKU = string.Empty;
                                int SalesOrderIDX = 0;
                                // Populate the order line details
                                for (int idx = 0; idx < SOI.items.Length; idx++)
                                {
                                    // v2.01f if the line has a 0 qty then delete the line
                                    if ((SOI.items[idx].base_price == "0") && (SOI.items[idx].sku == LastSKU))
                                    {
                                        //SOservice.Delete_SalesLines(idx)
                                        LastSKU = "";
                                    }

                                    else
                                    {
                                        try
                                        {
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = SOI.items[idx].sku;
                                            // v 2.01f This this is a configurable product, save the SKU so we can delete the following line for the simple product
                                            if (SOI.items[idx].product_type.ToString().ToLower() == "configurable")
                                            {
                                                LastSKU = SOI.items[idx].sku;
                                            }

                                            decimal decQty = 0;
                                            if (decimal.TryParse(SOI.items[idx].qty_ordered, out decQty))
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = decQty;
                                            }
                                            else
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 0;
                                            }
                                            decimal decPrice = 0;
                                            //if (decimal.TryParse(SOI.items[idx].price, out decPrice))
                                            if (decimal.TryParse(SOI.items[idx].base_original_price, out decPrice))
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decPrice;
                                            }
                                            else
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                            }

                                            //CS V2.01 Changed this back to use the Price field which excludes VAT because NAV then applies the VAT.                                          
                                            string strItemNo = string.Empty;
                                            strItemNo = nmFuncs.CheckIfItemExists(SOI.items[idx].sku, SOI.items[idx].item_id, SOI.items[idx].name);
                                            //we need to use thestrItemNo for the sales line
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = strItemNo;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description = SOI.items[idx].name;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description_2 = SOI.items[idx].sku;

                                        }
                                        catch (Exception ex1)
                                        {
                                            return (ex1.InnerException.ToString());
                                        }
                                        SalesOrderIDX = SalesOrderIDX + 1;
                                    }

                                }

                                // v2.01f only add a shipping line if there is a shipping amount on the order
                                decimal decShipPrice = 0;
                                if (decimal.TryParse(SOI.shipping_amount, out decShipPrice))
                                {
                                    if (decShipPrice > 0)
                                    {
                                        // v2.01a Add the SHIPPING Line
                                        try
                                        {                                            
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item;//SalesOrder_Service.Type.Item
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = "SHIPPING";
                                            newNAVSOrder.SalesLines[SalesOrderIDX].TypeSpecified = true;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description = SOI.shipping_description;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 1;

                                            // v2.01f Use the correct shipping amount for the order currency                                         
                                            if (decimal.TryParse(SOI.shipping_amount, out decShipPrice))
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decShipPrice;
                                            }
                                            else
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                            }
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Unit_PriceSpecified = true;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].QuantitySpecified = true;
                                        }
                                        catch (Exception slEx)
                                        {
                                            return ("Error inserting blank order lines onto order");
                                        }
                                        SalesOrderIDX = SalesOrderIDX + 1;
                                    }
                                }

                                // v2.01f If there is a discount, add the Discount line to the order
                                decimal decDiscountAmount = 0;
                                if (decimal.TryParse(SOI.discount_amount, out decDiscountAmount))
                                {
                                    if (decDiscountAmount != 0)
                                    {
                                        try
                                        {                                            
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.G_L_Account; //SalesOrder_Service.Type.G_L_Account
                                            newNAVSOrder.SalesLines[SalesOrderIDX].TypeSpecified = true;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = DiscountGLAccount;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 1;

                                            if (decimal.TryParse(SOI.discount_amount, out decDiscountAmount))
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
                                            decimal.TryParse(SOI.tax_amount, out decTaxAmount);
                                            if (decTaxAmount > 0)
                                            {
                                                decimal decGrandTaxAmount = 0;
                                                decimal.TryParse(SOI.grand_total, out decGrandTaxAmount);
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
                                            return ("Error inserting blank order lines onto order");
                                        }
                                    }
                                }


                                try
                                {
                                    SOService.Update(ref newNAVSOrder);
                                    OrdersInserted = OrdersInserted + 1;
                                    // 13/4/12 CS Update the order status to processing
                                    try
                                    {      //need to skip this if in test mode!!!
                                        //v2.02b If we want to download processing orders, then change the filter

                                        if (DownloadProcessingStatusOrders == true)
                                        {
                                            //magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "complete", "", 0);
                                            OrdersUpdated = OrdersUpdated + 1;
                                        }
                                        else
                                        {
                                            //magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "processing", "", 0);
                                        }
                                    }
                                    catch (Exception exl2)
                                    {
                                        return ("Couldn't update order status for Magento orderID " + MagentoSalesOrder.order_id + "     " + exl2.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ("Can't update order " + newNAVSOrder.No + "               " + ex.ToString());
                                }


                            }
                        }
                        catch (Exception e)
                        {
                            //do something
                            return "Error processing sales orders";
                        }
                    }
                }
            }
            return "Success";
        }
        */
        #endregion

        public string DownloadOrders(string strSessionID, string MagentoURL, string NAVWebURL, bool DownloadProcessingStatusOrders, string NAVCusAcc, string WHSupplyLocationCode, string strDefaultCustomer, string DiscountGLAccount, string strUsername, string strPassword, string strDomain, bool bUpdateMagentoOrders, string strNewOrders, string strInProgressOrders)
        {

            // not needed for Ellison
            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.Name = "Mage_Api_Model_Server_Wsi_HandlerPort";
            //myBinding.Name = "PortType";
            //"BasicHttpBinding_IService1";
            myBinding.Security.Mode = BasicHttpSecurityMode.None;
            //myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            //myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            //myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            System.TimeSpan timeout = new TimeSpan(0, 10, 0);
            myBinding.SendTimeout = timeout;
            myBinding.MaxBufferSize = 999999999;
            myBinding.MaxReceivedMessageSize = 999999999;
            myBinding.MaxBufferPoolSize = 999999999;
            //myBinding.TransferMode = TransferMode.Streamed;
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");



            MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
            // MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient("Port", MagentoURL + "/index.php/api/v2_soap/index/");

            //MagentoAPIv2.PortTypeClient("Port", endPointAddress);

            //myBinding, endPointAddress);



            //----------------------- Create new orders for pending items in Magento ---------------------------------------

            // 2.02b Define a tax rate %
            int tax_rate = 0;

            //v2.01f restrict the filter to orders with a status of Pending
            MagentoAPIv2.filters mf = new MagentoAPIv2.filters();
            MagentoAPIv2.complexFilter complexFilter = new MagentoAPIv2.complexFilter();
            //complexFilter.key = "Status";
            MagentoAPIv2.associativeEntity assEnt = new MagentoAPIv2.associativeEntity();
            assEnt.key = "status";
            //MOD001 AZ CM 241016 - change to the status of new orders, allowed be chosen in NAV +
            //assEnt.value = "pending"; //2.02g_b
            assEnt.value = strNewOrders;
            //MOD001 AZ CM 241016 - change to the status of new orders, allowed be chosen in NAV -
            // assEnt.key = "Increment_ID";
            // assEnt.value = "400003060";
             //complexFilter.key = "Increment_ID";
            //UNCOMMENT WHEN PUTTING BACK TO NORMAL
            complexFilter.key = "status";


            //assEnt.key = "Increment_ID";
            //assEnt.value = "100000039-1";

            //v2.02b If we want to download processing orders, then change the filter
            //DownloadProcessingStatusOrders = true; //2.02g
            if (DownloadProcessingStatusOrders == true)
            {
                assEnt.value = "processing";
            }
            //assEnt.key = "Order";
            //assEnt.value = "200006172";
            complexFilter.value = assEnt;
            MagentoAPIv2.complexFilter[] lstComplexFilter = new MagentoAPIv2.complexFilter[1];
            lstComplexFilter[0] = complexFilter;
            mf.complex_filter = lstComplexFilter;

            NetworkCredential netCred = setCredentials(strUsername, strPassword, strDomain);
            NavMagFunctions.MagentoFunctions nmFuncs = new NavMagFunctions.MagentoFunctions();
            nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
            nmFuncs.UseDefaultCredentials = true;
            nmFuncs.Credentials = netCred;

            MagentoAPIv2.salesOrderListRequest SalesOrderListRequest = new MagentoAPIv2.salesOrderListRequest(strSessionID, mf);


            MagentoAPIv2.salesOrderListResponse SalesOrderListResponse = new MagentoAPIv2.salesOrderListResponse();


            MagentoAPIv2.salesOrderListEntity[] soe = magev2.salesOrderList(strSessionID, mf);

            int OrdersInserted = 0;
            int OrdersUpdated = 0;

            string NAVOrderNo;

            if (soe.Length > 0)
            {
                foreach (MagentoAPIv2.salesOrderListEntity MagentoSalesOrder in soe)
                {

                    //If msoe.increment_id > LastSalesOrderID.Text And msoe.status = "pending" Then
                    //If msoe.status = "pending" Then
                    //if ((MagentoSalesOrder.status.ToString().ToLower().Equals("pending")) || (MagentoSalesOrder.status.ToString().ToLower().Equals("processing") && DownloadProcessingStatusOrders))
                    //MOD002 +
                    //if (MagentoSalesOrder.status.ToString().ToLower().Equals("processing")) //'2.02g 
                    //{
                    //MOD002 -

                    if (MagentoSalesOrder.increment_id.ToString().StartsWith("70") || MagentoSalesOrder.increment_id.ToString().StartsWith("40")) //'2.02g 
                    {

                        //"pending" Or (msoe.status = "processing" And DownloadProcessingStatusOrders = True) Then
                        try
                        {
                            MagentoAPIv2.salesOrderEntity SOI = magev2.salesOrderInfo(strSessionID, MagentoSalesOrder.increment_id);
                            if (SOI.items.Length > 0)
                            {


                                decimal intTaxAmount = 0;
                                // v2.01f if the Download to Single Customer Account is set then change the Customer Code to that specified
                                if (!string.IsNullOrEmpty(NAVCusAcc))
                                {
                                    MagentoSalesOrder.customer_id = NAVCusAcc;
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
                                    return "Error creating a new sales order";
                                }

                                // Update the header fields on the order
                                DateTime OrderDate;
                                if (DateTime.TryParse(MagentoSalesOrder.updated_at.ToString(), out OrderDate))
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
                                if (!decimal.TryParse(SOI.base_grand_total, out decTest))
                                {
                                    newNAVSOrder.TotalAmountMagentoOrder = (decimal)0;
                                }
                                else
                                {
                                    newNAVSOrder.TotalAmountMagentoOrder = decTest;
                                }

                                newNAVSOrder.Salesperson_Code = "CONS";
                                newNAVSOrder.External_Document_No = "SZUK" + SOI.increment_id;
                                //if (SOI.increment_id == "700002923")
                                // {
                                //    string strTest = "test";
                                // }

                                //newNAVSOrder.Email = SOI.customer_email; //CR3 this is pushed from magento to the sales header and from there to DPD                               

                                // v2.02b populate the contact from the Magento Order
                                if (!string.IsNullOrEmpty(MagentoSalesOrder.customer_lastname))
                                {
                                    newNAVSOrder.Bill_to_Contact = MagentoSalesOrder.customer_firstname + " " + MagentoSalesOrder.customer_lastname;
                                }
                                // Only populate current code if not local currency
                                if (MagentoSalesOrder.order_currency_code.ToString().ToUpper() != "GBP")
                                {
                                    newNAVSOrder.Currency_Code = MagentoSalesOrder.order_currency_code;
                                }

                                // CS 1.02f - If customer_id is blank, use the billing address id.
                                if (string.IsNullOrEmpty(MagentoSalesOrder.customer_id))
                                {
                                    MagentoSalesOrder.customer_id = MagentoSalesOrder.billing_address_id;
                                }

                                // v2.01f if a fixed customer account is specified then use that or use the one on the magento order
                                if (!string.IsNullOrEmpty(NAVCusAcc))
                                {
                                    newNAVSOrder.Sell_to_Customer_No = NAVCusAcc;
                                    newNAVSOrder.Sell_to_Customer_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    newNAVSOrder.Sell_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    //TEST
                                    if (SOI.increment_id == "700002947")
                                    {
                                        string strTest = "test";
                                    }

                                    if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                    {
                                        if (SOI.billing_address.street.Length > 50)
                                        {
                                            newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(0, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                        }
                                    }
                                    if (!String.IsNullOrEmpty(SOI.billing_address.city))
                                    {
                                        if (SOI.billing_address.city.Length > 50)
                                        {
                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city.ToString().Substring(0, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                        }
                                    }

                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region; //2.02f
                                    newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                    //TEST
                                    if (SOI.increment_id == "700002947")
                                    {
                                        string strTest = "test";
                                    }

                                    if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                    {
                                        if (SOI.shipping_address.street.Length > 50)
                                        {
                                            newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(0, 50);
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                        }
                                    }

                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                    newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                    //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                    //newNAVSOrder.Ship_to_Name = SOI.shipping_firstname + SOI.shipping_lastname;
                                    if (SOI.increment_id == "400001928")
                                    {
                                        string strTest = "test";
                                    }

                                    if (!string.IsNullOrEmpty(SOI.shipping_address.telephone))
                                    {
                                        newNAVSOrder.Ship_To_Phone_No = SOI.shipping_address.telephone;
                                    }
                                    else
                                    {
                                        newNAVSOrder.Ship_To_Phone_No = SOI.billing_address.telephone;
                                    }

                                    if (!(SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname).Equals(" "))
                                    {
                                        newNAVSOrder.Ship_to_Name = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;
                                        newNAVSOrder.Ship_to_Contact = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;                                        
                                    }
                                    else
                                    {
                                        //newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                        newNAVSOrder.Ship_to_Name = MagentoSalesOrder.billing_firstname + " " + MagentoSalesOrder.billing_lastname;
                                        newNAVSOrder.Ship_to_Contact = MagentoSalesOrder.billing_firstname + " " + MagentoSalesOrder.billing_lastname;                                        
                                    }
                                    //SOI.customer_firstname + " " + SOI.customer_lastname;

                                    if (decimal.TryParse(SOI.tax_amount, out intTaxAmount))
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
                                    if (!string.IsNullOrEmpty(MagentoSalesOrder.customer_id))
                                    {


                                        nmFuncs.Url = NAVWebURL + "/Codeunit/MagentoFunctions";
                                        nmFuncs.UseDefaultCredentials = true;
                                        string strCustomerNo = string.Empty; //customerNo.= "9 CJONES" or "9 CJONES01" or "9 CJONES02" if email different 
                                        if (SOI.billing_address.lastname.ToString().Length >= 10)
                                        {
                                            strCustomerNo = "9" + SOI.billing_address.firstname.Substring(0, 1).ToUpper() + SOI.billing_address.lastname.Substring(0, 10).ToUpper().Replace(" ", "");
                                        }
                                        else
                                        {
                                            strCustomerNo = "9" + SOI.billing_address.firstname.Substring(0, 1).ToUpper() + SOI.billing_address.lastname.ToUpper().Replace(" ", "");
                                        }
                                        //this calls a new function in NAV that returns the customer no. if it exists based on customer email and weird 9 FNLN logic :S
                                        newNAVSOrder.Sell_to_Customer_No = nmFuncs.CheckCustomer(MagentoSalesOrder.customer_email, strCustomerNo);
                                        //TEST
                                        if (SOI.increment_id == "700002947")
                                        {
                                            string strTest = "test";
                                        }

                                        //nmFuncs.GetCustomerIdByEcomID(MagentoSalesOrder.customer_id);
                                        if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                        {
                                            if (SOI.shipping_address.street.Length > 50)
                                            {
                                                newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(0, 50);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                            }
                                        }

                                        //newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                        if (!String.IsNullOrEmpty(SOI.shipping_address.city))
                                        {
                                            if (SOI.shipping_address.city.Length > 30)
                                            {
                                                newNAVSOrder.Ship_to_City = SOI.shipping_address.city.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                            }
                                        }


                                        newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                        if (!String.IsNullOrEmpty(SOI.shipping_address.region))
                                        {
                                            if (SOI.shipping_address.region.Length > 30)
                                            {
                                                newNAVSOrder.Ship_to_County = SOI.shipping_address.region.Substring(0, 30);
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_County = SOI.shipping_address.region;
                                            }
                                        }
                                        newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                        //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                        //newNAVSOrder.Ship_to_Name = SOI.shipping_firstname + " " + SOI.shipping_lastname;

                                        if (!string.IsNullOrEmpty(SOI.shipping_address.telephone))
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = SOI.shipping_address.telephone;
                                        }
                                        else
                                        {
                                            newNAVSOrder.Ship_To_Phone_No = SOI.billing_address.telephone;
                                        }

                                        if (!(SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname).Equals(" "))
                                        {
                                            newNAVSOrder.Ship_to_Name = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;
                                            newNAVSOrder.Ship_to_Contact = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;                                            
                                        }
                                        else
                                        {
                                            //newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                            newNAVSOrder.Ship_to_Name = MagentoSalesOrder.billing_firstname + " " + MagentoSalesOrder.billing_lastname;
                                            newNAVSOrder.Ship_to_Contact = MagentoSalesOrder.billing_firstname + " " + MagentoSalesOrder.billing_lastname;                                            
                                        }
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

                                                return "Error creating new customer with ecomm id: " + MagentoSalesOrder.customer_id.ToString();
                                            }


                                            newCustomer.Name = SOI.billing_address.company;
                                            newCustomer.Contact = SOI.billing_address.firstname + " " + SOI.billing_address.lastname;
                                            if (string.IsNullOrEmpty(newCustomer.Name))
                                            {
                                                newCustomer.Name = newCustomer.Contact;
                                            }
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newCustomer.Address = SOI.billing_address.street.Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newCustomer.Address = SOI.billing_address.street;
                                                }
                                            }
                                            if (!String.IsNullOrEmpty(SOI.billing_address.city))
                                            {
                                                if (SOI.billing_address.city.Length > 30)
                                                {
                                                    newCustomer.City = SOI.billing_address.city.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newCustomer.City = SOI.billing_address.city;
                                                }
                                            }

                                            if (!String.IsNullOrEmpty(SOI.billing_address.region))
                                            {
                                                if (SOI.billing_address.region.Length > 30)
                                                {
                                                    newCustomer.County = SOI.billing_address.region.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newCustomer.County = SOI.billing_address.region;
                                                }
                                            }
                                            newCustomer.Post_Code = SOI.billing_address.postcode;
                                            newCustomer.Phone_No = SOI.billing_address.telephone;

                                            if (!String.IsNullOrEmpty(SOI.customer_email))
                                            {
                                                if (SOI.customer_email.Length > 80)
                                                {
                                                    newCustomer.E_Mail = SOI.customer_email.Substring(0, 80);
                                                }
                                                else
                                                {
                                                    newCustomer.E_Mail = SOI.customer_email;
                                                }
                                            }

                                            if (!String.IsNullOrEmpty(SOI.billing_address.country_id))
                                            {
                                                if (SOI.billing_address.country_id.Length > 80)
                                                {
                                                    newCustomer.Country_Region_Code = SOI.billing_address.country_id.Substring(0, 80);
                                                }
                                                else
                                                {
                                                    newCustomer.Country_Region_Code = SOI.billing_address.country_id;
                                                }
                                            }


                                            newCustomer.Location_Code = WHSupplyLocationCode;
                                            newCustomer.Global_Dimension_2_Code = "Consumer"; //2.02g
                                            if (SOI.order_currency_code.ToString().ToUpper() != "GBP")
                                            {
                                                newCustomer.Currency_Code = SOI.order_currency_code;
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
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                                }
                                            }

                                            if (!String.IsNullOrEmpty(SOI.billing_address.city))
                                            {
                                                if (SOI.billing_address.city.Length > 30)
                                                {
                                                    newNAVSOrder.Sell_to_City = SOI.billing_address.city.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                                }
                                            }

                                            if (!String.IsNullOrEmpty(SOI.billing_address.region))
                                            {
                                                if (SOI.billing_address.region.Length > 30)
                                                {
                                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region;
                                                }
                                            }

                                            if (!String.IsNullOrEmpty(SOI.customer_email))
                                            {
                                                if (SOI.customer_email.Length > 80)
                                                {
                                                    newCustomer.E_Mail = SOI.customer_email.Substring(0, 80);
                                                }
                                                else
                                                {
                                                    newCustomer.E_Mail = SOI.customer_email;
                                                }
                                            }

                                            newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.shipping_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                                }
                                            }

                                            //newNAVSOrder.Ship_to_City = SOI.shipping_address.city; //replaced by below

                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.shipping_address.city.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city.Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                                }
                                            }


                                            //newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f //replaced by below
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.region))
                                            {
                                                if (SOI.shipping_address.region.Length > 30)
                                                {
                                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region;
                                                }
                                            }


                                            newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;


                                            //newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                            if (SOI.increment_id == "400001928")
                                            {
                                                string strTest = "test";
                                            }

                                            if (!string.IsNullOrEmpty(SOI.shipping_address.telephone))
                                            {
                                                newNAVSOrder.Ship_To_Phone_No = SOI.shipping_address.telephone;
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_To_Phone_No = SOI.billing_address.telephone;
                                            }
                                            if (!string.IsNullOrEmpty(SOI.shipping_address.telephone))
                                            {
                                                newNAVSOrder.Ship_To_Phone_No = SOI.shipping_address.telephone;
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_To_Phone_No = SOI.billing_address.telephone;
                                            }

                                            if (!(SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname).Equals(" "))
                                            {
                                                newNAVSOrder.Ship_to_Name = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;
                                                newNAVSOrder.Ship_to_Contact = SOI.shipping_address.firstname + " " + SOI.shipping_address.lastname;
                                            }
                                            else
                                            {
                                                newNAVSOrder.Ship_to_Name = SOI.customer_firstname + " " + SOI.customer_lastname;
                                                newNAVSOrder.Ship_to_Contact = SOI.customer_firstname + " " + SOI.customer_lastname;
                                            }
                                            //newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id; //replaced by below
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.country_id))
                                            {
                                                if (SOI.shipping_address.country_id.Length > 10)
                                                {
                                                    newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id.Substring(0, 10);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id;
                                                }
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

                                            //13/4/12 CS Added following in as requested by Moon.
                                            newNAVSOrder.Requested_Delivery_Date = System.DateTime.Today;

                                            //cs 4/4 Amended to pass down prices including VAT
                                            //cs 23/10 v2.01b - changed to exclude VAT so pricing is correct in NAV for UK and overseas orders. Will now use default from customer record.
                                            //newOrder.Prices_Including_VAT = True
                                            newCustomer.eCommerce_Enabled = true;
                                            newCustomer.eCommerceID = SOI.customer_id;
                                            CustService.Update(ref newCustomer);

                                        }
                                        else
                                        {
                                            if (!string.IsNullOrEmpty(strDefaultCustomer))
                                            {
                                                newNAVSOrder.Sell_to_Customer_No = strDefaultCustomer; /// usually customer no. 999
                                            }
                                            if (!String.IsNullOrEmpty(SOI.billing_address.street))
                                            {
                                                if (SOI.billing_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street.ToString().Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_Address = SOI.billing_address.street;
                                                }
                                            }

                                            //newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.billing_address.city.Length > 30)
                                                {
                                                    newNAVSOrder.Sell_to_City = SOI.billing_address.city.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_City = SOI.billing_address.city;
                                                }
                                            }
                                            newNAVSOrder.Sell_to_County = SOI.billing_address.region;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.region))
                                            {
                                                if (SOI.shipping_address.region.Length > 30)
                                                {
                                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Sell_to_County = SOI.billing_address.region;
                                                }
                                            }
                                            newNAVSOrder.Sell_to_Post_Code = SOI.billing_address.postcode;
                                            //CS 13/4/12 Added below to deal with ship to addresses.  Also limited the address to 50 characters.
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.street))
                                            {
                                                if (SOI.shipping_address.street.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street.ToString().Substring(0, 50);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Address = SOI.shipping_address.street;
                                                }
                                            }

                                            //newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.city))
                                            {
                                                if (SOI.shipping_address.city.Length > 50)
                                                {
                                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_City = SOI.shipping_address.city;
                                                }
                                            }
                                            //newNAVSOrder.Ship_to_County = SOI.shipping_address.region; //2.02f
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.region))
                                            {
                                                if (SOI.shipping_address.region.Length > 30)
                                                {
                                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region.Substring(0, 30);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_County = SOI.shipping_address.region;
                                                }
                                            }
                                            newNAVSOrder.Ship_to_Post_Code = SOI.shipping_address.postcode;
                                            if (!String.IsNullOrEmpty(SOI.shipping_address.country_id))
                                            {
                                                if (SOI.shipping_address.country_id.Length > 10)
                                                {
                                                    newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id.Substring(0, 10);
                                                }
                                                else
                                                {
                                                    newNAVSOrder.Ship_to_Country_Region_Code = SOI.shipping_address.country_id;
                                                }
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
                                switch (SOI.shipping_address.country_id)
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
                                if (SOI.increment_id == "400001918")
                                {
                                    string testIncreId = "test";
                                }

                                int NoOfSOLines = SOI.items.Length;
                                decimal decDisAmount = 0;
                                decimal.TryParse(SOI.discount_amount, out decDisAmount);
                                if (decDisAmount != 0)
                                {
                                    NoOfSOLines = NoOfSOLines + 1;

                                }
                                decimal desShippingAmount = 0;
                                decimal.TryParse(SOI.shipping_amount, out desShippingAmount);

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
                                }
                                catch (Exception soUpdateEx)
                                {
                                    return ("Unable to update order....     " + soUpdateEx.ToString());
                                }

                                string LastSKU = string.Empty;
                                int SalesOrderIDX = 0;
                                //set a bool to signify if bundles are used
                                bool bBundle = false;
                                string strSalesOrderNo = string.Empty;
                                int intLineNo = 0;
                                // Populate the order line details
                                for (int idx = 0; idx < SOI.items.Length; idx++)
                                {

                                    if (SOI.items[idx].product_type.ToLower() == "bundle")
                                    {
                                        bBundle = true;
                                    }
                                    // v2.01f if the line has a 0 qty then delete the line
                                    if ((SOI.items[idx].base_price == "0") && (SOI.items[idx].sku == LastSKU))
                                    {
                                        //SOservice.Delete_SalesLines(idx)
                                        LastSKU = "";
                                    }

                                    else if ((SOI.items[SalesOrderIDX].product_type.ToLower() == "bundle") || ((bBundle == false) & (SOI.items[SalesOrderIDX].product_type.ToLower() == "simple")))
                                    {
                                        try
                                        {
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item;
                                            //CS V2.01 Changed this back to use the Price field which excludes VAT because NAV then applies the VAT.                                          
                                            string strItemNo = string.Empty;
                                            string strSku = string.Empty;
                                            if (SOI.items[idx].sku.Length > 20)
                                            {
                                                strSku = SOI.items[idx].sku.Substring(0, 19);
                                            }
                                            else
                                            {
                                                strSku = SOI.items[idx].sku;
                                            }
                                            string strName = string.Empty;
                                            if (SOI.items[idx].name.Length > 50)
                                            {
                                                strName = SOI.items[idx].name.Substring(0, 49);
                                            }
                                            else
                                            {
                                                strName = SOI.items[idx].name;
                                            }
                                            string strVariant = string.Empty;

                                            if (SOI.items[idx].sku.Contains("#"))
                                            {
                                                string[] lstItemDetails = SOI.items[idx].sku.Split('#');
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
                                            strItemNo = nmFuncs.CheckIfItemExists(strSku, SOI.items[idx].product_id, ref strName, bBundle, strVariant, ref strItemDescrip2);
                                            //we need to use thestrItemNo for the sales line
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = strItemNo;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Variant_Code = strVariant;
                                            //newNAVSOrder.SalesLines[SalesOrderIDX].No = SOI.items[idx].sku;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description = strName;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description_2 = strItemDescrip2;//strSku;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Location_Code = WHSupplyLocationCode;

                                            // v 2.01f This this is a configurable product, save the SKU so we can delete the following line for the simple product
                                            if (SOI.items[idx].product_type.ToString().ToLower() == "configurable")
                                            {
                                                LastSKU = SOI.items[idx].sku;
                                            }

                                            decimal decQty = 0;
                                            if (decimal.TryParse(SOI.items[idx].qty_ordered, out decQty))
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
                                            if (SOI.increment_id == "400001918")
                                            {
                                                string testIncreId = "test";
                                            }

                                            decimal.TryParse(SOI.tax_amount, out intTaxAmount);

                                            if (intTaxAmount > 0)
                                            {
                                                if (decimal.TryParse(SOI.items[idx].base_price, out decPrice))
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
                                                if ((SOI.billing_address.country_id == "Switzerland") ||
                                                    (SOI.billing_address.country_id == "JE") ||
                                                    (SOI.billing_address.country_id == "CH") ||
                                                    (SOI.billing_address.country_id == "GG") ||
                                                    (SOI.billing_address.country_id == "NO"))
                                                {
                                                    if (decimal.TryParse(SOI.items[idx].base_price, out decPrice))
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
                                                    if (decimal.TryParse(SOI.items[idx].base_original_price, out decPrice))
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
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Total_Amount_Incl_VATSpecified = false;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Total_VAT_AmountSpecified = false;
                                            SOService.Update(ref newNAVSOrder); //we need to update here as we need the assembly no. back
                                            intLineNo = newNAVSOrder.SalesLines[SalesOrderIDX].Line_No;
                                            strSalesOrderNo = newNAVSOrder.SalesLines[SalesOrderIDX].Document_No;
                                        }
                                        catch (Exception ex1)
                                        {
                                            return (ex1.InnerException.ToString());
                                        }
                                        SalesOrderIDX = SalesOrderIDX + 1;
                                    }
                                    else if (((bBundle == true) & (SOI.items[SalesOrderIDX].product_type.ToLower() == "simple")))
                                    {
                                        //this point we need to create an assembly line
                                        string strAssemblyNo = nmFuncs.GetAssemblyOrderNo(strSalesOrderNo, intLineNo);
                                        if (!string.IsNullOrEmpty(strAssemblyNo))
                                        {
                                            string strSku = string.Empty;
                                            if (SOI.items[idx].sku.Length > 20)
                                            {
                                                strSku = SOI.items[idx].sku.Substring(0, 19);
                                            }
                                            else
                                            {
                                                strSku = SOI.items[idx].sku;
                                            }
                                            string strVariant = string.Empty;
                                            if (SOI.items[idx].sku.Contains("#"))
                                            {
                                                string[] lstItemDetails = SOI.items[idx].sku.Split('#');
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
                                            if (SOI.items[idx].name.Length > 50)
                                            {
                                                strName = SOI.items[idx].name.Substring(0, 49);
                                            }
                                            else
                                            {
                                                strName = SOI.items[idx].name;
                                            }
                                            decimal decQty = 0;
                                            decimal.TryParse(SOI.items[idx].qty_ordered, out decQty);
                                            //at this point we can add the sales line to the assembly order
                                            nmFuncs.SetAssemblyComponent(strAssemblyNo, strSku, decQty, strName, strSku, strVariant);

                                        }
                                        SalesOrderIDX = SalesOrderIDX + 1;
                                    }

                                }

                                // v2.01f only add a shipping line if there is a shipping amount on the order
                                decimal decShipPrice = 0;
                                if (decimal.TryParse(SOI.shipping_amount, out decShipPrice))
                                {
                                    if (decShipPrice > 0)
                                    {
                                        // v2.01a Add the SHIPPING Line
                                        try
                                        {
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Type = SalesOrder_service.Type.Item; //SalesOrder_Service.Type.G_L_Account                                               
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = "SHIPPING";
                                            newNAVSOrder.SalesLines[SalesOrderIDX].TypeSpecified = true;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description = SOI.shipping_description;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Quantity = 1;

                                            // v2.01f Use the correct shipping amount for the order currency                                         
                                            if (decimal.TryParse(SOI.shipping_amount, out decShipPrice))
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decShipPrice;
                                            }
                                            else
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
                                            }
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Unit_PriceSpecified = true;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].QuantitySpecified = true;
                                        }
                                        catch (Exception slEx)
                                        {
                                            return ("Error inserting blank order lines onto order");
                                        }
                                        SalesOrderIDX = SalesOrderIDX + 1;
                                    }
                                }

                                // v2.01f If there is a discount, add the Discount line to the order
                                decimal decDiscountAmount = 0;
                                if (decimal.TryParse(SOI.discount_amount, out decDiscountAmount))
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

                                            if (decimal.TryParse(SOI.discount_amount, out decDiscountAmount))
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
                                            decimal.TryParse(SOI.tax_amount, out decTaxAmount);
                                            if (decTaxAmount > 0)
                                            {
                                                decimal decGrandTaxAmount = 0;
                                                decimal.TryParse(SOI.grand_total, out decGrandTaxAmount);
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
                                            //MOD003 AZ CM 241016 - update orders to given status +
                                            //if (DownloadProcessingStatusOrders == true)
                                            //{
                                            //    magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "complete", "", 0);
                                            //    OrdersUpdated = OrdersUpdated + 1;
                                            //}
                                            //else
                                            //{
                                            //    magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "processing", "", 0);
                                            //}
                                            magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, strInProgressOrders, "", 0);
                                            OrdersUpdated = OrdersUpdated + 1;
                                            //MOD003 AZ CM 241016 - update orders to given status -
                                        }
                                    }
                                    catch (Exception exl2)
                                    {
                                        return ("Couldn't update order status for Magento orderID " + MagentoSalesOrder.order_id + "     " + exl2.ToString());
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return ("Can't update order " + newNAVSOrder.No + "               " + ex.ToString());
                                }


                            }
                        }
                        catch (Exception e)
                        {
                            //do something
                            return "Error processing sales orders";
                        }
                    }
                    //} //MOD002
                }
            }
            return "Success";
        }


        public bool UpdateOrderToShipped(string strSessionID, string MagentoURL, string NAVWebURL, string strIncrementID, string strShippedStatus)
        {
            try
            {
                // not needed for Ellison
                BasicHttpBinding myBinding = new BasicHttpBinding();
                myBinding.Name = "Mage_Api_Model_Server_Wsi_HandlerPort";
                //myBinding.Name = "PortType";
                //"BasicHttpBinding_IService1";
                myBinding.Security.Mode = BasicHttpSecurityMode.None;
                //myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                //myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                //myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                System.TimeSpan timeout = new TimeSpan(0, 10, 0);
                myBinding.SendTimeout = timeout;
                myBinding.MaxBufferSize = 999999999;
                myBinding.MaxReceivedMessageSize = 999999999;
                myBinding.MaxBufferPoolSize = 999999999;
                //myBinding.TransferMode = TransferMode.Streamed;
                EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");

                MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
                MagentoAPIv2.salesOrderEntity SOI = magev2.salesOrderInfo(strSessionID, strIncrementID);
                if (SOI != null)
                {
                    magev2.salesOrderAddComment(strSessionID, strIncrementID, strShippedStatus, "", 0);

                }
            }
            catch (Exception e)
            {
                return false;

            }

            return true;

        }

        public bool uploadInventoryLevels(string strSessionID, string MagentoURL, string NAVWebURL, string strUsername, string strPassword, string strDomain)
        {
            try
            {
                BasicHttpBinding myBinding = new BasicHttpBinding();
                myBinding.Name = "BasicHttpBinding_IService1";
                myBinding.Security.Mode = BasicHttpSecurityMode.None;
                myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                myBinding.MaxReceivedMessageSize = 999999999;
                System.TimeSpan timeout = new TimeSpan(0, 5, 0);
                myBinding.SendTimeout = timeout;
                EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
                //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
                MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

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
                MagentoAPIv2.catalogInventoryStockItemUpdateEntity itemUpdate;
                int intResponse = 0;
                if (lstItems.Length > 0)
                {
                    foreach (Item_Service.Item item in lstItems)
                    {
                        itemUpdate = new MagentoAPIv2.catalogInventoryStockItemUpdateEntity();
                        //NOT NEEDED FOR ELLISON SO COMMENTING OUT
                        /*
                        if (item.Has_Variants) //this means we treat each variant like its own item
                        {
                            ItemVariant.ItemVariants_Service itemVServ = new ItemVariant.ItemVariants_Service();
                            itemVServ.Url = NAVWebURL + "/Page/ItemVariants";
                            itemVServ.UseDefaultCredentials = true;
                            itemVServ.Credentials = setCredentials(strUsername, strPassword, strDomain);
                            ItemVariant.ItemVariants_Filter itemVFilter = new ItemVariant.ItemVariants_Filter();
                            itemVFilter.Field = ItemVariant.ItemVariants_Fields.Item_No;
                            itemVFilter.Criteria = item.No;
                            ItemVariant.ItemVariants_Filter[] lstItemVFilter = new ItemVariant.ItemVariants_Filter[] { itemVFilter };
                            ItemVariant.ItemVariants[] lstItemVariants = itemVServ.ReadMultiple(lstItemVFilter, null, 0);
                            if (lstItemVariants.Length > 0)
                            {
                                foreach (ItemVariant.ItemVariants itemVar in lstItemVariants)
                                {
                                    itemUpdate = new MagentoAPIv2.catalogInventoryStockItemUpdateEntity();
                                    itemUpdate.qty = itemVar.Inventory.ToString();
                                    if (itemVar.Inventory > 0)
                                    {
                                        itemUpdate.is_in_stock = 1;
                                        itemUpdate.is_in_stockSpecified = true;

                                    }
                                    else
                                    {
                                        itemUpdate.is_in_stock = 0;
                                        itemUpdate.is_in_stockSpecified = false;
                                        itemUpdate.qty = "0";
                                    }
                                    if (!String.IsNullOrEmpty(itemVar.EcommerceID))
                                    {
                                        intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, itemVar.EcommerceID, itemUpdate);
                                    }

                                }
                            }

                        }
                         * */

                        itemUpdate.qty = item.Inventory.ToString();

                        if (item.Inventory > 0)
                        {
                            itemUpdate.is_in_stock = 1;
                            itemUpdate.is_in_stockSpecified = true;
                        }
                        else
                        {
                            itemUpdate.is_in_stock = 0;
                            itemUpdate.is_in_stockSpecified = false;
                            itemUpdate.qty = "0";
                        }

                        if (!String.IsNullOrEmpty(item.eCommerceID))
                        {
                            intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, item.eCommerceID, itemUpdate);
                        }
                        //int intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, "150", itemUpdate);

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



//---------------------------------- Update existing orders that have a status of Processing in Magento
//v2.01f split this out from the section above when applying the filters



#region not known if this is needed
/*
 * 
 *   //v2.01f restrict the filter to orders with a status of Pending 
 *   
 * 
 * 
 * 
MagentoAPIv2.complexFilter complexFilter2 = new MagentoAPIv2.complexFilter();
complexFilter2.key = "Status";  
MagentoAPIv2.associativeEntity assEnt2 = new MagentoAPIv2.associativeEntity();
assEnt2.key = "status";
assEnt2.value = "processing";

complexFilter.value = assEnt2;
MagentoAPIv2.complexFilter[] lstComplexFilter2 = new MagentoAPIv2.complexFilter[1];
lstComplexFilter2[1]= complexFilter2;
mf.complex_filter = lstComplexFilter2;
MagentoAPIv2.salesOrderListEntity[] soe2 = magev2.salesOrderList(strSessionID,mf);
 * 
 * 
 * 
// v2.02b Only do this if the DownloadProcessing flag is false
if ((soe2.Length > 0) && DownloadProcessingStatusOrders == false) {
foreach (MagentoAPIv2.salesOrderListEntity msoe in soe2) {             
if (msoe.status.ToString().ToLower() == "processing") {
// Do SQL Query to see if SO is invoiced in NAV 

ToolStripStatusLabel1.Text = "Updating Magento OrderID " + msoe.increment_id + "....."
Me.StatusStrip1.Refresh()

Dim ConnString As String
ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
Dim con As New SqlConnection(ConnString)
Dim cmd As New SqlCommand("SELECT No_ FROM [" & NAVCompanyName.Text & "$Sales Header] WITH (NOLOCK) where [External Document No_] = '" & msoe.increment_id & "'", con)
con.Open()
Dim sdr As SqlDataReader = cmd.ExecuteReader()
If Not sdr.HasRows Then
Try
    Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "complete", "", 0)
    Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
    SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
Catch ex4 As Exception
    MessageBox.Show("Couldn't update order status to Complete for Magento orderID " + msoe.order_id + "     " + ex4.ToString())
End Try
Else
sdr.Read()
NAVOrderNo = sdr.GetString(0)
con.Close()
Dim cmd2 As New SqlCommand("SELECT sum(Quantity), sum([Quantity Invoiced]) FROM [" & NAVCompanyName.Text & "$Sales Line] WITH (NOLOCK) where [Document Type] = 1 and [Document No_] = '" & NAVOrderNo & "'", con)
con.Open()
Dim sdr2 As SqlDataReader = cmd2.ExecuteReader()
If sdr2.HasRows Then
    sdr2.Read()
    If sdr2.GetDecimal(0) = sdr2.GetDecimal(1) Then
        ' If SO in NAV is fully invoiced, update order record to Completed
        Try
            Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "complete", "", 0)
            Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
            SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
            OrdersUpdated = OrdersUpdated + 1

        Catch ex3 As Exception
            MessageBox.Show("Couldn't update order status to Complete for Magento orderID " + msoe.order_id + "     " + ex3.ToString())
        End Try
    End If
End If
End If
con.Close()
}
}
}

'---------------------------------- End of download/update - give feedback on number of records

Me.Cursor = Cursors.Default

ToolStripStatusLabel1.Text = "Order download complete - " & OrdersInserted & " new orders created, " & OrdersUpdated & " Magento order statuses updated."
Me.StatusStrip1.Refresh()
 **/

#endregion



//----------------------------------------










