#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

''' <summary>
''' Provides Win32 API constants and delegates.
''' </summary>
Friend Module Win32

    Friend Delegate Function EnumWindowsProc(hWnd As IntPtr, lParam As IntPtr) As Boolean

    Friend Const VK_CAPITAL As Byte = &H14
    Friend Const KEYEVENTF_KEYUP As Integer = &H2

    Friend Const SW_MINIMIZE As Integer = 6
    Friend Const SW_RESTORE As Integer = 9

    Friend Const WM_NULL As UInteger = &H0UI
    Friend Const SMTO_BLOCK As UInteger = &H1UI

End Module
