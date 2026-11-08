#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Collections.Generic

Imports DevCase.Runtime.TypeComparers

Imports Microsoft.VisualStudio.TestTools.UnitTesting

#End Region

#Region " StringNaturalComparer Tests "

''' <summary>
''' Unit tests for the <see cref="StringNaturalComparer"/> class and the <see cref="ComparerResult"/> enumeration.
''' </summary>
<TestClass>
Public NotInheritable Class StringNaturalComparerTests

#Region " ComparerResult "

    ''' <summary>
    ''' Verifies the underlying values of the <see cref="ComparerResult"/> enumeration members.
    ''' </summary>
    <TestMethod>
    Public Sub ComparerResult_Values_MatchIComparerContract()

        Assert.AreEqual(-1, CInt(ComparerResult.LessThan))
        Assert.AreEqual(0, CInt(ComparerResult.Equals))
        Assert.AreEqual(1, CInt(ComparerResult.GreaterThan))
    End Sub

#End Region

#Region " Null Handling "

    ''' <summary>
    ''' Verifies that two <see langword="Nothing"/> strings are considered equal.
    ''' </summary>
    <TestMethod>
    Public Sub Compare_BothNull_ReturnsEquals()

        Dim comparer As New StringNaturalComparer()

        Assert.AreEqual(CInt(ComparerResult.Equals), comparer.Compare(Nothing, Nothing))
    End Sub

    ''' <summary>
    ''' Verifies that a <see langword="Nothing"/> string precedes a non-null string.
    ''' </summary>
    <TestMethod>
    Public Sub Compare_FirstNull_ReturnsLessThan()

        Dim comparer As New StringNaturalComparer()

        Assert.AreEqual(CInt(ComparerResult.LessThan), comparer.Compare(Nothing, "a"))
    End Sub

    ''' <summary>
    ''' Verifies that a non-null string follows a <see langword="Nothing"/> string.
    ''' </summary>
    <TestMethod>
    Public Sub Compare_SecondNull_ReturnsGreaterThan()

        Dim comparer As New StringNaturalComparer()

        Assert.AreEqual(CInt(ComparerResult.GreaterThan), comparer.Compare("a", Nothing))
    End Sub

#End Region

#Region " Natural Ordering "

    ''' <summary>
    ''' Verifies that numeric parts are compared by their numeric value instead of character by character.
    ''' </summary>
    '''
    ''' <param name="lower">
    ''' The string expected to precede <paramref name="greater"/>.
    ''' </param>
    '''
    ''' <param name="greater">
    ''' The string expected to follow <paramref name="lower"/>.
    ''' </param>
    <TestMethod>
    <DataRow("file2.txt", "file10.txt")>
    <DataRow("file9", "file10")>
    <DataRow("1", "2")>
    <DataRow("9", "10")>
    <DataRow("List 2 - Part 9", "List 2 - Part 10")>
    <DataRow("a", "b")>
    Public Sub Compare_NaturalOrder_IsRespected(lower As String, greater As String)

        Dim comparer As New StringNaturalComparer()

        Assert.IsLessThan(0, comparer.Compare(lower, greater))
        Assert.IsGreaterThan(0, comparer.Compare(greater, lower))
    End Sub

    ''' <summary>
    ''' Verifies that identical strings, and strings that only differ by letter casing, are considered equal.
    ''' </summary>
    '''
    ''' <param name="first">
    ''' The first string.
    ''' </param>
    '''
    ''' <param name="second">
    ''' The second string.
    ''' </param>
    <TestMethod>
    <DataRow("", "")>
    <DataRow("file10.txt", "file10.txt")>
    <DataRow("FILE10.TXT", "file10.txt")>
    Public Sub Compare_EquivalentStrings_ReturnsEquals(first As String, second As String)

        Dim comparer As New StringNaturalComparer()

        Assert.AreEqual(CInt(ComparerResult.Equals), comparer.Compare(first, second))
    End Sub

    ''' <summary>
    ''' Verifies that sorting a list of file names produces the natural sort order.
    ''' </summary>
    <TestMethod>
    Public Sub Compare_SortingFileNames_ProducesNaturalOrder()

        Dim fileNames As New List(Of String) From {
            "file10.txt", "file2.txt", "File3.txt", "file1.txt", "file20.txt", "file100.txt"
        }

        fileNames.Sort(New StringNaturalComparer())

        Dim expected As String() = {
            "file1.txt", "file2.txt", "File3.txt", "file10.txt", "file20.txt", "file100.txt"
        }
        CollectionAssert.AreEqual(expected, fileNames)
    End Sub

#End Region

End Class

#End Region
