#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " AppGlobals Tests "

''' <summary>
''' Unit tests for the <see cref="AppGlobals"/> module.
''' <para></para>
''' These tests guard the values the UI automation relies on to detect qBittorrent windows and magnet links.
''' </summary>
<TestClass>
Public NotInheritable Class AppGlobalsTests

    ''' <summary>
    ''' Verifies that the magnet link prefix is the magnet URI scheme followed by a colon.
    ''' </summary>
    <TestMethod>
    Public Sub MagnetLinkPrefix_IsMagnetUriScheme()

        Assert.AreEqual("magnet:", AppGlobals.MAGNET_LINK_PREFIX)
    End Sub

    ''' <summary>
    ''' Verifies that the qBittorrent process name does not include the file extension,
    ''' as required by <see cref="Process.GetProcessesByName(String)"/>.
    ''' </summary>
    <TestMethod>
    Public Sub QBittorrentProcessName_HasNoFileExtension()

        Assert.AreEqual("qBittorrent", AppGlobals.QBITTORRENT_PROCESS_NAME)
        Assert.DoesNotEndWith(".exe", AppGlobals.QBITTORRENT_PROCESS_NAME, StringComparison.OrdinalIgnoreCase)
    End Sub

    ''' <summary>
    ''' Verifies that the main window title prefix starts with the qBittorrent process name.
    ''' </summary>
    <TestMethod>
    Public Sub QBittorrentWindowTitlePrefix_StartsWithProcessName()

        Assert.StartsWith(AppGlobals.QBITTORRENT_PROCESS_NAME, AppGlobals.QBITTORRENT_WINDOW_TITLE_PREFIX, StringComparison.Ordinal)
    End Sub

    ''' <summary>
    ''' Verifies that the Qt window class name is not empty.
    ''' </summary>
    <TestMethod>
    Public Sub QtWindowClass_IsNotEmpty()

        Assert.IsFalse(String.IsNullOrWhiteSpace(AppGlobals.QT_WINDOW_CLASS))
        Assert.StartsWith("Qt", AppGlobals.QT_WINDOW_CLASS, StringComparison.Ordinal)
    End Sub

    ''' <summary>
    ''' Verifies that the "Torrent is already present" dialog titles include the supported localizations.
    ''' </summary>
    '''
    ''' <param name="localizedTitle">
    ''' The localized dialog title.
    ''' </param>
    <TestMethod>
    <DataRow("Torrent is already present")>
    <DataRow("El torrent ya está presente")>
    Public Sub TorrentAlreadyPresentWindowTitles_ContainsLocalizedTitle(localizedTitle As String)

        Assert.Contains(localizedTitle, AppGlobals.TORRENT_ALREADY_PRESENT_WINDOW_TITLES)
    End Sub

    ''' <summary>
    ''' Verifies that the "Torrent is already present" dialog titles contain no empty entries,
    ''' because an empty title would match any window title.
    ''' </summary>
    <TestMethod>
    Public Sub TorrentAlreadyPresentWindowTitles_HasNoEmptyEntries()

        For Each title As String In AppGlobals.TORRENT_ALREADY_PRESENT_WINDOW_TITLES
            Assert.IsFalse(String.IsNullOrWhiteSpace(title))
        Next title
    End Sub

End Class

#End Region
