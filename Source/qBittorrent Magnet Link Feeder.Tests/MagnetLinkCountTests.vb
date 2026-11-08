#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " Magnet Link Count Tests "

''' <summary>
''' Unit tests for the <see cref="AutomationHelper.CountMagnetLinks(String)"/> function.
''' <para></para>
''' The expected values mimic the way qBittorrent parses the content of the "Download from URLs" dialog:
''' the text is split by line feeds, each line is trimmed, empty lines are ignored
''' and duplicated (trimmed) URLs are discarded using a case-sensitive comparison.
''' </summary>
<TestClass>
Public NotInheritable Class MagnetLinkCountTests

#Region " Constants "

    ''' <summary>
    ''' A sample magnet link.
    ''' </summary>
    Private Const MagnetA As String = "magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567&dn=Example+A"

    ''' <summary>
    ''' A sample magnet link, different from <see cref="MagnetA"/>.
    ''' </summary>
    Private Const MagnetB As String = "magnet:?xt=urn:btih:89ABCDEF0123456789ABCDEF0123456789ABCDEF&dn=Example+B"

    ''' <summary>
    ''' A sample magnet link, different from <see cref="MagnetA"/> and <see cref="MagnetB"/>.
    ''' </summary>
    Private Const MagnetC As String = "magnet:?xt=urn:btih:FEDCBA9876543210FEDCBA9876543210FEDCBA98&dn=Example+C"

#End Region

#Region " Argument Validation "

    ''' <summary>
    ''' Verifies that a <see langword="Nothing"/> text throws <see cref="ArgumentNullException"/>.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_NullText_ThrowsArgumentNullException()

        Dim ex As ArgumentNullException =
            Assert.ThrowsExactly(Of ArgumentNullException)(Sub() AutomationHelper.CountMagnetLinks(Nothing))

        Assert.AreEqual("text", ex.ParamName)
    End Sub

#End Region

#Region " Empty Input "

    ''' <summary>
    ''' Verifies that an empty or white-space only text contains no magnet links.
    ''' </summary>
    '''
    ''' <param name="text">
    ''' The text to parse.
    ''' </param>
    <TestMethod>
    <DataRow("")>
    <DataRow("   ")>
    <DataRow(vbTab)>
    <DataRow(vbCrLf)>
    <DataRow(vbLf & vbLf & vbLf)>
    <DataRow(" " & vbCrLf & vbTab & vbCrLf & " ")>
    Public Sub CountMagnetLinks_EmptyOrWhiteSpaceText_ReturnsZero(text As String)

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(0, result)
    End Sub

#End Region

#Region " Line Splitting "

    ''' <summary>
    ''' Verifies that a single magnet link is counted.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_SingleMagnetLink_ReturnsOne()

        Dim result As Integer = AutomationHelper.CountMagnetLinks(MagnetLinkCountTests.MagnetA)

        Assert.AreEqual(1, result)
    End Sub

    ''' <summary>
    ''' Verifies that magnet links are counted regardless of the line break style.
    ''' </summary>
    '''
    ''' <param name="lineBreak">
    ''' The line break used to join the magnet links.
    ''' </param>
    <TestMethod>
    <DataRow(vbCrLf)>
    <DataRow(vbLf)>
    Public Sub CountMagnetLinks_MultipleMagnetLinks_AreCountedForEachLineBreakStyle(lineBreak As String)

        Dim text As String = String.Join(lineBreak, {MagnetLinkCountTests.MagnetA, MagnetLinkCountTests.MagnetB, MagnetLinkCountTests.MagnetC})

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(3, result)
    End Sub

    ''' <summary>
    ''' Verifies that magnet links are counted when mixed line break styles are used.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_MixedLineBreaks_AreCounted()

        Dim text As String = $"{MagnetLinkCountTests.MagnetA}{vbCrLf}{MagnetLinkCountTests.MagnetB}{vbLf}{MagnetLinkCountTests.MagnetC}"

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(3, result)
    End Sub

    ''' <summary>
    ''' Verifies that empty lines and a trailing line break do not affect the count.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_EmptyLinesAndTrailingLineBreak_AreIgnored()

        Dim text As String = $"{vbCrLf}{MagnetLinkCountTests.MagnetA}{vbCrLf}{vbCrLf}   {vbCrLf}{MagnetLinkCountTests.MagnetB}{vbCrLf}"

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(2, result)
    End Sub

#End Region

