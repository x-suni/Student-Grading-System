using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Text.Json;

namespace StudentGradingSystem
{
    public partial class MainForm : Form
    {
        private List<Student> students = new();
        private readonly DatabaseManager dbManager;
        private DataGridView studentsGrid = null!;
        private GroupBox inputGroup = null!;
        private GroupBox subjectsGroup = null!;
        private GroupBox resultsGroup = null!;
        private TextBox nameTextBox = null!;
        private TextBox searchTextBox = null!;
        private Button searchButton = null!;
        private Button addButton = null!;
        private Button clearButton = null!;
        private Button calculateAllButton = null!;
        private Button addSubjectButton = null!;
        private Button removeSubjectButton = null!;
        private Label avgLabel = null!;
        private Label topStudentLabel = null!;
        private CheckedListBox subjectsCheckedList = null!;
        private FlowLayoutPanel subjectInputsPanel = null!;
        private readonly Dictionary<string, NumericUpDown> subjectInputs = new();

        public MainForm()
        {
            InitializeComponent();
            dbManager = new DatabaseManager();
            students = new List<Student>();
            subjectInputs = new Dictionary<string, NumericUpDown>();
            LoadStudentsFromDatabase();
        }

        private void InitializeComponent()
        {
            this.Size = new Size(1200, 800);
            this.Text = "Student Grading System - Multi-Subject";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 248, 255);

            CreateInputSection();
            CreateSubjectsSection();
            CreateResultsSection();
            CreateDataGrid();
        }

