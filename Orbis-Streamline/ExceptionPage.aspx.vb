Public Class ExceptionPage
  Inherits System.Web.UI.Page
  Private m_pdl_Manager As DL_Manager
  Private m_pcl_ErrManager As CL_ErrManager
  Private m_pcl_Utils As CL_Utils

  Protected Sub Page_Load(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Load
    Diagnostics.Debug.WriteLine("Exception.aspx: Loading exception page....")
    zSetupObjects()

    zPopulateDisplay()

    If Not Page.IsPostBack Then

    End If
  End Sub


  Private Sub zSetupObjects()
    'Restore dl_manager
    m_pdl_Manager = CType(Session("dl_Manager"), DL_Manager)

    'Create Utils...
    m_pcl_Utils = New CL_Utils()
    m_pcl_Utils.pPage = Page

    'Restore cl_ErrManager
    m_pcl_ErrManager = CType(Session("cl_ErrManager"), CL_ErrManager)

  End Sub
  Private Sub zPopulateDisplay()
    Dim bDisplayDetails As Boolean
    Dim sTmp As String

    With tblDetails
      Dim rowNew As New TableRow()
      Dim cellNew As New TableCell()

      cellNew.Text = "<b>We're really sorry about this!</b></br></br>"
      cellNew.Text += "An unexpected system error has occured on our system, it has been logged with our technical staff.</br></br>"
      cellNew.Text += "if you cannot complete your order, please contact us on 0800 772 0773 and we will be happy to help<br/></br>"
      cellNew.Text += "We apologise for any inconvenience caused.<br/></br>"
      If m_pdl_Manager.ServerName = "localhost" Then
        cellNew.Text += "<a href='/'>Click here to return to the home page</a> "
      Else
        cellNew.Text += "<a href='/'>Click here to return to the home page</a><br/><br/><br/>"
      End If

      rowNew.Cells.Add(cellNew)
      .Rows.Add(rowNew)

      Dim newRow2 As New TableRow
      Dim cellNew2 As New TableCell
      Dim newRow3 As New TableRow
      Dim cellNew3 As New TableCell

      cellNew2.Text = "&nbsp"
      cellNew3.Text = "&nbsp"

      newRow2.Cells.Add(cellNew2)
      newRow3.Cells.Add(cellNew3)

      .Rows.Add(newRow2)
      .Rows.Add(newRow3)

    End With


    sTmp = CStr(System.Configuration.ConfigurationManager.AppSettings("DisplayErrorDetails"))
    bDisplayDetails = (sTmp = "Yes")
    If bDisplayDetails Then
      With tblDetails
        Dim rowNew As New TableRow()
        Dim cellNew As New TableCell()
        cellNew.Text = Replace(m_pcl_ErrManager.ErrorDetails, vbCrLf, "<br/>")
        rowNew.Cells.Add(cellNew)
        .Rows.Add(rowNew)
      End With
    End If
    m_pcl_ErrManager.ClearError()
  End Sub


End Class