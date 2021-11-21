Imports System.Windows.Forms

Public Class OptionsDialog
    Private Sub btnAudioHelp_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnAudioHelp.Click
        MsgBox("To select an audio alert, you will need to put a .wav file in the same folder as this application.")
    End Sub


    Private Sub OK_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles OK_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.OK
        Me.Close()
    End Sub

    Private Sub Cancel_Button_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Cancel_Button.Click
        Me.DialogResult = System.Windows.Forms.DialogResult.Cancel
        Me.Close()
    End Sub

   
    Private Sub btnCheckForUpdatesNow_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btnCheckForUpdatesNow.Click
        MainForm.CheckForUpdates(True)
    End Sub
End Class
