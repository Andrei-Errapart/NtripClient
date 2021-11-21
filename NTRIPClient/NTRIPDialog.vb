Imports System.Windows.Forms

Public Class NTRIPDialog

    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub


    Private Sub boxProtocol_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles boxProtocol.SelectionChangeCommitted
        RefreshDisplayedItems()
    End Sub

    Private Sub boxManualGGA_SelectionChangeCommitted(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles boxManualGGA.SelectionChangeCommitted
        RefreshDisplayedItems()
    End Sub

    Public Sub RefreshDisplayedItems()
        If boxProtocol.SelectedIndex = 0 Then 'Raw TCP/IP
            lblUsername.Visible = False
            tbUsername.Visible = False
            lblPassword.Visible = False
            tbPassword.Visible = False

            GroupBox1.Text = "TCP/IP Server Settings"

            GroupBox2.Visible = False

        Else 'NTRIP
            lblUsername.Visible = True
            tbUsername.Visible = True
            lblPassword.Visible = True
            tbPassword.Visible = True

            GroupBox1.Text = "NTRIP Caster Settings"

            GroupBox2.Visible = True

            If boxManualGGA.SelectedIndex = 0 Then
                'lblLat.Text = "Send in position data:"
                'boxSendGGAFreq.Visible = True
                lblLat.Visible = False
                lblLon.Visible = False
                tbLatitude.Visible = False
                tbLongitude.Visible = False
            Else
                'lblLat.Text = "Latitude:"
                'boxSendGGAFreq.Visible = False
                lblLat.Visible = True
                lblLon.Visible = True
                tbLatitude.Visible = True
                tbLongitude.Visible = True
            End If
        End If
    End Sub
    
End Class
