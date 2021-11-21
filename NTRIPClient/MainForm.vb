Public Class MainForm
    'Send comments to Lance@Lefebure.com


    Public GPS As New NTRIPClient.GPS

    Dim settingsfile As String = Application.StartupPath & "\Settings.txt"
    Public UserCanNotEditSettings As Boolean = False
    Public UserCanNotDisconnectStuff As Boolean = False
    Public WriteEventsToFile As Boolean = False
    Public WriteNMEAToFile As Boolean = False

    Public SerialShouldBeConnected As Boolean = False
    Public SerialPort As Integer = 1
    Public SerialSpeed As Integer = 9600
    Public SerialDataBits As Integer = 8
    Public SerialStopBits As Integer = 1
    Dim ReceiveBuffer As String
    Dim WithEvents COMPort As New System.IO.Ports.SerialPort

    Public ReceiverType As Integer = 0 '0 is generic, 1 is NovAtel
    Public ReceiverMessageRate As Integer = 10 'Hz
    Public ReceiverCorrDataType As String = "RTCMV3"


    Public NTRIPShouldBeConnected As Boolean = False
    Public Shared NTRIPProtocol As Integer = 1
    Public Shared NTRIPUseManualGGA As Boolean = False
    'Public Shared NTRIPResendGGAEvery10Seconds As Boolean = True
    Public Shared NTRIPManualLat As Decimal = 41
    Public Shared NTRIPManualLon As Decimal = -91
    Public Shared NTRIPCaster As String = ""
    Public Shared NTRIPPort As Integer = 0
    Public Shared NTRIPUsername As String = ""
    Public Shared NTRIPPassword As String = ""
    Public Shared NTRIPMountPoint As String = ""
    Public PreferredMountPoint As String = ""
    Public NTRIPThread As Threading.Thread
    Public NTRIPIsConnected As Boolean = False
    Public NTRIPConnectionAttempt As Integer = 1
    Public Shared NTRIPStreamRequiresGGA As Boolean = False
    Dim NTRIPByteCount As Integer = 0
    Public NTRIPStreamArray(1, -1) As String
    Public Shared MostRecentGGA As String = ""

    Public CheckForUpdateDays As Integer = 7
    Dim LastCheckedForUpdates As Date = "1/1/2010"

    Public StartNTRIPThreadIn As Integer = 0
    Dim AudioFile As String = ""

    Dim lastLocation As Point = Location
    Dim lastSize As Size = Size

    Private Sub MainForm_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        lbl2.Text = ""
        lbl3.Text = ""
        Dim ver As String = "Welcome to Lefebure NTRIP Client version: " & My.Application.Info.Version.Major & "." & Format(My.Application.Info.Version.Minor, "00") & "." & Format(My.Application.Info.Version.Build, "00")
        If My.Application.Info.Version.Revision <> 0 Then ver += " (Rev " & My.Application.Info.Version.Revision & ")"
        rtbEvents.Text = ver

        CheckForQueuedUpdates()

        LoadSettingsFile()

        If My.Settings.WindowSize.Width > 100 And My.Settings.WindowSize.Height > 50 Then 'There are saved settings
            Me.Size = My.Settings.WindowSize
            Me.Location = My.Settings.WindowLocation
            If My.Settings.WindowState = FormWindowState.Minimized Then
                Me.WindowState = FormWindowState.Normal
            Else
                Me.WindowState = My.Settings.WindowState
            End If
        End If
        lastSize = Me.Size
        lastLocation = Me.Location

        GPS.ProcessNMEAdata("$GPGGA,,,,,,0,00,,,M,,M,,*66")

        If UserCanNotEditSettings Then
            btnSerialEdit.Visible = False
            btnNTRIPEdit.Visible = False
        End If

        LoadNTRIPSettings()

        RefreshMainScreenOptionsBasedOnProtocol()

        If SerialShouldBeConnected Then
            If UserCanNotDisconnectStuff Then
                btnSerialConnect.Visible = False
            End If
            OpenMySerialPort(False)
        End If

        If NTRIPShouldBeConnected Then
            If UserCanNotDisconnectStuff Then
                btnNTRIPConnect.Visible = False
            End If
            StartNTRIPThreadIn = 1
        End If

        Timer1.Start()

        GPS.GenerateGPGGAcode()
    End Sub
    Private Sub CheckForQueuedUpdates()
        'check if queued file exists
        Dim ProcessName As String = Application.ExecutablePath.Substring(Application.ExecutablePath.LastIndexOf("\") + 1)
        Dim NewFile As String = Application.StartupPath & "\" & ProcessName.Substring(0, ProcessName.LastIndexOf(".")) & ".new"
        If My.Computer.FileSystem.FileExists(NewFile) Then
            'Check for FileSwap.exe
            If My.Computer.FileSystem.FileExists(Application.StartupPath & "\fileswap.exe") Then
                'Look for /noupdate switch
                Dim cla As String() = Environment.GetCommandLineArgs()
                Dim OtherSwitches As String = ""
                If cla.Length > 1 Then 'The first element is always the executable path itself.
                    'Look for specific arguments here.
                    For Each arg As String In cla
                        'Ignore the case of the argument.
                        If String.Compare(arg, "/noupdate", True) = 0 Then
                            Exit Sub
                        Else
                            OtherSwitches += " " & arg
                        End If
                    Next arg
                End If

                'check queued file version
                Dim CurVer As Decimal = CDec(My.Application.Info.Version.Major * 10000 + My.Application.Info.Version.Minor * 100 + My.Application.Info.Version.Build + My.Application.Info.Version.Revision / 1000)
                Dim NewFileVI As FileVersionInfo = FileVersionInfo.GetVersionInfo(NewFile)
                Dim NewVer As Decimal = CDec(NewFileVI.ProductMajorPart * 10000 + NewFileVI.ProductMinorPart * 100 + NewFileVI.ProductBuildPart + NewFileVI.ProductPrivatePart / 1000)

                If NewVer > CurVer Then
                    'initiate file swap
                    Dim p As New Process
                    Dim pi As New ProcessStartInfo
                    pi.Arguments = "/sourceprocess:" & ProcessName & OtherSwitches
                    pi.FileName = Application.StartupPath & "\fileswap.exe"
                    p.StartInfo = pi
                    p.Start()
                    End
                End If
            End If
        End If
    End Sub

    Private Sub MainForm_LocationChanged(sender As System.Object, e As System.EventArgs) Handles MyBase.LocationChanged
        If Me.WindowState = FormWindowState.Normal Then lastLocation = Me.Location
    End Sub
    Private Sub MainForm_ResizeEnd(sender As System.Object, e As System.EventArgs) Handles MyBase.ResizeEnd
        If Me.WindowState = FormWindowState.Normal Then lastSize = Me.Size
    End Sub
    Private Sub MainForm_FormClosing(ByVal sender As System.Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles MyBase.FormClosing
        Timer1.Stop()

        'Close any open ports when the form is terminated
        If Not NTRIPThread Is Nothing Then
            If NTRIPThread.IsAlive Then
                LogEvent("Closing NTRIP Thread...")
                Threading.Thread.Sleep(10)
                Application.DoEvents()

                NTRIPIsConnected = False
                'Wait for the thread to notice the change and stop.
                Threading.Thread.Sleep(100)
                Application.DoEvents()
                Threading.Thread.Sleep(100)
                Application.DoEvents()
                If NTRIPThread.IsAlive Then
                    NTRIPThread.Abort() 'Ok, kill the thread if it is still running.
                    Threading.Thread.Sleep(100)
                    Application.DoEvents()
                    Threading.Thread.Sleep(100)
                    Application.DoEvents()
                End If

                LogEvent("NTRIP Thread Closed")
                Threading.Thread.Sleep(10)
                Application.DoEvents()
            End If
        End If


        If COMPort.IsOpen Then
            LogEvent("Closing Serial Port...")
            Threading.Thread.Sleep(10)
            Application.DoEvents()

            RemoveHandler COMPort.DataReceived, AddressOf SerialInput
            Application.DoEvents()
            Threading.Thread.Sleep(1000)
            'CloseMySerialPort()

            LogEvent("Serial Port Closed")
            Threading.Thread.Sleep(10)
            Application.DoEvents()
        End If

        'Clean out recording queue
        GPS.WriteRecordingQueueToFile()

        My.Settings.WindowState = Me.WindowState
        My.Settings.WindowLocation = lastLocation
        My.Settings.WindowSize = lastSize
        My.Settings.Save()
    End Sub
    Private Sub Timer1_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        If StartNTRIPThreadIn > 0 Then
            StartNTRIPThreadIn -= 1
            If StartNTRIPThreadIn = 0 Then StartNTRIP()
        End If
    End Sub
    Public Sub RedisplayTexts2and3()
        lbl2.Text = ""
        lbl3.Text = ""
        If COMPort.IsOpen Then
            If GPS.Disp2 = "age" Then lbl2.Text = "Age:" & GPS.LastCorrAge
            If GPS.Disp3 = "age" Then lbl3.Text = "Age:" & GPS.LastCorrAge
            If GPS.Disp2 = "speed-mph" Then lbl2.Text = Format(GPS.LastSpeed * 0.621371192, "0.0") & " MPH"
            If GPS.Disp3 = "speed-mph" Then lbl3.Text = Format(GPS.LastSpeed * 0.621371192, "0.0") & " MPH"
            If GPS.Disp2 = "speed-mph-smoothed" Then lbl2.Text = Format(GPS.LastSpeedSmoothed * 0.621371192, "0.0") & " MPH"
            If GPS.Disp3 = "speed-mph-smoothed" Then lbl3.Text = Format(GPS.LastSpeedSmoothed * 0.621371192, "0.0") & " MPH"
            If GPS.Disp2 = "speed-kmh" Then lbl2.Text = Format(GPS.LastSpeed, "0.0") & " km/h"
            If GPS.Disp3 = "speed-kmh" Then lbl3.Text = Format(GPS.LastSpeed, "0.0") & " km/h"
            If GPS.Disp2 = "speed-kmh-smoothed" Then lbl2.Text = Format(GPS.LastSpeedSmoothed, "0.0") & " km/h"
            If GPS.Disp3 = "speed-kmh-smoothed" Then lbl3.Text = Format(GPS.LastSpeedSmoothed, "0.0") & " km/h"
            If GPS.Disp2 = "heading" Then lbl2.Text = Format(GPS.LastHeading, "0.0") & Chr(176)
            If GPS.Disp3 = "heading" Then lbl3.Text = Format(GPS.LastHeading, "0.0") & Chr(176)
            If GPS.Disp2 = "pdop" Then lbl2.Text = "PDOP:" & GPS.LastPDOP
            If GPS.Disp3 = "pdop" Then lbl3.Text = "PDOP:" & GPS.LastPDOP
            If GPS.Disp2 = "hdop" Then lbl2.Text = "HDOP:" & GPS.LastHDOP
            If GPS.Disp3 = "hdop" Then lbl3.Text = "HDOP:" & GPS.LastHDOP
            If GPS.Disp2 = "vdop" Then lbl2.Text = "VDOP:" & GPS.LastVDOP
            If GPS.Disp3 = "vdop" Then lbl3.Text = "VDOP:" & GPS.LastVDOP
            If GPS.Disp2 = "elevation-meters" Then lbl2.Text = Format(GPS.LastAltitude, "#.00") & "m"
            If GPS.Disp3 = "elevation-meters" Then lbl3.Text = Format(GPS.LastAltitude, "#.00") & "m"
            If GPS.Disp2 = "elevation-feet" Then lbl2.Text = Format(GPS.LastAltitude * 3.2808399, "#.0") & "'"
            If GPS.Disp3 = "elevation-feet" Then lbl3.Text = Format(GPS.LastAltitude * 3.2808399, "#.0") & "'"
        End If
    End Sub

    Private Sub boxMountpoint_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles boxMountpoint.SelectionChangeCommitted
        PreferredMountPoint = boxMountpoint.SelectedItem
        SaveNTRIPSettings()
    End Sub

    Private Sub LoadSettingsFile()
        'Check to make sure directory exists, if not, throw a WTF message.
        If Not My.Computer.FileSystem.DirectoryExists(Application.StartupPath) Then
            MsgBox("Error: The Application's folder doesn't exist. Settings file not loaded.")
            Exit Sub
        End If

        If Not My.Computer.FileSystem.FileExists(settingsfile) Then 'File doesn't exist. Create it.
            Dim fn As New IO.StreamWriter(IO.File.Open(settingsfile, IO.FileMode.Create))
            fn.WriteLine("# This is the Lefebure GPS Data Path Pointer file. You need to use the format ""Key=Value"" for all settings.")
            fn.WriteLine("# Any line that starts with a # symbol will be ignored.")
            fn.WriteLine("# The only setting in this file should be the Data Path Location.")
            fn.WriteLine("")
            fn.Close()
        End If

        'Open and read file
        Dim SettingsArray(1, 0) As String
        Dim keyvalpair(1) As String
        Dim key As String
        Dim value As String
        Dim lCtr As Integer = 0

        Try
            Dim oRead As System.IO.StreamReader = System.IO.File.OpenText(settingsfile)
            Dim linein

            While oRead.Peek <> -1
                linein = Trim(oRead.ReadLine)
                If Len(linein) < 3 Then
                    'Line is too short
                ElseIf Asc(linein) = 35 Then
                    'Line starts with a #
                ElseIf InStr(linein, "=") < 2 Then
                    'There is no equal sign in the string
                Else
                    keyvalpair = Split(linein, "=", 2)
                    key = Trim(keyvalpair(0))
                    value = Trim(keyvalpair(1))
                    If Len(key) > 0 And Len(value) > 0 Then
                        'Looks good, add it to the array
                        ReDim Preserve SettingsArray(1, lCtr)
                        SettingsArray(0, lCtr) = LCase(key)
                        SettingsArray(1, lCtr) = value
                        lCtr = lCtr + 1
                    End If
                End If
            End While
            oRead.Close()
        Catch ex As Exception
        End Try

        If lCtr > 0 Then
            For i = 0 To UBound(SettingsArray, 2)
                value = SettingsArray(1, i)
                Select Case SettingsArray(0, i)
                    Case "serial should be connected"
                        If LCase(value) = "yes" Then SerialShouldBeConnected = True
                    Case "serial port number"
                        If IsNumeric(value) Then
                            Dim portnumber As Integer = CInt(value)
                            If portnumber > 0 And portnumber < 1025 Then
                                SerialPort = portnumber
                            Else
                                LogEvent("Specified Serial Port Number isn't in the range of 1 to 1024.")
                            End If
                        Else
                            LogEvent("Specified Serial Port Number isn't numeric.")
                        End If
                    Case "serial port speed"
                        If IsNumeric(value) Then
                            Dim portspeed As Integer = CInt(value)
                            If portspeed > 2399 And portspeed < 115201 Then
                                SerialSpeed = portspeed
                            Else
                                LogEvent("Specified Serial Port Speed isn't in the range of 2400 to 115200.")
                            End If
                        Else
                            LogEvent("Specified Serial Port Speed isn't numeric.")
                        End If
                    Case "serial port data bits"
                        If value = "7" Then
                            SerialDataBits = 7
                        ElseIf value = "8" Then
                            SerialDataBits = 8
                        Else
                            LogEvent("Specified Serial Port Data bits should be 7 or 8.")
                        End If
                    Case "serial port stop bits"
                        If value = "0" Then
                            SerialStopBits = 0
                        ElseIf value = "1" Then
                            SerialStopBits = 1
                        Else
                            LogEvent("Specified Serial Port Stop bits should be 0 or 1.")
                        End If


                    Case "protocol"
                        If LCase(value) = "rawtcpip" Then
                            NTRIPProtocol = 0 'Raw TCP/IP
                        Else
                            NTRIPProtocol = 1 'NTRIP
                        End If



                    Case "ntrip should be connected"
                        If LCase(value) = "yes" Then NTRIPShouldBeConnected = True
                    Case "ntrip use manual gga"
                        If LCase(value) = "yes" Then NTRIPUseManualGGA = True
                    Case "ntrip only send gga once"
                        'If LCase(value) = "yes" Then NTRIPResendGGAEvery10Seconds = False
                    Case "ntrip manual latitude"
                        If IsNumeric(value) Then
                            Dim inlat As Decimal = CDec(value)
                            If inlat > -90 And inlat < 90 Then
                                NTRIPManualLat = inlat
                            Else
                                LogEvent("Specified NTRIP Manual Latitude should be between -90 and 90.")
                            End If
                        Else
                            LogEvent("Specified NTRIP Manual Latitude should be numeric.")
                        End If
                    Case "ntrip manual longitude"
                        If IsNumeric(value) Then
                            Dim inlon As Decimal = CDec(value)
                            If inlon > -180 And inlon < 180 Then
                                NTRIPManualLon = inlon
                            Else
                                LogEvent("Specified NTRIP Manual Longitude should be between -180 and 180.")
                            End If
                        Else
                            LogEvent("Specified NTRIP Manual Longitude should be numeric.")
                        End If

                    Case "audio alert file"
                        AudioFile = value

                    Case "write events to file"
                        If LCase(value) = "yes" Then
                            WriteEventsToFile = True
                        End If
                    Case "write nmea to file"
                        If LCase(value) = "yes" Then
                            WriteNMEAToFile = True
                        End If

                    Case "display center"
                        GPS.Disp2 = LCase(value)
                    Case "display right"
                        GPS.Disp3 = LCase(value)

                    Case "check for updates interval"
                        If LCase(value) = "weekly" Then
                            CheckForUpdateDays = 7
                        Else
                            CheckForUpdateDays = 0
                        End If

                    Case "last checked for updates"
                        If IsDate(value) Then
                            LastCheckedForUpdates = CDate(value)
                        End If


                    Case "receiver type"
                        Select Case LCase(value)
                            Case "novatel"
                                ReceiverType = 1
                            Case Else
                                ReceiverType = 0
                        End Select

                    Case "receiver correction format"
                        Select Case LCase(value)
                            Case "rtcm"
                                ReceiverCorrDataType = "RTCM"
                            Case "rtcmv3"
                                ReceiverCorrDataType = "RTCMV3"
                            Case "cmr"
                                ReceiverCorrDataType = "CMR"
                            Case "rtca"
                                ReceiverCorrDataType = "RTCA"
                            Case "omnistar"
                                ReceiverCorrDataType = "OMNISTAR"
                            Case Else
                                ReceiverCorrDataType = "NOVATEL"
                        End Select

                    Case "receiver message rate"
                        Select Case value
                            Case "5"
                                ReceiverMessageRate = 5
                            Case "10"
                                ReceiverMessageRate = 10
                            Case Else
                                ReceiverMessageRate = 1
                        End Select


                    Case Else
                        'Key not found
                        If Not SettingsArray(0, i) = "" Then
                            'This will be blank if no settings were loaded
                            LogEvent("Just FYI, the """ & SettingsArray(0, i) & """ key in the settings file isn't valid, so it was skipped.")
                        End If
                End Select
            Next
        End If

        If CheckForUpdateDays > 0 Then
            Dim MinutesSince As Integer = DateDiff(DateInterval.Minute, LastCheckedForUpdates, Now)
            If MinutesSince > (CheckForUpdateDays * 3600) Then CheckForUpdates(False)
            'Dim stophere As Integer = 0
        End If
    End Sub
    Public Sub SaveSetting(ByVal key1 As String, ByVal value1 As String, Optional ByVal key2 As String = "", Optional ByVal value2 As String = "", Optional ByVal key3 As String = "", Optional ByVal value3 As String = "")
        If Not My.Computer.FileSystem.FileExists(settingsfile) Then 'File doesn't exist. Create it.
            Dim fn As New IO.StreamWriter(IO.File.Open(settingsfile, IO.FileMode.Create))
            fn.WriteLine("# This is the Lefebure NTRIP Client settings file. You need to use the format ""Key=Value"" for all settings.")
            fn.WriteLine("# Any line that starts with a # symbol will be ignored.")
            fn.WriteLine("")
            fn.Close()
        End If


        Dim keyvalpair(1) As String
        Dim oRead As System.IO.StreamReader = System.IO.File.OpenText(settingsfile)
        Dim linein As String
        Dim newfile As String = ""
        Dim foundkey1 As Boolean = False
        Dim foundkey2 As Boolean = False
        Dim foundkey3 As Boolean = False

        While oRead.Peek <> -1
            linein = Trim(oRead.ReadLine)
            If Len(linein) < 3 Then
                'Line is too short
                newfile += linein
            ElseIf Asc(linein) = 35 Then
                'Line starts with a #
                newfile += linein
            ElseIf InStr(linein, "=") < 2 Then
                'There is no equal sign in the string
                newfile += linein
            Else
                keyvalpair = Split(linein, "=", 2)
                If LCase(Trim(keyvalpair(0))) = LCase(key1) Then
                    'Found the right key, update it.
                    newfile += keyvalpair(0) & "=" & value1
                    foundkey1 = True
                ElseIf key2.Length > 0 And LCase(Trim(keyvalpair(0))) = LCase(key2) Then
                    newfile += keyvalpair(0) & "=" & value2
                    foundkey2 = True
                ElseIf key3.Length > 0 And LCase(Trim(keyvalpair(0))) = LCase(key3) Then
                    newfile += keyvalpair(0) & "=" & value3
                    foundkey3 = True
                Else
                    newfile += linein
                End If
            End If
            newfile += vbCrLf
        End While
        oRead.Close()

        If Not foundkey1 Then
            newfile += key1 & "=" & value1 & vbCrLf
        End If
        If key2.Length > 0 And Not foundkey2 Then
            newfile += key2 & "=" & value2 & vbCrLf
        End If
        If key3.Length > 0 And Not foundkey3 Then
            newfile += key3 & "=" & value3 & vbCrLf
        End If


        Try
            Dim sWriter As IO.StreamWriter = New IO.StreamWriter(settingsfile)
            sWriter.Write(newfile)
            sWriter.Flush()
            sWriter.Close()
        Catch ex As Exception
            MsgBox(ex.Message)
        End Try
    End Sub

    Public Sub SaveNTRIPSettings()
        Dim ntripsettings As String = "NTRIP Caster=" & NTRIPCaster & vbCrLf
        ntripsettings += "NTRIP Caster Port=" & NTRIPPort.ToString & vbCrLf
        ntripsettings += "NTRIP Username=" & NTRIPUsername & vbCrLf
        ntripsettings += "NTRIP Password=" & NTRIPPassword & vbCrLf
        ntripsettings += "NTRIP MountPoint=" & PreferredMountPoint & vbCrLf
        Dim targetfile As String = Application.StartupPath & "\ntripconfig.txt"
        My.Computer.FileSystem.WriteAllText(targetfile, ntripsettings, False)
    End Sub
    Public Sub LoadNTRIPSettings()
        boxMountpoint.Items.Add("Download Source Table")
        boxMountpoint.SelectedIndex = 0

        'Load NTRIP settings file
        Dim ntripconfigfile As String = Application.StartupPath & "\ntripconfig.txt"
        If My.Computer.FileSystem.FileExists(ntripconfigfile) Then
            Dim SettingsArray(1, 0) As String
            Dim keyvalpair(1) As String
            Dim key As String
            Dim value As String
            Dim lCtr As Integer = 0

            Try
                Dim oRead As System.IO.StreamReader = System.IO.File.OpenText(ntripconfigfile)
                Dim linein

                While oRead.Peek <> -1
                    linein = Trim(oRead.ReadLine)
                    If Len(linein) < 3 Then
                        'Line is too short
                    ElseIf Asc(linein) = 35 Then
                        'Line starts with a #
                    ElseIf InStr(linein, "=") < 2 Then
                        'There is no equal sign in the string
                    Else
                        keyvalpair = Split(linein, "=", 2)
                        key = Trim(keyvalpair(0))
                        value = Trim(keyvalpair(1))
                        If Len(key) > 0 And Len(value) > 0 Then
                            'Looks good, add it to the array
                            ReDim Preserve SettingsArray(1, lCtr)
                            SettingsArray(0, lCtr) = LCase(key)
                            SettingsArray(1, lCtr) = value
                            lCtr = lCtr + 1
                        End If
                    End If
                End While
                oRead.Close()
            Catch ex As Exception
            End Try

            If lCtr > 0 Then
                For i = 0 To UBound(SettingsArray, 2)
                    value = SettingsArray(1, i)
                    Select Case SettingsArray(0, i)
                        Case "ntrip caster"
                            MainForm.NTRIPCaster = value
                        Case "ntrip caster port"
                            If IsNumeric(value) Then
                                MainForm.NTRIPPort = CInt(value)
                            End If
                        Case "ntrip username"
                            MainForm.NTRIPUsername = value
                        Case "ntrip password"
                            MainForm.NTRIPPassword = value
                        Case "ntrip mountpoint"
                            PreferredMountPoint = value
                    End Select
                Next
            End If
        End If

        'Load sourcetable file into drop down list
        Dim sourcetablefile As String = Application.StartupPath & "\sourcetable.dat"
        If My.Computer.FileSystem.FileExists(sourcetablefile) Then
            'File exists. Open and parse
            Try
                Dim sourcefile As String = ""
                Dim linein As String
                Dim oRead As System.IO.StreamReader = System.IO.File.OpenText(sourcetablefile)
                While oRead.Peek <> -1
                    linein = Trim(oRead.ReadLine)
                    sourcefile += linein & vbCrLf
                End While
                oRead.Close()

                If sourcefile.Length > 10 Then
                    ParseSourceTable(sourcefile)
                End If
            Catch ex As Exception
            End Try
        End If

    End Sub
    Public Sub ParseSourceTable(ByVal table As String)
        ReDim NTRIPStreamArray(1, -1)
        Dim StreamCount As Integer = -1 'zero based array
        boxMountpoint.Items.Clear()
        boxMountpoint.Items.Add("Download Source Table")
        boxMountpoint.SelectedIndex = 0

        'Dim testlines13() As String = Split(table, Chr(13))
        'Dim testlines10() As String = Split(table, Chr(10))

        Dim lines() As String = Split(table, vbCrLf) 'Chr(13)
        For i = 0 To UBound(lines)
            Dim fields() As String = Split(lines(i), ";")
            If UBound(fields) > 4 Then '############################################ How can this be only 4 if we need field 11 below???????? 3/9/2011
                If LCase(fields(0)) = "str" Then 'We found a STReam
                    boxMountpoint.Items.Add(fields(1))
                    StreamCount += 1
                    ReDim Preserve NTRIPStreamArray(1, StreamCount)
                    NTRIPStreamArray(0, StreamCount) = fields(1)
                    NTRIPStreamArray(1, StreamCount) = fields(11)
                End If
            End If
        Next

        Dim k As Integer = 0
        Dim selectedmnt As Integer = 0
        For Each item In boxMountpoint.Items
            If item = PreferredMountPoint Then
                selectedmnt = k
            End If
            k += 1
        Next

        If boxMountpoint.Items.Count = 1 Then
            boxMountpoint.SelectedIndex = 0
        Else
            boxMountpoint.SelectedIndex = selectedmnt
        End If
    End Sub

    Public Sub RefreshMainScreenOptionsBasedOnProtocol()
        Select Case NTRIPProtocol
            Case 1 'NTRIP
                lblNTRIPStream.Text = "NTRIP Stream:"
                boxMountpoint.Visible = True
            Case Else '0 Raw TCP/IP
                lblNTRIPStream.Text = "Stream: Raw TCP/IP from " & NTRIPCaster & ":" & NTRIPPort
                boxMountpoint.Visible = False
        End Select
    End Sub


    Public Sub LogEvent(ByVal Message As String)
        If rtbEvents.TextLength > 5000 Then
            Dim NewText As String = Mid(rtbEvents.Text, 1000) 'Drop first 1000 characters
            NewText = NewText.Remove(0, NewText.IndexOf(ChrW(10)) + 1) 'Drop up to the next new line
            rtbEvents.Text = NewText
        End If

        rtbEvents.AppendText(vbCrLf & TimeOfDay() & " - " & Message)
        rtbEvents.SelectionStart = rtbEvents.TextLength
        rtbEvents.ScrollToCaret()

        If WriteEventsToFile Then
            Dim logfolder As String = Application.StartupPath & "\Logs"

            If Not My.Computer.FileSystem.DirectoryExists(logfolder) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(logfolder)
                Catch ex As Exception
                End Try
            End If

            Dim logfile As String = logfolder & "\" & Year(Now) & Format(Month(Now), "00") & Format(DatePart(DateInterval.Day, Now), "00") & ".txt"

            For i = 0 To 10
                Try
                    My.Computer.FileSystem.WriteAllText(logfile, Now() & " - " & Message & vbCrLf, True)
                    Exit For 'This worked, don't try it again
                    Threading.Thread.Sleep(20)
                    Application.DoEvents()
                Catch ex As Exception
                End Try
            Next
        End If
    End Sub
    Public Sub PlayAudioAlert()
        If AudioFile = "" Or AudioFile = "None" Then
            'No file specified. Do nothing
        Else
            Try
                If IO.File.Exists(Application.StartupPath & "\" & AudioFile) Then 'We know the file exists.
                    My.Computer.Audio.Play(Application.StartupPath & "\" & AudioFile, AudioPlayMode.Background)
                End If
            Catch ex As Exception
                MsgBox(ex.Message)
            End Try
        End If
    End Sub


    Private Sub btnOptions_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnOptions.Click
        My.Computer.Audio.Stop() 'In case some long .wav file is still playing

        Select Case GPS.Disp2
            Case "age"
                OptionsDialog.boxText2.SelectedIndex = 1
            Case "hdop"
                OptionsDialog.boxText2.SelectedIndex = 2
            Case "vdop"
                OptionsDialog.boxText2.SelectedIndex = 3
            Case "pdop"
                OptionsDialog.boxText2.SelectedIndex = 4
            Case "elevation-feet"
                OptionsDialog.boxText2.SelectedIndex = 5
            Case "elevation-meters"
                OptionsDialog.boxText2.SelectedIndex = 6
            Case "speed-mph"
                OptionsDialog.boxText2.SelectedIndex = 7
            Case "speed-mph-smoothed"
                OptionsDialog.boxText2.SelectedIndex = 8
            Case "speed-kmh"
                OptionsDialog.boxText2.SelectedIndex = 9
            Case "speed-kmh-smoothed"
                OptionsDialog.boxText2.SelectedIndex = 10
            Case "heading"
                OptionsDialog.boxText2.SelectedIndex = 11
            Case Else
                OptionsDialog.boxText2.SelectedIndex = 0
        End Select

        Select Case GPS.Disp3
            Case "age"
                OptionsDialog.boxText3.SelectedIndex = 1
            Case "hdop"
                OptionsDialog.boxText3.SelectedIndex = 2
            Case "vdop"
                OptionsDialog.boxText3.SelectedIndex = 3
            Case "pdop"
                OptionsDialog.boxText3.SelectedIndex = 4
            Case "elevation-feet"
                OptionsDialog.boxText3.SelectedIndex = 5
            Case "elevation-meters"
                OptionsDialog.boxText3.SelectedIndex = 6
            Case "speed-mph"
                OptionsDialog.boxText3.SelectedIndex = 7
            Case "speed-mph-smoothed"
                OptionsDialog.boxText3.SelectedIndex = 8
            Case "speed-kmh"
                OptionsDialog.boxText3.SelectedIndex = 9
            Case "speed-kmh-smoothed"
                OptionsDialog.boxText3.SelectedIndex = 10
            Case "heading"
                OptionsDialog.boxText3.SelectedIndex = 11
            Case Else
                OptionsDialog.boxText3.SelectedIndex = 0
        End Select

        OptionsDialog.boxAudioFile.Items.Clear()
        OptionsDialog.boxAudioFile.Items.Add("None")
        OptionsDialog.boxAudioFile.SelectedIndex = 0
        Dim di As New IO.DirectoryInfo(Application.StartupPath)
        For Each fi As IO.FileInfo In di.GetFiles
            If fi.Extension = ".wav" Then
                'MsgBox(fi.Name)
                OptionsDialog.boxAudioFile.Items.Add(fi.Name)
                If fi.Name = AudioFile Then
                    OptionsDialog.boxAudioFile.SelectedIndex = OptionsDialog.boxAudioFile.Items.Count - 1
                End If
            End If
        Next

        If WriteEventsToFile Then
            OptionsDialog.boxDoLogging.SelectedIndex = 1
        Else
            OptionsDialog.boxDoLogging.SelectedIndex = 0
        End If

        If WriteNMEAToFile Then
            OptionsDialog.boxDoSaveNMEA.SelectedIndex = 1
        Else
            OptionsDialog.boxDoSaveNMEA.SelectedIndex = 0
        End If

        If CheckForUpdateDays = 7 Then
            OptionsDialog.boxCheckWeekly.SelectedIndex = 1
        Else
            OptionsDialog.boxCheckWeekly.SelectedIndex = 0
        End If



        Dim DialogResult As Integer = OptionsDialog.ShowDialog()
        Dim result As Integer = Convert.ToInt32(DialogResult)

        If result = 1 Then
            Select Case OptionsDialog.boxText2.SelectedIndex
                Case 1
                    GPS.Disp2 = "age"
                Case 2
                    GPS.Disp2 = "hdop"
                Case 3
                    GPS.Disp2 = "vdop"
                Case 4
                    GPS.Disp2 = "pdop"
                Case 5
                    GPS.Disp2 = "elevation-feet"
                Case 6
                    GPS.Disp2 = "elevation-meters"
                Case 7
                    GPS.Disp2 = "speed-mph"
                Case 8
                    GPS.Disp2 = "speed-mph-smoothed"
                Case 9
                    GPS.Disp2 = "speed-kmh"
                Case 10
                    GPS.Disp2 = "speed-kmh-smoothed"
                Case 11
                    GPS.Disp2 = "heading"
                Case Else
                    GPS.Disp2 = ""
            End Select
            SaveSetting("Display Center", GPS.Disp2)

            Select Case OptionsDialog.boxText3.SelectedIndex
                Case 1
                    GPS.Disp3 = "age"
                Case 2
                    GPS.Disp3 = "hdop"
                Case 3
                    GPS.Disp3 = "vdop"
                Case 4
                    GPS.Disp3 = "pdop"
                Case 5
                    GPS.Disp3 = "elevation-feet"
                Case 6
                    GPS.Disp3 = "elevation-meters"
                Case 7
                    GPS.Disp3 = "speed-mph"
                Case 8
                    GPS.Disp3 = "speed-mph-smoothed"
                Case 9
                    GPS.Disp3 = "speed-kmh"
                Case 10
                    GPS.Disp3 = "speed-kmh-smoothed"
                Case 11
                    GPS.Disp3 = "heading"
                Case Else
                    GPS.Disp3 = ""
            End Select
            SaveSetting("Display Right", GPS.Disp3)

            If Not OptionsDialog.boxAudioFile.SelectedItem = AudioFile Then
                AudioFile = OptionsDialog.boxAudioFile.SelectedItem
                SaveSetting("Audio Alert File", AudioFile)
            End If

            If OptionsDialog.boxDoLogging.SelectedIndex = 0 Then
                If WriteEventsToFile Then
                    SaveSetting("Write Events to File", "No")
                    WriteEventsToFile = False
                End If
            Else
                If Not WriteEventsToFile Then
                    SaveSetting("Write Events to File", "Yes")
                    WriteEventsToFile = True
                End If
            End If

            If OptionsDialog.boxDoSaveNMEA.SelectedIndex = 0 Then
                If WriteNMEAToFile Then
                    SaveSetting("Write NMEA to File", "No")
                    WriteNMEAToFile = False
                End If
            Else
                If Not WriteNMEAToFile Then
                    SaveSetting("Write NMEA to File", "Yes")
                    WriteNMEAToFile = True
                End If
            End If

            If OptionsDialog.boxCheckWeekly.SelectedIndex = 0 Then
                If Not CheckForUpdateDays = 0 Then
                    CheckForUpdateDays = 0
                    SaveSetting("Check for Updates Interval", "Never")
                End If
            Else
                If Not CheckForUpdateDays = 7 Then
                    CheckForUpdateDays = 7
                    SaveSetting("Check for Updates Interval", "Weekly")
                End If
            End If


            RedisplayTexts2and3()
            LogEvent("Options Saved")
        End If
    End Sub

    Private Sub btnSerialConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSerialConnect.Click
        If btnSerialConnect.Text = "Connect" Then
            OpenMySerialPort(True)
        Else
            CloseMySerialPort()
            SaveSetting("Serial Should be Connected", "No")
        End If
    End Sub
    Private Sub btnSerialEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnSerialEdit.Click
        SerialDialog.boxSerialPort.Items.Clear()
        Dim targetport As String = "COM" & SerialPort.ToString

        Dim i As Integer = 0
        Dim portindex As Integer = 0
        For Each portName As String In My.Computer.Ports.SerialPortNames
            Dim portNumberChars() As Char = portName.Substring(3).ToCharArray() 'Remove "COM", put the rest in a character array
            portName = "COM" 'Start over with "COM"
            For Each portNumberChar As Char In portNumberChars
                If Char.IsDigit(portNumberChar) Then 'Good character, append to portName
                    portName += portNumberChar.ToString()
                End If
            Next
            SerialDialog.boxSerialPort.Items.Add(portName)
            If portName = targetport Then
                portindex = i
            End If
            i += 1
        Next
        If i = 0 Then
            SerialDialog.boxSerialPort.Items.Add("No Serial Ports Found")
        End If
        SerialDialog.boxSerialPort.SelectedIndex = portindex

        If SerialDialog.boxSpeed.Items.Count = 9 Then
            SerialDialog.boxSpeed.Items.RemoveAt(8)
        End If

        Select Case SerialSpeed
            Case 2400
                SerialDialog.boxSpeed.SelectedIndex = 0
            Case 4800
                SerialDialog.boxSpeed.SelectedIndex = 1
            Case 9600
                SerialDialog.boxSpeed.SelectedIndex = 2
            Case 14400
                SerialDialog.boxSpeed.SelectedIndex = 3
            Case 19200
                SerialDialog.boxSpeed.SelectedIndex = 4
            Case 38400
                SerialDialog.boxSpeed.SelectedIndex = 5
            Case 57600
                SerialDialog.boxSpeed.SelectedIndex = 6
            Case 115200
                SerialDialog.boxSpeed.SelectedIndex = 7
            Case Else 'How did this happen
                If SerialDialog.boxSpeed.Items.Count = 8 Then
                    SerialDialog.boxSpeed.Items.Add(SerialSpeed.ToString)
                End If
                SerialDialog.boxSpeed.SelectedIndex = 8
        End Select

        If SerialDataBits = 7 Then
            SerialDialog.boxDataBits.SelectedIndex = 0
        Else
            SerialDialog.boxDataBits.SelectedIndex = 1
        End If



        'Receiver auto-config stuff
        Select Case ReceiverType
            Case 1
                SerialDialog.boxReceiverType.SelectedIndex = 1
            Case Else
                SerialDialog.boxReceiverType.SelectedIndex = 0
        End Select
        SerialDialog.RedisplayAutoConfigOptions(ReceiverType)







        Dim DialogResult As Integer = SerialDialog.ShowDialog()
        Dim result As Integer = Convert.ToInt32(DialogResult)

        If result = 1 Then
            If SerialDialog.boxSerialPort.SelectedItem = "No Serial Ports Found" Then
                'Do nothing here
            Else 'Some serial port was selected
                SerialPort = CInt(Replace(SerialDialog.boxSerialPort.SelectedItem, "COM", ""))
                SaveSetting("Serial Port Number", SerialPort)
            End If

            Select Case SerialDialog.boxSpeed.SelectedIndex
                Case 0
                    SerialSpeed = 2400
                Case 1
                    SerialSpeed = 4800
                Case 2
                    SerialSpeed = 9600
                Case 3
                    SerialSpeed = 14400
                Case 4
                    SerialSpeed = 19200
                Case 5
                    SerialSpeed = 38400
                Case 6
                    SerialSpeed = 57600
                Case 7
                    SerialSpeed = 115200
                Case 8
                    'custom speed selected. Don't change it
            End Select

            If SerialDialog.boxDataBits.SelectedIndex = 0 Then
                SerialDataBits = 7
            Else
                SerialDataBits = 8
            End If


            Select Case SerialDialog.boxReceiverType.SelectedIndex
                Case 1
                    ReceiverType = 1
                    Select Case SerialDialog.boxCorrDataType.SelectedIndex
                        Case 1
                            ReceiverCorrDataType = "RTCMV3"
                        Case 2
                            ReceiverCorrDataType = "CMR"
                        Case 3
                            ReceiverCorrDataType = "RTCA"
                        Case 4
                            ReceiverCorrDataType = "OMNISTAR"
                        Case 5
                            ReceiverCorrDataType = "NOVATEL"
                        Case Else
                            ReceiverCorrDataType = "RTCM"
                    End Select

                    Select Case SerialDialog.boxMsgRate.SelectedIndex
                        Case 1
                            ReceiverMessageRate = 5
                        Case 2
                            ReceiverMessageRate = 10
                        Case 3
                            ReceiverMessageRate = 1
                    End Select

                    SaveSetting("Receiver Type", "NovAtel", "Receiver Correction Format", ReceiverCorrDataType, "Receiver Message Rate", ReceiverMessageRate)
                Case Else
                    ReceiverType = 0
                    SaveSetting("Receiver Type", "NoAutoConfig")
            End Select



            SaveSetting("Serial Port Speed", SerialSpeed, "Serial Port Data Bits", SerialDataBits, "Serial Port Stop Bits", SerialStopBits)

            LogEvent("Serial Port Settings Saved")
        End If
    End Sub

    Private Sub btnNTRIPConnect_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNTRIPConnect.Click
        If btnNTRIPConnect.Text = "Connect" Then
            NTRIPConnectionAttempt = 1
            NTRIPShouldBeConnected = True
            SaveSetting("NTRIP Should be Connected", "Yes")
            StartNTRIPThreadIn = 1
        Else
            NTRIPShouldBeConnected = False
            SaveSetting("NTRIP Should be Connected", "No")
            StopNTRIP()
        End If
    End Sub
    Private Sub btnNTRIPEdit_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnNTRIPEdit.Click
        Select Case NTRIPProtocol
            Case 1
                NTRIPDialog.boxProtocol.SelectedIndex = 1
            Case Else
                NTRIPDialog.boxProtocol.SelectedIndex = 0
        End Select

        NTRIPDialog.tbAddress.Text = NTRIPCaster
        NTRIPDialog.tbPort.Text = NTRIPPort
        NTRIPDialog.tbUsername.Text = NTRIPUsername
        NTRIPDialog.tbPassword.Text = NTRIPPassword
        NTRIPDialog.tbLatitude.Text = NTRIPManualLat
        NTRIPDialog.tbLongitude.Text = NTRIPManualLon

        'If NTRIPResendGGAEvery10Seconds Then
        '    NTRIPDialog.boxSendGGAFreq.SelectedIndex = 0
        'Else
        '    NTRIPDialog.boxSendGGAFreq.SelectedIndex = 1
        'End If

        If NTRIPUseManualGGA Then
            NTRIPDialog.boxManualGGA.SelectedIndex = 1
        Else
            NTRIPDialog.boxManualGGA.SelectedIndex = 0
        End If

        NTRIPDialog.RefreshDisplayedItems()

        NTRIPDialog.tbAddress.Focus()


        Dim DialogResult As Integer = NTRIPDialog.ShowDialog()
        Dim result As Integer = Convert.ToInt32(DialogResult)

        If result = 1 Then
            NTRIPCaster = NTRIPDialog.tbAddress.Text

            Select Case NTRIPDialog.boxProtocol.SelectedIndex
                Case 1 'NTRIP
                    If Not NTRIPProtocol = 1 Then
                        NTRIPProtocol = 1
                        SaveSetting("Protocol", "NTRIP")
                    End If
                Case 0 'Raw TCP/IP
                    If Not NTRIPProtocol = 0 Then
                        NTRIPProtocol = 0
                        SaveSetting("Protocol", "RawTCPIP")
                    End If
            End Select

            If IsNumeric(NTRIPDialog.tbPort.Text) Then
                Dim newport As Integer = CInt(NTRIPDialog.tbPort.Text)
                If newport > 0 And newport < 65536 Then
                    NTRIPPort = newport
                Else
                    LogEvent("The NTRIP Caster Port needs to be in the range of 1-65535.")
                End If
            Else
                LogEvent("The NTRIP Caster Port needs to be numeric.")
            End If

            NTRIPUsername = NTRIPDialog.tbUsername.Text
            NTRIPPassword = NTRIPDialog.tbPassword.Text
            SaveNTRIPSettings()


            If NTRIPDialog.boxManualGGA.SelectedIndex = 0 Then
                NTRIPUseManualGGA = False
                SaveSetting("NTRIP Use Manual GGA", "No")

                'If NTRIPDialog.boxSendGGAFreq.SelectedIndex = 0 Then
                '    If Not NTRIPResendGGAEvery10Seconds Then
                '        NTRIPResendGGAEvery10Seconds = True
                '        SaveSetting("NTRIP Only Send GGA Once", "No")
                '    End If
                'Else
                '    If NTRIPResendGGAEvery10Seconds Then
                '        NTRIPResendGGAEvery10Seconds = False
                '        SaveSetting("NTRIP Only Send GGA Once", "Yes")
                '    End If
                'End If

            Else
                NTRIPUseManualGGA = True

                If IsNumeric(NTRIPDialog.tbLatitude.Text) Then
                    Dim newlat As Decimal = CDec(NTRIPDialog.tbLatitude.Text)
                    If newlat > -90 And newlat < 90 Then
                        NTRIPManualLat = newlat
                    Else
                        LogEvent("The Manual Latitude needs to be in the range of -90 to 90.")
                    End If
                Else
                    LogEvent("The Manual Latitude needs to be numeric.")
                End If

                If IsNumeric(NTRIPDialog.tbLongitude.Text) Then
                    Dim newlon As Decimal = CDec(NTRIPDialog.tbLongitude.Text)
                    If newlon > -180 And newlon < 180 Then
                        NTRIPManualLon = newlon
                    Else
                        LogEvent("The Manual Longitude needs to be in the range of -180 to 180.")
                    End If
                Else
                    LogEvent("The Manual Longitude needs to be numeric.")
                End If

                SaveSetting("NTRIP Use Manual GGA", "Yes", "NTRIP Manual Latitude", NTRIPManualLat, "NTRIP Manual Longitude", NTRIPManualLon)
            End If

            LogEvent("NTRIP Settings Saved")

            RefreshMainScreenOptionsBasedOnProtocol()
        ElseIf result = 2 Then
            'Don't do anything
        End If
    End Sub


    Public Sub OpenMySerialPort(ByVal UserClickedConnect As Boolean)
        If COMPort.IsOpen Then
            COMPort.RtsEnable = False
            COMPort.DtrEnable = False
            COMPort.Close()
            Application.DoEvents()
            System.Threading.Thread.Sleep(500)
        End If

        lblSerialStatus.Text = "Connecting"
        lblStatus.Text = "Waiting for Data"
        GPS.LastFixQuality = -1

        COMPort.PortName = "COM" & SerialPort
        COMPort.BaudRate = SerialSpeed
        COMPort.DataBits = SerialDataBits
        'If SerialStopBits = 1 Then
        COMPort.StopBits = IO.Ports.StopBits.One
        'Else
        'COMPort.StopBits = IO.Ports.StopBits.None
        'End If
        COMPort.WriteTimeout = 2000
        COMPort.ReadTimeout = 2000

        AddHandler COMPort.DataReceived, AddressOf SerialInput

        Try
            COMPort.Open()
        Catch ex As Exception
            'MsgBox(ex.Message)
            LogEvent("### Error: " & ex.Message)
            If LCase(ex.Message).Contains("the semaphore timeout period has expired") Then
                LogEvent("Check that your bluetooth device is powered on and paired correctly.")
            End If

        End Try
        If COMPort.IsOpen Then
            COMPort.RtsEnable = True
            COMPort.DtrEnable = True

            'Kick start the serial port so it starts reading data.
            COMPort.BreakState = True
            System.Threading.Thread.Sleep((11000 / COMPort.BaudRate) + 2) ' Min. 11 bit delay (startbit, 8 data bits, parity bit, stopbit)
            COMPort.BreakState = False


            If UserClickedConnect Then
                If UserCanNotDisconnectStuff Then
                    btnSerialConnect.Visible = False
                End If
                SaveSetting("Serial Should be Connected", "Yes")
            End If

            'Change connect/disconnect button display status
            lblSerialStatus.Text = "Connected to COM " & SerialPort & " at " & SerialSpeed & "bps"
            btnSerialConnect.Text = "Disconnect"
            btnSerialEdit.Visible = False

            If ReceiverType = 1 Then AutoConfigNovatel()

            RedisplayTexts2and3()
        Else
            lblSerialStatus.Text = "Unable to open serial port"
            lblStatus.Text = "Not Connected"
        End If
    End Sub
    Public Sub AutoConfigNovatel()
        If COMPort.IsOpen Then
            Dim ontime As String = "1"
            If ReceiverMessageRate = 5 Then ontime = "0.2"
            If ReceiverMessageRate = 10 Then ontime = "0.1"

            COMPort.DiscardOutBuffer()
            COMPort.NewLine = vbCrLf

            COMPort.WriteLine("")
            COMPort.WriteLine("")
            COMPort.WriteLine("unlogall thisport")
            COMPort.WriteLine("log thisport gpggalong ontime " & ontime)
            COMPort.WriteLine("log thisport gprmc ontime " & ontime)
            COMPort.WriteLine("interfacemode thisport " & ReceiverCorrDataType & " novatel")
        End If
    End Sub
    Public Sub CloseMySerialPort()
        RemoveHandler COMPort.DataReceived, AddressOf SerialInput
        Application.DoEvents()
        Threading.Thread.Sleep(1000)

        If COMPort.IsOpen Then COMPort.Close()

        'Change connect/disconnect button display status
        btnSerialConnect.Text = "Connect"
        lblSerialStatus.Text = "Disconnected"

        If Not UserCanNotEditSettings Then
            btnSerialEdit.Visible = True
        End If
        lblStatus.Text = "Not Connected"
        lbl2.Text = ""
        lbl3.Text = ""
    End Sub

    Private Sub SerialInput(ByVal sender As Object, ByVal e As System.IO.Ports.SerialDataReceivedEventArgs)
        Try
            ReceiveBuffer += COMPort.ReadExisting

            If InStr(ReceiveBuffer, vbCrLf) Then
                'If InStr(ReceiveBuffer, Chr(13)) Then
                'Contains at least one carridge return
                Dim lines() As String = Split(ReceiveBuffer, vbCrLf)
                'Dim lines() As String = Split(ReceiveBuffer, Chr(13))
                For i = 0 To UBound(lines) - 1
                    If lines(i).Length > 5 Then
                        SendSerialLineToUIThread(Trim(lines(i)))
                    End If
                Next
                ReceiveBuffer = lines(UBound(lines))
            Else
                'Data doesn't contain any line breaks
                If ReceiveBuffer.Length > 4000 Then
                    ReceiveBuffer = ""
                    SendSerialLineToUIThread("No line breaks found in data stream.")
                End If
            End If
        Catch ex As TimeoutException
            'MsgBox(ex.ToString)
        End Try
    End Sub
    Private Sub SendSerialLineToUIThread(ByVal Line As String)
        Try
            Dim uidel As New SendSerialLineToUIThreadDelegate(AddressOf CallBackSerialtoUIThread)
            Dim o(0) As Object
            o(0) = Line
            Invoke(uidel, o)
        Catch ex As Exception
        End Try
    End Sub
    Delegate Sub SendSerialLineToUIThreadDelegate(ByVal Line As String)
    Private Sub CallBackSerialtoUIThread(ByVal Line As String)
        GPS.ProcessNMEAdata(Line)
    End Sub






    Public Sub StartNTRIP()
        'Check the status to see if it is already connected
        If Not NTRIPThread Is Nothing Then
            If NTRIPThread.IsAlive Then
                btnNTRIPConnect.Text = "Disconnect"
                LogEvent("NTRIP thread is already running. Please disconnect first before trying to connect again.")
                StopNTRIP()
                Exit Sub
            End If
        End If

        If NTRIPCaster = "" Then
            lblNTRIPStatus.Text = "No NTRIP Caster Specified"
            Exit Sub
        End If
        If NTRIPPort = 0 Then
            lblNTRIPStatus.Text = "No NTRIP Caster Port Specified"
            Exit Sub
        End If
        If NTRIPPort < 1 Or NTRIPPort > 65535 Then
            lblNTRIPStatus.Text = "Invalid Port Number"
            Exit Sub
        End If


        NTRIPMountPoint = boxMountpoint.SelectedItem
        If NTRIPMountPoint = "Download Source Table" Then
            NTRIPMountPoint = ""
        Else
            PreferredMountPoint = NTRIPMountPoint
        End If

        NTRIPStreamRequiresGGA = False
        For i = 0 To UBound(NTRIPStreamArray, 2)
            If NTRIPStreamArray(0, i) = NTRIPMountPoint Then
                If NTRIPStreamArray(1, i) = "1" Then
                    NTRIPStreamRequiresGGA = True
                End If
            End If
        Next

        boxMountpoint.Enabled = False
        btnNTRIPConnect.Text = "Disconnect"
        btnNTRIPEdit.Visible = False

        lblNTRIPStatus.Text = "Starting NTRIP Thread"
        Application.DoEvents()

        NTRIPIsConnected = True
        NTRIPThread = New Threading.Thread(AddressOf NTRIPLoop)
        NTRIPThread.Priority = Threading.ThreadPriority.AboveNormal
        NTRIPThread.Start()
    End Sub
    Public Sub StopNTRIP()
        'This gets called from the MyBase.FormClosed event as the app closes
        'Attempt to disconnect the nice way
        StartNTRIPThreadIn = 0
        NTRIPIsConnected = False
        lblNTRIPStatus.Text = "Disconnecting..."

        'Wait for the thread to notice the change and stop.
        Threading.Thread.Sleep(100)
        Application.DoEvents()
        Threading.Thread.Sleep(100)
        Application.DoEvents()

        'Ok, kill the thread if it is still running.
        If Not NTRIPThread Is Nothing Then
            If NTRIPThread.IsAlive Then
                NTRIPThread.Abort() 'Need to add the ability to truly kill a connection that is unresponsive. .Abort() doesn't seem to actually kill the thread
                Threading.Thread.Sleep(100)
                Application.DoEvents()
                Threading.Thread.Sleep(100)
                Application.DoEvents()
            End If
        End If

        pbNTRIP.Visible = False
        NTRIPConnectionAttempt += 1
        lblNTRIPStatus.Text = "Disconnected"

        If NTRIPConnectionAttempt > 10000 Then
            btnNTRIPConnect.Visible = True
            NTRIPShouldBeConnected = False
            SaveSetting("NTRIP Should be Connected", "No")
            LogEvent("NTRIP Client is Disconnected, 10000 Failed Connection Attempts.")
        End If
        
        If NTRIPShouldBeConnected Then
            StartNTRIPThreadIn = 10
            PlayAudioAlert() 'Play if we got disconnected, but are going to try to reconnect.
        Else
            btnNTRIPConnect.Text = "Connect"
            btnNTRIPConnect.Visible = True
            boxMountpoint.Enabled = True

            If Not UserCanNotEditSettings Then
                btnNTRIPEdit.Visible = True
            End If
        End If
    End Sub
    Public Sub NTRIPLoop()
        'Pause for a bit in case we just disconnected and are now reconnecting.
        Threading.Thread.Sleep(1000)
        Select Case NTRIPProtocol
            Case 1 'NTRIP Protocol
                Dim NeedsToSendGGA As Boolean = NTRIPStreamRequiresGGA 'This is a thread-local option that can get set to false later if only need to send GGA once.

                'This sub gets called on a new thread, it send/receives data, waits 100ms, then loops.
                If NeedsToSendGGA And Not NTRIPUseManualGGA Then 'Is GGA data required?
                    If MostRecentGGA = "" Then 'Has GGA data been received?
                        NTRIPUpdateUIThread(-1, "", Nothing) 'Waiting for GGA data
                        Do While True
                            If Not MostRecentGGA = "" Then
                                Exit Do
                            End If
                            If Not NTRIPIsConnected Then 'Flag changed, kill the thread
                                NTRIPThread.Abort()
                            End If
                            Threading.Thread.Sleep(100)
                        Loop
                    End If
                End If


                Dim sckt As Net.Sockets.Socket
                Dim lcount As Integer = 97
                NTRIPUpdateUIThread(0, "", Nothing) 'Connecting


                'Connect to server
                sckt = New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
                Try
                    'sckt.Connect(New Net.IPEndPoint(NTRIPCaster, NTRIPPort))
                    sckt.Connect(NTRIPCaster, NTRIPPort)
                Catch ex As Exception
                    NTRIPUpdateUIThread(100, "Server did not respond.", Nothing)
                    NTRIPThread.Abort()
                End Try


                NTRIPUpdateUIThread(1, "", Nothing) 'Connected

                'Build request message
                Dim msg As String = "GET /" & NTRIPMountPoint & " HTTP/1.0" & vbCr & vbLf
                msg += "User-Agent: NTRIP LefebureNTRIPClient/20131124" & vbCr & vbLf
                msg += "Accept: */*" & vbCr & vbLf & "Connection: close" & vbCr & vbLf
                If NTRIPUsername.Length > 0 Then
                    Dim auth As String = ToBase64(NTRIPUsername & ":" & NTRIPPassword)
                    msg += "Authorization: Basic " & auth & vbCr & vbLf 'This line can be removed if no authorization is needed
                End If
                msg += vbCr & vbLf

                'Send request
                Dim data As Byte() = System.Text.Encoding.ASCII.GetBytes(msg)
                sckt.Send(data)
                Threading.Thread.Sleep(100)

                'Wait for response
                'Dim returndata As Byte() = New Byte(255) {}
                Dim responseData As String = ""
                Try
                    For i = 0 To 300 'Wait 30 seconds for a response
                        Threading.Thread.Sleep(100)
                        Dim DataLength As Integer = sckt.Available
                        If DataLength > 0 Then
                            Dim InBytes(DataLength - 1) As Byte
                            sckt.Receive(InBytes, DataLength, Net.Sockets.SocketFlags.None)
                            responseData = System.Text.Encoding.ASCII.GetString(InBytes, 0, InBytes.Length)
                        End If
                        If responseData.Length > 0 Then Exit For
                    Next
                Catch ex As Exception
                    NTRIPUpdateUIThread(100, "Unknown Response.", Nothing)
                    NTRIPThread.Abort()
                End Try


                If responseData.Contains("SOURCETABLE 200 OK") Then
                    'Start of source table was downloaded. Check for more data.
                    For i = 0 To 100 'Wait another 10 seconds for source table
                        Threading.Thread.Sleep(100)
                        Dim DataLength As Integer = sckt.Available
                        If DataLength > 0 Then
                            Dim InBytes(DataLength - 1) As Byte
                            sckt.Receive(InBytes, DataLength, Net.Sockets.SocketFlags.None)
                            responseData += System.Text.Encoding.ASCII.GetString(InBytes, 0, InBytes.Length)
                        End If
                        If responseData.Contains("ENDSOURCETABLE") Then Exit For
                    Next

                    Dim targetfile As String = Application.StartupPath & "\sourcetable.dat"
                    My.Computer.FileSystem.WriteAllText(targetfile, responseData, False)
                    NTRIPUpdateUIThread(101, responseData, Nothing) 'Send on sourcetable for parsing

                    sckt.Disconnect(False)
                    NTRIPUpdateUIThread(100, "Downloaded Source Table", Nothing)
                    NTRIPThread.Abort()
                ElseIf responseData.Contains("401 Unauthorized") Then
                    'Login failed
                    sckt.Disconnect(False)
                    NTRIPUpdateUIThread(100, "Invalid Username or Password.", Nothing)
                    NTRIPThread.Abort()
                ElseIf responseData.Contains("ICY 200 OK") Then
                    NTRIPUpdateUIThread(2, "", Nothing) 'ICY 200 OK, Waiting for data
                    Dim DataNotReceivedFor As Integer = 0
                    Dim KeepRunning As Boolean = True
                    Do While KeepRunning
                        Dim DataLength As Integer = sckt.Available
                        If DataLength = 0 Then
                            DataNotReceivedFor += 1
                            If DataNotReceivedFor > 300 Then
                                'Data not received for 30 seconds. Terminate the connection.
                                KeepRunning = False
                                NTRIPUpdateUIThread(100, "Connection Timed Out.", Nothing)
                                NTRIPThread.Abort()
                            End If
                        Else
                            DataNotReceivedFor = 0
                            Dim InBytes(DataLength - 1) As Byte
                            sckt.Receive(InBytes, DataLength, Net.Sockets.SocketFlags.None)
                            NTRIPUpdateUIThread(3, Nothing, InBytes)
                        End If

                        lcount += 1
                        If lcount = 100 Then
                            If NeedsToSendGGA Then
                                Dim TheGGA As String
                                If NTRIPUseManualGGA Then
                                    TheGGA = GPS.GenerateGPGGAcode() 'This function runs in the NTRIP thread.
                                    'NeedsToSendGGA = False 'Only needs to be once when using a manual GGA
                                Else
                                    TheGGA = MostRecentGGA
                                End If

                                Dim nmeadata As Byte() = System.Text.Encoding.ASCII.GetBytes(TheGGA & vbCrLf)
                                Try
                                    sckt.Send(nmeadata)
                                Catch ex As Exception
                                    NTRIPUpdateUIThread(100, "Error: " & ex.Message, Nothing)
                                End Try
                                'If Not NTRIPResendGGAEvery10Seconds Then
                                'NeedsToSendGGA = False
                                'End If
                            End If
                            lcount = 0
                        End If
                        If Not NTRIPIsConnected Then 'Flag changed, kill the thread
                            sckt.Disconnect(False)
                            KeepRunning = False
                            NTRIPUpdateUIThread(100, "", Nothing)
                            NTRIPThread.Abort()
                        End If
                        Threading.Thread.Sleep(100)
                    Loop
                Else
                    sckt.Disconnect(False)
                    If responseData.Length = 0 Then
                        NTRIPUpdateUIThread(100, "No Response.", Nothing)
                    Else
                        NTRIPUpdateUIThread(100, "Unknown Response.", Nothing)
                    End If
                    NTRIPThread.Abort()
                End If


            Case Else '0 Raw TCP/IP Socket
                Dim sckt As Net.Sockets.Socket
                NTRIPUpdateUIThread(0, "", Nothing) 'Connecting

                'Connect to server
                sckt = New Net.Sockets.Socket(Net.Sockets.AddressFamily.InterNetwork, Net.Sockets.SocketType.Stream, Net.Sockets.ProtocolType.Tcp)
                Try
                    'sckt.Connect(New Net.IPEndPoint(NTRIPCaster, NTRIPPort))
                    sckt.Connect(NTRIPCaster, NTRIPPort)
                Catch ex As Exception
                    NTRIPUpdateUIThread(100, "Server did not respond.", Nothing)
                    NTRIPThread.Abort()
                End Try

                NTRIPUpdateUIThread(1, "", Nothing) 'Connected

                Dim DataNotReceivedFor As Integer = 0
                Dim KeepRunning As Boolean = True
                Do While KeepRunning
                    Dim DataLength As Integer = sckt.Available
                    If DataLength = 0 Then
                        DataNotReceivedFor += 1
                        If DataNotReceivedFor > 300 Then
                            'Data not received for 30 seconds. Terminate the connection.
                            KeepRunning = False
                            NTRIPUpdateUIThread(100, "Connection Timed Out.", Nothing)
                            NTRIPThread.Abort()
                        End If
                    Else
                        DataNotReceivedFor = 0
                        Dim InBytes(DataLength - 1) As Byte
                        sckt.Receive(InBytes, DataLength, Net.Sockets.SocketFlags.None)
                        NTRIPUpdateUIThread(3, Nothing, InBytes)
                    End If

                    If Not NTRIPIsConnected Then 'Flag changed, kill the thread
                        sckt.Disconnect(False)
                        KeepRunning = False
                        NTRIPUpdateUIThread(100, "", Nothing)
                        NTRIPThread.Abort()
                    End If
                    Threading.Thread.Sleep(100)
                Loop
        End Select

    End Sub
    Private Sub NTRIPUpdateUIThread(ByVal Item As Integer, ByVal Value As String, ByVal myBytes() As Byte)
        Try
            Dim uidel As New NTRIPUpdateUIThreadDelegate(AddressOf NTRIPCallBacktoUIThread)
            Dim o(2) As Object
            o(0) = Item
            o(1) = Value
            o(2) = myBytes
            Invoke(uidel, o)
        Catch ex As Exception
        End Try
    End Sub
    Delegate Sub NTRIPUpdateUIThreadDelegate(ByVal Item As Integer, ByVal Value As String, ByVal myBytes() As Byte)
    Private Sub NTRIPCallBacktoUIThread(ByVal Item As Integer, ByVal Value As String, ByVal myBytes() As Byte)
        Select Case Item
            Case -1
                lblNTRIPStatus.Text = "Waiting for NMEA GGA data..."
            Case 0
                lblNTRIPStatus.Text = "Connecting..."
                If NTRIPConnectionAttempt > 1 Then
                    lblNTRIPStatus.Text += " Attempt " & NTRIPConnectionAttempt
                    LogEvent("NTRIP Client is attempting to reconnect, Attempt " & NTRIPConnectionAttempt)
                Else
                    LogEvent("NTRIP Client is attempting to connect.")
                    If NTRIPStreamRequiresGGA And NTRIPUseManualGGA Then
                        LogEvent("NTRIP is using a simulated location of " & NTRIPManualLat & ", " & NTRIPManualLon)
                    End If
                End If
            Case 1
                lblNTRIPStatus.Text = "Connected, Requesting Data..."
                NTRIPByteCount = 0

            Case 2
                lblNTRIPStatus.Text = "Connected, Waiting for Data..."
                pbNTRIP.Value = 0
                pbNTRIP.Visible = True
                LogEvent("NTRIP Client is Connected, Waiting for Data.")

            Case 3
                Try
                    If COMPort.IsOpen Then
                        COMPort.Write(myBytes, 0, myBytes.Length)
                    End If
                Catch ex As Exception
                End Try
                If NTRIPByteCount = 0 Then
                    If UserCanNotDisconnectStuff Then
                        btnNTRIPConnect.Visible = False
                    End If
                    LogEvent("NTRIP Client is receiving data.")
                End If
                NTRIPByteCount += myBytes.Length
                lblNTRIPStatus.Text = "Connected, " & Format(NTRIPByteCount, "###,###,###,##0") & " bytes received."

                Dim remainder As Integer = CInt(NTRIPByteCount) Mod 5000
                remainder = CInt(remainder / 50)
                pbNTRIP.Value = remainder

            Case 100 'Thread commited suicide for some reason.
                If Value = "Invalid Username or Password." Then
                    NTRIPShouldBeConnected = False
                    SaveSetting("NTRIP Should be Connected", "No")
                End If
                If Value = "Unknown Response." Then
                    NTRIPShouldBeConnected = False
                    SaveSetting("NTRIP Should be Connected", "No")
                End If

                If Value = "" Then
                    'lblNTRIPStatus.Text = "Disconnected"
                    LogEvent("NTRIP Client is Disconnected.")
                Else
                    'lblNTRIPStatus.Text = "Disconnected, " & Value
                    LogEvent("NTRIP Client is Disconnected, " & Value)
                End If

                StopNTRIP()

            Case 101 'Got Source Table, parse it
                ParseSourceTable(Value)
                btnNTRIPConnect.Visible = True
                LogEvent("NTRIP Client downloaded the Source Table.")
                NTRIPShouldBeConnected = False
                SaveSetting("NTRIP Should be Connected", "No")
                StopNTRIP()

        End Select
    End Sub
    Private Function ToBase64(ByVal str As String) As String
        Dim asciiEncoding As System.Text.Encoding = System.Text.Encoding.ASCII
        Dim byteArray As Byte() = New Byte(asciiEncoding.GetByteCount(str) - 1) {}
        byteArray = asciiEncoding.GetBytes(str)
        Return Convert.ToBase64String(byteArray, 0, byteArray.Length)
    End Function

    Private Sub btnClearLog_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnClearLog.Click
        rtbEvents.Text = ""
    End Sub




    'This should only run once a week, and should ask the user if they want to update.
    Private WithEvents BackGroundWorkerCheckForUpdates As System.ComponentModel.BackgroundWorker = New System.ComponentModel.BackgroundWorker
    Public Sub CheckForUpdates(ByVal ShowUser As Boolean)
        BackGroundWorkerCheckForUpdates.Dispose()
        BackGroundWorkerCheckForUpdates.RunWorkerAsync(ShowUser)
        SaveSetting("Last Checked for Updates", Now)
    End Sub
    Private Sub BackGroundWorkerCheckForUpdates_DoWork(ByVal sender As System.Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BackGroundWorkerCheckForUpdates.DoWork
        Dim ShowUser As Boolean = CBool(e.Argument)

        'see if update program is running
        If Process.GetProcessesByName("LefebureUpdate").Length > 0 Then
            'switch to it
            Dim cancelDelegate As New ReportDownloadStatusSafe(AddressOf ReportDownloadStatus)
            Me.Invoke(cancelDelegate, "Updater Already Running")

        Else 'update program is not running
            'see if update program exists
            Dim UpdateFile As String = Application.StartupPath & "\LefebureUpdate.exe"
            If Not My.Computer.FileSystem.FileExists(UpdateFile) Then
                'doesn't exist. try to download it.

                'Creating the request and getting the response
                Dim theRequest As System.Net.WebRequest
                Dim theResponse As System.Net.WebResponse
                Try 'Checks if the file exist
                    theRequest = System.Net.WebRequest.Create("http://lefebure.com/software/updates/LefebureUpdate.exe")
                    theResponse = theRequest.GetResponse
                Catch ex As Exception
                    Dim cancelDelegate As New ReportDownloadStatusSafe(AddressOf ReportDownloadStatus)
                    Me.Invoke(cancelDelegate, "Updater Download Failed")
                    Exit Sub
                End Try
                Dim length As Long = theResponse.ContentLength 'Size of the response (in bytes)

                Dim safedelegate As New ReportDownloadStatusSafe(AddressOf ReportDownloadStatus)
                Me.Invoke(safedelegate, "Downloading Updater") 'Invoke the TreadsafeDelegate
                Dim speedtimer As New Stopwatch 'To calculate the download speed
                Dim currentspeed As Double = -1
                Dim readings As Integer = 0
                Dim nRead As Integer = 0
                Dim FileBytes(-1) As Byte
                Do
                    If BackGroundWorkerCheckForUpdates.CancellationPending Then 'If user abort download
                        Exit Do
                    End If
                    speedtimer.Start()
                    Dim readBytes(4095) As Byte
                    Dim bytesread As Integer = theResponse.GetResponseStream.Read(readBytes, 0, 4096)
                    nRead += bytesread
                    Dim percent As Short = CShort((nRead * 100) / length)
                    Me.Invoke(safedelegate, "Downloading Updater (" & percent & "%)")
                    If bytesread = 0 Then Exit Do

                    If bytesread < 4096 Then ReDim Preserve readBytes(bytesread - 1)
                    ReDim Preserve FileBytes(nRead - 1)
                    readBytes.CopyTo(FileBytes, (nRead - 1) - (bytesread - 1))
                    speedtimer.Stop()
                    readings += 1
                    If readings >= 5 Then 'For increase precision, the speed it's calculated only every five cycles
                        currentspeed = 20480 / (speedtimer.ElapsedMilliseconds / 1000)
                        speedtimer.Reset()
                        readings = 0
                    End If
                Loop

                'save the file
                Me.Invoke(safedelegate, "Saving Updater") 'Invoke the TreadsafeDelegate
                Dim oFileStream As System.IO.FileStream = New System.IO.FileStream(UpdateFile, System.IO.FileMode.Create)
                oFileStream.Write(FileBytes, 0, FileBytes.Length)
                oFileStream.Close()
                Me.Invoke(safedelegate, "Updater Saved") 'Invoke the TreadsafeDelegate

                'Close the streams
                theResponse.GetResponseStream.Close()
            End If


            'spawn update program (with silent or loud switch)
            If My.Computer.FileSystem.FileExists(UpdateFile) Then
                Dim p As New Process
                Dim pi As New ProcessStartInfo
                If Not ShowUser Then
                    pi.Arguments = "/silent"
                End If
                pi.FileName = UpdateFile
                p.StartInfo = pi
                p.Start()
                Dim safedelegate As New ReportDownloadStatusSafe(AddressOf ReportDownloadStatus)
                Me.Invoke(safedelegate, "Updater Started") 'Invoke the TreadsafeDelegate
            End If
        End If
    End Sub
    Delegate Sub ReportDownloadStatusSafe(ByVal status As String)
    Private Sub ReportDownloadStatus(ByVal status As String)
        'lblDLStatus.Text = status
    End Sub


    Private Sub btnToggleBottomPayne_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnToggleBottomPayne.Click
        If btnToggleBottomPayne.Text = "History" Then
            btnToggleBottomPayne.Text = "Elevations"
            rtbEvents.Visible = False
            btnClearLog.Visible = False
            btnElevStartPause.Visible = True
            btnElevReset.Visible = True
            lblElevMax.Visible = True
            lblElevMin.Visible = True
            lblElevNow.Visible = True
            lblElevRange.Visible = True
        Else
            btnToggleBottomPayne.Text = "History"
            rtbEvents.Visible = True
            btnClearLog.Visible = True
            btnElevStartPause.Visible = False
            btnElevReset.Visible = False
            lblElevMax.Visible = False
            lblElevMin.Visible = False
            lblElevNow.Visible = False
            lblElevRange.Visible = False
        End If
    End Sub



    Dim ElevIsRecording As Boolean = False
    Dim ElevIsFirstPoint As Boolean = True
    Dim ElevLastE As Decimal
    Dim ElevMaxE As Decimal = 0
    Dim ElevMinE As Decimal = 0

    Private Sub btnElevStartPause_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnElevStartPause.Click
        If btnElevStartPause.Text = "Start" Then
            ElevStartRecording()
        Else
            ElevPauseRecording()
        End If
    End Sub
    Public Sub ElevStartRecording()
        btnElevStartPause.Text = "Pause"
        ElevIsRecording = True
    End Sub
    Public Sub ElevPauseRecording()
        btnElevStartPause.Text = "Start"
        ElevIsRecording = False
    End Sub

    Private Sub btnElevReset_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnElevReset.Click
        ElevPauseRecording()
        ElevIsFirstPoint = True
        ElevMaxE = 0
        ElevMinE = 0
    End Sub

    Public Sub ElevNewE(ByVal NewE As Decimal)
        ElevLastE = NewE
        If ElevIsRecording Then
            If ElevIsFirstPoint Then
                ElevMaxE = NewE
                ElevMinE = NewE
                ElevIsFirstPoint = False
            End If

            If NewE > ElevMaxE Then
                ElevMaxE = NewE
            End If
            If NewE < ElevMinE Then
                ElevMinE = NewE
            End If
        End If

        If btnToggleBottomPayne.Text = "Elevations" Then RefreshElevLables()
    End Sub

    Private Sub RefreshElevLables()
        Dim YTop As Integer = btnElevReset.Location.Y
        Dim YBottom As Integer = btnClearLog.Location.Y
        Dim YRange As Integer = YBottom - YTop

        Dim EMost As Decimal = ElevMaxE
        If ElevLastE > ElevMaxE Then EMost = ElevLastE
        Dim ELeast As Decimal = ElevMinE
        If ElevLastE < ElevMinE Then ELeast = ElevLastE
        Dim ERange As Decimal = EMost - ELeast

        Dim YMax As Integer = CInt(((EMost - ElevMaxE) / ERange) * YRange) + YTop
        Dim YMin As Integer = CInt(((EMost - ElevMinE) / ERange) * YRange) + YTop
        Dim YLast As Integer = CInt(((EMost - ElevLastE) / ERange) * YRange) + YTop
        Dim MinMaxDiff As Integer = YMin - YMax

        If MinMaxDiff < 38 Then 'Max and min need to be 38 apart
            YMax -= CInt(38 - MinMaxDiff) / 2
            YMin += CInt(38 - MinMaxDiff) / 2
            If YMax < YTop Then
                Dim Diff As Integer = YTop - YMax
                YMax += Diff
                YMin += Diff
            End If
            If YMin > YBottom Then
                Dim Diff As Integer = YMin - YBottom
                YMax -= Diff
                YMin -= Diff
            End If
        End If

        ERange = ElevMaxE - ElevMinE

        lblElevMax.Location = New Point(6, YMax)
        lblElevMin.Location = New Point(6, YMin)
        lblElevNow.Location = New Point(330, YLast)

        'Recycle variables
        EMost = ElevLastE - ElevMaxE
        ELeast = ElevLastE - ElevMinE
        Dim ShowPlus As String = ""
        If EMost > 0 Then ShowPlus = "+"
        lblElevMax.Text = "Max: " & Format(ElevMaxE, "#.000") & "m (" & ShowPlus & Format(EMost, "#.00") & ")"
        ShowPlus = ""
        If ELeast > 0 Then ShowPlus = "+"
        lblElevMin.Text = "Min: " & Format(ElevMinE, "#.000") & "m (" & ShowPlus & Format(ELeast, "#.00") & ")"
        lblElevNow.Text = Format(ElevLastE, "#.000") & "m"

        lblElevRange.Text = "Range:" & vbCrLf & Format(ERange, "#.000") & "m"
    End Sub

End Class
