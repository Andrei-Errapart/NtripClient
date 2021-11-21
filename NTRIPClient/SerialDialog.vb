Imports System.Windows.Forms

Public Class SerialDialog
    Private Sub boxReceiverType_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles boxReceiverType.SelectionChangeCommitted
        RedisplayAutoConfigOptions(boxReceiverType.SelectedIndex)
    End Sub

    Public Sub RedisplayAutoConfigOptions(ByVal ReceiverType As Integer)
        Select Case ReceiverType
            Case 1
                boxCorrDataType.Items.Clear()
                boxCorrDataType.Items.Add("RTCM v2")
                boxCorrDataType.Items.Add("RTCM v3")
                boxCorrDataType.Items.Add("CMR or CMR+")
                boxCorrDataType.Items.Add("RTCA")
                boxCorrDataType.Items.Add("OmniSTAR")
                boxCorrDataType.Items.Add("NovAtel")
                Select Case LCase(MainForm.ReceiverCorrDataType)
                    Case "rtcm"
                        boxCorrDataType.SelectedIndex = 0
                    Case "rtcmv3"
                        boxCorrDataType.SelectedIndex = 1
                    Case "cmr"
                        boxCorrDataType.SelectedIndex = 2
                    Case "rtca"
                        boxCorrDataType.SelectedIndex = 3
                    Case "omnistar"
                        boxCorrDataType.SelectedIndex = 4
                    Case Else
                        boxCorrDataType.SelectedIndex = 5
                End Select
                lblCorrDataType.Visible = True
                boxCorrDataType.Visible = True

                Select Case MainForm.ReceiverMessageRate
                    Case 5
                        boxMsgRate.SelectedIndex = 1
                    Case 10
                        boxMsgRate.SelectedIndex = 2
                    Case Else
                        boxMsgRate.SelectedIndex = 0
                End Select

                lblMsgRate.Visible = True
                boxMsgRate.Visible = True


            Case Else
                lblCorrDataType.Visible = False
                boxCorrDataType.Visible = False

                lblMsgRate.Visible = False
                boxMsgRate.Visible = False
        End Select
    End Sub



    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

   
    
End Class
