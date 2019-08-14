Option Strict On
Imports System.IO

Public Class ProcessEvents
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DeviceManager As BL_DeviceManager
  Private m_iMaxAutoRun As Int32 = -1
  Private m_iRunCount As Int32 = 0


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Try

      zProcessQueryStrings()

      If Not zSetupObjects() Then
        Exit Sub
      End If

      If Not Page.IsPostBack Then

        Diagnostics.Debug.WriteLine("Ckeck Close, Max Run:" & m_iMaxAutoRun & ", Run #: " & m_iRunCount)
        lblText.Text += "<br/>Max Run:" & m_iMaxAutoRun & ", Run #: " & m_iRunCount

        If m_iRunCount >= m_iMaxAutoRun Then
          lblText.Text += "<br/>Send Close Window Command....."
          m_pcl_Utils.CloseBroswerWindow()
        Else


        End If

      End If

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex, True)
      Server.Transfer("ExceptionPage.aspx")

    End Try
  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '
    If Not m_oDB_Layer Is Nothing Then
      m_oDB_Layer.Dispose()
      m_oDB_Layer = Nothing
    End If

    If Not m_oBL_DeviceManager Is Nothing Then
      m_oBL_DeviceManager.Dispose()
      m_oBL_DeviceManager = Nothing
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


    m_oBL_DeviceManager = New BL_DeviceManager(m_pdl_Manager, m_pcl_ErrManager, m_oDB_Layer)

    If Session("AutoRunCount") IsNot Nothing Then
      m_iRunCount = CInt(Session("AutoRunCount")) + 1
      Session("AutoRunCount") = m_iRunCount
      Diagnostics.Debug.WriteLine("Session Run Count:" & m_iRunCount)
    Else
      m_iRunCount = 0
      Session("AutoRunCount") = 0
      Diagnostics.Debug.WriteLine("Init Run Count:" & m_iRunCount)
    End If



    Return True

  End Function
  Private Sub zProcessQueryStrings()


    If Not Request.QueryString("MaxAutoRun") Is Nothing Then
      m_iMaxAutoRun = CInt(Request.QueryString("MaxAutoRun"))
    Else
      If InStr(Request.Url.ToString, "localhost") = 0 Then
        'Live run, so don't run....
        m_iMaxAutoRun = -1
      Else
        'Test run, just one.
        m_iMaxAutoRun = 1
      End If
    End If


  End Sub



  Private Sub zSendErrorEmail(sSubject As String, sBody As String)
    Dim sFrom As String
    Dim sTo As String
    Dim sCC As String

    sFrom = "streamline@relay.horizon-it.co.uk"
    If InStr(Request.Url.ToString, "localhost") = 0 Then
      sTo = "peto@orbis-sys.com;prodigy1@orbis-sys.com"
      sCC = "paul.craven@horizon-it.co.uk"
    Else
      sTo = "paul.craven@horizon-it.co.uk"
      sCC = ""
    End If

    m_pdl_Manager.SendEmail(sFrom, sTo, sCC, "", sSubject, sBody)

  End Sub






#End Region

End Class