#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Linq
Imports System.Text
Imports System.Threading
Imports System.Windows.Forms

#End Region

Friend Module AutomationHelper

#Region " Public / Friend Methods "

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
    Friend Function WaitForDialogs(processId As Integer,
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
    Friend Sub ProcessDialogs(processId As Integer,
                               fileNameWithoutExt As String,
                               expectedCount As Integer)

        Dim processed As New HashSet(Of IntPtr)()

        For i As Integer = 1 To expectedCount
            Dim hwnd As IntPtr = AutomationHelper.GetNextDialog(processId, processed)

            If hwnd = IntPtr.Zero Then
                Dim exitMsg As String = $"[ERROR] Could not resolve dialog {i:N0} of {expectedCount:N0}"
                Program.ExitWithMessage(exitMsg, exitCode:=7, ConsoleColor.Red)
            End If
            processed.Add(hwnd)

            Program.WriteColoredLine($"Automating dialog {i:N0} of {expectedCount:N0} ({fileNameWithoutExt}.txt)...", ConsoleColor.DarkCyan)
            AutomationHelper.AutomateDialog(hwnd, fileNameWithoutExt, i)
        Next
    End Sub

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
    Friend Sub ExecuteSendKeysBatchSafely(targetWindowHandle As IntPtr,
                                          actions As IEnumerable(Of (Keys As String, delayAfterSendMs As Integer)),
                                 Optional focusTimeoutMs As Integer = 5000,
                                 Optional delayAfterFocusMs As Integer = 300)

        If targetWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Invalid window handle: {targetWindowHandle}"
            Program.ExitWithMessage(exitMsg, exitCode:=9, ConsoleColor.Red)
        End If

        Try
            NativeMethods.BlockInput(blockIt:=True)
            AutomationHelper.EnsureWindowReady(
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
    ''' Counts the number of magnet links currently present in the clipboard text.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' Number of detected magnet URLs.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function CountMagnetLinksFromClipboard() As Integer

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

            If line.StartsWith(AppGlobals.MAGET_LINK_PREFIX, StringComparison.OrdinalIgnoreCase) Then
                magnetUrlsCount += 1
            End If
        Next line

        Return magnetUrlsCount
    End Function

#End Region

#Region " Private Methods "

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

        Dim hwnds As List(Of IntPtr) = AutomationHelper.GetTargetDialogs(processId)

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
        AutomationHelper.SetCapsLock(False)

        ' Select all text and copy current download path from UI
        AutomationHelper.ExecuteSendKeysBatchSafely(hwnd,
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
        Dim normalizedBaseFileName As String = PathHelper.NormalizeBaseFileName(baseName)
        Dim normalizedBaseDirPath As String = PathHelper.NormalizeBaseDirPath(currentPath)

        Dim targetPath As String = $"{normalizedBaseDirPath}\{normalizedBaseFileName}\{index}"

        ' Write final path to UI, and confirm the dialog window.
        AutomationHelper.ExecuteSendKeysBatchSafely(hwnd,
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

        Dim titleBufferLen As Integer = AppGlobals.QBITTORRENT_WINDOW_TITLE_PREFIX.Length + 1

        NativeMethods.EnumWindows(
            Function(Hwnd, LParam)
                Dim currentWindowPid As Integer
                NativeMethods.GetWindowThreadProcessId(Hwnd, currentWindowPid)

                If currentWindowPid = TargetProcessId Then
                    Dim classNameBuffer As New StringBuilder(256)
                    NativeMethods.GetClassName(Hwnd, classNameBuffer, classNameBuffer.Capacity)

                    If classNameBuffer.ToString() = AppGlobals.QT_WINDOW_CLASS Then
                        Dim titleBuffer As New StringBuilder(titleBufferLen)
                        NativeMethods.GetWindowText(Hwnd, titleBuffer, titleBuffer.Capacity)
                        Dim windowTitle As String = titleBuffer.ToString().Trim()

                        ' Logic: Exclude the main window by checking if it starts with "qBittorrent v"
                        ' This targets only the torrent configuration dialogs (which have torrent names as titles)
                        If Not windowTitle.StartsWith(AppGlobals.QBITTORRENT_WINDOW_TITLE_PREFIX, StringComparison.OrdinalIgnoreCase) Then
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

                Dim className As New StringBuilder(AppGlobals.QT_WINDOW_CLASS.Length + 1)
                NativeMethods.GetClassName(hwnd, className, className.Capacity)
                If className.ToString() <> AppGlobals.QT_WINDOW_CLASS Then
                    Return True
                End If

                Dim title As New StringBuilder(128)
                NativeMethods.GetWindowText(hwnd, title, title.Capacity)

                If AppGlobals.TORRENT_ALREADY_PRESENT_WINDOW_TITLES.Any(
                        Function(localizedTitle As String)
                            Return title.ToString().IndexOf(localizedTitle, StringComparison.OrdinalIgnoreCase) >= 0
                        End Function) Then

                    AutomationHelper.EnsureWindowReady(hwnd)
                    SendKeys.SendWait("{ENTER}")
                    handled += 1
                End If

                Return True
            End Function, IntPtr.Zero)

        Return handled
    End Function

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
    Private Sub EnsureWindowReady(targetWindowHandle As IntPtr,
                        Optional focusTimeoutMs As Integer = 5000,
                        Optional delayAfterFocusMs As Integer = 300)

        If targetWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Invalid window handle: {targetWindowHandle}"
            Program.ExitWithMessage(exitMsg, exitCode:=9, ConsoleColor.Red)
        End If

        Dim isHidden As Boolean = Not NativeMethods.IsWindowVisible(targetWindowHandle)
        Dim isMinimized As Boolean = NativeMethods.IsIconic(targetWindowHandle)

        If isHidden OrElse isMinimized Then
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

        AutomationHelper.ForceWindowToForeground(targetWindowHandle, focusTimeoutMs)
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

        Dim current As Boolean = AutomationHelper.IsCapsLockOn()

        If current <> enabled Then
            NativeMethods.keybd_event(Win32.VK_CAPITAL, 0, 0, 0)
            NativeMethods.keybd_event(Win32.VK_CAPITAL, 0, Win32.KEYEVENTF_KEYUP, 0)
        End If
    End Sub

#End Region

End Module
