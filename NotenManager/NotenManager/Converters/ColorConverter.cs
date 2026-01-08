using System.Globalization;

namespace NotenManager.Converters
{
    public class SubjectColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = value as string;
            return color switch
            {
                "math" => Color.FromArgb("#f5576c"),
                "bio" => Color.FromArgb("#00f2fe"),
                "info" => Color.FromArgb("#38f9d7"),
                "deutsch" => Color.FromArgb("#fee140"),
                _ => Color.FromArgb("#764ba2")
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class PageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentPage = value as string;
            var targetPage = parameter as string;
            return currentPage == targetPage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class InversePageVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var currentPage = value as string;
            var targetPage = parameter as string;
            return currentPage != targetPage;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? 1.0 : 0.25;
            }
            return 0.25;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class IsNotNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter for RadioButton binding to string property
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string currentValue && parameter is string targetValue)
            {
                return currentValue == targetValue;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isChecked && isChecked && parameter is string targetValue)
            {
                return targetValue;
            }
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// Converter that takes a Note (model) and returns a display string: DisplayGrade if set, otherwise numeric grade formatted
    /// </summary>
    public class NoteGradeDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return string.Empty;

            // value is expected to be a Note
            var note = value as NotenManager.Models.Note;
            if (note == null) return string.Empty;

            // If DisplayGrade is set (for USA system), show it
            if (!string.IsNullOrEmpty(note.DisplayGrade))
                return note.DisplayGrade;

            // Otherwise show numeric grade
            // For values between 0-6, show one decimal
            if (note.Grade <= 10)
                return note.Grade.ToString("F1", CultureInfo.InvariantCulture);

            // For percentages, show no decimals
            if (note.Grade > 10)
                return note.Grade.ToString("F0", CultureInfo.InvariantCulture) + "%";

            return note.Grade.ToString("F1", CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converter specifically for displaying grades with proper formatting based on scale
    /// </summary>
    public class GradeValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double grade)
            {
                // If grade is in percentage range (0-100), show without decimal
                if (grade > 10)
                    return grade.ToString("F0") + "%";

                // Otherwise show one decimal
                return grade.ToString("F1");
            }
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string str)
            {
                str = str.Replace("%", "").Trim();
                if (double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out double result))
                    return result;
            }
            return 0.0;
        }
    }
}