Imports System.Data.SqlClient
Imports MySql.Data.MySqlClient
Imports System.Data


Public Class DB_LayerGeneric
  Implements IDisposable
  Dim m_sConn As String

  Dim m_bConnOpened As Boolean = False
  Private m_SqlConnection As SqlConnection
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_enDbType As enDatabaseType
  Private m_bDB_Log As Boolean = False
  Private m_MySqlConnection As MySqlConnection
  Private m_gUnqine As Guid = Guid.NewGuid
  Private m_bConnDebug As Boolean = True

    Public Enum QueryType
        Insert = 0
        Update = 1
    End Enum

    Public Enum enDatabaseType
    SqlServer = 0
    Access = 1
    MySql = 2
    XML = 3
  End Enum

  Public Sub New(sConn As String, ByRef pcl_ErrManager As CL_ErrManager)

    Try
      m_pcl_ErrManager = pcl_ErrManager
      m_sConn = sConn

            m_MySqlConnection = New MySqlConnection(m_sConn)

            m_enDbType = enDatabaseType.MySql

    Catch ex As Exception
      m_pcl_ErrManager.AddError(ex)
      Throw ex

    End Try

  End Sub

  Public Function RunSqlToDataRow(ByVal sSql As String, Optional dicParms As eDictionary = Nothing) As DataRow

    Dim dtStartTime As Date = Now()
    'Diagnostics.Debug.WriteLine("RunSqlToDataRow Start: " & Format(dtStartTime, "HH:mm:ss.fff"))

    'Use to retrieve a single data row.
    Dim dsNew As New DataSet()
    Dim drNew As DataRow = Nothing
    Dim dtNew As DataTable = Nothing
    Try
      If m_bDB_Log Then
        m_pcl_ErrManager.LogError("RunSqlToDataRow: " & Replace(sSql, vbCrLf, " "))
      End If
      Select Case m_enDbType
        Case enDatabaseType.SqlServer
          dtNew = RunSqlToTable(sSql, dsNew, "Temp1", dicParms)
          If dsNew.Tables(0).Rows.Count > 0 Then
            drNew = dsNew.Tables(0).Rows(0)
          Else
            drNew = Nothing
          End If
        Case enDatabaseType.MySql
          dtNew = RunSqlToTable(sSql, dsNew, "Temp1", dicParms)
          If dsNew.Tables(0).Rows.Count > 0 Then
            drNew = dsNew.Tables(0).Rows(0)
          Else
            drNew = Nothing
          End If

      End Select

      If m_bDB_Log Then
        m_pcl_ErrManager.LogError("RunSqlToDataRow: " & sSql & ", Done.")
      End If
      Return drNew


    Catch localException As Exception
      Dim sTemp As String
      Dim ocl_Utils As New CL_Utils

      sTemp = "Error in SQL: " & sSql
      sTemp += vbCrLf & ocl_Utils.DumpEdic(dicParms)
      sTemp += vbCrLf & "Error: " & localException.Message
      If localException.InnerException IsNot Nothing Then
        sTemp += vbCrLf & "Inner: " & localException.Message
      End If
      Dim exNew As New Exception(sTemp)
      m_pcl_ErrManager.AddError(exNew)
      Throw exNew

    Finally
      If Not dtNew Is Nothing Then
        dtNew.Dispose()
        drNew = Nothing
      End If
      dsNew.Dispose()
      drNew = Nothing
      'System.Diagnostics.Debug.WriteLine("RunSqlToDataRow End: " & Format(Now().Subtract(dtStartTime).TotalMilliseconds / 1000, "###0.00") & " seconds")
    End Try

  End Function
  Public Function RunSqlToTable(ByVal sSql As String, Optional ByVal pdataSet As DataSet = Nothing, Optional ByVal TableName As String = "Temp", Optional dicParms As eDictionary = Nothing) As DataTable
    Try

      Dim dtStartTime As Date = Now()
      'Diagnostics.Debug.WriteLine("RunSqlToTable Start: " & Format(dtStartTime, "HH:mm:ss.fff"))
      'Diagnostics.Debug.WriteLine("RunSqlToTable sSql: " & sSql)

      Dim pcl_Utils As New CL_Utils

      If m_bDB_Log Then
        m_pcl_ErrManager.LogError("RunSqlToTable: " & Replace(sSql, vbCrLf, " "))
      End If
      If pdataSet Is Nothing Then
        pdataSet = New DataSet()
      End If
      Select Case m_enDbType
        Case enDatabaseType.SqlServer
          If Not m_bConnOpened Then
            m_SqlConnection.Open()


            m_bConnOpened = True
          End If

          Dim cmd As SqlCommand = m_SqlConnection.CreateCommand
          cmd.CommandText = sSql


          'cmd = zProcessParms(cmd, dicParms, sSql)

          'If Not dicParms Is Nothing Then
          '  For iParm = 0 To dicParms.Count - 1
          '    'Add the parameters based on type....
          '    Select Case Left(CStr(dicParms(iParm)), 1)

          '      Case "D"
          '        cmd.Parameters.AddWithValue(dicParms.Keys(iParm), CDate(Mid(CStr(dicParms(iParm)), 3)))

          '      Case "T"
          '        Dim byTs() As Byte
          '        byTs = pcl_Utils.StringToByte(Mid(CStr(dicParms(iParm)), 3))
          '        cmd.Parameters.AddWithValue(dicParms.Keys(iParm), byTs)

          '      Case Else
          '        cmd.Parameters.AddWithValue(dicParms.Keys(iParm), Mid(CStr(dicParms(iParm)), 3))

          '    End Select

          '  Next
          'End If

          Dim sqlDA As New SqlDataAdapter(cmd)
          sqlDA.Fill(pdataSet, TableName)
          If m_bDB_Log Then
            m_pcl_ErrManager.LogError("RunSqlToTable: " & sSql & ", Done.")
          End If

          sqlDA.Dispose()
          sqlDA = Nothing
        Case enDatabaseType.MySql
          If Not m_bConnOpened Then
            m_MySqlConnection.Open()
            If m_bConnDebug Then
              Diagnostics.Debug.WriteLine("Connection Opened: " & m_gUnqine.ToString)
            End If
            'System.Diagnostics.Debug.WriteLine("RunSqlToTable Connection Complete: " & Format(Now().Subtract(dtStartTime).TotalMilliseconds / 1000, "###0.00") & " seconds")
            m_bConnOpened = True
          End If


          Dim cmd As MySqlCommand = m_MySqlConnection.CreateCommand
          cmd.CommandText = sSql

          cmd = zProcessMySqlParms(cmd, dicParms, sSql)

          Dim sqlOdbcDA As New MySqlDataAdapter(cmd)
          sqlOdbcDA.Fill(pdataSet, TableName)


      End Select

      'System.Diagnostics.Debug.WriteLine("RunSqlToTable End: " & Format(Now().Subtract(dtStartTime).TotalMilliseconds / 1000, "###0.00") & " seconds")

      Return pdataSet.Tables(TableName)

    Catch localException As Exception
      Dim sTemp As String
      Dim ocl_Utils As New CL_Utils

      sTemp = "Error in SQL: " & sSql
      If Not dicParms Is Nothing Then
        sTemp += vbCrLf & ocl_Utils.DumpEdic(dicParms)
      End If
      sTemp += vbCrLf & "Error: " & localException.Message
      If localException.InnerException IsNot Nothing Then
        sTemp += vbCrLf & "Inner: " & localException.Message
      End If
      Dim exNew As New Exception(sTemp)
      m_pcl_ErrManager.AddError(exNew)
      Throw exNew

    End Try

  End Function

  Public Function RunSqlNonQuery(ByVal sSQL As String, ByRef rowsAffected As Int32, ByVal IdRequired As Boolean, Optional ByVal TableName As String = "", Optional dicParms As eDictionary = Nothing) As Int32
    Try
      Dim iID As Int32
      Dim pcl_Utils As New CL_Utils

      Select Case m_enDbType
        Case enDatabaseType.SqlServer
          If Not m_bConnOpened Then
            m_SqlConnection.Open()
            m_bConnOpened = True
          End If

          Dim cmd As SqlCommand = m_SqlConnection.CreateCommand
          cmd.CommandText = sSQL

          'cmd = zProcessmyParms(cmd, dicParms, sSQL)

          rowsAffected = CInt(cmd.ExecuteNonQuery())
          If IdRequired And TableName <> "" Then
            If rowsAffected > 0 Then
              cmd.CommandText = "SELECT @@IDENTITY"
              iID = CInt(cmd.ExecuteScalar())
            Else
              iID = rowsAffected
            End If
          Else
            iID = rowsAffected
          End If

        Case enDatabaseType.MySql
          'm_MySqlConnection.State = ConnectionState.Openm_MySqlConnection.Open
          If Not m_bConnOpened Then
            m_MySqlConnection.Open()
            If m_bConnDebug Then
              Diagnostics.Debug.WriteLine("Connection Opened: " & m_gUnqine.ToString)
            End If
            m_bConnOpened = True
          End If
          Dim MySQLcommand As New MySqlCommand(sSQL, m_MySqlConnection)


          MySQLcommand = zProcessMySqlParms(MySQLcommand, dicParms, sSQL)
          MySQLcommand.CommandTimeout = "1200"

          rowsAffected = CInt(MySQLcommand.ExecuteNonQuery())

          If IdRequired Then
            If rowsAffected > 0 Then
              MySQLcommand.CommandText = "SELECT Last_Insert_ID()"
              iID = CInt(MySQLcommand.ExecuteScalar())
            Else
              iID = rowsAffected
            End If
          Else
            iID = rowsAffected
          End If

      End Select

      Return iID


    Catch localException As Exception
      Dim sTemp As String
      Dim ocl_Utils As New CL_Utils

      sTemp = "Error in SQL: " & sSQL
      sTemp += vbCrLf & ocl_Utils.DumpEdic(dicParms)
      sTemp += vbCrLf & "Error: " & localException.Message
      If localException.InnerException IsNot Nothing Then
        sTemp += vbCrLf & "Inner: " & localException.Message
      End If
      sTemp += vbCrLf & "Conn: " & m_sConn

      Dim exNew As New Exception(sTemp)
      m_pcl_ErrManager.AddError(exNew)
      Throw exNew
    End Try
  End Function


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
      If Not m_MySqlConnection Is Nothing Then
        If m_MySqlConnection.State = ConnectionState.Open Then
          m_MySqlConnection.Close()
          If m_bConnDebug Then
            Diagnostics.Debug.WriteLine("Connection Closed: " & m_gUnqine.ToString)
          End If

        End If
      End If
    End If
  End Sub

  Public Function GenerateSQL(ByVal eQueryType As QueryType, ByVal TableName As String, ByVal dicParms As eDictionary) As String
    Try
      Dim sSQL As String = ""
      Dim iParm As Int32

      Select Case eQueryType
        Case QueryType.Insert
          'Create an INSERT!!!
          sSQL = "INSERT INTO " & TableName & vbCrLf
          sSQL += "("
          For iParm = 0 To dicParms.Count - 1
            Dim sValue As String
            Dim sType As String
            sValue = CStr(dicParms.Item(iParm))
            sType = Left(sValue, 1)
            sValue = Mid(sValue, 3)
            'Do we need a comma?
            If sType = "T" Then
              If Right(sSQL, 4) = ", " & vbCrLf Then
                'comma from previous column, remove!
                sSQL = Left(sSQL, Len(sSQL) - 4)
              End If
            Else
              sSQL += dicParms.Keys(iParm)
              If iParm <> dicParms.Count - 1 Then
                sSQL += ", " & vbCrLf
              End If
            End If


          Next
          sSQL += ")" & vbCrLf & "VALUES (" & vbCrLf
          For iParm = 0 To dicParms.Count - 1
            Dim sValue As String
            Dim sType As String
            sValue = CStr(dicParms.Item(iParm))
            sType = Left(sValue, 1)
            sValue = Mid(sValue, 3)

            If dicParms Is Nothing Then
              'DO nothing....
            Else
              If sType <> "T" Then
                'Append non-timestamp
                sSQL += "@" & dicParms.Keys(iParm)
              End If
            End If

            'Do we need a comma?
            If sType = "T" Then
              If Right(sSQL, 4) = ", " & vbCrLf Then
                'comma from previous column, remove!
                sSQL = Left(sSQL, Len(sSQL) - 4)
              End If
            Else
              If iParm <> dicParms.Count - 1 Then
                sSQL += ", " & vbCrLf
              End If
            End If
          Next
          sSQL += ");" & vbCrLf
          'sSQL += "SELECT LAST_INSERT_ID();"

        Case QueryType.Update
          'Create an UPDATE
          sSQL = "Update " & TableName & vbCrLf
          sSQL += "Set "
          For iParm = 0 To dicParms.Count - 1
            Dim sValue As String
            Dim sType As String
            sValue = CStr(dicParms.Item(iParm))
            sType = Left(sValue, 1)
            sValue = Mid(sValue, 3)
            If sType <> "T" Then
              sSQL += dicParms.Keys().Item(iParm) & " = "
            End If

            If dicParms Is Nothing Then
              'Do Nothing....

            Else
              If sType <> "T" Then
                sSQL += "@" & dicParms.Keys(iParm)
              End If

            End If

            'Do we need a comma?
            If sType = "T" Then
              If Right(sSQL, 4) = ", " & vbCrLf Then
                'comma from previous column, remove!
                sSQL = Left(sSQL, Len(sSQL) - 4)
              End If
            Else
              If iParm <> dicParms.Count - 1 Then
                sSQL += ", " & vbCrLf
              End If
            End If

          Next

      End Select
      sSQL += " "
      Return sSQL

    Catch Localexception As Exception
      m_pcl_ErrManager.AddError(Localexception)
      Throw Localexception

    End Try
  End Function

  Public Function CleanStringForSql(ByVal sValue As String) As String
    Select Case m_enDbType
      Case enDatabaseType.Access
        sValue = Replace(sValue, "'", "''")
        sValue = Replace(sValue, Chr(34), Chr(34) & Chr(34))
      Case enDatabaseType.MySql
        sValue = Replace(sValue, "\", "\\")
        sValue = Replace(sValue, "'", "\'")
        sValue = Replace(sValue, Chr(34), "\" & Chr(34))
        sValue = Replace(sValue, "‘", "\‘")
        sValue = Replace(sValue, "’", "\’")

    End Select
    Return sValue

  End Function
  Public Function CheckDBNullStr(ByVal obj As Object) As String
    Dim objReturn As String
    If IsDBNull(obj) Then
      objReturn = ""
    Else
      objReturn = CStr(obj)
    End If
    'Use the null check to remove any single quotes...
    'Not the best place, but it'll do....
    'objReturn = Replace(CStr(objReturn), "'", "")
    Return objReturn
  End Function
  Public Function CheckDBNullInt(ByVal obj As Object) As Int32
    Dim objReturn As Int32
    If IsDBNull(obj) Then
      objReturn = 0
    Else
      objReturn = CInt(obj)
    End If
    Return objReturn
  End Function
  Public Function CheckDBNullLong(ByVal obj As Object) As Long
    Dim objReturn As Long
    If IsDBNull(obj) Then
      objReturn = 0
    Else
      objReturn = CLng(obj)
    End If
    Return objReturn
  End Function
  Public Function CheckDBNullDec(ByVal obj As Object) As Decimal
    Dim objReturn As Decimal
    If IsDBNull(obj) Then
      objReturn = 0
    Else
      objReturn = CDec(obj)
    End If
    Return objReturn
  End Function
  Public Function CheckDBNullBool(ByVal obj As Object) As Boolean
    Dim objReturn As Boolean
    If IsDBNull(obj) Then
      objReturn = False
    Else
      objReturn = CBool(obj)
    End If
    Return objReturn
  End Function
  Public Function CheckDBNullDate(ByVal obj As Object) As Date
    Dim objReturn As Date
    If IsDBNull(obj) Then
      objReturn = CDate("1900/01/01")
    Else
      objReturn = CDate(obj)
    End If
    Return objReturn
  End Function
  Public Function BeginTransaction() As DL_Manager.DB_Return
    Dim sSql As String
    Dim iRowAffected As Int32
    Dim dicParms As New eDictionary

    sSql = "BEGIN WORK"
    RunSqlNonQuery(sSql, iRowAffected, False, , dicParms)

    Return DL_Manager.DB_Return.ok

  End Function
  Public Function CommitTransaction() As DL_Manager.DB_Return
    Dim sSql As String
    Dim iRowAffected As Int32
    Dim dicParms As New eDictionary

    sSql = "Commit"
    RunSqlNonQuery(sSql, iRowAffected, False, , dicParms)

    Return DL_Manager.DB_Return.ok

  End Function
  Public Function RollbackTransaction() As DL_Manager.DB_Return
    Dim sSql As String
    Dim iRowAffected As Int32
    Dim dicParms As New eDictionary

    sSql = "Rollback"
    RunSqlNonQuery(sSql, iRowAffected, False, , dicParms)

    Return DL_Manager.DB_Return.ok

  End Function

  Public Function DumpSqlToText(sSql As String, dicParms As eDictionary) As String
    Dim sRet As String = ""
    Dim i As Int32

    sRet = LCase(sSql)

    For i = 0 To dicParms.Count - 1
      Select Case UCase(Left(CStr(dicParms(i)), 1))
        Case "D", "T", "S"
          sRet = Replace(sRet, "@" & LCase(dicParms.Keys(i)), "'" & LCase(Mid(CStr(dicParms(i)), 3)) & "'")

        Case "N"
          sRet = Replace(sRet, "@" & LCase(dicParms.Keys(i)), LCase(Mid(CStr(dicParms(i)), 3)))

        Case "X"
          sRet = Replace(sRet, "@" & LCase(dicParms.Keys(i)), "NULL")

      End Select

    Next

    Return sRet

  End Function


  Private Function zStripNumeric(ByVal sValue As String) As String
    sValue = Replace(sValue, ",", "")
    sValue = Replace(sValue, "£", "")
    sValue = Replace(sValue, "€", "")
    sValue = Replace(sValue, "$", "")
    sValue = Replace(sValue, "#", "")

    Return sValue
  End Function
  Private Function zProcessMySqlParms(cmd As MySqlCommand, dicparms As eDictionary, sSql As String) As MySqlCommand
    Dim iParm As Int32
    Dim pcl_Utils As New CL_Utils
    Dim bDebugOutput As Boolean = False

    If bDebugOutput Then
      Diagnostics.Debug.WriteLine("SQL: " & sSql)
    End If

    If Not dicparms Is Nothing Then
      For iParm = 0 To dicparms.Count - 1
        If bDebugOutput Then
          Diagnostics.Debug.WriteLine("Parm Key: " & dicparms.Keys(iParm) & ", Value: " & CStr(dicparms(iParm)))
        End If

        'Add the parameters based on type....
        Select Case Left(CStr(dicparms(iParm)), 1)

          Case "D"
            If Mid(CStr(dicparms(iParm)), 3) <> "" Then
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), CDate(Mid(CStr(dicparms(iParm)), 3)))
            Else
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), CDate("1900/01/01"))
            End If

          Case "T"
            If Mid(CStr(dicparms(iParm)), 3) <> "" Then
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), CDate(Mid(CStr(dicparms(iParm)), 3)))
            Else
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), CDate("1900/01/01"))
            End If

          Case "S"
            cmd.Parameters.AddWithValue(dicparms.Keys(iParm), Mid(CStr(dicparms(iParm)), 3))

          Case "N"
            If Mid(CStr(dicparms(iParm)), 3) <> "" Then
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), CDbl(Mid(CStr(dicparms(iParm)), 3)))
            Else
              'If space pass 0...
              cmd.Parameters.AddWithValue(dicparms.Keys(iParm), 0)
            End If

          Case "X"
            cmd.Parameters.AddWithValue(dicparms.Keys(iParm), DBNull.Value)

          Case Else
            cmd.Parameters.AddWithValue(dicparms.Keys(iParm), Mid(CStr(dicparms(iParm)), 3))

        End Select

      Next
    End If

    Return cmd

  End Function

End Class
