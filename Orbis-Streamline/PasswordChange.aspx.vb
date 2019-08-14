Public Class PasswordChange
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

    Me.Form.DefaultButton = cmdChangePassword.UniqueID
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
  Private Sub cmdChangePassword_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles cmdChangePassword.Click
    Try
      'Process the <Update> button.....
      Dim bValid As Boolean

      bValid = False
      If zbFormValid() Then

        If txtNewPassword1.Text.Trim <> "" Then
          If m_pbl_Operator.PWord <> txtNewPassword1.Text.Trim Then
            m_pbl_Operator.PWord = txtNewPassword1.Text.Trim
            m_pbl_Operator.UpdatePassword()
            m_pcl_Utils.DisplayMessage("Your passsword has been updated.")
          Else
            m_pcl_Utils.DisplayMessage("Old and new passwords are the same, please try again.")
          End If
        End If
      End If
    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("exception.aspx")

    End Try
  End Sub
  Private Function zbFormValid() As Boolean
    Try
      Dim bValid As Boolean
      bValid = False

      'Validate Passwords....
      If txtCurrent.Text.Trim <> "" Then
        If txtCurrent.Text.Trim = m_pbl_Operator.PWord Then
          If txtNewPassword1.Text.Trim <> "" And txtNewPassword2.Text.Trim <> "" Then
            If txtNewPassword1.Text.Trim = txtNewPassword2.Text.Trim Then
              bValid = True
            Else
              'Passwords do not match....
              m_pcl_Utils.DisplayMessage("The new passwords do not match, please try again.", txtNewPassword1.ID)
            End If
          Else
            'Passwords not entered...
            m_pcl_Utils.DisplayMessage("Enter the new password and confirmation, please try again.", txtNewPassword1.ID)
          End If
        Else
          'Current is wrong...
          m_pcl_Utils.DisplayMessage("The current password is incorrect, please try again.", txtCurrent.ID)
        End If
      Else
        'Current not entered.
        m_pcl_Utils.DisplayMessage("The current password must be entered, please try again.", txtCurrent.ID)
      End If

      'Reply back....
      Return bValid

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex
    End Try
  End Function

End Class