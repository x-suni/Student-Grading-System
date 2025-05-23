# Student Grading System

A Windows Forms application built with .NET 6.0 for managing student grades and subjects.

## Features

- Student management (add, edit, delete students)
- Grade tracking system
- Subject configuration
- SQLite database for data persistence

## Prerequisites

- .NET 6.0 SDK or later
- Windows Operating System

## How to Run

1. Open the solution in Visual Studio or your preferred IDE
2. Build the solution using `dotnet build`
3. Run the application using `dotnet run`

## Database

The application uses SQLite as its database engine. The database file (`students.db`) is automatically created in the application's output directory when the program runs for the first time.

## Project Structure

- `MainForm.cs` - The main Windows Forms interface
- `DatabaseManager.cs` - Handles all database operations
- `SubjectsConfig.cs` - Manages subject configuration
- `Program.cs` - Application entry point

## Technologies Used

- .NET 6.0
- Windows Forms
- SQLite
- Microsoft.Data.Sqlite