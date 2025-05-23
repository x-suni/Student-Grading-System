using System.Collections.Generic;

namespace StudentGradingSystem
{
    public static class SubjectsConfig
    {
        public static readonly HashSet<string> AllSubjects = new HashSet<string>
        {
            // Core Subjects
            "Mathematics",
            "English Language",
            "Kiswahili",
            
            // Sciences
            "Science",
            "Biology",
            "Chemistry",
            "Physics",
            
            // Humanities
            "History",
            "Geography",
            "Religious Education",
            
            // Languages
            "French",
            "German",
            "Spanish",
            "Arabic",
            "Chinese",
            "Latin",
            
            // Arts & Culture
            "Literature",
            "Music",
            "Drama",
            "Fine Art",
            "Dance",
            
            // Social Sciences & Life Skills
            "Social Studies",
            "Civic Education",
            "Life Skills",
            "Home Science",
            "Personal Development",
            
            // Technical & Business
            "Business Studies",
            "Accounting",
            "Agriculture",
            "Woodwork",
            "Metalwork",
            "Building & Construction",
            "Electrical Technology",
            "Computer Studies",
            
            // Physical Education & Health
            "Physical Education",
            "Health Education",
            "Nutrition",
            "Sports Science"
        };

        public static readonly Dictionary<string, string> SubjectCategories = new Dictionary<string, string>
        {
            // Core Subjects
            {"Mathematics", "Core"},
            {"English Language", "Core"},
            {"Kiswahili", "Core"},
            
            // Sciences
            {"Science", "Sciences"},
            {"Biology", "Sciences"},
            {"Chemistry", "Sciences"},
            {"Physics", "Sciences"},
            
            // Humanities
            {"History", "Humanities"},
            {"Geography", "Humanities"},
            {"Religious Education", "Humanities"},
            
            // Languages
            {"French", "Languages"},
            {"German", "Languages"},
            {"Spanish", "Languages"},
            {"Arabic", "Languages"},
            {"Chinese", "Languages"},
            {"Latin", "Languages"},
            
            // Arts & Culture
            {"Literature", "Arts & Culture"},
            {"Music", "Arts & Culture"},
            {"Drama", "Arts & Culture"},
            {"Fine Art", "Arts & Culture"},
            {"Dance", "Arts & Culture"},
            
            // Social Sciences & Life Skills
            {"Social Studies", "Social Sciences"},
            {"Civic Education", "Social Sciences"},
            {"Life Skills", "Life Skills"},
            {"Home Science", "Life Skills"},
            {"Personal Development", "Life Skills"},
            
            // Technical & Business
            {"Business Studies", "Business"},
            {"Accounting", "Business"},
            {"Agriculture", "Technical"},
            {"Woodwork", "Technical"},
            {"Metalwork", "Technical"},
            {"Building & Construction", "Technical"},
            {"Electrical Technology", "Technical"},
            {"Computer Studies", "Technical"},
            
            // Physical Education & Health
            {"Physical Education", "PE & Health"},
            {"Health Education", "PE & Health"},
            {"Nutrition", "PE & Health"},
            {"Sports Science", "PE & Health"}
        };
    }
}
