' 2.01d 24/11/12  CS   Amended so that the Magento API User and Key is stored in Registry from setting screen.  Also changed defaults to be non-customer specific.
' 2.01e 06/12/12  CS   Amended because Magento API was not setting the "endpoint" in each function so was not working properly for other clients.  Also removed
'                      reference to LastSalesOrderID
' 2.01f 21/12/12  CS   If "Template Item" flag on NAV item is set to no (0) then it will upload to Magento as a "simple" product.
' Renamed to 2.02a     When downloading orders, if the item number doesn't exist then look at the vendor cross reference to see if we can find it there.
'                      Uses the flag to specifiy that orders should all be downloaded to a single customer account
'                      If the order line quantity is 0 then it doesn't add the line to NAV
'                      Adds a discount line to the order in NAV if there is a discount on the Magento order
'                      Refreshes home screen when changing settings on the settings page
'                      If item record isn't found, still populates the items description on the order - even though using 999 for item no
'                      Now downloads new orders first (filter with a status of pending using a filter) then updated orders with a status of processing
' 2.02b 24/1/13  CS    Made following changes for CN Creative:
'                      - Include Order No on the Item Doesn't exist message
'                      - When item doens't exist, put Magento SKU in description2
'                      - Populate the Bill-to-contact with the contact from Magento
'                      - Add a new parameter for "Download all orders with Processing status and complete immediately"
'                      - Need to deduct VAT from discount if it is included - Magento doesn't tell us this so had to put logic in code to try a figure out.
' 2.02c 15/2/14  CS   Upload products/items wasn't working - was failing with a "cast error".  Found this was because it was
'                     using "Template Item" column on an Item to decide if it was a simple or configured product.  
'                     Changed this to use a unique field "ecommerce Product Type" and default to simple.
'                     also removed hard coded product categories.  Also changed the Upload Items/Products to only upload
'                     new items/products to prevent overwriting details added to items in Magento.
' 2.02d 12/04/14  CS  Upload Customers wasn't working.  Found an issue with the SQL Command to query customers from NAV.
' 2.02d 14/04/14  MS  After the previous correction, we've got the error "Index was outside the bounds of the array" because of the contact array field
' 2.02e 23/07/2014 MS Nutriculture, when is 'single customer account' is not inserted the bill and ship address from the customer. 
' 2.02f 20/10/2014 MS Add County field in sales order 


Imports System.Data.SqlClient
Imports Microsoft.Win32


