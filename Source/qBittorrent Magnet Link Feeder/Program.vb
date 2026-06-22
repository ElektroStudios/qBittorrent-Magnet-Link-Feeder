#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

Imports DevCase.Runtime.TypeComparers

#End Region

Public Module Program

#Region " Fields "

    ''' <summary>
    ''' Win32 class name used by qBittorrent Qt window instances.
    ''' <para></para>
    ''' This value is used to identify and filter valid qBittorrent dialog windows.
    ''' </summary>
    Private Const QT_WINDOW_CLASS As String = "Qt673QWindowIcon"

    ''' <summary>
    ''' Process name of the qBittorrent application without file extension.
    ''' <para></para>
    ''' Used to locate the running process instance in the system.
    ''' </summary>
    Private Const QBITTORRENT_PROCESS_NAME As String = "qBittorrent"

    ''' <summary>
    ''' Window title prefix used by qBittorrent main window instances.
    ''' <para></para>
    ''' Used to distinguish the main application window from torrent dialog windows.
    ''' </summary>
    Private Const QBITTORRENT_WINDOW_TITLE_PREFIX As String = "qBittorrent v"

    ''' <summary>
    ''' Localized texts shown in qBittorrent dialogs when a torrent is already present.
    ''' <para></para>
    ''' This string is used to detect and automatically handle duplicate torrent dialogs.
    ''' </summary>
    Private ReadOnly TORRENT_ALREADY_PRESENT_WINDOW_TITLES As New HashSet(Of String) From {
        "Torrent is already present",
        "El torrent ya está presente"
    }

    ''' <summary>
    ''' Prefix used to identify magnet URI links.
    ''' <para></para>
    ''' Used for parsing clipboard content and detecting torrent magnet entries.
    ''' </summary>
    Private Const MAGET_LINK_PREFIX As String = "magnet:"

    ''' <summary>
    ''' The encoding used for console output, 
    ''' and reading and writing CS/VB files.
    ''' <para></para>
    ''' It is set to UTF-8 with BOM (Byte Order Mark).
    ''' </summary>
    Private ReadOnly TextEncoding As New UTF8Encoding(True)

    ''' <summary>
    ''' The <see cref="CultureInfo"/> instance representing the "en-US" culture.
    ''' </summary>
    Private ReadOnly CultureInfoEnUs As New CultureInfo("en-US")

#End Region

