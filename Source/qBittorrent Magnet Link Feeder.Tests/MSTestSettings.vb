#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " MSTest Settings "

' Every test is self-contained (no shared mutable state, and each file system test uses its own temporary directory),
' so tests can safely run in parallel at method level.
<Assembly: Parallelize(Scope:=ExecutionScope.MethodLevel)>

#End Region
