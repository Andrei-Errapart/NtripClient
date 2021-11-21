<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class MainForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.components = New System.ComponentModel.Container()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(MainForm))
        Me.Label1 = New System.Windows.Forms.Label()
        Me.LineTop = New System.Windows.Forms.Label()
        Me.lblNTRIPStream = New System.Windows.Forms.Label()
        Me.lblNTRIPStatus = New System.Windows.Forms.Label()
        Me.boxMountpoint = New System.Windows.Forms.ComboBox()
        Me.pbNTRIP = New System.Windows.Forms.ProgressBar()
        Me.btnNTRIPConnect = New System.Windows.Forms.Button()
        Me.btnSerialConnect = New System.Windows.Forms.Button()
        Me.lblSerialStatus = New System.Windows.Forms.Label()
        Me.btnSerialEdit = New System.Windows.Forms.Button()
        Me.btnNTRIPEdit = New System.Windows.Forms.Button()
        Me.Label3 = New System.Windows.Forms.Label()
        Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
        Me.Label4 = New System.Windows.Forms.Label()
        Me.btnClearLog = New System.Windows.Forms.Button()
        Me.lblStatus = New System.Windows.Forms.Label()
        Me.lbl2 = New System.Windows.Forms.Label()
        Me.rtbEvents = New System.Windows.Forms.RichTextBox()
        Me.lbl3 = New System.Windows.Forms.Label()
        Me.btnOptions = New System.Windows.Forms.Button()
        Me.btnToggleBottomPayne = New System.Windows.Forms.Button()
        Me.btnElevStartPause = New System.Windows.Forms.Button()
        Me.btnElevReset = New System.Windows.Forms.Button()
        Me.lblElevMax = New System.Windows.Forms.Label()
        Me.lblElevMin = New System.Windows.Forms.Label()
        Me.lblElevNow = New System.Windows.Forms.Label()
        Me.lblElevRange = New System.Windows.Forms.Label()
        Me.SuspendLayout()
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(42, 60)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(100, 24)
        Me.Label1.TabIndex = 0
        Me.Label1.Text = "Serial Port:"
        '
        'LineTop
        '
        Me.LineTop.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LineTop.BackColor = System.Drawing.Color.Red
        Me.LineTop.Location = New System.Drawing.Point(0, 208)
        Me.LineTop.Margin = New System.Windows.Forms.Padding(0)
        Me.LineTop.Name = "LineTop"
        Me.LineTop.Size = New System.Drawing.Size(784, 2)
        Me.LineTop.TabIndex = 8
        '
        'lblNTRIPStream
        '
        Me.lblNTRIPStream.AutoSize = True
        Me.lblNTRIPStream.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNTRIPStream.ForeColor = System.Drawing.Color.White
        Me.lblNTRIPStream.Location = New System.Drawing.Point(8, 99)
        Me.lblNTRIPStream.Name = "lblNTRIPStream"
        Me.lblNTRIPStream.Size = New System.Drawing.Size(134, 24)
        Me.lblNTRIPStream.TabIndex = 9
        Me.lblNTRIPStream.Text = "NTRIP Stream:"
        '
        'lblNTRIPStatus
        '
        Me.lblNTRIPStatus.AutoSize = True
        Me.lblNTRIPStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblNTRIPStatus.ForeColor = System.Drawing.Color.White
        Me.lblNTRIPStatus.Location = New System.Drawing.Point(140, 135)
        Me.lblNTRIPStatus.Name = "lblNTRIPStatus"
        Me.lblNTRIPStatus.Size = New System.Drawing.Size(126, 24)
        Me.lblNTRIPStatus.TabIndex = 11
        Me.lblNTRIPStatus.Text = "Disconnected"
        '
        'boxMountpoint
        '
        Me.boxMountpoint.BackColor = System.Drawing.Color.White
        Me.boxMountpoint.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.boxMountpoint.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.boxMountpoint.FormattingEnabled = True
        Me.boxMountpoint.Location = New System.Drawing.Point(144, 94)
        Me.boxMountpoint.Name = "boxMountpoint"
        Me.boxMountpoint.Size = New System.Drawing.Size(388, 33)
        Me.boxMountpoint.TabIndex = 43
        '
        'pbNTRIP
        '
        Me.pbNTRIP.Location = New System.Drawing.Point(144, 172)
        Me.pbNTRIP.Name = "pbNTRIP"
        Me.pbNTRIP.Size = New System.Drawing.Size(388, 23)
        Me.pbNTRIP.TabIndex = 44
        Me.pbNTRIP.Visible = False
        '
        'btnNTRIPConnect
        '
        Me.btnNTRIPConnect.BackColor = System.Drawing.Color.White
        Me.btnNTRIPConnect.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNTRIPConnect.Location = New System.Drawing.Point(538, 94)
        Me.btnNTRIPConnect.Name = "btnNTRIPConnect"
        Me.btnNTRIPConnect.Size = New System.Drawing.Size(136, 33)
        Me.btnNTRIPConnect.TabIndex = 58
        Me.btnNTRIPConnect.Text = "Connect"
        Me.btnNTRIPConnect.UseVisualStyleBackColor = False
        '
        'btnSerialConnect
        '
        Me.btnSerialConnect.BackColor = System.Drawing.Color.White
        Me.btnSerialConnect.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSerialConnect.Location = New System.Drawing.Point(538, 55)
        Me.btnSerialConnect.Name = "btnSerialConnect"
        Me.btnSerialConnect.Size = New System.Drawing.Size(136, 33)
        Me.btnSerialConnect.TabIndex = 59
        Me.btnSerialConnect.Text = "Connect"
        Me.btnSerialConnect.UseVisualStyleBackColor = False
        '
        'lblSerialStatus
        '
        Me.lblSerialStatus.AutoSize = True
        Me.lblSerialStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblSerialStatus.ForeColor = System.Drawing.Color.White
        Me.lblSerialStatus.Location = New System.Drawing.Point(140, 60)
        Me.lblSerialStatus.Name = "lblSerialStatus"
        Me.lblSerialStatus.Size = New System.Drawing.Size(126, 24)
        Me.lblSerialStatus.TabIndex = 60
        Me.lblSerialStatus.Text = "Disconnected"
        '
        'btnSerialEdit
        '
        Me.btnSerialEdit.BackColor = System.Drawing.Color.White
        Me.btnSerialEdit.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnSerialEdit.Location = New System.Drawing.Point(680, 55)
        Me.btnSerialEdit.Name = "btnSerialEdit"
        Me.btnSerialEdit.Size = New System.Drawing.Size(97, 33)
        Me.btnSerialEdit.TabIndex = 61
        Me.btnSerialEdit.Text = "Edit"
        Me.btnSerialEdit.UseVisualStyleBackColor = False
        '
        'btnNTRIPEdit
        '
        Me.btnNTRIPEdit.BackColor = System.Drawing.Color.White
        Me.btnNTRIPEdit.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnNTRIPEdit.Location = New System.Drawing.Point(680, 94)
        Me.btnNTRIPEdit.Name = "btnNTRIPEdit"
        Me.btnNTRIPEdit.Size = New System.Drawing.Size(97, 33)
        Me.btnNTRIPEdit.TabIndex = 62
        Me.btnNTRIPEdit.Text = "Edit"
        Me.btnNTRIPEdit.UseVisualStyleBackColor = False
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.Label3.ForeColor = System.Drawing.Color.White
        Me.Label3.Location = New System.Drawing.Point(17, 135)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(125, 24)
        Me.Label3.TabIndex = 63
        Me.Label3.Text = "NTRIP Status:"
        '
        'Timer1
        '
        '
        'Label4
        '
        Me.Label4.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Label4.BackColor = System.Drawing.Color.Red
        Me.Label4.Location = New System.Drawing.Point(0, 46)
        Me.Label4.Margin = New System.Windows.Forms.Padding(0)
        Me.Label4.Name = "Label4"
        Me.Label4.Size = New System.Drawing.Size(784, 2)
        Me.Label4.TabIndex = 64
        '
        'btnClearLog
        '
        Me.btnClearLog.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnClearLog.BackColor = System.Drawing.Color.White
        Me.btnClearLog.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnClearLog.Location = New System.Drawing.Point(660, 519)
        Me.btnClearLog.Name = "btnClearLog"
        Me.btnClearLog.Size = New System.Drawing.Size(97, 33)
        Me.btnClearLog.TabIndex = 65
        Me.btnClearLog.Text = "Clear"
        Me.btnClearLog.UseVisualStyleBackColor = False
        '
        'lblStatus
        '
        Me.lblStatus.AutoSize = True
        Me.lblStatus.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblStatus.ForeColor = System.Drawing.Color.White
        Me.lblStatus.Location = New System.Drawing.Point(3, 4)
        Me.lblStatus.Name = "lblStatus"
        Me.lblStatus.Size = New System.Drawing.Size(214, 36)
        Me.lblStatus.TabIndex = 66
        Me.lblStatus.Text = "Not Connected"
        '
        'lbl2
        '
        Me.lbl2.AutoSize = True
        Me.lbl2.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl2.ForeColor = System.Drawing.Color.White
        Me.lbl2.Location = New System.Drawing.Point(249, 4)
        Me.lbl2.Name = "lbl2"
        Me.lbl2.Size = New System.Drawing.Size(89, 36)
        Me.lbl2.TabIndex = 67
        Me.lbl2.Text = "Text2"
        '
        'rtbEvents
        '
        Me.rtbEvents.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rtbEvents.BackColor = System.Drawing.Color.Black
        Me.rtbEvents.BorderStyle = System.Windows.Forms.BorderStyle.None
        Me.rtbEvents.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.rtbEvents.ForeColor = System.Drawing.Color.White
        Me.rtbEvents.Location = New System.Drawing.Point(3, 213)
        Me.rtbEvents.Name = "rtbEvents"
        Me.rtbEvents.ReadOnly = True
        Me.rtbEvents.Size = New System.Drawing.Size(781, 349)
        Me.rtbEvents.TabIndex = 68
        Me.rtbEvents.Text = "Events will show up here."
        '
        'lbl3
        '
        Me.lbl3.AutoSize = True
        Me.lbl3.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lbl3.ForeColor = System.Drawing.Color.White
        Me.lbl3.Location = New System.Drawing.Point(451, 4)
        Me.lbl3.Name = "lbl3"
        Me.lbl3.Size = New System.Drawing.Size(89, 36)
        Me.lbl3.TabIndex = 69
        Me.lbl3.Text = "Text3"
        '
        'btnOptions
        '
        Me.btnOptions.BackColor = System.Drawing.Color.White
        Me.btnOptions.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnOptions.Location = New System.Drawing.Point(680, 7)
        Me.btnOptions.Name = "btnOptions"
        Me.btnOptions.Size = New System.Drawing.Size(97, 33)
        Me.btnOptions.TabIndex = 70
        Me.btnOptions.Text = "Options"
        Me.btnOptions.UseVisualStyleBackColor = False
        '
        'btnToggleBottomPayne
        '
        Me.btnToggleBottomPayne.BackColor = System.Drawing.Color.White
        Me.btnToggleBottomPayne.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnToggleBottomPayne.Location = New System.Drawing.Point(644, 172)
        Me.btnToggleBottomPayne.Name = "btnToggleBottomPayne"
        Me.btnToggleBottomPayne.Size = New System.Drawing.Size(133, 33)
        Me.btnToggleBottomPayne.TabIndex = 71
        Me.btnToggleBottomPayne.Text = "History"
        Me.btnToggleBottomPayne.UseVisualStyleBackColor = False
        '
        'btnElevStartPause
        '
        Me.btnElevStartPause.BackColor = System.Drawing.Color.White
        Me.btnElevStartPause.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnElevStartPause.Location = New System.Drawing.Point(644, 259)
        Me.btnElevStartPause.Name = "btnElevStartPause"
        Me.btnElevStartPause.Size = New System.Drawing.Size(133, 40)
        Me.btnElevStartPause.TabIndex = 72
        Me.btnElevStartPause.Text = "Start"
        Me.btnElevStartPause.UseVisualStyleBackColor = False
        Me.btnElevStartPause.Visible = False
        '
        'btnElevReset
        '
        Me.btnElevReset.BackColor = System.Drawing.Color.White
        Me.btnElevReset.Font = New System.Drawing.Font("Microsoft Sans Serif", 16.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.btnElevReset.Location = New System.Drawing.Point(644, 213)
        Me.btnElevReset.Name = "btnElevReset"
        Me.btnElevReset.Size = New System.Drawing.Size(133, 40)
        Me.btnElevReset.TabIndex = 73
        Me.btnElevReset.Text = "Reset"
        Me.btnElevReset.UseVisualStyleBackColor = False
        Me.btnElevReset.Visible = False
        '
        'lblElevMax
        '
        Me.lblElevMax.AutoSize = True
        Me.lblElevMax.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblElevMax.ForeColor = System.Drawing.Color.White
        Me.lblElevMax.Location = New System.Drawing.Point(6, 213)
        Me.lblElevMax.Name = "lblElevMax"
        Me.lblElevMax.Size = New System.Drawing.Size(32, 36)
        Me.lblElevMax.TabIndex = 74
        Me.lblElevMax.Text = "0"
        Me.lblElevMax.Visible = False
        '
        'lblElevMin
        '
        Me.lblElevMin.AutoSize = True
        Me.lblElevMin.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblElevMin.ForeColor = System.Drawing.Color.White
        Me.lblElevMin.Location = New System.Drawing.Point(6, 251)
        Me.lblElevMin.Name = "lblElevMin"
        Me.lblElevMin.Size = New System.Drawing.Size(32, 36)
        Me.lblElevMin.TabIndex = 75
        Me.lblElevMin.Text = "0"
        Me.lblElevMin.Visible = False
        '
        'lblElevNow
        '
        Me.lblElevNow.AutoSize = True
        Me.lblElevNow.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblElevNow.ForeColor = System.Drawing.Color.White
        Me.lblElevNow.Location = New System.Drawing.Point(400, 232)
        Me.lblElevNow.Name = "lblElevNow"
        Me.lblElevNow.Size = New System.Drawing.Size(32, 36)
        Me.lblElevNow.TabIndex = 76
        Me.lblElevNow.Text = "0"
        Me.lblElevNow.Visible = False
        '
        'lblElevRange
        '
        Me.lblElevRange.AutoSize = True
        Me.lblElevRange.Font = New System.Drawing.Font("Microsoft Sans Serif", 22.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblElevRange.ForeColor = System.Drawing.Color.White
        Me.lblElevRange.Location = New System.Drawing.Point(638, 314)
        Me.lblElevRange.Name = "lblElevRange"
        Me.lblElevRange.Size = New System.Drawing.Size(111, 36)
        Me.lblElevRange.TabIndex = 77
        Me.lblElevRange.Text = "Range:"
        Me.lblElevRange.Visible = False
        '
        'MainForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.Black
        Me.ClientSize = New System.Drawing.Size(784, 564)
        Me.Controls.Add(Me.lblElevRange)
        Me.Controls.Add(Me.lblElevNow)
        Me.Controls.Add(Me.lblElevMin)
        Me.Controls.Add(Me.lblElevMax)
        Me.Controls.Add(Me.btnElevReset)
        Me.Controls.Add(Me.btnElevStartPause)
        Me.Controls.Add(Me.btnToggleBottomPayne)
        Me.Controls.Add(Me.btnOptions)
        Me.Controls.Add(Me.lbl3)
        Me.Controls.Add(Me.btnClearLog)
        Me.Controls.Add(Me.rtbEvents)
        Me.Controls.Add(Me.lbl2)
        Me.Controls.Add(Me.lblStatus)
        Me.Controls.Add(Me.Label4)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnNTRIPEdit)
        Me.Controls.Add(Me.btnSerialEdit)
        Me.Controls.Add(Me.lblSerialStatus)
        Me.Controls.Add(Me.btnSerialConnect)
        Me.Controls.Add(Me.btnNTRIPConnect)
        Me.Controls.Add(Me.pbNTRIP)
        Me.Controls.Add(Me.boxMountpoint)
        Me.Controls.Add(Me.lblNTRIPStatus)
        Me.Controls.Add(Me.lblNTRIPStream)
        Me.Controls.Add(Me.LineTop)
        Me.Controls.Add(Me.Label1)
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Name = "MainForm"
        Me.Text = "Lefebure NTRIP Client"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents LineTop As System.Windows.Forms.Label
    Friend WithEvents lblNTRIPStream As System.Windows.Forms.Label
    Friend WithEvents lblNTRIPStatus As System.Windows.Forms.Label
    Friend WithEvents boxMountpoint As System.Windows.Forms.ComboBox
    Friend WithEvents pbNTRIP As System.Windows.Forms.ProgressBar
    Friend WithEvents btnNTRIPConnect As System.Windows.Forms.Button
    Friend WithEvents btnSerialConnect As System.Windows.Forms.Button
    Friend WithEvents lblSerialStatus As System.Windows.Forms.Label
    Friend WithEvents btnSerialEdit As System.Windows.Forms.Button
    Friend WithEvents btnNTRIPEdit As System.Windows.Forms.Button
    Friend WithEvents Label3 As System.Windows.Forms.Label
    Friend WithEvents Timer1 As System.Windows.Forms.Timer
    Friend WithEvents Label4 As System.Windows.Forms.Label
    Friend WithEvents btnClearLog As System.Windows.Forms.Button
    Friend WithEvents lblStatus As System.Windows.Forms.Label
    Friend WithEvents lbl2 As System.Windows.Forms.Label
    Friend WithEvents rtbEvents As System.Windows.Forms.RichTextBox
    Friend WithEvents lbl3 As System.Windows.Forms.Label
    Friend WithEvents btnOptions As System.Windows.Forms.Button
    Friend WithEvents btnToggleBottomPayne As System.Windows.Forms.Button
    Friend WithEvents btnElevStartPause As System.Windows.Forms.Button
    Friend WithEvents btnElevReset As System.Windows.Forms.Button
    Friend WithEvents lblElevMax As System.Windows.Forms.Label
    Friend WithEvents lblElevMin As System.Windows.Forms.Label
    Friend WithEvents lblElevNow As System.Windows.Forms.Label
    Friend WithEvents lblElevRange As System.Windows.Forms.Label

End Class
