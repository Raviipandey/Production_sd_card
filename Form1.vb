Imports System.IO
Imports System.Drawing
Imports System.Windows.Forms
Imports System.Guid

Public Class Form1
    Private volumesListView As ListView
    Private filesListView As ListView
    Private selectedVolume As String
    Private uploadButton As Button
    Private generateUuidButton As Button
    Private uuidTextBox As TextBox
    Private selectAllCheckBox As CheckBox
    Private progressBar As ProgressBar

    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
    Me.Text = "Volume and File Selector"
    Me.Size = New Size(800, 600)
    Me.FormBorderStyle = FormBorderStyle.FixedSingle
    Me.MaximizeBox = False
    Me.BackColor = Color.FromArgb(240, 240, 240)
    Me.Font = New Font("Segoe UI", 9, FontStyle.Regular)

    ' Main layout panel
    Dim mainPanel As New TableLayoutPanel()
    mainPanel.Dock = DockStyle.Fill
    mainPanel.ColumnCount = 2
    mainPanel.RowCount = 2
    mainPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
    mainPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
    mainPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 80))
    mainPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 20))

    ' Volumes ListView
    volumesListView = New ListView()
    volumesListView.View = View.Details
    volumesListView.FullRowSelect = True
    volumesListView.GridLines = True
    volumesListView.Dock = DockStyle.Fill
    volumesListView.BackColor = Color.White

    volumesListView.Columns.Add("Volume Name", 150)
    volumesListView.Columns.Add("Type", 70)
    volumesListView.Columns.Add("Format", 70)
    volumesListView.Columns.Add("Size (GB)", 80)

    ' Panel for Files ListView and CheckBox
    Dim filesPanel As New Panel()
    filesPanel.Dock = DockStyle.Fill
    filesPanel.Padding = New Padding(10)

    ' Create FlowLayoutPanel to hold the checkbox at the top
    Dim filesTopPanel As New FlowLayoutPanel()
    filesTopPanel.Dock = DockStyle.Top
    filesTopPanel.AutoSize = True

    ' Select All checkbox
    selectAllCheckBox = New CheckBox()
    selectAllCheckBox.Text = "Select All Files"
    selectAllCheckBox.AutoSize = True
    selectAllCheckBox.Padding = New Padding(5)
    AddHandler selectAllCheckBox.CheckedChanged, AddressOf OnSelectAllCheckedChanged
    filesTopPanel.Controls.Add(selectAllCheckBox)

    ' Files ListView
    filesListView = New ListView()
    filesListView.View = View.Details
    filesListView.FullRowSelect = True
    filesListView.GridLines = True
    filesListView.Dock = DockStyle.Fill
    filesListView.BackColor = Color.White
    filesListView.CheckBoxes = True

    filesListView.Columns.Add("File Name", 200)
    filesListView.Columns.Add("Size (KB)", 100)

    ' Add ListView and CheckBox to the Panel
    filesPanel.Controls.Add(filesListView) ' Add the ListView first
    filesPanel.Controls.Add(filesTopPanel) ' Add the checkbox panel on top of ListView

    ' UUID Panel
    Dim uuidPanel As New TableLayoutPanel()
    uuidPanel.Dock = DockStyle.Fill
    uuidPanel.ColumnCount = 2
    uuidPanel.RowCount = 2
    uuidPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
    uuidPanel.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50))
    uuidPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 50))
    uuidPanel.RowStyles.Add(New RowStyle(SizeType.Percent, 50))

    generateUuidButton = New Button()
    generateUuidButton.Text = "Generate UUID"
    generateUuidButton.Dock = DockStyle.Fill
    generateUuidButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
    generateUuidButton.BackColor = Color.FromArgb(0, 150, 136)
    generateUuidButton.ForeColor = Color.White
    generateUuidButton.FlatStyle = FlatStyle.Flat
    generateUuidButton.Cursor = Cursors.Hand
    AddHandler generateUuidButton.Click, AddressOf OnGenerateUuidButtonClick

    uuidTextBox = New TextBox()
    uuidTextBox.Dock = DockStyle.Fill
    uuidTextBox.ReadOnly = True
    uuidTextBox.Font = New Font("Consolas", 9, FontStyle.Regular)

    uploadButton = New Button()
    uploadButton.Text = "Upload Selected Files"
    uploadButton.Dock = DockStyle.Fill
    uploadButton.Font = New Font("Segoe UI", 9, FontStyle.Bold)
    uploadButton.BackColor = Color.FromArgb(0, 120, 215)
    uploadButton.ForeColor = Color.White
    uploadButton.FlatStyle = FlatStyle.Flat
    uploadButton.Cursor = Cursors.Hand
    AddHandler uploadButton.Click, AddressOf OnUploadButtonClick

    ' Progress Bar
    progressBar = New ProgressBar()
    progressBar.Dock = DockStyle.Bottom
    progressBar.Height = 20
    progressBar.Minimum = 0
    progressBar.Maximum = 100
    progressBar.Value = 0
    progressBar.Style = ProgressBarStyle.Continuous
    Controls.Add(progressBar)

    ' Add controls to panels
    uuidPanel.Controls.Add(generateUuidButton, 0, 0)
    uuidPanel.Controls.Add(uuidTextBox, 1, 0)
    uuidPanel.Controls.Add(uploadButton, 0, 1)
    uuidPanel.SetColumnSpan(uploadButton, 2)

    mainPanel.Controls.Add(volumesListView, 0, 0)
    mainPanel.Controls.Add(filesPanel, 1, 0)
    mainPanel.Controls.Add(uuidPanel, 0, 1)
    mainPanel.SetColumnSpan(uuidPanel, 2)

    Controls.Add(mainPanel)

    ' Load data and set up event handlers
    LoadVolumes()
    LoadFilesFromHome()
    AddHandler volumesListView.SelectedIndexChanged, AddressOf OnVolumeSelected
    AddHandler filesListView.ItemChecked, AddressOf OnFileChecked

    UpdateUploadButtonState()
    End Sub


    Private Sub LoadVolumes()
        Dim volumes As List(Of String()) = GetAllVolumes()
        For Each volume In volumes
            Dim listViewItem As New ListViewItem(volume)
            volumesListView.Items.Add(listViewItem)
        Next
    End Sub

    Private Sub LoadFilesFromHome()
        Dim homeDir As String = Directory.GetCurrentDirectory()

        ' Check if the 'audio' folder exists
        Dim audioFolderPath As String = Path.Combine(homeDir, "audio")
        If Directory.Exists(audioFolderPath) Then
            ' Add the audio folder as a ListView item
            Dim folderItem As New ListViewItem("audio")
            folderItem.SubItems.Add("-") ' No size for folder
            filesListView.Items.Add(folderItem)
        End If

        ' List all mp3 files in the home directory
        Dim files As FileInfo() = New DirectoryInfo(homeDir).GetFiles("*.mp3")
        For Each file In files
            Dim fileDetails As String() = {
                file.Name,
                (file.Length / 1024).ToString("F2") ' File size in KB
            }
            Dim listViewItem As New ListViewItem(fileDetails)
            filesListView.Items.Add(listViewItem)
        Next
    End Sub


    Private Sub OnVolumeSelected(sender As Object, e As EventArgs)
        If volumesListView.SelectedItems.Count > 0 Then
            selectedVolume = volumesListView.SelectedItems(0).Text ' Get the selected volume name
            UpdateUploadButtonState()
        End If
    End Sub

    Private Sub OnFileChecked(sender As Object, e As ItemCheckedEventArgs)
        UpdateUploadButtonState()
    End Sub

    Private Sub UpdateUploadButtonState()
        uploadButton.Enabled = (Not String.IsNullOrEmpty(selectedVolume)) AndAlso (filesListView.CheckedItems.Count > 0) AndAlso (Not String.IsNullOrEmpty(uuidTextBox.Text))
        If uploadButton.Enabled Then
            uploadButton.BackColor = Color.FromArgb(0, 120, 215)
        Else
            uploadButton.BackColor = Color.Gray
        End If
    End Sub

    Private Sub OnGenerateUuidButtonClick(sender As Object, e As EventArgs)
        uuidTextBox.Text = Guid.NewGuid().ToString()
        UpdateUploadButtonState()
    End Sub

    
