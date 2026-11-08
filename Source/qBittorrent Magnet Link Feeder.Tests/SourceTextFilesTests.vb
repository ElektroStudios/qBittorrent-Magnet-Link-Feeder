#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.IO

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " Source Text Files Tests "

''' <summary>
''' Unit tests for the <see cref="Program.GetSourceTextFiles(String)"/> function.
''' <para></para>
''' Each test works on its own temporary directory, which is deleted after the test completes.
''' </summary>
<TestClass>
Public NotInheritable Class SourceTextFilesTests

#Region " Fields "

    ''' <summary>
    ''' The temporary directory used by the current test.
    ''' </summary>
    Private tempDirPath As String

#End Region

#Region " Test Initialization / Cleanup "

    ''' <summary>
    ''' Creates a new empty temporary directory for the current test.
    ''' </summary>
    <TestInitialize>
    Public Sub TestInitialize()

        Me.tempDirPath = Path.Combine(Path.GetTempPath(), $"qBittorrent_Magnet_Link_Feeder.Tests_{Guid.NewGuid():N}")
        Directory.CreateDirectory(Me.tempDirPath)
    End Sub

    ''' <summary>
    ''' Deletes the temporary directory of the current test.
    ''' </summary>
    <TestCleanup>
    Public Sub TestCleanup()

        Directory.Delete(Me.tempDirPath, recursive:=True)
    End Sub

#End Region

#Region " Argument Validation "

    ''' <summary>
    ''' Verifies that a <see langword="Nothing"/> directory path throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_NullDirectoryPath_ThrowsArgumentNullException()

        Dim ex As ArgumentNullException =
            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() Program.GetSourceTextFiles(Nothing))

        Assert.AreEqual("directoryPath", ex.ParamName)
    End Sub

    ''' <summary>
    ''' Verifies that a non-existent directory path throws <see cref="DirectoryNotFoundException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_NonExistentDirectory_ThrowsDirectoryNotFoundException()

        Dim nonExistentDirPath As String = Path.Combine(Me.tempDirPath, "Non-existent directory")

        Assert.ThrowsExactly(Of DirectoryNotFoundException)(Sub() Program.GetSourceTextFiles(nonExistentDirPath))
    End Sub

#End Region

#Region " File Discovery "

    ''' <summary>
    ''' Verifies that an empty directory returns an empty array.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_EmptyDirectory_ReturnsEmptyArray()

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        Assert.IsNotNull(result)
        Assert.IsEmpty(result)
    End Sub

    ''' <summary>
    ''' Verifies that the returned file paths are full paths sorted in natural sort order.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_MultipleFiles_ReturnsFullPathsInNaturalOrder()

        For Each fileName As String In {"List 10.txt", "List 2.txt", "List 1.txt", "list 3.txt"}
            Me.CreateFile(fileName)
        Next fileName

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        Dim expected As String() = {
            Path.Combine(Me.tempDirPath, "List 1.txt"),
            Path.Combine(Me.tempDirPath, "List 2.txt"),
            Path.Combine(Me.tempDirPath, "list 3.txt"),
            Path.Combine(Me.tempDirPath, "List 10.txt")
        }
        CollectionAssert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that the ".txt" extension is matched case-insensitively.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_UpperCaseExtension_IsIncluded()

        Me.CreateFile("List.TXT")

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        CollectionAssert.AreEqual({Path.Combine(Me.tempDirPath, "List.TXT")}, result)
    End Sub

    ''' <summary>
    ''' Verifies that files with an extension other than ".txt" are excluded.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_OtherExtensions_AreExcluded()

        For Each fileName As String In {"List.txt", "List.log", "List.md", "List", "List.txt.bak"}
            Me.CreateFile(fileName)
        Next fileName

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        CollectionAssert.AreEqual({Path.Combine(Me.tempDirPath, "List.txt")}, result)
    End Sub

    ''' <summary>
    ''' Verifies that files with longer extensions that start with "txt" are excluded,
    ''' even though the "*.txt" search pattern of the Windows file system API matches them.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_ExtensionsStartingWithTxt_AreExcluded()

        For Each fileName As String In {"List.txt", "List.txt2", "List.txtold", "List.text"}
            Me.CreateFile(fileName)
        Next fileName

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        CollectionAssert.AreEqual({Path.Combine(Me.tempDirPath, "List.txt")}, result)
    End Sub

    ''' <summary>
    ''' Verifies that only the root of the directory is scanned, not its subdirectories.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_FilesInSubdirectories_AreExcluded()

        Me.CreateFile("Root.txt")
        Directory.CreateDirectory(Path.Combine(Me.tempDirPath, "Subdirectory"))
        Me.CreateFile(Path.Combine("Subdirectory", "Nested.txt"))

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        CollectionAssert.AreEqual({Path.Combine(Me.tempDirPath, "Root.txt")}, result)
    End Sub

    ''' <summary>
    ''' Verifies that a directory named with a ".txt" extension is not returned as a file.
    ''' </summary>
    <TestMethod>
    Public Sub GetSourceTextFiles_DirectoryWithTxtExtension_IsExcluded()

        Directory.CreateDirectory(Path.Combine(Me.tempDirPath, "Folder.txt"))

        Dim result As String() = Program.GetSourceTextFiles(Me.tempDirPath)

        Assert.IsEmpty(result)
    End Sub

#End Region

#Region " Private Methods "

    ''' <summary>
    ''' Creates a file containing a sample magnet link in the temporary directory of the current test.
    ''' </summary>
    '''
    ''' <param name="relativeFilePath">
    ''' The file path, relative to the temporary directory.
    ''' </param>
    Private Sub CreateFile(relativeFilePath As String)

        Dim filePath As String = Path.Combine(Me.tempDirPath, relativeFilePath)
        File.WriteAllText(filePath, "magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")
    End Sub

#End Region

End Class

#End Region
