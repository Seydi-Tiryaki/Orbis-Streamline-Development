Public Class WebForm_test
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_GuidID As Guid
  Private m_sDeviceBasketRef As String
  Private m_iPageID As Int32
  Private m_iFeedID As Int32

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If



    Me.Form.DefaultButton = cmdLogin.UniqueID
    If Not Page.IsPostBack Then




    End If

  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '

    'If Not m_oBL_WebStoreMgr Is Nothing Then
    '  m_oBL_WebStoreMgr.Dispose()
    '  m_oBL_WebStoreMgr = Nothing
    'End If


    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    End If

    m_pbl_Client = Nothing
    m_pbl_Operator = Nothing
    m_pcl_ErrManager = Nothing
    m_pdl_Manager = Nothing
    m_pcl_Utils = Nothing



  End Sub

#Region "Private Routines"
  Private Function zSetupObjects() As Boolean
    Dim sCookieName As String = "DeviceBasketGuid"

    'm_pcl_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)
    ''Restore dl_manager
    'If Session("dl_Manager") Is Nothing Then
    '  Return False
    'End If
    'm_pdl_Manager = CType(Session("dl_Manager"), DL_Manager)

    ''Create Utils...
    'm_pcl_Utils = New CL_Utils()
    'm_pcl_Utils.pPage = Page

    'm_oDB_Layer = New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    ''Restore Client
    'm_pbl_Client = CType(Session("bl_Client"), BL_Client)

    ''Restore Operator
    'm_pbl_Operator = CType(Session("bl_Operator"), BL_Operator)

    ''Setup data managers
    'm_oBL_CustMgr = New BL_CustomerManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)
    'm_oBL_ProdMgr = New BL_ProductManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer, m_pbl_Operator)
    'm_oBL_WebStoreMgr = New BL_WebStoreManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer, m_pbl_Operator, m_oBL_ProdMgr)

    'm_pBL_CustLogin = m_oBL_WebStoreMgr.ProcessCustomerLoginSession(m_oBL_WebStoreMgr.iFeedID, Session)


    Return True

  End Function



