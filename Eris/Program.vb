Imports System.IO

Module Program

    ''' <summary>
    ''' A variable to use <see cref="System.Console"/>'s extended methods from <see cref="CommandLineEx"/>.
    ''' </summary>
    Friend console As Console

    ''' <summary>
    ''' A variable used to generate randomized bytes.
    ''' </summary>
    Friend rnd As New Random()

    ''' <summary>
    ''' The main entry of program.
    ''' </summary>
    ''' <param name="args">A command line arguments provided by the user.</param>
    Sub Main(args As String())

        ' Randomize bytes as initialization.
        RandomizeBytes(Nothing)
        ShowTitle()

        ' Prematurely exit a program if arguments are invalid or user abortss.
        If Not CheckArguments(args) Then End
        PrintInfos()

        ' Check for drive space
        console.PrintLine("Checking disk space...", PrintType.Verbose)
        Dim drive As New DriveInfo(TargetFile.FullName)
        Dim fileSize As Long = TargetFile.Length
        Dim totalLen As Long = BytesToPad + fileSize
        Dim totalAvailableSize As Long = drive.AvailableFreeSpace - totalLen
        Dim perce As Double = Math.Round(totalAvailableSize / drive.AvailableFreeSpace * 100, 2)
        If perce < 5.0 Then
            ' Prematurely exit when expected storage free space percentage is lower than 5%
            console.PrintLine("Available free space of '" & drive.Name & "' is too low.", PrintType.Error)
            End
        End If
        console.PrintLine("Disk space check completed. (" & CalculateBytes(totalAvailableSize) & " remaining after the padding.)", PrintType.Verbose)

        If Not CreateBackup() Then End

        Try
            console.PrintLine("Start file padding...", PrintType.Info)
            TargetFile.Delete() ' Delete the original file, since a backup one was created.

            Using paddedFile As FileStream = TargetFile.Open(FileMode.Create, FileAccess.ReadWrite)
                WriteAllBytes(paddedFile, totalLen - 1)

                Dim backupName As New FileInfo(TargetFile.FullName & ".bak")
                Using realFile As FileStream = backupName.Open(FileMode.Open, FileAccess.Read)
                    Dim len As Long = realFile.Length
                    Dim totalBytesWritten As Long = 0
                    Dim buffer As Byte() = New Byte(1023) {}
                    Dim readed As Integer = realFile.Read(buffer, 0, buffer.Length)

                    ' Start the concatenation.
                    While readed > 0
                        paddedFile.Write(buffer, 0, readed)
                        paddedFile.Flush()
                        totalBytesWritten += readed
                        console.PrintProgressBar(totalBytesWritten, len, "Writing real file's bytes to the padded file.")
                        If readed > 0 Then readed = realFile.Read(buffer, 0, buffer.Length)
                    End While
                End Using
            End Using

            ' Cleanup
            GC.Collect()
            Console.WriteLine()
            console.PrintLine("File padding complete.", PrintType.Info)
        Catch ex As Exception
            Console.WriteLine()
            console.PrintLine("Couldn't perform a task due to an error. E: " & ex.ToString(), PrintType.Error)
            End
        End Try
    End Sub

    ''' <summary>
    ''' Randomize bytes in a random iterations.
    ''' </summary>
    ''' <param name="byt">A byte array to be inserted with random numbers. 
    ''' If the value is <see langword="Nothing"/>, byte randomization still occur but nothing to be return to this parameter.</param>
    Private Sub RandomizeBytes(byt As Byte())
        For i = 0 To rnd.Next(2, 20)
            If byt Is Nothing Then
                rnd.NextBytes(New Byte(16) {})
                Continue For
            End If
            rnd.NextBytes(byt)
        Next
    End Sub

    ''' <summary>
    ''' Initiates file enlargement and filling up, based on <see cref="ArgumentsModule.PaddingType"/>.
    ''' </summary>
    ''' <param name="fs">A current <see cref="FileStream"/> object.</param>
    ''' <param name="lenToPad">A non-zero, non-negative value to determine for file enlargement.</param>
    Private Sub WriteAllBytes(fs As FileStream, lenToPad As Long)
        If fs Is Nothing Then Throw New ArgumentNullException(NameOf(fs))
        If lenToPad < 1 Then Throw New ArgumentException("A value of " & NameOf(lenToPad) & " is invalid.")
        ' Seek will move the position to the specified length by 'totalLen'
        ' if the file's size is less than 'totalLen', a possible file enlargement
        ' may occur once the fs.WriteByte was called.
        fs.Seek(lenToPad, SeekOrigin.Begin)

        Dim byt(1023) As Byte
        Select Case PaddingType
            Case BytePadType.NullByte
                fs.WriteByte(0)
                fs.Flush()
                fs.Position = 0
                console.PrintLine("Null byte padding finished.", PrintType.Info)
                Return
            Case BytePadType.ZeroByte
                For i = 0 To 1023
                    byt(i) = 48
                Next
            Case BytePadType.RandomByte
                RandomizeBytes(byt)
        End Select

        console.PrintLine("Non-null byte padding will be filled.", PrintType.Verbose)
        fs.WriteByte(IIf(PaddingType = BytePadType.ZeroByte, 48, rnd.Next(0, 255)))
        fs.Flush()
        fs.Position = 0

        Dim size As Long = fs.Length
        Dim segments As Long = size / byt.Length
        Dim lastSegment As Long = size Mod byt.Length

        Using writer As New BinaryWriter(fs, Text.Encoding.ASCII, True)
            ' Write bytes in iteration segments.
            For i = 0 To segments - 1
                If PaddingType = BytePadType.RandomByte Then RandomizeBytes(byt)
                writer.Write(byt)
            Next

            ' Last segmentation
            console.PrintLine("Finalizing file filling segmentation...", PrintType.Verbose)
            Dim lasSegByte As Byte() = New Byte(lastSegment) {}
            If PaddingType = BytePadType.RandomByte Then
                RandomizeBytes(byt)
            Else
                Array.Copy(byt, lasSegByte, lastSegment)
            End If

            writer.Write(lasSegByte)
        End Using

        fs.Position = 0
        console.PrintLine("File pad filling finished.", PrintType.Info)
    End Sub

    ''' <summary>
    ''' Creates a backup copy of a current <see cref="TargetFile"/> object.
    ''' </summary>
    ''' <returns>Returns <see langword="True"/> if the backup succeed. <see langword="False"/> otherwise, and error message will be printed.</returns>
    Private Function CreateBackup() As Boolean
        console.PrintLine("Creating backup...", PrintType.Verbose)
        Dim backupFileInfo As New FileInfo(TargetFile.FullName & ".bak")
        If backupFileInfo.Exists AndAlso Not console.Decision("Specified file already exists! Do you want to overwrite it?") Then
            console.PrintLine("User aborted", PrintType.Warning)
            Return False
        End If

        Try
            backupFileInfo.Delete()
            console.PrintLine("Attempting to open a file...", PrintType.Verbose)
            Using sauce As FileStream = TargetFile.Open(FileMode.Open, FileAccess.Read)
                console.PrintLine("Target file opened.", PrintType.Info, True, True)
                console.PrintLine("Attempting to create a backup...", PrintType.Verbose)
                Using dest As FileStream = backupFileInfo.Open(FileMode.Create, FileAccess.Write)
                    console.PrintLine("Copying file's content to backup file...", PrintType.Verbose, True, True)
                    Dim len As Long = sauce.Length
                    Dim totalBytesWritten As Long = 0
                    dest.SetLength(len)
                    dest.Position = 0
                    sauce.Position = 0

                    Dim buffer As Byte() = New Byte(1023) {}
                    Dim readed As Integer = sauce.Read(buffer, 0, buffer.Length)
                    While readed > 0
                        dest.Write(buffer, 0, readed)
                        dest.Flush()
                        totalBytesWritten += readed
                        console.PrintProgressBar(totalBytesWritten, len, "Copying file's content to backup file...")
                        If readed > 0 Then readed = sauce.Read(buffer, 0, buffer.Length)
                    End While
                End Using
            End Using
        Catch ex As Exception
            Console.WriteLine()
            console.PrintLine("Failed to create a backup. E: " & ex.Message, PrintType.Warning)
            Return False
        End Try
        Console.WriteLine()
        console.PrintLine("Backup created. Location: " & backupFileInfo.FullName, PrintType.Info)
        Return True
    End Function

    ''' <summary>
    ''' Show the help text.
    ''' </summary>
    Sub ShowHelp()
        Console.WriteLine(My.Resources.Help)
    End Sub

    ''' <summary>
    ''' Show the title of a program and disclaimer.
    ''' </summary>
    Sub ShowTitle()
        Console.WriteLine(My.Resources.Title)
        console.PrintLine(My.Resources.Disclaimer, PrintType.Warning, False)
        Console.WriteLine("===========================================================")
    End Sub

    ''' <summary>
    ''' Prints an information from the <see cref="ArgumentsModule"/>'s properties.
    ''' </summary>
    Sub PrintInfos()
        Console.WriteLine("=======================================")
        Console.WriteLine("File: " & TargetFile?.FullName)
        Console.WriteLine("Size to Pad: " & CalculateBytes(BytesToPad) & " (" & BytesToPad & " bytes)")
        Console.WriteLine("Padded Bytes Type: " & PaddingType.ToString())
        Console.WriteLine("=======================================")
    End Sub
End Module