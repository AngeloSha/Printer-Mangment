using System; // Importing the base .NET System namespace for core functionality like data types, exceptions, etc.
using System.Collections.Generic; // Importing the namespace for using generic collections such as Dictionary, List, etc.
using System.Diagnostics; // Importing the namespace for working with processes, event logs, and performance counters.
using System.Drawing; // Importing the namespace for GDI+ basic graphics functionality like colors and drawing.
using System.Drawing.Printing; // Importing the namespace for printing functionality.
using System.Globalization; // Importing the namespace for culture-related information and formatting.
using System.IO; // Importing the namespace for file and data stream handling.
using System.Linq; // Importing the namespace for LINQ queries which operate on collections.
using System.Runtime.InteropServices; // Importing the namespace for interacting with unmanaged code.
using System.Threading; // Importing the namespace for threading and synchronization functionalities.
using System.Threading.Tasks; // Importing the namespace for parallel and asynchronous task execution.
using System.Windows.Forms; // Importing the namespace for Windows Forms application development.
using System.Xml.Linq; // Importing the namespace for working with XML.
using PdfiumViewer; // Importing the namespace for handling PDF viewing and printing.

namespace FilePrinterApp // Declaring a namespace for the application, which encapsulates the classes.
{
    public partial class Form1 : Form // Defining a partial class Form1 that inherits from Form, making it a Windows Form.
    {
        private readonly Dictionary<string, List<string>> folderFiles; // Dictionary to store folder names and their corresponding file lists.
        private readonly Dictionary<string, string> filePaths; // Dictionary to map file names to their full file paths.
        private readonly string logFilePath = @"F:\Backoffice\PrinterLog\print_log.txt"; // File path for storing logs.
        private readonly string userNameFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "user_name.txt"); // Path for storing the username.
        private readonly string foldersFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "folders.txt"); // Path for storing folder information.
        private readonly string filePathsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FilePrinterApp", "file_paths.txt"); // Path for storing file paths.
        private readonly string sharedScheduledJobsFilePath = @"F:\Backoffice\PrinterLog\shared_scheduled_jobs.txt"; // Path for storing shared scheduled job information.
        private readonly PrintDocument printDocument = new PrintDocument(); // Creating an instance of PrintDocument to manage printing.
        private readonly List<ScheduledPrintJob> scheduledPrintJobs = new(); // List to hold scheduled print jobs.
        private DataGridViewCell? selectedCell = null; // Variable to store the selected cell in the DataGridView.
        private string? userName; // Variable to store the username.
        private ScheduledPrintJob? selectedCalendarJob; // Variable to store the selected scheduled print job.
        private NotifyIcon? notifyIcon; // Variable to store the system tray icon.
        private ContextMenuStrip? contextMenuStrip; // Variable to store the context menu for the notify icon.

        public Form1() // Constructor for the Form1 class.
        {
            InitializeComponent(); // Initializes form components.
            folderFiles = new Dictionary<string, List<string>>(); // Initializes the folderFiles dictionary.
            filePaths = new Dictionary<string, string>(); // Initializes the filePaths dictionary.
            this.Icon = new Icon("appicon.ico"); // Sets the application icon.

            ValidatePaths(); // Validates and ensures necessary paths and directories exist.

            if (!File.Exists(logFilePath)) // Checks if the log file exists.
            {
                File.Create(logFilePath).Close(); // Creates the log file if it doesn't exist.
            }

            if (!File.Exists(userNameFilePath)) // Checks if the username file exists.
            {
                PromptForUserName(); // Prompts the user for their name if the file doesn't exist.
            }
            else
            {
                userName = File.ReadAllText(userNameFilePath); // Reads the username from the file.
            }

            LoadFolders(); // Loads folders and associated files from storage.
            LoadFilePaths(); // Loads file paths from storage.
            LoadScheduledJobs(); // Loads any scheduled print jobs from storage.
            InitializeWeeklyCalendar(); // Initializes the weekly calendar display.
            PopulateWeeklyCalendar(); // Populates the calendar with scheduled jobs.

            InitializeNotifyIcon(); // Initializes the system tray icon.
        }

        private void ValidatePaths() // Method to validate paths and create directories if necessary.
        {
            string localDirectoryPath = Path.GetDirectoryName(userNameFilePath) ?? string.Empty; // Gets the local directory path.
            if (!Directory.Exists(localDirectoryPath)) // Checks if the directory exists.
            {
                Directory.CreateDirectory(localDirectoryPath); // Creates the directory if it doesn't exist.
            }

            string networkDirectoryPath = Path.GetDirectoryName(logFilePath) ?? string.Empty; // Gets the network directory path.
            if (!Directory.Exists(networkDirectoryPath)) // Checks if the directory exists.
            {
                throw new DirectoryNotFoundException($"Network directory not found: {networkDirectoryPath}"); // Throws an exception if the directory doesn't exist.
            }

            networkDirectoryPath = Path.GetDirectoryName(sharedScheduledJobsFilePath) ?? string.Empty; // Gets the shared job directory path.
            if (!Directory.Exists(networkDirectoryPath)) // Checks if the directory exists.
            {
                throw new DirectoryNotFoundException($"Network directory not found: {networkDirectoryPath}"); // Throws an exception if the directory doesn't exist.
            }
        }

        private void PromptForUserName() // Method to prompt the user for their name.
        {
            userName = Microsoft.VisualBasic.Interaction.InputBox("Enter your name:", "User Name", ""); // Prompts the user for their name.
            if (!string.IsNullOrEmpty(userName)) // Checks if the user entered a name.
            {
                File.WriteAllText(userNameFilePath, userName); // Saves the username to a file.
            }
            else
            {
                MessageBox.Show("User name is required.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if no name is entered.
                PromptForUserName(); // Prompts again for the username.
            }
        }

        private void PrintPdf(string filePath) // Method to print a PDF file.
        {
            try
            {
                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(filePath) + "_flattened.pdf"); // Creates a temporary file path for the flattened PDF.

                FlattenPdf(filePath, tempFilePath); // Flattens the PDF to ensure proper rendering.

                using (var document = PdfiumViewer.PdfDocument.Load(tempFilePath)) // Loads the flattened PDF for printing.
                {
                    using (var printDocument = document.CreatePrintDocument()) // Creates a PrintDocument for the PDF.
                    {
                        printDocument.DocumentName = Path.GetFileName(filePath); // Sets the document name.
                        printDocument.PrinterSettings = this.printDocument.PrinterSettings; // Uses the main printer settings.
                        printDocument.PrintController = new StandardPrintController(); // Uses a standard print controller to avoid the print dialog.
                        printDocument.Print(); // Sends the document to the printer.
                    }
                }

                File.Delete(tempFilePath); // Deletes the temporary flattened PDF file.

                LogToFile($"Printed PDF: {filePath}"); // Logs the successful print operation.
            }
            catch (FileNotFoundException fnfEx)
            {
                LogToFile($"Failed to print PDF {filePath}: File not found - {fnfEx.FileName}"); // Logs if the file was not found.
            }
            catch (IOException ioEx)
            {
                LogToFile($"Failed to print PDF {filePath}: IO issue - {ioEx.Message}"); // Logs if there was an I/O error.
            }
            catch (UnauthorizedAccessException uaEx)
            {
                LogToFile($"Failed to print PDF {filePath}: Unauthorized access - {uaEx.Message}"); // Logs if there was an unauthorized access error.
            }
            catch (System.Drawing.Printing.InvalidPrinterException ipEx)
            {
                LogToFile($"Failed to print PDF {filePath}: Invalid printer - {ipEx.Message}"); // Logs if there was a printer error.
            }
            catch (Exception ex)
            {
                LogToFile($"Failed to print PDF {filePath}: General error - {ex.Message}"); // Logs any other general errors.
            }
        }

        private void FlattenPdf(string inputPdfPath, string outputPdfPath) // Method to flatten a PDF using iTextSharp.
        {
            using (var reader = new iTextSharp.text.pdf.PdfReader(inputPdfPath)) // Reads the PDF file.
            {
                using (var stamper = new iTextSharp.text.pdf.PdfStamper(reader, new FileStream(outputPdfPath, FileMode.Create))) // Creates a stamper to flatten the PDF.
                {
                    stamper.FormFlattening = true; // Sets the stamper to flatten form fields.
                }
            }
        }

        private void SelectButton_Click(object sender, EventArgs e) // Event handler for the file selection button.
        {
            OpenFileDialog openFileDialog = new() // Opens a file dialog to select files.
            {
                Multiselect = true, // Allows multiple file selection.
                Title = "Select files" // Sets the title of the dialog.
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK) // Checks if files were selected.
            {
                string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null; // Gets the selected folder.
                if (string.IsNullOrEmpty(selectedFolder)) // Checks if a folder was selected.
                {
                    MessageBox.Show("Please select a folder to add files to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if no folder is selected.
                    return;
                }

                if (!folderFiles.ContainsKey(selectedFolder)) // Checks if the folder exists in the dictionary.
                {
                    folderFiles[selectedFolder] = new List<string>(); // Creates a new entry in the dictionary if the folder doesn't exist.
                }

                foreach (string file in openFileDialog.FileNames) // Iterates through the selected files.
                {
                    string fileName = Path.GetFileName(file); // Gets the file name.
                    filePaths[fileName] = file; // Maps the file name to its full path.
                    folderFiles[selectedFolder].Add(fileName); // Adds the file name to the folder's file list.
                }

                UpdateFileListView(); // Updates the file list view in the UI.
                SaveFolders(); // Saves the folders to storage.
                SaveFilePaths(); // Saves the file paths to storage.
            }
        }

        private void ScheduleSelectButton_Click(object sender, EventArgs e) // Event handler for selecting files for scheduled printing.
        {
            OpenFileDialog openFileDialog = new() // Opens a file dialog to select files.
            {
                Multiselect = true, // Allows multiple file selection.
                Title = "Select files" // Sets the title of the dialog.
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK) // Checks if files were selected.
            {
                string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null; // Gets the selected folder.
                if (string.IsNullOrEmpty(selectedFolder)) // Checks if a folder was selected.
                {
                    MessageBox.Show("Please select a folder to add files to.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if no folder is selected.
                    return;
                }

                if (!folderFiles.ContainsKey(selectedFolder)) // Checks if the folder exists in the dictionary.
                {
                    folderFiles[selectedFolder] = new List<string>(); // Creates a new entry in the dictionary if the folder doesn't exist.
                }

                foreach (string file in openFileDialog.FileNames) // Iterates through the selected files.
                {
                    string fileName = Path.GetFileName(file); // Gets the file name.
                    filePaths[fileName] = file; // Maps the file name to its full path.
                    folderFiles[selectedFolder].Add(fileName); // Adds the file name to the folder's file list.
                }

                UpdateScheduleFileListView(); // Updates the file list view in the UI for scheduled files.
                SaveFolders(); // Saves the folders to storage.
                SaveFilePaths(); // Saves the file paths to storage.
            }
        }

        private void UpdateFileListView() // Method to update the file list view.
        {
            fileListView.Items.Clear(); // Clears the current items in the file list view.
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null; // Gets the selected folder.
            if (string.IsNullOrEmpty(selectedFolder)) return; // Returns if no folder is selected.

            if (folderFiles.ContainsKey(selectedFolder)) // Checks if the folder exists in the dictionary.
            {
                foreach (string file in folderFiles[selectedFolder]) // Iterates through the files in the folder.
                {
                    ListViewItem item = new ListViewItem(file); // Creates a new list view item for each file.
                    string extension = Path.GetExtension(file).ToLower(); // Gets the file extension.
                    switch (extension) // Determines the icon based on the file extension.
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
                    fileListView.Items.Add(item); // Adds the item to the file list view.
                }
            }
        }

        private void UpdateScheduleFileListView() // Method to update the scheduled file list view.
        {
            scheduleFileListView.Items.Clear(); // Clears the current items in the scheduled file list view.
            string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null; // Gets the selected folder.
            if (string.IsNullOrEmpty(selectedFolder)) return; // Returns if no folder is selected.

            if (folderFiles.ContainsKey(selectedFolder)) // Checks if the folder exists in the dictionary.
            {
                foreach (string file in folderFiles[selectedFolder]) // Iterates through the files in the folder.
                {
                    ListViewItem item = new ListViewItem(file); // Creates a new list view item for each file.
                    string extension = Path.GetExtension(file).ToLower(); // Gets the file extension.
                    switch (extension) // Determines the icon based on the file extension.
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
                    scheduleFileListView.Items.Add(item); // Adds the item to the scheduled file list view.
                }
            }
        }

        private void CreateFolderButton_Click(object sender, EventArgs e) // Event handler for creating a new folder.
        {
            string folderName = Microsoft.VisualBasic.Interaction.InputBox("Enter folder name:", "Create Folder", "New Folder"); // Prompts the user to enter a folder name.
            if (!string.IsNullOrEmpty(folderName) && !folderFiles.ContainsKey(folderName)) // Checks if the folder name is valid and doesn't already exist.
            {
                folderFiles[folderName] = new List<string>(); // Adds the new folder to the dictionary.

                ListViewItem item1 = new ListViewItem(folderName ?? string.Empty) // Creates a new list view item for the folder.
                {
                    ImageKey = "folder"
                };
                folderListView.Items.Add(item1); // Adds the folder to the folder list view.

                ListViewItem item2 = new ListViewItem(folderName ?? string.Empty) // Creates another list view item for the scheduled folder list view.
                {
                    ImageKey = "folder"
                };
                scheduleFolderListView.Items.Add(item2); // Adds the folder to the scheduled folder list view.

                SaveFolders(); // Saves the folders to storage.
            }
        }

        private void DeleteFolderButton_Click(object sender, EventArgs e) // Event handler for deleting a folder.
        {
            if (folderListView.SelectedItems.Count > 0) // Checks if a folder is selected.
            {
                string? selectedFolder = folderListView.SelectedItems[0].Text; // Gets the selected folder name.
                if (!string.IsNullOrEmpty(selectedFolder)) // Checks if the folder name is valid.
                {
                    folderFiles.Remove(selectedFolder); // Removes the folder from the dictionary.
                    folderListView.Items.Remove(folderListView.SelectedItems[0]); // Removes the folder from the folder list view.
                    var scheduleItem = scheduleFolderListView.Items.Cast<ListViewItem>().FirstOrDefault(i => i.Text == selectedFolder); // Finds the folder in the scheduled folder list view.
                    if (scheduleItem != null)
                    {
                        scheduleFolderListView.Items.Remove(scheduleItem); // Removes the folder from the scheduled folder list view.
                    }
                    UpdateFileListView(); // Updates the file list view.
                    SaveFolders(); // Saves the folders to storage.
                }
            }
            else
            {
                MessageBox.Show("No folder selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if no folder is selected.
            }
        }

        private void FolderListView_SelectedIndexChanged(object sender, EventArgs e) // Event handler for when the folder selection changes.
        {
            UpdateFileListView(); // Updates the file list view.
        }

        private void ScheduleFolderListView_SelectedIndexChanged(object sender, EventArgs e) // Event handler for when the scheduled folder selection changes.
        {
            UpdateScheduleFileListView(); // Updates the scheduled file list view.
        }

        private void FileListView_MouseDoubleClick(object sender, MouseEventArgs e) // Event handler for double-clicking a file in the list view.
        {
            if (fileListView.SelectedItems.Count > 0) // Checks if a file is selected.
            {
                string fileName = fileListView.SelectedItems[0].Text; // Gets the selected file name.
                if (filePaths.TryGetValue(fileName, out string? filePath)) // Tries to get the full file path.
                {
                    try
                    {
                        Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true }); // Opens the file with the associated application.
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to open file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if the file fails to open.
                    }
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e) // Event handler for deleting selected files.
        {
            if (fileListView.SelectedItems.Count > 0) // Checks if any files are selected.
            {
                string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null; // Gets the selected folder.
                if (string.IsNullOrEmpty(selectedFolder)) return; // Returns if no folder is selected.

                foreach (ListViewItem selectedItem in fileListView.SelectedItems) // Iterates through the selected files.
                {
                    string? fileName = selectedItem.Text; // Gets the file name.
                    if (fileName != null)
                    {
                        folderFiles[selectedFolder].Remove(fileName); // Removes the file from the folder's file list.
                        filePaths.Remove(fileName); // Removes the file from the file paths dictionary.
                    }
                }
                UpdateFileListView(); // Updates the file list view.
                SaveFolders(); // Saves the folders to storage.
                SaveFilePaths(); // Saves the file paths to storage.
            }
        }

        private void DeleteAllButton_Click(object sender, EventArgs e) // Event handler for deleting all files in a folder.
        {
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null; // Gets the selected folder.
            if (!string.IsNullOrEmpty(selectedFolder)) // Checks if a folder is selected.
            {
                folderFiles[selectedFolder].Clear(); // Clears all files in the selected folder.
                UpdateFileListView(); // Updates the file list view.
                SaveFolders(); // Saves the folders to storage.
                SaveFilePaths(); // Saves the file paths to storage.
            }
        }

        private void PrintButton_Click(object sender, EventArgs e) // Event handler for printing files in a selected folder.
        {
            string? selectedFolder = folderListView.SelectedItems.Count > 0 ? folderListView.SelectedItems[0].Text : null; // Gets the selected folder.
            if (string.IsNullOrEmpty(selectedFolder) || folderFiles[selectedFolder].Count == 0) // Checks if a folder is selected and if it contains files.
            {
                MessageBox.Show("No files selected.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error if no files are selected.
                return;
            }

            printButton.Enabled = false; // Disables the print button to prevent multiple clicks.
            Thread printThread = new(() => ProcessPrintQueue(selectedFolder)); // Creates a new thread to handle printing.
            printThread.Start(); // Starts the printing thread.
        }

        private void ProcessPrintQueue(string folderName) // Method to process the printing queue for a folder.
        {
            int fileCount = 0; // Counter for the number of files printed.
            List<string> remainingFiles = new(folderFiles[folderName]); // Gets the list of files to print.
            int totalFiles = remainingFiles.Count; // Gets the total number of files.

            Invoke(new Action(() => // Invokes an action on the UI thread.
            {
                progressBar.Maximum = totalFiles; // Sets the maximum value for the progress bar.
                progressBar.Value = 0; // Resets the progress bar value.
                progressBar.Step = 1; // Sets the step value for the progress bar.
                statusLabel.Text = $"Printing {totalFiles} files..."; // Updates the status label.
            }));

            int maxDegreeOfParallelism = 5; // Sets the maximum number of parallel tasks.

            Parallel.ForEach(remainingFiles, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, file => // Processes each file in parallel.
            {
                try
                {
                    if (!filePaths.TryGetValue(file, out var filePath)) // Tries to get the full file path.
                    {
                        throw new FileNotFoundException("File path not found.", file); // Throws an exception if the file path is not found.
                    }
                    if (!File.Exists(filePath)) // Checks if the file exists.
                    {
                        throw new FileNotFoundException("File path not found.", filePath); // Throws an exception if the file does not exist.
                    }

                    string extension = Path.GetExtension(file).ToLower(); // Gets the file extension.
                    if (extension == ".pdf") // Checks if the file is a PDF.
                    {
                        PrintPdf(filePath); // Prints the PDF.
                    }
                    else
                    {
                        PrintDocument printDocument = new PrintDocument // Initializes a PrintDocument for non-PDF files.
                        {
                            DocumentName = Path.GetFileName(filePath),
                            PrinterSettings = new PrinterSettings
                            {
                                PrinterName = this.printDocument.PrinterSettings.PrinterName // Uses the main printer settings.
                            }
                        };

                        printDocument.PrintPage += (sender, e) => // Defines how to print non-PDF documents.
                        {
                            using (Image image = Image.FromFile(filePath)) // Loads the image file.
                            {
                                e.Graphics.DrawImage(image, new Point(0, 0)); // Draws the image on the print page.
                            }
                        };
                        printDocument.Print(); // Sends the document to the printer.
                    }

                    LogToFile($"Printed: {filePath}"); // Logs the successful print operation.
                    Interlocked.Increment(ref fileCount); // Increments the file count.

                    Invoke(new Action(() => // Invokes an action on the UI thread.
                    {
                        progressBar.PerformStep(); // Updates the progress bar.
                        statusLabel.Text = $"Printed {fileCount}/{totalFiles} files."; // Updates the status label.
                    }));
                }
                catch (FileNotFoundException fnfEx)
                {
                    LogToFile($"Failed to print {file}: File not found - {fnfEx.FileName}"); // Logs if the file was not found.
                }
                catch (IOException ioEx)
                {
                    LogToFile($"Failed to print {file}: IO issue - {ioEx.Message}"); // Logs if there was an I/O error.
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    LogToFile($"Failed to print {file}: Unauthorized access - {uaEx.Message}"); // Logs if there was an unauthorized access error.
                }
                catch (System.Drawing.Printing.InvalidPrinterException ipEx)
                {
                    LogToFile($"Failed to print {file}: Invalid printer - {ipEx.Message}"); // Logs if there was a printer error.
                }
                catch (Exception ex)
                {
                    LogToFile($"Failed to print {file}: General error - {ex.Message}"); // Logs any other general errors.
                }
            });

            Invoke(new Action(() => // Invokes an action on the UI thread after all tasks complete.
            {
                PrintDone(fileCount); // Displays a message when all files are printed.
                MarkFolderAsPrinted(folderName); // Marks the folder as printed.
                statusLabel.Text = "Printing completed."; // Updates the status label.
                LogToFile("All files sent to printer.", folderName, fileCount); // Logs the printing summary.
            }));
        }

        private void PrintDone(int fileCount) // Method to display a message when printing is completed.
        {
            MessageBox.Show($"All files sent to printer. Number of files sent: {fileCount}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
            printButton.Enabled = true; // Re-enables the print button.
        }

        private async void LogToFile(string message, string folderName = "", int fileCount = 0) // Method to log messages to the log file.
        {
            bool isLogged = false; // Flag to check if logging was successful.
            int attempts = 0; // Counter for the number of attempts.
            string logMessage = $"{DateTime.Now}: {message}"; // Creates the log message.

            if (!string.IsNullOrEmpty(folderName) && fileCount > 0) // Checks if folder name and file count are provided.
            {
                logMessage = $"{DateTime.Now}: {userName} printed {fileCount} files from folder '{folderName}'. Details: {message}"; // Updates the log message with folder and file details.
            }

            while (!isLogged && attempts < 5) // Tries to log the message up to 5 times.
            {
                try
                {
                    using StreamWriter writer = new(logFilePath, true); // Opens the log file for writing.
                    await writer.WriteLineAsync(logMessage); // Writes the log message asynchronously.
                    isLogged = true; // Sets the flag to true if logging was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isLogged) // Checks if logging failed after multiple attempts.
            {
                MessageBox.Show("Failed to write to log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void LoadLogButton_Click(object sender, EventArgs e) // Event handler for loading the log file.
        {
            bool isLoaded = false; // Flag to check if loading was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isLoaded && attempts < 5) // Tries to load the log file up to 5 times.
            {
                try
                {
                    if (File.Exists(logFilePath)) // Checks if the log file exists.
                    {
                        logListBox.Items.Clear(); // Clears the current items in the log list box.
                        string[] logLines = await File.ReadAllLinesAsync(logFilePath); // Reads all lines from the log file asynchronously.
                        foreach (string line in logLines) // Iterates through the log lines.
                        {
                            logListBox.Items.Add(line); // Adds each line to the log list box.
                        }
                    }
                    else
                    {
                        MessageBox.Show("No log file found.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a message if the log file is not found.
                    }
                    isLoaded = true; // Sets the flag to true if loading was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isLoaded) // Checks if loading failed after multiple attempts.
            {
                MessageBox.Show("Failed to load log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void ClearLogButton_Click(object sender, EventArgs e) // Event handler for clearing the log file.
        {
            bool isCleared = false; // Flag to check if clearing was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isCleared && attempts < 5) // Tries to clear the log file up to 5 times.
            {
                try
                {
                    if (File.Exists(logFilePath)) // Checks if the log file exists.
                    {
                        await File.WriteAllTextAsync(logFilePath, string.Empty); // Clears the log file asynchronously.
                        logListBox.Items.Clear(); // Clears the log list box.
                        MessageBox.Show("Log file cleared.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
                    }
                    isCleared = true; // Sets the flag to true if clearing was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isCleared) // Checks if clearing failed after multiple attempts.
            {
                MessageBox.Show("Failed to clear log file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private void SelectPrinterButton_Click(object sender, EventArgs e) // Event handler for selecting a printer.
        {
            PrintDialog printDialog = new() { Document = printDocument }; // Creates a new print dialog with the current print document.
            if (printDialog.ShowDialog() == DialogResult.OK) // Shows the print dialog and checks if a printer was selected.
            {
                printDocument.PrinterSettings = printDialog.PrinterSettings; // Updates the printer settings.
                MessageBox.Show($"Printer selected: {printDocument.PrinterSettings.PrinterName}", "Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
            }
        }

        private void SelectPrintFilesPrinterButton_Click(object sender, EventArgs e) // Event handler for selecting a printer for the print files.
        {
            PrintDialog printDialog = new() { Document = printDocument }; // Creates a new print dialog with the current print document.
            if (printDialog.ShowDialog() == DialogResult.OK) // Shows the print dialog and checks if a printer was selected.
            {
                printDocument.PrinterSettings = printDialog.PrinterSettings; // Updates the printer settings.
                MessageBox.Show($"Printer selected: {printDocument.PrinterSettings.PrinterName}", "Printer Selected", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
            }
        }

        private void ScheduleButton_Click(object sender, EventArgs e) // Event handler for scheduling a print job.
        {
            string? selectedFolder = scheduleFolderListView.SelectedItems.Count > 0 ? scheduleFolderListView.SelectedItems[0].Text : null; // Gets the selected folder.
            if (string.IsNullOrEmpty(selectedFolder) || !scheduleDateTimePicker.Checked) // Checks if a folder and time are selected.
            {
                MessageBox.Show("No folder selected or no time specified.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
                return;
            }

            DateTime scheduledTime = scheduleDateTimePicker.Value; // Gets the selected scheduled time.
            if (IsTimeSlotTaken(scheduledTime, out string existingUser, out string existingFolder)) // Checks if the time slot is already taken.
            {
                MessageBox.Show($"Time slot is already taken by {existingUser} for folder '{existingFolder}'.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
                return;
            }

            TimeSpan timeUntilPrint = scheduledTime - DateTime.Now; // Calculates the time until the scheduled print.

            if (timeUntilPrint <= TimeSpan.Zero) // Checks if the scheduled time is in the future.
            {
                MessageBox.Show("Scheduled time must be in the future.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
                return;
            }

            List<string> scheduledFiles;
            try
            {
                scheduledFiles = folderFiles[selectedFolder].Select(f => // Tries to get the file paths for the selected folder.
                {
                    if (filePaths.ContainsKey(f))
                    {
                        return filePaths[f];
                    }
                    else
                    {
                        throw new KeyNotFoundException($"File path not found for file: {f}"); // Throws an exception if a file path is not found.
                    }
                }).ToList();
            }
            catch (KeyNotFoundException ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message if a file path is not found.
                return;
            }

            ScheduledPrintJob printJob = new() // Creates a new scheduled print job.
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

            printJob.Timer.Tick += (s, args) => // Defines the tick event for the timer.
            {
                printJob.Timer.Stop(); // Stops the timer when the time is reached.
                SchedulePrintFiles(printJob); // Schedules the print files.
            };
            printJob.Timer.Start(); // Starts the timer.

            scheduledPrintJobs.Add(printJob); // Adds the scheduled print job to the list.
            UpdateScheduledJobsListBox(); // Updates the scheduled jobs list box.
            LogScheduledJob(printJob); // Logs the scheduled job.
            SaveScheduledJobs(); // Saves the scheduled jobs to storage.

            MessageBox.Show($"Folder '{selectedFolder}' scheduled to print at {scheduledTime}.", "Scheduled", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
            UpdateCalendar(); // Updates the calendar.
        }

        private void UpdateScheduledJobsListBox() // Method to update the scheduled jobs list box.
        {
            scheduledJobsListBox.Items.Clear(); // Clears the current items in the scheduled jobs list box.
            foreach (var job in scheduledPrintJobs) // Iterates through the scheduled print jobs.
            {
                scheduledJobsListBox.Items.Add($"Scheduled by {job.UserName} at {job.ScheduledTime}, Folder: {job.FolderName}, Files: {string.Join(", ", job.Files.Select(Path.GetFileName))}"); // Adds each job to the list box.
            }
        }

        private void SchedulePrintFiles(ScheduledPrintJob job) // Method to schedule the printing of files.
        {
            foreach (string filePath in job.Files) // Iterates through the files in the scheduled job.
            {
                string fileName = Path.GetFileName(filePath); // Gets the file name.
                try
                {
                    if (!File.Exists(filePath)) // Checks if the file exists.
                    {
                        throw new FileNotFoundException("File path not found.", filePath); // Throws an exception if the file does not exist.
                    }

                    string extension = Path.GetExtension(fileName).ToLower(); // Gets the file extension.
                    if (extension == ".pdf") // Checks if the file is a PDF.
                    {
                        PrintPdf(filePath); // Prints the PDF.
                    }
                    else
                    {
                        Process.Start(new ProcessStartInfo(filePath) { Verb = "print", UseShellExecute = true }); // Prints the file using the associated application.
                    }
                    LogToFile($"Scheduled Printed: {filePath}"); // Logs the successful print operation.
                }
                catch (FileNotFoundException fnfEx)
                {
                    LogToFile($"Failed to print {fileName}: File not found - {fnfEx.FileName}"); // Logs if the file was not found.
                }
                catch (Exception ex)
                {
                    LogToFile($"Failed to print {fileName}: {ex.Message}"); // Logs any other general errors.
                }
            }
            Invoke(new Action(() => // Invokes an action on the UI thread.
            {
                scheduledPrintJobs.Remove(job); // Removes the completed job from the list.
                UpdateScheduledJobsListBox(); // Updates the scheduled jobs list box.
                SaveScheduledJobs(); // Saves the scheduled jobs to storage.
                LoadScheduledJobs(); // Reloads the scheduled jobs.
                MarkFolderAsPrinted(job.FolderName); // Marks the folder as printed.
            }));
        }

        private bool IsTimeSlotTaken(DateTime scheduledTime, out string existingUser, out string existingFolder) // Method to check if a time slot is taken.
        {
            existingUser = string.Empty; // Initializes the existing user.
            existingFolder = string.Empty; // Initializes the existing folder.

            if (!File.Exists(sharedScheduledJobsFilePath)) return false; // Returns false if the shared scheduled jobs file doesn't exist.

            string[] jobLines = File.ReadAllLines(sharedScheduledJobsFilePath); // Reads all lines from the shared scheduled jobs file.
            foreach (string line in jobLines) // Iterates through the job lines.
            {
                string[] parts = line.Split('|'); // Splits the line into parts.
                string userName = parts[0]; // Gets the user name.
                string folderName = parts[1]; // Gets the folder name.
                DateTime loggedTime = ParseDateTime(parts[3]); // Parses the scheduled time.

                if (Math.Abs((loggedTime - scheduledTime).TotalMinutes) < 1) // Checks if the time slot is within 1 minute.
                {
                    existingUser = userName; // Sets the existing user.
                    existingFolder = folderName; // Sets the existing folder.
                    return true; // Returns true if the time slot is taken.
                }
            }
            return false; // Returns false if the time slot is not taken.
        }

        private async void LogScheduledJob(ScheduledPrintJob job) // Method to log a scheduled print job.
        {
            bool isLogged = false; // Flag to check if logging was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isLogged && attempts < 5) // Tries to log the message up to 5 times.
            {
                try
                {
                    using StreamWriter writer = new(sharedScheduledJobsFilePath, true); // Opens the shared scheduled jobs file for writing.
                    await writer.WriteLineAsync($"{job.UserName}|{job.FolderName}|{string.Join(",", job.Files)}|{job.ScheduledTime.ToString("O")}|{job.Timer.Interval}"); // Writes the scheduled job details asynchronously.
                    isLogged = true; // Sets the flag to true if logging was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isLogged) // Checks if logging failed after multiple attempts.
            {
                MessageBox.Show("Failed to write scheduled job to file after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void LoadScheduledJobs() // Method to load scheduled print jobs.
        {
            bool isLoaded = false; // Flag to check if loading was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isLoaded && attempts < 5) // Tries to load the scheduled jobs up to 5 times.
            {
                try
                {
                    if (!File.Exists(sharedScheduledJobsFilePath)) return; // Returns if the shared scheduled jobs file doesn't exist.

                    scheduledPrintJobs.Clear(); // Clears the current scheduled jobs list.
                    string[] jobLines = await File.ReadAllLinesAsync(sharedScheduledJobsFilePath); // Reads all lines from the shared scheduled jobs file asynchronously.
                    foreach (string line in jobLines) // Iterates through the job lines.
                    {
                        string[] parts = line.Split('|'); // Splits the line into parts.
                        string userName = parts[0]; // Gets the user name.
                        string folderName = parts[1]; // Gets the folder name.
                        List<string> files = parts[2].Split(',').ToList(); // Gets the list of files.
                        DateTime scheduledTime = ParseDateTime(parts[3]); // Parses the scheduled time.
                        int interval = int.Parse(parts[4]); // Parses the timer interval.

                        TimeSpan timeUntilPrint = scheduledTime - DateTime.Now; // Calculates the time until the scheduled print.
                        if (timeUntilPrint > TimeSpan.Zero) // Checks if the scheduled time is in the future.
                        {
                            ScheduledPrintJob printJob = new() // Creates a new scheduled print job.
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

                            printJob.Timer.Tick += (s, args) => // Defines the tick event for the timer.
                            {
                                printJob.Timer.Stop(); // Stops the timer when the time is reached.
                                SchedulePrintFiles(printJob); // Schedules the print files.
                            };
                            printJob.Timer.Start(); // Starts the timer.

                            scheduledPrintJobs.Add(printJob); // Adds the scheduled print job to the list.
                        }
                    }
                    isLoaded = true; // Sets the flag to true if loading was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (isLoaded) // Checks if loading was successful.
            {
                UpdateScheduledJobsListBox(); // Updates the scheduled jobs list box.
                UpdateCalendar(); // Updates the calendar.
            }
            else
            {
                MessageBox.Show("Failed to load scheduled jobs after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void SaveScheduledJobs() // Method to save scheduled print jobs.
        {
            bool isSaved = false; // Flag to check if saving was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isSaved && attempts < 5) // Tries to save the scheduled jobs up to 5 times.
            {
                try
                {
                    using StreamWriter writer = new(sharedScheduledJobsFilePath); // Opens the shared scheduled jobs file for writing.
                    foreach (var job in scheduledPrintJobs) // Iterates through the scheduled print jobs.
                    {
                        await writer.WriteLineAsync($"{job.UserName}|{job.FolderName}|{string.Join(",", job.Files)}|{job.ScheduledTime.ToString("O")}|{job.Timer.Interval}"); // Writes each scheduled job details asynchronously.
                    }
                    isSaved = true; // Sets the flag to true if saving was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isSaved) // Checks if saving failed after multiple attempts.
            {
                MessageBox.Show("Failed to save scheduled jobs after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private DateTime ParseDateTime(string dateTimeString) // Method to parse a date-time string.
        {
            if (DateTime.TryParse(dateTimeString, null, DateTimeStyles.RoundtripKind, out DateTime dateTime)) // Tries to parse the date-time string.
            {
                return dateTime; // Returns the parsed date-time.
            }
            throw new FormatException($"Date string '{dateTimeString}' was not recognized as a valid DateTime."); // Throws an exception if parsing fails.
        }

        private async void LoadFolders() // Method to load folders and associated files.
        {
            bool isLoaded = false; // Flag to check if loading was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isLoaded && attempts < 5) // Tries to load the folders up to 5 times.
            {
                try
                {
                    if (!File.Exists(foldersFilePath)) return; // Returns if the folders file doesn't exist.

                    folderFiles.Clear(); // Clears the current folder dictionary.
                    folderListView.Items.Clear(); // Clears the folder list view.
                    scheduleFolderListView.Items.Clear(); // Clears the scheduled folder list view.

                    string[] folderLines = await File.ReadAllLinesAsync(foldersFilePath); // Reads all lines from the folders file asynchronously.
                    foreach (string line in folderLines) // Iterates through the folder lines.
                    {
                        string[] parts = line.Split(new[] { ": " }, StringSplitOptions.None); // Splits the line into parts.
                        string folderName = parts[0]; // Gets the folder name.
                        List<string> files = parts[1].Split(new[] { ", " }, StringSplitOptions.None).ToList(); // Gets the list of files.

                        folderFiles[folderName] = files; // Adds the folder and its files to the dictionary.

                        ListViewItem item1 = new ListViewItem(folderName ?? string.Empty) // Creates a new list view item for the folder.
                        {
                            ImageKey = "folder"
                        };
                        folderListView.Items.Add(item1); // Adds the folder to the folder list view.

                        ListViewItem item2 = new ListViewItem(folderName ?? string.Empty) // Creates another list view item for the scheduled folder list view.
                        {
                            ImageKey = "folder"
                        };
                        scheduleFolderListView.Items.Add(item2); // Adds the folder to the scheduled folder list view.
                    }
                    isLoaded = true; // Sets the flag to true if loading was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.   await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isLoaded) // Checks if loading failed after multiple attempts.
            {
                MessageBox.Show("Failed to load folders after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void SaveFolders() // Method to save folders and associated files.
        {
            bool isSaved = false; // Flag to check if saving was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isSaved && attempts < 5) // Tries to save the folders up to 5 times.
            {
                try
                {
                    using StreamWriter writer = new(foldersFilePath); // Opens the folders file for writing.
                    foreach (var folder in folderFiles) // Iterates through the folders.
                    {
                        await writer.WriteLineAsync($"{folder.Key}: {string.Join(", ", folder.Value)}"); // Writes each folder and its files asynchronously.
                    }
                    isSaved = true; // Sets the flag to true if saving was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isSaved) // Checks if saving failed after multiple attempts.
            {
                MessageBox.Show("Failed to save folders after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void LoadFilePaths() // Method to load file paths.
        {
            bool isLoaded = false; // Flag to check if loading was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isLoaded && attempts < 5) // Tries to load the file paths up to 5 times.
            {
                try
                {
                    if (!File.Exists(filePathsFilePath)) return; // Returns if the file paths file doesn't exist.

                    filePaths.Clear(); // Clears the current file paths dictionary.
                    string[] lines = await File.ReadAllLinesAsync(filePathsFilePath); // Reads all lines from the file paths file asynchronously.
                    foreach (string line in lines) // Iterates through the file path lines.
                    {
                        string[] parts = line.Split(new[] { ": " }, StringSplitOptions.None); // Splits the line into parts.
                        if (parts.Length == 2)
                        {
                            filePaths[parts[0]] = parts[1]; // Adds the file path to the dictionary.
                        }
                    }
                    isLoaded = true; // Sets the flag to true if loading was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isLoaded) // Checks if loading failed after multiple attempts.
            {
                MessageBox.Show("Failed to load file paths after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private async void SaveFilePaths() // Method to save file paths.
        {
            bool isSaved = false; // Flag to check if saving was successful.
            int attempts = 0; // Counter for the number of attempts.
            while (!isSaved && attempts < 5) // Tries to save the file paths up to 5 times.
            {
                try
                {
                    using StreamWriter writer = new(filePathsFilePath); // Opens the file paths file for writing.
                    foreach (var kvp in filePaths) // Iterates through the file paths.
                    {
                        await writer.WriteLineAsync($"{kvp.Key}: {kvp.Value}"); // Writes each file path asynchronously.
                    }
                    isSaved = true; // Sets the flag to true if saving was successful.
                }
                catch (IOException)
                {
                    attempts++; // Increments the attempt counter if an IO exception occurs.
                    await Task.Delay(500); // Waits for 500ms before retrying.
                }
            }

            if (!isSaved) // Checks if saving failed after multiple attempts.
            {
                MessageBox.Show("Failed to save file paths after multiple attempts.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private void UpdateCalendar() // Method to update the weekly calendar.
        {
            InitializeWeeklyCalendar(); // Initializes the weekly calendar.
            PopulateWeeklyCalendar(); // Populates the weekly calendar with scheduled jobs.
        }

        private void InitializeWeeklyCalendar() // Method to initialize the weekly calendar.
        {
            weeklyCalendarGridView.Columns.Clear(); // Clears the columns in the calendar grid view.
            weeklyCalendarGridView.Rows.Clear(); // Clears the rows in the calendar grid view.

            for (int i = 0; i < 7; i++) // Iterates through the days of the week.
            {
                var column = new DataGridViewTextBoxColumn // Creates a new column for each day.
                {
                    Name = ((DayOfWeek)i).ToString(), // Sets the column name to the day of the week.
                    HeaderText = DateTime.Now.AddDays(i - (int)DateTime.Now.DayOfWeek).ToString("dddd dd/MM"), // Sets the header text to the day and date.
                    Width = 200 // Sets the column width.
                };
                weeklyCalendarGridView.Columns.Add(column); // Adds the column to the calendar grid view.
            }

            for (int hour = 0; hour < 24; hour++) // Iterates through the hours of the day.
            {
                for (int minute = 0; minute < 60; minute++) // Iterates through the minutes of the hour.
                {
                    var row = new DataGridViewRow // Creates a new row for each time slot.
                    {
                        Height = 40 // Sets the row height.
                    };
                    row.HeaderCell.Value = $"{hour:D2}:{minute:D2}"; // Sets the row header to the time.
                    weeklyCalendarGridView.Rows.Add(row); // Adds the row to the calendar grid view.
                }
            }

            weeklyCalendarGridView.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToAllHeaders); // Resizes the row headers to fit the text.
            weeklyCalendarGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Sets the columns to fill the available space.
            weeklyCalendarGridView.DefaultCellStyle.WrapMode = DataGridViewTriState.True; // Sets the cell style to wrap text.
            weeklyCalendarGridView.DefaultCellStyle.Font = new Font("Arial", 10); // Sets the cell font.
            weeklyCalendarGridView.ColumnHeadersDefaultCellStyle.Font = new Font("Arial", 10, FontStyle.Bold); // Sets the column header font.
        }

        private void PopulateWeeklyCalendar() // Method to populate the weekly calendar with scheduled jobs.
        {
            foreach (var job in scheduledPrintJobs) // Iterates through the scheduled print jobs.
            {
                if (job.ScheduledTime.Date >= DateTime.Now.Date && job.ScheduledTime.Date < DateTime.Now.Date.AddDays(7)) // Checks if the job is scheduled within the next 7 days.
                {
                    int columnIndex = (int)job.ScheduledTime.DayOfWeek; // Gets the column index for the day of the week.
                    int rowIndex = job.ScheduledTime.Hour * 60 + job.ScheduledTime.Minute; // Gets the row index for the time.

                    var cell = weeklyCalendarGridView.Rows[rowIndex].Cells[columnIndex]; // Gets the cell for the scheduled time.
                    cell.Style.BackColor = Color.LightCoral; // Sets the cell background color.

                    string existingText = cell.Value?.ToString(); // Gets the existing text in the cell.
                    if (string.IsNullOrEmpty(existingText)) // Checks if the cell is empty.
                    {
                        cell.Value = $"[{job.ScheduledTime:HH:mm}] {job.UserName} - {job.FolderName}"; // Sets the cell value to the job details.
                    }
                    else
                    {
                        cell.Value = existingText + Environment.NewLine + $"[{job.ScheduledTime:HH:mm}] {job.UserName} - {job.FolderName}"; // Adds the job details to the cell.
                    }
                }
            }
        }

        private void TabControl_SelectedIndexChanged(object sender, EventArgs e) // Event handler for when the tab control index changes.
        {
            if (tabControl.SelectedTab == tabPage3) // Checks if the calendar tab is selected.
            {
                UpdateCalendar(); // Updates the calendar.
            }
        }

        private void RefreshCalendarButton_Click(object sender, EventArgs e) // Event handler for refreshing the calendar.
        {
            LoadScheduledJobs(); // Reloads the scheduled jobs.
        }

        private void MakeRealButton_Click(object sender, EventArgs e) // Event handler for creating a real folder with the files.
        {
            if (folderListView.SelectedItems.Count == 0) // Checks if a folder is selected.
            {
                MessageBox.Show("Please select a folder.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
                return;
            }

            string selectedFolder = folderListView.SelectedItems[0].Text; // Gets the selected folder.

            if (folderFiles.ContainsKey(selectedFolder)) // Checks if the folder exists in the dictionary.
            {
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()) // Opens a folder browser dialog to select the target path.
                {
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK) // Checks if a folder was selected.
                    {
                        string targetPath = folderBrowserDialog.SelectedPath; // Gets the target path.
                        string sanitizedFolderName = SanitizeFileName(selectedFolder); // Sanitizes the folder name.
                        string newFolderPath = Path.Combine(targetPath, sanitizedFolderName); // Combines the target path with the sanitized folder name.

                        Directory.CreateDirectory(newFolderPath); // Creates the new folder.

                        foreach (string file in folderFiles[selectedFolder]) // Iterates through the files in the selected folder.
                        {
                            string sourceFilePath = filePaths[file]; // Gets the source file path.
                            string targetFilePath = Path.Combine(newFolderPath, file); // Combines the new folder path with the file name.
                            File.Copy(sourceFilePath, targetFilePath, true); // Copies the file to the new folder.
                        }

                        MessageBox.Show($"Folder '{selectedFolder}' and its files have been created at {newFolderPath}.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
                    }
                }
            }
            else
            {
                MessageBox.Show("Selected folder does not exist.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private string SanitizeFileName(string fileName) // Method to sanitize a file name.
        {
            foreach (char c in Path.GetInvalidFileNameChars()) // Iterates through invalid file name characters.
            {
                fileName = fileName.Replace(c, '_'); // Replaces invalid characters with an underscore.
            }
            return fileName; // Returns the sanitized file name.
        }

        private void DeleteScheduledJobButton_Click(object sender, EventArgs e) // Event handler for deleting a scheduled job.
        {
            if (selectedCalendarJob != null) // Checks if a scheduled job is selected.
            {
                selectedCalendarJob.Timer.Stop(); // Stops the timer for the selected job.
                scheduledPrintJobs.Remove(selectedCalendarJob); // Removes the job from the scheduled jobs list.
                UpdateScheduledJobsListBox(); // Updates the scheduled jobs list box.
                SaveScheduledJobs(); // Saves the scheduled jobs to storage.
                UpdateCalendar(); // Updates the calendar.
                MessageBox.Show("Scheduled job deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); // Displays a success message.
                selectedCalendarJob = null; // Resets the selected job.
            }
            else
            {
                MessageBox.Show("No scheduled job selected in the calendar.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); // Displays an error message.
            }
        }

        private void WeeklyCalendarGridView_CellClick(object sender, DataGridViewCellEventArgs e) // Event handler for clicking a cell in the calendar grid view.
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) // Checks if a valid cell is clicked.
            {
                return;
            }

            var cellValue = weeklyCalendarGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Value; // Gets the cell value.
            if (cellValue != null)
            {
                string cellText = cellValue.ToString(); // Converts the cell value to text.
                selectedCalendarJob = scheduledPrintJobs.FirstOrDefault(job => // Finds the scheduled job corresponding to the cell value.
                    cellText.Contains(job.FolderName) && cellText.Contains(job.UserName) && cellText.Contains(job.ScheduledTime.ToString("HH:mm")));

                if (selectedCalendarJob != null) // Checks if a job is found.
                {
                    MessageBox.Show($"Selected Job: {selectedCalendarJob.FolderName} scheduled by {selectedCalendarJob.UserName} at {selectedCalendarJob.ScheduledTime}"); // Displays the selected job details.
                }
            }
        }

        private void scheduledJobsListBox_MeasureItem(object sender, MeasureItemEventArgs e) // Event handler for measuring the height of items in the scheduled jobs list box.
        {
            e.ItemHeight = 50; // Sets the item height.
        }

        private void scheduledJobsListBox_DrawItem(object sender, DrawItemEventArgs e) // Event handler for drawing items in the scheduled jobs list box.
        {
            if (e.Index < 0 || e.Index >= scheduledJobsListBox.Items.Count) // Checks if a valid item is drawn.
            {
                return;
            }

            e.DrawBackground(); // Draws the background of the item.
            var item = scheduledJobsListBox.Items[e.Index].ToString(); // Gets the item text.
            e.Graphics.DrawString(item, e.Font, Brushes.Black, e.Bounds); // Draws the item text.
            e.DrawFocusRectangle(); // Draws the focus rectangle around the item.
        }

        private void MarkFolderAsPrinted(string folderName) // Method to mark a folder as printed.
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // Gets the current timestamp.
            string newFolderName = $"{folderName} (Printed at {timestamp})"; // Creates the new folder name with the timestamp.

            if (folderFiles.ContainsKey(folderName)) // Checks if the folder exists in the dictionary.
            {
                List<string> files = folderFiles[folderName]; // Gets the files in the folder.
                folderFiles.Remove(folderName); // Removes the old folder name.
                folderFiles[newFolderName] = files; // Adds the new folder name with the files.
            }

            foreach (ListViewItem item in folderListView.Items) // Iterates through the items in the folder list view.
            {
                if (item.Text == folderName) // Checks if the item text matches the folder name.
                {
                    item.Text = newFolderName; // Updates the item text with the new folder name.
                    break;
                }
            }

            foreach (ListViewItem item in scheduleFolderListView.Items) // Iterates through the items in the scheduled folder list view.
            {
                if (item.Text == folderName) // Checks if the item text matches the folder name.
                {
                    item.Text = newFolderName; // Updates the item text with the new folder name.
                    break;
                }
            }

            SaveFolders(); // Saves the folders to storage.
        }

        private void InitializeNotifyIcon() // Method to initialize the system tray icon.
        {
            notifyIcon = new NotifyIcon // Creates a new notify icon.
            {
                Icon = new Icon("appicon.ico"), // Sets the icon for the notify icon.
                Text = "File Printer App", // Sets the text for the notify icon.
                Visible = true // Makes the notify icon visible.
            };

            contextMenuStrip = new ContextMenuStrip(); // Creates a new context menu strip for the notify icon.
            contextMenuStrip.Items.Add("Show", null, Show_Click); // Adds a "Show" item to the context menu.
            contextMenuStrip.Items.Add("Exit", null, Exit_Click); // Adds an "Exit" item to the context menu.

            notifyIcon.ContextMenuStrip = contextMenuStrip; // Assigns the context menu to the notify icon.
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick; // Assigns the double-click event handler for the notify icon.
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e) // Event handler for double-clicking the notify icon.
        {
            ShowForm(); // Shows the main form.
        }

        private void Show_Click(object? sender, EventArgs e) // Event handler for clicking "Show" in the context menu.
        {
            ShowForm(); // Shows the main form.
        }

        private void Exit_Click(object? sender, EventArgs e) // Event handler for clicking "Exit" in the context menu.
        {
            notifyIcon!.Visible = false; // Hides the notify icon.
            Application.Exit(); // Exits the application.
        }

        private void ShowForm() // Method to show the main form.
        {
            this.Show(); // Shows the form.
            this.WindowState = FormWindowState.Normal; // Restores the form if it's minimized.
            this.BringToFront(); // Brings the form to the front.
        }

        protected override void OnResize(EventArgs e) // Event handler for resizing the form.
        {
            base.OnResize(e); // Calls the base class resize method.

            if (this.WindowState == FormWindowState.Minimized) // Checks if the form is minimized.
            {
                this.Hide(); // Hides the form.
                notifyIcon?.ShowBalloonTip(1000, "File Printer App", "Application minimized to tray.", ToolTipIcon.Info); // Shows a balloon tip from the notify icon.
            }
        }

        protected override void Dispose(bool disposing) // Method to dispose resources.
        {
            if (disposing) // Checks if disposing is true.
            {
                components?.Dispose(); // Disposes the form components.
                notifyIcon?.Dispose(); // Disposes the notify icon.
            }
            base.Dispose(disposing); // Calls the base class dispose method.
        }
    }

    public class ScheduledPrintJob // Class to represent a scheduled print job.
    {
        public string UserName { get; set; } = string.Empty; // Property to store the username.
        public string FolderName { get; set; } = string.Empty; // Property to store the folder name.
        public List<string> Files { get; set; } = new List<string>(); // Property to store the list of files.
        public DateTime ScheduledTime { get; set; } // Property to store the scheduled time.
        public System.Windows.Forms.Timer Timer { get; set; } = new System.Windows.Forms.Timer(); // Property to store the timer.
    }
}
