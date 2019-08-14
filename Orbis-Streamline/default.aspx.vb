Public Class _default
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_obl_OpMgr As BL_OperatorManager


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Session.Abandon()
      Response.Redirect("default.aspx")
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

    If Not m_obl_OpMgr Is Nothing Then
      m_obl_OpMgr.Dispose()
      m_obl_OpMgr = Nothing
    End If

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

    m_pcl_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)

    'Restore dl_manager
    If Session("dl_Manager") Is Nothing Then
      Return False
    End If
    m_pdl_Manager = CType(Session("dl_Manager"), DL_Manager)

    'Create Utils...
    m_pcl_Utils = New CL_Utils()
    m_pcl_Utils.pPage = Page

    m_oDB_Layer = New DB_LayerGeneric(m_pdl_Manager.sServerConnString, m_pcl_ErrManager)

    'Restore Client
    m_pbl_Client = CType(Session("bl_Client"), BL_Client)

    'Restore Operator
    m_pbl_Operator = CType(Session("bl_Operator"), BL_Operator)

    m_obl_OpMgr = New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    Return True

  End Function



#End Region

  Protected Sub cmdLogin_Click(sender As Object, e As EventArgs) Handles cmdLogin.Click
    Dim lrRet As BL_Operator.enLogonResponse = BL_Operator.enLogonResponse.lrDeactivated
    Dim sControl As String = ""
    Dim dicEmailSubs As New eDictionary
    Dim sEmail As String
    Dim sPassword As String

    'Process the logon request....
    If zbFormvalid() Then


      'Check login and validate, and load bl_operator...
      sEmail = m_oDB_Layer.CleanStringForSql(txtUsername.Text)
      sPassword = m_oDB_Layer.CleanStringForSql(txtPword.Text)
      m_pcl_ErrManager.LogError("Customer Logon Attempt: " & txtUsername.Text & ", " & Request.Url.ToString)

      lrRet = m_pbl_Operator.UserLogon(sEmail, sPassword)

      Select Case lrRet
        Case BL_Operator.enLogonResponse.lrOK
          m_pcl_ErrManager.LogError("Successful User Logon: " & txtUsername.Text)

          'Prepare for the redirect to the main page....
          Response.Redirect("DashBoard01.aspx", False)

        Case BL_Operator.enLogonResponse.lrSuspended
          m_pcl_Utils.DisplayMessage("User is suspended.", txtUsername.ClientID)

        Case Else
          'Invalid login send email to user
          m_pcl_ErrManager.LogError("Failed Logon: " & m_oDB_Layer.CleanStringForSql(txtUsername.Text) & ", " & Request.UserHostAddress & ", " & Request.UserHostName)
          'Invalid, display message
          m_pdl_Manager.LogEvent2DB("LOGON", "Logon: " & m_oDB_Layer.CleanStringForSql(txtUsername.Text), "Logon Failed", 0, Request, Session.SessionID)
          sControl = "txtPword"
          m_pcl_Utils.DisplayMessage("Invalid Email and Password combination.", txtUsername.ClientID)

          m_pbl_Operator.SendInvalidLoginEmail(sEmail, m_oDB_Layer, Request)

      End Select
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
    Dim sEmail As String
    Dim drOperator As DataRow
    Dim dicParms As New eDictionary
    Dim sResetID As String
    Dim dbRet As DL_Manager.DB_Return
    Dim dicEmailSubs As New eDictionary
    Dim bOK As Boolean


    If txtReminderEmail.Text.Trim <> "" Then
      revReminder.Validate()
      If revReminder.IsValid Then
        sEmail = m_oDB_Layer.CleanStringForSql(txtReminderEmail.Text.Trim)

        'Check login and validate, and load bl_operator...
        m_pcl_ErrManager.LogError("Customer Logon Reset: " & sEmail & ", " & Request.Url.ToString)

        drOperator = m_obl_OpMgr.GetOperatorByEmail(sEmail, m_pbl_Client.ClientID)

        If drOperator IsNot Nothing Then
          m_pcl_ErrManager.LogError("User Logon Reset found: " & sEmail)

          sResetID = Guid.NewGuid.ToString

          dicParms.Clear()
          dicParms.Add("Operator_PwdResetID", "S:" & sResetID)
          dicParms.Add("Operator_PwdResetExpire", "D:" & Format(Now().AddDays(1), "yyyy-MM-dd HH:mm:ss"))
          dicParms.Add("Operator_TimeStamp", "T:" & Format(CDate(drOperator!Operator_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
          dbRet = m_obl_OpMgr.UpdateOperator(CInt(drOperator!operator_id), dicParms)

          If dbRet = DL_Manager.DB_Return.ok Then

            bOK = m_pbl_Operator.SendReminder(sEmail, False, sResetID, m_pcl_Utils)
            m_pcl_Utils.DisplayMessage("A password reset link has been sent to your email address, if it is registered on StreamLine.")
          Else
            m_pcl_ErrManager.LogError("Operator Logon Problem Updating the customer record: " & sEmail)
            m_pcl_Utils.DisplayMessage("There was a problem sending the reset email, please try again if the problem persists please call us.", txtReminderEmail.ClientID)
          End If

        Else
          m_pcl_Utils.DisplayMessage("If your email is registered you will receive a password reset link.", txtUsername.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please enter a valid email address.", txtReminderEmail.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter your Email Address", txtReminderEmail.ClientID)
    End If
  End Sub




End Class