Imports System.Web.Optimization

Public Class Global_asax
  Inherits HttpApplication

  Public Sub New()

  End Sub

  Sub Application_Start(sender As Object, e As EventArgs)
    ' Fires when the application is started
    RouteConfig.RegisterRoutes(RouteTable.Routes)
    'BundleConfig.RegisterBundles(BundleTable.Bundles)

    Diagnostics.Debug.WriteLine("Application_Start")

    End Sub

  Sub Application_BeginRequest(ByVal sender As Object, ByVal e As EventArgs)
    '  ' Fires at the beginning of each request
    Diagnostics.Debug.WriteLine("Begin Request: " & Request.RawUrl)

  End Sub



  Sub Session_Start(ByVal sender As Object, ByVal e As EventArgs)
    ' Fires when the session is started
    'Diagnostics.Debug.WriteLine("-----------------------------------------------------")
    'Diagnostics.Debug.WriteLine("Session start: " & Session.SessionID & " @ " & Now.ToString)
    'Diagnostics.Debug.WriteLine("-----------------------------------------------------")
    'zLogError("Session Start: " & Session.SessionID & ", URL: " & Request.RawUrl)

    Diagnostics.Debug.WriteLine("Session_Start")
    Diagnostics.Debug.WriteLine("Session URL: " & Request.RawUrl)


    Dim pcl_ErrorManager As CL_ErrManager
    Dim pdl_Manager As DL_Manager

    ' Code that runs when a new session is started
    If Not Session("dl_Manager") Is Nothing Then
      pdl_Manager = CType(Session("dl_Manager"), DL_Manager)
      If pdl_Manager.ServerName = "localhost" Then
        Session("dl_Manager") = Nothing
      End If
    End If

    If Session("dl_Manager") Is Nothing Then
      'First time in!!!
      Diagnostics.Debug.WriteLine("Create DL Manager.")
      'Create Core Session classes....
      Dim ocl_ErrorManager As New CL_ErrManager()
      ocl_ErrorManager.ServerName = Request.ServerVariables.Item("SERVER_NAME").ToLower
      Session("cl_ErrManager") = ocl_ErrorManager
      pcl_ErrorManager = ocl_ErrorManager

      pdl_Manager = New DL_Manager()
      pdl_Manager.sServerConnString = zGetDBConnect()
      pdl_Manager.sFilePath = HttpContext.Current.Request.PhysicalApplicationPath


      'Get messages for session.....
      Dim oDB_Layer As New DB_LayerGeneric(pdl_Manager.sServerConnString, pcl_ErrorManager)
      Dim ssql As String
      ssql = "select * from sys_messages "
      pdl_Manager.SetMessages(oDB_Layer.RunSqlToTable(ssql))

      oDB_Layer.Dispose()

      pdl_Manager.ServerName = Request.ServerVariables.Item("SERVER_NAME").ToLower
      pdl_Manager.pCL_ErrorManager = ocl_ErrorManager
      Session("dl_Manager") = pdl_Manager

    Else
      pcl_ErrorManager = CType(Session("cl_ErrManager"), CL_ErrManager)
      pdl_Manager = CType(Session("dl_Manager"), DL_Manager)
    End If

    'Create Client
    Dim oBL_Client As New BL_Client(pdl_Manager, pcl_ErrorManager)

    oBL_Client.ClientID = oBL_Client.GetClientIDFromHostname(Request.Url.Host)
    If Not oBL_Client.ValidClientID() Then
      pcl_ErrorManager.LogError("Session Start, Client problem: " & oBL_Client.sMsg & ", URL: " & Request.Url.Host)
      Try
        Dim ex2 As New Exception("Session Start, Client problem: " & oBL_Client.sMsg & ", URL: " & Request.Url.Host)
        Throw ex2
      Catch ex As Exception
        pcl_ErrorManager.AddError(ex, True)
        Server.Transfer("ExceptionPage.aspx")

      End Try
    End If
    Session("bl_Client") = oBL_Client

    'Create Operator
    Dim pBL_Operator As New BL_Operator(pdl_Manager, pcl_ErrorManager, oBL_Client)
    Session("bl_Operator") = pBL_Operator

    End Sub


  Sub Session_End(ByVal sender As Object, ByVal e As EventArgs)

    'obtain the shutdown reason
    Dim shutdownReason As System.Web.ApplicationShutdownReason = System.Web.Hosting.HostingEnvironment.ShutdownReason

    Dim sShutdownDetail As String = ""

    Select Case shutdownReason
      Case ApplicationShutdownReason.BinDirChangeOrDirectoryRename
        sShutdownDetail = "A change was made to the bin directory or the directory was renamed"

      Case ApplicationShutdownReason.BrowsersDirChangeOrDirectoryRename
        sShutdownDetail = "A change was made to the App_browsers folder or the files contained in it"

      Case ApplicationShutdownReason.ChangeInGlobalAsax
        sShutdownDetail = "A change was made in the global.asax file"

      Case ApplicationShutdownReason.ChangeInSecurityPolicyFile
        sShutdownDetail = "A change was made in the code access security policy file"

      Case ApplicationShutdownReason.CodeDirChangeOrDirectoryRename
        sShutdownDetail = "A change was made in the App_Code folder or the files contained in it"

      Case ApplicationShutdownReason.ConfigurationChange
        sShutdownDetail = "A change was made to the application level configuration"

      Case ApplicationShutdownReason.HostingEnvironment
        sShutdownDetail = "The hosting environment shut down the application"

      Case ApplicationShutdownReason.HttpRuntimeClose
        sShutdownDetail = "A call to Close() was requested"

      Case ApplicationShutdownReason.IdleTimeout
        sShutdownDetail = "The idle time limit was reached"

      Case ApplicationShutdownReason.InitializationError
        sShutdownDetail = "An error in the initialization of the AppDomain"

      Case ApplicationShutdownReason.MaxRecompilationsReached
        sShutdownDetail = "The maximum number of dynamic recompiles of a resource limit was reached"

      Case ApplicationShutdownReason.PhysicalApplicationPathChanged
        sShutdownDetail = "A change was made to the physical path to the application"

      Case ApplicationShutdownReason.ResourcesDirChangeOrDirectoryRename
        sShutdownDetail = "A change was made to the App_GlobalResources foldr or the files contained within it"

      Case ApplicationShutdownReason.UnloadAppDomainCalled
        sShutdownDetail = "A call to UnloadAppDomain() was completed"

      Case Else
        sShutdownDetail = "Unknown shutdown reason: " & shutdownReason.ToString & ", triggered by logoff session abort."

    End Select

    zLogError("Session End: " & sShutdownDetail)

  End Sub

  Protected Function zGetDBConnect() As String
    'Connect to the Database	
    Dim sConn As String = ""
    Dim sLocation As String
    Dim bTest As Boolean

    Dim pcl_Utils As New CL_Utils()

    Dim Var1 As String
    Dim Var2 As String
    Dim Var3 As String
    Dim Var4 As String


    System.Net.ServicePointManager.SecurityProtocol = Net.SecurityProtocolType.Tls12


    sLocation = CStr(System.Configuration.ConfigurationManager.AppSettings("Location")).ToLower
    bTest = CStr(System.Configuration.ConfigurationManager.AppSettings("TestOrLive")) = "test"

        If sLocation = "horizon" Then
            If bTest Then
                Select Case UCase(System.Environment.MachineName)
                    Case "PC006"
                        'Horizon Test Database....
                        Var1 = "SERVER=hit-server05;port=3399;"
                        Var2 = "DATABASE=OrbisStreamLine;"
                        Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var3")))
                        Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var4")))
                    Case ""
                        'Seydi Test Database....
                        '--------------------------------------------
                        ' Seydi PC name in the SERVER
                        '--------------------------------------------
                        Var1 = "SERVER=seydi_mysql_server;port=3306;"
                        Var2 = "DATABASE=OrbisStreamLine;"
                        Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var3")))
                        Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var4")))

                End Select


            Else
                'Live from locahost dev!!!!
                Var1 = "SERVER=88.208.217.104;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
                Var2 = "DATABASE=OrbisStreamLine;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

                Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3ML")))
                Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4ML")))
            End If
        Else
            If bTest Then
                'Remote and test
                Var1 = "SERVER=88.208.217.104;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
                Var2 = "DATABASE=OrbisStreamLine-test;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

                Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3MT")))
                Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4MT")))
            Else
                'Live!!!
                Var1 = "SERVER=77.68.9.76;port=3399;" 'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var1"))
                Var2 = "DATABASE=OrbisStreamLine;"  'pcl_Utils.DecryptString(pDL_Manager.GetSetting("System", "Var2"))

                Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var3ML")))
                Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("system-var4ML")))

            End If
        End If

        Var1 = "SERVER=localhost;port=3306;"
        Var2 = "DATABASE=OrbisStreamLine;"
        Var3 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var3")))
        Var4 = pcl_Utils.DecryptFromBase64String(CStr(System.Configuration.ConfigurationManager.AppSettings("systest-var4")))

        sConn = Var1 & Var2 & Var3 & "; " & Var4 & ";" & "pooling=true;Max Pool Size=1000; CharSet=utf8mb4; Connect Timeout=1200"

    'zLogError("Connection: " & sConn)

    Return sConn

  End Function
  Private Sub zLogError(sText As String)
    ' Get the current HTTPContext
    Dim context As HttpContext = HttpContext.Current

    ' Get location of ErrorLogFile from Web.config fie
    Dim sPath As String = CStr(System.Configuration.ConfigurationManager.AppSettings("ErrorLogFile"))
    sPath = sPath.Replace("%date%", Format(Now, "yyyy-MM-dd"))
    If context IsNot Nothing Then
      Dim filePath As String = context.Server.MapPath("~") & "\" & sPath

      'Dim filePath As String = context.Server.MapPath(sPath)

      ' Calculate GMT offset
      Dim gmtOffset As Integer = DateTime.Compare(DateTime.Now, DateTime.UtcNow)

      Dim gmtPrefix As String
      If gmtOffset > 0 Then
        gmtPrefix = "+"
      Else
        gmtPrefix = ""
      End If

      ' Create DateTime string
      Dim errorDateTime As String =
        DateTime.Now.Year.ToString & "." & DateTime.Now.Month.ToString("00") & "." & DateTime.Now.Day.ToString("00") _
        & " @ " &
        DateTime.Now.Hour.ToString("00") & ":" & DateTime.Now.Minute.ToString("00") & ":" & DateTime.Now.Second.ToString("00") _
        & "." & DateTime.Now.Millisecond.ToString("000") _
        & " (GMT " & gmtPrefix & gmtOffset.ToString & ")"

      ' Write message to error file
      Dim sw As System.IO.StreamWriter = Nothing
      Try
        sw = New System.IO.StreamWriter(filePath, True)
        sw.WriteLine(errorDateTime & " - " & sText)
      Catch ex As Exception
        ' If error writing to file, simply continue
        Dim a As Int32 = 0
        Diagnostics.Debug.WriteLine("Error Writing to Log File:" & filePath & ", Error: " & ex.Message)

      Finally
        If Not sw Is Nothing Then
          sw.Close()
        End If

      End Try
    End If
  End Sub

  Private Sub _Global_Error(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Error
    Dim ex As Exception
    ex = Server.GetLastError
    zLogError("Global Error has occured!!!")

    If Not ex Is Nothing Then
      Diagnostics.Debug.WriteLine(ex.Message)
      zLogError("Global Main: " & ex.Message)
      If Not ex.InnerException Is Nothing Then
        zLogError("Global Inner: " & ex.InnerException.Message)
      End If
      Select Case True
        Case TypeOf ex.GetBaseException Is System.Web.HttpException
          'Http exception, what shall we do?
          Dim WebExp As HttpException = CType(ex.GetBaseException, HttpException)
          zLogError("Global error 404: " & WebExp.Message)

          If WebExp.TargetSite.Name.ToLower = "checkvirtualfileexists" Then
            'I guess we've had a 404...
            Response.Status = "404 Not Found"
            Response.StatusCode = 404
          Else
            Dim pcl_ErrMgr As CL_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)
            If Not pcl_ErrMgr Is Nothing Then
              pcl_ErrMgr.AddError(ex.GetBaseException, True)
              zLogError("Global error Redirect to exception page 1....")
              Server.Transfer("exception.aspx", False)
            End If

          End If


        Case Else
          Dim pcl_ErrMgr As CL_ErrManager

          If HttpContext.Current.Session IsNot Nothing Then
            pcl_ErrMgr = CType(HttpContext.Current.Session("cl_ErrManager"), CL_ErrManager)
            If Not pcl_ErrMgr Is Nothing Then
              pcl_ErrMgr.AddError(ex.GetBaseException, True)
              zLogError("Global error NO Redirect to exception page 2....")
              Server.Transfer("exception.aspx", False)
              'Response.Redirect("exception.aspx", False)
            End If
          End If

      End Select
    Else
      Dim pcl_ErrMgr As CL_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)
      pcl_ErrMgr.AddError(New Exception("Unknown Error has occured! Sounds like trouble"), True)
      Server.Transfer("exception.aspx", False)

    End If
  End Sub


End Class