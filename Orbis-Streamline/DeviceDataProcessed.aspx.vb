Option Strict On
Imports System.Drawing
Imports Telerik.Web.UI
Imports MySql.Data
Imports System.IO


Public Class DeviceDataProcessed
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DevManager As BL_DeviceManager
  ' Private m_dicProbabilities As New eDictionary


  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Dim sTemp As String
    Dim sDataType As String

    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - Page Load Start")

    If Not zSetupObjects() Then
      Exit Sub
    End If

    m_pcl_ErrManager.LogError("DeviceDataProcessed: Me.Load start")

    If Not Page.IsPostBack Then
      Randomize()
      txtFormRef.Text = CStr(Int(Rnd(1) * 1000000))

      If Request.QueryString("devid") IsNot Nothing Then
        Try
          sTemp = m_pcl_Utils.DecryptStringOld(Request.QueryString("devid"))
          If IsNumeric(sTemp) Then
            txtDevID.Text = sTemp
            zLoadDropdowns()

            If Request.QueryString("graphtype") IsNot Nothing Then
              sDataType = Request.QueryString("graphtype")
              If sDataType = "" Then
                sDataType = "LD"
              End If
            Else
              sDataType = "LD"
            End If

            m_pcl_Utils.DropDownSelectByValue(ddDataType, sDataType)

            ddDataType_SelectedIndexChanged(Nothing, Nothing)



          Else
              Response.Redirect("DashBoard01.aspx")
          End If

        Catch ex As Exception
          m_pcl_ErrManager.AddError(ex, True)
          Server.Transfer("ExceptionPage.aspx")
        End Try
      Else
        Response.Redirect("DashBoard01.aspx")
      End If
    End If


    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - Page Load End")

    m_pcl_ErrManager.LogError("DeviceDataProcessed: Me.Load End")

  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '

    If m_oBL_DevManager IsNot Nothing Then
      m_oBL_DevManager.Dispose()
      m_oBL_DevManager = Nothing
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

    m_oBL_DevManager = New BL_DeviceManager(m_pdl_Manager, m_pcl_ErrManager, m_oDB_Layer)

    'If Session("DeviceDataProcessed-Probabilities-" & txtFormRef.Text) IsNot Nothing Then
    '  m_dicProbabilities = CType(Session("DeviceDataProcessed-Probabilities-" & txtFormRef.Text), eDictionary)
    'End If

    Return True

  End Function


  Private Sub zLoadDeviceDetails()
    Dim drDevice As DataRow
    Dim dtDeviceEvents As DataTable
    Dim drDeviceEvents As DataRow
    Dim i As Int32
    Dim sEventsFileNameRootPath As String

    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - zLoadDeviceDetails Start")


    pnlConcat.Controls.Clear()

    sEventsFileNameRootPath = "/DevicePostProcessedData/" & Format(CInt(txtDevID.Text), "00000000") & "/"

    If ddDataType.SelectedItem.Value = "LD" Then
      pnlConcatCont.Visible = False
    Else
      pnlConcatCont.Visible = True

    End If

    drDevice = m_oBL_DevManager.GetDeviceByID(CInt(txtDevID.Text))
    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - zLoadDeviceDetails Get GetDeviceDataEvents Start")
    dtDeviceEvents = m_oBL_DevManager.GetDeviceDataEvents(CInt(txtDevID.Text), ddDataType.SelectedItem.Value, CInt(ddDateRange.SelectedItem.Value))
    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - zLoadDeviceDetails Get GetDeviceDataEvents Done")

    lblDeviceDetails.Text = m_oDB_Layer.CheckDBNullStr(drDevice!org_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!cust_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!loc_name) & ", "
    lblDeviceDetails.Text += m_oDB_Layer.CheckDBNullStr(drDevice!subloc_name)
    lblDeviceDetails.Text += ", Location Ref: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_LocationRef)
    lblDeviceDetails.Text += ", Asset Tag: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_AssetTag)
    'lblDeviceDetails.Text += ", IMEI: " & m_oDB_Layer.CheckDBNullStr(drDevice!dev_IMEI)

    ' m_dicProbabilities.Clear()

    lstDeviceEvents.Items.Clear()

    For i = dtDeviceEvents.Rows.Count - 1 To 0 Step -1
      Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - zLoadDeviceDetails row: " & i)

      drDeviceEvents = dtDeviceEvents.Rows(i)

      Dim liNew As New ListItem
      liNew.Text = Format(m_oDB_Layer.CheckDBNullDate(drDeviceEvents!ddata_LocalDateTime), "yyyy-MM-dd ddd HH:mm:ss")
      liNew.Value = m_oDB_Layer.CheckDBNullStr(drDeviceEvents!ddata_id)
      If Not IsDBNull(drDeviceEvents!ddata_PipeConditionGraphic) Then
        liNew.Text += " (PC)"
      End If
      If Not IsDBNull(drDeviceEvents!ddata_LeakDetectionGraphic) Then
        liNew.Text += " (" & Format(m_oDB_Layer.CheckDBNullDec(drDeviceEvents!ddata_LeakProbability)) & ")"
        'Select Case m_oDB_Layer.CheckDBNullDec(drDeviceEvents!ddata_LeakProbability)
        '  Case > CDec(1.9)
        '    liNew.Attributes.Add("style", "color: red;font-weight:bold;")
        '    liNew.Text += " (Leak)"
        '  Case > CDec(1.4)
        '    liNew.Attributes.Add("style", "color: orange;")
        '    liNew.Text += " (Warning)"
        '  Case Else
        '    liNew.Attributes.Add("style", "color: Green;")
        'End Select
      End If

      lstDeviceEvents.Items.Add(liNew)

      'm_dicProbabilities.Add(CStr(i), m_oDB_Layer.CheckDBNullDec(drDeviceEvents!ddata_LeakProbability))

    Next

    'Session("DeviceDataProcessed-Probabilities-" & txtFormRef.Text) = m_dicProbabilities

    If lstDeviceEvents.Items.Count > 0 Then
      lstDeviceEvents.ClearSelection()
      lstDeviceEvents.SelectedIndex = 0
      lstDeviceEvents_SelectedIndexChanged(Nothing, Nothing)
    Else
      cmdGenerateAudio.Visible = False
      pnlPlayAudio.Visible = False
    End If



    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - zLoadDeviceDetails End")


  End Sub
  Private Sub zLoadConcat()
    Dim dtDeviceEvents As DataTable
    Dim drDeviceEvents As DataRow
    Dim sEventsFileNameRootPath As String

    pnlConcat.Controls.Clear()

    If ddDataType.SelectedItem.Value = "PC" Then
      'Only load for Pipe Condition

      sEventsFileNameRootPath = "/DevicePostProcessedData/" & Format(CInt(txtDevID.Text), "00000000") & "/"

      dtDeviceEvents = m_oBL_DevManager.GetDeviceDataEvents(CInt(txtDevID.Text), ddDataType.SelectedItem.Value, CInt(ddDateRange.SelectedItem.Value))

      For i = dtDeviceEvents.Rows.Count - 1 To 0 Step -1
        drDeviceEvents = dtDeviceEvents.Rows(i)
        If ddDataType.SelectedItem.Value = "PC" And CInt(txtDevID.Text) <> 3 Then
          Dim imgNew As New System.Web.UI.WebControls.Image
          imgNew.ImageUrl = sEventsFileNameRootPath & m_oDB_Layer.CheckDBNullStr(drDeviceEvents!ddata_PipeConditionGraphic)
          imgNew.CssClass = "PipeConditionSliceImg"
          imgNew.ToolTip = Format(m_oDB_Layer.CheckDBNullDate(drDeviceEvents!ddata_LocalDateTime), "yyyy-MM-dd ddd HH:mm:ss")
          pnlConcat.Controls.Add(imgNew)



        End If
      Next

    End If

  End Sub

  Private Sub zLoadDropdowns()

    ddDataType.Items.Clear()
    ddDataType.Items.Add(New ListItem("Pipe Condition", "PC"))
    ddDataType.Items.Add(New ListItem("Leak Detection", "LD"))



    ddDateRange.Items.Clear()
    ddDateRange.Items.Add(New ListItem("7 Days", "7"))
    ddDateRange.Items.Add(New ListItem("28 Days", "28"))
    ddDateRange.Items.Add(New ListItem("2 Months", "60"))
    ddDateRange.Items.Add(New ListItem("4 Months", "120"))
    ddDateRange.Items.Add(New ListItem("6 Months", "180"))
    ddDateRange.Items.Add(New ListItem("1 Year", "365"))
    ddDateRange.Items.Add(New ListItem("2 Years", "730"))

    ddDateRange.ClearSelection()
    m_pcl_Utils.DropDownSelectByValue(ddDateRange, "28")

  End Sub