#Region " Entry Point "

    ''' <summary>
    ''' The main entry point of the application.
    ''' </summary>
    ''' 
    ''' <param name="args">
    ''' The command-line arguments passed to the application.
    ''' <para></para>
    ''' The first argument (args(0)) is expected to be the path to the source directory containing text files to process.
    ''' </param>
    <DebuggerStepperBoundary>
    Public Sub Main(args As String())

        Thread.CurrentThread.CurrentCulture = Program.CultureInfoEnUs
        Thread.CurrentThread.CurrentUICulture = Program.CultureInfoEnUs

        Console.OutputEncoding = Program.TextEncoding
        Console.BackgroundColor = ConsoleColor.Black
        Console.ForegroundColor = ConsoleColor.White

        Dim consoleTitle As String = $"{My.Application.Info.Title} {My.Application.Info.Version.ToString(fieldCount:=3)} — by ElektroStudios"
#If Debug Then
        Console.Title = consoleTitle
#End If
        Program.WriteColoredLine(" " & consoleTitle, ConsoleColor.Cyan)
        Console.WriteLine("╭───────────────────────────────────────────────────────────────────────────────────────╮")
        Console.WriteLine("│ Purpose:                                                                              │")
        Console.WriteLine("│   This application automates importing magnet links from .txt files into qBittorrent, │")
        Console.WriteLine("│   handling UI confirmation dialogs, and organizing downloads into structured folders  │")
        Console.WriteLine("│   based on the source text file name and torrent index to avoid naming conflicts.     │")
        Console.WriteLine("│                                                                                       │")
        Console.WriteLine("│   A directory path must to be provided as a command-line argument, which will be      │")
        Console.WriteLine("│   scanned for .txt files located in the root directory and containing magnet links.   │")
        Console.WriteLine("│                                                                                       │")
        Console.WriteLine("│ UI Automation:                                                                        │")
        Console.WriteLine("│   The application relies on SendKeys automation for UI interaction, ensuring safe     │")
        Console.WriteLine("│   input execution by blocking keyboard and mouse user input during automation.        │")
        Console.WriteLine("│   You can press CTRL+ALT+DEL to interrupt the blocking if needed.                     │")
        Console.WriteLine("│                                                                                       │")
        Console.WriteLine("│ [!] Disclaimer:                                                                       │")
        Console.WriteLine("│   This program is shared 'as-is', without any warranty; Use it at your own risk.      │")
        Console.WriteLine("╰───────────────────────────────────────────────────────────────────────────────────────╯")
        Console.WriteLine()

        If args.Length = 0 Then
            Dim executableName As String = Process.GetCurrentProcess().ProcessName & ".exe"
            Dim exitMsg As String = "[ERROR] Source directory path argument is required." &
                                    Environment.NewLine & Environment.NewLine &
                                   $"Usage: {executableName} <directory_path>"
            Program.ExitWithMessage(exitMsg, exitCode:=1, ConsoleColor.Red)
        End If

        Dim sourceDirPath As String = args(0)
        Try
            If Not Directory.Exists(sourceDirPath) Then
                Dim exitMsg As String = $"[ERROR] The specified directory path does not exist: {sourceDirPath}"
                Program.ExitWithMessage(exitMsg, exitCode:=2, ConsoleColor.Red)
            End If

            sourceDirPath = Path.GetFullPath(sourceDirPath)

            Dim naturalSortOrderComparer As New StringNaturalComparer()

            Dim sourceFiles As New SortedSet(Of String)(
                Directory.GetFiles(sourceDirPath, "*.txt", SearchOption.TopDirectoryOnly), naturalSortOrderComparer)

            If sourceFiles.Count = 0 Then
                Dim exitMsg As String = $"[ERROR] No .txt files were found in source directory: {sourceDirPath}"
                Program.ExitWithMessage(exitMsg, exitCode:=3, ConsoleColor.Red)
            End If

            Dim totalSourceFileCount As Integer = sourceFiles.Count

            Program.WriteColoredLine($"Source directory path: {sourceDirPath}", ConsoleColor.Blue)
            Program.WriteColoredLine($"Supported files found: {totalSourceFileCount:N0} .txt files", ConsoleColor.Blue)
            Console.WriteLine()
            Program.WriteColoredLine("Press 'Y' key to start start processing text files, or 'Escape' key to exit...", ConsoleColor.Yellow)
            Do
                Dim keyInfo As ConsoleKeyInfo = Console.ReadKey(intercept:=True)
                If keyInfo.Key = ConsoleKey.Y Then
                    Exit Do
                ElseIf keyInfo.Key = ConsoleKey.Escape Then
                    Environment.Exit(0)
                End If
            Loop
            Console.WriteLine()

            For i As Integer = 0 To totalSourceFileCount - 1
                Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(sourceFiles(i))
                Program.WriteColoredLine($"Processing file {i + 1:N0} of {totalSourceFileCount:N0} ({fileNameWithoutExt}.txt)...", ConsoleColor.Cyan)
                Program.ProcessTextFile(sourceFiles(i))
#If Debug Then
                    Thread.CurrentThread.Join(0) ' Prevents ContextSwitchDeadlock on long-running iterations.
#End If
            Next i

            Console.WriteLine()
            Program.ExitWithMessage($"All files have been processed successfully.", exitCode:=0, ConsoleColor.Green)

        Catch ex As Exception
            Console.WriteLine()
            Program.ExitWithMessage($"FATAL ERROR 0x{ex.HResult:X8}: {ex.Message}", exitCode:=ex.HResult, ConsoleColor.Red)

        End Try

    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Processes a text file, copies its content to the clipboard, focuses qBittorrent,
    ''' opens the magnet dialog, and automates torrent import dialogs based on magnet links.
    ''' </summary>
    ''' 
    ''' <param name="filePath">
    ''' Full path of the text file to process.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ProcessTextFile(filePath As String)

        Dim fileNameWithoutExt As String = Path.GetFileNameWithoutExtension(filePath)
        Dim fileContent As String = File.ReadAllText(filePath, Program.TextEncoding)

        If String.IsNullOrWhiteSpace(fileContent) Then
            Exit Sub
            ' Throw New Exception($"File is empty: {filePath}")
        End If

        ' Copy to Clipboard
        Clipboard.SetText(fileContent)

        ' Focus qBittorrent
        Dim qbtProcess() As Process = Process.GetProcessesByName(Program.QBITTORRENT_PROCESS_NAME)
        If qbtProcess.Length = 0 Then
            Dim exitMsg As String = $"[ERROR] {Program.QBITTORRENT_PROCESS_NAME}.exe is not running."
            Program.ExitWithMessage(exitMsg, exitCode:=4, ConsoleColor.Red)
        End If

        Dim mainWindowHandle As IntPtr = qbtProcess(0).MainWindowHandle
        If mainWindowHandle = IntPtr.Zero Then
            ' Fallback if minimized to tray
            mainWindowHandle = NativeMethods.FindWindow(Program.QT_WINDOW_CLASS, Nothing)
        End If

        If mainWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Could not find {Program.QBITTORRENT_PROCESS_NAME}.exe main window."
            Program.ExitWithMessage(exitMsg, exitCode:=5, ConsoleColor.Red)
        End If

        ' Step 3: Open Magnet Dialog (CTRL+SHIFT+O)
        Program.ExecuteSendKeysBatchSafely(mainWindowHandle,
            {
                ("^+o", 1000),
                ("^a", 250),
                ("^c", 250),
                ("{ENTER}", 100)
            }, delayAfterFocusMs:=500
        )

        ' Count magnet URLs from clipboard
        Dim magnetUrlsCount As Integer = Program.CountMagnetLinksFromClipboard()

        ' Step 4: Wait for dialogs to stabilize
        Dim dialogHandles As List(Of IntPtr) = Program.WaitForDialogs(qbtProcess(0).Id, magnetUrlsCount)

        ' Step 5: Iterate dialogs
        Program.ProcessDialogs(qbtProcess(0).Id, fileNameWithoutExt, dialogHandles.Count)

        Console.WriteLine()
    End Sub

    ''' <summary>
    ''' Waits until the expected number of torrent dialog windows appear and stabilize for a given process.
    ''' </summary>
    ''' 
    ''' <param name="processId">
    ''' Target process ID (qBittorrent).
    ''' </param>
    ''' 
    ''' <param name="expectedDialogsCount">
    ''' Number of expected dialog windows.</param>
    ''' 
    ''' <param name="timeout">
    ''' Maximum time to wait before throwing a timeout exception.
    ''' </param>
    ''' 
    ''' <param name="pollIntervalMs">
    ''' Polling interval in milliseconds.
    ''' </param>
    ''' 
    ''' <returns>
    ''' List of window handles corresponding to detected dialogs.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function WaitForDialogs(processId As Integer,
                                    expectedDialogsCount As Integer,
                           Optional timeout As TimeSpan = Nothing,
                           Optional pollIntervalMs As Integer = 500) As List(Of IntPtr)

        If expectedDialogsCount <= 0 Then
            Return New List(Of IntPtr)()
        End If

        If timeout = TimeSpan.Zero Then
            timeout = TimeSpan.FromMinutes(5)
        End If

        Program.WriteColoredLine($"Waiting for {expectedDialogsCount:N0} torrent dialog windows to appear...", Console.ForegroundColor)

        Dim stopwatch As Stopwatch = Stopwatch.StartNew()

        Dim lastStable As List(Of IntPtr) = Nothing

        While stopwatch.Elapsed < timeout

            Dim currentHandles As List(Of IntPtr) = GetTargetDialogs(processId)
            Dim removed As Integer = HandleAlreadyPresentDialogs(processId)
            If removed > 0 Then
                expectedDialogsCount -= removed
                Program.WriteColoredLine($"Duplicate torrents dialog detected. Adjusted expected dialogs count: {expectedDialogsCount:N0}", ConsoleColor.Yellow)
            End If

            If expectedDialogsCount < 0 Then
                expectedDialogsCount = 0
            End If

            If currentHandles.Count >= expectedDialogsCount Then

                If lastStable IsNot Nothing Then

                    Dim same As Boolean =
                        lastStable.Count = currentHandles.Count AndAlso
                        Not lastStable.Except(currentHandles).Any()

                    If same Then
                        Dim dummy As IntPtr = IntPtr.Zero

                        For Each hwnd As IntPtr In currentHandles
                            NativeMethods.SendMessageTimeout(hwnd, Win32.WM_NULL, IntPtr.Zero, IntPtr.Zero, Win32.SMTO_BLOCK, 99999, dummy)
                        Next

                        Program.WriteColoredLine($"Detected {currentHandles.Count:N0} dialogs (stable state).", ConsoleColor.Green)
                        Return currentHandles

                    End If
                End If

                lastStable = currentHandles
            End If

            Thread.Sleep(pollIntervalMs)
            Thread.CurrentThread.Join(0)
        End While

        Dim exitMsg As String = $"[ERROR] Dialog wait has timed out. Expected: {expectedDialogsCount:N0} dialogs."
        Program.ExitWithMessage(exitMsg, exitCode:=6, ConsoleColor.Red)
        Return Nothing
    End Function

    ''' <summary>
    ''' Processes all detected torrent dialogs sequentially.
    ''' </summary>
    ''' 
    ''' <param name="processId">
    ''' Target process ID.
    ''' </param>
    ''' 
    ''' <param name="fileNameWithoutExt">
    ''' Base filename used for folder naming.
    ''' </param>
    ''' 
    ''' <param name="expectedCount">
    ''' Number of dialogs expected to process.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ProcessDialogs(processId As Integer,
                               fileNameWithoutExt As String,
                               expectedCount As Integer)

        Dim processed As New HashSet(Of IntPtr)()

        For i As Integer = 1 To expectedCount
            Dim hwnd As IntPtr = Program.GetNextDialog(processId, processed)

            If hwnd = IntPtr.Zero Then
                Dim exitMsg As String = $"[ERROR] Could not resolve dialog {i:N0} of {expectedCount:N0}"
                Program.ExitWithMessage(exitMsg, exitCode:=7, ConsoleColor.Red)
            End If
            processed.Add(hwnd)

            Program.WriteColoredLine($"Automating dialog {i:N0} of {expectedCount:N0} ({fileNameWithoutExt}.txt)...", ConsoleColor.DarkCyan)
            Program.AutomateDialog(hwnd, fileNameWithoutExt, i)
        Next
    End Sub

    ''' <summary>
    ''' Retrieves the next unprocessed dialog window handle for the given process.
    ''' </summary>
    ''' 
    ''' <param name="processId">
    ''' Target process ID.
    ''' </param>
    ''' 
    ''' <param name="processed">
    ''' Set of already processed window handles.
    ''' </param>
    ''' 
    ''' <returns>
    ''' Handle of the next available dialog or IntPtr.Zero if none found.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetNextDialog(processId As Integer,
                                   processed As HashSet(Of IntPtr)) As IntPtr

        Dim hwnds As List(Of IntPtr) = Program.GetTargetDialogs(processId)

        For Each hwnd As IntPtr In hwnds
            If Not processed.Contains(hwnd) Then
                Return hwnd
            End If
        Next

        Return IntPtr.Zero
    End Function

    ''' <summary>
    ''' Automates a torrent dialog by setting the download path and confirming the dialog.
    ''' </summary>
    ''' 
    ''' <param name="hwnd">
    ''' Handle of the dialog window.</param>
    ''' 
    ''' <param name="baseName">
    ''' Base folder name derived from the file.
    ''' </param>
    ''' 
    ''' <param name="index">
    ''' Dialog index for subfolder creation.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub AutomateDialog(hwnd As IntPtr, baseName As String, index As Integer)

        ' Ensure Caps Lock is disabled before automation
        Program.SetCapsLock(False)

        ' Select all text and copy current download path from UI
        Program.ExecuteSendKeysBatchSafely(hwnd,
            {
                ("^a", 100),
                ("^c", 100)
            }, delayAfterFocusMs:=200
        )

        Dim stopwatch As Stopwatch = Stopwatch.StartNew()
        Dim currentPath As String = String.Empty

        Do Until Not String.IsNullOrWhiteSpace(currentPath)
            If stopwatch.Elapsed.TotalSeconds >= 10.0R Then
                Dim exitMsg As String = "[ERROR] Failed to retrieve download path from the clipboard within 10 seconds. Clipboard content still empty."
                Program.ExitWithMessage(exitMsg, exitCode:=8, ConsoleColor.Red)
            End If

            currentPath = Clipboard.GetText()
            Thread.Sleep(100)
        Loop

        ' Normalize and build final target path.
        Dim normalizedBaseName As String =
            baseName.Replace(Strings.ChrW(160), " "c).
                     Replace("/"c, "-"c).
                     Replace("\"c, "-"c).
                     Replace("!"c, "-"c).
                     Replace("^"c, "-"c).
                     Replace("*"c, "-"c).
                     Replace("%"c, "-"c).
                     Replace(""""c, "-"c).
                     Replace("|"c, "-"c).
                     Replace("?"c, "-"c).
                     Trim({"."c, ","c, ";"c, " "c})

        Dim normalizedBasePath As String =
            currentPath.Replace(Strings.ChrW(160), " "c).
                        TrimEnd({"\"c, "."c, " "c}).
                        Trim()

        Dim targetPath As String = $"{normalizedBasePath}\{normalizedBaseName}\{index}"

        ' Write final path to UI, and confirm the dialog window.
        Program.ExecuteSendKeysBatchSafely(hwnd,
            {
                ("{BACKSPACE}", 100),
                (targetPath, 300),
                ("{ENTER}", 100)
            }, delayAfterFocusMs:=200
        )
    End Sub

    ''' <summary>
    ''' Retrieves all torrent-related dialog windows belonging to the target process.
    ''' </summary>
    ''' 
    ''' <param name="TargetProcessId">
    ''' Process ID to filter windows.
    ''' </param>
    ''' 
    ''' <returns>
    ''' List of window handles matching torrent dialogs.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function GetTargetDialogs(TargetProcessId As Integer) As List(Of IntPtr)

        Dim foundHandles As New List(Of IntPtr)()

        Dim titleBufferLen As Integer = Program.QBITTORRENT_WINDOW_TITLE_PREFIX.Length + 1

        NativeMethods.EnumWindows(
            Function(Hwnd, LParam)
                Dim currentWindowPid As Integer
                NativeMethods.GetWindowThreadProcessId(Hwnd, currentWindowPid)

                If currentWindowPid = TargetProcessId Then
                    Dim classNameBuffer As New StringBuilder(256)
                    NativeMethods.GetClassName(Hwnd, classNameBuffer, classNameBuffer.Capacity)

                    If classNameBuffer.ToString() = Program.QT_WINDOW_CLASS Then
                        Dim titleBuffer As New StringBuilder(titleBufferLen)
                        NativeMethods.GetWindowText(Hwnd, titleBuffer, titleBuffer.Capacity)
                        Dim windowTitle As String = titleBuffer.ToString().Trim()

                        ' Logic: Exclude the main window by checking if it starts with "qBittorrent v"
                        ' This targets only the torrent configuration dialogs (which have torrent names as titles)
                        If Not windowTitle.StartsWith(Program.QBITTORRENT_WINDOW_TITLE_PREFIX, StringComparison.OrdinalIgnoreCase) Then
                            foundHandles.Add(Hwnd)
                        End If
                    End If
                End If
                Return True
            End Function, IntPtr.Zero)

        Return foundHandles
    End Function

    ''' <summary>
    ''' Detects and automatically handles "Torrent already present" dialog windows.
    ''' </summary>
    ''' 
    ''' <param name="processId">
    ''' Target process ID.
    ''' </param>
    ''' 
    ''' <returns>
    ''' Number of dialogs handled.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function HandleAlreadyPresentDialogs(processId As Integer) As Integer

        Dim handled As Integer = 0

        NativeMethods.EnumWindows(
            Function(hwnd As IntPtr, lParam As IntPtr) As Boolean

                Dim pid As Integer = 0
                NativeMethods.GetWindowThreadProcessId(hwnd, pid)
                If pid <> processId Then
                    Return True
                End If

                Dim className As New StringBuilder(Program.QT_WINDOW_CLASS.Length + 1)
                NativeMethods.GetClassName(hwnd, className, className.Capacity)
                If className.ToString() <> Program.QT_WINDOW_CLASS Then
                    Return True
                End If

                Dim title As New StringBuilder(128)
                NativeMethods.GetWindowText(hwnd, title, title.Capacity)

                If Program.TORRENT_ALREADY_PRESENT_WINDOW_TITLES.Any(
                        Function(localizedTitle As String)
                            Return title.ToString().IndexOf(localizedTitle, StringComparison.OrdinalIgnoreCase) >= 0
                        End Function) Then

                    Program.EnsureWindowReady(hwnd)
                    SendKeys.SendWait("{ENTER}")
                    handled += 1
                End If

                Return True
            End Function, IntPtr.Zero)

        Return handled
    End Function

    ''' <summary>
    ''' Executes a batch of SendKeys actions safely by blocking user input and ensuring window focus.
    ''' </summary>
    ''' 
    ''' <param name="targetWindowHandle">
    ''' Target window handle.
    ''' </param>
    ''' 
    ''' <param name="actions">
    ''' Sequence of keys and delays.
    ''' </param>
    ''' 
    ''' <param name="focusTimeoutMs">
    ''' Maximum time to focus the window.
    ''' </param>
    ''' 
    ''' <param name="delayAfterFocusMs">
    ''' Delay after focusing before input starts.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub ExecuteSendKeysBatchSafely(targetWindowHandle As IntPtr,
                                          actions As IEnumerable(Of (Keys As String, delayAfterSendMs As Integer)),
                                 Optional focusTimeoutMs As Integer = 5000,
                                 Optional delayAfterFocusMs As Integer = 300)

        If targetWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Invalid window handle: {targetWindowHandle}"
            Program.ExitWithMessage(exitMsg, exitCode:=9, ConsoleColor.Red)
        End If

        Try
            NativeMethods.BlockInput(blockIt:=True)
            Program.EnsureWindowReady(
                targetWindowHandle:=targetWindowHandle,
                focusTimeoutMs:=focusTimeoutMs,
                delayAfterFocusMs:=delayAfterFocusMs
            )

            For Each action As (Keys As String, delayAfterSendMs As Integer) In actions

                SendKeys.SendWait(action.Keys)

                If action.delayAfterSendMs > 0 Then
                    Thread.Sleep(action.delayAfterSendMs)
                End If
                Thread.CurrentThread.Join(0)
            Next

        Finally
            NativeMethods.BlockInput(blockIt:=False)

        End Try

    End Sub

    ''' <summary>
    ''' Ensures the target window is restored, visible, and ready to receive input focus.
    ''' </summary>
    ''' 
    ''' <param name="targetWindowHandle">
    ''' Window handle to prepare.
    ''' </param>
    ''' 
    ''' <param name="focusTimeoutMs">
    ''' Maximum time to acquire focus.
    ''' </param>
    ''' 
    ''' <param name="delayAfterFocusMs">
    ''' Delay after focus is set.
    ''' </param>
    <DebuggerStepThrough>
    Public Sub EnsureWindowReady(targetWindowHandle As IntPtr,
                        Optional focusTimeoutMs As Integer = 5000,
                        Optional delayAfterFocusMs As Integer = 300)

        If targetWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Invalid window handle: {targetWindowHandle}"
            Program.ExitWithMessage(exitMsg, exitCode:=9, ConsoleColor.Red)
        End If

        ' Restore if minimized / hidden in systray icon
        If Not NativeMethods.IsWindowVisible(targetWindowHandle) Then
            ' Initial Restore from taskbar or system tray
            NativeMethods.ShowWindow(targetWindowHandle, Win32.SW_RESTORE)
            Thread.Sleep(500)

            ' Force Minimize to Taskbar (Forces context recreation)
            NativeMethods.ShowWindow(targetWindowHandle, Win32.SW_MINIMIZE)
            Thread.Sleep(500)

            ' Final restore
            NativeMethods.ShowWindow(targetWindowHandle, Win32.SW_RESTORE)
            Thread.Sleep(1000)
            NativeMethods.UpdateWindow(targetWindowHandle)
        End If

        Dim startTick As Integer = Environment.TickCount

        Program.ForceWindowToForeground(targetWindowHandle, focusTimeoutMs)
        Thread.Sleep(delayAfterFocusMs)
    End Sub

    ''' <summary>
    ''' Forces a window into the foreground, attempting repeated focus acquisition until timeout.
    ''' </summary>
    ''' 
    ''' <param name="targetWindowHandle">
    ''' Target window handle.
    ''' </param>
    ''' 
    ''' <param name="focusTimeoutMs">
    ''' Maximum time allowed to set foreground focus.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub ForceWindowToForeground(targetWindowHandle As IntPtr, focusTimeoutMs As Integer)

        If targetWindowHandle = IntPtr.Zero Then
            Dim exitMsg1 As String = $"[ERROR] Invalid window handle: {targetWindowHandle}"
            Program.ExitWithMessage(exitMsg1, exitCode:=9, ConsoleColor.Red)
        End If

        Dim currentThreadId As Integer = NativeMethods.GetCurrentThreadId()
        Dim stopwatch As Stopwatch = Stopwatch.StartNew()

        While stopwatch.ElapsedMilliseconds < focusTimeoutMs

            Dim foregroundHwnd As IntPtr = NativeMethods.GetForegroundWindow()
            Dim foregroundProcessId As Integer = 0
            Dim foregroundThreadId As Integer = 0

            If foregroundHwnd <> IntPtr.Zero Then
                foregroundThreadId = NativeMethods.GetWindowThreadProcessId(foregroundHwnd, foregroundProcessId)
            End If

            Dim attached As Boolean = False
            Try
                If foregroundThreadId <> 0 AndAlso foregroundThreadId <> currentThreadId Then
                    attached = NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, True)
                End If

                NativeMethods.BringWindowToTop(targetWindowHandle)
                NativeMethods.SetForegroundWindow(targetWindowHandle)
                Exit Sub

            Finally
                If attached Then
                    NativeMethods.AttachThreadInput(currentThreadId, foregroundThreadId, False)
                End If
            End Try

            Thread.Sleep(100)
            Thread.CurrentThread.Join(0)
        End While

        Dim exitMsg2 As String = "[ERROR] Timed out waiting for target window focus."
        Program.ExitWithMessage(exitMsg2, exitCode:=6, ConsoleColor.Red)
    End Sub

    ''' <summary>
    ''' Counts the number of magnet links currently present in the clipboard text.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' Number of detected magnet URLs.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function CountMagnetLinksFromClipboard() As Integer

        Dim clipboardText As String = Clipboard.GetText()
        If String.IsNullOrWhiteSpace(clipboardText) Then
            Return 0
        End If

        Dim lines As String() =
            clipboardText.Split({ControlChars.CrLf, ControlChars.Lf}, StringSplitOptions.None).
                          Distinct.ToArray()

        Dim magnetUrlsCount As Integer = 0

        For Each line As String In lines

            If String.IsNullOrWhiteSpace(line) Then
                Continue For
            End If

            If line.StartsWith(Program.MAGET_LINK_PREFIX, StringComparison.OrdinalIgnoreCase) Then
                magnetUrlsCount += 1
            End If
        Next line

        Return magnetUrlsCount
    End Function

    ''' <summary>
    ''' Determines whether Caps Lock is currently enabled.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' True if Caps Lock is active; otherwise False.
    ''' </returns>
    <DebuggerStepThrough>
    Private Function IsCapsLockOn() As Boolean

        Return (NativeMethods.GetKeyState(Win32.VK_CAPITAL) And 1) <> 0
    End Function

    ''' <summary>
    ''' Sets the Caps Lock state to the desired value.
    ''' </summary>
    ''' 
    ''' <param name="enabled">
    ''' Desired Caps Lock state.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub SetCapsLock(enabled As Boolean)

        Dim current As Boolean = Program.IsCapsLockOn()

        If current <> enabled Then
            NativeMethods.keybd_event(Win32.VK_CAPITAL, 0, 0, 0)
            NativeMethods.keybd_event(Win32.VK_CAPITAL, 0, Win32.KEYEVENTF_KEYUP, 0)
        End If
    End Sub

    ''' <summary>
    ''' Writes a message to the console in a specified foreground color, 
    ''' then resets the color back to the original.
    ''' </summary>
    ''' 
    ''' <param name="message">
    ''' The message to display. If empty or null, no message is displayed.
    ''' </param>
    ''' 
    ''' <param name="foreColor">
    ''' The console foreground color to use when displaying the message. 
    ''' <para></para>
    ''' After writing the message, the console color is reset to its original value.
    ''' </param>
    <DebuggerStepThrough>
    Private Sub WriteColoredLine(message As String, foreColor As ConsoleColor)

        Dim originalForeColor As ConsoleColor = Console.ForegroundColor
        Console.ForegroundColor = foreColor
        Console.WriteLine(message)
        Console.ForegroundColor = originalForeColor
    End Sub

    ''' <summary>
    ''' Displays a message to the console and exits the application with the specified exit code.
    ''' </summary>
    ''' 
    ''' <param name="message">
    ''' The message to display before exiting. If empty or null, no message is displayed.
    ''' </param>
    ''' 
    ''' <param name="exitCode">
    ''' The exit code to return to the operating system. Typically 0 for success, non-zero for errors.
    ''' </param>
    ''' 
    ''' <remarks>
    ''' After displaying the message (if provided), the method waits for the user to press any key
    ''' before terminating the application. This allows the user to see the output in the console window.
    ''' </remarks>
    <DebuggerStepThrough>
    Private Sub ExitWithMessage(message As String, exitCode As Integer, foreColor As ConsoleColor)

        If Not String.IsNullOrEmpty(message) Then
            Program.WriteColoredLine(message, foreColor)
            Console.WriteLine()
        End If

        Console.WriteLine($"Exiting application with exit code: {exitCode} (0x{exitCode:X8}) ...")
#If DEBUG Then
        Console.WriteLine()
        Program.WriteColoredLine("[!] This message only appears in DEBUG mode to prevent accidental termination.", ConsoleColor.Yellow)
        Console.WriteLine("Press any key to exit...")
        Console.ReadKey(intercept:=True)
#End If
        Environment.Exit(exitCode)
    End Sub

#End Region

End Module
