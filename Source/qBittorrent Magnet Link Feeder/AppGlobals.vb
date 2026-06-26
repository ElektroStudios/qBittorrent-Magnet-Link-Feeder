#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic

#End Region

Friend Module AppGlobals

    ''' <summary>
    ''' Win32 class name used by qBittorrent Qt window instances.
    ''' <para></para>
    ''' This value is used to identify and filter valid qBittorrent dialog windows.
    ''' </summary>
    Friend Const QT_WINDOW_CLASS As String = "Qt673QWindowIcon"

    ''' <summary>
    ''' Process name of the qBittorrent application without file extension.
    ''' <para></para>
    ''' Used to locate the running process instance in the system.
    ''' </summary>
    Friend Const QBITTORRENT_PROCESS_NAME As String = "qBittorrent"

    ''' <summary>
    ''' Window title prefix used by qBittorrent main window instances.
    ''' <para></para>
    ''' Used to distinguish the main application window from torrent dialog windows.
    ''' </summary>
    Friend Const QBITTORRENT_WINDOW_TITLE_PREFIX As String = "qBittorrent v"

    ''' <summary>
    ''' Prefix used to identify magnet URI links.
    ''' <para></para>
    ''' Used for parsing clipboard content and detecting torrent magnet entries.
    ''' </summary>
    Friend Const MAGET_LINK_PREFIX As String = "magnet:"

    ''' <summary>
    ''' Localized texts shown in qBittorrent dialogs when a torrent is already present.
    ''' <para></para>
    ''' This string is used to detect and automatically handle duplicate torrent dialogs.
    ''' </summary>
    Friend ReadOnly TORRENT_ALREADY_PRESENT_WINDOW_TITLES As New HashSet(Of String) From {
        "Torrent is already present",
        "El torrent ya está presente"
    }

End Module
