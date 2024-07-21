namespace FilePrinterApp
{
    partial class Form1
    {
        private System.ComponentModel.IContainer? components = null;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.ListView folderListView;
        private System.Windows.Forms.ListView fileListView;
        private System.Windows.Forms.Button selectButton;
        private System.Windows.Forms.Button deleteButton;
        private System.Windows.Forms.Button deleteAllButton;
        private System.Windows.Forms.Button printButton;
        private System.Windows.Forms.Button createFolderButton;
        private System.Windows.Forms.Button deleteFolderButton;
        private System.Windows.Forms.ListBox logListBox;
        private System.Windows.Forms.Button loadLogButton;
        private System.Windows.Forms.Button clearLogButton;
        private System.Windows.Forms.DataGridView weeklyCalendarGridView;
        private System.Windows.Forms.ListView scheduleFolderListView;
        private System.Windows.Forms.ListView scheduleFileListView;
        private System.Windows.Forms.DateTimePicker scheduleDateTimePicker;
        private System.Windows.Forms.Button scheduleSelectButton;
        private System.Windows.Forms.Button scheduleButton;
        private System.Windows.Forms.ListBox scheduledJobsListBox;
        private System.Windows.Forms.Button selectPrinterButton;
        private System.Windows.Forms.Button selectPrintFilesPrinterButton;
        private System.Windows.Forms.Button refreshCalendarButton;
        private System.Windows.Forms.Button makeRealButton;
        private System.Windows.Forms.Button deleteScheduledJobButton;
        private System.Windows.Forms.ImageList imageList;

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            tabControl = new TabControl();
            tabPage1 = new TabPage();
            folderListView = new ListView();
            imageList = new ImageList(components);
            fileListView = new ListView();
            selectButton = new Button();
            deleteButton = new Button();
            deleteAllButton = new Button();
            printButton = new Button();
            createFolderButton = new Button();
            deleteFolderButton = new Button();
            selectPrintFilesPrinterButton = new Button();
            makeRealButton = new Button();
            tabPage2 = new TabPage();
            loadLogButton = new Button();
            clearLogButton = new Button();
            logListBox = new ListBox();
            tabPage3 = new TabPage();
            weeklyCalendarGridView = new DataGridView();
            scheduleFolderListView = new ListView();
            scheduleFileListView = new ListView();
            scheduleDateTimePicker = new DateTimePicker();
            scheduleSelectButton = new Button();
            scheduleButton = new Button();
            scheduledJobsListBox = new ListBox();
            deleteScheduledJobButton = new Button();
            selectPrinterButton = new Button();
            refreshCalendarButton = new Button();
            weeklyCalendarGridView.CellClick += WeeklyCalendarGridView_CellClick;
            tabControl.SuspendLayout();
            tabPage1.SuspendLayout();
            tabPage2.SuspendLayout();
            tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)weeklyCalendarGridView).BeginInit();
            SuspendLayout();
            // 
            // tabControl
            // 
            tabControl.Controls.Add(tabPage1);
            tabControl.Controls.Add(tabPage2);
            tabControl.Controls.Add(tabPage3);
            tabControl.Dock = DockStyle.Fill;
            tabControl.Location = new Point(0, 0);
            tabControl.Name = "tabControl";
            tabControl.SelectedIndex = 0;
            tabControl.Size = new Size(1200, 700);
            tabControl.TabIndex = 0;
            tabControl.SelectedIndexChanged += TabControl_SelectedIndexChanged;
            // 
            // tabPage1
            // 
            tabPage1.Controls.Add(folderListView);
            tabPage1.Controls.Add(fileListView);
            tabPage1.Controls.Add(selectButton);
            tabPage1.Controls.Add(deleteButton);
            tabPage1.Controls.Add(deleteAllButton);
            tabPage1.Controls.Add(printButton);
            tabPage1.Controls.Add(createFolderButton);
            tabPage1.Controls.Add(deleteFolderButton);
            tabPage1.Controls.Add(selectPrintFilesPrinterButton);
            tabPage1.Controls.Add(makeRealButton);
            tabPage1.Location = new Point(4, 24);
            tabPage1.Name = "tabPage1";
            tabPage1.Padding = new Padding(3);
            tabPage1.Size = new Size(1192, 672);
            tabPage1.TabIndex = 0;
            tabPage1.Text = "Print Files";
            tabPage1.UseVisualStyleBackColor = true;
            // 
            // folderListView
            // 
            folderListView.Location = new Point(8, 6);
            folderListView.Name = "folderListView";
            folderListView.Size = new Size(300, 454);
            folderListView.SmallImageList = imageList;
            folderListView.TabIndex = 0;
            folderListView.UseCompatibleStateImageBehavior = false;
            folderListView.View = View.SmallIcon;
            folderListView.SelectedIndexChanged += FolderListView_SelectedIndexChanged;
            // 
            // imageList
            // 
            imageList.ColorDepth = ColorDepth.Depth32Bit;
            imageList.ImageSize = new Size(16, 16);
            imageList.TransparentColor = Color.Transparent;
            imageList.Images.Add("folder", Image.FromFile(@"folder-icon.png"));
            imageList.Images.Add("pdf", Image.FromFile(@"pdf-icon.png"));
            imageList.Images.Add("excel", Image.FromFile(@"excel-icon.png"));
            imageList.Images.Add("word", Image.FromFile(@"word-icon.png"));
            // 
            // fileListView
            // 
            fileListView.Location = new Point(314, 6);
            fileListView.Name = "fileListView";
            fileListView.Size = new Size(870, 454);
            fileListView.SmallImageList = imageList;
            fileListView.TabIndex = 1;
            fileListView.UseCompatibleStateImageBehavior = false;
            fileListView.View = View.SmallIcon;
            fileListView.MouseDoubleClick += FileListView_MouseDoubleClick;
            // 
            // selectButton
            // 
            selectButton.FlatStyle = FlatStyle.Flat;
            selectButton.Location = new Point(314, 466);
            selectButton.Name = "selectButton";
            selectButton.Size = new Size(95, 23);
            selectButton.TabIndex = 2;
            selectButton.Text = "Select Files";
            selectButton.UseVisualStyleBackColor = true;
            selectButton.Click += SelectButton_Click;
            // 
            // deleteButton
            // 
            deleteButton.FlatStyle = FlatStyle.Flat;
            deleteButton.Location = new Point(415, 466);
            deleteButton.Name = "deleteButton";
            deleteButton.Size = new Size(95, 23);
            deleteButton.TabIndex = 3;
            deleteButton.Text = "Delete File";
            deleteButton.UseVisualStyleBackColor = true;
            deleteButton.Click += DeleteButton_Click;
            // 
            // deleteAllButton
            // 
            deleteAllButton.FlatStyle = FlatStyle.Flat;
            deleteAllButton.Location = new Point(516, 466);
            deleteAllButton.Name = "deleteAllButton";
            deleteAllButton.Size = new Size(95, 23);
            deleteAllButton.TabIndex = 4;
            deleteAllButton.Text = "Delete All";
            deleteAllButton.UseVisualStyleBackColor = true;
            deleteAllButton.Click += DeleteAllButton_Click;
            // 
            // printButton
            // 
            printButton.FlatStyle = FlatStyle.Flat;
            printButton.Location = new Point(617, 466);
            printButton.Name = "printButton";
            printButton.Size = new Size(95, 23);
            printButton.TabIndex = 5;
            printButton.Text = "Print";
            printButton.UseVisualStyleBackColor = true;
            printButton.Click += PrintButton_Click;
            // 
            // createFolderButton
            // 
            createFolderButton.FlatStyle = FlatStyle.Flat;
            createFolderButton.Location = new Point(718, 466);
            createFolderButton.Name = "createFolderButton";
            createFolderButton.Size = new Size(95, 23);
            createFolderButton.TabIndex = 6;
            createFolderButton.Text = "Create Folder";
            createFolderButton.UseVisualStyleBackColor = true;
            createFolderButton.Click += CreateFolderButton_Click;
            // 
            // deleteFolderButton
            // 
            deleteFolderButton.FlatStyle = FlatStyle.Flat;
            deleteFolderButton.Location = new Point(819, 466);
            deleteFolderButton.Name = "deleteFolderButton";
            deleteFolderButton.Size = new Size(95, 23);
            deleteFolderButton.TabIndex = 7;
            deleteFolderButton.Text = "Delete Folder";
            deleteFolderButton.UseVisualStyleBackColor = true;
            deleteFolderButton.Click += DeleteFolderButton_Click;
            // 
            // selectPrintFilesPrinterButton
            // 
            selectPrintFilesPrinterButton.FlatStyle = FlatStyle.Flat;
            selectPrintFilesPrinterButton.Location = new Point(920, 466);
            selectPrintFilesPrinterButton.Name = "selectPrintFilesPrinterButton";
            selectPrintFilesPrinterButton.Size = new Size(120, 23);
            selectPrintFilesPrinterButton.TabIndex = 8;
            selectPrintFilesPrinterButton.Text = "Select Printer";
            selectPrintFilesPrinterButton.UseVisualStyleBackColor = true;
            selectPrintFilesPrinterButton.Click += SelectPrintFilesPrinterButton_Click;
            // 
            // makeRealButton
            // 
            makeRealButton.FlatStyle = FlatStyle.Flat;
            makeRealButton.Location = new Point(1046, 466);
            makeRealButton.Name = "makeRealButton";
            makeRealButton.Size = new Size(95, 23);
            makeRealButton.TabIndex = 9;
            makeRealButton.Text = "Make Real";
            makeRealButton.UseVisualStyleBackColor = true;
            makeRealButton.Click += MakeRealButton_Click;
            // 
            // tabPage2
            // 
            tabPage2.Controls.Add(loadLogButton);
            tabPage2.Controls.Add(clearLogButton);
            tabPage2.Controls.Add(logListBox);
            tabPage2.Location = new Point(4, 24);
            tabPage2.Name = "tabPage2";
            tabPage2.Padding = new Padding(3);
            tabPage2.Size = new Size(1192, 672);
            tabPage2.TabIndex = 1;
            tabPage2.Text = "Log";
            tabPage2.UseVisualStyleBackColor = true;
            // 
            // loadLogButton
            // 
            loadLogButton.FlatStyle = FlatStyle.Flat;
            loadLogButton.Location = new Point(8, 643);
            loadLogButton.Name = "loadLogButton";
            loadLogButton.Size = new Size(75, 23);
            loadLogButton.TabIndex = 2;
            loadLogButton.Text = "Load Log";
            loadLogButton.UseVisualStyleBackColor = true;
            loadLogButton.Click += LoadLogButton_Click;
            // 
            // clearLogButton
            // 
            clearLogButton.FlatStyle = FlatStyle.Flat;
            clearLogButton.Location = new Point(89, 643);
            clearLogButton.Name = "clearLogButton";
            clearLogButton.Size = new Size(75, 23);
            clearLogButton.TabIndex = 1;
            clearLogButton.Text = "Clear Log";
            clearLogButton.UseVisualStyleBackColor = true;
            clearLogButton.Click += ClearLogButton_Click;
            // 
            // logListBox
            // 
            logListBox.FormattingEnabled = true;
            logListBox.ItemHeight = 15;
            logListBox.Location = new Point(8, 6);
            logListBox.Name = "logListBox";
            logListBox.Size = new Size(1176, 619);
            logListBox.TabIndex = 0;
            // 
            // tabPage3
            // 
            tabPage3.Controls.Add(weeklyCalendarGridView);
            tabPage3.Controls.Add(scheduleFolderListView);
            tabPage3.Controls.Add(scheduleFileListView);
            tabPage3.Controls.Add(scheduleDateTimePicker);
            tabPage3.Controls.Add(scheduleSelectButton);
            tabPage3.Controls.Add(scheduleButton);
            tabPage3.Controls.Add(scheduledJobsListBox);
            tabPage3.Controls.Add(deleteScheduledJobButton);
            tabPage3.Controls.Add(selectPrinterButton);
            tabPage3.Controls.Add(refreshCalendarButton);
            tabPage3.Location = new Point(4, 24);
            tabPage3.Name = "tabPage3";
            tabPage3.Padding = new Padding(3);
            tabPage3.Size = new Size(1192, 672);
            tabPage3.TabIndex = 2;
            tabPage3.Text = "Schedule Print";
            tabPage3.UseVisualStyleBackColor = true;
            // 
            // weeklyCalendarGridView
            // 
            weeklyCalendarGridView.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            weeklyCalendarGridView.Location = new Point(214, 6);
            weeklyCalendarGridView.Name = "weeklyCalendarGridView";
            weeklyCalendarGridView.RowTemplate.Height = 40;
            weeklyCalendarGridView.Size = new Size(970, 560);
            weeklyCalendarGridView.TabIndex = 7;
            // 
            // scheduleFolderListView
            // 
            scheduleFolderListView.Location = new Point(8, 6);
            scheduleFolderListView.Name = "scheduleFolderListView";
            scheduleFolderListView.Size = new Size(200, 154);
            scheduleFolderListView.SmallImageList = imageList;
            scheduleFolderListView.TabIndex = 0;
            scheduleFolderListView.UseCompatibleStateImageBehavior = false;
            scheduleFolderListView.View = View.SmallIcon;
            scheduleFolderListView.SelectedIndexChanged += ScheduleFolderListView_SelectedIndexChanged;
            // 
            // scheduleFileListView
            // 
            scheduleFileListView.Location = new Point(8, 166);
            scheduleFileListView.Name = "scheduleFileListView";
            scheduleFileListView.Size = new Size(200, 154);
            scheduleFileListView.SmallImageList = imageList;
            scheduleFileListView.TabIndex = 1;
            scheduleFileListView.UseCompatibleStateImageBehavior = false;
            scheduleFileListView.View = View.SmallIcon;
            // 
            // scheduleDateTimePicker
            // 
            scheduleDateTimePicker.CustomFormat = "MM/dd/yyyy HH:mm";
            scheduleDateTimePicker.Format = DateTimePickerFormat.Custom;
            scheduleDateTimePicker.Location = new Point(8, 326);
            scheduleDateTimePicker.Name = "scheduleDateTimePicker";
            scheduleDateTimePicker.ShowCheckBox = true;
            scheduleDateTimePicker.Size = new Size(200, 23);
            scheduleDateTimePicker.TabIndex = 2;
            // 
            // scheduleSelectButton
            // 
            scheduleSelectButton.FlatStyle = FlatStyle.Flat;
            scheduleSelectButton.Location = new Point(214, 596);
            scheduleSelectButton.Name = "scheduleSelectButton";
            scheduleSelectButton.Size = new Size(95, 23);
            scheduleSelectButton.TabIndex = 3;
            scheduleSelectButton.Text = "Select Files";
            scheduleSelectButton.UseVisualStyleBackColor = true;
            scheduleSelectButton.Click += ScheduleSelectButton_Click;
            // 
            // scheduleButton
            // 
            scheduleButton.FlatStyle = FlatStyle.Flat;
            scheduleButton.Location = new Point(315, 596);
            scheduleButton.Name = "scheduleButton";
            scheduleButton.Size = new Size(95, 23);
            scheduleButton.TabIndex = 4;
            scheduleButton.Text = "Schedule Print";
            scheduleButton.UseVisualStyleBackColor = true;
            scheduleButton.Click += ScheduleButton_Click;
            // 
            // scheduledJobsListBox
            // 
            scheduledJobsListBox.FormattingEnabled = true;
            scheduledJobsListBox.ItemHeight = 15;
            scheduledJobsListBox.Location = new Point(416, 596);
            scheduledJobsListBox.Name = "scheduledJobsListBox";
            scheduledJobsListBox.Size = new Size(770, 64);
            scheduledJobsListBox.TabIndex = 5;
            scheduledJobsListBox.DrawMode = DrawMode.OwnerDrawVariable;
            scheduledJobsListBox.MeasureItem += new MeasureItemEventHandler(scheduledJobsListBox_MeasureItem);
            scheduledJobsListBox.DrawItem += new DrawItemEventHandler(scheduledJobsListBox_DrawItem);
            // 
            // deleteScheduledJobButton
            // 
            deleteScheduledJobButton.FlatStyle = FlatStyle.Flat;
            deleteScheduledJobButton.Location = new Point(8, 360);
            deleteScheduledJobButton.Name = "deleteScheduledJobButton";
            deleteScheduledJobButton.Size = new Size(200, 23);
            deleteScheduledJobButton.TabIndex = 8;
            deleteScheduledJobButton.Text = "Delete Scheduled Job";
            deleteScheduledJobButton.UseVisualStyleBackColor = true;
            deleteScheduledJobButton.Click += DeleteScheduledJobButton_Click;
            // 
            // selectPrinterButton
            // 
            selectPrinterButton.FlatStyle = FlatStyle.Flat;
            selectPrinterButton.Location = new Point(214, 625);
            selectPrinterButton.Name = "selectPrinterButton";
            selectPrinterButton.Size = new Size(95, 23);
            selectPrinterButton.TabIndex = 6;
            selectPrinterButton.Text = "Select Printer";
            selectPrinterButton.UseVisualStyleBackColor = true;
            selectPrinterButton.Click += SelectPrinterButton_Click;
            // 
            // refreshCalendarButton
            // 
            refreshCalendarButton.FlatStyle = FlatStyle.Flat;
            refreshCalendarButton.Location = new Point(315, 625);
            refreshCalendarButton.Name = "refreshCalendarButton";
            refreshCalendarButton.Size = new Size(95, 23);
            refreshCalendarButton.TabIndex = 9;
            refreshCalendarButton.Text = "Refresh Calendar";
            refreshCalendarButton.UseVisualStyleBackColor = true;
            refreshCalendarButton.Click += RefreshCalendarButton_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1200, 700);
            Controls.Add(tabControl);
            Name = "Form1";
            Text = "File Printer App";
            Icon = new Icon("appicon.ico"); // Set the icon for the form
            tabControl.ResumeLayout(false);
            tabPage1.ResumeLayout(false);
            tabPage2.ResumeLayout(false);
            tabPage3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)weeklyCalendarGridView).EndInit();
            ResumeLayout(false);
        }
    }
}