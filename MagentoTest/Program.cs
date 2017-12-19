using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MagentoTest
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }

        private static NetworkCredential setCredentials(string strUser, string strPassword, string strDomain)
        {
            System.Configuration.AppSettingsReader asr = new System.Configuration.AppSettingsReader();
            return new NetworkCredential(strUser, strPassword, strDomain);
        }


        public static string DownloadOrders(string strSessionID, string MagentoURL, string NAVWebURL, bool DownloadProcessingStatusOrders, string NAVCusAcc, string WHSupplyLocationCode, string strDefaultCustomer, string DiscountGLAccount, string strUsername, string strPassword, string strDomain, bool bEbay, bool bAmazon, bool bMagento, bool bUpdateMagentoOrders)
        {

            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.Name = "BasicHttpBinding_IService1";
            myBinding.Security.Mode = BasicHttpSecurityMode.None;
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            System.TimeSpan timeout = new TimeSpan(0, 5, 0);
            myBinding.SendTimeout = timeout;
            myBinding.MaxReceivedMessageSize = 999999;
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
            //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
            MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);
           
            //----------------------- Create new orders for pending items in Magento ---------------------------------------

            // 2.02b Define a tax rate %
            int tax_rate = 0;

            //v2.01f restrict the filter to orders with a status of Pending
            MagentoAPIv2.filters mf = new MagentoAPIv2.filters();
            MagentoAPIv2.complexFilter complexFilter = new MagentoAPIv2.complexFilter();
            //complexFilter.key = "Status";
            MagentoAPIv2.associativeEntity assEnt = new MagentoAPIv2.associativeEntity();
            assEnt.key = "status";
            assEnt.value = "pending";
            //complexFilter.key = "Increment_ID";
            //assEnt.key = "Increment_ID";
            //assEnt.value = "100000039-1";

            //v2.02b If we want to download processing orders, then change the filter
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
                                            if (!string.IsNullOrEmpty(strDefaultCustomer))
                                            {
                                                newNAVSOrder.Sell_to_Customer_No = strDefaultCustomer; /// usually customer no. 999
                                            }
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
                                //Store the amazon/ebay/magento number                                
                                if (bEbay)
                                {
                                    newNAVSOrder.Ebay_Order_Number = SOI.store_id;
                                }
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
                                          

                                            strItemNo = nmFuncs.CheckIfItemExists(strSku, SOI.items[idx].product_id, strName, bBundle,strVariant);
                                            //we need to use thestrItemNo for the sales line
                                            newNAVSOrder.SalesLines[SalesOrderIDX].No = strItemNo;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Variant_Code = strVariant;
                                            //newNAVSOrder.SalesLines[SalesOrderIDX].No = SOI.items[idx].sku;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description = strName;
                                            newNAVSOrder.SalesLines[SalesOrderIDX].Description_2 = strSku;
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
                                            if (decimal.TryParse(SOI.items[idx].base_original_price, out decPrice))
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = decPrice;
                                            }
                                            else
                                            {
                                                newNAVSOrder.SalesLines[SalesOrderIDX].Unit_Price = 0;
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
                                            nmFuncs.SetAssemblyComponent(strAssemblyNo, strSku, decQty, strName, strSku,strVariant);

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
                                    // 13/4/12 CS Update the order status to processing
                                    try
                                    {      //need to skip this if in test mode!!!
                                        //v2.02b If we want to download processing orders, then change the filter
                                        if (bUpdateMagentoOrders)
                                        {
                                            if (DownloadProcessingStatusOrders == true)
                                            {
                                                magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "complete", "", 0);
                                                OrdersUpdated = OrdersUpdated + 1;
                                            }
                                            else
                                            {
                                                magev2.salesOrderAddComment(strSessionID, MagentoSalesOrder.increment_id, "processing", "", 0);
                                            }
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

        public static bool uploadInventoryLevels(string strSessionID, string MagentoURL, string NAVWebURL, string strUsername, string strPassword, string strDomain)
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

                //Create Service Reference 
                Item_Service.Item_Service itemService = new Item_Service.Item_Service();
                itemService.Url = NAVWebURL + "/Page/Item";
                itemService.UseDefaultCredentials = true;
                itemService.Credentials = setCredentials(strUsername, strPassword, strDomain);
                Item_Service.Item_Filter itemFilter = new Item_Service.Item_Filter();
                itemFilter.Field = Item_Service.Item_Fields.ecommerce_Enabled;
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

                        if (!String.IsNullOrEmpty(item.ecommerceID))
                        {
                            intResponse = magev2.catalogInventoryStockItemUpdate(strSessionID, item.ecommerceID, itemUpdate);
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



        public static MagentoAPIv2.catalogProductEntity[] ResyncProdIDsToNAVToolStripMenuItem(string strSessionID, string MagentoURL, ref int intCount)
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


        public static void UploadCustomersToolStripMenuItem(string strSessionID, string MagentoURL,
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
                            strError = strError + exc2.InnerException.Message.ToString();//"Failed: customer no: " + CustomerID + " ";
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
                            strError = strError + "Failed: customer address no: " + CustomerAddressID + " ";
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
                            strError = strError + exc2.InnerException.Message.ToString(); //"Failed: customer no: " +  CustomerID + " ";
                        }
                    }
                } //end else

            }
            catch (Exception overallE)
            {
                //strError = strError + "issue with connection";
                strError = strError + overallE.InnerException.Message.ToString();
            }

            #region oldcode
            /*
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
                MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);

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
                }else
                {
                    CustomerID = string.Empty;

                }
       
                CustomerAddressID = CustomereCommerceAddressID; //CustomerAddressID = Val(sdr.GetString(13))
                CustomerRecord.email = CustomerE_Mail; //email - can't be a duplicate otherwise createcustomer fails!!

                //2.02d error: "Index was outside the bounds of the array"
                //Dim Name() As String = Split((sdr.GetString(5)), " ")
                //CustomerRecord.firstname = Name(0)
                //CustomerRecord.lastname = Name(1) 
                //2.02d that causes an error when the field GetString(5)=contact is blank or one word 
                string[] lstName = CustomerContact.Split(new Char [] {' '});
                if (lstName.Length > 1){
                    CustomerRecord.firstname = lstName[0];
                    CustomerRecord.lastname = lstName[1];
                }else{
                    CustomerRecord.firstname = CustomerContact;
                }
              

                //check the value in Magento to assig the right value at 'Associate to Website' by exporting a customer in csv
                //CustomerRecord.store_idSpecified = true;
               // CustomerRecord.store_id = 1;
                //CustomerRecord.website_idSpecified = true;
                //CustomerRecord.website_id = 1;
                //2.02d


                // V1.03d Replace old password details with random generated 10 character password
                if (string.IsNullOrEmpty(CustomerID)) {
                    //CustomerRecord.password = "password"
                    //New code to generate a random password
                    NewPassword = "";
                    Random ran = new Random();
                    for (int i = 1; i <= 2; i++)
                    {
                        NewPassword =  NewPassword + (ran.Next(1, 9999) * 10).ToString();
                    }
                    CustomerRecord.password = NewPassword;
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
                if (!Int32.TryParse(CustomerID, out intCustomerID)){

                    intCustomerID = 0;                 
                }else{
                    Int32.TryParse(CustomerID, out intCustomerID);
                }
                //CUSTOMER RECORD
                if (!string.IsNullOrEmpty(CustomerID))
                {
                    //Update CUSTOMER
                    try
                    {   //magev2.customerCustomerUpdate(strSessionID, CustomerID, CustomerRecord);
                        
                        magev2.customerCustomerUpdate(strSessionID,intCustomerID,CustomerRecord);
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
                              // if it fails, try to update the record
                              RecordsFailed = RecordsFailed + 1;
                              strError = strError + exc2.InnerException.Message.ToString();//"Failed: customer no: " + CustomerID + " ";
                          }
                      }   
                   } //end else

                   //CUSTOMER ADDRESS RECORD  The Customer address ID is already set then update the record
                   if (!string.IsNullOrEmpty(CustomerAddressID))
                   {
                       int intCustomerAddressID;
                        if (!Int32.TryParse(CustomerAddressID, out intCustomerAddressID)){

                            intCustomerAddressID = 0;                 
                        }else 
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
                                strError = strError + "Failed: customer address no: " + CustomerAddressID + " ";
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
                                  strError = strError + exc2.InnerException.Message.ToString(); //"Failed: customer no: " +  CustomerID + " ";
                              }
                          }   
                       } //end else
               
            }
            catch (Exception overallE)
            {
                 strError = strError + "issue with connection";
            }
            */
            #endregion
        }

    }
}
