<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.DownloadOrders = New System.Windows.Forms.Button()
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.ToolStripStatusLabel1 = New System.Windows.Forms.ToolStripStatusLabel()
        Me.NAVCompanyName = New System.Windows.Forms.TextBox()
        Me.NAVDBName = New System.Windows.Forms.TextBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.Label2 = New System.Windows.Forms.Label()
        Me.MagentoURL = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Label4 = New System.Windows.Forms.Label()
        Me.LastSalesOrderID = New System.Windows.Forms.TextBox()
        Me.Label5 = New System.Windows.Forms.Label()
        Me.Label6 = New System.Windows.Forms.Label()
        Me.SQLInstance = New System.Windows.Forms.TextBox()
        Me.Label7 = New System.Windows.Forms.Label()
        Me.NAVWebURL = New System.Windows.Forms.TextBox()
        Me.MenuStrip1 = New System.Windows.Forms.MenuStrip()
        Me.UploadToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UploadItemsProductsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UploadCustomersToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ToolsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.ResyncProdIDsToNAVToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UpdateOrderStatusToProcessingToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.UploadStockLevels = New System.Windows.Forms.ToolStripMenuItem()
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.SettingsToolStripMenuItem = New System.Windows.Forms.ToolStripMenuItem()
        Me.PictureBox1 = New System.Windows.Forms.PictureBox()
        Me.StatusStrip1.SuspendLayout()
        Me.MenuStrip1.SuspendLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'DownloadOrders
        '
        Me.DownloadOrders.Location = New System.Drawing.Point(131, 263)
        Me.DownloadOrders.Name = "DownloadOrders"
        Me.DownloadOrders.Size = New System.Drawing.Size(246, 55)
        Me.DownloadOrders.TabIndex = 9
        Me.DownloadOrders.Text = "Download Orders"
        Me.DownloadOrders.UseVisualStyleBackColor = True
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ToolStripStatusLabel1})
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 377)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(694, 22)
        Me.StatusStrip1.TabIndex = 4
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'ToolStripStatusLabel1
        '
        Me.ToolStripStatusLabel1.Name = "ToolStripStatusLabel1"
        Me.ToolStripStatusLabel1.Size = New System.Drawing.Size(214, 17)
        Me.ToolStripStatusLabel1.Text = "                                                                     "
        '
        'NAVCompanyName
        '
        Me.NAVCompanyName.Location = New System.Drawing.Point(144, 83)
        Me.NAVCompanyName.Name = "NAVCompanyName"
        Me.NAVCompanyName.ReadOnly = True
        Me.NAVCompanyName.Size = New System.Drawing.Size(250, 20)
        Me.NAVCompanyName.TabIndex = 3
        Me.NAVCompanyName.Text = "CRONUS UK Ltd."
        '
        'NAVDBName
        '
        Me.NAVDBName.Location = New System.Drawing.Point(144, 57)
        Me.NAVDBName.Name = "NAVDBName"
        Me.NAVDBName.ReadOnly = True
        Me.NAVDBName.Size = New System.Drawing.Size(250, 20)
        Me.NAVDBName.TabIndex = 2
        Me.NAVDBName.Text = "DemoDatabase"
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Location = New System.Drawing.Point(24, 60)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(109, 13)
        Me.Label1.TabIndex = 7
        Me.Label1.Text = "NAV Database Name"
        '
        'Label2
        '
        Me.Label2.AutoSize = True
        Me.Label2.Location = New System.Drawing.Point(24, 86)
        Me.Label2.Name = "Label2"
        Me.Label2.Size = New System.Drawing.Size(107, 13)
        Me.Label2.TabIndex = 8
        Me.Label2.Text = "NAV Company Name"
        '
        'MagentoURL
        '
        Me.MagentoURL.Enabled = False
        Me.MagentoURL.Location = New System.Drawing.Point(144, 109)
        Me.MagentoURL.Name = "MagentoURL"
        Me.MagentoURL.ReadOnly = True
        Me.MagentoURL.Size = New System.Drawing.Size(250, 20)
        Me.MagentoURL.TabIndex = 4
        Me.MagentoURL.Text = "http://www.magentowebshop.com"
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Location = New System.Drawing.Point(24, 112)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(74, 13)
        Me.Label3.TabIndex = 10
        Me.Label3.Text = "Magento URL"
        '
        'Label4
        '
        Me.Label4.AutoSize = True
        Me.Label4.Location = New System.Drawing.Point(477, 346)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(155, 13)
        Me.Label4.TabIndex = 11
        Me.Label4.Text = "(C) 2014 Azzure IT Ltd    v2.02f"
        '
        'LastSalesOrderID
        '
        Me.LastSalesOrderID.Location = New System.Drawing.Point(225, 135)
        Me.LastSalesOrderID.Name = "LastSalesOrderID"
        Me.LastSalesOrderID.ReadOnly = True
        Me.LastSalesOrderID.Size = New System.Drawing.Size(152, 20)
        Me.LastSalesOrderID.TabIndex = 5
        Me.LastSalesOrderID.Text = "0"
        Me.LastSalesOrderID.Visible = False
        '
        'Label5
        '
        Me.Label5.AutoSize = True
        Me.Label5.Location = New System.Drawing.Point(24, 138)
        Me.Label5.Name = "Label5"
        Me.Label5.Size = New System.Drawing.Size(162, 13)
        Me.Label5.TabIndex = 13
        Me.Label5.Text = "Last Sales Order ID Downloaded"
        Me.Label5.Visible = False
        '
        'Label6
        '
        Me.Label6.AutoSize = True
        Me.Label6.Location = New System.Drawing.Point(24, 34)
        Me.Label6.Name = "Label6"
        Me.Label6.Size = New System.Drawing.Size(106, 13)
        Me.Label6.TabIndex = 15
        Me.Label6.Text = "SQL Server Instance"
        '
        'SQLInstance
        '
        Me.SQLInstance.Location = New System.Drawing.Point(144, 31)
        Me.SQLInstance.Name = "SQLInstance"
        Me.SQLInstance.ReadOnly = True
        Me.SQLInstance.Size = New System.Drawing.Size(250, 20)
        Me.SQLInstance.TabIndex = 1
        Me.SQLInstance.Text = "MSSQLSERVER"
        '
        'Label7
        '
        Me.Label7.AutoSize = True
        Me.Label7.Location = New System.Drawing.Point(24, 164)
        Me.Label7.Name = "Label7"
        Me.Label7.Size = New System.Drawing.Size(114, 13)
        Me.Label7.TabIndex = 17
        Me.Label7.Text = "NAV Webservice URL"
        '
        'NAVWebURL
        '
        Me.NAVWebURL.Location = New System.Drawing.Point(144, 161)
        Me.NAVWebURL.Name = "NAVWebURL"
        Me.NAVWebURL.ReadOnly = True
        Me.NAVWebURL.Size = New System.Drawing.Size(401, 20)
        Me.NAVWebURL.TabIndex = 6
        Me.NAVWebURL.Text = "http://localhost:7047/DynamicsNAV/WS/CRONUS%20UK%20Ltd."
        '
        'MenuStrip1
        '
        Me.MenuStrip1.Items.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UploadToolStripMenuItem, Me.ToolsToolStripMenuItem})
        Me.MenuStrip1.Location = New System.Drawing.Point(0, 0)
        Me.MenuStrip1.Name = "MenuStrip1"
        Me.MenuStrip1.Size = New System.Drawing.Size(694, 24)
        Me.MenuStrip1.TabIndex = 20
        Me.MenuStrip1.Text = "MenuStrip1"
        '
        'UploadToolStripMenuItem
        '
        Me.UploadToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.UploadItemsProductsToolStripMenuItem, Me.UploadCustomersToolStripMenuItem})
        Me.UploadToolStripMenuItem.Name = "UploadToolStripMenuItem"
        Me.UploadToolStripMenuItem.Size = New System.Drawing.Size(57, 20)
        Me.UploadToolStripMenuItem.Text = "Upload"
        '
        'UploadItemsProductsToolStripMenuItem
        '
        Me.UploadItemsProductsToolStripMenuItem.Name = "UploadItemsProductsToolStripMenuItem"
        Me.UploadItemsProductsToolStripMenuItem.Size = New System.Drawing.Size(223, 22)
        Me.UploadItemsProductsToolStripMenuItem.Text = "Upload New Items/Products"
        '
        'UploadCustomersToolStripMenuItem
        '
        Me.UploadCustomersToolStripMenuItem.Name = "UploadCustomersToolStripMenuItem"
        Me.UploadCustomersToolStripMenuItem.Size = New System.Drawing.Size(223, 22)
        Me.UploadCustomersToolStripMenuItem.Text = "Upload Customers"
        '
        'ToolsToolStripMenuItem
        '
        Me.ToolsToolStripMenuItem.DropDownItems.AddRange(New System.Windows.Forms.ToolStripItem() {Me.ResyncProdIDsToNAVToolStripMenuItem, Me.UpdateOrderStatusToProcessingToolStripMenuItem, Me.UploadStockLevels, Me.ChangeALLMagentoProductsToComplexToolStripMenuItem, Me.SettingsToolStripMenuItem})
        Me.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem"
        Me.ToolsToolStripMenuItem.Size = New System.Drawing.Size(48, 20)
        Me.ToolsToolStripMenuItem.Text = "Tools"
        '
        'ResyncProdIDsToNAVToolStripMenuItem
        '
        Me.ResyncProdIDsToNAVToolStripMenuItem.Name = "ResyncProdIDsToNAVToolStripMenuItem"
        Me.ResyncProdIDsToNAVToolStripMenuItem.Size = New System.Drawing.Size(303, 22)
        Me.ResyncProdIDsToNAVToolStripMenuItem.Text = "Resync Prod. IDs to NAV"
        '
        'UpdateOrderStatusToProcessingToolStripMenuItem
        '
        Me.UpdateOrderStatusToProcessingToolStripMenuItem.Name = "UpdateOrderStatusToProcessingToolStripMenuItem"
        Me.UpdateOrderStatusToProcessingToolStripMenuItem.Size = New System.Drawing.Size(303, 22)
        Me.UpdateOrderStatusToProcessingToolStripMenuItem.Text = "Update Order Status to 'Processing'"
        '
        'UploadStockLevels
        '
        Me.UploadStockLevels.Name = "UploadStockLevels"
        Me.UploadStockLevels.Size = New System.Drawing.Size(303, 22)
        Me.UploadStockLevels.Text = "Upload stock levels from NAV to Magento"
        '
        'ChangeALLMagentoProductsToComplexToolStripMenuItem
        '
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem.Enabled = False
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem.Name = "ChangeALLMagentoProductsToComplexToolStripMenuItem"
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem.Size = New System.Drawing.Size(303, 22)
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem.Text = "Change ALL Magento Products to Complex"
        Me.ChangeALLMagentoProductsToComplexToolStripMenuItem.Visible = False
        '
        'SettingsToolStripMenuItem
        '
        Me.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem"
        Me.SettingsToolStripMenuItem.Size = New System.Drawing.Size(303, 22)
        Me.SettingsToolStripMenuItem.Text = "Settings"
        '
        'PictureBox1
        '
        Me.PictureBox1.Image = CType(resources.GetObject("PictureBox1.Image"), System.Drawing.Image)
        Me.PictureBox1.Location = New System.Drawing.Point(444, 257)
        Me.PictureBox1.Name = "PictureBox1"
        Me.PictureBox1.Size = New System.Drawing.Size(229, 86)
        Me.PictureBox1.TabIndex = 21
        Me.PictureBox1.TabStop = False
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(694, 399)
        Me.Controls.Add(Me.PictureBox1)
        Me.Controls.Add(Me.Label7)
        Me.Controls.Add(Me.NAVWebURL)
        Me.Controls.Add(Me.Label6)
        Me.Controls.Add(Me.SQLInstance)
        Me.Controls.Add(Me.Label5)
        Me.Controls.Add(Me.LastSalesOrderID)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.MagentoURL)
        Me.Controls.Add(Me.Label2)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.NAVDBName)
        Me.Controls.Add(Me.NAVCompanyName)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Controls.Add(Me.MenuStrip1)
        Me.Controls.Add(Me.DownloadOrders)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MainMenuStrip = Me.MenuStrip1
        Me.Name = "MainForm"
        Me.Text = "Azzure MagentoNAV Connector"
        Me.StatusStrip1.ResumeLayout(False)
        Me.StatusStrip1.PerformLayout()
        Me.MenuStrip1.ResumeLayout(False)
        Me.MenuStrip1.PerformLayout()
        CType(Me.PictureBox1, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents DownloadOrders As System.Windows.Forms.Button
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents ToolStripStatusLabel1 As System.Windows.Forms.ToolStripStatusLabel
    Friend WithEvents NAVCompanyName As System.Windows.Forms.TextBox
    Friend WithEvents NAVDBName As System.Windows.Forms.TextBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents Label2 As System.Windows.Forms.Label
    Friend WithEvents MagentoURL As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents LastSalesOrderID As System.Windows.Forms.TextBox
    Friend WithEvents Label5 As System.Windows.Forms.Label
    Friend WithEvents Label6 As System.Windows.Forms.Label
    Friend WithEvents SQLInstance As System.Windows.Forms.TextBox
    Friend WithEvents Label7 As System.Windows.Forms.Label
    Friend WithEvents NAVWebURL As System.Windows.Forms.TextBox
    Friend WithEvents MenuStrip1 As System.Windows.Forms.MenuStrip
    Friend WithEvents ToolsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ResyncProdIDsToNAVToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UpdateOrderStatusToProcessingToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UploadToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UploadItemsProductsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UploadCustomersToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents SettingsToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents ChangeALLMagentoProductsToComplexToolStripMenuItem As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents UploadStockLevels As System.Windows.Forms.ToolStripMenuItem
    Friend WithEvents PictureBox1 As System.Windows.Forms.PictureBox

End Class
