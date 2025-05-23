using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace StudentGradingSystem
{
    public class DatabaseManager
    {
        private readonly string connectionString;
        private readonly string dbPath;        public DatabaseManager()
        {
            // Create database in the same folder as the executable
            dbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "students.db");
            connectionString = $"Data Source={dbPath}";
            InitializeDatabase(); // Only initialize the database if it doesn't exist
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var createTableCommand = connection.CreateCommand();
            createTableCommand.CommandText = @"
                CREATE TABLE IF NOT EXISTS Students (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    SubjectMarksJson TEXT NOT NULL,
                    Average REAL NOT NULL,
                    Grade TEXT NOT NULL,
                    DateAdded TEXT NOT NULL
                )";
            createTableCommand.ExecuteNonQuery();
        }

        private void RecreateDatabase()
        {
            try
            {
                if (File.Exists(dbPath))
                {
                    File.Delete(dbPath);
                }
                
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var createTableCommand = connection.CreateCommand();
                createTableCommand.CommandText = @"
                    CREATE TABLE IF NOT EXISTS Students (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        SubjectMarksJson TEXT NOT NULL,
                        Average REAL NOT NULL,
                        Grade TEXT NOT NULL,
                        DateAdded TEXT NOT NULL
                    )";
                createTableCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error recreating database: {ex.Message}");
                throw;
            }
        }

        public bool AddStudent(Student student)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    INSERT INTO Students (Name, SubjectMarksJson, Average, Grade, DateAdded)
                    VALUES ($name, $subjectMarks, $average, $grade, $dateAdded)";

                string subjectMarksJson = JsonSerializer.Serialize(student.SubjectMarks);

                command.Parameters.AddWithValue("$name", student.Name);
                command.Parameters.AddWithValue("$subjectMarks", subjectMarksJson);
                command.Parameters.AddWithValue("$average", student.Average);
                command.Parameters.AddWithValue("$grade", student.Grade);
                command.Parameters.AddWithValue("$dateAdded", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                var result = command.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding student: {ex.Message}");
                return false;
            }
        }

        public List<Student> GetAllStudents()
        {
            var students = new List<Student>();

            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Students ORDER BY DateAdded DESC";

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    var student = new Student
                    {                        Id = reader.GetInt32(reader.GetOrdinal("Id")),
                        Name = reader.GetString(reader.GetOrdinal("Name")),
                        DateAdded = DateTime.Parse(reader.GetString(reader.GetOrdinal("DateAdded")))
                    };

                    // Deserialize subject marks
                    string subjectMarksJson = reader.GetString(reader.GetOrdinal("SubjectMarksJson"));
                    student.SubjectMarks = JsonSerializer.Deserialize<Dictionary<string, double>>(subjectMarksJson) ?? new Dictionary<string, double>();
                    
                    // Recalculate grade (in case grading system changed)
                    student.CalculateGrade();
                    students.Add(student);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving students: {ex.Message}");
            }

            return students;
        }

        public bool DeleteStudent(int studentId)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Students WHERE Id = $id";
                command.Parameters.AddWithValue("$id", studentId);

                var result = command.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting student: {ex.Message}");
                return false;
            }
        }

        public bool UpdateStudent(Student student)
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"
                    UPDATE Students 
                    SET Name = $name, SubjectMarksJson = $subjectMarks, 
                        Average = $average, Grade = $grade
                    WHERE Id = $id";

                string subjectMarksJson = JsonSerializer.Serialize(student.SubjectMarks);

                command.Parameters.AddWithValue("$name", student.Name);
                command.Parameters.AddWithValue("$subjectMarks", subjectMarksJson);
                command.Parameters.AddWithValue("$average", student.Average);
                command.Parameters.AddWithValue("$grade", student.Grade);
                command.Parameters.AddWithValue("$id", student.Id);

                var result = command.ExecuteNonQuery();
                return result > 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating student: {ex.Message}");
                return false;
            }
        }

        public bool ClearAllStudents()
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM Students";
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing students: {ex.Message}");
                return false;
            }
        }

        public string GetDatabasePath()
        {
            return dbPath;
        }

        public int GetStudentCount()
        {
            try
            {
                using var connection = new SqliteConnection(connectionString);
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = "SELECT COUNT(*) FROM Students";
                return Convert.ToInt32(command.ExecuteScalar());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting student count: {ex.Message}");
                return 0;
            }
        }

        public Dictionary<string, double> GetSubjectAverages()
        {
            var subjectAverages = new Dictionary<string, double>();

            try
            {
                var students = GetAllStudents();
                var allSubjects = students.SelectMany(s => s.SubjectMarks.Keys).Distinct();

                foreach (var subject in allSubjects)
                {
                    var marksForSubject = students
                        .Where(s => s.SubjectMarks.ContainsKey(subject))
                        .Select(s => s.SubjectMarks[subject])
                        .ToList();

                    if (marksForSubject.Any())
                    {
                        subjectAverages[subject] = marksForSubject.Average();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calculating subject averages: {ex.Message}");
            }

            return subjectAverages;
        }
    }
}