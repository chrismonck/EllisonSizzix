<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Settings
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.Label8 = New System.Windows.Forms.Label()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.NAVWebURL = New System.Windows.Forms.TextBox()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.SQLInstance = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.LastSalesOrderID = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.MagentoURL = New System.Windows.Forms.TextBox()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.NAVDBName = New System.Windows.Forms.TextBox()
        Me.NAVCompanyName = New System.Windows.Forms.TextBox()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.SettingsOK = New System.Windows.Forms.Button()
        Me.SettingsCancel = New System.Windows.Forms.Button()
        Me.MagentoAPIUsername = New System.Windows.Forms.TextBox()
        Me.MagentoAPIKey = New System.Windows.Forms.TextBox()
        Me.Label9 = New System.Windows.Forms.Label()
        Me.Label11 = New System.Windows.Forms.Label()
        Me.NAVCusAcc = New System.Windows.Forms.TextBox()
        Me.WHSupplyLocation = New System.Windows.Forms.TextBox()
        Me.Label12 = New System.Windows.Forms.Label()
        Me.DiscountGLAccount = New System.Windows.Forms.TextBox()
        Me.Label13 = New System.Windows.Forms.Label()
        Me.Label10 = New System.Windows.Forms.Label()
        Me.DownloadProcessingStatusOrders = New System.Windows.Forms.CheckBox()
        Me.SuspendLayout()
        '
        'Label8
        '
        Me.Label8.AutoSize = True
        Me.Label8.Location = New System.Drawing.Point(556, 394)
        Me.Label8.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label8.Name = "Label8"
        Me.Label8.Size = New System.Drawing.Size(350, 20)
        Me.Label8.TabIndex = 34
        Me.Label8.Text = "DON'T CHANGE THIS UNLESS INSTRUCTED!"
        Me.Label8.Visible = False
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(18, 162)
        Me.Label7.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(165, 20)
        Me.Label7.TabIndex = 33
        Me.Label7.Text = "NAV Webservice URL"
        '
        'NAVWebURL
        '
        Me.NAVWebURL.Location = New System.Drawing.Point(210, 157)
        Me.NAVWebURL.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.NAVWebURL.Name = "NAVWebURL"
        Me.NAVWebURL.Size = New System.Drawing.Size(600, 26)
        Me.NAVWebURL.TabIndex = 27
        Me.NAVWebURL.Text = "http://localhost:7047/DynamicsNAV/WS/CRONUS%20UK%20Ltd."
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(18, 37)
        Me.Label6.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(157, 20)
        Me.Label6.TabIndex = 32
        Me.Label6.Text = "SQL Server Instance"
        '
        'SQLInstance
        '
        Me.SQLInstance.Location = New System.Drawing.Point(210, 32)
        Me.SQLInstance.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SQLInstance.Name = "SQLInstance"
        Me.SQLInstance.Size = New System.Drawing.Size(373, 26)
        Me.SQLInstance.TabIndex = 22
        Me.SQLInstance.Text = "MSSQLSERVER"
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(18, 394)
        Me.Label5.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(242, 20)
        Me.Label5.TabIndex = 31
        Me.Label5.Text = "Last Sales Order ID Downloaded"
        Me.Label5.Visible = False
        '
        'LastSalesOrderID
        '
        Me.LastSalesOrderID.Location = New System.Drawing.Point(320, 389)
        Me.LastSalesOrderID.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.LastSalesOrderID.Name = "LastSalesOrderID"
        Me.LastSalesOrderID.Size = New System.Drawing.Size(226, 26)
        Me.LastSalesOrderID.TabIndex = 26
        Me.LastSalesOrderID.Text = "0"
        Me.LastSalesOrderID.Visible = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(18, 278)
        Me.Label3.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(109, 20)
        Me.Label3.TabIndex = 30
        Me.Label3.Text = "Magento URL"
        '
        'MagentoURL
        '
        Me.MagentoURL.Location = New System.Drawing.Point(210, 274)
        Me.MagentoURL.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.MagentoURL.Name = "MagentoURL"
        Me.MagentoURL.Size = New System.Drawing.Size(373, 26)
        Me.MagentoURL.TabIndex = 25
        Me.MagentoURL.Text = "http://www.magentoshop.com"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(18, 117)
        Me.Label2.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(159, 20)
        Me.Label2.TabIndex = 29
        Me.Label2.Text = "NAV Company Name"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(18, 77)
        Me.Label1.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(162, 20)
        Me.Label1.TabIndex = 28
        Me.Label1.Text = "NAV Database Name"
        '
        'NAVDBName
        '
        Me.NAVDBName.Location = New System.Drawing.Point(210, 72)
        Me.NAVDBName.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.NAVDBName.Name = "NAVDBName"
        Me.NAVDBName.Size = New System.Drawing.Size(373, 26)
        Me.NAVDBName.TabIndex = 23
        Me.NAVDBName.Text = "DemoDatabase"
        '
        'NAVCompanyName
        '
        Me.NAVCompanyName.Location = New System.Drawing.Point(210, 112)
        Me.NAVCompanyName.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.NAVCompanyName.Name = "NAVCompanyName"
        Me.NAVCompanyName.Size = New System.Drawing.Size(373, 26)
        Me.NAVCompanyName.TabIndex = 24
        Me.NAVCompanyName.Text = "CRONUS UK Ltd."
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(18, 320)
        Me.Label4.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(180, 20)
        Me.Label4.TabIndex = 35
        Me.Label4.Text = "Magento API Username"
        '
        'SettingsOK
        '
        Me.SettingsOK.Location = New System.Drawing.Point(237, 509)
        Me.SettingsOK.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SettingsOK.Name = "SettingsOK"
        Me.SettingsOK.Size = New System.Drawing.Size(180, 49)
        Me.SettingsOK.TabIndex = 36
        Me.SettingsOK.Text = "OK"
        Me.SettingsOK.UseVisualStyleBackColor = True
        '
        'SettingsCancel
        '
        Me.SettingsCancel.Location = New System.Drawing.Point(462, 509)
        Me.SettingsCancel.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.SettingsCancel.Name = "SettingsCancel"
        Me.SettingsCancel.Size = New System.Drawing.Size(180, 49)
        Me.SettingsCancel.TabIndex = 37
        Me.SettingsCancel.Text = "Cancel"
        Me.SettingsCancel.UseVisualStyleBackColor = True
        '
        'MagentoAPIUsername
        '
        Me.MagentoAPIUsername.Location = New System.Drawing.Point(210, 315)
        Me.MagentoAPIUsername.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.MagentoAPIUsername.Name = "MagentoAPIUsername"
        Me.MagentoAPIUsername.Size = New System.Drawing.Size(373, 26)
        Me.MagentoAPIUsername.TabIndex = 38
        Me.MagentoAPIUsername.Text = "NAVuser"
        '
        'MagentoAPIKey
        '
        Me.MagentoAPIKey.Location = New System.Drawing.Point(210, 354)
        Me.MagentoAPIKey.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.MagentoAPIKey.Name = "MagentoAPIKey"
        Me.MagentoAPIKey.Size = New System.Drawing.Size(373, 26)
        Me.MagentoAPIKey.TabIndex = 40
        Me.MagentoAPIKey.Text = "NAVPassword"
        '
        'Label9
        '
        Me.Label9.AutoSize = True
        Me.Label9.Location = New System.Drawing.Point(18, 358)
        Me.Label9.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label9.Name = "Label9"
        Me.Label9.Size = New System.Drawing.Size(132, 20)
        Me.Label9.TabIndex = 39
        Me.Label9.Text = "Magento API Key"
        '
        'Label11
        '
        Me.Label11.AutoSize = True
        Me.Label11.Location = New System.Drawing.Point(18, 202)
        Me.Label11.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label11.Name = "Label11"
        Me.Label11.Size = New System.Drawing.Size(365, 20)
        Me.Label11.TabIndex = 44
        Me.Label11.Text = "Download all orders to a single Cus. Account Code"
        '
        'NAVCusAcc
        '
        Me.NAVCusAcc.Location = New System.Drawing.Point(424, 197)
        Me.NAVCusAcc.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.NAVCusAcc.Name = "NAVCusAcc"
        Me.NAVCusAcc.Size = New System.Drawing.Size(175, 26)
        Me.NAVCusAcc.TabIndex = 43
        Me.NAVCusAcc.Text = "WEBSALES"
        '
        'WHSupplyLocation
        '
        Me.WHSupplyLocation.Location = New System.Drawing.Point(238, 232)
        Me.WHSupplyLocation.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.WHSupplyLocation.Name = "WHSupplyLocation"
        Me.WHSupplyLocation.Size = New System.Drawing.Size(175, 26)
        Me.WHSupplyLocation.TabIndex = 45
        Me.WHSupplyLocation.Text = "L1"
        '
        'Label12
        '
        Me.Label12.AutoSize = True
        Me.Label12.Location = New System.Drawing.Point(18, 237)
        Me.Label12.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label12.Name = "Label12"
        Me.Label12.Size = New System.Drawing.Size(189, 20)
        Me.Label12.TabIndex = 46
        Me.Label12.Text = "New Cus Supply Location"
        '
        'DiscountGLAccount
        '
        Me.DiscountGLAccount.Location = New System.Drawing.Point(630, 232)
        Me.DiscountGLAccount.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DiscountGLAccount.Name = "DiscountGLAccount"
        Me.DiscountGLAccount.Size = New System.Drawing.Size(175, 26)
        Me.DiscountGLAccount.TabIndex = 47
        '
        'Label13
        '
        Me.Label13.AutoSize = True
        Me.Label13.Location = New System.Drawing.Point(458, 237)
        Me.Label13.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label13.Name = "Label13"
        Me.Label13.Size = New System.Drawing.Size(137, 20)
        Me.Label13.TabIndex = 48
        Me.Label13.Text = "Discount G/L Acc."
        '
        'Label10
        '
        Me.Label10.AutoSize = True
        Me.Label10.Location = New System.Drawing.Point(18, 428)
        Me.Label10.Margin = New System.Windows.Forms.Padding(4, 0, 4, 0)
        Me.Label10.Name = "Label10"
        Me.Label10.Size = New System.Drawing.Size(539, 20)
        Me.Label10.TabIndex = 49
        Me.Label10.Text = "Download all orders with status of ""Processing"" and change to ""Complete""?"
        '
        'DownloadProcessingStatusOrders
        '
        Me.DownloadProcessingStatusOrders.AutoSize = True
        Me.DownloadProcessingStatusOrders.Location = New System.Drawing.Point(600, 428)
        Me.DownloadProcessingStatusOrders.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.DownloadProcessingStatusOrders.Name = "DownloadProcessingStatusOrders"
        Me.DownloadProcessingStatusOrders.Size = New System.Drawing.Size(22, 21)
        Me.DownloadProcessingStatusOrders.TabIndex = 50
        Me.DownloadProcessingStatusOrders.UseVisualStyleBackColor = True
        '
        'Settings
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(934, 595)
        Me.Controls.Add(Me.DownloadProcessingStatusOrders)
        Me.Controls.Add(Me.Label10)
        Me.Controls.Add(Me.Label13)
        Me.Controls.Add(Me.DiscountGLAccount)
        Me.Controls.Add(Me.Label12)
        Me.Controls.Add(Me.WHSupplyLocation)
        Me.Controls.Add(Me.Label11)
        Me.Controls.Add(Me.NAVCusAcc)
        Me.Controls.Add(Me.MagentoAPIKey)
        Me.Controls.Add(Me.Label9)
        Me.Controls.Add(Me.MagentoAPIUsername)
        Me.Controls.Add(Me.SettingsCancel)
        Me.Controls.Add(Me.SettingsOK)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label8)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.NAVWebURL)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.SQLInstance)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.LastSalesOrderID)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.MagentoURL)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.NAVDBName)
        Me.Controls.Add(Me.NAVCompanyName)
        Me.Margin = New System.Windows.Forms.Padding(4, 5, 4, 5)
        Me.Name = "Settings"
        Me.ShowIcon = False
        Me.Text = "Settings"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label8 As System.Windows.Forms.Label
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents NAVWebURL As System.Windows.Forms.TextBox
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents SQLInstance As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents LastSalesOrderID As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents MagentoURL As System.Windows.Forms.TextBox
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents NAVDBName As System.Windows.Forms.TextBox
    Friend WithEvents NAVCompanyName As System.Windows.Forms.TextBox
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents SettingsOK As System.Windows.Forms.Button
    Friend WithEvents SettingsCancel As System.Windows.Forms.Button
    Friend WithEvents MagentoAPIUsername As System.Windows.Forms.TextBox
    Friend WithEvents MagentoAPIKey As System.Windows.Forms.TextBox
    Friend WithEvents Label9 As System.Windows.Forms.Label
    Friend WithEvents Label11 As System.Windows.Forms.Label
    Friend WithEvents NAVCusAcc As System.Windows.Forms.TextBox
    Friend WithEvents WHSupplyLocation As System.Windows.Forms.TextBox
    Friend WithEvents Label12 As System.Windows.Forms.Label
    Friend WithEvents DiscountGLAccount As System.Windows.Forms.TextBox
    Friend WithEvents Label13 As System.Windows.Forms.Label
    Friend WithEvents Label10 As System.Windows.Forms.Label
    Friend WithEvents DownloadProcessingStatusOrders As System.Windows.Forms.CheckBox
End Class
