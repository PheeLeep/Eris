Imports System.Runtime.CompilerServices
Imports System.Text

''' <summary>
''' A module containing extension methods of <see cref="System.Console"/>.
''' </summary>
Module CommandLineEx

    Private ReadOnly locker As Object = New Object()

    Enum PrintType
        None
        Verbose
        Question
        Info
        Warning
        [Error]
    End Enum

    ''' <summary>
    ''' Prompts a user's decision, in Yes/No choices.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    ''' <param name="message">A message to be prompt.</param>
    ''' <param name="pType">Specify a print symbol before a message. Default was <see cref="PrintType.Question"/>.</param>
    ''' <returns>Returns a <see cref="Boolean"/> value based on user's decision.</returns>
    <Extension()>
    Function Decision(console As Console, message As String, Optional pType As PrintType = PrintType.Question) As Boolean
        Dim response As ConsoleKey
        Do While response <> ConsoleKey.Y AndAlso response <> ConsoleKey.N
            ColorizeStatusInfo(pType)
            Console.Write(message & " [y/n]:")
            response = Console.ReadKey(False).Key
            If response <> ConsoleKey.Enter Then Console.WriteLine()
        Loop
        Return response = ConsoleKey.Y
    End Function

    ''' <summary>
    ''' Prints a message with symbol, date & time, along with a line termination.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    ''' <param name="message">A message to be printed to the terminal.</param>
    ''' <param name="pType">Specify a print symbol before a message. Default was <see cref="PrintType.None"/>.</param>
    ''' <param name="pDate">Prints the current date and time when it was printed. Default was <see langword="True"/></param>
    ''' <param name="delLine">Remove the current line before printing an output. Default was <see langword="False"/></param>
    <Extension()>
    Sub PrintLine(console As Console, message As String, Optional pType As PrintType = PrintType.None, Optional pDate As Boolean = True, Optional delLine As Boolean = False)
        Print(console, message & Environment.NewLine, pType, pDate, delLine)
    End Sub

    ''' <summary>
    ''' Prints a message with symbol and date & time value.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    ''' <param name="message">A message to be printed to the terminal.</param>
    ''' <param name="pType">Specify a print symbol before a message. Default was <see cref="PrintType.None"/>.</param>
    ''' <param name="pDate">Prints the current date and time when it was printed. Default was <see langword="True"/></param>
    ''' <param name="delLine">Remove the current line before printing an output. Default was <see langword="False"/></param>
    <Extension()>
    Sub Print(console As Console, message As String, Optional pType As PrintType = PrintType.None, Optional pDate As Boolean = True, Optional delLine As Boolean = False)
        SyncLock locker
            If delLine Then ClearLine(console)
            If pDate Then Console.Write(Date.Now.ToString("[dd/MM/yyyy hh:mm:ss]"))
            ColorizeStatusInfo(pType)
            Console.Write(message)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Clears the current line of a terminal.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    <Extension()>
    Sub ClearLine(console As Console)
        SyncLock locker
            Dim cLineCur As Integer = Console.CursorTop
            Console.SetCursorPosition(0, Console.CursorTop)
            Console.Write(New String(" "c, Console.WindowWidth))
            Console.SetCursorPosition(0, cLineCur)
        End SyncLock
    End Sub

    ''' <summary>
    ''' Prints a progress bar and provided text to the terminal.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    ''' <param name="current">A current value.</param>
    ''' <param name="max">A maximum value.</param>
    ''' <param name="msg">Prints a message after the progress bar. Default was an empty <see cref="String"/> value.</param>
    <Extension()>
    Sub PrintProgressBar(console As Console, current As Long, max As Long, Optional msg As String = "")
        If current > max OrElse max = 0 OrElse max < 1 Then Return
        SyncLock locker
            Dim b As New StringBuilder()
            b.Append("["c)
            Dim perce As Double = (current / max) * 100
            For i = 0 To 24
                Dim writtenPerce As Double = i / 25 * 100
                b.Append(IIf(writtenPerce >= perce, " ", "="))
            Next
            b.Append("] " & Math.Round(perce, 2) & "%" & IIf(String.IsNullOrEmpty(msg), "", " [" & msg & "]"))
            console.ClearLine()
            Console.Write(b.ToString())
        End SyncLock
    End Sub

    ''' <summary>
    ''' Colorizes the symbol based on specified <see cref="PrintType"/> value.
    ''' </summary>
    ''' <param name="console">A current <see cref="System.Console"/> object for accessing extension methods.</param>
    ''' <param name="pType">Specify a print symbol. Default was <see cref="PrintType.None"/></param>
    <Extension()>
    Private Sub ColorizeStatusInfo(pType As PrintType)
        Select Case pType
            Case PrintType.None
                Console.ForegroundColor = ConsoleColor.White
                Console.Write("[ ]: ")
            Case PrintType.Verbose
                Console.ForegroundColor = ConsoleColor.Cyan
                Console.Write("[*]: ")
            Case PrintType.Info
                Console.ForegroundColor = ConsoleColor.Green
                Console.Write("[i]: ")
            Case PrintType.Warning
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.Write("[!]: ")
            Case PrintType.Error
                Console.ForegroundColor = ConsoleColor.Red
                Console.Write("[x]: ")
            Case PrintType.Question
                Console.ForegroundColor = ConsoleColor.Blue
                Console.Write("[?]: ")
        End Select
        Console.ResetColor()
    End Sub
End Module
