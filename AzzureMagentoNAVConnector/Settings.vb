Imports Microsoft.Win32

Public Class Settings

    Private Sub Label4_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label4.Click

    End Sub

    Private Sub SettingsCancel_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles SettingsCancel.Click
        Me.Close()
    End Sub

    Private Sub TextBox1_TextChanged(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles NAVCusAcc.TextChanged

    End Sub

    Private Sub Label11_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Label11.Click

    End Sub

    Private Sub SettingsOK_Click(sender As System.Object, e As System.EventArgs) Handles SettingsOK.Click
        'Save the setting to registry
        Dim regKey As RegistryKey
        regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE", True)
        regKey.CreateSubKey("AzzureNAVMagento")
        regKey.Close()

        regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\AzzureNAVMagento", True)
        regKey.SetValue("AppName", "Azzure NAV Magento Connector")
        regKey.SetValue("Version", "2.01a")
        regKey.SetValue("SQL Server Instance", SQLInstance.Text)
        regKey.SetValue("NAV Database Name", NAVDBName.Text)
        regKey.SetValue("NAV Company Name", NAVCompanyName.Text)
        regKey.SetValue("Magento URL", MagentoURL.Text)
        regKey.SetValue("Magento API User", MagentoAPIUsername.Text)
        regKey.SetValue("Magento API Key", MagentoAPIKey.Text)
        'regKey.SetValue("Last Sales Order Downloaded", LastSalesOrderID.Text)
        regKey.SetValue("NAV Webservice URL", NAVWebURL.Text)
        'v2.01f removed this tick box - now just looks if there is a code specified
        'regKey.SetValue("Download to Single Cus. Account", SingleCusAccount.Text)
        regKey.SetValue("NAV Cus. Account", NAVCusAcc.Text)
        regKey.SetValue("Discount G/L Account", DiscountGLAccount.Text)
        regKey.SetValue("New Cus. Supply Location", WHSupplyLocation.Text)
        'v2.02b
        regKey.SetValue("Download Processing Orders and set to Complete", DownloadProcessingStatusOrders.Checked)

        regKey.Close()

        ' Need to refresh the field values on the Mainform before closing.
        ' v2.01f This wasn't working, fixed in this version.
        MainForm.SQLInstance.Text = SQLInstance.Text
        MainForm.NAVDBName.Text = NAVDBName.Text
        MainForm.NAVCompanyName.Text = NAVCompanyName.Text
        MainForm.MagentoURL.Text = MagentoURL.Text
        MainForm.NAVWebURL.Text = NAVWebURL.Text

        MainForm.Update()

        Me.Close()

    End Sub

    Private Sub Settings_Load(sender As Object, e As System.EventArgs) Handles Me.Load

        ' Retrieve the field values from registry
        Dim regKey As RegistryKey
        Try
            regKey = Registry.CurrentUser.OpenSubKey("SOFTWARE\AzzureNAVMagento", True)
            SQLInstance.Text = regKey.GetValue("SQL Server Instance")
            NAVDBName.Text = regKey.GetValue("NAV Database Name")
            NAVCompanyName.Text = regKey.GetValue("NAV Company Name")
            MagentoURL.Text = regKey.GetValue("Magento URL")
            MagentoAPIUsername.Text = regKey.GetValue("Magento API User")
            MagentoAPIKey.Text = regKey.GetValue("Magento API Key")
            'LastSalesOrderID.Text = regKey.GetValue("Last Sales Order Downloaded")
            NAVWebURL.Text = regKey.GetValue("NAV Webservice URL")
            'v2.01f Removed flag - now just checks if there is a customer code specified
            'SingleCusAccount.Text = regKey.GetValue("Download to Single Cus. Account")
            NAVCusAcc.Text = regKey.GetValue("NAV Cus. Account")
            DiscountGLAccount.Text = regKey.GetValue("Discount G/L Account")
            WHSupplyLocation.Text = regKey.GetValue("New Cus. Supply Location")
            'v 2.02b
            DownloadProcessingStatusOrders.Checked = regKey.GetValue("Download Processing Orders and set to Complete")
            regKey.Close()
        Catch

        End Try



    End Sub

    Private Sub NAVDBName_TextChanged(sender As System.Object, e As System.EventArgs) Handles NAVDBName.TextChanged

    End Sub

    Private Sub Label10_Click(sender As System.Object, e As System.EventArgs) Handles Label10.Click

    End Sub
End Class