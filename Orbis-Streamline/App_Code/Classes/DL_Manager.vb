Option Explicit On
Imports System.Net.Mail
Imports System.Net.Sockets
Imports System.Text
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Collections
Imports System.Data

Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters


<Serializable> Public Class DL_Manager

  Private m_pdtMessages As DataTable
  Private m_sLanguage As String
  Private m_sServerConnString As String
  Private m_sFileLocation As String
  Private m_sFilePath As String
  Private m_pCL_ErrManager As CL_ErrManager
  Private m_sServerName As String
  Private m_sWebPath As String

  Public Enum DB_Return
    ok
    TimeStampMismatch
    DataProblemRetry
    DeleteFailed

  End Enum


#Region "Properties and Methods"
  Public Property pCL_ErrorManager() As CL_ErrManager
    Get
      pCL_ErrorManager = m_pCL_ErrManager
    End Get
    Set(ByVal Value As CL_ErrManager)
      m_pCL_ErrManager = Value
    End Set
  End Property
  Public Property sServerConnString() As String
    Get
      sServerConnString = m_sServerConnString
    End Get
    Set(ByVal Value As String)
      m_sServerConnString = Value
    End Set
  End Property
  Public Property ServerName() As String
    Get
      ServerName = m_sServerName
    End Get
    Set(ByVal Value As String)
      m_sServerName = Value
    End Set
  End Property
  Public Property sFilePath() As String
    Get
      sFilePath = m_sFilePath
    End Get
    Set(ByVal Value As String)
      m_sFilePath = Value
    End Set
  End Property

  Public Sub SetMessages(dtMessages As DataTable)
    m_pdtMessages = dtMessages

    Dim keyColumns(0) As DataColumn
    keyColumns(0) = m_pdtMessages.Columns("msg_id")
    m_pdtMessages.PrimaryKey = keyColumns
  End Sub
  Public Function SendEmail(ByVal sFrom As String, ByVal sTo As String, ByVal sCC As String, _
                       ByVal sBCC As String, ByVal sSubject As String, ByVal sBody As String, _
                       Optional ByVal bIsHTML As Boolean = False, _
                       Optional ByVal dicParms As eDictionary = Nothing, Optional ByVal sAttachFile As String = "") As Boolean

    Try
      Dim objEmail As New System.Net.Mail.MailMessage()
      Dim oSmtpClient As SmtpClient
      Dim sSubject2 As String
      Dim sBody2 As String
      Dim sDests() As String = sTo.Split(";".ToCharArray)
      Dim bOK As Boolean = False

      'Construct the body text, if loader ID has been supplied....

      'Replace strings if required....
      If dicParms Is Nothing Then
        sSubject2 = sSubject
        sBody2 = sBody
      Else
        sSubject2 = zReplaceString(sSubject, dicParms)
        sBody2 = zReplaceString(sBody, dicParms)
      End If

      If sAttachFile.Trim <> "" Then
        objEmail.Attachments.Add(New Attachment(sAttachFile))
      End If
      Dim i As Int32
      With objEmail
        For i = 0 To sDests.GetUpperBound(0)
          .To.Add(New MailAddress(sDests(i)))
        Next
        .From = New MailAddress(sFrom)
        If sCC <> "" Then
          .CC.Add(New MailAddress(sCC))
        End If
        If sBCC <> "" Then
          .Bcc.Add(New MailAddress(sBCC))
        End If
        .Subject = sSubject
        .Body = sBody2
        .Priority = MailPriority.Normal
        .IsBodyHtml = bIsHTML
      End With

      If m_sServerName = "localhost" Then
        oSmtpClient = New SmtpClient("ds3b.horizon-it.co.uk", 25)
        oSmtpClient.Credentials = New System.Net.NetworkCredential("streamline@relay.horizon-it.co.uk", "Swimmer1024!")
        oSmtpClient.EnableSsl = False
      Else
        oSmtpClient = New SmtpClient("ds3b.horizon-it.co.uk", 25)
        oSmtpClient.Credentials = New System.Net.NetworkCredential("streamline@relay.horizon-it.co.uk", "Swimmer1024!")
        oSmtpClient.EnableSsl = False
      End If
      Try
        oSmtpClient.Send(objEmail)
        Diagnostics.Debug.WriteLine("Email send to: " & sTo)

        bOK = True

      Catch localexception As Exception
        m_pCL_ErrManager.LogError("Email Send Error: " & localexception.Message)
        If Not localexception.InnerException Is Nothing Then
          m_pCL_ErrManager.LogError("Email Send Error: " & localexception.InnerException.Message)
        End If
        Diagnostics.Debug.WriteLine("Email failed to send to: " & sTo)
        bOK = False
        Throw localexception
      End Try

      Return bOK

    Catch ex As Exception
      'm_pCL_ErrManager.AddError(ex)
      Throw ex
    End Try
  End Function

  Public Sub LogEvent2DB(ByVal Tag As String, ByVal Summary As String, ByVal Notes As String, ByVal Result As Int32, _
                      ByVal Request As HttpRequest, ByVal SessionID As String)
    'Log to database....




  End Sub
  Public Function GetMessage(ByVal lMsgID As Int32, Optional ByVal sDefault As String = "", Optional ByVal sSubs() As String = Nothing) As String
    'Locate the message...
    Dim drMessage As DataRow
    Dim sKeys(0) As String
    Dim sMsg As String

    sKeys(0) = CStr(lMsgID)
    drMessage = m_pdtMessages.Rows.Find(sKeys)
    If Not drMessage Is Nothing Then
      sMsg = CStr(drMessage!msg_text)
    Else
      If sDefault = "" Then
        sMsg = "Not Found: " & lMsgID
      Else
        sMsg = sDefault
      End If
    End If
    'Replace the substitution strings....
    If Not sSubs Is Nothing Then
      Dim i As Int32
      For i = 1 To sSubs.GetUpperBound(0) + 1
        sMsg = sMsg.Replace("%" & i.ToString, sSubs(i - 1))
      Next
    End If
    Return sMsg

  End Function
  Public Function GetSetting(ByVal sKey1 As String) As String
    Dim sRet As String = ""
    sRet = CStr(System.Configuration.ConfigurationManager.AppSettings(sKey1))


    Return sRet

  End Function
  Public Sub UpdateControlLabels(ByRef byRefpControls As ControlCollection)
    Dim pcontrol As Control
    For Each pcontrol In byRefpControls
      If pcontrol.Controls.Count > 0 Then
        UpdateControlLabels(pcontrol.Controls)
      End If
      Select Case pcontrol.GetType.Name
        Case "Label"
          Dim pLabel As Label
          pLabel = CType(pcontrol, Label)
          pLabel.Text = zReplaceCaption(pLabel.Text)
          If pLabel.CssClass = "" Then
            pLabel.CssClass = "Label1"
          End If
          pLabel = Nothing

        Case "HyperLink"
          Dim pHyper As HyperLink
          pHyper = CType(pcontrol, HyperLink)
          If pHyper.CssClass = "" Then
            pHyper.CssClass = "Hyper1"
          End If
          pHyper.Text = zReplaceCaption(pHyper.Text)
          pHyper = Nothing

        Case "LinkButton"
          Dim pHyper As LinkButton
          pHyper = CType(pcontrol, LinkButton)
          pHyper.Text = zReplaceCaption(pHyper.Text)
          If pHyper.CssClass = "" Then
            pHyper.CssClass = "Label1"
          End If
          pHyper = Nothing

        Case "TextBox"
          Dim pText As TextBox
          pText = CType(pcontrol, TextBox)
          pText.Text = zReplaceCaption(pText.Text)
          pText.ToolTip = zReplaceCaption(pText.ToolTip)
          If pText.CssClass = "" Then
            pText.CssClass = "form-control"
          End If
          pText = Nothing

        Case "DropDownList"
          Dim pList As DropDownList
          pList = CType(pcontrol, DropDownList)
          pList.ToolTip = zReplaceCaption(pList.ToolTip)
          If pList.CssClass = "" Then
            pList.CssClass = "form-control"
          End If
          pList = Nothing

        Case "DataGrid"
          Dim pGrid As DataGrid
          pGrid = CType(pcontrol, DataGrid)
          If pGrid.CssClass = "" Then
            pGrid.CssClass = "DataGridMain"
          End If
          If pGrid.HeaderStyle.CssClass = "" Then
            pGrid.HeaderStyle.CssClass = "DataGridHeader1"
          End If
          If pGrid.ItemStyle.CssClass = "" Then
            pGrid.ItemStyle.CssClass = "DataGrid1"
          End If
          If pGrid.AlternatingItemStyle.CssClass = "" Then
            pGrid.AlternatingItemStyle.CssClass = "DataGridAlt1"
          End If
          pGrid = Nothing

        Case "RadioButton"
          Dim pRadio As RadioButton
          pRadio = CType(pcontrol, RadioButton)
          pRadio.Text = zReplaceCaption(pRadio.Text)
          pRadio.ToolTip = zReplaceCaption(pRadio.ToolTip)
          If pRadio.CssClass = "" Then
            pRadio.CssClass = "Label1"
          End If
          pRadio = Nothing

        Case "Button"
          Dim pButton As Button
          pButton = CType(pcontrol, Button)
          pButton.Text = zReplaceCaption(pButton.Text)
          pButton.ToolTip = zReplaceCaption(pButton.ToolTip)
          If pButton.CssClass = "" Then
            pButton.CssClass = "form-control"
          End If

          pButton = Nothing

        Case "CheckBox"
          Dim pCheckBox As CheckBox
          pCheckBox = CType(pcontrol, CheckBox)
          pCheckBox.Text = zReplaceCaption(pCheckBox.Text)
          pCheckBox.ToolTip = zReplaceCaption(pCheckBox.ToolTip)
          pCheckBox.CssClass = "Label1"
          pCheckBox = Nothing

        Case "Image"
          Dim pImage As System.Web.UI.WebControls.Image
          pImage = CType(pcontrol, System.Web.UI.WebControls.Image)
          If pImage.CssClass = "" Then
            pImage.CssClass = "Image1"
          End If
          pImage.AlternateText = zReplaceCaption(pImage.AlternateText)
          pImage = Nothing

        Case "Menu"
          '	Dim pMenu As Menu
          '	Dim i As Int32


          '	pMenu = CType(pcontrol, Menu)
          '	For i = 0 To pMenu.Items.Count - 1
          '		pMenu.Items(i).Text = zReplaceCaption(pMenu.Items(i).Text)
          '	Next i
          ''	pLabel = Nothing

        Case Else
          'System.Diagnostics.Debug.Write("Unknown Control: " & pcontrol.GetType.Name & ", " & pcontrol.ID & vbCrLf)

      End Select
    Next
  End Sub


#End Region
#Region "Private Routines"
  Protected Function zReplaceCaption(ByVal sText As String) As String
    Dim sTmp As String
    'Replace the string with one from the messages table.....
    sText = sText.Trim
    If sText.StartsWith("!") Then
      sTmp = sText.Substring(1)
      If InStr(sTmp, "!") > 0 Then
        sTmp = sTmp.Substring(0, InStr(sTmp, "!") - 1)
        If IsNumeric(sTmp) Then
          zReplaceCaption = GetMessage(CInt(sTmp))
          zReplaceCaption = Replace(zReplaceCaption, vbCrLf, "<br>")
        Else
          zReplaceCaption = sText
        End If
      Else
        zReplaceCaption = sText
      End If
    Else
      zReplaceCaption = sText
    End If
  End Function

  Protected Function zReplaceString(ByVal sText As String, ByVal dicParms As eDictionary) As String
    Dim iParm As Int32
    'Replace the keys with the value supplied.....
    For iParm = 0 To dicParms.Count - 1
      sText = Replace(sText, dicParms.Keys(iParm), CStr(dicParms.Item(iParm)))
    Next
    Return sText
  End Function
#End Region

End Class

