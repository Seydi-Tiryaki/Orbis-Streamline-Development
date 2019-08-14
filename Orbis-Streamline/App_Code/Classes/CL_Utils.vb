Imports System.Text.RegularExpressions
Imports System.Security.Cryptography
Imports System.IO
Imports System.Data
Imports System.Security



<Serializable> Public Class CL_Utils

  Dim m_pPage As Page
  Dim m_sTempP As String = ""
  Private m_key() As Byte = {} ' we are going to pass in the key portion in our method calls
  Private m_IV() As Byte = {&H12, &H34, &H56, &H78, &H90, &HAB, &HCD, &HEF}

  Public Enum enumTextCase
    tcTitle = 0
    tcSentance = 1
    tcLower = 2
    tcUpper = 3
  End Enum
  Public Enum enumDateFormat
    ddmmyyyy = 0
    yyyymmdd = 1
    mmddyyyy = 2
    yyyymmddn = 3
    paypal = 4
  End Enum

  Public Enum enumDateReturn
    Ok = 0
    InvalidInput = 1
    InvalidDate = 2
  End Enum
  Public Property pPage() As Page
    Get
      pPage = m_pPage
    End Get
    Set(ByVal Value As Page)
      m_pPage = Value
    End Set
  End Property
  Public Sub DisplayMessage(ByVal sText As String, Optional ByVal sSetFocusField As String = "", Optional ByVal bUseAjax As Boolean = False, Optional ByVal pPage As Control = Nothing)
    'Pass a message box back to the broswer, and setfocus if required

    Dim sScript As String = ""
    Dim sTemp As String
    If Not bUseAjax Then
      sScript = "<script language=JavaScript>"
    End If
    sTemp = sText.Replace(vbCrLf, "\n")
    sTemp = sTemp.Replace(Chr(34), "\" & Chr(34))
    sScript += "alert(""" & sTemp & """);"

    If Not sSetFocusField.Equals("") Then
      'If Not Parent Is Nothing Then
      '	'We have a parent...
      '	If InStr(Parent.GetType.Name, "_ascx") > 0 Then
      '		'We have a user control!
      '		Dim sName As String
      '		sName = Left(Parent.GetType.Name, InStrRev(Parent.GetType.Name, "_") - 1)
      '		sSetFocusField = sName & "1_" & sSetFocusField
      '	End If

      'End If

      'Pass the setfocus command
      sScript += "document.forms[0]." & sSetFocusField & ".focus();"
    End If
    If Not bUseAjax Then
      sScript += "</script>"
    End If

    'Only Put script if it 
    If Not bUseAjax Then
      If Not m_pPage.ClientScript.IsStartupScriptRegistered("clientScript") Then
        m_pPage.ClientScript.RegisterStartupScript(m_pPage.GetType, "clientScript", sScript)
      End If
    Else
      ScriptManager.RegisterStartupScript(pPage, pPage.GetType(), "MyScript", sScript, True)
    End If
  End Sub
  Public Sub SetFocusControl(ByVal sSetFocusField As String, Optional ByVal Parent As Object = Nothing)
    'setfocus to required control
    Dim sScript As String

    If Not sSetFocusField.Trim.Equals("") Then
      'Is it a usercontrol?
      If Not Parent Is Nothing Then
        'We have a parent...
        If InStr(Parent.GetType.Name, "_ascx") > 0 Then
          'We have a user control!
          Dim sName As String
          sName = Left(Parent.GetType.Name, InStrRev(Parent.GetType.Name, "_") - 1)
          sSetFocusField = sName & "1_" & sSetFocusField
        End If

      End If

      'Pass the setfocus command
      sScript = "<script language=JavaScript>"
      sScript += "document.forms[0]." & sSetFocusField & ".focus();"
      sScript += "</script>"

      'Only Put script if it is empty...
      If Not m_pPage.ClientScript.IsStartupScriptRegistered("clientScript") Then
        m_pPage.ClientScript.RegisterStartupScript(Me.GetType, "clientScript", sScript)
      End If
    End If

  End Sub
  Public Function EncryptStringOld(ByVal sR As String) As String
    Dim sTemp As String = ""
    Dim i As Int32
    Dim iKey As Int32

    sR = Trim(sR)
    If sR <> "" Then
      iKey = CInt(Int((9 * Rnd()) + 1))                       'Pick a random value between 1 and 9
      'lblStatus.Caption = "Encrypt: iKey=" & iKey       'Show what the Key value is going to be
      If Len(sR) < 4 Then                               'If len is less than 4
        i = 4 - Len(sR)                               'Find out how short the string is
        sR = sR & Space(i)                     'Pad with spaces
      End If
      For i = 1 To Len(sR)                              'Loop through each char in the given string
        If i = 3 Then                                 'If we are in the third space,
          sTemp = sTemp & CStr(iKey * 10)           'Then insert the key 2 digit value
        End If
        sTemp = sTemp & Hex(Asc(Mid$(sR, i, 1)) + iKey) 'Convert the char to asc, add the random value, then to convert to hex
      Next i
      EncryptStringOld = Trim$(sTemp)                     'Return the string
    Else
      EncryptStringOld = Trim$(sTemp)                     'Return the string
    End If
  End Function
  Public Function EncryptToBase64String(ByVal stringToEncrypt As String) As String
    zSetTemp()
    m_key = System.Text.Encoding.UTF8.GetBytes(Left(m_sTempP, 8))
    Dim des As New DESCryptoServiceProvider()
    ' convert our input string to a byte array
    Dim inputByteArray() As Byte = Encoding.UTF8.GetBytes(stringToEncrypt)
    'now encrypt the bytearray
    Dim ms As New MemoryStream()
    Dim cs As New CryptoStream(ms, des.CreateEncryptor(m_key, m_IV), CryptoStreamMode.Write)
    cs.Write(inputByteArray, 0, inputByteArray.Length)
    cs.FlushFinalBlock()
    ' now return the byte array as a "safe for XMLDOM" Base64 String
    Return Convert.ToBase64String(ms.ToArray())
  End Function
  Public Function DecryptStringOld(ByVal sR As String) As String
    Dim sTemp As String = ""
    Dim i As Integer, iKey As Integer

    sR = Trim(sR)

    If sR <> "" Then
      'On Error Resume Next                              'Trap for error
      iKey = CInt(Mid$(sR, 5, 1))                             'Extract the key value
      'On Error GoTo 0                                   'Turn off error trapping
      For i = 1 To Len(sR) Step 2                       'Loop through each char in the given string
        If i <> 5 Then                                 'If we are in the fifth place, then do nothing
          sTemp = sTemp & Chr(CInt(Val("&H" & Mid$(sR, i, 2) & "&")) - iKey)
          'sTemp = sTemp & Chr(Val("&H" & Mid$(sR, i, 2) & "&") - iKey)

        End If
      Next i
      DecryptStringOld = Trim$(sTemp)                     'Return the string
    Else
      DecryptStringOld = Trim$(sTemp)                     'Return the string
    End If
  End Function
  Public Function DecryptFromBase64String(ByVal stringToDecrypt As String) As String
    zSetTemp()
    Dim inputByteArray(stringToDecrypt.Length) As Byte
    ' Note: The DES CryptoService only accepts certain key byte lengths
    ' We are going to make things easy by insisting on an 8 byte legal key length

    m_key = System.Text.Encoding.UTF8.GetBytes(Left(m_sTempP, 8))
    Dim des As New DESCryptoServiceProvider()
    ' we have a base 64 encoded string so first must decode to regular unencoded (encrypted) string
    inputByteArray = Convert.FromBase64String(stringToDecrypt)
    ' now decrypt the regular string
    Dim ms As New MemoryStream()
    Dim cs As New CryptoStream(ms, des.CreateDecryptor(m_key, m_IV), CryptoStreamMode.Write)
    cs.Write(inputByteArray, 0, inputByteArray.Length)
    cs.FlushFinalBlock()
    Dim encoding As System.Text.Encoding = System.Text.Encoding.UTF8
    Return encoding.GetString(ms.ToArray())
  End Function
  Public Sub FieldEditMode(ByVal bEdit As Boolean, ByVal pControls As ControlCollection)
    'Set the control status....
    Dim pcontrol As Control
    For Each pcontrol In pControls
      Select Case pcontrol.GetType.Name
        Case "TextBox"
          Dim pText As TextBox
          pText = CType(pcontrol, TextBox)
          pText.ReadOnly = Not bEdit
          If bEdit Then
            'pText.BackColor = System.Drawing.SystemColors.Window
          Else
            pText.BackColor = System.Drawing.Color.GhostWhite
          End If
          pText = Nothing

        Case "DropDownList"
          Dim pDDList As DropDownList
          pDDList = CType(pcontrol, DropDownList)
          pDDList.Enabled = bEdit
          If bEdit Then
            'pDDList.BackColor = System.Drawing.SystemColors.Window
          Else
            pDDList.BackColor = System.Drawing.SystemColors.Control
          End If
          pDDList = Nothing

        Case "CheckBox"
          Dim pCheck As CheckBox
          pCheck = CType(pcontrol, CheckBox)
          pCheck.Enabled = bEdit
          If bEdit Then
            'pCheck.BackColor = System.Drawing.SystemColors.Window
          Else
            pCheck.BackColor = System.Drawing.SystemColors.Control
          End If
          pCheck = Nothing

        Case "RadioButton"
          Dim pRadio As RadioButton
          pRadio = CType(pcontrol, RadioButton)
          pRadio.Enabled = bEdit
          If bEdit Then
            'pRadio.BackColor = System.Drawing.SystemColors.Window
          Else
            pRadio.BackColor = System.Drawing.SystemColors.Control
          End If
          pRadio = Nothing

        Case "ListBox"
          Dim pList As ListBox
          pList = CType(pcontrol, ListBox)
          pList.Enabled = bEdit
          If bEdit Then
            'pList.BackColor = System.Drawing.SystemColors.Window
          Else
            pList.BackColor = System.Drawing.SystemColors.Control
          End If
          pList = Nothing
      End Select
    Next

  End Sub
  Public Function FormatBytes(ByVal num_bytes As Int32) As String
    Const ONE_KB As Double = 1024
    Const ONE_MB As Double = ONE_KB * 1024
    Const ONE_GB As Double = ONE_MB * 1024
    Const ONE_TB As Double = ONE_GB * 1024
    Const ONE_PB As Double = ONE_TB * 1024
    Const ONE_EB As Double = ONE_PB * 1024
    Const ONE_ZB As Double = ONE_EB * 1024
    Const ONE_YB As Double = ONE_ZB * 1024

    ' See how big the value is.
    If num_bytes <= 999 Then
      ' Format in bytes.
      Return Format$(num_bytes, "0") & " bytes"
    ElseIf num_bytes <= ONE_KB * 999 Then
      ' Format in KB.
      Return zThreeNonZeroDigits(num_bytes / ONE_KB) & " " & "KB"
    ElseIf num_bytes <= ONE_MB * 999 Then
      ' Format in MB.
      Return zThreeNonZeroDigits(num_bytes / ONE_MB) & " " & "MB"
    ElseIf num_bytes <= ONE_GB * 999 Then
      ' Format in GB.
      Return zThreeNonZeroDigits(num_bytes / ONE_GB) & " " & "GB"
    ElseIf num_bytes <= ONE_TB * 999 Then
      ' Format in TB.
      Return zThreeNonZeroDigits(num_bytes / ONE_TB) & " " & "TB"
    ElseIf num_bytes <= ONE_PB * 999 Then
      ' Format in PB.
      Return zThreeNonZeroDigits(num_bytes / ONE_PB) & " " & "PB"
    ElseIf num_bytes <= ONE_EB * 999 Then
      ' Format in EB.
      Return zThreeNonZeroDigits(num_bytes / ONE_EB) & " " & "EB"
    ElseIf num_bytes <= ONE_ZB * 999 Then
      ' Format in ZB.
      Return zThreeNonZeroDigits(num_bytes / ONE_ZB) & " " & "ZB"
    Else
      ' Format in YB.
      Return zThreeNonZeroDigits(num_bytes / ONE_YB) & " " & "YB"
    End If
  End Function
  Public Sub FlagReadOnlyControls(ByVal pControls As ControlCollection)
    'Set the control status....
    Dim pcontrol As Control
    Dim colLocked As System.Drawing.Color
    colLocked = System.Drawing.Color.Gainsboro
    For Each pcontrol In pControls
      If pcontrol.Controls.Count > 0 Then
        FlagReadOnlyControls(pcontrol.Controls)
      End If

      Select Case pcontrol.GetType.Name
        Case "TextBox"
          Dim pText As TextBox
          pText = CType(pcontrol, TextBox)
          If Not pText.ReadOnly Then
            'pText.BackColor = System.Drawing.SystemColors.Window
          Else
            pText.BackColor = colLocked
          End If
          pText = Nothing

        Case "DropDownList"
          Dim pDDList As DropDownList
          pDDList = CType(pcontrol, DropDownList)
          If pDDList.Enabled Then
            'pDDList.BackColor = System.Drawing.SystemColors.Window
          Else
            pDDList.BackColor = colLocked
          End If
          pDDList = Nothing

        Case "CheckBox"
          Dim pCheckBox As CheckBox
          pCheckBox = CType(pcontrol, CheckBox)
          If pCheckBox.Enabled Then
            'pCheckBox.BackColor = System.Drawing.SystemColors.Window
          Else
            pCheckBox.BackColor = colLocked
          End If
          pCheckBox = Nothing

        Case "RadioButton"
          Dim pRadio As RadioButton
          pRadio = CType(pcontrol, RadioButton)
          If pRadio.Enabled Then
            'pRadio.BackColor = System.Drawing.SystemColors.Window
          Else
            pRadio.BackColor = colLocked
          End If
          pRadio = Nothing

        Case "ListBox"
          Dim pListbox As ListBox
          pListbox = CType(pcontrol, ListBox)
          If pListbox.Enabled Then
            pListbox.BackColor = System.Drawing.SystemColors.Window
          Else
            pListbox.BackColor = colLocked
          End If
          pListbox = Nothing
      End Select
    Next

  End Sub
  Public Sub EnableDisableControls(ByVal pControls As ControlCollection, ByVal bEnabled As Boolean)
    'Set the control status....
    Dim pcontrol As Control
    Dim colLocked As System.Drawing.Color
    colLocked = System.Drawing.Color.Gainsboro
    For Each pcontrol In pControls
      If pcontrol.Controls.Count > 0 Then
        EnableDisableControls(pcontrol.Controls, bEnabled)
      End If

      Select Case pcontrol.GetType.Name
        Case "TextBox"
          Dim pText As TextBox
          pText = CType(pcontrol, TextBox)
          pText.Enabled = bEnabled
          pText = Nothing

        Case "DropDownList"
          Dim pDDList As DropDownList
          pDDList = CType(pcontrol, DropDownList)
          pDDList.Enabled = bEnabled
          pDDList = Nothing

        Case "CheckBox"
          Dim pCheckBox As CheckBox
          pCheckBox = CType(pcontrol, CheckBox)
          pCheckBox.Enabled = bEnabled
          pCheckBox = Nothing

        Case "RadioButton"
          Dim pRadio As RadioButton
          pRadio = CType(pcontrol, RadioButton)
          pRadio.Enabled = bEnabled
          pRadio = Nothing

        Case "ListBox"
          Dim pListbox As ListBox
          pListbox = CType(pcontrol, ListBox)
          pListbox.Enabled = bEnabled
          pListbox = Nothing
        Case "Button"
          Dim pButton As Button
          pButton = CType(pcontrol, Button)
          pButton.Enabled = bEnabled
          pButton = Nothing
      End Select
    Next

  End Sub

  Public Function Parse(ByVal sString As String) As String()
    '--------------------------------------------------------
    'function converts search string (like in Altavista) into
    'SQL query string for database search
    '
    'To make this work in your database, you need to
    'replace "table" and "field" with appropriate values
    '--------------------------------------------------------

    Dim iBlank As Integer       'first iBlank space position
    Dim iNextBlank As Integer   'Next iBlank space position (d)
    Dim iCount As Integer       'iCount variable
    Dim sFirstLeft As String    'first character following iBlank
    Dim sSecondLeft As String   'first character following sFirstLeft
    Dim sSQLStmt As String      'SQL statement
    Dim sWord As String         'each Word within string
    Dim sChars As String        'All chars. Used for error checking.
    Dim bAnyChars As Boolean    'Is there any alpha and num characters in sString
    'Used for error checking.

    Dim sWords(-1) As String
    Dim iKounter As Int32 = 0

    'Begin Error checking
    sChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890"

    bAnyChars = False
    For iCount = 1 To 36
      If InStr(1, UCase(sString), Mid(sChars, iCount, 1)) <> 0 Then
        bAnyChars = True
      End If
    Next iCount
    If Not bAnyChars Then
      Return sWords
    End If
    'End Error checking

    sString = Chr(32) & Trim(sString) & Chr(32)
    iCount = 0
    iBlank = 0
    sSQLStmt = "SELECT * FROM table WHERE"

    Do While InStr(sString, Chr(32)) <> 0
      iBlank = InStr(sString, Chr(32))

      If iBlank = 0 Then
        sFirstLeft = Mid(sString, iBlank, 1)
        sSecondLeft = Mid(sString, iBlank + 1, 1)
      Else
        sFirstLeft = Mid(sString, iBlank + 1, 1)
        sSecondLeft = Mid(sString, iBlank + 2, 1)
      End If

      iNextBlank = InStr(iBlank + 1, sString, Chr(32))

      If sFirstLeft = """" Then
        Dim iLen As Int32
        iLen = InStr(iBlank + 2, sString, Chr(34)) - 3
        If iLen = 0 Then
          iLen = sString.Length
        End If
        sWord = Mid(sString, InStr(iBlank, sString, Chr(34)) + 1, iLen).Trim
        iNextBlank = InStr(iBlank + 2, sString, Chr(34)) + 1
        If sWord <> "" Then
          iKounter += 1
          ReDim Preserve sWords(iKounter - 1)
          sWords(sWords.GetUpperBound(0)) = sWord
        End If
      Else
        If sSecondLeft = """" Then
          Dim iLen As Int32
          iLen = InStr(iBlank + 4, sString, Chr(34)) - 4
          If iLen = 0 Then
            iLen = sString.Length
          End If
          sWord = Chr(32) & Chr(32) & Mid(sString, InStr(iBlank, sString, Chr(34)) + 1, iLen).Trim
          If sWord <> "" Then
            iKounter += 1
            ReDim Preserve sWords(iKounter - 1)
            sWords(sWords.GetUpperBound(0)) = sWord
          End If
          iNextBlank = InStr(iBlank + 4, sString, Chr(34)) + 1
        Else
          sWord = Mid(sString, 1, InStr(iBlank + 2, sString, Chr(32))).Trim
          If sWord <> "" Then
            iKounter += 1
            ReDim Preserve sWords(iKounter - 1)
            sWords(sWords.GetUpperBound(0)) = sWord
          End If
        End If
      End If

      iCount = iCount + 1
      sString = Right(sString, Len(sString) - iNextBlank + 1)

      If sFirstLeft = "" Then
        Exit Do
      End If
    Loop

    Return sWords
  End Function
  Public Function DecodeURL(ByVal sRawData As String) As String
    Dim sNewString As String = ""
    Dim sTmp As String
    Dim i As Int64

    For i = 1 To sRawData.Length
      sTmp = Mid(sRawData, CInt(i), 1)
      If sTmp = "%" Then
        Dim sChar As String

        sChar = Mid(sRawData, CInt(i + 1), 2)
        sNewString += Chr(CInt("&H" & sChar))
        i += 2
      Else
        If sTmp <> "+" Then
          sNewString += sTmp
        Else
          sNewString += " "
        End If
      End If
    Next

    Return sNewString

  End Function
  Public Function EncodeURL(ByVal sRawData As String) As String
    Dim sNewString As String = ""
    Dim sTmp As String
    Dim i As Int64

    For i = 1 To sRawData.Length
      sTmp = Mid(sRawData, CInt(i), 1)
      Select Case Hex(Asc(sTmp))
        Case "24", "2B", "2C", "3B", "3F", "40", "5E", "7E", "5B", "5D", "60", _
            "22", "3C", "3E", "23", "25", "7B", "7D", "7C", "5C"
          sNewString += "%" & Hex(Asc(sTmp))
        Case "20"
          sNewString += "+"
        Case Else
          sNewString += sTmp

      End Select
    Next

    Return sNewString

  End Function
  Public Function EncodeString4URL(ByVal sRawData As String) As String
    Dim sNewString As String = ""
    Dim sTmp As String
    Dim i As Int64

    For i = 1 To sRawData.Length
      sTmp = Mid(sRawData, CInt(i), 1)
      Select Case Hex(Asc(sTmp))
        Case "24", "26", "2B", "2C", "3B", "3F", "40", "5E", "7E", "5B", "5D", "60", _
            "22", "3C", "3E", "23", "25", "7B", "7D", "7C", "5C"
          sNewString += "%" & Hex(Asc(sTmp))
        Case "20"
          sNewString += "+"
        Case Else
          sNewString += sTmp

      End Select
    Next

    Return sNewString

  End Function
  Public Function TextFormat(ByVal sInput As String, ByVal enCase As enumTextCase) As String

    Dim sret As String
    Dim sConvText As String = ""
    Dim sNextChar As String
    Dim bNewSentance As Boolean
    Dim bNewWord As Boolean
    Dim i As Integer

    Select Case enCase
      Case enumTextCase.tcTitle

        bNewWord = True
        sInput = LCase(sInput)
        For i = 1 To Len(sInput)

          sNextChar = Mid$(sInput, i, 1)
          Select Case sNextChar
            Case " ", ".", ",", "-"
              bNewWord = True
              sConvText = sConvText & sNextChar
            Case Else
              If bNewWord Then
                sConvText = sConvText & UCase(sNextChar)
                bNewWord = False
              Else
                sConvText = sConvText & sNextChar
              End If
          End Select
        Next
        sret = sConvText

      Case enumTextCase.tcSentance
        sInput = LCase(sInput)
        bNewSentance = True
        For i = 1 To Len(sInput)

          sNextChar = Mid$(sInput, i, 1)
          Select Case sNextChar
            Case "."
              bNewSentance = True
              sConvText = sConvText & sNextChar
            Case " "
              sConvText = sConvText & sNextChar
            Case Else
              If IsNumeric(sNextChar) Or bIsText(sNextChar) Then
                If bNewSentance Then
                  sConvText = sConvText & UCase(sNextChar)
                  bNewSentance = False
                Else
                  sConvText = sConvText & sNextChar
                End If
              Else
                ' special chars such as CR
                sConvText = sConvText & sNextChar
              End If
          End Select
        Next
        sret = sConvText

      Case enumTextCase.tcUpper
        sret = UCase(sInput)

      Case enumTextCase.tcLower
        sret = LCase(sInput)

      Case Else
        sret = sInput

    End Select

    TextFormat = sret

  End Function
  Public Function ValidateDate(ByVal sDate As String, ByVal enDateFormat As enumDateFormat) As enumDateReturn
    'OK Let's check this date!!!
    Dim sDD As String = ""
    Dim sMM As String = ""
    Dim sYYYY As String = ""
    Dim sNewDate As String
    Dim iPos1 As Int32
    Dim iPos2 As Int32
    Dim enRet As enumDateReturn = enumDateReturn.Ok
    Dim sMonths() As String = Regex.Split("Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec", ",")
    'Create a standard separator
    sNewDate = sDate.Replace("-", "/")
    sNewDate = sNewDate.Replace("\", "/")
    sNewDate = sNewDate.Replace(".", "/")
    sNewDate = sNewDate.Replace(" ", "/")

    'Parse the input date....
    Select Case enDateFormat
      Case enumDateFormat.ddmmyyyy
        'UK date format.....
        'Find the separators....
        iPos1 = InStr(sNewDate, "/")
        iPos2 = InStrRev(sNewDate, "/")
        If iPos1 <> iPos2 Then
          'The separators are not the same! 
          'Chop it up....
          sDD = Left(sNewDate, iPos1 - 1)
          sMM = Mid(sNewDate, iPos1 + 1, iPos2 - iPos1 - 1)
          sYYYY = Mid(sNewDate, iPos2 + 1)
        Else
          enRet = enumDateReturn.InvalidInput
        End If


    End Select
    Dim iMM As Int32
    iMM = CInt(sMM)
    If iMM < 1 Or iMM > 12 Then
      enRet = enumDateReturn.InvalidDate
    End If
    If enRet = enumDateReturn.Ok Then
      'Construct a date!!!
      If IsDate(sDD & " " & sMonths(CInt(sMM) - 1) & sYYYY) Then
        enRet = enumDateReturn.Ok
      Else
        enRet = enumDateReturn.InvalidDate

      End If

    End If
    Return enRet

  End Function
  Public Function ConvertToDate(ByVal sDate As String, ByVal enDateFormat As enumDateFormat) As Date
    'OK Let's check this date!!!
    Dim sDD As String = ""
    Dim sMM As String = ""
    Dim sYYYY As String = ""
    Dim sNewDate As String
    Dim iPos1 As Int32
    Dim iPos2 As Int32
    Dim dtRet As Date = #1/1/1900#
    Dim sTime As String = ""
    Dim sMonths() As String = Regex.Split("Jan,Feb,Mar,Apr,May,Jun,Jul,Aug,Sep,Oct,Nov,Dec", ",")
    'Create a standard separator
    sNewDate = sDate.Replace("-", "/")
    sNewDate = sNewDate.Replace("\", "/")
    sNewDate = sNewDate.Replace(".", "/")
    sNewDate = sNewDate.Replace(" ", "/")

    'Parse the input date....
    Select Case enDateFormat
      Case enumDateFormat.ddmmyyyy
        'UK date format.....
        'Find the separators....
        iPos1 = InStr(sNewDate, "/")
        iPos2 = InStrRev(sNewDate, "/")
        If iPos1 <> iPos2 Then
          'The separators are not the same! 
          'Chop it up....
          sDD = Left(sNewDate, iPos1 - 1)
          sMM = Mid(sNewDate, iPos1 + 1, iPos2 - iPos1 - 1)
          sYYYY = Mid(sNewDate, iPos2 + 1)
        Else
          Return Nothing
          Exit Function
        End If

      Case enumDateFormat.yyyymmddn
        'Reverse order, no separators....
        sDD = Right(sNewDate, 2)
        sMM = Mid(sNewDate, 5, 2)
        sYYYY = Left(sNewDate, 4)

      Case enumDateFormat.paypal
        'Damn Paypal, american, arse about tit, piece of crap date formatting...
        'e.g.: 09:29:24 Nov 03, 2016 PDT
        sTime = sNewDate.Substring(0, 8)
        sMM = sNewDate.Substring(9, 3)
        sDD = sNewDate.Substring(13, 2)
        sYYYY = sNewDate.Substring(17, 4)

      Case Else
        Throw New Exception("pcl_Utils ConvertToDate - Invalid date enum: " & enDateFormat.ToString)

    End Select
    Dim iMM As Int32
    If IsNumeric(sMM) Then
      iMM = CInt(sMM)
      If iMM < 1 Or iMM > 12 Then
        Return Nothing
        Exit Function
      End If
      sMM = sMonths(CInt(sMM) - 1)

    End If
    Dim sConvDate As String
    sConvDate = sDD & " " & sMM & " " & sYYYY
    If sTime <> "" Then
      sConvDate += " " & sTime
    End If

    'Construct a date!!!
    If IsDate(sConvDate) Then
      dtRet = CDate(sConvDate)
    Else
      Return Nothing
      Exit Function
    End If

    Return dtRet

  End Function
  Public Function FormatTextForUrl(ByVal sInput As String) As String
    Try
      'remove any chars that may bugger up the URL!
      Dim sret As String
      sret = Replace(sInput, " ", "_")
      sret = Replace(sret, Chr(34), "")
      sret = Replace(sret, "'", "")
      sret = Replace(sret, ";", "")
      sret = Replace(sret, ",", "")
      sret = Replace(sret, "?", "")
      sret = Replace(sret, "&", "")
      sret = Replace(sret, "/", "")
      sret = Replace(sret, "\", "")
      sret = Replace(sret, ":", "")
      sret = Replace(sret, "%", "")
      sret = Replace(sret, "$", "")
      sret = Replace(sret, "£", "")
      sret = Replace(sret, "!", "")
      sret = Replace(sret, "(", "")
      sret = Replace(sret, ")", "")
      sret = Replace(sret, "{", "")
      sret = Replace(sret, "}", "")
      sret = Replace(sret, "@", "")
      sret = Replace(sret, "#", "")
      sret = Replace(sret, "~", "")
      sret = Replace(sret, "¦", "")
      sret = Replace(sret, "|", "")
      sret = Replace(sret, "=", "")
      sret = Replace(sret, "+", "")

      Return sret

    Catch ex As Exception
      Throw ex
    End Try
  End Function
  Public Function RoundUp(ByVal X As Decimal, Optional ByVal iPlaces As Decimal = 2) As Decimal
    Try
      Dim dTemp As Decimal
      Dim dNudge As Decimal
      dTemp = CDec(10 ^ iPlaces)
      dNudge = CDec(1 / (dTemp * 100)) 'adjust the value by a very small amount...
      X += dNudge
      RoundUp = CDec(CInt((X) * dTemp) / dTemp)
    Catch ex As Exception
      Dim exNew As New Exception("Error in RoundUp, x=" & X & ", DP:" & iPlaces & vbCrLf & ex.Message)
      Throw exNew

    End Try
  End Function

  Public Function FindControl(ByVal ControlName As String, ByVal CurrentControl As Control) As Control
    Dim ctr As Control
    For Each ctr In CurrentControl.Controls
      If ctr.ID = ControlName Then
        Return ctr
      Else
        ctr = FindControl(ControlName, ctr)
        If Not ctr Is Nothing Then
          Return ctr
        End If
      End If
    Next ctr
    Return Nothing
  End Function

  Public Function DropDownSelectByValue(ByRef pDDL As DropDownList, ByVal sValue As String) As Boolean
    'Sometime finditembyvalue.selected doesn't work, it should but it doesn't.... :(
    Dim iItem As Int32
    Dim bRet As Boolean = False
    pDDL.ClearSelection()
    For iItem = 0 To pDDL.Items.Count - 1
      If Trim(pDDL.Items(iItem).Value) = Trim(sValue) Then
        pDDL.SelectedIndex = iItem
        bRet = True
        Exit For
      End If
    Next

    Return bRet
  End Function
  Public Function DropDownSelectByText(pDDL As DropDownList, ByVal sValue As String) As Boolean
    'Sometime finditembyvalue.selected doesn't work, it should but it doesn't.... :(
    Dim iItem As Int32
    Dim bRet As Boolean = False
    pDDL.ClearSelection()
    For iItem = 0 To pDDL.Items.Count - 1
      If Trim(pDDL.Items(iItem).Text) = Trim(sValue) Then
        pDDL.SelectedIndex = iItem
        bRet = True
        Exit For
      End If
    Next

    Return bRet
  End Function
  Public Function CheckboxListSelectByText(ByRef pCheckBoxList As CheckBoxList, ByVal sValue As String) As Boolean
    'Sometime finditembyvalue.selected doesn't work, it should but it doesn't.... :(
    Dim iItem As Int32
    Dim bRet As Boolean = False

    For iItem = 0 To pCheckBoxList.Items.Count - 1
      If Trim(pCheckBoxList.Items(iItem).Text) = Trim(sValue) Then
        pCheckBoxList.Items(iItem).Selected = True
        bRet = True
        Exit For
      End If
    Next

    Return bRet
  End Function


  Public Sub CloseBroswerWindow()
    Dim strscript As String = "<script language=javascript>window.top.close();</script>"
    'Dim strscript As String = "<script language=javascript>window.opener='x';window.close()</script>"
    If (Not m_pPage.ClientScript.IsStartupScriptRegistered("clientScript")) Then
      m_pPage.ClientScript.RegisterStartupScript(m_pPage.GetType(), "clientScript", strscript)
    End If
  End Sub

  Public Function StripString4JavaScript(ByVal sValue As String) As String
    sValue = Replace(sValue, "'", "`")
    sValue = Replace(sValue, Chr(34), "``")
    Return sValue
  End Function
  Public Function Strip2Alphanumeric(ByVal sInput As String, ByVal sReplacementChar As String, Optional ByVal bAllowNumeric As Boolean = True, Optional ByVal bAllowSpace As Boolean = False, _
                                     Optional ByVal bAllowComma As Boolean = False, Optional ByVal bLcase As Boolean = False, Optional ByVal bAllowDash As Boolean = False, _
                                     Optional ByVal bAllowDot As Boolean = False, Optional ByVal bAllowUnderscore As Boolean = False, Optional ByVal bAllowBrackets As Boolean = False, _
                                     Optional ByVal bAllowTilda As Boolean = False, Optional ByVal bSwapAmpersandWithPlus As Boolean = False, Optional ByVal bAllowAtSymbol As Boolean = False, _
                                     Optional ByVal bAllowPlus As Boolean = False) As String
    Dim iPos As Int32
    Dim sOut As String = ""
    Dim bValid As Boolean
    Dim sValidChars As String
    Dim sChar As String

    sValidChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz"
    If bAllowNumeric Then
      sValidChars += "0123456789"
    End If
    If bAllowSpace Then
      sValidChars += " "
    End If
    If bAllowComma Then
      sValidChars += ","
    End If
    If bAllowDash Then
      sValidChars += "-"
    End If
    If bAllowDot Then
      sValidChars += "."
    End If
    If bAllowUnderscore Then
      sValidChars += "_"
    End If
    If bAllowBrackets Then
      sValidChars += "()[]{}"
    End If
    If bAllowTilda Then
      sValidChars += "~"
    End If
    If bAllowAtSymbol Then
      sValidChars += "@"
    End If
    If bSwapAmpersandWithPlus Then
      sValidChars += "+"
    End If
    If bAllowPlus Then
      sValidChars += "+"
    End If

    If InStr(sInput, "&") > 0 Then
      sInput += ""
    End If


    If bSwapAmpersandWithPlus Then
      sInput = Replace(sInput, "&", "+")
    End If

    iPos = 1
    While iPos <= Len(sInput)
      sChar = Mid(sInput, iPos, 1)
      If InStr(sValidChars, sChar) > 0 Then
        bValid = True
      Else
        bValid = False
      End If

      If bValid Then
        If bLcase Then
          sOut += LCase(Mid(sInput, iPos, 1))
        Else
          sOut += Mid(sInput, iPos, 1)
        End If
      Else
        sOut += sReplacementChar
      End If
      iPos = iPos + 1
    End While
    Return sOut
  End Function
  'Public Function ByteToString(ByVal rowVersion As Byte()) As String
  '  Return Convert.ToBase64String(rowVersion)
  'End Function
  'Public Function ByteToString(ByVal rowVersion As Object) As String
  '  Dim tmp() As Byte
  '  tmp = CType(rowVersion, Byte())
  '  Return Convert.ToBase64String(tmp)
  'End Function
  'Public Function StringToByte(ByVal sRowVersion As String) As Byte()
  '  Return Convert.FromBase64String(sRowVersion)
  'End Function
  Public Sub ScrollIntoView(ByVal sSetFocusField As String, Optional ByVal Parent As Object = Nothing)
    'setfocus to required control
    Dim sScript As String

    If Not sSetFocusField.Trim.Equals("") Then
      'Is it a usercontrol?
      If Not Parent Is Nothing Then
        'We have a parent...
        If InStr(Parent.GetType.Name, "_ascx") > 0 Then
          'We have a user control!
          Dim sName As String
          sName = Left(Parent.GetType.Name, InStrRev(Parent.GetType.Name, "_") - 1)
          sSetFocusField = sName & "1_" & sSetFocusField
        End If

      End If

      'Pass the setfocus command
      sScript = "<script language=JavaScript>"
      sScript += "document.getElementById." & sSetFocusField & ".scrollIntoView(true);"
      sScript += "</script>"

      'Only Put script if it is empty...
      If Not m_pPage.ClientScript.IsStartupScriptRegistered("clientScript") Then
        m_pPage.ClientScript.RegisterStartupScript(Me.GetType, "clientScript", sScript)
      End If
    End If

  End Sub
  Public Function FindTreeNodeByValue(ByVal ctlTree As TreeView, ByVal sValue As String) As TreeNode
    Dim i As Int32
    Dim tnRet As TreeNode = Nothing

    For i = 0 To ctlTree.Nodes.Count - 1
      tnRet = zFindNodeByValue(ctlTree.Nodes(i), sValue)
      If Not tnRet Is Nothing Then
        Exit For
      End If
    Next
    Return tnRet

  End Function
  Public Function FindTreeNodeByText(ByVal ctlTree As TreeView, ByVal sValue As String) As TreeNode
    Dim i As Int32
    Dim tnRet As TreeNode = Nothing

    For i = 0 To ctlTree.Nodes.Count - 1
      tnRet = zFindNodeByText(ctlTree.Nodes(i), sValue)
      If Not tnRet Is Nothing Then
        Exit For
      End If
    Next
    Return tnRet

  End Function
  Public Function DumpEdic(dicDump As eDictionary) As String
    Dim sRet As String = ""
    Dim i As Int32

    If Not dicDump Is Nothing Then
      If dicDump.Count > 0 Then
        For i = 0 To dicDump.Count - 1
          sRet += dicDump.Keys(i) & ": " & CStr(dicDump.Item(i))
          sRet += vbCrLf
        Next
      Else
        sRet = "No Parms"
      End If
    Else
      sRet = "Null Parms"
    End If

    Return sRet
  End Function
  Public Sub DeleteFilesOnWildcard(sfolder As String, sWild As String)
    Dim sPath As String

    sPath = Trim(sfolder)
    If Right(sPath, 1) <> "\" Then
      sPath += "\"
    End If

    Dim diPath As New DirectoryInfo(sPath)
    If diPath.Exists Then
      Dim fiFiles() As FileInfo

      fiFiles = diPath.GetFiles(sWild)
      For Each fiFile In fiFiles
        fiFile.Delete()
      Next
    End If
  End Sub

  Public Function CalcNetFromGross(ByVal dSalesPrice As Decimal, ByVal dVatRate As Decimal) As Decimal
    Dim dVatCalc As Decimal
    Dim dNet As Decimal

    dVatCalc = 1 + (dVatRate / 100)
    dNet = Decimal.Round(dSalesPrice / dVatCalc, 2)

    Return dNet

  End Function

  Public Function DumpRowToText(ByVal drInput As DataRow) As String
    Dim i As Int32
    Dim sRet As String = "No data in datarow."

    If Not drInput Is Nothing Then
      sRet = ""
      For i = 0 To drInput.ItemArray.Count - 1
        sRet += drInput.Table.Columns(i).ColumnName & ": " & vbTab
        If Not IsDBNull(drInput.ItemArray(i)) Then
          sRet += CStr(drInput.ItemArray(i))
        Else
          sRet += "NULL"
        End If
        sRet += vbCrLf
      Next
    End If

    Return sRet
  End Function
  Public Function FindControlRecursively(ByVal parentControl As System.Web.UI.Control, ByVal controlId As String) As System.Web.UI.Control

    If String.IsNullOrEmpty(controlId) = True OrElse controlId = String.Empty Then
      Return Nothing
    End If

    Dim ctlType As Type = parentControl.GetType()
    'Diagnostics.Debug.WriteLine("FindControlRecursively: Parent ID: " & LCase(parentControl.ID) & ", Type: " & ctlType.Name & ", Search Control: " & controlId)

    If LCase(parentControl.ID) = LCase(controlId) Then
      Return parentControl
    End If


    If parentControl.HasControls Then
      For Each c As System.Web.UI.Control In parentControl.Controls
        If c.Controls.Count > 0 Then
          Dim child As System.Web.UI.Control = FindControlRecursively(c, controlId)
          If child IsNot Nothing Then
            Return child
          End If
        Else
          Dim ctlType2 As Type = c.GetType()
          'Diagnostics.Debug.WriteLine("Control ID: " & LCase(c.ID) & ", Type: " & ctlType2.Name & ", Search Control: " & controlId)
          If LCase(c.ID) = LCase(controlId) Then
            Return c
          End If
        End If
      Next
    End If

    Return Nothing

  End Function
  Public Function FormatStringForCsv(ByVal sInput As String) As String
    Try
      'remove any chars that may bugger up the output...
      Dim sret As String = sInput
      sret = Replace(sret, Chr(34), "''") 'Swap out the double quote for two singles!!

      Return sret

    Catch ex As Exception
      Throw ex
    End Try
  End Function
  Public Function GenerateDicFromRow(drIn As DataRow, dtIn As DataTable, bIgnoreNulls As Boolean) As eDictionary
    Dim iCol As Int32
    Dim dicOut As New eDictionary

    For iCol = 0 To drIn.ItemArray.Count - 1
      If (IsDBNull(drIn(iCol)) And bIgnoreNulls = False) Or (Not IsDBNull(drIn(iCol))) Then
        Select Case dtIn.Columns(iCol).DataType.FullName
          Case "System.Int16", "System.Int32", "System.Int64", "System.Decimal", "System.Long", "System.SByte"
            'Numeric
            dicOut.Add(dtIn.Columns(iCol).ColumnName, "N:" & CStr(drIn(iCol)))

          Case "System.DateTime"
            'Date Time
            dicOut.Add(dtIn.Columns(iCol).ColumnName, "D:" & CStr(drIn(iCol)))

          Case "System.String"
            'String
            dicOut.Add(dtIn.Columns(iCol).ColumnName, "S:" & CStr(drIn(iCol)))

          Case "System.Boolean"
            'String
            If CBool(drIn(iCol)) Then
              dicOut.Add(dtIn.Columns(iCol).ColumnName, "N:" & "-1")
            Else
              dicOut.Add(dtIn.Columns(iCol).ColumnName, "N:" & "0")
            End If


          Case Else
            Diagnostics.Debug.WriteLine("GenerateDicFromRow, unknown datatype: " & dtIn.Columns(iCol).DataType.FullName)


        End Select

      End If
    Next


    Return dicOut


  End Function

  Public Function CheckWordMinLength(sInput As String, iMinLength As Int32, sAllowedShortWords As String) As Boolean
    Dim sWords() As String
    Dim bret As Boolean = True
    Dim sAllowed(-1) As String
    Dim iAllow As Int32
    Dim bAllow As Boolean

    If Not sAllowedShortWords Is Nothing Then
      sAllowed = LCase(sAllowedShortWords).Split(",".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)
    End If

    sWords = sInput.Split(" ".ToCharArray(), System.StringSplitOptions.RemoveEmptyEntries)
    If sWords.Length > 0 Then
      For iWord = 0 To sWords.Length - 1
        If Len(sWords(iWord)) < iMinLength Then
          If sAllowed.Length = 0 Then
            bret = False
            Exit For
          Else
            bAllow = False
            For iAllow = 0 To sAllowed.Length - 1
              If LCase(sWords(iWord)) = LCase(sAllowed(iAllow)) Then
                bAllow = True
              End If
            Next

            If bAllow = False Then
              bret = False
              Exit For
            End If

          End If
        End If
      Next
    End If

    Return bret

  End Function
  Public Function ReplaceNoCase(ByVal originalString As String, ByVal oldValue As String, newValue As String, ByVal comparisonType As StringComparison) As String
    If String.IsNullOrEmpty(originalString) = False AndAlso String.IsNullOrEmpty(oldValue) = False AndAlso IsNothing(newValue) = False Then
      Dim startIndex As Int32

      Do While True
        startIndex = originalString.IndexOf(oldValue, startIndex, comparisonType)
        If startIndex = -1 Then Exit Do
        originalString = originalString.Substring(0, startIndex) & newValue & originalString.Substring(startIndex + oldValue.Length)
        startIndex += newValue.Length
      Loop
    End If

    Return originalString
  End Function


  Public Function ValidIMEI(ByVal CardNumber As String) As Boolean

    Dim intCount As Integer
    Dim intValue As Integer
    Dim intArr() As Integer
    Dim intStart As Integer
    Dim intArrValue As Integer
    Dim bRet As Boolean


    bRet = False

    If IsNumeric(CardNumber) Then

      ReDim intArr(Len(CardNumber))
      For intCount = Len(CardNumber) - 1 To 1 Step -2
        intValue = Mid(CardNumber, intCount, 1) * 2
        intArr(intCount) = intValue
      Next intCount

      intValue = 0

      If Len(CardNumber) Mod 2 = 0 Then
        intStart = 2
      Else
        intStart = 1
      End If

      For intCount = intStart To Len(CardNumber) Step 2
        intValue = intValue + Mid(CardNumber, intCount, 1)
        intArrValue = intArr(intCount - 1)
        If intArrValue < 10 Then
          intValue = intValue + intArrValue
        Else
          intValue = intValue + Left(intArrValue, 1) +
              Right(intArrValue, 1)
        End If
      Next intCount

    End If

    If intValue Mod 10 <> 0 Then
      bRet = False
    Else
      bRet = True
    End If

    Return bRet

  End Function



#Region "Private Routines"
  Private Function zThreeNonZeroDigits(ByVal value As Double) As String
    If value >= 100 Then
      ' No digits after the decimal.
      Return Format$(CInt(value))
    ElseIf value >= 10 Then
      ' One digit after the decimal.
      Return Format$(value, "0.0")
    Else
      ' Two digits after the decimal.
      Return Format$(value, "0.00")
    End If
  End Function
  Private Function bIsText(ByVal sChar As String) As Boolean
    Dim iAscvalue As Integer
    iAscvalue = Asc(sChar)
    Select Case iAscvalue
      Case 65 To 90
        bIsText = True
      Case 97 To 122
        bIsText = True
      Case Else
        bIsText = False
    End Select
  End Function
  Private Sub zSetTemp()
    If m_sTempP = "" Then
      m_sTempP = Mid(CStr(System.Configuration.ConfigurationManager.AppSettings("Temp")), 15, 8)
    End If
  End Sub
  Private Sub TraverseDirectory(ByVal di As DirectoryInfo)
    For Each diChild As DirectoryInfo In di.GetDirectories()
      TraverseDirectory(diChild)
    Next

    CleanAllFilesInDirectory(di)

    If di.GetFiles().Count = 0 Then
      di.Delete()
    End If
  End Sub
  Private Sub CleanAllFilesInDirectory(ByVal DirectoryToClean As DirectoryInfo)
    For Each fi As FileInfo In DirectoryToClean.GetFiles()
      'Read only files can not be deleted, so mark the attribute as 'IsReadOnly = False'
      If fi.IsReadOnly Then
        fi.IsReadOnly = False
      End If
      fi.Delete()
    Next
    System.Threading.Thread.Sleep(50) '50 millisecond stall (0.05 Seconds)
  End Sub
  Private Function zFindNodeByValue(ByVal tnNode As TreeNode, ByVal sValue As String) As TreeNode
    If tnNode.Value = sValue Then
      Return tnNode
    Else
      Dim i As Int32
      Dim tnRet As TreeNode = Nothing
      For i = 0 To tnNode.ChildNodes.Count - 1
        tnRet = zFindNodeByValue(tnNode.ChildNodes(i), sValue)
        If Not tnRet Is Nothing Then
          Exit For
        End If
      Next

      Return tnRet
    End If
  End Function
  Private Function zFindNodeByText(ByVal tnNode As TreeNode, ByVal sValue As String) As TreeNode
    If tnNode.Text = sValue Then
      Return tnNode
    Else
      Dim i As Int32
      Dim tnRet As TreeNode = Nothing
      For i = 0 To tnNode.ChildNodes.Count - 1
        tnRet = zFindNodeByText(tnNode.ChildNodes(i), sValue)
        If Not tnRet Is Nothing Then
          Exit For
        End If
      Next

      Return tnRet
    End If
  End Function

  Function ConvertToSecureString(ByVal str As String) As securestring
    Dim password As New SecureString
    For Each c As Char In str.ToCharArray
      password.AppendChar(c)
    Next
    Return password
  End Function


#End Region

End Class

