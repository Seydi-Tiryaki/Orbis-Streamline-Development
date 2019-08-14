Option Strict On

Public Class StreamLine02
  Inherits System.Web.UI.MasterPage

  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_OpMgr As BL_OperatorManager
  Private m_dtMenuItems As DataTable

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If

    zCheckAccess()

    zBuildNav()



  End Sub
  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '


    If Not m_oBL_OpMgr Is Nothing Then
      m_oBL_OpMgr.Dispose()
      m_oBL_OpMgr = Nothing
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

    m_oBL_OpMgr = New BL_OperatorManager(m_pdl_Manager, m_pcl_ErrManager, m_pbl_Client, m_oDB_Layer)

    'Store the menu items in a session table....
    If Session("MenuItems") Is Nothing Then
      m_dtMenuItems = m_oBL_OpMgr.GetAllMenuItems()
      Session("MenuItems") = m_dtMenuItems
    Else
      m_dtMenuItems = CType(Session("MenuItems"), DataTable)
    End If


    Return True

  End Function

  Private Sub zCheckAccess()
    Dim sPage As String
    Dim uURI As Uri
    Dim iRequestClient As Int32


    Try
      uURI = Request.Url

      iRequestClient = m_pbl_Client.GetClientIDFromHostname(Request.Url.Host)
      If (Not m_pbl_Operator.ID <> -1) AndAlso iRequestClient <> CInt(m_pbl_Client.ClientID) Then
        Diagnostics.Debug.WriteLine("Menu: URL and Session Client ID mismatch.....")
        Session.Abandon()
        Response.Redirect("Default.aspx", True)
      Else
        If Not Request.QueryString("bOverRideLogon") Is Nothing Then
          If CStr(Request.QueryString("bOverRideLogon")) = "Y" Then
            Exit Sub
          End If
        End If
        sPage = uURI.Segments(uURI.Segments.GetUpperBound(0))
        If m_oBL_OpMgr.bIgnoreOperatorSecurity = False Then
          If Not m_oBL_OpMgr.CheckAccess(sPage, m_dtMenuItems, m_pbl_Operator) Then
            Diagnostics.Debug.WriteLine("Menu: Operator does not have access to: " & sPage)
            m_pcl_ErrManager.LogError("Menu: Operator does not have access to: " & sPage)

            Session.Abandon()
            Response.Redirect("Default.aspx", True)
          End If
        End If

      End If


    Catch ex As Threading.ThreadAbortException
      'Do nothing..

    Catch localException As Exception
      m_pcl_ErrManager.AddError(localException)
      Throw localException

    Finally

    End Try


  End Sub


  Private Sub zBuildNav()

    Dim sTemp As String = ""
    Dim sBaseUrl As String = ""


    'build the nav.

    Dim sbNav As New StringBuilder()
    'Dim dicWebCats As eDictionary

    'dicWebCats = m_oBL_WebStoreMgr.GetWebCatsForDic(m_pbl_Client.ClientID, m_iFeedID)

    For iSeg = 0 To Request.Url.Segments.Count - 2
      sBaseUrl += Request.Url.Segments(iSeg)
    Next

    hypLogo.NavigateUrl = sBaseUrl & "default.aspx"

    'Start building the menu....
    sbNav.Append("<ul class=""nav navbar-nav"">")

    'If InStr(Request.RawUrl.ToLower, "dashboard01.aspx") > 0 Then
    '  sbNav.Append("<li class='active'><a href='" & sBaseUrl & "dashboard01.aspx'>Dashboard</a></li>")
    'Else
    '  sbNav.Append("<li><a href='" & sBaseUrl & "dashboard01.aspx'>Dashboard</a></li>")
    'End If


    'Build the web cats, starting at the top level.
    sbNav.Append(zGenerateMenu(0))

    'Add the brands link
    If m_pbl_Operator.LoggedOn Then
      sbNav.Append("<li class='nav-link pull-right'><a href='" & sBaseUrl & "logoff.aspx'>Log Off</a></li>")
    End If
    sbNav.Append("</ul>")

    'If m_pdl_Manager.ServerName = "localhost" Then
    '  sTemp = m_pcl_Utils.ReplaceNoCase(sTemp, "href=""https://www.bathroomtraders.co.uk/", "href=""http://localhost/bathroomtraders/", StringComparison.OrdinalIgnoreCase)
    'End If


    navMenu.InnerHtml = sbNav.ToString

  End Sub

  Private Function zGenerateMenu(iLevel As Int32) As String
    Dim sReturnHtml As String = ""
    Dim dtTopMenuItems As DataTable
    Dim drMenu As DataRow
    Dim sTemp As String
    Dim sNavLabel As String = ""
    Dim bAdd As Boolean



    dtTopMenuItems = m_oBL_OpMgr.GetMenuItems(iLevel)

    For i = 0 To dtTopMenuItems.Rows.Count - 1
      drMenu = dtTopMenuItems.Rows(i)
      bAdd = False




      Select Case m_pbl_Operator.LoggedOn Or m_oBL_OpMgr.bIgnoreOperatorSecurity
        Case True
          'Logged in....
          If CBool(drMenu!Menu_Post_login) = False Then
            'Menu item not visible after login....
            bAdd = False
          Else

            If m_oBL_OpMgr.bIgnoreOperatorSecurity = False Then 'For testing
              'Verify the operator permission.....
              If Not IsDBNull(drMenu!PermGroup_ID) Then
                If m_oDB_Layer.CheckDBNullInt(drMenu!PermGroup_ID) <> -1 Then
                  If Not IsDBNull(drMenu!Perm_ID) Then
                    If m_pbl_Operator.HasAccess(CInt(drMenu!PermGroup_ID), CInt(drMenu!Perm_ID)) Then
                      'Operator has permission....
                      If Not CBool(drMenu!Menu_Hidden) Then
                        bAdd = True
                      Else
                        bAdd = False
                      End If
                    Else
                      'No Operator permission....
                      bAdd = False
                    End If
                  Else
                    'Permission mismatch, no access.....
                    bAdd = False
                  End If
                Else
                  bAdd = True
                End If
              Else
                bAdd = False
              End If
            Else
              'No security checks!!
              bAdd = True
            End If
          End If



        Case False
          'Not logged in...
          If CBool(drMenu!Menu_requires_login) Then
            'Item requires a login....
            bAdd = False
          Else
            'Menu item displayed at any time.
            If Not CBool(drMenu!Menu_Hidden) Then
              bAdd = True
            Else
              bAdd = False
            End If
          End If
      End Select

      If bAdd Then

        If UCase(m_oDB_Layer.CheckDBNullStr(drMenu!menu_ItemType)) = "I" Then
          'no sub-menu....
          '<li Class="nav-item">
          '  <a Class="nav-link" href="#">Link</a>
          '</li>
          If iLevel = 0 Then
            sReturnHtml += "<li Class=""nav-item"">" & vbCrLf
          End If

          If iLevel = 0 Then
            sReturnHtml += "  <a Class=""nav-link"" href=""" & m_oDB_Layer.CheckDBNullStr(drMenu!menu_OutLink) & """>" & m_oDB_Layer.CheckDBNullStr(drMenu!menu_Text) & "</a>" & vbCrLf
          Else
            sReturnHtml += " <div class=""dropdown-item"">"
            sReturnHtml += "   <a Class=""nav-item nav-link dropdown-item"" href=""" & m_oDB_Layer.CheckDBNullStr(drMenu!menu_OutLink) & """>" & m_oDB_Layer.CheckDBNullStr(drMenu!menu_Text) & "</a>" & vbCrLf
            sReturnHtml += " </div>"
          End If

          If iLevel = 0 Then
            sReturnHtml += "</li>" & vbCrLf
          End If

        Else
          '<li Class="nav-item dropdown">
          '  <a Class="nav-link dropdown-toggle" href="#" id="navbarDropdown" role="button" data-toggle="dropdown" aria-haspopup="true" aria-expanded="false">
          '    Dropdown
          '  </a>
          '  <div Class="dropdown-menu" aria-labelledby="navbarDropdown">
          '    <a Class="dropdown-item" href="#">Action</a>
          '    <a Class="dropdown-item" href="#">Another action</a>
          '    <div Class="dropdown-divider"></div>
          '    <a Class="dropdown-item" href="#">Something else here</a>
          '  </div>
          '</li>

          sNavLabel = "navbar" & Format(drMenu!menu_ID, "0000")
          sReturnHtml += "<li Class=""nav-item dropdown"">" & vbCrLf
          sReturnHtml += "  <a Class=""nav-link dropdown-toggle"" href=" & Chr(34) & m_oDB_Layer.CheckDBNullStr(drMenu!menu_OutLink) & Chr(34)
          sReturnHtml += " id=" & Chr(34) & sNavLabel & Chr(34) & " role=""button"" data-toggle=""dropdown"" aria-haspopup=""true"" aria-expanded=""false"""
          sReturnHtml += " >" & m_oDB_Layer.CheckDBNullStr(drMenu!menu_Text) & "</a>" & vbCrLf
          sReturnHtml += "  <div Class=""dropdown-menu"" aria-labelledby=""" & sNavLabel & """>" & vbCrLf
          sTemp = zGenerateMenu(m_oDB_Layer.CheckDBNullInt(drMenu!menu_SubMenuToDisplay))
          sReturnHtml += sTemp
          sReturnHtml += "  </div>" & vbCrLf
          sReturnHtml += "</li>" & vbCrLf



        End If
      End If
    Next

    Return sReturnHtml
  End Function

#End Region

End Class