        private void CreateInputSection()
        {
            inputGroup = new GroupBox
            {
                Text = "Student Information",
                Location = new Point(20, 20),
                Size = new Size(300, 180),  // Increased height for search controls
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            // Search controls
            var searchLabel = new Label
            {
                Text = "Search Students:",
                Location = new Point(15, 30),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9)
            };

            searchTextBox = new TextBox
            {
                Location = new Point(15, 55),
                Size = new Size(180, 25),
                Font = new Font("Segoe UI", 9),
                PlaceholderText = "Enter student name..."
            };
            searchTextBox.TextChanged += SearchTextBox_TextChanged;

            searchButton = new Button
            {
                Text = "Search",
                Location = new Point(205, 54),
                Size = new Size(80, 27),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            searchButton.Click += SearchButton_Click;

            // Original controls with adjusted positions
            var nameLabel = new Label
            {
                Text = "Student Name:",
                Location = new Point(15, 90),
                Size = new Size(100, 25),
                Font = new Font("Segoe UI", 9)
            };
            nameTextBox = new TextBox
            {
                Location = new Point(15, 115),
                Size = new Size(270, 25),
                Font = new Font("Segoe UI", 9)
            };

            addButton = new Button
            {
                Text = "Add Student",
                Location = new Point(15, 145),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(0, 123, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            addButton.Click += AddStudent_Click;

            clearButton = new Button
            {
                Text = "Clear All",
                Location = new Point(125, 145),
                Size = new Size(80, 30),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            clearButton.Click += ClearAll_Click;

            inputGroup.Controls.AddRange(new Control[] { 
                searchLabel, searchTextBox, searchButton,
                nameLabel, nameTextBox, addButton, clearButton 
            });
            this.Controls.Add(inputGroup);
        }

        private void CreateSubjectsSection()
        {
            subjectsGroup = new GroupBox
            {
                Text = "Subject Selection & Marks",
                Location = new Point(340, 20),
                Size = new Size(520, 350),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            var selectLabel = new Label
            {
                Text = "Select Subjects:",
                Location = new Point(15, 25),
                Size = new Size(100, 20),
                Font = new Font("Segoe UI", 9)
            };

            subjectsCheckedList = new CheckedListBox
            {
                Location = new Point(15, 50),
                Size = new Size(200, 250),
                Font = new Font("Segoe UI", 8),
                CheckOnClick = true
            };

            // Group subjects by category for better organization
            var groupedSubjects = SubjectsConfig.AllSubjects
                .GroupBy(s => SubjectsConfig.SubjectCategories.ContainsKey(s) ? SubjectsConfig.SubjectCategories[s] : "Other")
                .OrderBy(g => g.Key);

            foreach (var group in groupedSubjects)
            {
                subjectsCheckedList.Items.Add($"--- {group.Key} ---", false);
                foreach (var subject in group.OrderBy(s => s))
                {
                    subjectsCheckedList.Items.Add($"  {subject}", false);
                }
            }

            subjectsCheckedList.ItemCheck += SubjectsCheckedList_ItemCheck;

            var marksLabel = new Label
            {
                Text = "Enter Marks (0-100):",
                Location = new Point(230, 25),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 9)
            };

            subjectInputsPanel = new FlowLayoutPanel
            {
                Location = new Point(230, 50),
                Size = new Size(270, 250),
                AutoScroll = true,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                BackColor = Color.FromArgb(248, 249, 250)
            };

            addSubjectButton = new Button
            {
                Text = "Select Subjects →",
                Location = new Point(15, 310),
                Size = new Size(120, 30),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };

            removeSubjectButton = new Button
            {
                Text = "Clear Selection",
                Location = new Point(145, 310),
                Size = new Size(100, 30),
                BackColor = Color.FromArgb(255, 193, 7),
                ForeColor = Color.Black,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8, FontStyle.Bold)
            };
            removeSubjectButton.Click += RemoveSubjects_Click;

            subjectsGroup.Controls.AddRange(new Control[] { 
                selectLabel, subjectsCheckedList, marksLabel, subjectInputsPanel, 
                addSubjectButton, removeSubjectButton 
            });
            this.Controls.Add(subjectsGroup);
        }

        private void CreateResultsSection()
        {
            resultsGroup = new GroupBox
            {
                Text = "Statistics & Analysis",
                Location = new Point(880, 20),
                Size = new Size(300, 350),
                BackColor = Color.White,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };

            avgLabel = new Label
            {
                Text = "Class Average: N/A",
                Location = new Point(15, 35),
                Size = new Size(270, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(40, 167, 69)
            };

            topStudentLabel = new Label
            {
                Text = "Top Student: N/A",
                Location = new Point(15, 65),
                Size = new Size(270, 25),
                Font = new Font("Segoe UI", 10),
                ForeColor = Color.FromArgb(255, 193, 7)
            };

            calculateAllButton = new Button
            {
                Text = "Calculate Statistics",
                Location = new Point(80, 100),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(40, 167, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            calculateAllButton.Click += CalculateStatistics_Click;

            var exportButton = new Button
            {
                Text = "Export Report",
                Location = new Point(80, 150),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(108, 117, 125),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            exportButton.Click += ExportReport_Click;

            var subjectStatsButton = new Button
            {
                Text = "Subject Analysis",
                Location = new Point(80, 195),
                Size = new Size(140, 35),
                BackColor = Color.FromArgb(23, 162, 184),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            subjectStatsButton.Click += ShowSubjectAnalysis_Click;

            resultsGroup.Controls.AddRange(new Control[] { 
                avgLabel, topStudentLabel, calculateAllButton, exportButton, subjectStatsButton 
            });
            this.Controls.Add(resultsGroup);
        }

        private void CreateDataGrid()
        {
            studentsGrid = new DataGridView
            {
                Location = new Point(20, 390),
                Size = new Size(1160, 380),
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.Fixed3D,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                Font = new Font("Segoe UI", 9),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };

            studentsGrid.Columns.Add("Name", "Student Name");
            studentsGrid.Columns.Add("Subjects", "Subjects Taken");
            studentsGrid.Columns.Add("Average", "Average %");
            studentsGrid.Columns.Add("Grade", "Grade");
            studentsGrid.Columns.Add("DateAdded", "Date Added");

            // Style the grid
            studentsGrid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(0, 123, 255);
            studentsGrid.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            studentsGrid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            studentsGrid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(248, 249, 250);

            // Add context menu for detailed view and deletion
            var contextMenu = new ContextMenuStrip();
            var viewDetailsItem = new ToolStripMenuItem("View Details");
            var deleteItem = new ToolStripMenuItem("Delete Student");
            viewDetailsItem.Click += ViewDetails_Click;
            deleteItem.Click += DeleteStudent_Click;
            contextMenu.Items.AddRange(new ToolStripItem[] { viewDetailsItem, deleteItem });
            studentsGrid.ContextMenuStrip = contextMenu;

            this.Controls.Add(studentsGrid);
        }

        private void SubjectsCheckedList_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            // Prevent checking category headers
            string item = subjectsCheckedList.Items[e.Index].ToString();
            if (item.StartsWith("---"))
            {
                e.NewValue = CheckState.Unchecked;
                return;
            }

            // Use timer to delay execution until after the check state changes
            var timer = new System.Windows.Forms.Timer { Interval = 1 };
            timer.Tick += (s, args) =>
            {
                timer.Stop();
                UpdateSubjectInputs();
            };
            timer.Start();
        }

        private void UpdateSubjectInputs()
        {
            subjectInputsPanel.Controls.Clear();
            subjectInputs.Clear();

            foreach (int index in subjectsCheckedList.CheckedIndices)
            {
                string item = subjectsCheckedList.Items[index].ToString();
                if (item.StartsWith("---")) continue;

                string subject = item.Trim();
                CreateSubjectInput(subject);
            }
        }

        private void CreateSubjectInput(string subject)
        {
            var panel = new Panel
            {
                Size = new Size(250, 35),
                Margin = new Padding(5)
            };

            var label = new Label
            {
                Text = subject + ":",
                Location = new Point(5, 8),
                Size = new Size(150, 20),
                Font = new Font("Segoe UI", 8)
            };

            var numericUpDown = new NumericUpDown
            {
                Location = new Point(160, 5),
                Size = new Size(80, 25),
                Maximum = 100,
                Font = new Font("Segoe UI", 9)
            };

            panel.Controls.AddRange(new Control[] { label, numericUpDown });
            subjectInputsPanel.Controls.Add(panel);
            subjectInputs[subject] = numericUpDown;
        }

        private void AddStudent_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(nameTextBox.Text))
            {
                MessageBox.Show("Please enter a student name.", "Input Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (subjectInputs.Count == 0)
            {
                MessageBox.Show("Please select at least one subject and enter marks.", "Subjects Required", 
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var student = new Student
            {
                Name = nameTextBox.Text.Trim()
            };

            // Add subject marks
            foreach (var kvp in subjectInputs)
            {
                student.AddSubjectMark(kvp.Key, (double)kvp.Value.Value);
            }

            student.CalculateGrade();
            
            // Save to database
            if (dbManager.AddStudent(student))
            {
                LoadStudentsFromDatabase(); // Reload to get updated data
                ClearInputs();

                MessageBox.Show($"Student {student.Name} added successfully!\n" +
                              $"Subjects: {student.SubjectMarks.Count}\n" +
                              $"Average: {student.Average:F2}%\n" +
                              $"Grade: {student.Grade}\n" +
                              $"Saved to database!", 
                              "Student Added", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Failed to save student to database.", "Database Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearAll_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to clear all students?\nThis will permanently delete all data from the database!", 
                                       "Confirm Clear", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                if (dbManager.ClearAllStudents())
                {
                    students.Clear();
                    UpdateGrid();
                    avgLabel.Text = "Class Average: N/A";
                    topStudentLabel.Text = "Top Student: N/A";
                    this.Text = "Student Grading System - Multi-Subject";
                    MessageBox.Show("All student data cleared from database.", "Data Cleared", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Failed to clear data from database.", "Database Error", 
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CalculateStatistics_Click(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                MessageBox.Show("No students to calculate statistics for.", "No Data", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            double classAverage = students.Average(s => s.Average);
            var topStudent = students.OrderByDescending(s => s.Average).First();

            avgLabel.Text = $"Class Average: {classAverage:F2}%";
            topStudentLabel.Text = $"Top Student: {topStudent.Name} ({topStudent.Average:F2}%)";
        }

        private void RemoveSubjects_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < subjectsCheckedList.Items.Count; i++)
            {
                subjectsCheckedList.SetItemChecked(i, false);
            }
            UpdateSubjectInputs();
        }

        private void ViewDetails_Click(object sender, EventArgs e)
        {
            if (studentsGrid.SelectedRows.Count > 0)
            {
                var selectedIndex = studentsGrid.SelectedRows[0].Index;
                if (selectedIndex < students.Count)
                {
                    var student = students[selectedIndex];
                    ShowStudentDetails(student);
                }
            }
        }

        private void ShowStudentDetails(Student student)
        {
            var details = $"Student: {student.Name}\n";
            details += $"Date Added: {student.DateAdded:yyyy-MM-dd HH:mm}\n";
            details += $"Number of Subjects: {student.SubjectMarks.Count}\n\n";
            details += "Subject Breakdown:\n";
            details += new string('-', 30) + "\n";

            foreach (var subject in student.SubjectMarks.OrderBy(kvp => kvp.Key))
            {
                details += $"{subject.Key}: {subject.Value}%\n";
            }

            details += new string('-', 30) + "\n";
            details += $"Overall Average: {student.Average:F2}%\n";
            details += $"Grade: {student.Grade}\n";

            MessageBox.Show(details, "Student Details", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void DeleteStudent_Click(object sender, EventArgs e)
        {
            if (studentsGrid.SelectedRows.Count > 0)
            {
                var selectedIndex = studentsGrid.SelectedRows[0].Index;
                if (selectedIndex < students.Count)
                {
                    var student = students[selectedIndex];
                    var result = MessageBox.Show($"Are you sure you want to delete {student.Name}?", 
                                               "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    
                    if (result == DialogResult.Yes)
                    {
                        if (dbManager.DeleteStudent(student.Id))
                        {
                            LoadStudentsFromDatabase();
                            MessageBox.Show($"{student.Name} deleted successfully.", "Student Deleted", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete student from database.", "Database Error", 
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
        }

        private void ExportReport_Click(object sender, EventArgs e)
        {
            if (students.Count == 0)
            {
                MessageBox.Show("No data to export.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            try
            {
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                    DefaultExt = "txt",
                    FileName = $"StudentReport_{DateTime.Now:yyyyMMdd_HHmm}.txt"
                };

                if (saveDialog.ShowDialog() == DialogResult.OK)
                {
                    var report = GenerateReport();
                    System.IO.File.WriteAllText(saveDialog.FileName, report);
                    MessageBox.Show($"Report exported successfully to:\n{saveDialog.FileName}", 
                                  "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", 
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GenerateReport()
        {
            var report = "STUDENT GRADING SYSTEM REPORT\n";
            report += $"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n";
            report += new string('=', 50) + "\n\n";

            report += $"Total Students: {students.Count}\n";
            if (students.Count > 0)
            {
                report += $"Class Average: {students.Average(s => s.Average):F2}%\n";
                var topStudent = students.OrderByDescending(s => s.Average).First();
                report += $"Top Student: {topStudent.Name} ({topStudent.Average:F2}%)\n";
            }
            report += "\n";

            // Subject statistics
            var subjectAverages = dbManager.GetSubjectAverages();
            if (subjectAverages.Any())
            {
                report += "SUBJECT AVERAGES:\n";
                report += new string('-', 30) + "\n";
                foreach (var subject in subjectAverages.OrderByDescending(kvp => kvp.Value))
                {
                    report += $"{subject.Key}: {subject.Value:F2}%\n";
                }
                report += "\n";
            }

            // Individual student details
            report += "INDIVIDUAL STUDENT RECORDS:\n";
            report += new string('-', 50) + "\n";

            foreach (var student in students.OrderByDescending(s => s.Average))
            {
                report += $"\nStudent: {student.Name}\n";
                report += $"Grade: {student.Grade} (Average: {student.Average:F2}%)\n";
                report += $"Subjects ({student.SubjectMarks.Count}):\n";
                
                foreach (var subject in student.SubjectMarks.OrderBy(kvp => kvp.Key))
                {
                    report += $"  {subject.Key}: {subject.Value}%\n";
                }
                report += $"Date Added: {student.DateAdded:yyyy-MM-dd HH:mm}\n";
            }

            return report;
        }

        private void ShowSubjectAnalysis_Click(object sender, EventArgs e)
        {
            var subjectAverages = dbManager.GetSubjectAverages();
            if (!subjectAverages.Any())
            {
                MessageBox.Show("No subject data available for analysis.", "No Data", 
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var analysis = "SUBJECT PERFORMANCE ANALYSIS\n";
            analysis += new string('=', 40) + "\n\n";

            var sortedSubjects = subjectAverages.OrderByDescending(kvp => kvp.Value);
            
            analysis += "SUBJECT RANKINGS (by average performance):\n";
            analysis += new string('-', 40) + "\n";
            
            int rank = 1;
            foreach (var subject in sortedSubjects)
            {
                analysis += $"{rank:D2}. {subject.Key}: {subject.Value:F2}%\n";
                rank++;
            }

            analysis += "\nPERFORMANCE CATEGORIES:\n";
            analysis += new string('-', 25) + "\n";

            var excellent = subjectAverages.Where(kvp => kvp.Value >= 80).Count();
            var good = subjectAverages.Where(kvp => kvp.Value >= 70 && kvp.Value < 80).Count();
            var average = subjectAverages.Where(kvp => kvp.Value >= 60 && kvp.Value < 70).Count();
            var needsWork = subjectAverages.Where(kvp => kvp.Value < 60).Count();

            analysis += $"Excellent (80%+): {excellent} subjects\n";
            analysis += $"Good (70-79%): {good} subjects\n";
            analysis += $"Average (60-69%): {average} subjects\n";
            analysis += $"Needs Improvement (<60%): {needsWork} subjects\n";

            MessageBox.Show(analysis, "Subject Analysis", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ClearInputs()
        {
            nameTextBox.Clear();
            foreach (var input in subjectInputs.Values)
            {
                input.Value = 0;
            }
            nameTextBox.Focus();
        }

        private void UpdateGrid()
        {
            studentsGrid.Rows.Clear();
            foreach (var student in students)
            {
                var subjectsList = string.Join(", ", student.GetTakenSubjects().Take(3));
                if (student.SubjectMarks.Count > 3)
                    subjectsList += $" (+{student.SubjectMarks.Count - 3} more)";

                studentsGrid.Rows.Add(
                    student.Name, 
                    subjectsList,
                    $"{student.Average:F2}%", 
                    student.Grade, 
                    student.DateAdded.ToString("yyyy-MM-dd HH:mm")
                );
            }
        }

        private void LoadStudentsFromDatabase()
        {
            try
            {
                students = dbManager.GetAllStudents();
                UpdateGrid();
                
                int studentCount = dbManager.GetStudentCount();
                this.Text = $"Student Grading System - Multi-Subject ({studentCount} students)";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading students from database: {ex.Message}", 
                              "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SearchTextBox_TextChanged(object? sender, EventArgs e)
        {
            // Search as user types
            FilterStudents(searchTextBox.Text);
        }

        private void SearchButton_Click(object? sender, EventArgs e)
        {
            FilterStudents(searchTextBox.Text);
        }

        private void FilterStudents(string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                // If search is empty, show all students
                LoadStudentsFromDatabase();
                return;
            }

            // Filter the grid based on the search text
            studentsGrid.Rows.Clear();
            var filteredStudents = students.Where(s => s.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();
            
            foreach (var student in filteredStudents)
            {
                var subjectsList = string.Join(", ", student.GetTakenSubjects().Take(3));
                if (student.SubjectMarks.Count > 3)
                    subjectsList += $" (+{student.SubjectMarks.Count - 3} more)";

                studentsGrid.Rows.Add(
                    student.Name,
                    subjectsList,
                    $"{student.Average:F2}%",
                    student.Grade,
                    student.DateAdded.ToString("yyyy-MM-dd HH:mm")
                );
            }

            // Update window title with filtered count
            this.Text = $"Student Grading System - Multi-Subject ({filteredStudents.Count} of {students.Count} students)";
        }
    }

    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, double> SubjectMarks { get; set; }
        public double Average { get; private set; }
        public string Grade { get; private set; } = "N/A";
        public DateTime DateAdded { get; set; } = DateTime.Now;

        public Student()
        {
            SubjectMarks = new Dictionary<string, double>();
        }
        
        public void CalculateGrade()
        {
            if (SubjectMarks.Count == 0)
            {
                Average = 0;
                Grade = "N/A";
                return;
            }

            Average = SubjectMarks.Values.Average();

            if (Average >= 90)
                Grade = "A+";
            else if (Average >= 80)
                Grade = "A";
            else if (Average >= 70)
                Grade = "B";
            else if (Average >= 60)
                Grade = "C";
            else if (Average >= 50)
                Grade = "D";
            else
                Grade = "F";
        }

        public void AddSubjectMark(string subject, double mark)
        {
            SubjectMarks[subject] = mark;
        }

        public double GetSubjectMark(string subject)
        {
            return SubjectMarks.ContainsKey(subject) ? SubjectMarks[subject] : 0;
        }

        public List<string> GetTakenSubjects()
        {
            return SubjectMarks.Keys.ToList();
        }
    }
}