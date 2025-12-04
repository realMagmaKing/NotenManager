using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

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
}