#End Region

  Protected Sub cmdLogin_Click(sender As Object, e As EventArgs) Handles cmdLogin.Click
    Dim lrRet As BL_Operator.enLogonResponse = BL_Operator.enLogonResponse.lrDeactivated
    Dim sControl As String = ""
    Dim dicEmailSubs As New eDictionary
    'Dim drFeed As DataRow
    'Dim sEmail As String
    'Dim sPassword As String


    'Process the logon request....
    If zbFormvalid() Then
      'drFeed = m_oBL_ProdMgr.GetStoreFeedbyID(m_iFeedID)

      ''Check login and validate, and load bl_operator...
      'sEmail = m_oDB_Layer.CleanStringForSql(txtUsername.Text)
      'sPassword = m_oDB_Layer.CleanStringForSql(txtPword.Text)
      'm_pcl_ErrManager.LogError("Customer Logon Attempt: " & txtUsername.Text & ", " & Request.Url.ToString)

      'lrRet = m_oBL_CustMgr.CustomerLogon(m_iFeedID, sEmail, sPassword, m_pBL_CustLogin)
      'Session("bl_CustomerLogin") = m_pBL_CustLogin
      'Select Case lrRet
      '  Case BL_Operator.enLogonResponse.lrOK
      '    m_pcl_ErrManager.LogError("Successful Customer Logon: " & txtUsername.Text)

      '    'Prepare for the redirect to the main page....
      '    Response.Redirect("default.aspx", False)

      '  Case Else
      '    m_pcl_ErrManager.LogError("Failed Logon: " & m_oDB_Layer.CleanStringForSql(txtUsername.Text) & ", " & Request.UserHostAddress & ", " & Request.UserHostName)
      '    'Invalid, display message
      '    m_pdl_Manager.LogEvent2DB("LOGON", "Logon: " & m_oDB_Layer.CleanStringForSql(txtUsername.Text), "Logon Failed", 0, Request, Session.SessionID)
      '    sControl = "txtPword"
      '    m_pcl_Utils.DisplayMessage("Invalid Email and Password combination.", txtUsername.ClientID)
      '    Dim sBody As String
      '    sBody = m_oBL_WebStoreMgr.GetWebMessage(6, m_iFeedID)
      '    dicEmailSubs.Add("%EMAIL%", m_oDB_Layer.CleanStringForSql(txtUsername.Text))
      '    dicEmailSubs.Add("%IP%", Request.UserHostAddress)
      '    dicEmailSubs.Add("%FOOTER%", m_oBL_WebStoreMgr.GetWebMessage(5, m_iFeedID))
      '    dicEmailSubs.Add("%COMPANY%", m_oDB_Layer.CheckDBNullStr(drFeed!feed_CustomerDescription))

      '    Try
      '      dicEmailSubs.Add("%PC%", System.Net.Dns.GetHostEntry(Request.UserHostAddress).HostName)
      '    Catch
      '      dicEmailSubs.Add("%PC%", "Unknown Host Name")
      '    End Try
      '    If lrRet = BL_Operator.enLogonResponse.lrInvalidPassword Then
      '      m_pdl_Manager.SendEmail(CStr(System.Configuration.ConfigurationManager.AppSettings("EmailFrom")), _
      '                   sEmail, "", ConfigurationManager.AppSettings("LoginFailEmailCc"), _
      '                  m_oDB_Layer.CheckDBNullStr(drFeed!feed_CustomerDescription) & " Invalid login attempt!", sBody, True, dicEmailSubs)
      '    End If

      ' End Select
    End If
  End Sub


  Private Function zbFormvalid() As Boolean

    'Validate the screen fields!!!
    Dim bOK As Boolean = False

    If txtUsername.Text.Trim <> "" Then
      revEmail.Validate()
      If revEmail.IsValid Then
        If txtPword.Text.Trim <> "" Then
          bOK = True
        Else
          'Invalid Password                                                                                 
          m_pcl_Utils.DisplayMessage("Please enter your password.", txtPword.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please enter a valid email address.", txtUsername.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter your Email Address", txtUsername.ClientID)
    End If

    Return bOK

  End Function

  Private Sub cmdRemind_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdRemind.Click
    'Dim sEmail As String
    'Dim drFeed As DataRow
    'Dim drCust As DataRow
    'Dim dicParms As New eDictionary
    'Dim sResetID As String
    'Dim sResetIDEnc As String
    'Dim dbRet As DL_Manager.DB_Return
    'Dim dicEmailSubs As New eDictionary
    'Dim bOK As Boolean
    'Dim sHtmlLink As String
    'Dim sLinkText As String


    'If txtReminderEmail.Text.Trim <> "" Then
    '  revReminder.Validate()
    '  If revReminder.IsValid Then
    '    drFeed = m_oBL_ProdMgr.GetStoreFeedbyID(m_iFeedID)
    '    sEmail = m_oDB_Layer.CleanStringForSql(txtReminderEmail.Text.Trim)

    '    'Check login and validate, and load bl_operator...
    '    m_pcl_ErrManager.LogError("Customer Logon Reset: " & sEmail & ", " & Request.Url.ToString)

    '    drCust = m_oBL_CustMgr.GetCustomerByExternalID(m_pbl_Client.ClientID, sEmail, m_iFeedID)

    '    If drCust IsNot Nothing Then
    '      m_pcl_ErrManager.LogError("Customer Logon Reset found: " & sEmail)

    '      sResetID = Guid.NewGuid.ToString
    '      sResetIDEnc = m_pcl_Utils.EncryptStringOld(sResetID)


    '      dicParms.Clear()
    '      dicParms.Add("cust_PwdResetID", "S:" & sResetID)
    '      dicParms.Add("cust_PwdResetExpire", "D:" & Format(Now().AddDays(1), "yyyy-MM-dd HH:mm:ss"))
    '      dicParms.Add("cust_timestamp", "T:" & Format(CDate(drCust!cust_timestamp), "yyyy-MM-dd HH:mm:ss"))
    '      dbRet = m_oBL_CustMgr.UpdateCustomer(CInt(drCust!Cust_id), dicParms)

    '      If dbRet = DL_Manager.DB_Return.ok Then
    '        Dim sBody As String
    '        sBody = m_oBL_WebStoreMgr.GetWebMessage(7, m_iFeedID)

    '        If m_pdl_Manager.ServerName = "localhost" Then
    '          sHtmlLink = "<a href='http://localhost/BathroomTraders/passwordreset.aspx?key=" & sResetIDEnc & "'>Click Here To Reset Password</a>"
    '          sLinkText = "http://localhost/BathroomTraders/passwordreset.aspx?key=" & sResetIDEnc
    '        Else
    '          sHtmlLink = "<a href='https://www.bathroomtraders.co.uk/passwordreset.aspx?key=" & sResetIDEnc & "'>Click Here To Reset Password</a>"
    '          sLinkText = "https://www.bathroomtraders.co.uk/passwordreset.aspx?key=" & sResetIDEnc

    '        End If


    '        dicEmailSubs.Add("%LINK%", sHtmlLink)
    '        dicEmailSubs.Add("%LINKTEXT%", sLinkText)
    '        dicEmailSubs.Add("%FOOTER%", m_oBL_WebStoreMgr.GetWebMessage(5, m_iFeedID))
    '        dicEmailSubs.Add("%COMPANY%", m_oDB_Layer.CheckDBNullStr(drFeed!feed_CustomerDescription))

    '        bOK = m_pdl_Manager.SendEmail(CStr(System.Configuration.ConfigurationManager.AppSettings("EmailFrom")), _
    '                       sEmail, "", ConfigurationManager.AppSettings("LoginFailEmailCc"), _
    '                      m_oDB_Layer.CheckDBNullStr(drFeed!feed_CustomerDescription) & " Password Reset Request", sBody, True, dicEmailSubs)

    '        If bOK Then
    '          m_pcl_ErrManager.LogError("Customer Logon Email sent ok: " & sEmail)

    '          m_pcl_Utils.DisplayMessage("If your email is registered you will recieve a password reset link.\nPlease check your Spam \ Junk folders if it does not appear in the next few minutes.", txtUsername.ClientID)
    '        Else
    '          m_pcl_ErrManager.LogError("Customer Logon Problem sending the reset email: " & sEmail)

    '          m_pcl_Utils.DisplayMessage("There was a problem sending the reset email, please try again if the problem persists please call us.", txtUsername.ClientID)

    '        End If


    '      Else
    '        m_pcl_ErrManager.LogError("Customer Logon Problem Updating the customer record: " & sEmail)
    '        m_pcl_Utils.DisplayMessage("There was a problem sending the reset email, please try again if the problem persists please call us.", txtUsername.ClientID)
    '      End If



    '    Else

    '      m_pcl_Utils.DisplayMessage("If your email is registered you will recieve a password reset link.", txtUsername.ClientID)


    '    End If
    '  Else
    '    m_pcl_Utils.DisplayMessage("Please enter a valid email address.", txtReminderEmail.ClientID)
    '  End If
    'Else
    '  m_pcl_Utils.DisplayMessage("Please enter your Email Address", txtReminderEmail.ClientID)
    'End If
  End Sub



End Class