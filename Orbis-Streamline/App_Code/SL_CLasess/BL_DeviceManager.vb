Option Strict On

Imports System.IO
Public Class BL_DeviceManager

  Implements IDisposable

  Private m_pdl_Manager As DL_Manager
  Private m_pcl_Utils As New CL_Utils()
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_pDB_Layer As DB_LayerGeneric


  Public Sub New(ByVal pDL_manager As DL_Manager, ByVal pCL_ErrManager As CL_ErrManager, pDB_Layer As DB_LayerGeneric)
    m_pdl_Manager = pDL_manager
    m_pcl_ErrManager = pCL_ErrManager
    m_pDB_Layer = pDB_Layer

  End Sub
#Region "Dispose"
  Public Overloads Sub Dispose() Implements IDisposable.Dispose
    zDispose(True)
    GC.SuppressFinalize(Me)
  End Sub
  Protected Overrides Sub Finalize()
    zDispose(False)
    MyBase.Finalize()
  End Sub
  Private Sub zDispose(ByVal bCloseDown As Boolean)
    If bCloseDown Then
      'Shutdown the SQL Connection....
      m_pDB_Layer = Nothing
      m_pcl_ErrManager = Nothing
      m_pdl_Manager = Nothing
    End If
  End Sub
#End Region

#Region "Public Routines"

  Public Function GetAllBufferRows() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * From iotbuffer "
    sSql += " order by buf_ID"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetDeviceTypeByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("devtype_id", "N:" & iID)

    sSql = "Select * from devicetypes where devtype_id=@devtype_id"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetDeviceByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iID)

    sSql = "Select * from devices as d1 "
    sSql += " Left join devicehardwaretypes as dh  on dh.devh_id = d1.devh_id "
    sSql += " Left join organizations as o1     on d1.org_id = o1.org_id "
    sSql += " Left join customers as c1         on d1.cust_id = c1.cust_id "
    sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    sSql += " Left join sublocations as s1         on d1.subloc_id = s1.subloc_id "
    sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    sSql += " Left join timezones as tz         on tz.timezone_ID = gl1.timezone_ID "

    sSql += " where dev_id=@dev_id"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetDeviceByImei(ByVal sImei As String) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("dev_IMEI", "S:" & sImei)

    sSql = "Select * from devices as d1 "
    sSql += " Left join devicehardwaretypes as dh  on dh.devh_id = d1.devh_id "
    sSql += " Left join organizations as o1     on d1.org_id = o1.org_id "
    sSql += " Left join customers as c1         on d1.cust_id = c1.cust_id "
    sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    sSql += " Left join timezones as tz        on tz.timezone_ID = gl1.timezone_ID "

    sSql += " where dev_IMEI=@dev_IMEI"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetAllDevices(bLiveOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from devices as d1 "
    sSql += " Left join devicehardwaretypes as dh  on dh.devh_id = d1.devh_id "
    sSql += " Left join organizations as o1 on d1.org_id = o1.org_id "
    sSql += " Left join customers as c1 on d1.cust_id = c1.cust_id "
    sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    sSql += " Left join timezones as tz on tz.timezone_ID = gl1.timezone_ID "

    If bLiveOnly Then
      sSql += " Where dev_TestData=0"
    End If
    sSql += " order by dev_id"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function InsertDevice(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "devices", dicParms)
    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "devices", dicParms)
    Return iID

  End Function
  Public Function UpdateDevice(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "devices", dicParms)
    sSql += " Where dev_id=" & iID
    sSql += " and dev_TimeStamp=@dev_TimeStamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet

  End Function
  Public Function InsertDeviceData(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "devicedata", dicParms)
    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "devicedata", dicParms)
    Return iID

  End Function
  Public Function UpdateDeviceData(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "devicedata", dicParms)
    sSql += " Where ddata_ID=" & iID
    sSql += " and ddata_TimeStamp=@ddata_TimeStamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet

  End Function
  Public Function GetLastDeviceData(ByVal iDevID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)

    sSql = "select * from devicedata where dev_id = @dev_id"
    sSql += " order by ddata_id desc limit 1"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetDeviceDataByID(ByVal iDataID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("ddata_ID", "N:" & iDataID)

    sSql = "Select * from devicedata as dd1 "
    sSql += " Left join devices as d1 on d1.dev_id = dd1.dev_id "
    sSql += " Left join customers as c1         on d1.cust_id = c1.cust_id "

    sSql += " where ddata_ID=@ddata_ID "

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetDeviceDataAndRawByID(ByVal iDataID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("ddata_ID", "N:" & iDataID)

    sSql = "Select * from devicedata as dd1 "
    sSql += " Left join devices as d1 on d1.dev_id = dd1.dev_id "
    sSql += " Left join customers as c1         on d1.cust_id = c1.cust_id "
    sSql += " left join rawdatafiles as rd on dd1.ddata_id = rd.ddata_id  "
    'sSql += " Left join sublocations as s1         on d1.subloc_id = s1.subloc_id "
    'sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    'sSql += " Left join timezones as tz         on tz.timezone_ID = gl1.timezone_ID "

    sSql += " where dd1.ddata_ID=@ddata_ID"
    sSql += " and rd.rdf_Tag = 'AUDO'"
    sSql += "  and (rdf_Type = 'T' or (rdf_Type = 'P' and rdf_TriggerNumber = -1))"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function

  Public Function GetDeviceDataByDevIDandTrigger(ByVal iDevID As Int32, iTriggerID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("dev_ID", "N:" & iDevID)
    dicParms.Add("ddata_RawDataTriggerNumber", "N:" & iTriggerID)

    sSql = "Select * from devicedata as dd1 "
    sSql += " Left join devices as d1 on d1.dev_id = dd1.dev_id "
    'sSql += " Left join customers as c1         on d1.cust_id = c1.cust_id "
    'sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    'sSql += " Left join sublocations as s1         on d1.subloc_id = s1.subloc_id "
    'sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    'sSql += " Left join timezones as tz         on tz.timezone_ID = gl1.timezone_ID "

    sSql += " where dd1.dev_ID=@dev_ID and ddata_RawDataTriggerNumber=@ddata_RawDataTriggerNumber"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function GetDeviceDataByDevIDandTriggerUnprocessed(ByVal iDevID As Int32, iTriggerID As Long) As DataTable
    Dim dtTemp As DataTable
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("dev_ID", "N:" & iDevID)
    dicParms.Add("ddata_RawDataTriggerNumber", "N:" & iTriggerID)

    sSql = "Select * from devicedata as dd1 "
    sSql += " Left join devices as d1 on d1.dev_id = dd1.dev_id "

    sSql += " where dd1.dev_ID=@dev_ID and ddata_RawDataTriggerNumber=@ddata_RawDataTriggerNumber"
    sSql += " and ddata_RawDataFileExtracted = 0"

    dtTemp = m_pDB_Layer.RunSqlToTable(sSql, ,, dicParms)
    Return dtTemp

  End Function


  Public Function DeleteIotBuffer(ByVal iID As Int32) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String

    'Fields....
    sSql = "delete From iotbuffer "
    sSql += " where buf_ID = " & iID
    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False)

    Return iRowsAffected

  End Function
  Public Function InsertIotProcessed(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "iotprocessed", dicParms)
    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "iotprocessed", dicParms)
    Return iID

  End Function
  Public Function InsertIotFailed(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "iotfailed", dicParms)
    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "iotfailed", dicParms)
    Return iID

  End Function
  Public Function GetDeviceDataForExport(dateFrom As Date, dateTo As Date) As DataTable
    Dim sSql As String
    Dim dicParms As New eDictionary
    Dim dtOrderlines As DataTable = Nothing

    'Get the Order Lines.
    dicParms.Clear()
    dicParms.Add("DateFrom", "D:" & Format(dateFrom, "yyyy-MM-dd HH:mm:ss"))
    dicParms.Add("DateTo", "D:" & Format(dateTo, "yyyy-MM-dd HH:mm:ss"))

    sSql = "SELECT * from devicedata as dd"
    sSql += " left join devices as dv on dv.dev_id = dd.dev_id"
    sSql += " left join timezones as tz on tz.timezone_ID = dd.timezone_ID"
    sSql += " left join organizations as org on org.org_id = dv.dev_id"
    sSql += " left join customers as cs on cs.cust_id = dv.cust_id"
    sSql += " left join locations as loc on loc.loc_id = dv.loc_id"
    sSql += " left join sublocations as subloc on subloc.subloc_id = dv.subloc_id"
    sSql += " left join generallocations as gl on gl.genloc_id = loc.genloc_id"
    sSql += " Left Join pipediameters as pd on pd.piped_id = dv.piped_id "
    sSql += " Left Join pipeorientations as po on po.pipeo_id = dv.pipeo_id "


    sSql += " Where ddata_DateTimeUtc >= @DateFrom and ddata_DateTimeUtc <= @DateTo "
    sSql += " order by cust_Name, loc_name, subloc_name, dev_AssetTag, ddata_ID ;"
    dtOrderlines = m_pDB_Layer.RunSqlToTable(sSql, , , dicParms)


    Return dtOrderlines
  End Function
  'Public Function GetAllorganizations() As DataTable
  '  Dim sSql As String
  '  Dim dtReturn As DataTable

  '  sSql = "Select * from organizations as o1 "
  '  sSql += " order by org_name"

  '  dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

  '  Return dtReturn

  'End Function
  Public Function getDeviceDataSummaryByType(iOrgID As Int32, iCustID As Int32, iLocID As Int32, iSubLocID As Int32, bLiveOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""
    Dim dicParms As New eDictionary

    dicParms.Add("org_id", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_id", "N:" & iSubLocID)

    sSql = "Select devtype_Name, count(*) as TypeCount, devtype_GraphColour  from devices as d1 "
    sSql += " Left join devicetypes as t1 on d1.devtype_id=t1.devtype_id"

    If iOrgID <> -1 Then
      sWhere += " where org_id=@Org_ID "
    End If

    If iCustID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += "  cust_id=@cust_ID "
    End If

    If iLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " loc_id=@loc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " subloc_id=@subloc_ID "
    End If



    If bLiveOnly Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " dev_live = -1 "
    End If



    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " group by devtype_Name "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function getDeviceCommsSummary(iOrgID As Int32, iCustID As Int32, iLocID As Int32, iSubLocID As Int32, bLiveOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""
    Dim dicParms As New eDictionary

    dicParms.Add("org_id", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_id", "N:" & iSubLocID)

    sSql = "Select AgeCheck, Count(*) as DevCount, AgeColour from "
    sSql += " (SELECT ddm.MaxDdataId,dd1.ddata_DateTimeUtc, "
    sSql += " now(),TIMESTAMPDIFF(hour, ddata_DateTimeUtc, NOW()) as AgeHours, "
    sSql += " Case "
    sSql += " WHEN  TIMESTAMPDIFF(hour, dd1.ddata_DateTimeUtc, NOW()) > 168 THEN '> 1 Week'"
    sSql += " WHEN  TIMESTAMPDIFF(hour, dd1.ddata_DateTimeUtc, NOW()) > 24 THEN '> 1 Day'"
    sSql += " ELSE 'Today'"
    sSql += " END as AgeCheck,"
    sSql += " Case "
    sSql += " WHEN  TIMESTAMPDIFF(hour, dd1.ddata_DateTimeUtc, NOW()) > 168 THEN '#F08080'"
    sSql += " WHEN  TIMESTAMPDIFF(hour, dd1.ddata_DateTimeUtc, NOW()) > 24 THEN '#fafad2'"
    sSql += " ELSE '#90ee90'"
    sSql += " END as AgeColour"
    sSql += " From devicedata As dd1,"
    sSql += " orbisstreamline.devices as d1"
    sSql += " left join (select dev_id, max(ddata_id) as MaxDdataId from devicedata group by dev_id) as ddm on ddm.dev_id = d1.dev_id"
    sSql += " where ddm.MaxDdataId = dd1.ddata_id"


    If iOrgID <> -1 Then
      sWhere += " and org_id=@Org_ID "
    End If

    If iCustID <> -1 Then
      sWhere += " and cust_id=@cust_ID "
    End If

    If iLocID <> -1 Then
      sWhere += " and loc_id=@loc_ID "
    End If

    If iSubLocID <> -1 Then
      sWhere += " and subloc_id=@subloc_ID "
    End If

    If bLiveOnly Then
      sWhere += " and dev_live = -1 "
    End If

    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " ) As temp1"
    sSql += " Group by AgeCheck"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function getDeviceLocationSummary(iOrgID As Int32, iCustID As Int32, iLocID As Int32, iSubLocID As Int32, bLiveOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""
    Dim dicParms As New eDictionary

    dicParms.Add("org_id", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_id", "N:" & iSubLocID)

    sSql = "Select loc_name, count(*) as DevCount  from devices as d1 "
    sSql += " Left join locations as l1 on d1.loc_id=l1.loc_id"

    If iOrgID <> -1 Then
      sWhere += " where org_id=@Org_ID "
    End If

    If iCustID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += "  d1.cust_id=@cust_ID "
    End If

    If iLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.loc_id=@loc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.subloc_id=@subloc_ID "
    End If

    If bLiveOnly Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " dev_live = -1 "
    End If



    If sWhere <> "" Then
      sSql += sWhere
    End If

    sSql += " group by loc_name "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceDataDailyAvg(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select STR_TO_DATE(DATE_FORMAT(ddata_LocalDateTime, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup,"
    sSql += " STR_TO_DATE(DATE_FORMAT(ddata_LocalDateTime, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup2,"
    sSql += " STR_TO_DATE(DATE_FORMAT(ddata_LocalDateTime, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup3"
    sSql += " , cast(avg(ddata_AdmBatteryVoltage) as decimal) As avgBattery"
    sSql += " , cast(avg(ddata_Battery7DMA) as decimal) As avgBattery7DMA"
    sSql += " , format(avg(ddata_BoardTemperature), 1) as avgBoardTemp"
    sSql += " , format(avg(ddata_AmbientTempC), 1) as avgPipeTemp"
    sSql += " , cast(min(ddata_StrainAvg) as decimal) As avgStrain"
    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    sSql += " , count(*) as CheckInCount"
    sSql += " from devicedata as dd"
    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    sSql += " group by DateGroup order by dategroup"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceDataStrainDailyAvg(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select STR_TO_DATE(DATE_FORMAT(ddata_LocalDateTime, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup, "
    sSql += " cast(avg(ddata_StrainAvg) as decimal) As avgStrain , "
    sSql += " cast(min(ddata_Strain7DMA) as decimal) As _7dayMovingAvg , "
    sSql += " (avg(ddata_Strain7DMA) +  abs( avg(ddata_Strain7DMA)* 0.25)) as _7dayMovingAvgPlus50,"
    sSql += " (avg(ddata_Strain7DMA) -  abs( avg(ddata_Strain7DMA)* 0.25)) as _7dayMovingAvgMinus50, "
    sSql += " (avg(ddata_Strain7DMA) +  abs( avg(ddata_Strain7DMA)* 0.4)) as _7dayMovingAvgPlus95, "
    sSql += " (avg(ddata_Strain7DMA) -  abs( avg(ddata_Strain7DMA)* 0.4)) as _7dayMovingAvgMinus95  "
    sSql += " from devicedata as dd Left join devices as d1 on d1.dev_id=dd.dev_id "
    sSql += " where dd.dev_id=@dev_id  and dev_live=-1 and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays and ddata_StrainAvg is not null"
    sSql += " group by DateGroup order by dategroup"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function

  Public Function GetDeviceFlowDailyAvg(iDevID As Int32, iDays As Int32, bVibrationOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select STR_TO_DATE(DateSeq, '%Y-%m-%d') as DateSeq, count(ddata_FlowEvent) as FlowEventCount from "
    sSql += " (select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateSeq from devicedata "
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays and dev_id=@dev_id  "
    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")) as ds"
    sSql += " left join devicedata as dd on ds.DateSeq = DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "
    sSql += " and dd.dev_id=@dev_id and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3  "
    If bVibrationOnly Then
      sSql += " and ddata_FlowEventTrigger = 1 "
    End If
    sSql += " group by DateSeq order by DateSeq"





    'Select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateGroup"
    '    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    '    sSql += " from devicedata as dd"
    '    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    '    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    '    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    '    sSql += " and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3 "
    '    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  'Public Function GetDeviceFlowDailyVibration(iDevID As Int32, iDays As Int32) As DataTable
  '  Dim sSql As String
  '  Dim dtReturn As DataTable
  '  Dim dicParms As New eDictionary

  '  dicParms.Add("dev_id", "N:" & iDevID)
  '  dicParms.Add("iDays", "N:" & iDays)

  '  sSql = "SELECT STR_TO_DATE(DATE_FORMAT(ddata_LocalDateTime, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup,  count(*) as VibrationKounter"
  '  sSql += " From (select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateSeq from devicedata "
  '  sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
  '  sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")) as ds"
  '  sSql += " left join devicedata as dd on ds.DateSeq = DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "
  '  sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
  '  sSql += " and ddata_FlowEvent = 1 and ddata_FlowEventTrigger = 1 and  ddata_FlowConfidence > 3 and dev_id=@dev_id "
  '  sSql += " group by DateGroup order by DateGroup"

  '  dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

  '  Return dtReturn

  'End Function


  Public Function GetDeviceLeakCountDaily(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select DateSeq, max(ddata_CordLeak) as CordLeakCount, avg(dd.ddata_LeakProbability) as LeakProbability, "
    sSql += " case When max(ddata_CordLeak) > 10 then 1"
    sSql += "      ELSE 0 end as CordLeakTafficLight"
    sSql += " from (select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateSeq from devicedata "
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays "
    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")) as ds"
    sSql += " left join devicedata as dd on ds.DateSeq = DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")  and dd.dev_id=@dev_id "
    sSql += " where dd.ddata_LeakProbability <> 0"
    sSql += " group by DateSeq order by DateSeq"





    'Select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateGroup"
    '    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    '    sSql += " from devicedata as dd"
    '    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    '    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    '    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    '    sSql += " and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3 "
    '    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceLeakDetailed(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "SELECT ddata_LeakProbability,  dd.* FROM orbisstreamline.devicedata as dd where ddata_LeakProbability <> 0"
    sSql += " And DateDiff(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    sSql += " And dev_id =@dev_id order by ddata_DateTimeUtc"

    'Select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateGroup"
    '    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    '    sSql += " from devicedata as dd"
    '    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    '    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    '    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    '    sSql += " and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3 "
    '    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDevicePlaciboDaily(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select DateSeq, FLOOR(RAND()*(100-0+1)+0) as PlaciboCount from "
    sSql += " (select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateSeq from devicedata "
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")) as ds"
    sSql += " group by DateSeq order by DateSeq"





    'Select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateGroup"
    '    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    '    sSql += " from devicedata as dd"
    '    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    '    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    '    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    '    sSql += " and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3 "
    '    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceZeroesDaily(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select DateSeq, 0 as PlaciboCount from "
    sSql += " (select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateSeq from devicedata "
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"")) as ds"
    sSql += " group by DateSeq order by DateSeq"





    'Select DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") as DateGroup"
    '    sSql += " , count(ddata_FlowEvent) as FlowEventCount"
    '    sSql += " from devicedata as dd"
    '    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    '    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    '    sSql += " and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays"
    '    sSql += " and ddata_FlowEvent = 1 and  ddata_FlowConfidence > 3 "
    '    sSql += " group by DATE_FORMAT(ddata_LocalDateTime, ""%Y-%m-%d"") "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetFlowUsonicSample(iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select dates, 0 as est_flow  from zz_sampleultrasonic"
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), dates) <= @iDays and dates <= NOW()"
    sSql += " order by dates "

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetStrainSample(iDays As Int32) As DataTable
    Dim sSql As String = ""
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("iDays", "N:" & iDays)

    'sSql = "Select * from zz_samplestrain"
    'sSql += " where DATEDIFF(UTC_TIMESTAMP(), dates) <= @iDays"
    'sSql += " order by dates "

    'sSql += "Select tmp1.*, 7dayMovingAvg as _7dayMovingAvg, 7dayMovingAvg * 1.02 as _7dayMovingAvgPlus50, 7dayMovingAvg * 0.5 as _7dayMovingAvgMinus50, 7dayMovingAvg * 1.95 as _7dayMovingAvgPlus95, 7dayMovingAvg * 0.05 as _7dayMovingAvgMinus50"
    'sSql += " from (Select a.dates, a.strn, Round( ( SELECT SUM(b.strn) / COUNT(strn) FROM zz_samplestrain AS b WHERE a.dates >= b.dates  and b.dates >= date_add(a.dates, INTERVAL -6 DAY)"
    'sSql += " ), 2 ) AS '7dayMovingAvg'  From zz_samplestrain as a"
    'sSql += " where DATEDIFF(UTC_TIMESTAMP(), dates) <= @iDays order by dates) as tmp1"

    sSql += " Select tmp1.*,MaxMin.*, Max1 - Min1 as range1,7dayMovingAvg as _7dayMovingAvg,"
    sSql += " 7dayMovingAvg +  ((Max1 - Min1)/2) as _7dayMovingAvgPlus50, 7dayMovingAvg -  ((Max1 - Min1)/2) as _7dayMovingAvgMinus50,"
    sSql += " 7dayMovingAvg +  ((Max1 - Min1)/1) as _7dayMovingAvgPlus95,7dayMovingAvg - ((Max1 - Min1)/1) as _7dayMovingAvgMinus95"
    sSql += " from (Select a.dates, a.strn,  Round( ( SELECT SUM(b.strn) / COUNT(strn) FROM zz_samplestrain AS b WHERE a.dates >= b.dates  and b.dates >= date_add(a.dates, INTERVAL -6 DAY)"
    sSql += " ), 2 ) AS '7dayMovingAvg' "
    sSql += " from zz_samplestrain as a"
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), dates) <= 28 and dates <= NOW() order by dates) as tmp1,"
    sSql += " (select max(strn) as Max1, min(strn) as Min1 FROM zz_samplestrain AS c where DATEDIFF(UTC_TIMESTAMP(), c.dates) <= 28 and c.dates <= NOW()) as MaxMin"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function

  Public Function GetDeviceDataTempMA(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String = ""
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql += " select STR_TO_DATE(DATE_FORMAT(ddata_DateTimeUtc, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup, "
    sSql += " avg(PipeAvg) as avgPipeTemp, avg(ddata_BoardTemperature) as avgBoardTemp, Avg(Temp7MA) as avgTemp7MA, "
    sSql += " Avg(Temp7MA) +  ((Max1 - Min1)/3) as _7dayMovingAvgPlus50, Avg(Temp7MA) - ((Max1 - Min1)/3) as _7dayMovingAvgMinus50,"
    sSql += " Avg(Temp7MA) +  ((Max1 - Min1)/2) as _7dayMovingAvgPlus95, Avg(Temp7MA) - ((Max1 - Min1)/2) as _7dayMovingAvgMinus95"
    sSql += " from "
    sSql += " (Select a.ddata_id, a.ddata_DateTimeUtc, (a.ddata_Temp1Avg1+a.ddata_Temp1Avg2) /2 as PipeAvg, a.ddata_BoardTemperature,  Round( ( SELECT SUM(b.ddata_BoardTemperature) / COUNT(ddata_BoardTemperature)  "
    sSql += " FROM devicedata AS b WHERE dev_id = @dev_id and b.ddata_BoardTemperature is not null  and a.ddata_DateTimeUtc >= b.ddata_DateTimeUtc  "
    sSql += " And b.ddata_DateTimeUtc >= date_add(a.ddata_DateTimeUtc, INTERVAL - 6 DAY) and DATEDIFF(UTC_TIMESTAMP(), a.ddata_DateTimeUtc) <= 28"
    sSql += " ), 2 ) As 'Temp7MA' "
    sSql += " from devicedata as a"
    sSql += " where dev_id = @dev_id and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= 28 and ddata_DateTimeUtc <= NOW()  and ddata_BoardTemperature is not null order by ddata_DateTimeUtc) as tmp1,"
    sSql += " (select max(ddata_BoardTemperature) As Max1, min(ddata_BoardTemperature) As Min1 FROM devicedata As c where dev_id = @dev_id And DATEDIFF(UTC_TIMESTAMP(), c.ddata_DateTimeUtc) <= 28 And c.ddata_DateTimeUtc <= NOW()) As MaxMin"
    sSql += " Group by DateGroup order by dateGroup"


    'sSql += " select STR_TO_DATE(DATE_FORMAT(ddata_DateTimeUtc, '%Y-%m-%d'), '%Y-%m-%d')  as DateGroup, "
    'sSql += " avg(ddata_Temp1Avg1) as avgTemp, Avg(Temp7MA) as avgTemp7MA, "
    'sSql += " Avg(Temp7MA) +  ((Max1 - Min1)/3) as _7dayMovingAvgPlus50, Avg(Temp7MA) - ((Max1 - Min1)/3) as _7dayMovingAvgMinus50,"
    'sSql += " Avg(Temp7MA) +  ((Max1 - Min1)/2) as _7dayMovingAvgPlus95, Avg(Temp7MA) - ((Max1 - Min1)/2) as _7dayMovingAvgMinus95"
    'sSql += " from "
    'sSql += " (Select a.ddata_id, a.ddata_DateTimeUtc, a.ddata_Temp1Avg1,  Round( ( SELECT SUM(b.ddata_Temp1Avg1) / COUNT(ddata_Temp1Avg1)  "
    'sSql += " FROM devicedata AS b WHERE dev_id = @dev_id and b.ddata_Temp1Avg1 is not null  and a.ddata_DateTimeUtc >= b.ddata_DateTimeUtc  "
    'sSql += " And b.ddata_DateTimeUtc >= date_add(a.ddata_DateTimeUtc, INTERVAL - 6 DAY) and DATEDIFF(UTC_TIMESTAMP(), a.ddata_DateTimeUtc) <= 28"
    'sSql += " ), 2 ) As 'Temp7MA' "
    'sSql += " from devicedata as a"
    'sSql += " where dev_id = @dev_id and DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= 28 and ddata_DateTimeUtc <= NOW()  and ddata_Temp1Avg1 is not null order by ddata_DateTimeUtc) as tmp1,"
    'sSql += " (select max(ddata_Temp1Avg1) As Max1, min(ddata_Temp1Avg1) As Min1 FROM devicedata As c where dev_id = @dev_id And DATEDIFF(UTC_TIMESTAMP(), c.ddata_DateTimeUtc) <= 28 And c.ddata_DateTimeUtc <= NOW()) As MaxMin"
    'sSql += " Group by DateGroup order by dateGroup"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    For i = 0 To dtReturn.Rows.Count - 1
      Diagnostics.Debug.WriteLine(m_pcl_Utils.DumpRowToText(dtReturn.Rows(i)))
    Next

    Return dtReturn

  End Function
  Public Function GetDeviceDetailed(iDevID As Int32, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary
    Dim dtFrom As Date

    dtFrom = Today.AddDays(iDays * -1)

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)
    dicParms.Add("FromDate", "D:" & Format(dtFrom, "yyyy-MM-dd 00:00:00"))

    sSql = "Select DATE_FORMAT(ddata_LocalDateTime, '%Y/%m/%d %h:%i' ) as GraphDate, dd.*, d1.*, c1.* FROM orbisstreamline.devicedata as dd"
    sSql += " Left join devices as d1 on d1.dev_id=dd.dev_id"
    sSql += " Left join customers as c1 on d1.cust_id=c1.cust_id"
    sSql += " where dd.dev_id=@dev_id and dev_live=-1"
    sSql += " and ddata_DateTimeUtc >= @FromDate order by ddata_ID"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDevicesForList(iOrgID As Int32, iCustID As Int32, iLocID As Int32, iSubLocID As Int32, bLiveOnly As Boolean) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""
    Dim dicParms As New eDictionary

    dicParms.Add("org_id", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_id", "N:" & iSubLocID)

    sSql = "Select * from devices as d1 "
    sSql += " Left join organizations as o1 on d1.org_id = o1.org_id "
    sSql += " Left join customers as c1 on d1.cust_id = c1.cust_id "
    sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    sSql += " Left join timezones as tz on tz.timezone_ID = gl1.timezone_ID "
    sSql += " Left join sublocations as sl1         on d1.subloc_id = sl1.subloc_id "
    sSql += " left join (select d2.* from devicedata as d2, (select dev_id , max(ddata_id) as MaxID from devicedata group by Dev_ID) as t1 where d2.ddata_id=t1.MaxID) as ldd on ldd.Dev_ID = d1.dev_id "


    If iOrgID <> -1 Then
      sWhere += " where d1.org_id=@Org_ID "
    End If

    If iCustID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += "  d1.cust_id=@cust_ID "
    End If

    If iLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.loc_id=@loc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.subloc_id=@subloc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.subloc_id=@subloc_ID "
    End If

    If bLiveOnly Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.dev_live = -1 "
    End If



    If sWhere <> "" Then
      sSql += sWhere
    End If
    sSql += " order by org_name,cust_name,loc_name,subloc_name, dev_AssetTag"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDevicesForStatusList(iOrgID As Int32, iCustID As Int32, iLocID As Int32, iSubLocID As Int32, bLiveOnly As Boolean, iHours As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim sWhere As String = ""
    Dim dicParms As New eDictionary
    Dim dtFromDate As Date

    dtFromDate = Today.AddHours(iHours * -1)

    dicParms.Add("org_id", "N:" & iOrgID)
    dicParms.Add("cust_id", "N:" & iCustID)
    dicParms.Add("loc_id", "N:" & iLocID)
    dicParms.Add("subloc_id", "N:" & iSubLocID)
    dicParms.Add("FromDate", "D:" & Format(dtFromDate, "yyyy-MM-dd 00:00:00"))

    sSql = "Select * from devices as d1 "
    sSql += " Left join organizations as o1 on d1.org_id = o1.org_id "
    sSql += " Left join customers as c1 on d1.cust_id = c1.cust_id "
    sSql += " Left join locations as l1         on d1.loc_id = l1.loc_id "
    sSql += " Left join generallocations as gl1 on l1.genloc_id = gl1.genloc_id "
    sSql += " Left join timezones as tz on tz.timezone_ID = gl1.timezone_ID "
    sSql += " Left join sublocations as sl1         on d1.subloc_id = sl1.subloc_id "
    ' sSql += " left join (select dev_id, max(ddata_DateTimeUtc) as MaxDdataDateUtc, max(ddata_LocalDateTime) as MaxDdataDateLocal from devicedata group by dev_id) as ddm on ddm.dev_id = d1.dev_id"
    sSql += " left join (select d2.* from devicedata as d2, (select dev_id , max(ddata_id) as MaxID from devicedata group by Dev_ID) as t1 where d2.ddata_id=t1.MaxID) as ldd on ldd.Dev_ID = d1.dev_id "
    'sSql += " left join (select dev_id, count(ddata_FlowEvent) as FlowEventCount from devicedata FORCE INDEX (DateTimeUtc3) where  HOUR(TIMEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc)) <= @iHours and ddata_FlowEvent = 1 and ddata_FlowConfidence > 3  group by dev_id) as FE on fe.Dev_ID = d1.dev_id  "
    'sSql += " left join (select dev_id, count(ddata_FlowEvent) as FlowVibrationCount from devicedata FORCE INDEX (DateTimeUtc3) where HOUR(TIMEDIFF(UTC_TIMESTAMP() and ddata_FlowEvent = 1 and ddata_FlowConfidence > 3 and ddata_FlowEventTrigger = 1 , ddata_DateTimeUtc)) <= @iHours group by dev_id) as FEV on FEV.Dev_ID = d1.dev_id  "
    'sSql += " left join (select dev_id, count(ddata_FlowEvent) as FlowAcousticCount from devicedata  FORCE INDEX (DateTimeUtc3) where HOUR(TIMEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc)) <= @iHours and ddata_FlowEvent = 1 and ddata_FlowConfidence > 3 and ddata_FlowEventTrigger = 2  group by dev_id) as FEA on FEA.Dev_ID = d1.dev_id  "
    'sSql += " left join (select dev_id, (avg(ddata_Temp1Avg1) + avg(ddata_Temp1Avg2)) /2 as PipeTempAvg  , avg(ddata_StrainAvg) as PressureAvg, avg(ddata_Strain7DMA) as _7dmaAvg from devicedata  FORCE INDEX (DateTimeUtc3)  where  HOUR(TIMEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc)) <= @iHours group by dev_id) as PTA on PTA.Dev_ID = d1.dev_id   "
    sSql += " left join (select dev_id, avg(ddata_AmbientTempC)  as PipeTempAvg  , avg(ddata_StrainAvg) as PressureAvg, avg(ddata_Strain7DMA) as _7dmaAvg from devicedata  FORCE INDEX (DateTimeUtc3)  where  ddata_LocalDateTime >= @FromDate group by dev_id) as PTA on PTA.Dev_ID = d1.dev_id   "
    ' sSql += " LEFT JOIN (select dev_id, count(ddata_LeakProbability) as LeakDetectCount,Avg(ddata_LeakProbability) as LeakDetectAvg,max(ddata_LeakProbability) as LeakDetectMax "
    ' sSql += " from devicedata  FORCE INDEX (DateTimeUtc3)  where HOUR(TIMEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc)) <= @iHours and ddata_LeakProbability > 1 group by dev_id) as LD  on LD.Dev_ID = d1.dev_id"



    If iOrgID <> -1 Then
      sWhere += " where d1.org_id=@Org_ID "
    End If

    If iCustID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += "  d1.cust_id=@cust_ID "
    End If

    If iLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.loc_id=@loc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.subloc_id=@subloc_ID "
    End If

    If iSubLocID <> -1 Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.subloc_id=@subloc_ID "
    End If

    If bLiveOnly Then
      If sWhere = "" Then
        sWhere += " Where "
      Else
        sWhere += " and "
      End If
      sWhere += " d1.dev_live = -1 "
    End If



    If sWhere <> "" Then
      sSql += sWhere
    End If
    sSql += " order by org_name,cust_name,loc_name,subloc_name, dev_AssetTag"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetAllDeviceTypes() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from devicetypes as d1 "
    sSql += " order by devtype_name"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetAllDeviceHardwareTypes() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from devicehardwaretypes as d1 "
    sSql += " order by devh_Description"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetAllPipeOrientations() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from pipeorientations as d1 "
    sSql += " order by pipeo_Orientation"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetAllPipeDiameters() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable

    sSql = "Select * from pipediameters as d1 "
    sSql += " order by piped_Diameter"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql)

    Return dtReturn

  End Function
  Public Function GetDeviceDataEvents(iDevID As Int32, sEventType As String, iDays As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)
    dicParms.Add("iDays", "N:" & iDays)

    sSql = "Select *  from devicedata as dd "
    sSql += " left join rawdatafiles as rd on dd.ddata_id = rd.ddata_id "
    sSql += " where DATEDIFF(UTC_TIMESTAMP(), ddata_DateTimeUtc) <= @iDays and dd.dev_ID=@dev_id "
    Select Case sEventType
      Case "LD"
        'Leak Detection
        sSql += " and ddata_LeakDetectionGraphic is not null "
        If iDevID <> 3 Then
          sSql += " And rd.rdf_Tag = 'AUDO'"
        End If

      Case "PC"
        'Pipe Condition
        sSql += " and ddata_PipeConditionGraphic is not null "
        If iDevID <> 3 Then
          sSql += " And rd.rdf_Tag = 'PING'"
        End If

      Case "-1"
        sSql += " and ((ddata_PipeConditionGraphic is not null  and rd.rdf_Tag = 'PING') or ( ddata_LeakDetectionGraphic is not null  and rd.rdf_Tag = 'AUDO')"


    End Select
    If iDevID <> 3 Then

      sSql += " and (rdf_Type = 'T' or (rdf_Type = 'P' and rdf_TriggerNumber = -1)) "
    End If
    'sSql += " and ddata_LeakProbability > 0"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceDataForDownload() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    sSql = "Select *  from devicedata as dd "
    sSql += " LEFT JOIN devices as d1 on dd.dev_id = d1.dev_id"
    sSql += " where  ddata_RawDataFileName <> '' and ddata_RawDataFileName IS NOT NULL and ddata_RawDataFileDownloaded=0"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceDataForExtract() As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    sSql = "Select *  from devicedata as dd "
    sSql += " LEFT JOIN devices as d1 on dd.dev_id = d1.dev_id"
    sSql += " where  ddata_RawDataFileName <> '' and ddata_RawDataFileName IS NOT NULL and ddata_RawDataFileDownloaded=-1 and ddata_RawDataFileExtracted=0"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function GetDeviceHardwareTypeByCode(ByVal sCode As String) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("devh_Code", "S:" & sCode)

    sSql = "select * from devicehardwaretypes where devh_Code = @devh_Code"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function
  Public Function InsertRawDataFile(ByVal dicParms As eDictionary) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim iID As Int32

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Insert, "rawdatafiles", dicParms)
    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, "rawdatafiles", dicParms)
    Return iID

  End Function
  Public Function GetRawDeviceFileByID(ByVal iID As Int32) As DataRow
    Dim drTemp As DataRow
    Dim sSql As String
    Dim dicParms As New eDictionary

    dicParms.Add("rdf_ID", "N:" & iID)

    sSql = "Select * from rawdatafiles as rd  "
    sSql += " LEFT JOIN devicedata as dd on dd.ddata_id = rd.ddata_ID"
    sSql += " LEFT JOIN devices as d1 on d1.dev_id = rd.dev_ID"
    sSql += " where rdf_ID=@rdf_ID"

    drTemp = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    Return drTemp

  End Function

  Public Function UpdateRawDeviceFile(ByVal iID As Int32, ByVal dicParms As eDictionary) As DL_Manager.DB_Return
    Dim iRowsAffected As Int32
    Dim sSql As String
    Dim dbRet As DL_Manager.DB_Return

    'Fields....
    sSql = m_pDB_Layer.GenerateSQL(DB_LayerGeneric.QueryType.Update, "rawdatafiles", dicParms)
    sSql += " Where rdf_ID=" & iID
    sSql += " and rdf_TimeStamp=@rdf_TimeStamp"

    iID = m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, True, , dicParms)
    If iRowsAffected > 0 Then
      dbRet = DL_Manager.DB_Return.ok
    Else
      dbRet = DL_Manager.DB_Return.TimeStampMismatch
    End If

    Return dbRet

  End Function
  Public Function DeleteRawDataFileRow(ByVal iID As Int32) As Int32
    Dim iRowsAffected As Int32
    Dim sSql As String

    'Fields....
    sSql = "delete From rawdatafiles "
    sSql += " where rdf_ID = " & iID
    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False)

    Return iRowsAffected

  End Function

  Public Function getRawDataFiles4Processing(sType As String) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    sSql = "Select *  from rawdatafiles as rd "
    sSql += " LEFT JOIN devicedata as dd on dd.ddata_id = rd.ddata_ID"
    sSql += " LEFT JOIN devices as d1 on d1.dev_id = rd.dev_ID"
    Select Case sType
      Case "AUDIOAVERAGE"
        sSql += " where (rdf_Tag = 'AUDO' and (rdf_AudioEnergyProcessed IS NULL )) "

      Case "AUDIOLEAK1"
        sSql += " where (rdf_Tag = 'AUDO' and (rdf_AudioLeak1Processed IS NULL) and rdf_AudioEnergyStatus=1)  "

      Case "TEMPERATURE1"
        sSql += " where (rdf_Tag = 'TMPS' and (rdf_Temp1Processed IS NULL)) "

      Case "STRAIN1"
        sSql += " where (rdf_Tag = 'STRN' and (rdf_Strain1Processed IS NULL)) "

      Case "PIPECONDITION1"
        sSql += " where (rdf_Tag = 'PING' and (rdf_PipeCond1Processed IS NULL)) "

    End Select

    sSql += " Order by rdf_ID"
    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function

  Public Function getRawDataFilesPerdOnly(iDevID As Int32) As DataTable
    Dim sSql As String
    Dim dtReturn As DataTable
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)

    sSql = "SELECT * FROM orbisstreamline.rawdatafiles as rd  "
    'left join devicedata as dd on rd.ddata_id = dd.ddata_id
    sSql += " where rdf_ExtractedFilename like '%\_P\_%' and rd.ddata_id <> -1"

    dtReturn = m_pDB_Layer.RunSqlToTable(sSql,,, dicParms)

    Return dtReturn

  End Function
  Public Function getRawDataFilesLowestAudioPERD(iDevID As Int32) As DataRow
    Dim sSql As String
    Dim drReturn As DataRow
    Dim dicParms As New eDictionary

    dicParms.Add("dev_id", "N:" & iDevID)

    sSql = "SELECT * FROM rawdatafiles as rd  "
    sSql += " LEFT JOIN devices as d1 on d1.dev_id = rd.dev_ID"
    sSql += " where rdf_Tag = 'AUDO' and rd.dev_id = @dev_id and rdf_Type='P' "
    sSql += " and (rdf_AudioEnergy is not NULL and rdf_AudioEnergy <> 0) "
    sSql += " order by rdf_AudioEnergy asc limit 1"

    drReturn = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)

    Return drReturn

  End Function

  Public Function ConvertPatchTempToC(iPatchTemp As Int32) As Decimal
    Dim dRet As Decimal

    dRet = CDec(1.0E-19 * iPatchTemp ^ 3 + 0.0000000000003 * iPatchTemp ^ 2 + 0.000007 * iPatchTemp + 24.631)

    Return dRet

  End Function


  Public Function UpdateDeviceData47dmaCalc() As Integer
    Dim sSql As String
    Dim iRowsAffected As Int32
    Dim dicParms As New eDictionary
    Dim drAffected As DataRow

    dicParms.Add("ForcedUpdateDate", "D:" & Format(Now().AddDays(-1), "yyyy-MM-dd 00:00:00"))

    'let's smash out those 7DMA!!
    '8760 = 1 year, 192 is last 8 days data history for calcing 7DMA, we group by day and hour"

    'sSql = "  set @@cte_max_recursion_depth = 100010; "
    'sSql += " set @StartDate = DATE_ADD(STR_TO_DATE(DATE_FORMAT(now(), '%Y-%m-%d %H '), '%Y-%m-%d %H'), INTERVAL -365 DAY); "

    'sSql += " WITH RECURSIVE nums AS ( SELECT 1 AS seqvalue UNION ALL SELECT seqvalue + 1 AS seqvalue FROM nums WHERE nums.seqvalue <= 192) "
    'sSql += " update devicedata as dd4  "
    'sSql += " inner join ( "
    'sSql += " Select tmp2.dev_id, Refdate, "
    'sSql += " BattAvg, AVG(BattAvg) OVER (partition by dev_id ORDER BY dev_id,refdate ASC ROWS 168 PRECEDING) AS Batt7DMA, "
    'sSql += " BoardTempAvg, AVG(BoardTempAvg) OVER (partition by dev_id ORDER BY dev_id,refdate ASC ROWS 168 PRECEDING) AS BoardTemp7DMA, "
    'sSql += " PipeTempAvg, AVG(PipeTempAvg) OVER (partition by dev_id ORDER BY dev_id,refdate ASC ROWS 168 PRECEDING) AS PipeTemp7DMA, "
    'sSql += " LeakProbAvg, AVG(LeakProbAvg) OVER (partition by dev_id ORDER BY dev_id,refdate ASC ROWS 168 PRECEDING) AS LeakProb7DMA, "
    'sSql += " StrainAvg, AVG(StrainAvg) OVER (partition by dev_id ORDER BY dev_id,refdate ASC ROWS 168 PRECEDING) AS Strain7DMA "
    'sSql += " from "
    'sSql += " (Select refs.dev_id, refdate , DateGroup, BattAvg, BoardTempAvg, LeakProbAvg, PipeTempAvg, StrainAvg from "
    'sSql += " (SELECT dev_id, seqvalue, DATE_ADD(@StartDate, INTERVAL seqvalue hour) as RefDate FROM nums, devices order by dev_id, refdate) as refs "
    'sSql += " Left join "
    'sSql += " (SELECT dev_id, STR_TO_DATE(DATE_FORMAT(ddata_DateTimeUtc, '%Y-%m-%d %H'), '%Y-%m-%d %H')  as DateGroup, "
    'sSql += " avg(ddata_AdmBatteryVoltage) as BattAvg, "
    'sSql += " avg(ddata_BoardTemperature) as BoardTempAvg, "
    'sSql += " avg(ddata_AmbientTempC) as PipeTempAvg, "
    'sSql += " avg(ddata_LeakProbability) as LeakProbAvg, "
    'sSql += " avg(ddata_StrainAvg) as StrainAvg"
    'sSql += " from devicedata"
    'sSql += " group by dev_id,DateGroup) as tmp1 on tmp1.dategroup = refs.refdate and tmp1.dev_id=refs.dev_id"
    'sSql += " group by refs.dev_id, refdate "
    'sSql += " order by refs.dev_id, refdate ) as tmp2"
    'sSql += " where dategroup is not null"
    'sSql += " order by tmp2.dev_id , refdate"
    'sSql += " ) as tmp3 on (dd4.dev_id= tmp3.dev_id and STR_TO_DATE(DATE_FORMAT(dd4.ddata_DateTimeUtc, '%Y-%m-%d %H'), '%Y-%m-%d %H') = tmp3.refdate)"
    'sSql += " set dd4.ddata_Battery7DMA = Batt7DMA, "
    'sSql += " ddata_Strain7dma = Strain7DMA, "
    'sSql += " ddata_BoardTemp7DMA = BoardTemp7DMA,"
    'sSql += " ddata_PipeTemp7DMA = PipeTemp7DMA,"
    'sSql += " ddata_LeadProb7DMA = LeakProb7DMA"
    'sSql += " where dd4.ddata_DateTimeUtc >= @ForcedUpdateDate "
    'sSql += " Or (dd4.ddata_Battery7DMA Is null "
    'sSql += " or ddata_Strain7dma is null "
    'sSql += " or ddata_BoardTemp7DMA is Null "
    'sSql += " or ddata_PipeTemp7DMA is null "
    'sSql += " Or ddata_LeadProb7DMA Is null)"

    sSql = "call orbisstreamline.DeviceDataCalc7DMA(@ForcedUpdateDate);"

    drAffected = m_pDB_Layer.RunSqlToDataRow(sSql, dicParms)
    If drAffected IsNot Nothing Then
      iRowsAffected = CInt(drAffected!AffectedRows)
    Else
      iRowsAffected = 0
    End If


    Return iRowsAffected

  End Function
  Public Function UpdateDeviceData4Battery7dmaCalc() As Integer
    Dim sSql As String
    Dim iRowsAffected As Int32
    Dim dicParms As New eDictionary

    dicParms.Add("FromDate", "D:" & Format(Now().AddDays(-1), "yyyy-MM-dd 00:00:00"))


    sSql = "  update devicedata as dd2 "
    sSql += " inner join (SELECT dd1.ddata_id,SUM(b.ddata_AdmBatteryVoltage) / COUNT(b.ddata_AdmBatteryVoltage) AS 7dayMovAvg "
    sSql += " FROM devicedata dd1 "
    sSql += " JOIN devicedata b ON dd1.dev_id = b.dev_id  AND dd1.ddata_DateTimeUtc >= b.ddata_DateTimeUtc  "
    sSql += " AND b.ddata_DateTimeUtc >= DATE_ADD(DATE_FORMAT(dd1.ddata_DateTimeUtc, '%Y-%m-%d 00:00:00'), INTERVAL - 7 DAY)"
    sSql += " AND b.ddata_AdmBatteryVoltage IS NOT NULL "
    sSql += "     GROUP BY dd1.ddata_id ) as tmp1 on dd2.ddata_id = tmp1.ddata_id "
    sSql += " set dd2.ddata_Battery7DMA = 7dayMovAvg"
    sSql += " where dd2.ddata_AdmBatteryVoltage is not null and (dd2.ddata_DateTimeUtc >= @FromDate or dd2.ddata_Battery7DMA is null    ) "

    m_pDB_Layer.RunSqlNonQuery(sSql, iRowsAffected, False,, dicParms)


    Return iRowsAffected

  End Function
  Public Sub ClearAlgoSessionFiles(sTempAlgoSessionFolder As String)
    Try
      'm_pcl_ErrManager.LogError("zClearAlgoSessionFiles start: " & sTempAlgoSessionFolder)



      Dim fiFiles() As FileInfo
      Dim diAlgoSession As New DirectoryInfo(sTempAlgoSessionFolder)
      If diAlgoSession IsNot Nothing Then
        If diAlgoSession.Exists Then
          fiFiles = diAlgoSession.GetFiles
          m_pcl_ErrManager.LogError("zClearAlgoSessionFiles File Count: " & fiFiles.Length)
          For Each fiFile In fiFiles
            'm_pcl_ErrManager.LogError("zClearAlgoSessionFiles File: " & fiFile.Name & ", " & fiFile.Exists.ToString)
            'fiFile = Nothing
            fiFile.Delete()
          Next
        End If
        diAlgoSession = Nothing

      Else
        'm_pcl_ErrManager.LogError("zClearAlgoSessionFiles diAlgoSession is nothing!!!")

      End If
      diAlgoSession = Nothing


      'm_pcl_ErrManager.LogError("zClearAlgoSessionFiles End. ")


    Catch ex As Exception
      Throw ex
    End Try

  End Sub
#End Region

End Class
