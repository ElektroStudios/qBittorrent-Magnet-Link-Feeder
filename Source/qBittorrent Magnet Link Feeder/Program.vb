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

#Region " Program "

''' <summary>
''' Represents the main application execution context.
''' </summary>
Public Module Program

#Region " Fields "

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
    <STAThread>
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
        Console.WriteLine("│   This program is distributed 'as-is', without any warranty; Use it at your own risk. │")
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

            Program.WriteColoredLine($"Source directory path: {sourceDirPath}", ConsoleColor.DarkYellow)
            Program.WriteColoredLine($"Supported files found: {totalSourceFileCount:N0} .txt files", ConsoleColor.DarkYellow)
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
#If DEBUG Then
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
    <DebuggerStepperBoundary>
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
        Dim qbtProcess() As Process = Process.GetProcessesByName(AppGlobals.QBITTORRENT_PROCESS_NAME)
        If qbtProcess.Length = 0 Then
            Dim exitMsg As String = $"[ERROR] {AppGlobals.QBITTORRENT_PROCESS_NAME}.exe is not running."
            Program.ExitWithMessage(exitMsg, exitCode:=4, ConsoleColor.Red)
        End If

        Dim mainWindowHandle As IntPtr = qbtProcess(0).MainWindowHandle
        If mainWindowHandle = IntPtr.Zero Then
            ' Fallback if minimized to tray
            mainWindowHandle = NativeMethods.FindWindow(AppGlobals.QT_WINDOW_CLASS, Nothing)
        End If

        If mainWindowHandle = IntPtr.Zero Then
            Dim exitMsg As String = $"[ERROR] Could not find {AppGlobals.QBITTORRENT_PROCESS_NAME}.exe main window."
            Program.ExitWithMessage(exitMsg, exitCode:=5, ConsoleColor.Red)
        End If

        ' Step 3: Open Magnet Dialog (CTRL+SHIFT+O)
        AutomationHelper.ExecuteSendKeysBatchSafely(mainWindowHandle,
            {
                ("^+o", 1000),
                ("^a", 250),
                ("^c", 250),
                ("{ENTER}", 100)
            }, delayAfterFocusMs:=500
        )

        ' Count magnet URLs from clipboard
        Dim magnetUrlsCount As Integer = AutomationHelper.CountMagnetLinksFromClipboard()

        ' Step 4: Wait for dialogs to stabilize
        Dim dialogHandles As List(Of IntPtr) = AutomationHelper.WaitForDialogs(qbtProcess(0).Id, magnetUrlsCount)

        ' Step 5: Iterate dialogs
        AutomationHelper.ProcessDialogs(qbtProcess(0).Id, fileNameWithoutExt, dialogHandles.Count)

        Console.WriteLine()
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
    Friend Sub WriteColoredLine(message As String, foreColor As ConsoleColor)

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
    Friend Sub ExitWithMessage(message As String, exitCode As Integer, foreColor As ConsoleColor)

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

#End Region
