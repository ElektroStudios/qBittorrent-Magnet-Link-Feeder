#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " PathHelper Tests "

''' <summary>
''' Unit tests for the <see cref="PathHelper"/> module.
''' </summary>
<TestClass>
Public NotInheritable Class PathHelperTests

#Region " Fields "

    ''' <summary>
    ''' A non-breaking space character (U+00A0).
    ''' </summary>
    Private Shared ReadOnly NonBreakingSpace As Char = ChrW(160)

#End Region

#Region " NormalizeBaseFileName "

    ''' <summary>
    ''' Verifies that <see langword="Nothing"/> and empty strings are normalized to an empty string.
    ''' </summary>
    '''
    ''' <param name="baseName">
    ''' The base name to normalize.
    ''' </param>
    <TestMethod>
    <DataRow(Nothing)>
    <DataRow("")>
    Public Sub NormalizeBaseFileName_NullOrEmpty_ReturnsEmptyString(baseName As String)

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual(String.Empty, result)
    End Sub

    ''' <summary>
    ''' Verifies that every invalid Windows file name character is replaced with a hyphen.
    ''' </summary>
    '''
    ''' <param name="baseName">
    ''' The base name to normalize.
    ''' </param>
    '''
    ''' <param name="expected">
    ''' The expected normalized base name.
    ''' </param>
    <TestMethod>
    <DataRow("a/b", "a-b")>
    <DataRow("a\b", "a-b")>
    <DataRow("a*b", "a-b")>
    <DataRow("a""b", "a-b")>
    <DataRow("a|b", "a-b")>
    <DataRow("a?b", "a-b")>
    <DataRow("a:b", "a-b")>
    <DataRow("a<b", "a-b")>
    <DataRow("a>b", "a-b")>
    <DataRow("<Movies: Part 1/2?>", "-Movies- Part 1-2--")>
    Public Sub NormalizeBaseFileName_InvalidCharacters_AreReplacedWithHyphen(baseName As String, expected As String)

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that non-breaking spaces are replaced with regular spaces.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseFileName_NonBreakingSpace_IsReplacedWithSpace()

        Dim baseName As String = $"My{PathHelperTests.NonBreakingSpace}List"

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual("My List", result)
    End Sub

    ''' <summary>
    ''' Verifies that leading and trailing dots, commas, semicolons and spaces are trimmed.
    ''' </summary>
    '''
    ''' <param name="baseName">
    ''' The base name to normalize.
    ''' </param>
    '''
    ''' <param name="expected">
    ''' The expected normalized base name.
    ''' </param>
    <TestMethod>
    <DataRow(" Name ", "Name")>
    <DataRow("Name.", "Name")>
    <DataRow("Name...", "Name")>
    <DataRow(".Name", "Name")>
    <DataRow(" .,;Name;,. ", "Name")>
    Public Sub NormalizeBaseFileName_LeadingAndTrailingDelimiters_AreTrimmed(baseName As String, expected As String)

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that delimiters located inside the base name are preserved.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseFileName_InnerDelimiters_ArePreserved()

        Const baseName As String = "My.File, Name; 2026"

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual(baseName, result)
    End Sub

    ''' <summary>
    ''' Verifies that an already valid base name is returned unchanged.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseFileName_ValidName_IsReturnedUnchanged()

        Const baseName As String = "Movies 2026 (1080p) [Remux] - Part #1 & 2"

        Dim result As String = PathHelper.NormalizeBaseFileName(baseName)

        Assert.AreEqual(baseName, result)
    End Sub

    ''' <summary>
    ''' Verifies that a base name composed only of trimmable delimiters is normalized to an empty string.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseFileName_OnlyDelimiters_ReturnsEmptyString()

        Dim result As String = PathHelper.NormalizeBaseFileName(" ..,; ")

        Assert.AreEqual(String.Empty, result)
    End Sub

#End Region

#Region " NormalizeBaseDirPath "

    ''' <summary>
    ''' Verifies that <see langword="Nothing"/> and empty strings are normalized to an empty string.
    ''' </summary>
    '''
    ''' <param name="basePath">
    ''' The base path to normalize.
    ''' </param>
    <TestMethod>
    <DataRow(Nothing)>
    <DataRow("")>
    Public Sub NormalizeBaseDirPath_NullOrEmpty_ReturnsEmptyString(basePath As String)

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual(String.Empty, result)
    End Sub

    ''' <summary>
    ''' Verifies that an already valid directory path is returned unchanged.
    ''' </summary>
    '''
    ''' <param name="basePath">
    ''' The base path to normalize.
    ''' </param>
    <TestMethod>
    <DataRow("C:\Downloads")>
    <DataRow("C:\Downloads\Torrents (2026)")>
    <DataRow("\\server\share\Downloads")>
    <DataRow("Downloads\Torrents")>
    Public Sub NormalizeBaseDirPath_ValidPath_IsReturnedUnchanged(basePath As String)

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual(basePath, result)
    End Sub

    ''' <summary>
    ''' Verifies that trailing directory separators and delimiters are trimmed.
    ''' </summary>
    '''
    ''' <param name="basePath">
    ''' The base path to normalize.
    ''' </param>
    '''
    ''' <param name="expected">
    ''' The expected normalized base path.
    ''' </param>
    <TestMethod>
    <DataRow("C:\Downloads\", "C:\Downloads")>
    <DataRow("C:\Downloads\\", "C:\Downloads")>
    <DataRow("C:\Downloads/", "C:\Downloads")>
    <DataRow("C:\Downloads.. ", "C:\Downloads")>
    <DataRow("C:\Downloads\ ;,. \", "C:\Downloads")>
    <DataRow("\\server\share\Downloads\", "\\server\share\Downloads")>
    <DataRow("Downloads\Torrents\", "Downloads\Torrents")>
    Public Sub NormalizeBaseDirPath_TrailingDelimiters_AreTrimmed(basePath As String, expected As String)

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that the root of a drive is preserved as-is, including its directory separator.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseDirPath_DriveRoot_IsPreserved()

        Dim result As String = PathHelper.NormalizeBaseDirPath("C:\")

        Assert.AreEqual("C:\", result)
    End Sub

    ''' <summary>
    ''' Verifies that invalid path characters located after the root are replaced with hyphens,
    ''' while the drive colon of the root is preserved.
    ''' </summary>
    '''
    ''' <param name="basePath">
    ''' The base path to normalize.
    ''' </param>
    '''
    ''' <param name="expected">
    ''' The expected normalized base path.
    ''' </param>
    <TestMethod>
    <DataRow("C:\Down:loads", "C:\Down-loads")>
    <DataRow("C:\A*B\C?D", "C:\A-B\C-D")>
    <DataRow("C:\A""B\C|D", "C:\A-B\C-D")>
    <DataRow("C:\<Dir>", "C:\-Dir-")>
    Public Sub NormalizeBaseDirPath_InvalidCharacters_AreReplacedWithHyphen(basePath As String, expected As String)

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that surrounding white-spaces and line breaks (e.g. copied from the clipboard) are removed.
    ''' </summary>
    '''
    ''' <param name="suffix">
    ''' The white-space suffix appended to the path.
    ''' </param>
    <TestMethod>
    <DataRow(vbCrLf)>
    <DataRow(vbLf)>
    <DataRow(vbTab)>
    <DataRow(" " & vbCrLf)>
    Public Sub NormalizeBaseDirPath_SurroundingWhiteSpaces_AreRemoved(suffix As String)

        Dim basePath As String = $"  C:\Downloads\{suffix}"

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual("C:\Downloads", result)
    End Sub

    ''' <summary>
    ''' Verifies that non-breaking spaces are replaced with regular spaces.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseDirPath_NonBreakingSpace_IsReplacedWithSpace()

        Dim basePath As String = $"C:\My{PathHelperTests.NonBreakingSpace}Downloads"

        Dim result As String = PathHelper.NormalizeBaseDirPath(basePath)

        Assert.AreEqual("C:\My Downloads", result)
    End Sub

    ''' <summary>
    ''' Verifies that a white-space only path is normalized to an empty string.
    ''' </summary>
    <TestMethod>
    Public Sub NormalizeBaseDirPath_WhiteSpaceOnly_ReturnsEmptyString()

        Dim result As String = PathHelper.NormalizeBaseDirPath($"   {vbCrLf}")

        Assert.AreEqual(String.Empty, result)
    End Sub

#End Region

#Region " BuildTargetDirPath "

    ''' <summary>
    ''' Verifies that the target path is built in the form "{baseDirPath}\{baseFileName}\{index}".
    ''' </summary>
    '''
    ''' <param name="baseDirPath">
    ''' The base directory path.
    ''' </param>
    '''
    ''' <param name="baseFileName">
    ''' The base file name.
    ''' </param>
    '''
    ''' <param name="index">
    ''' The torrent index.
    ''' </param>
    '''
    ''' <param name="expected">
    ''' The expected target path.
    ''' </param>
    <TestMethod>
    <DataRow("C:\Downloads", "My List", 1, "C:\Downloads\My List\1")>
    <DataRow("C:\Downloads\", "My List", 2, "C:\Downloads\My List\2")>
    <DataRow("D:\Torrents\Incoming", "List", 10, "D:\Torrents\Incoming\List\10")>
    <DataRow("\\server\share\Downloads", "List", 3, "\\server\share\Downloads\List\3")>
    <DataRow("\\server\share", "List", 4, "\\server\share\List\4")>
    <DataRow("C:\Downloads", "Movies: Part 1?", 5, "C:\Downloads\Movies- Part 1-\5")>
    <DataRow("C:\Down:loads\", " List. ", 6, "C:\Down-loads\List\6")>
    Public Sub BuildTargetDirPath_ValidArguments_ReturnsExpectedPath(baseDirPath As String, baseFileName As String,
                                                                     index As Integer, expected As String)

        Dim result As String = PathHelper.BuildTargetDirPath(baseDirPath, baseFileName, index)

        Assert.AreEqual(expected, result)
    End Sub

    ''' <summary>
    ''' Verifies that a drive root base directory path does not produce a doubled directory separator.
    ''' </summary>
    '''
    ''' <param name="baseDirPath">
    ''' The base directory path.
    ''' </param>
    <TestMethod>
    <DataRow("C:\")>
    <DataRow("C:\\")>
    <DataRow("C:\ ")>
    Public Sub BuildTargetDirPath_DriveRoot_DoesNotDoubleSeparator(baseDirPath As String)

        Dim result As String = PathHelper.BuildTargetDirPath(baseDirPath, "List", 1)

        Assert.AreEqual("C:\List\1", result)
    End Sub

    ''' <summary>
    ''' Verifies that a base directory path copied from the clipboard with a trailing line break
    ''' and a trailing directory separator does not produce a doubled directory separator.
    ''' </summary>
    <TestMethod>
    Public Sub BuildTargetDirPath_TrailingLineBreak_DoesNotDoubleSeparator()

        Dim result As String = PathHelper.BuildTargetDirPath($"C:\Downloads\{vbCrLf}", "List", 1)

        Assert.AreEqual("C:\Downloads\List\1", result)
    End Sub

    ''' <summary>
    ''' Verifies that a <see langword="Nothing"/> base directory path throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildTargetDirPath_NullBaseDirPath_ThrowsArgumentNullException()

        Dim ex As ArgumentNullException =
            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PathHelper.BuildTargetDirPath(Nothing, "List", 1))

        Assert.AreEqual("baseDirPath", ex.ParamName)
    End Sub

    ''' <summary>
    ''' Verifies that a <see langword="Nothing"/> base file name throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub BuildTargetDirPath_NullBaseFileName_ThrowsArgumentNullException()

        Dim ex As ArgumentNullException =
            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() PathHelper.BuildTargetDirPath("C:\Downloads", Nothing, 1))

        Assert.AreEqual("baseFileName", ex.ParamName)
    End Sub

    ''' <summary>
    ''' Verifies that an index lower than 1 throws <see cref="ArgumentOutOfRangeException"/>.
    ''' </summary>
    '''
    ''' <param name="index">
    ''' The torrent index.
    ''' </param>
    <TestMethod>
    <DataRow(0)>
    <DataRow(-1)>
    <DataRow(Integer.MinValue)>
    Public Sub BuildTargetDirPath_IndexLowerThanOne_ThrowsArgumentOutOfRangeException(index As Integer)

        Dim ex As ArgumentOutOfRangeException =
            Assert.ThrowsExactly(Of ArgumentOutOfRangeException)(Sub() PathHelper.BuildTargetDirPath("C:\Downloads", "List", index))

        Assert.AreEqual("index", ex.ParamName)
    End Sub

    ''' <summary>
    ''' Verifies that a base directory path that resolves to an empty string after normalization
    ''' throws <see cref="ArgumentException"/>.
    ''' </summary>
    '''
    ''' <param name="baseDirPath">
    ''' The base directory path.
    ''' </param>
    <TestMethod>
    <DataRow("")>
    <DataRow("   ")>
    <DataRow(vbCrLf)>
    <DataRow("...")>
    Public Sub BuildTargetDirPath_EmptyNormalizedBaseDirPath_ThrowsArgumentException(baseDirPath As String)

        Dim ex As ArgumentException =
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PathHelper.BuildTargetDirPath(baseDirPath, "List", 1))

        Assert.AreEqual("baseDirPath", ex.ParamName)
    End Sub

    ''' <summary>
    ''' Verifies that a base file name that resolves to an empty string after normalization
    ''' throws <see cref="ArgumentException"/>.
    ''' </summary>
    '''
    ''' <param name="baseFileName">
    ''' The base file name.
    ''' </param>
    <TestMethod>
    <DataRow("")>
    <DataRow("   ")>
    <DataRow("...")>
    <DataRow(" .,; ")>
    Public Sub BuildTargetDirPath_EmptyNormalizedBaseFileName_ThrowsArgumentException(baseFileName As String)

        Dim ex As ArgumentException =
            Assert.ThrowsExactly(Of ArgumentException)(Sub() PathHelper.BuildTargetDirPath("C:\Downloads", baseFileName, 1))

        Assert.AreEqual("baseFileName", ex.ParamName)
    End Sub

#End Region

End Class

#End Region
