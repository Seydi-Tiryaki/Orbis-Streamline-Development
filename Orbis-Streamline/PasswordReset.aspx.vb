Public Class PasswordReset
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
      Exit Sub
    End If

    Me.Form.DefaultButton = cmdReset.UniqueID
    If Not Page.IsPostBack Then
      zProcessQueryStrings()
      zProcessResetID()

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

  Protected Sub cmdReset_Click(sender As Object, e As EventArgs) Handles cmdReset.Click
    Dim lrRet As BL_Operator.enLogonResponse = BL_Operator.enLogonResponse.lrDeactivated
    Dim sControl As String = ""
    Dim dicParms As New eDictionary
    Dim sPassword As String
    Dim drOperator As DataRow
    Dim dbRet As DL_Manager.DB_Return
    Dim sUrl As String
    Dim sScript As String
    Dim sMsg As String

    'Process the logon request....
    If zbFormvalid() Then

      'Get the customer row from the reset ID.
      drOperator = m_obl_OpMgr.GetOperatorByID(CInt(txtOperatorID.Text))

      If drOperator IsNot Nothing Then

        sPassword = m_oDB_Layer.CleanStringForSql(txtPassword1.Text.Trim)
        m_pcl_ErrManager.LogError("User Reset: " & txtOperatorID.Text & ", " & Request.Url.ToString)

        dicParms.Clear()
        dicParms.Add("Operator_Password", "S:" & m_pcl_Utils.EncryptToBase64String(sPassword))
        dicParms.Add("Operator_PwdResetID", "X:")
        dicParms.Add("Operator_PwdResetExpire", "X:")
        dicParms.Add("Operator_timestamp", "T:" & Format(CDate(drOperator!Operator_timestamp), "yyyy-MM-dd HH:mm:ss"))
        dbRet = m_obl_OpMgr.UpdateOperator(CInt(drOperator!operator_id), dicParms)

        If dbRet = DL_Manager.DB_Return.ok Then
          sMsg = "Your password has now been reset, please login to the system."
          sUrl = "default.aspx"
          sScript = "window.onload = function(){ alert('"
          sScript += sMsg
          sScript += "');"
          sScript += "window.location = '"
          sScript += sUrl
          sScript += "'; }"
          ClientScript.RegisterStartupScript(Me.GetType(), "Redirect", sScript, True)
        Else
          m_pcl_Utils.DisplayMessage("There was a problem updating the password, please perform another password reset.")
        End If

      Else
        m_pcl_Utils.DisplayMessage("User details could not be found, please contact us for support")
      End If
    Else

    End If
  End Sub

  Private Function zbFormvalid() As Boolean

    'Validate the screen fields!!!
    Dim bOK As Boolean = False

    If txtPassword1.Text.Trim <> "" Then
      If txtPassword2.Text.Trim <> "" Then
        If txtPassword1.Text.Trim = txtPassword2.Text.Trim Then
          bOK = True
        Else
          m_pcl_Utils.DisplayMessage("The passwords entered do not match, please try again.", txtPassword2.ClientID)
        End If
      Else
        m_pcl_Utils.DisplayMessage("Please enter the Password Confirmation.", txtPassword2.ClientID)
      End If
    Else
      m_pcl_Utils.DisplayMessage("Please enter your New Password.", txtPassword1.ClientID)
    End If

    Return bOK

  End Function
  Private Sub zProcessQueryStrings()
    Dim sTemp As String

    txtResetID.Text = ""
    pnlError.Visible = False
    pnlReset.Visible = False

    If Not Request.QueryString("key") Is Nothing Then
      Try
        sTemp = CStr(Request.QueryString("key"))
        sTemp = m_pcl_Utils.DecryptStringOld(sTemp)
        txtResetID.Text = sTemp
        pnlReset.Visible = True


      Catch ex As Exception
        sTemp = ""
        pnlError.Visible = True
        lblFailureMsg.Text = "Invalid Reset ID Supplied."
      End Try
    End If


  End Sub
  Private Sub zProcessResetID()
    Dim drOperator As DataRow

    pnlReset.Visible = False
    pnlError.Visible = False

    'Get the customer row from the reset ID.
    drOperator = m_obl_OpMgr.GetOperatorByPwdResetID(m_pbl_Client.ClientID, txtResetID.Text)

    If drOperator IsNot Nothing Then
      If Now() < CDate(drOperator!Operator_PwdResetExpire) Then
        txtOperatorID.Text = CInt(drOperator!operator_id)
        pnlReset.Visible = True
      Else
        pnlError.Visible = True
        lblFailureMsg.Text = "Reset ID has expired, please request a new Password Reset."
      End If
    Else
      pnlError.Visible = True
      lblFailureMsg.Text = "Password Reset ID not found, it could have already been used, please request a new Password Reset."
    End If


  End Sub

End Class