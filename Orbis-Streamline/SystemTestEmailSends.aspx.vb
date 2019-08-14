Imports System.IO
Public Class SystemTestEmailSends
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pbl_Operator As BL_Operator
  Private m_pbl_Client As BL_Client
  Private m_pcl_Utils As CL_Utils
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_oDB_Layer As DB_LayerGeneric
  Private m_oBL_DeviceManager As BL_DeviceManager

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    If Not zSetupObjects() Then
      Exit Sub
    End If

    If Not Page.IsPostBack Then
      ddEmailType.Items.Clear()
      ddEmailType.Items.Add("Invalid Login")
      ddEmailType.Items.Add("New User")
      ddEmailType.Items.Add("User Reset")

    End If


  End Sub

  Protected Sub Page_Unload(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Unload
    '
    ' Tidy everything up and drop all pointers.
    '
    If Not m_oBL_DeviceManager Is Nothing Then
      m_oBL_DeviceManager.Dispose()
      m_oBL_DeviceManager = Nothing
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
    m_oBL_DeviceManager = New BL_DeviceManager(m_pdl_Manager, m_pcl_ErrManager, m_oDB_Layer)


    Return True

  End Function

  Protected Sub cmdSend_Click(sender As Object, e As EventArgs) Handles cmdSend.Click
    Dim bOK As Boolean = False
    Dim sResetID As String

    Select Case ddEmailType.SelectedItem.Text
      Case "Invalid Login"
        bOK = m_pbl_Operator.SendInvalidLoginEmail(txtToEmail.Text, m_oDB_Layer, Request)
      Case "New User"
        bOK = m_pbl_Operator.SendReminder(txtToEmail.Text, True, "", m_pcl_Utils)

      Case "User Reset"
        sResetID = Guid.NewGuid.ToString
        bOK = m_pbl_Operator.SendReminder(txtToEmail.Text, False, sResetID, m_pcl_Utils)

      Case Else
        m_pcl_Utils.DisplayMessage("Unknown email type: " & ddEmailType.SelectedItem.Text)
    End Select

    If bOK Then
      m_pcl_Utils.DisplayMessage("Email sent successfully")
    Else
      m_pcl_Utils.DisplayMessage("Email send failed.")

    End If


  End Sub

  Protected Sub cmdFixDupTriggers_Click(sender As Object, e As EventArgs) Handles cmdFixDupTriggers.Click
    Dim dtDevices As DataTable
    Dim drDevice As DataRow
    Dim dtDeviceData As DataTable
    Dim drDeviceData As DataRow
    Dim iDevice As Int32
    Dim iDeviceData As Int32
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim dtTester As DataTable
    Dim iTriggerNew As Int32 = 0
    Dim iTriggerStartBump As Int32 = 10000000
    Dim iTriggerBump As Int32 = 1000000
    Dim drDataTrigTest As DataRow
    Dim dbRet As DL_Manager.DB_Return

    sSql = "Select * From"
    sSql += " (select dd.dev_id, ddata_RawDataTriggerNumber, count(*) as Kounter"
    sSql += " from devicedata as dd left join devices as d1 on d1.dev_id = dd.dev_id"
    sSql += " where d1.dev_Live = -1 and ddata_RawDataTriggerNumber is not null and ddata_RawDataFileExtracted = 0 and d1.dev_id = @dev_id "
    sSql += " group by dd.dev_id, ddata_RawDataTriggerNumber having kounter > 1 order by dd.dev_id, ddata_RawDataTriggerNumber) as tmp1"
    sSql += " left join devicedata as dd2 on dd2.dev_id = tmp1.dev_id and dd2.ddata_RawDataTriggerNumber = tmp1.ddata_RawDataTriggerNumber and ddata_RawDataFileExtracted = 0"
    sSql += " order by tmp1.dev_id, tmp1.ddata_RawDataTriggerNumber, ddata_DateTimeUtc desc"

    dtDevices = m_oBL_DeviceManager.GetAllDevices(False) 'including test..... 
    For iDevice = 0 To dtDevices.Rows.Count - 1
      drDevice = dtDevices.Rows(iDevice)
      Diagnostics.Debug.WriteLine("Processing Dev: " & CStr(drDevice!dev_id) & "...")

      'Get the device data for the device

      dicParms.Clear()
      dicParms.Add("dev_id", "N:" & CStr(drDevice!dev_id))
      dtDeviceData = m_oDB_Layer.RunSqlToTable(sSql,,, dicParms)
      For iDeviceData = 0 To dtDeviceData.Rows.Count - 1
        drDeviceData = dtDeviceData.Rows(iDeviceData)
        dtTester = m_oBL_DeviceManager.GetDeviceDataByDevIDandTriggerUnprocessed(CInt(drDevice!dev_id), CInt(drDeviceData!ddata_RawDataTriggerNumber))
        If dtTester.Rows.Count > 1 Then
          Diagnostics.Debug.Write("Device: " & CStr(drDevice!dev_id) & ", Trigger: " & CStr(drDeviceData!ddata_RawDataTriggerNumber) & ": ")

          'we have to remove this trigger as it's a newer duplicate of an existing unprocessed trigger.
          iTriggerNew = CInt(drDeviceData!ddata_RawDataTriggerNumber) + iTriggerStartBump
          drDataTrigTest = m_oBL_DeviceManager.GetDeviceDataByDevIDandTrigger(CInt(drDevice!dev_id), iTriggerNew)

          Do While drDataTrigTest IsNot Nothing
            'Keep looping until we have a unqiue value.
            iTriggerNew += iTriggerBump
            drDataTrigTest = m_oBL_DeviceManager.GetDeviceDataByDevIDandTrigger(CInt(drDevice!dev_id), iTriggerNew)
          Loop

          Diagnostics.Debug.Write("New: " & iTriggerNew & ", ")

          dicParms.Clear()
          dicParms.Add("ddata_RawDataTriggerNumber", "N:" & iTriggerNew)
          dicParms.Add("ddata_RawDataFileExtracted", "N:" & "-2") 'flag as dodgy!!
          dicParms.Add("ddata_TimeStamp", "T:" & Format(CDate(drDeviceData!ddata_TimeStamp), "yyyy-MM-dd HH:mm:ss"))

          dbRet = m_oBL_DeviceManager.UpdateDeviceData(CInt(drDeviceData!ddata_id), dicParms)
          If dbRet = DL_Manager.DB_Return.ok Then
            Diagnostics.Debug.WriteLine("Updated")
          Else
            Diagnostics.Debug.WriteLine("X")
          End If


        Else
          'all good, just move on!!
          Diagnostics.Debug.WriteLine("Device: " & CStr(drDevice!dev_id) & ", Trigger: " & CStr(drDeviceData!ddata_RawDataTriggerNumber) & ": Only 1 now!!!")
        End If

      Next
    Next


  End Sub

  Protected Sub cmdProcessRawData_Click(sender As Object, e As EventArgs) Handles cmdProcessRawData.Click

  End Sub

  Protected Sub cmdCreateDir_Click(sender As Object, e As EventArgs) Handles cmdCreateDir.Click
    Dim sFolder As String
    Dim fiFiles() As FileInfo

    sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
    Dim diAlgoProcessing As New DirectoryInfo(sFolder)
    If diAlgoProcessing.Exists = False Then
      diAlgoProcessing.Create()
    End If
    diAlgoProcessing = Nothing

    sFolder += "Session_" & Session.SessionID
    Dim diAlgoSession As New DirectoryInfo(sFolder)
    If diAlgoSession.Exists = False Then
      diAlgoSession.Create()
    End If
    fiFiles = diAlgoSession.GetFiles()

    diAlgoSession.Delete()

    diAlgoSession = Nothing




  End Sub

  Protected Sub cmdCallAlgo_Click(sender As Object, e As EventArgs) Handles cmdCallAlgo.Click
    Dim sFolder As String
    Dim sFolderStub As String = "Session_Test\"
    Dim sbatchFileLocation As String

    sFolder = m_pdl_Manager.sFilePath & "App_Data\Temp\AlgoProcessing\"
    sbatchFileLocation = sFolder & "RunPlotCsv.bat"
    '" .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

    Dim oProcess As New Process
    oProcess.StartInfo.FileName = sbatchFileLocation
    oProcess.StartInfo.Arguments = " .\Session_Test\ RD_TEST_BASE_AUDO.csv RD352753090475615_5_T_001139_AUDO.csv"

    oProcess.StartInfo.WorkingDirectory = Path.GetDirectoryName(sbatchFileLocation)
    oProcess.StartInfo.UseShellExecute = False

    oProcess.Start()
    oProcess.WaitForExit()



  End Sub
End Class