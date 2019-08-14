<Serializable> Public Class BL_OpPermission
  Private m_iPermGroupID As Int32
  Private m_sPermGroupDesc As String
  Private m_iPermID As Int32
  Private m_sPermDesc As String
  Private m_iPermLevel As Int32
  Private m_bOpPermExists As Boolean
  Private m_iOriginalLevel As Int32
  Private m_sTimeStamp As String

  Public Property PermGroupID() As Int32
    Get
      PermGroupID = m_iPermGroupID
    End Get
    Set(ByVal Value As Int32)
      m_iPermGroupID = Value
    End Set
  End Property
  Public Property PermGroupDesc() As String
    Get
      PermGroupDesc = m_sPermGroupDesc
    End Get
    Set(ByVal Value As String)
      m_sPermGroupDesc = Value
    End Set
  End Property
  Public Property PermID() As Int32
    Get
      PermID = m_iPermID
    End Get
    Set(ByVal Value As Int32)
      m_iPermID = Value
    End Set
  End Property
  Public Property PermDesc() As String
    Get
      PermDesc = m_sPermDesc
    End Get
    Set(ByVal Value As String)
      m_sPermDesc = Value
    End Set
  End Property
  Public Property PermLevel() As Int32
    Get
      PermLevel = m_iPermLevel
    End Get
    Set(ByVal Value As Int32)
      m_iPermLevel = Value
    End Set
  End Property
  Public Property OpPermExists() As Boolean
    Get
      OpPermExists = m_bOpPermExists
    End Get
    Set(ByVal Value As Boolean)
      m_bOpPermExists = Value
    End Set
  End Property
  Public Property OriginalLevel() As Int32
    Get
      OriginalLevel = m_iOriginalLevel
    End Get
    Set(ByVal Value As Int32)
      m_iOriginalLevel = Value
    End Set
  End Property
  Public Property TimeStamp() As String
    Get
      TimeStamp = m_sTimeStamp
    End Get
    Set(ByVal Value As String)
      m_sTimeStamp = Value
    End Set
  End Property



End Class
