#Region " Option Statements "

Option Strict On
Option Explicit On
Option Infer Off

#End Region

#Region " Imports "

Imports System.Diagnostics
Imports System.IO

#End Region

Friend Module PathHelper

#Region " Public / Friend Methods "

    ''' <summary>
    ''' Normalizes a file or directory base name by replacing invalid Windows characters with hyphens and trimming specific delimiters.
    ''' </summary>
    ''' 
    ''' <param name="baseName">
    ''' The raw base name to normalize.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The normalized base name string.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function NormalizeBaseFileName(baseName As String) As String

        If String.IsNullOrEmpty(baseName) Then
            Return String.Empty
        End If

        Dim trimChars As Char() = {
            "."c, ","c, ";"c, " "c
        }

        Dim normalized As String =
            baseName.Replace(Strings.ChrW(160), " "c).
                     Replace("/"c, "-"c).
                     Replace("\"c, "-"c).
                     Replace("*"c, "-"c).
                     Replace(""""c, "-"c).
                     Replace("|"c, "-"c).
                     Replace("?"c, "-"c).
                     Replace(":"c, "-"c).
                     Replace("<"c, "-"c).
                     Replace(">"c, "-"c).
                     Trim(trimChars)

        Return normalized
    End Function

    ''' <summary>
    ''' Normalizes a directory base path by replacing invalid Windows characters with hyphens and trimming specific trailing delimiters.
    ''' </summary>
    ''' 
    ''' <param name="basePath">
    ''' The raw base path to normalize.
    ''' </param>
    ''' 
    ''' <returns>
    ''' The normalized base path string.
    ''' </returns>
    <DebuggerStepThrough>
    Friend Function NormalizeBaseDirPath(basePath As String) As String

        If String.IsNullOrEmpty(basePath) Then
            Return String.Empty
        End If

        ' Remove surrounding white-spaces and line breaks (e.g. a trailing CR/LF copied from the clipboard),
        ' otherwise they would prevent the root detection and the trimming of trailing directory separators.
        '
        ' Characters that can never be part of a path root are replaced here, before the root detection,
        ' because on .NET Framework 'Path.IsPathRooted' and 'Path.GetPathRoot' throw an ArgumentException
        ' ("Illegal characters in path") when the path contains any of these characters.
        basePath = basePath.Replace(Strings.ChrW(160), " "c).
                            Replace(""""c, "-"c).
                            Replace("|"c, "-"c).
                            Replace("<"c, "-"c).
                            Replace(">"c, "-"c).
                            Trim()

        Dim root As String = Nothing
        If Path.IsPathRooted(basePath) Then
            root = Path.GetPathRoot(basePath)
        End If

        Dim dirPath As String = basePath
        If Not String.IsNullOrEmpty(root) Then
            dirPath = dirPath.Substring(root.Length)
        End If

        Dim trailingChars As Char() = {
            "."c, ","c, ";"c, " "c, "\"c, "/"c
        }

        Dim normalizedDirPath As String =
            dirPath.Replace("*"c, "-"c).
                     Replace("?"c, "-"c).
                     Replace(":"c, "-"c).
                     TrimEnd(trailingChars).
                     Trim()

        Return $"{root}{normalizedDirPath}"
    End Function

    ''' <summary>
    ''' Builds the final download directory path for a torrent, in the form:
    ''' <c>{baseDirPath}\{baseFileName}\{index}</c>
    ''' <para></para>
    ''' Both <paramref name="baseDirPath"/> and <paramref name="baseFileName"/> are normalized
    ''' by <see cref="NormalizeBaseDirPath"/> and <see cref="NormalizeBaseFileName"/> respectively.
    ''' </summary>
    '''
    ''' <param name="baseDirPath">
    ''' The raw base directory path (typically the default download path shown in the qBittorrent dialog).
    ''' </param>
    '''
    ''' <param name="baseFileName">
    ''' The raw base file name (typically the source text file name without extension).
    ''' </param>
    '''
    ''' <param name="index">
    ''' The one-based torrent index, used as the last directory name to avoid naming conflicts.
    ''' </param>
    '''
    ''' <returns>
    ''' The resulting download directory path.
    ''' </returns>
    '''
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="baseDirPath"/> or <paramref name="baseFileName"/> is <see langword="Nothing"/>.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when <paramref name="index"/> is less than 1.
    ''' </exception>
    '''
    ''' <exception cref="ArgumentException">
    ''' Thrown when <paramref name="baseDirPath"/> or <paramref name="baseFileName"/>
    ''' resolves to an empty string after normalization.
    ''' </exception>
    <DebuggerStepThrough>
    Friend Function BuildTargetDirPath(baseDirPath As String, baseFileName As String, index As Integer) As String

        If baseDirPath Is Nothing Then
            Throw New ArgumentNullException(NameOf(baseDirPath))
        End If

        If baseFileName Is Nothing Then
            Throw New ArgumentNullException(NameOf(baseFileName))
        End If

        If index < 1 Then
            Throw New ArgumentOutOfRangeException(NameOf(index), index, "Index must be greater than or equal to 1.")
        End If

        Dim normalizedBaseDirPath As String = PathHelper.NormalizeBaseDirPath(baseDirPath)
        If normalizedBaseDirPath.Length = 0 Then
            Throw New ArgumentException($"The base directory path resolves to an empty string after normalization: '{baseDirPath}'", NameOf(baseDirPath))
        End If

        Dim normalizedBaseFileName As String = PathHelper.NormalizeBaseFileName(baseFileName)
        If normalizedBaseFileName.Length = 0 Then
            Throw New ArgumentException($"The base file name resolves to an empty string after normalization: '{baseFileName}'", NameOf(baseFileName))
        End If

        ' A drive or share root (e.g. "C:\") already ends with a directory separator.
        Dim endsWithSeparator As Boolean = normalizedBaseDirPath.EndsWith("\", StringComparison.Ordinal) OrElse
                                           normalizedBaseDirPath.EndsWith("/", StringComparison.Ordinal)

        Dim separator As String = If(endsWithSeparator, String.Empty, "\")

        Return $"{normalizedBaseDirPath}{separator}{normalizedBaseFileName}\{index}"
    End Function

#End Region

End Module
