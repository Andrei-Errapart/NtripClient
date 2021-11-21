Public Class GPS
    Public LastFixQuality As Integer = -1
    Public LastSatsTracked As Integer = 0
    Public LastStationID As String = "0"
    Public LastCorrAge As String = "-"
    Public LastHDOP As Decimal = 0
    Public LastVDOP As Single = 0
    Public LastPDOP As Single = 0
    Public LastAltitude As Single = -1
    Public LastHeading As Single = 0
    Public LastSpeed As Single = 0
    Public LastSpeedSmoothed As Single = 0
    Private RecentSpeeds() As Single = {0, 0, 0, 0, 0, 0, 0, 0, 0, 0}
    Public RecordQueue As String = ""

    Public Disp2 As String = "age"
    Public Disp3 As String = "speed-mph-smoothed"


    'age
    'hdop
    'vdop
    'pdop
    'elevation-feet
    'elevation-meters
    'speed-mph
    'speed-mph-smoothed
    'speed-kmh
    'speed-kmh-smoothed
    'heading

    'Speed and Heading data require RMC sentences
    'VDOP and PDOP data require GSA sentences ???


    Public Sub ProcessNMEAdata(ByVal x As String)
        'GPRMC (Required) contains time, lat, lon, speed, heading, date - We use RMC for **ALL** of the logging and steering commands
        'GPGGA (Required) contains fix quality, # of sats tracked, HDOP, and Altitude. Only useful to tell us when we lose sat signal.
        'GPGSV (Not Required) contains location and signal strength about up to 4 of the satellites in view. This is just used to display what sats are where.
        'GPGSA (Not Required) contains fix type, sat PRNs used, PDOP, HDOP, and VDOP. This just compliments GPGSV.

        Dim charlocation As Integer = x.LastIndexOf("$") 'Find location of the last $
        If charlocation = -1 Or charlocation + 1 > x.Length - 5 Then Exit Sub 'no $ found or not enough data left
        x = Mid(x, charlocation + 1) 'drop characters before the $

        charlocation = x.IndexOf("*") 'Find location of first *
        If charlocation = -1 Then Exit Sub 'no * found
        If x.Length < charlocation + 3 Then 'there aren't 2 characters after the *
            Exit Sub
        ElseIf x.Length > charlocation + 3 Then 'there is extra data after the *
            x = Mid(x, 1, charlocation + 3) 'remove the extra data after 2 chars after the *
        End If
        If x.Length < 8 Then Exit Sub 'not enough data left

        Dim aryNMEALine() As String = Split(x, "*")
        'lets see if the checksum matches the stuff before the astrix
        If CalculateChecksum(Replace(aryNMEALine(0), "$", "")) = aryNMEALine(1) Then
            'Checksum matches, send it to the respective subroutine for processing.
            If Left(aryNMEALine(0), 6) = "$GPRMC" Or Left(aryNMEALine(0), 6) = "$GNRMC" Then
                ProcessGPRMC(aryNMEALine(0))
            End If
            If Left(aryNMEALine(0), 6) = "$GPGGA" Or Left(aryNMEALine(0), 6) = "$GNGGA" Then
                MainForm.MostRecentGGA = x
                ProcessGPGGA(aryNMEALine(0))
                If MainForm.WriteNMEAToFile Then RecordLine(x)
            End If
            If Left(aryNMEALine(0), 6) = "$GPGSA" Or Left(aryNMEALine(0), 6) = "$GNGSA" Then
                ProcessGPGSA(aryNMEALine(0))
            End If
        End If
    End Sub

    Private Sub ProcessGPGGA(ByVal code As String)
        'This is a GGA line and has 11+ fields; check that we have at least 11
        '$GPGGA,123519,4807.038,N,01131.000,E,1,08,0.9,545.4,M,46.9,M,,*47
        '0     ,1     ,2       ,3,4        ,5,6,7 ,8  ,9   ,10,11  ,12
        '$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A

        Dim InLatitude As Double = 0
        Dim InLongitude As Double = 0

        Dim aryNMEAString() As String = Split(code, ",")
        If UBound(aryNMEAString) > 13 Then 'we have at least 14 fields.
            'Parse Lat/Lon
            If aryNMEAString(2) <> "" And aryNMEAString(3) <> "" And aryNMEAString(4) <> "" And aryNMEAString(5) <> "" Then
                If IsNumeric(aryNMEAString(2)) And IsNumeric(aryNMEAString(4)) Then
                    Dim snglat As Double = (CDbl(aryNMEAString(2)) / 100)
                    Dim snglatmins As Double = snglat Mod 1
                    snglat = snglat - snglatmins
                    snglatmins = snglatmins * 100 / 60
                    InLatitude = snglat + snglatmins
                    If aryNMEAString(3) = "S" Then
                        InLatitude = 0 - InLatitude
                    End If

                    Dim snglon As Double = (CDbl(aryNMEAString(4)) / 100)
                    Dim snglonmins As Double = snglon Mod 1
                    snglon = snglon - snglonmins
                    snglonmins = snglonmins * 100 / 60
                    InLongitude = snglon + snglonmins
                    If aryNMEAString(5) = "W" Then
                        InLongitude = 0 - InLongitude
                    End If
                Else
                    'non-numeric data in gga message
                    MainForm.lblStatus.Text = "Bad GGA data"
                End If
            Else
                'empty GGA message
                MainForm.lblStatus.Text = "Empty GGA data"
            End If

         

            If IsNumeric(aryNMEAString(6)) And IsNumeric(aryNMEAString(7)) And IsNumeric(aryNMEAString(8)) And IsNumeric(aryNMEAString(9)) Then
                Dim InFixQuality As Integer = CInt(aryNMEAString(6))
                Dim InSatsTracked As Integer = CInt(aryNMEAString(7))
                Dim InHDOP As Decimal = CDec(aryNMEAString(8))
                Dim InAltitude As Single = CSng(aryNMEAString(9))

                If Not InFixQuality = LastFixQuality Or Not InSatsTracked = LastSatsTracked Then 'fix quality has changed
                    Dim gpstype As String
                    Dim shorttype As String
                    Select Case InFixQuality
                        Case 1 'GPS fix (SPS)
                            gpstype = "GPS fix (No Differential Correction)"
                            shorttype = "GPS"
                        Case 2 'DGPS fix
                            gpstype = "DGPS"
                            shorttype = "DGPS"
                        Case 3 'PPS fix
                            gpstype = "PPS Fix"
                            shorttype = "PPS"
                        Case 4 'Real Time Kinematic
                            gpstype = "RTK"
                            shorttype = "RTK"
                        Case 5 'Float RTK
                            gpstype = "Float RTK"
                            shorttype = "FloatRTK"
                        Case 6 'estimated (dead reckoning) (2.3 feature)
                            gpstype = "Estimated"
                            shorttype = "Estimated"
                        Case 7 'Manual input mode
                            gpstype = "Manual Input Mode"
                            shorttype = "Manual"
                        Case 8 'Simulation mode
                            gpstype = "Simulation"
                            shorttype = "Simulation"
                        Case 9 'WAAS
                            gpstype = "WAAS"
                            shorttype = "WAAS"
                        Case Else '0 = invalid
                            gpstype = "Invalid"
                            shorttype = "Invalid"
                    End Select

                    MainForm.lblStatus.Text = shorttype & ":" & InSatsTracked

                    If Not InFixQuality = LastFixQuality Then
                        If LastFixQuality = -1 Then 'This only happens on startup
                            LastFixQuality = 0
                        Else 'This happens every time except at startup
                            MainForm.PlayAudioAlert()
                        End If

                        Dim status As String = ""
                        If InFixQuality = 5 And LastFixQuality = 4 Then
                            status = "Degraded"
                        ElseIf InFixQuality = 4 And LastFixQuality = 5 Then
                            status = "Increased"
                        ElseIf InFixQuality = 8 Or LastFixQuality = 8 Then
                            status = "Changed"
                        ElseIf InFixQuality > LastFixQuality Then
                            status = "Increased"
                        Else
                            status = "Degraded"
                        End If
                        MainForm.LogEvent("GPS Fix Quality " & status & " from " & LastFixQuality & " to " & InFixQuality & " (" & gpstype & ")")
                        If LastFixQuality = 4 Then 'was RTK
                            MainForm.LogEvent("Correction Age was " & LastCorrAge)
                        End If
                        If LastHDOP = InHDOP Then
                            MainForm.LogEvent("H-DOP unchanged at " & InHDOP)
                        Else
                            MainForm.LogEvent("H-DOP was " & LastHDOP & " and now is " & InHDOP)
                        End If


                        LastFixQuality = InFixQuality
                    End If

                    If Not InSatsTracked = LastSatsTracked Then
                        'sat count has changed

                        If InSatsTracked > LastSatsTracked Then
                            MainForm.LogEvent("Number of Satellites tracked Increased from " & LastSatsTracked & " to " & InSatsTracked)
                        Else
                            MainForm.LogEvent("Number of Satellites tracked Decreased from " & LastSatsTracked & " to " & InSatsTracked)
                        End If

                        LastSatsTracked = InSatsTracked
                    End If
                End If


                If Not InHDOP = LastHDOP Then 'hdop has changed
                    LastHDOP = InHDOP
                    If Disp2 = "hdop" Then MainForm.lbl2.Text = "HDOP:" & LastHDOP
                    If Disp3 = "hdop" Then MainForm.lbl3.Text = "HDOP:" & LastHDOP
                End If

                If Not InAltitude = LastAltitude Then 'altitude has changed
                    LastAltitude = InAltitude
                    If Disp2 = "elevation-meters" Then MainForm.lbl2.Text = Format(LastAltitude, "#.00") & "m"
                    If Disp3 = "elevation-meters" Then MainForm.lbl3.Text = Format(LastAltitude, "#.00") & "m"
                    If Disp2 = "elevation-feet" Then MainForm.lbl2.Text = Format(LastAltitude * 3.2808399, "#.0") & "'"
                    If Disp3 = "elevation-feet" Then MainForm.lbl3.Text = Format(LastAltitude * 3.2808399, "#.0") & "'"
                    If InFixQuality = 4 Then MainForm.ElevNewE(CDec(InAltitude))
                End If
            End If


            Dim inCorrAge As String = aryNMEAString(13)
            If inCorrAge.Length = 0 Then inCorrAge = "N/A"
            If Not LastCorrAge = inCorrAge Then
                LastCorrAge = inCorrAge
                If Disp2 = "age" Then MainForm.lbl2.Text = "Age:" & inCorrAge
                If Disp3 = "age" Then MainForm.lbl3.Text = "Age:" & inCorrAge
            End If


            Dim inStationID As String = aryNMEAString(14)
            If inStationID.Length = 0 Then inStationID = "0"
            If Not LastStationID = inStationID Then
                MainForm.LogEvent("Correction Station ID changed from " & LastStationID & " to " & inStationID)
                LastStationID = inStationID
                MainForm.PlayAudioAlert()
            End If
        End If
    End Sub
    Private Sub ProcessGPGSA(ByVal code As String)
        Dim aryNMEAString() As String = Split(code, ",")
        If UBound(aryNMEAString) >= 17 Then 'we have at least 15 fields.
            If IsNumeric(aryNMEAString(15)) And IsNumeric(aryNMEAString(16)) And IsNumeric(aryNMEAString(17)) Then
                Dim InPDOP As Single = CSng(aryNMEAString(15))
                Dim InHDOP As Single = CSng(aryNMEAString(16))
                Dim InVDOP As Single = CSng(aryNMEAString(17))

                If Not InHDOP = LastPDOP Then 'pdop has changed
                    LastPDOP = InPDOP
                    If Disp2 = "pdop" Then MainForm.lbl2.Text = "PDOP:" & LastPDOP
                    If Disp3 = "pdop" Then MainForm.lbl3.Text = "PDOP:" & LastPDOP
                End If
                If Not InHDOP = LastHDOP Then 'hdop has changed
                    LastHDOP = InHDOP
                    If Disp2 = "hdop" Then MainForm.lbl2.Text = "HDOP:" & LastHDOP
                    If Disp3 = "hdop" Then MainForm.lbl3.Text = "HDOP:" & LastHDOP
                End If
                If Not InVDOP = LastVDOP Then 'vdop has changed
                    LastVDOP = InVDOP
                    If Disp2 = "vdop" Then MainForm.lbl2.Text = "VDOP:" & LastVDOP
                    If Disp3 = "vdop" Then MainForm.lbl3.Text = "VDOP:" & LastVDOP
                End If
            End If
        End If
    End Sub
    Private Sub ProcessGPRMC(ByVal code As String)
        'This is a RMC line and has 9+ fields; check that it's active (good data)
        '$GPRMC,123519,A,4807.038,N,01131.000,E,022.4,084.4,230394,003.1,W*6A
        '0     ,1     ,2,3       ,4,5        ,6,7    ,8    ,9     ,10   ,11

        Dim aryNMEAString() As String = Split(code, ",")

        If aryNMEAString(7) <> "" Then
            If IsNumeric(aryNMEAString(7)) Then
                LastSpeed = CSng(aryNMEAString(7)) * 1.852 'Convert knots to km/h

                Dim speedsum As Single = LastSpeed
                For i = 0 To 8
                    RecentSpeeds(i) = RecentSpeeds(i + 1)
                    speedsum += RecentSpeeds(i)
                Next
                RecentSpeeds(9) = LastSpeed
                LastSpeedSmoothed = speedsum / 10

                If Disp2 = "speed-mph" Then MainForm.lbl2.Text = Format(LastSpeed * 0.621371192, "0.0") & " MPH"
                If Disp3 = "speed-mph" Then MainForm.lbl3.Text = Format(LastSpeed * 0.621371192, "0.0") & " MPH"
                If Disp2 = "speed-mph-smoothed" Then MainForm.lbl2.Text = Format(LastSpeedSmoothed * 0.621371192, "0.0") & " MPH"
                If Disp3 = "speed-mph-smoothed" Then MainForm.lbl3.Text = Format(LastSpeedSmoothed * 0.621371192, "0.0") & " MPH"

                If Disp2 = "speed-kmh" Then MainForm.lbl2.Text = Format(LastSpeed, "0.0") & " km/h"
                If Disp3 = "speed-kmh" Then MainForm.lbl3.Text = Format(LastSpeed, "0.0") & " km/h"
                If Disp2 = "speed-kmh-smoothed" Then MainForm.lbl2.Text = Format(LastSpeedSmoothed, "0.0") & " km/h"
                If Disp3 = "speed-kmh-smoothed" Then MainForm.lbl3.Text = Format(LastSpeedSmoothed, "0.0") & " km/h"
            End If
        End If

        If aryNMEAString(8) <> "" Then
            If IsNumeric(aryNMEAString(8)) Then
                LastHeading = CSng(aryNMEAString(8))
                If Disp2 = "heading" Then MainForm.lbl2.Text = Format(LastHeading, "0.0") & Chr(176)
                If Disp3 = "heading" Then MainForm.lbl3.Text = Format(LastHeading, "0.0") & Chr(176)
            End If
        End If
    End Sub


    Private Sub RecordLine(ByVal Line As String)
        RecordQueue += Line & vbCrLf
        If RecordQueue.Length > 8000 Then
            WriteRecordingQueueToFile()
        End If
    End Sub

    Public Sub WriteRecordingQueueToFile()
        If RecordQueue.Length > 0 Then
            Dim nmeafolder As String = Application.StartupPath & "\NMEA"
            If Not My.Computer.FileSystem.DirectoryExists(nmeafolder) Then
                Try
                    My.Computer.FileSystem.CreateDirectory(nmeafolder)
                Catch ex As Exception
                End Try
            End If

            Dim nmeafile As String = nmeafolder & "\" & Year(Now) & Format(Month(Now), "00") & Format(DatePart(DateInterval.Day, Now), "00") & ".txt"
            Try
                My.Computer.FileSystem.WriteAllText(nmeafile, RecordQueue, True)
                RecordQueue = "" 'Clear that out
            Catch ex As Exception
            End Try
        End If
    End Sub



    Public Function GenerateGPGGAcode() As String
        Dim posnum As Double = 0
        Dim minutes As Double = 0

        Dim UTCTime As Date = Date.UtcNow

        '$GPGGA,052158,4158.7333,N,09147.4277,W,2,08,3.1,260.4,M,-32.6,M,,*79

        Dim mycode As String = "GPGGA,"
        If Hour(UTCTime) < "10" Then
            mycode = mycode & "0"
        End If
        mycode = mycode & Hour(UTCTime)
        If Minute(UTCTime) < "10" Then
            mycode = mycode & "0"
        End If
        mycode = mycode & Minute(UTCTime)
        If Second(UTCTime) < "10" Then
            mycode = mycode & "0"
        End If
        mycode = mycode & Second(UTCTime)
        mycode = mycode & ","

        posnum = Math.Abs(MainForm.NTRIPManualLat)
        minutes = posnum Mod 1
        posnum = posnum - minutes
        minutes = minutes * 60
        posnum = (posnum * 100) + minutes
        mycode = mycode & posnum.ToString("0000.00####", System.Globalization.CultureInfo.InvariantCulture)

        If MainForm.NTRIPManualLat > 0 Then
            mycode = mycode & ",N,"
        Else
            mycode = mycode & ",S,"
        End If

        posnum = Math.Abs(MainForm.NTRIPManualLon)
        minutes = posnum Mod 1
        posnum = posnum - minutes
        minutes = minutes * 60
        posnum = (posnum * 100) + minutes
        mycode = mycode & posnum.ToString("00000.00####", System.Globalization.CultureInfo.InvariantCulture)

        If MainForm.NTRIPManualLon > 0 Then
            mycode = mycode & ",E,"
        Else
            mycode = mycode & ",W,"
        End If


        mycode = mycode & "4,10,1,200,M,1,M,"

        mycode = mycode & (Second(Now) Mod 6) + 3 & ",0"
        mycode = "$" & mycode & "*" & CalculateChecksum(mycode)   'Add checksum data
        Return mycode
    End Function




    Public Function CalculateChecksum(ByVal sentence As String) As String
        ' Calculates the checksum for a sentence
        ' Loop through all chars to get a checksum
        Dim Character As Char
        Dim Checksum As Integer
        For Each Character In sentence
            Select Case Character
                Case "$"c
                    ' Ignore the dollar sign
                Case "*"c
                    ' Stop processing before the asterisk
                    Exit For
                Case Else
                    ' Is this the first value for the checksum?
                    If Checksum = 0 Then
                        ' Yes. Set the checksum to the value
                        Checksum = Convert.ToByte(Character)
                    Else
                        ' No. XOR the checksum with this character's value
                        Checksum = Checksum Xor Convert.ToByte(Character)
                    End If
            End Select
        Next
        ' Return the checksum formatted as a two-character hexadecimal
        Return Checksum.ToString("X2")
    End Function

End Class
