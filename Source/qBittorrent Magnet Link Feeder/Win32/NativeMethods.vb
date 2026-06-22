
#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Text

#End Region

''' <summary>
''' Platform Invocation methods (P/Invoke), access unmanaged code.
''' </summary>
<SuppressUnmanagedCodeSecurity>
Friend Module NativeMethods

#Region " kernel32.dll "

    <DllImport("kernel32.dll")>
    Friend Function GetCurrentThreadId() As Integer
    End Function

#End Region

#Region " shlwapi.dll "

    <DllImport("shlwapi.dll", SetLastError:=False, CharSet:=CharSet.Unicode, ExactSpelling:=True)>
    Friend Function StrCmpLogicalW(first As String, second As String) As Integer
    End Function

#End Region

#Region " user32.dll "

    <DllImport("user32.dll")>
    Friend Function GetKeyState(nVirtKey As Integer) As Short
    End Function

    <DllImport("user32.dll")>
    Friend Sub keybd_event(bVk As Byte, bScan As Byte, dwFlags As Integer, dwExtraInfo As Integer)
    End Sub

    <DllImport("user32.dll", SetLastError:=True)>
    Friend Function BlockInput(blockIt As Boolean) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
    Friend Function GetWindowText(hWnd As IntPtr, lpString As StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Friend Function FindWindow(lpClassName As String, lpWindowName As String) As IntPtr
    End Function

    <DllImport("user32.dll", SetLastError:=True)>
    Friend Function GetWindowThreadProcessId(hWnd As IntPtr, <Out> ByRef lpdwProcessId As Integer) As Integer
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Friend Function EnumWindows(lpEnumFunc As EnumWindowsProc, lParam As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", CharSet:=CharSet.Auto)>
    Friend Function GetClassName(hWnd As IntPtr, lpClassName As System.Text.StringBuilder, nMaxCount As Integer) As Integer
    End Function

    <DllImport("user32.dll")>
    Friend Function IsIconic(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function BringWindowToTop(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function AttachThreadInput(idAttach As Integer, idAttachTo As Integer, fAttach As Boolean) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function IsWindowVisible(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function ShowWindow(hWnd As IntPtr, nCmdShow As Integer) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function SetForegroundWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll")>
    Friend Function GetForegroundWindow() As IntPtr
    End Function

    <DllImport("user32.dll")>
    Friend Function UpdateWindow(hWnd As IntPtr) As Boolean
    End Function

    <DllImport("user32.dll", SetLastError:=True, CharSet:=CharSet.Auto)>
    Friend Function SendMessageTimeout(
            hWnd As IntPtr,
            Msg As UInteger,
            wParam As IntPtr,
            lParam As IntPtr,
            fuFlags As UInteger,
            uTimeout As UInteger,
            ByRef lpdwResult As IntPtr
        ) As IntPtr
    End Function
#End Region

End Module
