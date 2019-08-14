Public Class StreamLine01
  Inherits System.Web.UI.MasterPage

  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load

    If Not zSetupObjects() Then
      Exit Sub
    End If

    zBuildNav()



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



    Return True

  End Function

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

    If InStr(Request.RawUrl.ToLower, "default.aspx") > 0 Then
      sbNav.Append("<li class='active'><a href='" & sBaseUrl & "default.aspx'>Home</a></li>")
    Else
      sbNav.Append("<li><a href='" & sBaseUrl & "default.aspx'>Home</a></li>")
    End If
    hypLogo.NavigateUrl = sBaseUrl & "default.aspx"

    'Build the web cats, starting at the top level.
    sbNav.Append(zGenerateMenu())

    'Add the brands link
    If InStr(Request.RawUrl.ToLower, "brands.aspx") > 0 Then
      sbNav.Append("<li class='active'><a href='" & sBaseUrl & "brands.aspx'>Brands</a></li>")
    Else
      sbNav.Append("<li><a href='" & sBaseUrl & "brands.aspx'>Brands</a></li>")
    End If




    sbNav.Append("<li>Item 2</li>")
    sTemp = sbNav.ToString()

    If m_pdl_Manager.ServerName = "localhost" Then
      sTemp = m_pcl_Utils.ReplaceNoCase(sTemp, "href=""https://www.bathroomtraders.co.uk/", "href=""http://localhost/bathroomtraders/", StringComparison.OrdinalIgnoreCase)

    End If



    'navMenu.InnerHtml = sTemp

  End Sub

  Private Function zGenerateMenu() As String
    Dim sReturnHtml As String = ""

    sReturnHtml += "<li><a href='" & "\" & "'>" & "Dashboard" & "</a></li>" & vbCrLf

    Return sReturnHtml
  End Function

#End Region

End Class