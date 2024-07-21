using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace FilePrinterApp
{
    public partial class Form1 : Form
    {
        private readonly Dictionary<string, List<string>> folderFiles;
        private readonly Dictionary<string, string> filePaths;
        private readonly string logFilePath = @"\\ANGELO\New folder\print_log.txt";
        private readonly string userNameFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "user_name.txt");
        private readonly string foldersFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "folders.txt");
        private readonly string filePathsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "file_paths.txt");
        private readonly string sharedScheduledJobsFilePath = @"\\ANGELO\New folder\shared_scheduled_jobs.txt";
        private readonly PrintDocument printDocument = new PrintDocument();
        private readonly List<ScheduledPrintJob> scheduledPrintJobs = new();
        private DataGridViewCell? selectedCell = null;
        private string? userName;
        private ScheduledPrintJob? selectedCalendarJob;
        private NotifyIcon? notifyIcon;
        private ContextMenuStrip? contextMenuStrip;

        [DllImport("shell32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr ShellExecute(IntPtr hwnd, string lpVerb, string lpFile, string lpParameters, string lpDirectory, int nShowCmd);

        public Form1()
        {
            InitializeComponent();
            folderFiles = new Dictionary<string, List<string>>();
            filePaths = new Dictionary<string, string>();
            this.Icon = new Icon("appicon.ico");
            


            ValidatePaths();

            if (!File.Exists(logFilePath))
            {
                File.Create(logFilePath).Close();
            }

            if (!File.Exists(userNameFilePath))
            {
                PromptForUserName();
            }
            else
            {
                userName = File.ReadAllText(userNameFilePath);
            }

            LoadFolders();
            LoadFilePaths();
            LoadScheduledJobs();
            InitializeWeeklyCalendar();
            PopulateWeeklyCalendar();

            InitializeNotifyIcon();
        }

        private void ValidatePaths()
        {
            string localDirectoryPath = Path.GetDirectoryName(userNameFilePath) ?? string.Empty;
            if (!Directory.Exists(localDirectoryPath))
            {
                Directory.CreateDirectory(localDirectoryPath);
            }

            string networkDirectoryPath = Path.GetDirectoryName(logFilePath) ?? string.Empty;
            if (!Directory.Exists(networkDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Network directory not found: {networkDirectoryPath}");
            }

            networkDirectoryPath = Path.GetDirectoryName(sharedScheduledJobsFilePath) ?? string.Empty;
            if (!Directory.Exists(networkDirectoryPath))
            {
                throw new DirectoryNotFoundException($"Network directory not found: {networkDirectoryPath}");
            }
        }

        private void PromptForUserName()
        {
            userName = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "User Name", "");
            if (!string.IsNullOrEmpty(userName))
            {
                File.WriteAllText(userNameFilePath, userName);
            }
            else
            {
                MessageBox.Show("User name is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                PromptForUserName();
            }
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Title = "Select files"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null;
                if (string.IsNullOrEmpty(selectedFolder))
                {
                    MessageBox.Show("Please select a folder to add files to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!folderFiles.ContainsKey(selectedFolder))
                {
                    folderFiles[selectedFolder] = new List<string>();
                }

                foreach (string file in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(file);
                    filePaths[fileName] = file;
                    folderFiles[selectedFolder].Add(fileName);
                }

                UpdateFileListView();
                SaveFolders();
                SaveFilePaths();
            }
        }

        private void ScheduleSelectButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new()
            {
                Multiselect = true,
                Title = "Select files"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null;
                if (string.IsNullOrEmpty(selectedFolder))
                {
                    MessageBox.Show("Please select a folder to add files to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                if (!folderFiles.ContainsKey(selectedFolder))
                {
                    folderFiles[selectedFolder] = new List<string>();
                }

                foreach (string file in openFileDialog.FileNames)
                {
                    string fileName = Path.GetFileName(file);
                    filePaths[fileName] = file;
                    folderFiles[selectedFolder].Add(fileName);
                }

                UpdateScheduleFileListView();
                SaveFolders();
                SaveFilePaths();
            }
        }

        private void UpdateFileListView()
        {
            fileListView.Items.Clear();
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null;
            if (string.IsNullOrEmpty(selectedFolder)) return;

            if (folderFiles.ContainsKey(selectedFolder))
            {
                foreach (string file in folderFiles[selectedFolder])
                {
                    ListViewItem item = new ListViewItem(file);
                    string extension = Path.GetExtension(file).ToLower();
                    switch (extension)
                    {
                        case ".pdf":
                            item.ImageKey = "pdf";
                            break;
                        case ".xls":
                        case ".xlsx":
                            item.ImageKey = "excel";
                            break;
                        case ".doc":
                        case ".docx":
                            item.ImageKey = "word";
                            break;
                        default:
                            item.ImageKey = "file";
                            break;
                    }
                    fileListView.Items.Add(item);
                }
            }
        }

        private void UpdateScheduleFileListView()
        {
            scheduleFileListView.Items.Clear();
            string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null;
            if (string.IsNullOrEmpty(selectedFolder)) return;

            if (folderFiles.ContainsKey(selectedFolder))
            {
                foreach (string file in folderFiles[selectedFolder])
                {
                    ListViewItem item = new ListViewItem(file);
                    string extension = Path.GetExtension(file).ToLower();
                    switch (extension)
                    {
                        case ".pdf":
                            item.ImageKey = "pdf";
                            break;
                        case ".xls":
                        case ".xlsx":
                            item.ImageKey = "excel";
                            break;
                        case ".doc":
                        case ".docx":
                            item.ImageKey = "word";
                            break;
                        default:
                            item.ImageKey = "file";
                            break;
                    }
                    scheduleFileListView.Items.Add(item);
                }
            }
        }

        private void CreateFolderButton_Click(object sender, EventArgs e)
        {
            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Enter folder name:", "Create Folder", "New Folder");
            if (!string.IsNullOrEmpty(folderName) && !folderFiles.ContainsKey(folderName))
            {
                folderFiles[folderName] = new List<string>();

                ListViewItem item1 = new ListViewItem(folderName ?? string.Empty)
                {
                    ImageKey = "folder"
                };
                folderListView.Items.Add(item1);

                ListViewItem item2 = new ListViewItem(folderName ?? string.Empty)
                {
                    ImageKey = "folder"
                };
                scheduleFolderListView.Items.Add(item2);

                SaveFolders();
            }
        }

        private void DeleteFolderButton_Click(object sender, EventArgs e)
        {
            if (folderListView.SelectedItems.Count > 0)
            {
                string? selectedFolder = folderListView.SelectedItems[0].Text;
                if (!string.IsNullOrEmpty(selectedFolder))
                {
                    folderFiles.Remove(selectedFolder);
                    folderListView.Items.Remove(folderListView.SelectedItems[0]);
                    var scheduleItem = scheduleFolderListView.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == selectedFolder);
                    if (scheduleItem != null)
                    {
                        scheduleFolderListView.Items.Remove(scheduleItem);
                    }
                    UpdateFileListView();
                    SaveFolders();
                }
            }
            else
            {
                MessageBox.Show("No folder selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FolderListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateFileListView();
        }

        private void ScheduleFolderListView_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateScheduleFileListView();
        }

        private void FileListView_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (fileListView.SelectedItems.Count > 0)
            {
                string fileName = fileListView.SelectedItems[0].Text;
                if (filePaths.TryGetValue(fileName, out string? filePath))
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            if (fileListView.SelectedItems.Count > 0)
            {
                string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null;
                if (string.IsNullOrEmpty(selectedFolder)) return;

                foreach (ListViewItem selectedItem in fileListView.SelectedItems)
                {
                    string? fileName = selectedItem.Text;
                    if (fileName != null)
                    {
                        folderFiles[selectedFolder].Remove(fileName);
                        filePaths.Remove(fileName);
                    }
                }
                UpdateFileListView();
                SaveFolders();
                SaveFilePaths();
            }
        }

        private void DeleteAllButton_Click(object sender, EventArgs e)
        {
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null;
            if (!string.IsNullOrEmpty(selectedFolder))
            {
                folderFiles[selectedFolder].Clear();
                UpdateFileListView();
                SaveFolders();
                SaveFilePaths();
            }
        }

        private void PrintButton_Click(object sender, EventArgs e)
        {
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null;
            if (string.IsNullOrEmpty(selectedFolder) || folderFiles[selectedFolder].Count == 0)
            {
                MessageBox.Show("No files selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            printButton.Enabled = false;
            Thread printThread = new(() => ProcessPrintQueue(selectedFolder));
            printThread.Start();
        }

        private void ProcessPrintQueue(string folderName)
        {
            int fileCount = 0;
            List<string> remainingFiles = new(folderFiles[folderName]);
            while (remainingFiles.Count > 0)
            {
                List<string> batch = remainingFiles.GetRange(0, Math.Min(15, remainingFiles.Count));
                remainingFiles.RemoveRange(0, batch.Count);

                foreach (string file in batch)
                {
                    try
                    {
                        if (!filePaths.TryGetValue(file, out var filePath))
                        {
                            throw new FileNotFoundException("File path not found.", file);
                        }
                        if (!File.Exists(filePath))
                        {
                            throw new FileNotFoundException("File path not found.", filePath);
                        }
                        ShellExecute(IntPtr.Zero, "print", filePath, null, null, 0);
                        LogToFile($"Printed: {filePath}");
                        fileCount++;
                    }
                    catch (FileNotFoundException fnfEx)
                    {
                        LogToFile($"Failed to print {file}: File not found - {fnfEx.FileName}");
                    }
                    catch (Exception ex)
                    {
                        LogToFile($"Failed to print {file}: {ex.Message}");
                    }
                }

                Thread.Sleep(2000);
            }

            Invoke(new Action(() => PrintDone(fileCount)));
            Invoke(new Action(() => MarkFolderAsPrinted(folderName)));
        }

        private void PrintDone(int fileCount)
        {
            MessageBox.Show($"All files sent to printer. Number of files sent: {fileCount}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            printButton.Enabled = true;
        }

        private async void LogToFile(string message)
        {
            bool isLogged = false;
            int attempts = 0;
            while (!isLogged && attempts < 5)
            {
                try
                {
                    using StreamWriter writer = new(logFilePath, true);
                    await writer.WriteLineAsync($"{DateTime.Now}: {message}");
                    isLogged = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isLogged)
            {
                MessageBox.Show("Failed to write to log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadLogButton_Click(object sender, EventArgs e)
        {
            bool isLoaded = false;
            int attempts = 0;
            while (!isLoaded && attempts < 5)
            {
                try
                {
                    if (File.Exists(logFilePath))
                    {
                        logListBox.Items.Clear();
                        string[] logLines = await File.ReadAllLinesAsync(logFilePath);
                        foreach (string line in logLines)
                        {
                            logListBox.Items.Add(line);
                        }
                    }
                    else
                    {
                        MessageBox.Show("No log file found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    isLoaded = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isLoaded)
            {
                MessageBox.Show("Failed to load log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void ClearLogButton_Click(object sender, EventArgs e)
        {
            bool isCleared = false;
            int attempts = 0;
            while (!isCleared && attempts < 5)
            {
                try
                {
                    if (File.Exists(logFilePath))
                    {
                        await File.WriteAllTextAsync(logFilePath, string.Empty);
                        logListBox.Items.Clear();
                        MessageBox.Show("Log file cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    isCleared = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isCleared)
            {
                MessageBox.Show("Failed to clear log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectPrinterButton_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new() { Document = printDocument };
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.PrinterSettings = printDialog.PrinterSettings;
                MessageBox.Show($"Printer selected: {printDocument.PrinterSettings.PrinterName}", "Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void SelectPrintFilesPrinterButton_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new() { Document = printDocument };
            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.PrinterSettings = printDialog.PrinterSettings;
                MessageBox.Show($"Printer selected: {printDocument.PrinterSettings.PrinterName}", "Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void ScheduleButton_Click(object sender, EventArgs e)
        {
            string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null;
            if (string.IsNullOrEmpty(selectedFolder) || !scheduleDateTimePicker.Checked)
            {
                MessageBox.Show("No folder selected or no time specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime scheduledTime = scheduleDateTimePicker.Value;
            if (IsTimeSlotTaken(scheduledTime, out string existingUser, out string existingFolder))
            {
                MessageBox.Show($"Time slot is already taken by {existingUser} for folder '{existingFolder}'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            TimeSpan timeUntilPrint = scheduledTime - DateTime.Now;

            if (timeUntilPrint <= TimeSpan.Zero)
            {
                MessageBox.Show("Scheduled time must be in the future.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            List<string> scheduledFiles;
            try
            {
                scheduledFiles = folderFiles[selectedFolder].Select(f =>
                {
                    if (filePaths.ContainsKey(f))
                    {
                        return filePaths[f];
                    }
                    else
                    {
                        throw new KeyNotFoundException($"File path not found for file: {f}");
                    }
                }).ToList();
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ScheduledPrintJob printJob = new()
            {
                UserName = userName,
                FolderName = selectedFolder,
                Files = scheduledFiles,
                ScheduledTime = scheduledTime,
                Timer = new System.Windows.Forms.Timer
                {
                    Interval = (int)timeUntilPrint.TotalMilliseconds
                }
            };

            printJob.Timer.Tick += (s, args) =>
            {
                printJob.Timer.Stop();
                SchedulePrintFiles(printJob);
            };
            printJob.Timer.Start();

            scheduledPrintJobs.Add(printJob);
            UpdateScheduledJobsListBox();
            LogScheduledJob(printJob);
            SaveScheduledJobs();

            MessageBox.Show($"Folder '{selectedFolder}' scheduled to print at {scheduledTime}.", "Scheduled", MessageBoxButtons.OK, MessageBoxIcon.Information);
            UpdateCalendar();
        }

        private void UpdateScheduledJobsListBox()
        {
            scheduledJobsListBox.Items.Clear();
            foreach (var job in scheduledPrintJobs)
            {
                scheduledJobsListBox.Items.Add($"Scheduled by {job.UserName} at {job.ScheduledTime}, Folder: {job.FolderName}, Files: {string.Join(", ", job.Files.Select(Path.GetFileName))}");
            }
        }

        private void SchedulePrintFiles(ScheduledPrintJob job)
        {
            foreach (string filePath in job.Files)
            {
                string fileName = Path.GetFileName(filePath);
                try
                {
                    if (!File.Exists(filePath))
                    {
                        throw new FileNotFoundException("File path not found.", filePath);
                    }

                    ShellExecute(IntPtr.Zero, "print", filePath, null, null, 0);
                    LogToFile($"Scheduled Printed: {filePath}");
                }
                catch (FileNotFoundException fnfEx)
                {
                    LogToFile($"Failed to print {fileName}: File not found - {fnfEx.FileName}");
                }
                catch (Exception ex)
                {
                    LogToFile($"Failed to print {fileName}: {ex.Message}");
                }
            }
            Invoke(new Action(() =>
            {
                scheduledPrintJobs.Remove(job);
                UpdateScheduledJobsListBox();
                SaveScheduledJobs();
                LoadScheduledJobs();
                MarkFolderAsPrinted(job.FolderName);
            }));
        }

        private bool IsTimeSlotTaken(DateTime scheduledTime, out string existingUser, out string existingFolder)
        {
            existingUser = string.Empty;
            existingFolder = string.Empty;

            if (!File.Exists(sharedScheduledJobsFilePath)) return false;

            string[] jobLines = File.ReadAllLines(sharedScheduledJobsFilePath);
            foreach (string line in jobLines)
            {
                string[] parts = line.Split('|');
                string userName = parts[0];
                string folderName = parts[1];
                DateTime loggedTime = ParseDateTime(parts[3]);

                if (Math.Abs((loggedTime - scheduledTime).TotalMinutes) < 1)
                {
                    existingUser = userName;
                    existingFolder = folderName;
                    return true;
                }
            }
            return false;
        }

        private async void LogScheduledJob(ScheduledPrintJob job)
        {
            bool isLogged = false;
            int attempts = 0;
            while (!isLogged && attempts < 5)
            {
                try
                {
                    using StreamWriter writer = new(sharedScheduledJobsFilePath, true);
                    await writer.WriteLineAsync($"{job.UserName}|{job.FolderName}|{string.Join(",", job.Files)}|{job.ScheduledTime.ToString("O")}|{job.Timer.Interval}");
                    isLogged = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isLogged)
            {
                MessageBox.Show("Failed to write scheduled job to file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadScheduledJobs()
        {
            bool isLoaded = false;
            int attempts = 0;
            while (!isLoaded && attempts < 5)
            {
                try
                {
                    if (!File.Exists(sharedScheduledJobsFilePath)) return;

                    scheduledPrintJobs.Clear();
                    string[] jobLines = await File.ReadAllLinesAsync(sharedScheduledJobsFilePath);
                    foreach (string line in jobLines)
                    {
                        string[] parts = line.Split('|');
                        string userName = parts[0];
                        string folderName = parts[1];
                        List<string> files = parts[2].Split(',').ToList();
                        DateTime scheduledTime = ParseDateTime(parts[3]);
                        int interval = int.Parse(parts[4]);

                        TimeSpan timeUntilPrint = scheduledTime - DateTime.Now;
                        if (timeUntilPrint > TimeSpan.Zero)
                        {
                            ScheduledPrintJob printJob = new()
                            {
                                UserName = userName,
                                FolderName = folderName,
                                Files = files,
                                ScheduledTime = scheduledTime,
                                Timer = new System.Windows.Forms.Timer
                                {
                                    Interval = (int)timeUntilPrint.TotalMilliseconds
                                }
                            };

                            printJob.Timer.Tick += (s, args) =>
                            {
                                printJob.Timer.Stop();
                                SchedulePrintFiles(printJob);
                            };
                            printJob.Timer.Start();

                            scheduledPrintJobs.Add(printJob);
                        }
                    }
                    isLoaded = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (isLoaded)
            {
                UpdateScheduledJobsListBox();
                UpdateCalendar();
            }
            else
            {
                MessageBox.Show("Failed to load scheduled jobs after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveScheduledJobs()
        {
            bool isSaved = false;
            int attempts = 0;
            while (!isSaved && attempts < 5)
            {
                try
                {
                    using StreamWriter writer = new(sharedScheduledJobsFilePath);
                    foreach (var job in scheduledPrintJobs)
                    {
                        await writer.WriteLineAsync($"{job.UserName}|{job.FolderName}|{string.Join(",", job.Files)}|{job.ScheduledTime.ToString("O")}|{job.Timer.Interval}");
                    }
                    isSaved = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isSaved)
            {
                MessageBox.Show("Failed to save scheduled jobs after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DateTime ParseDateTime(string dateTimeString)
        {
            if (DateTime.TryParse(dateTimeString, null, DateTimeStyles.RoundtripKind, out DateTime dateTime))
            {
                return dateTime;
            }
            throw new FormatException($"Date string '{dateTimeString}' was not recognized as a valid DateTime.");
        }

        private async void LoadFolders()
        {
            bool isLoaded = false;
            int attempts = 0;
            while (!isLoaded && attempts < 5)
            {
                try
                {
                    if (!File.Exists(foldersFilePath)) return;

                    folderFiles.Clear();
                    folderListView.Items.Clear();
                    scheduleFolderListView.Items.Clear();

                    string[] folderLines = await File.ReadAllLinesAsync(foldersFilePath);
                    foreach (string line in folderLines)
                    {
                        string[] parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                        string folderName = parts[0];
                        List<string> files = parts[1].Split(new[] { ", " }, StringSplitOptions.None).ToList();

                        folderFiles[folderName] = files;

                        ListViewItem item1 = new ListViewItem(folderName ?? string.Empty)
                        {
                            ImageKey = "folder"
                        };
                        folderListView.Items.Add(item1);

                        ListViewItem item2 = new ListViewItem(folderName ?? string.Empty)
                        {
                            ImageKey = "folder"
                        };
                        scheduleFolderListView.Items.Add(item2);
                    }
                    isLoaded = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isLoaded)
            {
                MessageBox.Show("Failed to load folders after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveFolders()
        {
            bool isSaved = false;
            int attempts = 0;
            while (!isSaved && attempts < 5)
            {
                try
                {
                    using StreamWriter writer = new(foldersFilePath);
                    foreach (var folder in folderFiles)
                    {
                        await writer.WriteLineAsync($"{folder.Key}: {string.Join(", ", folder.Value)}");
                    }
                    isSaved = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isSaved)
            {
                MessageBox.Show("Failed to save folders after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void LoadFilePaths()
        {
            bool isLoaded = false;
            int attempts = 0;
            while (!isLoaded && attempts < 5)
            {
                try
                {
                    if (!File.Exists(filePathsFilePath)) return;

                    filePaths.Clear();
                    string[] lines = await File.ReadAllLinesAsync(filePathsFilePath);
                    foreach (string line in lines)
                    {
                        string[] parts = line.Split(new[] { ": " }, StringSplitOptions.None);
                        if (parts.Length == 2)
                        {
                            filePaths[parts[0]] = parts[1];
                        }
                    }
                    isLoaded = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isLoaded)
            {
                MessageBox.Show("Failed to load file paths after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void SaveFilePaths()
        {
            bool isSaved = false;
            int attempts = 0;
            while (!isSaved && attempts < 5)
            {
                try
                {
                    using StreamWriter writer = new(filePathsFilePath);
                    foreach (var kvp in filePaths)
                    {
                        await writer.WriteLineAsync($"{kvp.Key}: {kvp.Value}");
                    }
                    isSaved = true;
                }
                catch (IOException)
                {
                    attempts++;
                    await Task.Delay(500);
                }
            }

            if (!isSaved)
            {
                MessageBox.Show("Failed to save file paths after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UpdateCalendar()
        {
            InitializeWeeklyCalendar();
            PopulateWeeklyCalendar();
        }

        private void InitializeWeeklyCalendar()
        {
            weeklyCalendarGridView.Columns.Clear();
            weeklyCalendarGridView.Rows.Clear();

            for (int i = 0; i < 7; i++)
            {
                var column = new DataGridViewTextBoxColumn
                {
                    Name = ((DayOfWeek)i).ToString(),
                    HeaderText = DateTime.Now.AddDays(i - (int)DateTime.Now.DayOfWeek).ToString("dddd dd/MM"),
                    Width = 200
                };
                weeklyCalendarGridView.Columns.Add(column);
            }

            for (int hour = 0; hour < 24; hour++)
            {
                for (int minute = 0; minute < 60; minute++)
                {
                    var row = new DataGridViewRow
                    {
                        Height = 40
                    };
                    row.HeaderCell.Value = $"{hour:D2}:{minute:D2}";
                    weeklyCalendarGridView.Rows.Add(row);
                }
            }

            weeklyCalendarGridView.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders);
            weeklyCalendarGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            weeklyCalendarGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            weeklyCalendarGridView.DefaultCellStyle.Font = new Font("Arial", 10);
            weeklyCalendarGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold);
        }

        private void PopulateWeeklyCalendar()
        {
            foreach (var job in scheduledPrintJobs)
            {
                if (job.ScheduledTime.Date >= DateTime.Now.Date && job.ScheduledTime.Date < DateTime.Now.Date.AddDays(7))
                {
                    int columnIndex = (int)job.ScheduledTime.DayOfWeek;
                    int rowIndex = job.ScheduledTime.Hour * 60 + job.ScheduledTime.Minute;

                    var cell = weeklyCalendarGridView.Rows[rowIndex].Cells[columnIndex];
                    cell.Style.BackColor = Color.LightCoral;

                    string existingText = cell.Value?.ToString();
                    if (string.IsNullOrEmpty(existingText))
                    {
                        cell.Value = $"[{job.ScheduledTime:HH:mm}] {job.UserName} - {job.FolderName}";
                    }
                    else
                    {
                        cell.Value = existingText + Environment.NewLine + $"[{job.ScheduledTime:HH:mm}] {job.UserName} - {job.FolderName}";
                    }
                }
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.SelectedTab == tabPage3)
            {
                UpdateCalendar();
            }
        }

        private void RefreshCalendarButton_Click(object sender, EventArgs e)
        {
            LoadScheduledJobs();
        }

        private void MakeRealButton_Click(object sender, EventArgs e)
        {
            if (folderListView.SelectedItems.Count == 0)
            {
                MessageBox.Show("Please select a folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string selectedFolder = folderListView.SelectedItems[0].Text;

            if (folderFiles.ContainsKey(selectedFolder))
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        string targetPath = folderBrowserDialog.SelectedPath;
                        string sanitizedFolderName = SanitizeFileName(selectedFolder);
                        string newFolderPath = Path.Combine(targetPath, sanitizedFolderName);

                        Directory.CreateDirectory(newFolderPath);

                        foreach (string file in folderFiles[selectedFolder])
                        {
                            string sourceFilePath = filePaths[file];
                            string targetFilePath = Path.Combine(newFolderPath, file);
                            File.Copy(sourceFilePath, targetFilePath, true);
                        }

                        MessageBox.Show($"Folder '{selectedFolder}' and its files have been created at {newFolderPath}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            else
            {
                MessageBox.Show("Selected folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string SanitizeFileName(string fileName)
        {
            foreach (char c in Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c, '_');
            }
            return fileName;
        }

        private void DeleteScheduledJobButton_Click(object sender, EventArgs e)
        {
            if (selectedCalendarJob != null)
            {
                selectedCalendarJob.Timer.Stop();
                scheduledPrintJobs.Remove(selectedCalendarJob);
                UpdateScheduledJobsListBox();
                SaveScheduledJobs();
                UpdateCalendar();
                MessageBox.Show("Scheduled job deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                selectedCalendarJob = null; // Reset the selected job
            }
            else
            {
                MessageBox.Show("No scheduled job selected in the calendar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void WeeklyCalendarGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
            {
                return;
            }

            var cellValue = weeklyCalendarGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;
            if (cellValue != null)
            {
                string cellText = cellValue.ToString();
                selectedCalendarJob = scheduledPrintJobs.FirstOrDefault(job =>
                    cellText.Contains(job.FolderName) && cellText.Contains(job.UserName) && cellText.Contains(job.ScheduledTime.ToString("HH:mm")));

                if (selectedCalendarJob != null)
                {
                    MessageBox.Show($"Selected Job: {selectedCalendarJob.FolderName} scheduled by {selectedCalendarJob.UserName} at {selectedCalendarJob.ScheduledTime}");
                }
            }
        }

        private void scheduledJobsListBox_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            e.ItemHeight = 50;
        }

        private void scheduledJobsListBox_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0 || e.Index >= scheduledJobsListBox.Items.Count)
            {
                return;
            }

            e.DrawBackground();
            var item = scheduledJobsListBox.Items[e.Index].ToString();
            e.Graphics.DrawString(item, e.Font, Brushes.Black, e.Bounds);
            e.DrawFocusRectangle();
        }

        private void MarkFolderAsPrinted(string folderName)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string newFolderName = $"{folderName} (Printed at {timestamp})";

            // Update in folderFiles dictionary
            if (folderFiles.ContainsKey(folderName))
            {
                List<string> files = folderFiles[folderName];
                folderFiles.Remove(folderName);
                folderFiles[newFolderName] = files;
            }

            // Update in folderListView
            foreach (ListViewItem item in folderListView.Items)
            {
                if (item.Text == folderName)
                {
                    item.Text = newFolderName;
                    break;
                }
            }

            // Update in scheduleFolderListView
            foreach (ListViewItem item in scheduleFolderListView.Items)
            {
                if (item.Text == folderName)
                {
                    item.Text = newFolderName;
                    break;
                }
            }

            SaveFolders();
        }

        private void InitializeNotifyIcon()
        {
            notifyIcon = new NotifyIcon
            {
                Icon = new Icon("appicon.ico"), // Replace with the actual path to your icon file
                Text = "File Printer App",
                Visible = true
            };

            contextMenuStrip = new ContextMenuStrip();
            contextMenuStrip.Items.Add("Show", null, Show_Click);
            contextMenuStrip.Items.Add("Exit", null, Exit_Click);

            notifyIcon.ContextMenuStrip = contextMenuStrip;
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            ShowForm();
        }

        private void Show_Click(object? sender, EventArgs e)
        {
            ShowForm();
        }

        private void Exit_Click(object? sender, EventArgs e)
        {
            notifyIcon!.Visible = false;
            Application.Exit();
        }

        private void ShowForm()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                notifyIcon?.ShowBalloonTip(1000, "File Printer App", "Application minimized to tray.", ToolTipIcon.Info);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                notifyIcon?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public class ScheduledPrintJob
    {
        public string UserName { get; set; } = string.Empty;
        public string FolderName { get; set; } = string.Empty;
        public List<string> Files { get; set; } = new List<string>();
        public DateTime ScheduledTime { get; set; }
        public System.Windows.Forms.Timer Timer { get; set; } = new System.Windows.Forms.Timer();
    }
}