Public Class MainForm
    
    Dim magev2 As New MagentoAPIv2.PortTypeClient

    Dim excMsg As String

    'Dim sessionId As String
    Dim mlogin As String
    Dim WHSupplyLocationCode As String
    Dim MagentoAPIUser As String
    Dim MagentoAPIKey As String

    Dim SingleCusAccount As String
    Dim NAVCusAcc As String
    Dim DiscountGLAccount As String
    'v2.02 
    Dim DownloadProcessingStatusOrders As Boolean

    Dim Result As Integer
    Dim ref As Object


    Private Sub Button1_Click(sender As System.Object, e As System.EventArgs)
        ' Login and get a session ID
        GetSessionId()

    End Sub
    Private Sub UpdateInvetory()
        'v1.03d
        'Query the NAV Invetory Table and upload the free stock figure

        'MagentoID from Item is ProductID
        'Data is Qty, Is_In_Stock
        'Dim x As New MagentoAPIv2.catalogInventoryStockItemUpdateRequest(productId, Data)


    End Sub


    Private Function GetSessionId()

        If mlogin <> "" Then
            Return mlogin
        End If

        ToolStripStatusLabel1.Text = "Logging into Magento API...."
        Me.StatusStrip1.Refresh()

        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        
        Dim loginResponse As New MagentoAPIv2.loginResponse()
        ' 2.01d CS AMended so it uses the Magento API details stored in Registry
        'Dim loginRequest As New MagentoAPIv2.loginRequest("NAVConnect", "NAVConnect")
        Dim loginRequest As New MagentoAPIv2.loginRequest(MagentoAPIUser, MagentoAPIKey)

        Try
            loginResponse = magev2.login(loginRequest)
        Catch exc As Exception
            excMsg = exc.ToString
            MessageBox.Show(excMsg)
            End
        End Try
        mlogin = loginResponse.result.ToString

        ToolStripStatusLabel1.Text = ""

        Return mlogin


    End Function

    Private Sub UploadProducts_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)


    End Sub

    Private Sub DownloadOrders_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles DownloadOrders.Click

        Me.Cursor = Cursors.WaitCursor

        ' Get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ToolStripStatusLabel1.Text = "Retrieving new orders from Magento....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        '----------------------- Create new orders for pending items in Magento ---------------------------------------
        'Dim mf As New MagentoAPIv2.filters()
        'Dim mf As New MagentoAPIv2.filters

        'mf.filter(1) = New MagentoAPIv2.associativeEntity
        'mf.filter(1).key = "Status"
        'mf.filter(1).value = "Pending"

        ' 2.02b Define a tax rate %
        Dim tax_rate As Integer = 0

        'v2.01f restrict the filter to orders with a status of Pending
        Dim mf As New MagentoAPIv2.filters
        Dim complexFilter(0) As MagentoAPIv2.complexFilter
        complexFilter(0) = New MagentoAPIv2.complexFilter
        complexFilter(0).key = "Status"
        Dim assEnt As New MagentoAPIv2.associativeEntity
        With assEnt
            .key = "status"
            .value = "pending"
        End With
        'v2.02b If we want to download processing orders, then change the filter
        If DownloadProcessingStatusOrders = True Then
            assEnt.value = "processing"
        End If

        complexFilter(0).value = assEnt
        mf.complex_filter = complexFilter
        'v2.01d end of change

        Dim SalesOrderListRequest As New MagentoAPIv2.salesOrderListRequest(mlogin, mf)
        Dim SalesOrderListResponse As New MagentoAPIv2.salesOrderListResponse
        SalesOrderListResponse = magev2.salesOrderList(SalesOrderListRequest)
        Dim soe As MagentoAPIv2.salesOrderListEntity() = SalesOrderListResponse.result

        Dim OrdersInserted As Integer = 0
        Dim OrdersUpdated As Integer = 0

        Dim NAVOrderNo As String

        If soe.Length > 0 Then
            For Each msoe As MagentoAPIv2.salesOrderListEntity In soe
                'If msoe.increment_id > LastSalesOrderID.Text And msoe.status = "pending" Then
                ' If msoe.status = "pending" Then
                If msoe.status = "pending" Or (msoe.status = "processing" And DownloadProcessingStatusOrders = True) Then
                    Try
                        Dim SalesOrderInfoRequest As New MagentoAPIv2.salesOrderInfoRequest(mlogin, msoe.increment_id)
                        Dim SalesOrderInfoResponse As New MagentoAPIv2.salesOrderInfoResponse
                        SalesOrderInfoResponse = magev2.salesOrderInfo(SalesOrderInfoRequest)

                        Dim SOI As MagentoAPIv2.salesOrderItemEntity() = SalesOrderInfoResponse.result.items

                        If SOI.Length > 0 Then
                            ' List NAV Services
                            ' http://localhost:7047/DynamicsNAV/WS/CRONUS%20UK%20Ltd./services
                            ' Sales Order Service
                            ' http://localhost:7047/DynamicsNAV/WS/CRONUS%20UK%20Ltd/Page/SalesOrder

                            ToolStripStatusLabel1.Text = "Processing OrderID " + msoe.increment_id + "....."
                            Me.StatusStrip1.Refresh()

                            ' v2.01f if the Download to Single Customer Account is set then change the Customer Code to that specified
                            If NAVCusAcc > "" Then
                                msoe.customer_id = NAVCusAcc
                            End If


                            'Create Service Reference 
                            Dim SOservice As New SalesOrder_Service.SalesOrder_Service()
                            SOservice.Url = NAVWebURL.Text + "/Page/SalesOrder"
                            SOservice.UseDefaultCredentials = True

                            'Create Service Reference for customer
                            Dim CustService As New Customer_Service.Customer_Service()
                            CustService.Url = NAVWebURL.Text + "/Page/Customer"
                            CustService.UseDefaultCredentials = True

                            'Create the Order header 
                            Dim newOrder As New SalesOrder_Service.SalesOrder()
                            Try
                                SOservice.Create(newOrder)
                            Catch ex As Exception
                                MessageBox.Show("Can't connect to NAV WebService to create order." + "                     " + ex.ToString)
                                End
                            End Try

                            ' Update the header fields on the order
                            newOrder.Order_Date = msoe.updated_at
                            newOrder.External_Document_No = SalesOrderInfoResponse.result.increment_id
                            ' v2.02b populate the contact from the Magento Order
                            If SalesOrderInfoResponse.result.customer_lastname > "" Then
                                newOrder.Bill_to_Contact = SalesOrderInfoResponse.result.customer_firstname + " " + SalesOrderInfoResponse.result.customer_lastname
                            End If
                            ' Only populate current code if not local currency
                            If msoe.order_currency_code <> "GBP" Then
                                newOrder.Currency_Code = msoe.order_currency_code
                            End If

                            ' CS 1.02f - If customer_id is blank, use the billing address id.
                            If Not msoe.customer_id > "" Then msoe.customer_id = msoe.billing_address_id

                            ' v2.01f if a fixed customer account is specified then use that or use the one on the magento order
                            If NAVCusAcc > "" Then
                                newOrder.Sell_to_Customer_No = NAVCusAcc
                                ' 2.02e Added addresses for the customer though is single customer 
                                newOrder.Sell_to_Customer_Name = SalesOrderInfoResponse.result.customer_firstname + " " + SalesOrderInfoResponse.result.customer_lastname
                                newOrder.Sell_to_Contact = SalesOrderInfoResponse.result.customer_firstname + " " + SalesOrderInfoResponse.result.customer_lastname
                                newOrder.Sell_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.billing_address.street, 50)
                                'newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                newOrder.Sell_to_County = SalesOrderInfoResponse.result.billing_address.region '2.02f
                                newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                newOrder.Ship_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.shipping_address.street, 50)
                                'newOrder.Ship_to_Address_2 = SalesOrderInfoResponse.result.shipping_address.region
                                newOrder.Ship_to_City = SalesOrderInfoResponse.result.shipping_address.city
                                newOrder.Ship_to_County = SalesOrderInfoResponse.result.shipping_address.region '2.02f
                                newOrder.Ship_to_Post_Code = SalesOrderInfoResponse.result.shipping_address.postcode
                                newOrder.Ship_to_Contact = SalesOrderInfoResponse.result.customer_firstname + " " + SalesOrderInfoResponse.result.customer_lastname
                                newOrder.Ship_to_Name = SalesOrderInfoResponse.result.customer_firstname + " " + SalesOrderInfoResponse.result.customer_lastname
                                ' 2.02e Added addresses for the customer though is single customer 
                            Else
                                ' Query the customer table to get the customer code for the Magento CustomerID
                                If msoe.customer_id > "" Then
                                    Dim ConnString As String
                                    ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
                                    Dim con As New SqlConnection(ConnString)

                                    Dim cmd As New SqlCommand("SELECT No_ FROM [" & NAVCompanyName.Text & "$Customer] WITH (NOLOCK) where [ecommerceID]=" & msoe.customer_id, con)
                                    con.Open()
                                    Dim sdr As SqlDataReader = cmd.ExecuteReader()
                                    If sdr.HasRows Then
                                        sdr.Read()
                                        newOrder.Sell_to_Customer_No = sdr.GetString(0)
                                    Else
                                        'CS 1.03c removed next line to improve processing
                                        'MessageBox.Show("Customer doesn't exist in NAV.   Customer will be created.")
                                        'newOrder.Sell_to_Customer_No = "999"
                                        Dim newCustomer As New Customer_Service.Customer()
                                        Try
                                            CustService.Create(newCustomer)
                                        Catch ex As Exception
                                            MessageBox.Show("Can't connect to NAV WebService to create Customer." + "                     " + ex.ToString)
                                            End
                                        End Try
                                        newCustomer.Name = SalesOrderInfoResponse.result.billing_address.company
                                        newCustomer.Contact = SalesOrderInfoResponse.result.billing_address.firstname + " " + SalesOrderInfoResponse.result.billing_address.lastname
                                        If newCustomer.Name = "" Then
                                            newCustomer.Name = newCustomer.Contact
                                        End If
                                        newCustomer.Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.billing_address.street, 50)
                                        'newCustomer.Address_2 = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.billing_address.region, 50)
                                        newCustomer.City = SalesOrderInfoResponse.result.billing_address.city
                                        newCustomer.County = SalesOrderInfoResponse.result.billing_address.region '2.02f
                                        newCustomer.Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                        newCustomer.Phone_No = SalesOrderInfoResponse.result.billing_address.telephone
                                        newCustomer.E_Mail = SalesOrderInfoResponse.result.customer_email
                                        newCustomer.Country_Region_Code = SalesOrderInfoResponse.result.billing_address.country_id
                                        newCustomer.Location_Code = WHSupplyLocationCode
                                        If SalesOrderInfoResponse.result.order_currency_code <> "GBP" Then
                                            newCustomer.Currency_Code = SalesOrderInfoResponse.result.order_currency_code
                                        End If
                                        ' Determine if the customer is VATABALE by checking the country code
                                        Select Case newCustomer.Country_Region_Code
                                            Case "GB"
                                                newCustomer.Gen_Bus_Posting_Group = "DOMESTIC"
                                                newCustomer.VAT_Bus_Posting_Group = "DOMESTIC"
                                                newCustomer.Customer_Posting_Group = "DOMESTIC"
                                            Case "AT", "BE", "BG", "CY", "CZ", "DK", "EE", "FI", "FR", "DE", "EL", "HE", "IE", "IT", "LV", "LT", "LU", "MT", "NL", "PL", "PT", "SO", "SK", "SI", "ES", "SE"
                                                newCustomer.Gen_Bus_Posting_Group = "EU"
                                                newCustomer.VAT_Bus_Posting_Group = "EUINCVAT"
                                                newCustomer.Customer_Posting_Group = "EU"
                                            Case Else
                                                newCustomer.Gen_Bus_Posting_Group = "FOREIGN"
                                                newCustomer.VAT_Bus_Posting_Group = "FOREIGN"
                                                newCustomer.Customer_Posting_Group = "FOREIGN"
                                        End Select

                                        newOrder.Sell_to_Customer_No = newCustomer.No
                                        '13/4/12 CS Changed the below so it is consistent
                                        'newOrder.Sell_to_Address = SalesOrderInfoResponse.result.billing_address.street
                                        'newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                        'newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                        'newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                        newOrder.Sell_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.billing_address.street, 50)
                                        'newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                        newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                        newOrder.Sell_to_County = SalesOrderInfoResponse.result.billing_address.region '2.02f
                                        newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                        newOrder.Ship_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.shipping_address.street, 50)
                                        'newOrder.Ship_to_Address_2 = SalesOrderInfoResponse.result.shipping_address.region
                                        newOrder.Ship_to_City = SalesOrderInfoResponse.result.shipping_address.city
                                        newOrder.Ship_to_County = SalesOrderInfoResponse.result.shipping_address.region '2.02f
                                        newOrder.Ship_to_Post_Code = SalesOrderInfoResponse.result.shipping_address.postcode

                                        '13/4/12 CS Added following in as requested by Moon.
                                        newOrder.Requested_Delivery_Date = Today

                                        'cs 4/4 Amended to pass down prices including VAT
                                        'cs 23/10 v2.01b - changed to exclude VAT so pricing is correct in NAV for UK and overseas orders. Will now use default from customer record.
                                        'newOrder.Prices_Including_VAT = True
                                        CustService.Update(newCustomer)

                                        'Do an SQL update to update the eCommerceID and eCommerce Enabled flags on the customer record.
                                        Dim con2 As New SqlConnection(ConnString)
                                        con2.Open()
                                        Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Customer] set ecommerceID = '" & SalesOrderInfoResponse.result.customer_id & "', [eCommerce Enabled] = 1 where [No_] = '" & newCustomer.No & "'"
                                        Dim UpdateCmd As New SqlCommand(updateSql, con2)
                                        UpdateCmd.ExecuteNonQuery()
                                        con2.Close()

                                    End If
                                    con.Close()

                                Else
                                    MessageBox.Show("No customer ID is specified on the order.   Customer code 999 will be used.")
                                    newOrder.Sell_to_Customer_No = "999"
                                    newOrder.Sell_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.billing_address.street, 50)
                                    'newOrder.Sell_to_Address_2 = SalesOrderInfoResponse.result.billing_address.region
                                    newOrder.Sell_to_City = SalesOrderInfoResponse.result.billing_address.city
                                    newOrder.Sell_to_County = SalesOrderInfoResponse.result.billing_address.region '2.02f
                                    newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.billing_address.postcode
                                    'newOrder.Sell_to_Post_Code = SalesOrderInfoResponse.result.customer_email
                                    'CS 13/4/12 Added below to deal with ship to addresses.  Also limited the address to 50 characters.
                                    newOrder.Ship_to_Address = Microsoft.VisualBasic.Left(SalesOrderInfoResponse.result.shipping_address.street, 50)
                                    'newOrder.Ship_to_Address_2 = SalesOrderInfoResponse.result.shipping_address.region
                                    newOrder.Ship_to_City = SalesOrderInfoResponse.result.shipping_address.city
                                    newOrder.Ship_to_County = SalesOrderInfoResponse.result.shipping_address.region '2.02f
                                    newOrder.Ship_to_Post_Code = SalesOrderInfoResponse.result.shipping_address.postcode


                                End If
                            End If

                            ' Create the blank order lines


                            ' v2.01f - need to add extra records for discount and shipping
                            'newOrder.SalesLines = New SalesOrder_Service.Sales_Order_Line(SOI.Length) {}
                            Dim NoOfSOLines = SOI.Length - 1
                            If SalesOrderInfoResponse.result.discount_amount <> 0 Then
                                NoOfSOLines = NoOfSOLines + 1
                            End If
                            If SalesOrderInfoResponse.result.shipping_amount > 0 Then
                                NoOfSOLines = NoOfSOLines + 1
                            End If
                            newOrder.SalesLines = New SalesOrder_Service.Sales_Order_Line(NoOfSOLines) {}

                            ' v2.01a Change this so it adds a line for SHIPPING at end
                            ' For idx = 0 To (SOI.Length - 1)
                            'v2.01f user NoOfSOLines parameter instead
                            'For idx = 0 To (SOI.Length)
                            For idx = 0 To (NoOfSOLines)
                                Try
                                    newOrder.SalesLines(idx) = New SalesOrder_Service.Sales_Order_Line
                                Catch ex As Exception
                                    MessageBox.Show("Error inserting blank order lines onto order       ", ex.ToString)
                                End Try
                            Next
                            ' Update the order 
                            Try
                                SOservice.Update(newOrder)
                            Catch ex As Exception
                                MessageBox.Show("Unable to update order....     " + ex.ToString)
                            End Try

                            Dim LastSKU As String
                            LastSKU = ""
                            Dim SalesOrderIDX As Integer
                            SalesOrderIDX = 0

                            ' Populate the order line details
                            For idx = 0 To (SOI.Length - 1)
                                ' v2.01f if the line has a 0 qty then delete the line
                                If SOI(idx).base_price = 0 And SOI(idx).sku = LastSKU Then
                                    'SOservice.Delete_SalesLines(idx)
                                    LastSKU = ""
                                Else
                                    Try
                                        newOrder.SalesLines(SalesOrderIDX).Type = SalesOrder_Service.Type.Item
                                        newOrder.SalesLines(SalesOrderIDX).No = SOI(idx).sku
                                        ' v 2.01f This this is a configurable product, save the SKU so we can delete the following line for the simple product
                                        If SOI(idx).product_type = "configurable" Then
                                            LastSKU = SOI(idx).sku
                                        End If

                                        newOrder.SalesLines(SalesOrderIDX).Quantity = SOI(idx).qty_ordered
                                        newOrder.SalesLines(SalesOrderIDX).Unit_Price = SOI(idx).price
                                        'CS V2.01 Changed this back to use the Price field which excludes VAT because NAV then applies the VAT.  
                                        ' picked this up when testing the new SHIPPING functionality because shipping is only available without VAT.
                                        'newOrder.SalesLines(idx).Unit_Price = SOI(idx).original_price

                                        ' validate that the Item exists in NAV
                                        Dim ConnString As String
                                        ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
                                        Dim con As New SqlConnection(ConnString)
                                        Dim cmd As New SqlCommand("SELECT No_ FROM [" & NAVCompanyName.Text & "$Item] WITH (NOLOCK) where [No_]='" & SOI(idx).sku & "'", con)

                                        con.Open()
                                        Dim sdr As SqlDataReader = cmd.ExecuteReader()
                                        If Not sdr.HasRows Then
                                            ' MessageBox.Show("Item " & SOI(idx).sku & " doesn't exist in NAV.   Item No. 999 will be used")
                                            ' newOrder.SalesLines(idx).No = "999"
                                            ' 2.01f if item doesn't exist, look in Item Cross Reference table
                                            Dim ConnString4 As String
                                            ConnString4 = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
                                            Dim con4 As New SqlConnection(ConnString4)
                                            Dim cmd4 As New SqlCommand("SELECT [Item No_] FROM [" & NAVCompanyName.Text & "$Item Cross Reference] WITH (NOLOCK) where [Cross-Reference No_]='" & SOI(idx).sku & "'", con4)
                                            con4.Open()
                                            Dim sdr4 As SqlDataReader = cmd4.ExecuteReader()
                                            If sdr4.HasRows Then
                                                sdr4.Read()
                                                newOrder.SalesLines(SalesOrderIDX).No = sdr4.GetString(0)
                                            Else
                                                ' v2.02b MessageBox.Show("Item " & SOI(idx).sku & " doesn't exist in NAV.   Item No. 999 will be used")
                                                MessageBox.Show("OrderID " & msoe.increment_id & ", Item " & SOI(idx).sku & " doesn't exist in NAV.   Item No. 999 will be used")
                                                newOrder.SalesLines(SalesOrderIDX).No = "999"
                                                ' v2.01f if the product is not found, use 999 but still fill in the description from Magento.
                                                newOrder.SalesLines(SalesOrderIDX).Description = SOI(idx).name
                                                ' v2.02b populate desc 2 with the SKU
                                                newOrder.SalesLines(SalesOrderIDX).Description_2 = SOI(idx).sku

                                            End If
                                            con4.Close()
                                            ' 2.01f end of changes
                                        End If

                                        con.Close()


                                    Catch ex As Exception
                                        MessageBox.Show(ex.ToString)
                                    End Try
                                    SalesOrderIDX = SalesOrderIDX + 1
                                End If

                            Next

                            ' v2.01f only add a shipping line if there is a shipping amount on the order
                            If SalesOrderInfoResponse.result.shipping_amount > 0 Then
                                ' v2.01a Add the SHIPPING Line
                                newOrder.SalesLines(SalesOrderIDX).Type = SalesOrder_Service.Type.Item
                                newOrder.SalesLines(SalesOrderIDX).No = "SHIPPING"
                                newOrder.SalesLines(SalesOrderIDX).Description = SalesOrderInfoResponse.result.shipping_description
                                newOrder.SalesLines(SalesOrderIDX).Quantity = 1
                                ' v2.01f Use the correct shipping amount for the order currency
                                'newOrder.SalesLines(SOI.Length).Unit_Price = SalesOrderInfoResponse.result.base_shipping_amount
                                newOrder.SalesLines(SalesOrderIDX).Unit_Price = SalesOrderInfoResponse.result.shipping_amount
                                SalesOrderIDX = SalesOrderIDX + 1
                            End If

                            ' v2.01f If there is a discount, add the Discount line to the order
                            If SalesOrderInfoResponse.result.discount_amount <> 0 Then
                                newOrder.SalesLines(SalesOrderIDX).Type = SalesOrder_Service.Type.G_L_Account
                                newOrder.SalesLines(SalesOrderIDX).No = DiscountGLAccount
                                newOrder.SalesLines(SalesOrderIDX).Quantity = 1
                                newOrder.SalesLines(SalesOrderIDX).Unit_Price = SalesOrderInfoResponse.result.discount_amount
                                '2.02b if VAT is applied to order, deduct VAT from the discount amount.  We've had to assume if tax is applied then 
                                ' it needs the 20% vat deducting
                                If SalesOrderInfoResponse.result.tax_amount > 0 Then
                                    tax_rate = SalesOrderInfoResponse.result.tax_amount / SalesOrderInfoResponse.result.grand_total
                                    newOrder.SalesLines(SalesOrderIDX).Unit_Price = SalesOrderInfoResponse.result.discount_amount -
                                        (SalesOrderInfoResponse.result.discount_amount * tax_rate)
                                End If
                            End If


                            Try
                                SOservice.Update(newOrder)
                                OrdersInserted = OrdersInserted + 1
                                ' 13/4/12 CS Update the order status to processing
                                Try      'need to skip this if in test mode!!!
                                    'v2.02b If we want to download processing orders, then change the filter
                                    'Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "processing", "", 0)
                                    'Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
                                    'SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)

                                    If DownloadProcessingStatusOrders = True Then
                                        Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "complete", "", 0)
                                        Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
                                        SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
                                        OrdersUpdated = OrdersUpdated + 1
                                    Else
                                        Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "processing", "", 0)
                                        Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
                                        SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
                                    End If
                                Catch ex2 As Exception
                                    MessageBox.Show("Couldn't update order status for Magento orderID " + msoe.order_id + "     " + ex2.ToString())
                                End Try
                            Catch ex As Exception
                                MessageBox.Show("Can't update order " + newOrder.No + "               " + ex.ToString)
                            End Try

                        End If


                    Catch merror As Exception
                        MessageBox.Show("Couldn't process Magento orderID " + msoe.order_id + "     " + merror.ToString())
                    End Try
                End If
            Next
        End If

        '---------------------------------- Update existing orders that have a status of Processing in Magento
        'v2.01f split this out from the section above when applying the filters
        
        'v2.01f restrict the filter to orders with a status of Pending
        complexFilter(0) = New MagentoAPIv2.complexFilter
        complexFilter(0).key = "Status"
        With assEnt
            .key = "status"
            .value = "processing"
        End With
        complexFilter(0).value = assEnt
        mf.complex_filter = complexFilter

        SalesOrderListResponse = magev2.salesOrderList(SalesOrderListRequest)
        Dim soe2 As MagentoAPIv2.salesOrderListEntity() = SalesOrderListResponse.result


        ' v2.02b Only do this if the DownloadProcessing flag is false
        'If soe2.Length > 0 Then
        If soe2.Length > 0 And DownloadProcessingStatusOrders = False Then
            For Each msoe As MagentoAPIv2.salesOrderListEntity In soe2
                If msoe.status = "processing" Then
                    ' Do SQL Query to see if SO is invoiced in NAV 

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
                End If
            Next
        End If

        '---------------------------------- End of download/update - give feedback on number of records

        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Order download complete - " & OrdersInserted & " new orders created, " & OrdersUpdated & " Magento order statuses updated."
        Me.StatusStrip1.Refresh()

    End Sub

    Private Sub UploadCustomers_Click(ByVal sender As Object, ByVal e As System.EventArgs)


    End Sub




    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        NAVWebURL.Text = "http://localhost:7047/DynamicsNAV/WS/" & NAVCompanyName.Text


    End Sub


    Private Sub ResyncProdIDs_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)


    End Sub

    Private Sub MagentoURL_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MagentoURL.TextChanged

    End Sub

    Private Sub Label4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label4.Click

    End Sub

    Private Sub Button1_Click_1(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub ResyncProdIDsToNAVToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ResyncProdIDsToNAVToolStripMenuItem.Click
        If MessageBox.Show("This function will read through all products in Magento and then update the corresponding Item record in NAV with the Magento product ID", "Warning", MessageBoxButtons.OKCancel) = Windows.Forms.DialogResult.Cancel Then
            End
        End If


        Me.Cursor = Cursors.WaitCursor

        ' Get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ToolStripStatusLabel1.Text = "Retrieving products from Magento....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        Dim mf As New MagentoAPIv2.filters()

        'mf.filter(1) = New MagentoAPIv2.associativeEntity
        'mf.filter(1).key = ">"
        'mf.filter(1).value = 1

        Dim ProductListRequest As New MagentoAPIv2.catalogProductListRequest(mlogin, mf, "")
        Dim ProductListResponse As New MagentoAPIv2.catalogProductListResponse
        ProductListResponse = magev2.catalogProductList(ProductListRequest)

        Dim ItemEntityList As MagentoAPIv2.catalogProductEntity() = ProductListResponse.result

        Dim ProductsUpdated As Integer = 0

        Dim ConnString As String
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

    End Sub

    Private Sub UpdateOrderStatusToProcessingToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UpdateOrderStatusToProcessingToolStripMenuItem.Click
        If MessageBox.Show("This function will read through all orders in Magento up to the 'Last Sales Order ID Downloaded' and set their status to 'Processing'.", "Warning", MessageBoxButtons.OKCancel) = Windows.Forms.DialogResult.Cancel Then
            End
        End If

        Me.Cursor = Cursors.WaitCursor

        ' Get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ToolStripStatusLabel1.Text = "Retrieving orders from Magento....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        Dim mf As New MagentoAPIv2.filters()

        Dim SalesOrderListRequest As New MagentoAPIv2.salesOrderListRequest(mlogin, mf)
        Dim SalesOrderListResponse As New MagentoAPIv2.salesOrderListResponse
        SalesOrderListResponse = magev2.salesOrderList(SalesOrderListRequest)
        Dim soe As MagentoAPIv2.salesOrderListEntity() = SalesOrderListResponse.result

        Dim OrdersInserted As Integer = 0

        If soe.Length > 0 Then
            For Each msoe As MagentoAPIv2.salesOrderListEntity In soe
                'If msoe.increment_id > LastSalesOrderID.Text And msoe.status = "pending" Then
                If msoe.status = "pending" Then
                    ToolStripStatusLabel1.Text = "Processing Magento Order ID. " & msoe.increment_id & "....."
                    Me.StatusStrip1.Refresh()
                    Try
                        Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "processing", "", 0)
                        Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
                        SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
                        OrdersInserted = OrdersInserted + 1

                    Catch ex As Exception
                        Try
                            Dim SalesOrderCommentRequest As New MagentoAPIv2.salesOrderAddCommentRequest(mlogin, msoe.increment_id, "processing", "", 0)
                            Dim SalesOrderCommentResponse As New MagentoAPIv2.salesOrderAddCommentResponse
                            SalesOrderCommentResponse = magev2.salesOrderAddComment(SalesOrderCommentRequest)
                            OrdersInserted = OrdersInserted + 1

                        Catch merror As Exception
                            MessageBox.Show("Couldn't process Magento orderID " + msoe.order_id + "     " + merror.ToString())
                        End Try
                    End Try
                End If
            Next
        End If

        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Order status update complete - " & OrdersInserted & " orders updated."

    End Sub

    Private Sub Label8_Click(ByVal sender As System.Object, ByVal e As System.EventArgs)

    End Sub

    Private Sub UploadItemsProductsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UploadItemsProductsToolStripMenuItem.Click
        Me.Cursor = Cursors.WaitCursor

        ' Login and get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ' Look for all products in NAV and loop

        ToolStripStatusLabel1.Text = "Querying Dynamics NAV Item Table....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        ' Connect to the NAV Item Table
        Dim ConnString As String
        ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
        Dim con As New SqlConnection(ConnString)

        con.Open()

        ' Define the new Magento Product Record
        ' StoreID for Moon is 2 but uses default if not specified.
        Dim ProductRecord As New MagentoAPIv2.catalogProductCreateEntity
        Dim ProductSKU As String
        Dim ProductID As String
        ' v1.03d
        'Dim ProductType As String = "simple"
        'v 2.02c change the default back to simple as default.  If you specify the type as configurable in NAV then it will upload as onfigurable.
        'however, the default should be simple for those customer who haven't set up configurable items.
        'Dim ProductType As String = "configurable"
        Dim ProductType As String = "simple"

        Dim ProductAttrSet As String = "4"
        Dim Store As String = ""

        ' Select the items to upload
        ' 2.02c Change to use specific field for Magento Product Type.  Also changed to just upload items with no eCommerceID
        ' so it will only upload new products - this is to prevent overwriting new details added to records in Magento.
        'Dim cmd As New SqlCommand("SELECT No_, Description, [Unit Price],[Last Date Modified],[Meta Description], [Meta Title], [Meta Keywords], [Net Weight], eCommerceID, [VAT Prod_ Posting Group], [Template Item] FROM [" & NAVCompanyName.Text & "$Item] with (NOLOCK) where [ecommerce Enabled]=1", con)
        Dim cmd As New SqlCommand("SELECT No_, Description, [Unit Price],[Last Date Modified],[Meta Description], [Meta Title], [Meta Keywords], [Net Weight], eCommerceID, [VAT Prod_ Posting Group], [eCommerce Product Type] FROM [" & NAVCompanyName.Text & "$Item] with (NOLOCK) where [ecommerce Enabled]=1 and eCommerceID=''", con)

        Dim sdr As SqlDataReader = cmd.ExecuteReader()
        Dim RecordsInserted As Integer = 0
        Dim RecordsUpdated As Integer = 0
        Dim RecordsFailed As Integer = 0


        ' Loop through the records that have been found   
        If sdr.HasRows Then
            Do While sdr.Read()
                ToolStripStatusLabel1.Text = "Processing Item No. " & sdr.GetString(0) & "....."
                Me.StatusStrip1.Refresh()
                ProductID = sdr.GetString(8)
                ProductRecord.meta_description = sdr.GetString(4) ' This is 255 chars
                ProductRecord.meta_keyword = sdr.GetString(6)
                ProductRecord.meta_title = sdr.GetString(5)
                ProductRecord.name = sdr.GetString(1)
                ProductRecord.price = sdr.GetDecimal(2)
                ' If product ID is empty then we are creating a record so put in a description
                If ProductID = "" Then
                    ProductRecord.description = sdr.GetString(1) ' Should be Blob in NAV
                    ProductRecord.short_description = sdr.GetString(1) ' Should be Blob in NAV
                End If

                ' v2.01f If it is not a template item, then mark it as a simple product
                'If sdr.GetInt16(10) = 0 Then

                ' v2.02c - change to use specific field for Magento fields type
                'If sdr.GetSqlInt16(10) = 0 Then
                'ProductType = "simple"
                'End If
                If sdr.GetInt32(10) = 2 Then
                    ProductType = "configurable"
                End If

                ProductRecord.weight = sdr.GetDecimal(7)
                ProductSKU = sdr.GetString(0)
                ProductRecord.status = "1"
                'ProductRecord.additional_attributes = ""  ' this is where you specify the attribute codes
                'ProductRecord.has_options = "1" ' Says that this product has options.  Then need to set up the options.
                'ProductRecord.url_key = "Url Key"
                'ProductRecord.url_path = "Url Path"
                ProductRecord.visibility = "4" '4 = Catalog,Search
                'ProductRecord.categories(1) = "Pants & Shorts" '****** NEED TO POPULATE *******
                ' v2.02c Can't populate categories because these are different for each customer.
                ' ProductRecord.categories = {"13,17"}   '13=   17=

                'V2.02c Original VAT treatment didn't work in Magento 1.8.1.0.   Values are 0=None, 1=Invalid, 2=Taxable, 3=Invalid
                'Changed so that if the VAT Prod Posting group is "NO VAT" then it will set to None.  Otherwise it will
                'set to taxable
                'If sdr.GetString(9) = "STANDARD" Then
                '    ProductRecord.tax_class_id = "2"  '1=None,2=Taxable  
                'Else
                '    ProductRecord.tax_class_id = "1"
                'End If
                ProductRecord.tax_class_id = "2"  '2=Taxable  
                If sdr.GetString(9) = "NO VAT" Then
                    ProductRecord.tax_class_id = "0"  '0=None
                End If

                ' The block of code will query a product by id
                'ProductID = "1"
                'Dim Attributes As New MagentoAPIv2.catalogProductRequestAttributes()
                'Dim ProductEnquiryRequest As New MagentoAPIv2.catalogProductInfoRequest(mlogin, ProductID, Store, Attributes, "id")
                'Dim ProductEnquiryResponse As New MagentoAPIv2.catalogProductInfoResponse
                'ProductEnquiryResponse = magev2.catalogProductInfo(ProductEnquiryRequest)
                ' --------------------------------------------------------------------

                Dim ProductCreateRequest As New MagentoAPIv2.catalogProductCreateRequest(mlogin, ProductType, ProductAttrSet, ProductSKU, ProductRecord, Store)
                Dim ProductCreateResponse As New MagentoAPIv2.catalogProductCreateResponse
                Dim ProductUpdateRequest As New MagentoAPIv2.catalogProductUpdateRequest(mlogin, ProductID, ProductRecord, "", "")
                Dim ProductUpdateResponse As New MagentoAPIv2.catalogProductUpdateResponse

                ' If NAV Item already has a MagentoID then try to update the record.  If it doesn't then create a new record and store the ID in NAV
                If ProductID > "" Then
                    'Update the record
                    Try
                        ProductUpdateResponse = magev2.catalogProductUpdate(ProductUpdateRequest)
                        RecordsUpdated = RecordsUpdated + 1
                    Catch exc As Exception
                        ' If it fails, try again because Magento often returns a comms error
                        Try
                            ProductUpdateResponse = magev2.catalogProductUpdate(ProductUpdateRequest)
                            RecordsUpdated = RecordsUpdated + 1
                        Catch exc2 As Exception
                            RecordsFailed = RecordsFailed + 1
                            excMsg = exc2.ToString
                            MessageBox.Show(excMsg)
                        End Try
                    End Try
                Else
                    ' insert the record and update the NAV item with the ID
                    Try
                        ProductCreateResponse = magev2.catalogProductCreate(ProductCreateRequest)
                        RecordsInserted = RecordsInserted + 1
                        ' Update NAV Item Record with ID
                        If ProductCreateResponse.result > 0 Then
                            Dim SQLcon As New SqlConnection(ConnString)
                            Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Item] set eCommerceID = '" & ProductCreateResponse.result & "' where [No_] = '" & ProductSKU & "'"
                            SQLcon.Open()
                            Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                            'v2.02c Catch the exception if we can't update the item record in NAV with the ecommerceID
                            Try
                                UpdateCmd.ExecuteNonQuery()
                            Catch ex As Exception
                                excMsg = ex.ToString
                                MessageBox.Show(excMsg)
                            End Try
                            SQLcon.Close()
                        End If
                    Catch exc As Exception
                        ' Magento API often returns a communication error so have a 2nd try if it fails
                        Try
                            ProductCreateResponse = magev2.catalogProductCreate(ProductCreateRequest)
                            RecordsInserted = RecordsInserted + 1
                            ' Update NAV Item Record with ID
                            If ProductCreateResponse.result > 0 Then
                                Dim SQLcon As New SqlConnection(ConnString)
                                Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Item] set eCommerceID = '" & ProductCreateResponse.result & "' where [No_] = '" & ProductSKU & "'"
                                SQLcon.Open()
                                Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                                UpdateCmd.ExecuteNonQuery()
                                SQLcon.Close()
                            End If
                        Catch exc2 As Exception
                            ' if it fails, try to update the record
                            Try
                                ProductUpdateResponse = magev2.catalogProductUpdate(ProductUpdateRequest)
                                RecordsUpdated = RecordsUpdated + 1
                                If ProductUpdateResponse.result > 0 Then
                                    Dim SQLcon As New SqlConnection(ConnString)
                                    Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Item] set eCommerceID = '" & ProductUpdateResponse.result & "' where [No_] = '" & ProductSKU & "'"
                                    SQLcon.Open()
                                    Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                                    UpdateCmd.ExecuteNonQuery()
                                    SQLcon.Close()
                                End If
                            Catch exc3 As Exception
                                RecordsFailed = RecordsFailed + 1
                                excMsg = exc3.ToString
                                MessageBox.Show(excMsg)
                            End Try

                        End Try
                    End Try
                End If

            Loop
        End If

        sdr.Close()

        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Product upload complete - " & RecordsInserted & " created, " & RecordsUpdated & " updated, " & RecordsFailed & " failed."

    End Sub

    Private Sub UploadCustomersToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles UploadCustomersToolStripMenuItem.Click
        Me.Cursor = Cursors.WaitCursor

        ' Login and get sessionID
        GetSessionId()
        ' See if login failed

        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ' Look for all customers in NAV and loop

        ToolStripStatusLabel1.Text = "Querying Dynamics NAV Customer Table....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        ' Connect to the NAV Item Table
        Dim ConnString As String
        ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
        Dim con As New SqlConnection(ConnString)

        con.Open()

        ' Define the new Magento Customer Record
        ' StoreID for Moon is 2 but uses default if not specified.
        Dim CustomerRecord As New MagentoAPIv2.customerCustomerEntityToCreate
        Dim CustomerAddressRecord As New MagentoAPIv2.customerAddressEntityCreate
        Dim CustomerID As Integer = 0
        Dim CustomerAddressID As Integer = 0
        Dim CustomerNo As String
        Dim NewPassword As String
        Dim X As Integer

        ' Select the customer to upload
        ' v2.02d SQL Command below was incorrect so amended.
        '        Dim cmd As New SqlCommand("SELECT No_, " & NAVCompanyName.Text & "$Customer.Name, Address, [Address 2], City, Contact, [Phone No_], [Post Code], County, " &
        Dim cmd As New SqlCommand("SELECT No_, Name, Address, [Address 2], City, Contact, [Phone No_], [Post Code], County, " &
                                  "[E-Mail], [Fax No_], [Country_Region Code], eCommerceID, [eCommerce Address ID] " &
                                  "FROM [" & NAVCompanyName.Text & "$Customer] WITH (NOLOCK) where " & " [eCommerce Enabled]=1 ", con)

        ' Next 3 lines allow the retrieving of the Country Name
        '    "$Country_Region.Name " &
        '    "FROM [" & NAVCompanyName.Text & "$Customer], " & NAVCompanyName.Text & "$Country_Region where " &
        '   "[ecommerce Enabled]=1 and [EU Country_Region Code] = [Country_Region Code]", con)

        Dim sdr As SqlDataReader = cmd.ExecuteReader()
        Dim RecordsInserted As Integer = 0
        Dim RecordsUpdated As Integer = 0
        Dim RecordsFailed As Integer = 0

        ' Loop through the records that have been found and process them
        If sdr.HasRows Then
            Do While sdr.Read()
                ToolStripStatusLabel1.Text = "Processing Customer No. " & sdr.GetString(0) & "....."
                Me.StatusStrip1.Refresh()

                ' Populate the new Magento Record 
                CustomerNo = sdr.GetString(0)
                CustomerID = Val(sdr.GetString(12))
                CustomerAddressID = Val(sdr.GetString(13))

                'CustomerRecord.customer_id = 0  'customer ID is numeric
                CustomerRecord.email = sdr.GetString(9)   'email - can't be a duplicate otherwise createcustomer fails!!


                '2.02d error: "Index was outside the bounds of the array"
                'Dim Name() As String = Split((sdr.GetString(5)), " ")
                'CustomerRecord.firstname = Name(0)
                'CustomerRecord.lastname = Name(1) '2.02d that causes an error when the field GetString(5)=contact is blank or one word 
                Dim Name() As String = Split((sdr.GetString(5)), " ")
                Dim int1 As Integer
                int1 = UBound(Name) - LBound(Name) + 1
                If int1 = 2 Then 'Name and surname 
                    CustomerRecord.firstname = Name(0)
                    CustomerRecord.lastname = Name(1)
                Else
                    'RecordsFailed = RecordsFailed + 1
                    'MessageBox.Show("Customer " & sdr.GetString(0) & " can't be uploaded because the contact is empty")
                    CustomerRecord.firstname = Name(0)
                End If

                'check the value in Magento to assig the right value at 'Associate to Website' by exporting a customer in csv
                CustomerRecord.store_idSpecified = True
                CustomerRecord.store_id = 1
                CustomerRecord.website_idSpecified = True
                CustomerRecord.website_id = 1
                '2.02d





                ' V1.03d Replace old password details with random generated 10 character password
                If CustomerID < "0" Then
                    'CustomerRecord.password = "password"
                    'New code to generate a random password
                    NewPassword = ""
                    Randomize()
                    For X = 1 To 10
                        NewPassword = NewPassword & Int(Rnd() * 10)
                    Next X
                End If

                'CustomerRecord.store_id = 2
                'CustomerRecord.website_id = 
                CustomerAddressRecord.city = sdr.GetString(4)
                CustomerAddressRecord.company = sdr.GetString(1)
                CustomerAddressRecord.country_id = sdr.GetString(11)   'Country code
                'CustomerAddressRecord.country_id = sdr.GetString(14)   'Country Name
                CustomerAddressRecord.fax = sdr.GetString(10)
                CustomerAddressRecord.firstname = CustomerRecord.firstname
                CustomerAddressRecord.is_default_billing = True
                CustomerAddressRecord.is_default_billingSpecified = True
                CustomerAddressRecord.is_default_shipping = True
                CustomerAddressRecord.is_default_shippingSpecified = True
                CustomerAddressRecord.lastname = CustomerRecord.lastname
                CustomerAddressRecord.postcode = sdr.GetString(7)
                CustomerAddressRecord.region = sdr.GetString(8)
                CustomerAddressRecord.street = {sdr.GetString(2), sdr.GetString(3)}
                CustomerAddressRecord.telephone = sdr.GetString(6)
                If CustomerAddressRecord.telephone = "" Then CustomerAddressRecord.telephone = "."

                ' The block of code will query a customer by id
                'CustomerID = 13
                'Dim Attribs() As String = {"firstname", "lastname", "email", "store_id"}
                'Dim CustomerEnquiryRequest As New MagentoAPIv2.customerCustomerInfoRequest(mlogin, CustomerID, Attribs)
                'Dim CustomerEnquiryResponse As New MagentoAPIv2.customerCustomerInfoResponse
                'CustomerEnquiryResponse = magev2.customerCustomerInfo(CustomerEnquiryRequest)
                '----------------------------------------------------------
                ' This block of code will query an address by id
                'CustomerAddressID = 9
                'Dim CustomerAddrEnquiryRequest As New MagentoAPIv2.customerAddressInfoRequest(mlogin, CustomerAddressID)
                'Dim CustomerAddrEnquiryResponse As New MagentoAPIv2.customerAddressInfoResponse
                'CustomerAddrEnquiryResponse = magev2.customerAddressInfo(CustomerAddrEnquiryRequest)
                '----------------------------------------------------------

                Dim CustomerCreateRequest As New MagentoAPIv2.customerCustomerCreateRequest(mlogin, CustomerRecord)
                Dim CustomerCreateResponse As New MagentoAPIv2.customerCustomerCreateResponse
                Dim CustomerUpdateRequest As New MagentoAPIv2.customerCustomerUpdateRequest(mlogin, CustomerID, CustomerRecord)
                Dim CustomerUpdateResponse As New MagentoAPIv2.customerCustomerUpdateResponse

                ' Create the customer record
                If CustomerID > 0 Then
                    'Update the record
                    Try
                        CustomerUpdateResponse = magev2.customerCustomerUpdate(CustomerUpdateRequest)
                        RecordsUpdated = RecordsUpdated + 1
                    Catch exc As Exception
                        ' If it fails, try again because Magento often returns a comms error
                        Try
                            CustomerUpdateResponse = magev2.customerCustomerUpdate(CustomerUpdateRequest)
                            RecordsUpdated = RecordsUpdated + 1
                        Catch exc2 As Exception
                            RecordsFailed = RecordsFailed + 1
                            excMsg = exc2.ToString
                            MessageBox.Show(excMsg)
                        End Try
                    End Try
                Else
                    ' insert the record and update the NAV item with the ID
                    Try
                        CustomerCreateResponse = magev2.customerCustomerCreate(CustomerCreateRequest)
                        RecordsInserted = RecordsInserted + 1
                        CustomerID = CustomerCreateResponse.result

                        ' Update NAV Customer Record with ID
                        If CustomerCreateResponse.result > 0 Then
                            Dim SQLcon As New SqlConnection(ConnString)
                            Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Customer] set ecommerceID = '" & CustomerCreateResponse.result & "' where [No_] = '" & CustomerNo & "'"
                            SQLcon.Open()
                            Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                            UpdateCmd.ExecuteNonQuery()
                            SQLcon.Close()
                        End If
                    Catch exc As Exception
                        ' Magento API often returns a communication error so have a 2nd try if it fails
                        Try
                            CustomerCreateResponse = magev2.customerCustomerCreate(CustomerCreateRequest)
                            RecordsInserted = RecordsInserted + 1
                            CustomerID = CustomerCreateResponse.result

                            ' Update NAV Item Record with ID
                            If CustomerCreateResponse.result > 0 Then
                                Dim SQLcon As New SqlConnection(ConnString)
                                Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Customer] set ecommerceID = '" & CustomerCreateResponse.result & "' where [No_] = '" & CustomerNo & "'"
                                SQLcon.Open()
                                Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                                UpdateCmd.ExecuteNonQuery()
                                SQLcon.Close()
                            End If
                        Catch exc2 As Exception
                            RecordsFailed = RecordsFailed + 1
                            excMsg = exc2.ToString
                            MessageBox.Show(excMsg)
                        End Try
                    End Try
                End If

                ' Create/update the customer address
                If CustomerAddressID > 0 Then   ' The Customer address ID is already set then update the record
                    Try
                        Dim CustomerAddrUpdateRequest As New MagentoAPIv2.customerAddressUpdateRequest(mlogin, CustomerAddressID, CustomerAddressRecord)
                        Dim CustomerAddrUpdateResponse As New MagentoAPIv2.customerAddressUpdateResponse
                        CustomerAddrUpdateResponse = magev2.customerAddressUpdate(CustomerAddrUpdateRequest)

                    Catch exc As Exception
                        Try
                            Dim CustomerAddrUpdateRequest As New MagentoAPIv2.customerAddressUpdateRequest(mlogin, CustomerAddressID, CustomerAddressRecord)
                            Dim CustomerAddrUpdateResponse As New MagentoAPIv2.customerAddressUpdateResponse
                            CustomerAddrUpdateResponse = magev2.customerAddressUpdate(CustomerAddrUpdateRequest)
                        Catch exc2 As Exception
                            excMsg = exc2.ToString
                            MessageBox.Show(excMsg)

                        End Try
                    End Try
                Else
                    ' Create a new record and update the NAV Customer Record with the ID
                    Try
                        Dim CustomerAddrCreateRequest As New MagentoAPIv2.customerAddressCreateRequest(mlogin, CustomerID, CustomerAddressRecord)
                        Dim CustomerAddrCreateResponse As New MagentoAPIv2.customerAddressCreateResponse

                        CustomerAddrCreateResponse = magev2.customerAddressCreate(CustomerAddrCreateRequest)

                        ' Update NAV Item Record with ID
                        If CustomerAddrCreateResponse.result > 0 Then
                            Dim SQLcon As New SqlConnection(ConnString)
                            Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Customer] set [eCommerce Address ID] = '" & CustomerAddrCreateResponse.result & "' where [No_] = '" & CustomerNo & "'"
                            SQLcon.Open()
                            Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                            UpdateCmd.ExecuteNonQuery()
                            SQLcon.Close()
                        End If
                    Catch exc As Exception
                        Try
                            Dim CustomerAddrCreateRequest As New MagentoAPIv2.customerAddressCreateRequest(mlogin, CustomerID, CustomerAddressRecord)
                            Dim CustomerAddrCreateResponse As New MagentoAPIv2.customerAddressCreateResponse

                            CustomerAddrCreateResponse = magev2.customerAddressCreate(CustomerAddrCreateRequest)

                            ' Update NAV Item Record with ID
                            If CustomerAddrCreateResponse.result > 0 Then
                                Dim SQLcon As New SqlConnection(ConnString)
                                Dim updateSql As String = "UPDATE [" & NAVCompanyName.Text & "$Customer] set [eCommerce Address ID] = '" & CustomerAddrCreateResponse.result & "' where [No_] = '" & CustomerNo & "'"
                                SQLcon.Open()
                                Dim UpdateCmd As New SqlCommand(updateSql, SQLcon)
                                UpdateCmd.ExecuteNonQuery()
                                SQLcon.Close()
                            End If

                        Catch exc2 As Exception
                            excMsg = exc2.ToString
                            MessageBox.Show(excMsg)
                        End Try

                    End Try

                End If

            Loop
        End If

        sdr.Close()

        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Customer upload complete - " & RecordsInserted & " created, " & RecordsUpdated & " updated, " & RecordsFailed & " failed."

    End Sub

    Private Sub SettingsToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SettingsToolStripMenuItem.Click
        Settings.ShowDialog()
    End Sub

    Private Sub MainForm_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        ' Retrieve the field values from registry
        Dim regKey As RegistryKey

        Try
            regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\AzzureNAVMagento", True)
            SQLInstance.Text = regKey.GetValue("SQL Server Instance")
            NAVDBName.Text = regKey.GetValue("NAV Database Name")
            NAVCompanyName.Text = regKey.GetValue("NAV Company Name")
            MagentoURL.Text = regKey.GetValue("Magento URL")
            MagentoAPIUser = regKey.GetValue("Magento API User")
            MagentoAPIKey = regKey.GetValue("Magento API Key")
            'LastSalesOrderID.Text = regKey.GetValue("Last Sales Order Downloaded")
            NAVWebURL.Text = regKey.GetValue("NAV Webservice URL")
            WHSupplyLocationCode = regKey.GetValue("New Cus. Supply Location")
            SingleCusAccount = regKey.GetValue("Download to Single Cus. Account")
            NAVCusAcc = regKey.GetValue("NAV Cus. Account")
            DiscountGLAccount = regKey.GetValue("Discount G/L Account")
            ' v2.02
            DownloadProcessingStatusOrders = regKey.GetValue("Download Processing Orders and set to Complete")
            regKey.Close()
        Catch
            MessageBox.Show("Please ensure that you enter the Setting before running any function.")
        End Try


    End Sub

    Private Sub ChangeALLMagentoProductsToComplexToolStripMenuItem_Click(sender As System.Object, e As System.EventArgs) Handles ChangeALLMagentoProductsToComplexToolStripMenuItem.Click
        If MessageBox.Show("This function will read through all products in Magento and then update them to be complex products", "Warning", MessageBoxButtons.OKCancel) = Windows.Forms.DialogResult.Cancel Then
            End
        End If


        Me.Cursor = Cursors.WaitCursor

        ' Get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ToolStripStatusLabel1.Text = "Retrieving products from Magento....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        Dim mf As New MagentoAPIv2.filters()

        'mf.filter(1) = New MagentoAPIv2.associativeEntity
        'mf.filter(1).key = ">"
        'mf.filter(1).value = 1

        Dim ProductListRequest As New MagentoAPIv2.catalogProductListRequest(mlogin, mf, "")
        Dim ProductListResponse As New MagentoAPIv2.catalogProductListResponse
        ProductListResponse = magev2.catalogProductList(ProductListRequest)

        Dim ItemEntityList As MagentoAPIv2.catalogProductEntity() = ProductListResponse.result


        Dim ProductsUpdated As Integer = 0
        Dim ProductID As String = " "

        Dim ProductRecord As New MagentoAPIv2.catalogProductCreateEntity
        Dim ProductUpdateRequest As New MagentoAPIv2.catalogProductUpdateRequest(mlogin, ProductID, ProductRecord, "", "")
        Dim ProductUpdateResponse As New MagentoAPIv2.catalogProductUpdateResponse

        If ItemEntityList.Length > 0 Then
            For Each ItemEntity As MagentoAPIv2.catalogProductEntity In ItemEntityList
                Try
                    ToolStripStatusLabel1.Text = "Updating.... " & ItemEntity.sku
                    Me.StatusStrip1.Refresh()

                    
                    ProductID = ItemEntity.product_id()
                    
                    ' update of stock levels using the 
                    'MagentoAPIv2.catalogInventoryStockItemUpdateEntity()
                    'MagentoAPIv2.catalogInventoryStockItemUpdateRequest()
                    'MagentoAPIv2.catalogInventoryStockItemUpdateResponse()


                    Try
                        ProductUpdateResponse = magev2.catalogProductUpdate(ProductUpdateRequest)
                        ProductsUpdated = ProductsUpdated + 1
                    Catch exc As Exception
                        excMsg = exc.ToString
                        MessageBox.Show(excMsg)
                    End Try
                Catch exc As Exception
                    excMsg = exc.ToString
                    MessageBox.Show(excMsg)
                End Try

            Next
        End If


        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Product update complete - " & ProductsUpdated & " products updated."

    End Sub

    Private Sub ToolStripMenuItem1_Click(sender As System.Object, e As System.EventArgs) Handles UploadStockLevels.Click
        If MessageBox.Show("This function will upload stock levels from NAV into Magento for all Catalog Products in Magento", "Warning", MessageBoxButtons.OKCancel) = Windows.Forms.DialogResult.Cancel Then
            End
        End If


        Me.Cursor = Cursors.WaitCursor

        ' Get sessionID
        GetSessionId()

        ' See if login failed
        If mlogin = "" Then
            MessageBox.Show("Login failed.")
            End
        End If

        ToolStripStatusLabel1.Text = "Retrieving products from Magento....."
        Me.StatusStrip1.Refresh()

        ' Set the Magento API Endpoint
        'Dim magev2 As New MagentoAPIv2.Mage_Api_Model_Server_Wsi_HandlerPortTypeClient("Mage_Api_Model_Server_Wsi_HandlerPort", MagentoURL.Text + "/index.php/api/v2_soap/index/")
        Dim magev2 As New MagentoAPIv2.PortTypeClient("Port", MagentoURL.Text + "/index.php/api/v2_soap/index/")

        Dim mf As New MagentoAPIv2.filters()

        'mf.filter(1) = New MagentoAPIv2.associativeEntity
        'mf.filter(1).key = ">"
        'mf.filter(1).value = 1

        Dim ProductListRequest As New MagentoAPIv2.catalogProductListRequest(mlogin, mf, "")
        Dim ProductListResponse As New MagentoAPIv2.catalogProductListResponse
        ProductListResponse = magev2.catalogProductList(ProductListRequest)

        Dim ItemEntityList As MagentoAPIv2.catalogProductEntity() = ProductListResponse.result

        Dim ProductsUpdated As Integer = 0
        Dim ProductID As String = " "

        Dim InventoryUpdateEntity As New MagentoAPIv2.catalogInventoryStockItemUpdateEntity
        Dim InventoryUpdateRequest As New MagentoAPIv2.catalogInventoryStockItemUpdateRequest(mlogin, ProductID, InventoryUpdateEntity)
        Dim InventoryUpdateResponse As New MagentoAPIv2.catalogInventoryStockItemUpdateResponse

        If ItemEntityList.Length > 0 Then
            For Each ItemEntity As MagentoAPIv2.catalogProductEntity In ItemEntityList
                Try
                    ToolStripStatusLabel1.Text = "Updating.... " & ItemEntity.sku
                    Me.StatusStrip1.Refresh()

                    ProductID = ItemEntity.product_id()


                    ' Read the item record from NAV
                    Dim ConnString As String
                    ConnString = "Data Source=" & SQLInstance.Text & ";Integrated Security=SSPI;Initial Catalog=" & NAVDBName.Text
                    Dim con As New SqlConnection(ConnString)
                    Dim cmd As New SqlCommand("SELECT [Item No_] FROM [" & NAVCompanyName.Text & "$Item Ledger Entry] WITH (NOLOCK) where [Item No_]='" & ItemEntity.sku & "'", con)

                    con.Open()
                    Dim sdr As SqlDataReader = cmd.ExecuteReader()
                    If sdr.HasRows Then
                        con.Close()
                        Dim cmd2 As New SqlCommand("SELECT sum(Quantity) FROM [" & NAVCompanyName.Text & "$Item Ledger Entry] WITH (NOLOCK) where [Item No_]='" & ItemEntity.sku & "'", con)
                        con.Open()
                        Dim sdr2 As SqlDataReader = cmd2.ExecuteReader()

                        ' Upload the stock value
                        sdr2.Read()
                        InventoryUpdateEntity.qty = sdr2.GetDecimal(0)                        
                        
                        Try
                            InventoryUpdateRequest.productId = ProductID
                            InventoryUpdateResponse = magev2.catalogInventoryStockItemUpdate(InventoryUpdateRequest)
                            ProductsUpdated = ProductsUpdated + 1
                        Catch exc As Exception
                            excMsg = exc.ToString
                            MessageBox.Show(excMsg)
                        End Try

                    End If

                    con.Close()

                Catch exc As Exception
                    excMsg = exc.ToString
                    MessageBox.Show(excMsg)
                End Try

            Next
        End If


        Me.Cursor = Cursors.Default

        ToolStripStatusLabel1.Text = "Upload stock levels complete - " & ProductsUpdated & " products updated."

    End Sub
End Class

