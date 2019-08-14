Imports System.Collections.Specialized
' Add references to Soap and Binary formatters. 
Imports System.Runtime.Serialization.Formatters.Binary
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization


<Serializable> Public Class eDictionary
  Inherits NameObjectCollectionBase
  Implements ISerializable

 

  ' The special constructor is used to deserialize values. 
  Public Sub New(info As SerializationInfo, context As StreamingContext)
    ' Reset the property value using the GetValue method.
    MyBase.New(info, context)
  End Sub

  Public Sub New()

  End Sub

  Public Sub Add(ByVal sKey As String, ByVal oObject As Object)
    ' Add an object object to the collection 
    ' using a given key.
    MyBase.BaseAdd(sKey, oObject)
  End Sub
  Public Sub Add(ByVal oObject As Object)
    ' Add an object object to the collection 
    ' using a given key.
    MyBase.BaseAdd("", oObject)
  End Sub

  Default Public Property Item(ByVal index As Integer) As Object
    ' Get or set an item by key.  
    Get
      Return CType(MyBase.BaseGet(index), Object)
    End Get
    Set(ByVal Value As Object)
      MyBase.BaseSet(index, Value)
    End Set
  End Property


  Default Public Property Item(ByVal sKey As String) As Object
    Get
      Return CType(MyBase.BaseGet(sKey), Object)
    End Get
    Set(ByVal Value As Object)
      MyBase.BaseSet(sKey, Value)
    End Set
  End Property
  Public ReadOnly Property AllKeys() As String()
    Get
      Return MyBase.BaseGetAllKeys()
    End Get
  End Property
  Public ReadOnly Property AllobjectValues() As Object()
    Get
      Return CType(MyBase.BaseGetAllValues((New Object()).GetType), Object())
    End Get
  End Property
  Public Sub Clear()
    ' Remove all items.
    If MyBase.Count > 0 Then
      Dim i As Int32
      For i = MyBase.Count - 1 To 0 Step -1
        If TypeOf MyBase.BaseGet(i) Is IDisposable Then
          Dim obj As IDisposable
          obj = CType(MyBase.BaseGet(i), IDisposable)
          obj.Dispose()
        Else
          Dim oOut As Object
          oOut = MyBase.BaseGet(i)
          oOut = Nothing
        End If
        MyBase.BaseRemoveAt(i)
      Next
      MyBase.BaseClear()
    End If
  End Sub
  Public Sub Remove(ByVal index As Integer)
    ' Remove an item by index.
    MyBase.BaseRemoveAt(index)
  End Sub
  Public Sub Remove(ByVal last_comma_first As String)
    ' Remove an item by key.
    MyBase.BaseRemove(last_comma_first)
  End Sub
  Public Function KeyExists(ByVal sKey As String) As Boolean
    Dim iKounter As Int32
    For iKounter = 0 To MyBase.Count - 1
      If LCase(sKey) = LCase(MyBase.BaseGetKey(iKounter)) Then
        Return True
      End If
    Next
    Return False
  End Function
  Public Sub ParseNameValues(ByVal sInput As String)
    Dim sVars() As String
    Dim i As Int32
    Dim sPair As String
    Dim sName As String
    Dim sValue As String

    sVars = sInput.Split(CChar(","))
    For i = 0 To sVars.GetUpperBound(0)
      sPair = sVars(i)
      If InStr(sPair, "=") <> 0 Then
        sName = Left(sPair, InStr(sPair, "=") - 1).Trim
        sValue = Mid(sPair, InStr(sPair, "=") + 1).Trim
        If sName.Length > 0 And sValue.Length > 0 Then
          'Add to dic!
          Add(sName, sValue)
        End If
      End If
    Next

  End Sub
  Public Function DumpData() As String
    Dim i As Int32
    Dim sRet As String = ""

    If MyBase.Count = 0 Then
      sRet = "No data in eDictionary."
    Else
      For i = 0 To MyBase.Count - 1
        sRet += MyBase.BaseGetKey(i) & ": " & vbTab
        sRet += CStr(MyBase.BaseGet(i))
        sRet += vbCrLf
      Next
    End If

    Return sRet
  End Function
End Class