#Region " Trimming "

    ''' <summary>
    ''' Verifies that magnet links surrounded by white-spaces are counted,
    ''' because qBittorrent trims each line before processing it.
    ''' </summary>
    '''
    ''' <param name="prefix">
    ''' The white-space prefix of the line.
    ''' </param>
    '''
    ''' <param name="suffix">
    ''' The white-space suffix of the line.
    ''' </param>
    <TestMethod>
    <DataRow("  ", "")>
    <DataRow(vbTab, "")>
    <DataRow("", "  ")>
    <DataRow(vbTab, vbTab)>
    Public Sub CountMagnetLinks_SurroundingWhiteSpaces_AreCounted(prefix As String, suffix As String)

        Dim text As String = $"{prefix}{MagnetLinkCountTests.MagnetA}{suffix}"

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(1, result)
    End Sub

#End Region

#Region " Duplicates "

    ''' <summary>
    ''' Verifies that exact duplicated magnet links are counted only once.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_ExactDuplicates_AreCountedOnce()

        Dim text As String = String.Join(vbCrLf, {MagnetLinkCountTests.MagnetA, MagnetLinkCountTests.MagnetB, MagnetLinkCountTests.MagnetA, MagnetLinkCountTests.MagnetA})

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(2, result)
    End Sub

    ''' <summary>
    ''' Verifies that duplicated magnet links that only differ by surrounding white-spaces are counted only once,
    ''' because qBittorrent discards duplicates after trimming each line.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_DuplicatesDifferingByWhiteSpaces_AreCountedOnce()

        Dim text As String = String.Join(vbCrLf, {
            MagnetLinkCountTests.MagnetA,
            $"{MagnetLinkCountTests.MagnetA} ",
            $"  {MagnetLinkCountTests.MagnetA}",
            $"{vbTab}{MagnetLinkCountTests.MagnetA}{vbTab}"
        })

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(1, result)
    End Sub

    ''' <summary>
    ''' Verifies that magnet links that only differ by letter casing are counted as different links,
    ''' because qBittorrent discards duplicates using a case-sensitive comparison.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_DuplicatesDifferingByCase_AreCountedSeparately()

        Dim text As String = String.Join(vbCrLf, {MagnetLinkCountTests.MagnetA, MagnetLinkCountTests.MagnetA.ToUpperInvariant()})

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(2, result)
    End Sub

#End Region

#Region " Prefix Matching "

    ''' <summary>
    ''' Verifies that the magnet scheme is matched case-insensitively.
    ''' </summary>
    '''
    ''' <param name="line">
    ''' The line to parse.
    ''' </param>
    <TestMethod>
    <DataRow("magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    <DataRow("MAGNET:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    <DataRow("Magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    Public Sub CountMagnetLinks_MagnetSchemeCasing_IsIgnored(line As String)

        Dim result As Integer = AutomationHelper.CountMagnetLinks(line)

        Assert.AreEqual(1, result)
    End Sub

    ''' <summary>
    ''' Verifies that lines that are not magnet links are not counted.
    ''' </summary>
    '''
    ''' <param name="line">
    ''' The line to parse.
    ''' </param>
    <TestMethod>
    <DataRow("http://example.com/file.torrent")>
    <DataRow("https://example.com/magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    <DataRow("Some plain text")>
    <DataRow("magnet")>
    <DataRow("magnet?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    <DataRow("xmagnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    <DataRow("Link: magnet:?xt=urn:btih:0123456789ABCDEF0123456789ABCDEF01234567")>
    Public Sub CountMagnetLinks_NonMagnetLines_AreNotCounted(line As String)

        Dim result As Integer = AutomationHelper.CountMagnetLinks(line)

        Assert.AreEqual(0, result)
    End Sub

    ''' <summary>
    ''' Verifies that only the magnet links are counted in a text that also contains other kinds of lines.
    ''' </summary>
    <TestMethod>
    Public Sub CountMagnetLinks_MixedContent_CountsOnlyMagnetLinks()

        Dim text As String = String.Join(vbCrLf, {
            "# My magnet list",
            MagnetLinkCountTests.MagnetA,
            "http://example.com/file.torrent",
            String.Empty,
            MagnetLinkCountTests.MagnetB,
            "Some plain text"
        })

        Dim result As Integer = AutomationHelper.CountMagnetLinks(text)

        Assert.AreEqual(2, result)
    End Sub

#End Region

End Class

#End Region