Private Async Sub OnUploadButtonClick(sender As Object, e As EventArgs)
    If String.IsNullOrEmpty(selectedVolume) OrElse filesListView.CheckedItems.Count = 0 OrElse String.IsNullOrEmpty(uuidTextBox.Text) Then
        MessageBox.Show("Please select at least one file, a volume, and generate a UUID.")
        Return
    End If

    Try
        ' Disable all controls except the progress bar
        DisableControls(True)

        ' Show a message to the user
        Dim uploadMessageLabel As New Label()
        uploadMessageLabel.Text = "Please wait while the files are being uploaded..."
        uploadMessageLabel.Font = New Font("Segoe UI", 9, FontStyle.Bold)
        uploadMessageLabel.ForeColor = Color.Red
        uploadMessageLabel.Dock = DockStyle.Top
        uploadMessageLabel.TextAlign = ContentAlignment.MiddleCenter
        Controls.Add(uploadMessageLabel)
        Controls.SetChildIndex(uploadMessageLabel, 0)

        Dim driveLetter As String = selectedVolume.Substring(selectedVolume.IndexOf("(") + 1, 2)
        Dim dataFolder As String = Path.Combine(driveLetter, "data")
        Dim mediaFolder As String = Path.Combine(driveLetter, "media")
        Dim audioFolder As String = Path.Combine(mediaFolder, "audio")
        Dim toysFolder As String = Path.Combine(mediaFolder, "toys")

        If Not Directory.Exists(dataFolder) Then Directory.CreateDirectory(dataFolder)
        If Not Directory.Exists(mediaFolder) Then Directory.CreateDirectory(mediaFolder)
        If Not Directory.Exists(audioFolder) Then Directory.CreateDirectory(audioFolder)
        If Not Directory.Exists(toysFolder) Then Directory.CreateDirectory(toysFolder)

        Dim uploadedFiles As New List(Of String)
        Dim totalItems As Integer = filesListView.CheckedItems.Count
        Dim currentItem As Integer = 0

        progressBar.Value = 0
        progressBar.Maximum = totalItems

        ' Run the file copy operation on a background thread
        Await Task.Run(Sub()
                           For Each checkedItem As ListViewItem In filesListView.CheckedItems
                               Dim selectedItem As String = checkedItem.SubItems(0).Text

                               If selectedItem = "audio" Then
                                   Dim sourceAudioFolderPath As String = Path.Combine(Directory.GetCurrentDirectory(), "audio")
                                   If Directory.Exists(sourceAudioFolderPath) Then
                                       Dim mp3Files As FileInfo() = New DirectoryInfo(sourceAudioFolderPath).GetFiles("*.mp3")
                                       For Each mp3File In mp3Files
                                           Dim destinationFilePath As String = Path.Combine(audioFolder, mp3File.Name)
                                           File.Copy(mp3File.FullName, destinationFilePath, True)
                                           uploadedFiles.Add($"audio/{mp3File.Name}")
                                       Next
                                   End If
                               Else
                                   Dim sourceFilePath As String = Path.Combine(Directory.GetCurrentDirectory(), selectedItem)
                                   Dim destinationFilePath As String = Path.Combine(mediaFolder, selectedItem)
                                   File.Copy(sourceFilePath, destinationFilePath, True)
                                   uploadedFiles.Add(selectedItem)
                               End If

                               ' Update progress on the UI thread
                               currentItem += 1
                               Me.Invoke(Sub() progressBar.Value = currentItem)
                           Next
                       End Sub)

        ' Save UUID and log files
        Dim uuidFilePath As String = Path.Combine(dataFolder, "boxid.cubbies")
        File.WriteAllText(uuidFilePath, uuidTextBox.Text)
        uploadedFiles.Add("boxid.cubbies")

        Dim logFilePath As String = Path.Combine(dataFolder, "log.cubbies")
        File.Create(logFilePath).Dispose()
        uploadedFiles.Add("log.cubbies")

        ' Append UUID and date to uuids.csv
        Dim csvFilePath As String = Path.Combine(Directory.GetCurrentDirectory(), "uuids.csv")
        Dim currentDate As String = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        Dim csvLine As String = $"{uuidTextBox.Text},{currentDate}"

        If Not File.Exists(csvFilePath) Then
            ' Create file and add headers
            File.AppendAllText(csvFilePath, "UUID,Date" & Environment.NewLine)
        End If

        ' Append the current UUID and date
        File.AppendAllText(csvFilePath, csvLine & Environment.NewLine)

        progressBar.Value = progressBar.Maximum

        ' Display success message
        Dim successMessage As String = $"Files uploaded successfully to {selectedVolume}!"
        MessageBox.Show(successMessage, "Upload Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)

        ' Close the application after success message
        Application.Exit()
    Catch ex As Exception
        MessageBox.Show($"Error: {ex.Message}")
    Finally
        ' Re-enable controls
        DisableControls(False)
    End Try
End Sub



Private Sub DisableControls(disable As Boolean)
    ' Disable/Enable all controls except the progress bar
    For Each ctrl As Control In Controls
        If Not TypeOf ctrl Is ProgressBar Then
            ctrl.Enabled = Not disable
        End If
    Next
End Sub


    Private Function GetAllVolumes() As List(Of String())
        Dim drives As DriveInfo() = DriveInfo.GetDrives()
        Dim volumeInfo As New List(Of String())

        For Each drive In drives
            If drive.IsReady Then
                Dim volumeLabel As String = drive.VolumeLabel
                Dim displayName As String = If(String.IsNullOrWhiteSpace(volumeLabel), drive.Name, $"{volumeLabel} ({drive.Name.TrimEnd("\")})")

                Dim volumeDetails As String() = {
                    displayName,
                    drive.DriveType.ToString(),
                    drive.DriveFormat,
                    (drive.TotalSize / (1024 ^ 3)).ToString("F2")
                }
                volumeInfo.Add(volumeDetails)
            End If
        Next

        Return volumeInfo
    End Function

    Private Sub OnSelectAllCheckedChanged(sender As Object, e As EventArgs)

    ' Disable event handling while checking/unchecking items to avoid unintended behavior
    RemoveHandler filesListView.ItemChecked, AddressOf OnFileChecked

    ' Loop through each file item in the ListView
    For Each item As ListViewItem In filesListView.Items
        item.Checked = selectAllCheckBox.Checked
    Next

    ' Re-enable event handling after all items are updated
    AddHandler filesListView.ItemChecked, AddressOf OnFileChecked

    ' Manually update the upload button state after selection
    UpdateUploadButtonState()
    End Sub

End Class
