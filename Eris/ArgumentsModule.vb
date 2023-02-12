Imports System.IO

Friend Module ArgumentsModule

    Enum BytePadType
        ZeroByte
        NullByte
        RandomByte
    End Enum

    Private _TargetFile As FileInfo
    Private _BytesToPad As Long = 0
    Private _BytePadType As BytePadType = BytePadType.NullByte

    ''' <summary>
    ''' Determines if the program is allowed to use random ASCII bytes as padding bytes, instead of zeroes.
    ''' </summary>
    ''' <returns>Returns True if it's allowed to use random ASCII bytes, False if it's zero-based padding.</returns>
    Friend ReadOnly Property PaddingType As BytePadType
        Get
            Return _BytePadType
        End Get
    End Property

    ''' <summary>
    ''' Determines the size of a padded bytes to be added.
    ''' </summary>
    ''' <returns>Returns the length of a bytes to be padded.</returns>
    Friend ReadOnly Property BytesToPad As Long
        Get
            Return _BytesToPad
        End Get
    End Property

    ''' <summary>
    ''' Determines the target file's info.
    ''' </summary>
    ''' <returns>Returns a target <see cref="FileInfo"/> object.</returns>
    Friend ReadOnly Property TargetFile As FileInfo
        Get
            Return _TargetFile
        End Get
    End Property

    ''' <summary>
    ''' Checks and loads values provided by the command line arguments.
    ''' </summary>
    ''' <param name="args">The command line arguments.</param>
    ''' <returns>Returns <see langword="True"/> if the arguments were validated and loaded its values. <see langword="False"/> otherwise, and prints an error message.</returns>
    Friend Function CheckArguments(args() As String) As Boolean
        If args.Length = 0 OrElse args(0).Equals("-h") Then
            ShowHelp()
            Return False
        End If

        If args.Length < 2 Then
            console.PrintLine("Insufficient arguments passed. Run -h to show help.", PrintType.Warning)
            Return False
        End If

        Try
            For i = 0 To args.Length - 1
                Select Case args(i)
                    Case "-path"
                        If ((i + 1) >= args.Length) Then
                            console.PrintLine("No file specified.", PrintType.Warning)
                            Return False
                        End If
                        Dim file As String = args(i + 1)
                        If Not IO.File.Exists(file) Then
                            console.PrintLine("Couldn't find the specified file or it was a directory.", PrintType.Warning)
                            Return False
                        End If
                        _TargetFile = New FileInfo(file)
                        i += 1

                    Case "-sizeGB"
                        If Not CheckPadBytes(IIf((i + 1) >= args.Length, "", args(i + 1)), True) Then
                            Return False
                        End If
                        i += 1
                    Case "-sizeMB"
                        If Not CheckPadBytes(IIf((i + 1) >= args.Length, "", args(i + 1))) Then
                            Return False
                        End If
                        i += 1
                    Case "-n"
                        _BytePadType = BytePadType.NullByte
                    Case "-z"
                        _BytePadType = BytePadType.ZeroByte
                    Case "-r"
                        _BytePadType = BytePadType.RandomByte
                    Case Else
                        console.PrintLine("Invalid argument specified. Run -h to show help.", PrintType.Warning)
                        Return False
                End Select
            Next
        Catch ex As Exception
            console.Print("Parse argument failed. Run -h to show help. (E: " & ex.Message & ")", PrintType.Error)
            Return False
        End Try


        If _BytesToPad = 0 Then _BytesToPad = 104857600 ' Set as 100 MB size.
        Return True
    End Function

    ''' <summary>
    ''' Performs a validation of given padded byte length, and inserts its values once validated.
    ''' </summary>
    ''' <param name="byt">A string of given padded byte length.</param>
    ''' <param name="isGigaByte">True will maximize the byte length in gigabytes (GB). False otherwise. Default was False</param>
    ''' <returns>Returns True if the given byte length was validated and inserts its value to <see cref="BytesToPad"/>. False otherwise, and prints an error message.</returns>
    Private Function CheckPadBytes(byt As String, Optional isGigaByte As Boolean = False) As Boolean
        If String.IsNullOrWhiteSpace(byt) Then
            console.PrintLine("No pad byte length specified.", PrintType.Warning)
            Return False
        End If

        If Not Long.TryParse(byt, _BytesToPad) OrElse _BytesToPad <= 0 Then
            console.PrintLine("[!]: Pad byte length is invalid.", PrintType.Warning)
            Return False
        End If

        Dim _PredefinedByteMultiplication As Long = 1024 * 1024 ' Set as MB first
        If (isGigaByte) Then _PredefinedByteMultiplication *= 1024
        _BytesToPad = _PredefinedByteMultiplication * _BytesToPad

        Return True
    End Function

    ''' <summary>
    ''' Converts the raw byte length into human-readable length.
    ''' </summary>
    ''' <param name="byteLen">A long number value.</param>
    ''' <returns>Returns a human-readable byte length string.</returns>
    Function CalculateBytes(byteLen As Long) As String
        Try
            If byteLen >= 1099511627775 Then
                Return Math.Round(Convert.ToDouble(byteLen / CDbl(1099511627775)), 2).ToString() & " TB"
            End If
            If byteLen >= 1073741824 AndAlso byteLen < 1099511627775 Then
                Return Math.Round(Convert.ToDouble(byteLen / CDbl(1073741824)), 2).ToString() & " GB"
            End If
            If byteLen >= 1048575 AndAlso byteLen < 1073741824 Then
                Return Math.Round(Convert.ToDouble(byteLen / CDbl(1048576)), 2).ToString() & " MB"
            End If
            If byteLen >= 1024 AndAlso byteLen < 1048575 Then
                Return Math.Round(Convert.ToDouble(byteLen / CDbl(1024)), 2).ToString() & " KB"
            End If
            If byteLen < 1024 Then
                Return Math.Round(Convert.ToDouble(byteLen), 2).ToString() & " bytes"
            End If
            Return "0 bytes"
        Catch ex As Exception
            Return "0 bytes"
        End Try
    End Function
End Module