#End Region
#Region "Control Routines"
  Protected Sub ddDateRange_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddDateRange.SelectedIndexChanged
    zLoadDeviceDetails()
  End Sub

  Protected Sub lstDeviceEvents_SelectedIndexChanged(sender As Object, e As EventArgs) Handles lstDeviceEvents.SelectedIndexChanged
    Dim drDeviceData As DataRow
    Dim sEventsFileNameRootPath As String

    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - lstDeviceEvents_SelectedIndexChanged start")


    drDeviceData = m_oBL_DevManager.GetDeviceDataByID(CInt(lstDeviceEvents.SelectedItem.Value))

    sEventsFileNameRootPath = "/DevicePostProcessedData/" & Format(CInt(drDeviceData!dev_id), "00000000") & "/"

    If ddDataType.SelectedItem.Value = "LD" Then
      If Not IsDBNull(drDeviceData!ddata_LeakDetectionGraphic) Then
        Dim imgNew As New System.Web.UI.WebControls.Image
        imgNew.ImageUrl = sEventsFileNameRootPath & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_LeakDetectionGraphic)
        imgNew.CssClass = "AlgoImage1"
        pnlImages.Controls.Add(imgNew)
      End If
    End If
    If ddDataType.SelectedItem.Value = "-1" Or ddDataType.SelectedItem.Value = "PC" Then
      If Not IsDBNull(drDeviceData!ddata_PipeConditionGraphic) Then
        Dim imgNew As New System.Web.UI.WebControls.Image
        imgNew.ImageUrl = sEventsFileNameRootPath & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_PipeConditionGraphic)
        imgNew.CssClass = "AlgoImage1"
        pnlImages.Controls.Add(imgNew)
        imgNew.CssClass = "PipeConditionMainImg"

      End If
    End If


    If ddDataType.SelectedItem.Value = "LD" And CInt(drDeviceData!dev_id) <> 3 Then
      If IsDBNull(drDeviceData!ddata_AudioFile) Then
        cmdGenerateAudio.Visible = True
        pnlPlayAudio.Visible = False
      Else
        cmdGenerateAudio.Visible = False
        pnlPlayAudio.Visible = True
        audioPlayer.Src = sEventsFileNameRootPath & m_oDB_Layer.CheckDBNullStr(drDeviceData!ddata_AudioFile)

      End If
    Else
      cmdGenerateAudio.Visible = False
      pnlPlayAudio.Visible = False

    End If

    zLoadConcat()


    m_pcl_Utils.SetFocusControl(lstDeviceEvents.ClientID)

    'For i = 0 To lstDeviceEvents.Items.Count - 1

    '  Select Case CDec(m_dicProbabilities.Item(i))
    '    Case > CDec(1.9)
    '      lstDevicePdfs.Items(i).Attributes.Add("style", "color: red;font-weight:bold;")
    '    Case > CDec(1.4)
    '      lstDevicePdfs.Items(i).Attributes.Add("style", "color: Orange;")
    '    Case Else
    '      lstDevicePdfs.Items(i).Attributes.Add("style", "color: Green;")
    '  End Select

    '  m_dicProbabilities.Add(CStr(i), CDec(1.9))

    'Next
    Diagnostics.Debug.WriteLine(Format(Now(), "HH:mm:ss.fff") & " - lstDeviceEvents_SelectedIndexChanged End")


  End Sub

  Protected Sub ddDataType_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ddDataType.SelectedIndexChanged
    If ddDataType.SelectedItem.Value = "LD" Then
      litDesciption.Text = m_pdl_Manager.GetMessage(16)
      litConcatsDesc.Text = ""
    Else
      litDesciption.Text = m_pdl_Manager.GetMessage(17)
      litConcatsDesc.Text = m_pdl_Manager.GetMessage(18)
    End If
    zLoadDeviceDetails()
  End Sub

  Protected Sub cmdGenerateAudio_Click(sender As Object, e As EventArgs) Handles cmdGenerateAudio.Click
    Dim sTempAlgoFolder As String
    Dim sTempAlgoSessionFolder As String
    Dim sSessionFolderStub As String
    Dim drDevDataAndRaw As DataRow
    Dim iDataID As Int32
    Dim dicParms As New eDictionary
    Dim sDestFileName As String
    Dim dbRet As DL_Manager.DB_Return


    iDataID = CInt(lstDeviceEvents.SelectedItem.Value)

    drDevDataAndRaw = m_oBL_DevManager.GetDeviceDataAndRawByID(iDataID)

    If Not IsDBNull(drDevDataAndRaw!rdf_ExtractedFilename) Then
      'Check / Create the Temp Algo folder
      sTempAlgoFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"

      sSessionFolderStub = "Session_" & Session.SessionID
      sTempAlgoSessionFolder = sTempAlgoFolder & sSessionFolderStub
      Dim diAlgoSession As New DirectoryInfo(sTempAlgoSessionFolder)
      If diAlgoSession.Exists = False Then
        diAlgoSession.Create()
      End If
      diAlgoSession = Nothing


      'Ok, let's Check the session folder and clear it required....
      m_pcl_ErrManager.LogError("zProcessAudioLeak1, sTempAlgoSessionFolder: " & sTempAlgoSessionFolder)
      m_oBL_DevManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)


      'OK, we have matching device data, let's process.....

      'Copy Files into folder....
      'Leak BaseLine

      'File to Compare
      Dim fiRawCsv As New FileInfo(m_pdl_Manager.sFilePath & m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_FilenamePath) & m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_ExtractedFilename))
      fiRawCsv.CopyTo(sTempAlgoSessionFolder & "\" & m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_ExtractedFilename))

      m_pcl_ErrManager.LogError("cmdGenerateAudio_Click File: " & m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_ExtractedFilename))


      Dim sFolder As String
      Dim sbatchFileLocation As String

      sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
      sbatchFileLocation = sFolder & "RunCsv2Wav1.bat"


      Dim oProcess As New Process
      oProcess.StartInfo.FileName = sbatchFileLocation
      '    oProcess.StartInfo.Arguments = " .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

      'oProcess.StartInfo.UserName = "53FF8F5\AdministratorDS"
      'oProcess.StartInfo.Password = m_pcl_Utils.ConvertToSecureString("Lancster!1979")
      oProcess.StartInfo.Arguments = " .\" & sSessionFolderStub & "\ " & " " & m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_ExtractedFilename)

      oProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(sbatchFileLocation)
      oProcess.StartInfo.UseShellExecute = False

      oProcess.Start()
      oProcess.WaitForExit()

      Dim fiCheckErrorLevel As New FileInfo(sTempAlgoSessionFolder & "\ErrorLevel.Log")
      If fiCheckErrorLevel.Exists Then
        fiCheckErrorLevel = Nothing
        Dim sfileCheckErrorLevel As String = sTempAlgoSessionFolder & "\ErrorLevel.Log"
        Dim srCheckErrorLevel As StreamReader
        Dim sErrorLevel As String

        srCheckErrorLevel = File.OpenText(sfileCheckErrorLevel)
        'Now, read the entire file into a string
        sErrorLevel = srCheckErrorLevel.ReadToEnd()
        sErrorLevel = Replace(sErrorLevel, vbCrLf, "")
        'There is an odd char at the end of the error level file, just removing it.
        sErrorLevel = Left(sErrorLevel, Len(sErrorLevel) - 1)

        srCheckErrorLevel.Close()

        If sErrorLevel = "ErrorLevel:0" Then
          'we have a winner!!!!
          'Check the post processed folder....
          Dim diProcessedDeviceFolder As New DirectoryInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drDevDataAndRaw!dev_id), "00000000") & "/")
          If diProcessedDeviceFolder.Exists = False Then
            diProcessedDeviceFolder.Create()
          End If
          diProcessedDeviceFolder = Nothing

          'copy the PNG...
          Dim fiAudioFile As New FileInfo(sTempAlgoSessionFolder & "\" & Replace(m_oDB_Layer.CheckDBNullStr(drDevDataAndRaw!rdf_ExtractedFilename), "csv", "wav"))

          sDestFileName = Format(CLng(drDevDataAndRaw!ddata_id), "0000000000")
          sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullDate(drDevDataAndRaw!ddata_DateTimeUtc), "yyyy-MM-dd--HH-mm-ss")
          sDestFileName += "_" & Format(m_oDB_Layer.CheckDBNullInt(drDevDataAndRaw!ddata_RawDataTriggerNumber), "00000000")
          sDestFileName += "_AUD1.Wav"
          Dim fiAudioFileDest As New FileInfo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drDevDataAndRaw!dev_id), "00000000") & "/" & sDestFileName)

          If fiAudioFileDest.Exists Then
            fiAudioFileDest.Delete()
          End If
          fiAudioFileDest = Nothing

          fiAudioFile.CopyTo(m_pdl_Manager.sFilePath & "DevicePostProcessedData/" & Format(CInt(drDevDataAndRaw!dev_id), "00000000") & "/" & sDestFileName)
          fiAudioFile = Nothing



          Dim drDevicedata As DataRow
          drDevicedata = m_oBL_DevManager.GetDeviceDataByID(CInt(drDevDataAndRaw!ddata_id))
          dicParms.Clear()
          dicParms.Add("ddata_AudioFile", "S:" & sDestFileName)
          dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDevicedata!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))
          dbRet = m_oBL_DevManager.UpdateDeviceData(CInt(drDevicedata!ddata_id), dicParms)
          If dbRet <> DL_Manager.DB_Return.ok Then
            Throw New Exception("Error updating Device data with Audio File")
          End If

          m_pcl_ErrManager.LogError("cmdGenerateAudio_Click: SUCCESS!!: " & sDestFileName)


          lstDeviceEvents_SelectedIndexChanged(Nothing, Nothing)


        Else
          m_pcl_ErrManager.LogError("cmdGenerateAudio_Click: Fail ErrorLevel: " & sErrorLevel)
          Dim fiCheckErrorLog As New FileInfo(sTempAlgoSessionFolder & "\Error.Log")
          If fiCheckErrorLog.Exists Then
            fiCheckErrorLevel = Nothing
            Dim srCheckErrorLog As StreamReader
            Dim sErrorLog As String
            'Now, read the entire file into a string
            srCheckErrorLog = File.OpenText(sTempAlgoSessionFolder & "\Error.Log")
            sErrorLog = srCheckErrorLog.ReadToEnd()
            Throw New Exception("cmdGenerateAudio_Click:" & sErrorLog)

          Else
            Throw New Exception("cmdGenerateAudio_Click:" & sErrorLevel)
          End If
          fiCheckErrorLevel = Nothing

        End If

      Else
        m_pcl_ErrManager.LogError("cmdGenerateAudio_Click: Fail ErrorLevel Missing ")
      End If

      m_oBL_DevManager.ClearAlgoSessionFiles(sTempAlgoSessionFolder)
      Dim diAlgoSession2 As New DirectoryInfo(sTempAlgoSessionFolder)
      If diAlgoSession2.Exists Then
        diAlgoSession2.Delete()
      End If
      diAlgoSession2 = Nothing

    Else
      'The trigger or Perd does not have corresponding DeviceData, so flag as failed....
      Throw New Exception("cmdGenerateAudio_Click:" & "The trigger or Perd does not have corresponding raw audio File: " & iDataID)

    End If


  End Sub






#End Region
End Class