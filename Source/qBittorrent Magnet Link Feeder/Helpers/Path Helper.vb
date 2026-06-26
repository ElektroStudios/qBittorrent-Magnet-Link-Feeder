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

        Dim root As String = Nothing
        If Path.IsPathRooted(basePath) Then
            root = Path.GetPathRoot(basePath)
        End If

        Dim dirPath As String = basePath
        If Not String.IsNullOrEmpty(root) Then
            dirPath = dirPath.Substring(root.Length)
        End If

        Dim trailingChars As Char() = {
            "."c, ","c, ";"c, " "c, "\"c
        }

        Dim normalizedDirPath As String =
            dirPath.Replace(Strings.ChrW(160), " "c).
                     Replace("*"c, "-"c).
                     Replace(""""c, "-"c).
                     Replace("|"c, "-"c).
                     Replace("?"c, "-"c).
                     Replace(":"c, "-"c).
                     Replace("<"c, "-"c).
                     Replace(">"c, "-"c).
                     TrimEnd(trailingChars).
                     Trim()

        Return $"{root}{normalizedDirPath}"
    End Function

#End Region

End Module
