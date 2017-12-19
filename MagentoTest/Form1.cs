using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MagentoTest
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //string strResult = getSessionID("http://www.moonclimbing.com", "NAVConnect", "NAVConnect");
            string strResult = getSessionID("http://us.sizzix.qa.ellison.com", "NavConnect", "NAVConnect123");
            
        }

        public string getSessionID(string MagentoURL, string MagentoAPIUser, string MagentoAPIKey)
        {
            string mLogin = string.Empty;

            BasicHttpBinding myBinding = new BasicHttpBinding();
            myBinding.Name = "Mage_Api_Model_Server_Wsi_HandlerPort";
            myBinding.Security.Mode = BasicHttpSecurityMode.None;
            myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
            myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
            myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
            myBinding.MaxReceivedMessageSize = 999999999;
            EndpointAddress endPointAddress = new EndpointAddress(MagentoURL + "/index.php/api/v2_soap/index/");
            //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
            MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);



            //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.ToString() + "/index.php/api/v2_soap/index/");
            
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

        private void button2_Click(object sender, EventArgs e)
        {
             
            try
            {
                // Set the Magento API Endpoint
                string strResult = getSessionID("http://www.moonclimbing.com", "NAVConnect", "NAVConnect");
                //Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
                BasicHttpBinding myBinding = new BasicHttpBinding();
                myBinding.Name = "BasicHttpBinding_IService1";
                myBinding.Security.Mode = BasicHttpSecurityMode.None;
                myBinding.Security.Transport.ClientCredentialType = HttpClientCredentialType.None;
                myBinding.Security.Transport.ProxyCredentialType = HttpProxyCredentialType.None;
                myBinding.Security.Message.ClientCredentialType = BasicHttpMessageCredentialType.UserName;
                myBinding.MaxReceivedMessageSize = 999999;
                EndpointAddress endPointAddress = new EndpointAddress("http://www.moonclimbing.com" + "/index.php/api/v2_soap/index/");
                //MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient magev2 = new MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient(myBinding, endPointAddress);
                MagentoAPIv2.PortTypeClient magev2 = new MagentoAPIv2.PortTypeClient(myBinding, endPointAddress);

                MagentoAPIv2.filters mf = new MagentoAPIv2.filters();
                MagentoAPIv2.catalogProductListRequest ProductListRequest = new MagentoAPIv2.catalogProductListRequest(strResult, mf, "");
                MagentoAPIv2.catalogProductEntity[] ItemEntityList = magev2.catalogProductList(strResult, mf, "");
                textBox1 .Text = ItemEntityList.Length.ToString();
            }
            catch (Exception exc)
            {
                MagentoAPIv2.catalogProductEntity[] ItemEntityList = null;
             

            }
        /*
        int ProductsUpdated = 0;

        string ConnString = string.Empty;
        ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text

        If ItemEntityList.Length > 0 Then
            For Each ItemEntity As MagentoAPIv2.catalogProductEntity In ItemEntityList
                Try
                    ToolStripStatusLabel1.Text = "Updating.... " & ItemEntity.sku
                    Me.StatusStrip1.Refresh()

                    Dim SQLcon As New SqlConnection(ConnString)
                    Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Item] set ecommerceID = '" & ItemEntity.product_id & "' where [No_] = '" & ItemEntity.sku & "'"
                    SQLcon.Open()
                    Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                    UpdateCmd.ExecuteNonQuery()
                    SQLcon.Close()
                    ProductsUpdated = ProductsUpdated + 1
                Catch ex As Exception
                End Try
            Next
        End If


        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Product resync complete - " & ProductsUpdated & " products updated."
         */
        
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string strResult = getSessionID("http://www.moonclimbing.com", "NAVConnect", "NAVConnect");
            int ecommID = 0, ecommAddrID = 0;
            string strError = string.Empty;
            UploadCustomersToolStripMenuItem(strResult, "http://www.moonclimbing.com",
                "XXXX",
                "xxx",
                "1 somewhere road",
                "nowhere town",
                "Sheffield",
                "Bill Gates",
                "01246 201102",
                "S41 2wn",
                "Yorkshire",
                "chris.monck@azzure-it.com",
                "01246 234432",
                "UK",
                "", "", ref ecommID, ref ecommAddrID, ref strError);
        }

        public void UploadCustomersToolStripMenuItem(string strSessionID, string MagentoURL,
            string CustomerNo, string CustomerName, string CustomerAddress, string CustomerAddress2, string CustomerCity,
            string CustomerContact, string CustomerPhoneNo, string CustomerPostCode, string CustomerCounty,
            string CustomerE_Mail, string CustomeFaxNo, string CustomerCountry, string CustomereCommerceID, string CustomereCommerceAddressID,
            ref int intEcommerceID, ref int intEcommerceAddressID, ref string strError)
        {
          //  try
           // {   // Set the Magento API Endpoint
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

                //2.02d error: "Index was outside the bounds of the array"
                //Dim Name() As String = Split((sdr.GetString(5)), " ")
                //CustomerRecord.firstname = Name(0)
                //CustomerRecord.lastname = Name(1) 
                //2.02d that causes an error when the field GetString(5)=contact is blank or one word 
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
               // CustomerRecord.store_idSpecified = true;
              //  CustomerRecord.store_id = 1;
               // CustomerRecord.website_idSpecified = true;
              //  CustomerRecord.website_id = 1;
                //2.02d


                // V1.03d Replace old password details with random generated 10 character password
                if (string.IsNullOrEmpty(CustomerID))
                {
                    //CustomerRecord.password = "password"
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
                       // try
                      //  {
                            intEcommerceID = magev2.customerCustomerCreate(strSessionID, CustomerRecord);
                            RecordsInserted = RecordsInserted + 1;
                            intCustomerID = intEcommerceID;
                      //  }
                       // catch (Exception exc2)
                        //{
                            // if it fails, try to update the record
                        //    RecordsFailed = RecordsFailed + 1;
                       //     strError = strError + exc2.InnerException.Message.ToString();//"Failed: customer no: " + CustomerID + " ";
                       // }
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

            //}
            //catch (Exception overallE)
           // {
            //    strError = strError + "issue with connection";
            //}


        }

        private void button4_Click(object sender, EventArgs e)
        {
          
            //dev MAG
            //string strResult = getSessionID("http://us.sizzix.qa.ellison.com", "NavConnect", "NAVConnect123");
            //LIVE MAG
            string strResult = getSessionID("http://www.sizzix.com", "NavConnect", "NAVConnect123");

            Magento_NAV_Integration.MagentoHelper mag = new Magento_NAV_Integration.MagentoHelper();
            //DEV NAV
            //mag.DownloadOrders(strResult, "http://us.sizzix.qa.ellison.com", "http://192.168.10.12:9047/EllisonDev/WS/Ellison%20Europe%20Ltd.", true, "", "WREXHAM", string.Empty, "1040", "azzure.support", "Balloon/8", "Ellison", true, "Open", "Processing");
            //LIVE NAV LIVE MAG
            mag.DownloadOrders(strResult, "http://www.sizzix.com", "http://192.168.10.12:7047/DynamicsNAV80/WS/Ellison%20Europe%20Ltd.", false, "", "WREXHAM", string.Empty, "1040", "azzure.support", "Balloon/8", "Ellison", true, "Open", "Processing");
            string strError = string.Empty;
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            String strTest = "test";
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration.MagentoVersion2Helper magentoVersion2 = new Magento_Version2_NAV_Integration.MagentoVersion2Helper();
            int intECom = 0;
            string strError = string.Empty;
            /*

            magentoVersion2.UploadItems(
                  "AZCMTEST01",
           "test123",
            1,             
            "test123",
            "test123",
            "test123",
            1,
            "",
            "NO VAT",
            "simple",
            1,
            ref intECom, 
            ref strError);
            //mag2.UpdateItem()
             * */
        }

        private void button7_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.getSessionID_B2B("https://www.sizzix.co.uk/soap?",
                "Azzure",
                "2dmwyw8MghwFeN");
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration.MagentoVersion2Helper();
            int intCount = 0;
            //   Magento_Version2_NAV_Integration.MagentoProductCatalog.CatalogDataProductInterface[] _lstCatalog = mage.ResyncProdIDsToNAVToolStripMenuItem("Azzure", "2dmwyw8MghwFeN",ref intCount);
          //  textBox2.Text = _lstCatalog.Length.ToString();

        }

        private void button9_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration.MagentoVersion2Helper();
            
            mage.DownloadOrders(
             //   "https://sizzix-azzure.c3preview.co.uk/soap/b2c_uk?",
                //"https://sizzix.c3preview.co.uk/soap/b2c_uk?",
            //    "https://www.sizzix.co.uk/soap?",
            "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure", 
                "2dmwyw8MghwFeN",
                "http://192.168.10.12:7047/DynamicsNAV80/WS/Ellison%20Europe%20Ltd.",
                false, 
                "",
                "WREXHAM", 
                string.Empty, 
                "1040", 
                "azzure.support", 
                "Balloon/8", 
                "Ellison",
                false,
                "open",
                "processing",
                true); //for logging
        }

        private void button10_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration.MagentoVersion2Helper();
            mage.UpdateOrderToShipped(
                 //"https://sizzix.c3preview.co.uk/soap/b2c_uk?",
                 "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                 "Azzure",
                "2dmwyw8MghwFeN",
                "400002706",
                "pending_review",
                "This is a comment"
                );
        }

        private void button11_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            bool bResponse = mage.downloadCustomers(
                //"https://sizzix-aws.c3preview.co.uk/soap/b2c_uk?",
                //    "https://sizzix-b2b.c3preview.co.uk/soap?",
                // "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
                //  "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
                "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                 "Azzure",
                 "2dmwyw8MghwFeN",
                "http://localhost:9147/EllisonDev/WS/Ellison%20Europe%20Ltd.",                
               "azzure.support",
               "Balloon/8",
               "Ellison");
        }

        private void button12_Click(object sender, EventArgs e)
        {

            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.downloadPreOrdersMagentoToNAV(
                // "https://sizzix-b2b.c3preview.co.uk/soap?",
            "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
               "http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               "azzure.support",
               "Balloon/8",
               "Ellison",
               "", 
                "WREXHAM", 
               "");
        }

        private void button13_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.updateMagentoCustomer
                (
                 // "https://sizzix-b2b.c3preview.co.uk/soap?",
                 "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
               "http://localhost:9147/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               "azzure.support",
               "Balloon/8",
               "Ellison"               
                );
        }

        private void button14_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            int intEcommerceID = 0;
            string strError = string.Empty;
            mage.UploadItems(
                 // "https://sizzix-b2b.c3preview.co.uk/soap?",
                   "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                "CMUUU123",
                "NEW ITEM TEST",
                100,
                120,
                "",
                //1,
                //2,
                //3,
                //1,
                "ANIMALS",
                "MACHINE",
                "",
                "Brenda Pinnick",
                "Design DIms",
                "Pre-Release",
               // 1, 2, 3, 4,
                "3DTI", 
                "",  //item discount group
                "Wrexham",
                0,
                1,
                "",
                System.DateTime.Now,
                "",
                "Brenda Pinnick",
                (float)150.50,
                1,
                ref intEcommerceID,
                ref strError
                );


                
                
                
        }

        private void button15_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.UpdateOrderToShipped(
                // "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
                 //"https://sizzix-b2b.c3preview.co.uk/soap/all?",
                 "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                "3000000055",
                "complete",
                "",
                "track123",
                "dhl");
                //string strMagentoURL,string strUser, string strPass, string strIncrementID, string strShippedStatus, string strComment)
            

        }

        private void button16_Click(object sender, EventArgs e)
        {
            string strError = string.Empty;
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.UpdateItem
                (

                "https://sizzix-b2b.c3preview.co.uk/soap/all?", //web service
                "Azzure", //username
                "2dmwyw8MghwFeN", //password               
             "JAMIE TEST PRODUCT 2", //VVV123", //sku
                "26329", //product id
                "update Description", //name
                0, //weight
                0,//gross weight
               "",// "Dimensions",              
               "XMAS",// "Birthday", //theme
               "SIZZIX PaddlePunch",// technology
               "XXX",//"Sizzix Textured Impressions",// sub technology
               "Brenda Walton",// designer
                "",//design dims                
                "Inactive",  //life cycle
                "PLAN",// planogram,
                "", //item desc. group
                "Wrexham",  //virtual warehouse               
                0, //boxqty
                0,//pallet qty
                "chapter", //chapter                
                System.DateTime.Now, //release date
                "die", //product type                
                "SIZZIX",//brand,
                (float)150.50, //price
                1, // 
                ref strError);

        }

        private void button17_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.SyncInteractions(
                 //"https://sizzix-b2b.c3preview.co.uk/soap?",
                 "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                 "http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               "azzure.support",
               "Balloon/8",
               "Ellison"
                );
        }

        private void button18_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            bool bResult = mage.UploadAccountCoordinator(
                //"https://sizzix-b2b.c3preview.co.uk/soap?",
                "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                // "http://localhost:9047/EllisonDev/WS/Ellison%20Europe%20Ltd.",
                "http://192.168.10.12:9147/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               "azzure.support",
               "Balloon/8",
               "Ellison"
                );

        }

        private void button19_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.DownloadOrders(
                //   "https://sizzix-azzure.c3preview.co.uk/soap/b2c_uk?",
                //"https://sizzix.c3preview.co.uk/soap/b2c_uk?",
               //"https://sizzix-aws.c3preview.co.uk/soap/b2c_uk?",
              // "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
              "https://sizzix-b2b.c3preview.co.uk/soap/all?",
              
               "Azzure",
               "2dmwyw8MghwFeN",
               "http://localhost:9147/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               false,
               "",
               "WREXHAM",
               string.Empty,
               "1040",
               "azzure.support",
               "Balloon/8",
               "Ellison",
               true,
                // "inprocess",
                "Open",
               "processing");
        }

        private void button20_Click(object sender, EventArgs e)
        {
            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.uploadSalesDiscounts(
                // "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
                "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                 "http://localhost:9147/EllisonDev/WS/Ellison%20Europe%20Ltd.",
               "azzure.support",
               "Balloon/8",
               "Ellison"
               );
        }

        private void button21_Click(object sender, EventArgs e)
        {
            FileStream fs = new FileStream("C:\\Azzure\\ellisonEuropeLogo.gif", FileMode.Open, FileAccess.Read);

            // Create a byte array of file stream length
            byte[] ImageData = new byte[fs.Length];

            //Read block of bytes from stream into the byte array
            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            string strBytes = System.Convert.ToBase64String(ImageData);
                //System.Text.Encoding.UTF8.GetString(ImageData);

            
        }

        private void button22_Click(object sender, EventArgs e)
        {

            FileStream fs = new FileStream("C:\\Azzure\\ellisonEuropeLogo.gif", FileMode.Open, FileAccess.Read);

            // Create a byte array of file stream length
            byte[] ImageData = new byte[fs.Length];

            //Read block of bytes from stream into the byte array
            fs.Read(ImageData, 0, System.Convert.ToInt32(fs.Length));

            //Close the File Stream
            fs.Close();
            string strBytes = System.Convert.ToBase64String(ImageData);

            Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper mage = new Magento_Version2_NAV_Integration_Sizzix.MagentoVersion2Helper();
            mage.submitInteractionToMagento(
                //  "https://sizzix-b2b.c3preview.co.uk/soap/b2b_gbp?",
                "https://sizzix-b2b.c3preview.co.uk/soap/all?",
                "Azzure",
                "2dmwyw8MghwFeN",
                "503609", "Chris Test", "This is the subject with attachment", "This is the body with attachment", strBytes, "Ellison Logo", "gif");
        }
    }
}

