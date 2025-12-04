using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using NotenManager.Models;
using NotenManager.Services;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace NotenManager.ViewModels
{
 public partial class MainViewModel : ObservableObject
 {
 private readonly DataService _dataService;

 private string _currentPage = "Overview";
 public string CurrentPage { get => _currentPage; set => SetProperty(ref _currentPage, value); }

 private Subject _selectedSubject;
 public Subject SelectedSubject { get => _selectedSubject; set => SetProperty(ref _selectedSubject, value); }

 private ObservableCollection<Subject> _subjects;
 public ObservableCollection<Subject> Subjects { get => _subjects; set => SetProperty(ref _subjects, value); }

 private double _overallAverage;
 public double OverallAverage { get => _overallAverage; set => SetProperty(ref _overallAverage, value); }

 private bool _isAddSubjectModalVisible;
 public bool IsAddSubjectModalVisible { get => _isAddSubjectModalVisible; set => SetProperty(ref _isAddSubjectModalVisible, value); }

 private bool _isAddNoteModalVisible;
 public bool IsAddNoteModalVisible { get => _isAddNoteModalVisible; set => SetProperty(ref _isAddNoteModalVisible, value); }

 private bool _isEditNoteModalVisible;
 public bool IsEditNoteModalVisible { get => _isEditNoteModalVisible; set => SetProperty(ref _isEditNoteModalVisible, value); }

 private string _newSubjectName = string.Empty;
 public string NewSubjectName { get => _newSubjectName; set => SetProperty(ref _newSubjectName, value); }

 private int _newSubjectLessons =3;
 public int NewSubjectLessons { get => _newSubjectLessons; set => SetProperty(ref _newSubjectLessons, value); }

 private string _newNoteType = string.Empty;
 public string NewNoteType { get => _newNoteType; set => SetProperty(ref _newNoteType, value); }

 private string _newNoteGrade = string.Empty;
 public string NewNoteGrade { get => _newNoteGrade; set => SetProperty(ref _newNoteGrade, value); }

 private DateTime _newNoteDate = DateTime.Now;
 public DateTime NewNoteDate { get => _newNoteDate; set => SetProperty(ref _newNoteDate, value); }

 private Note _editingNote;

 public MainViewModel()
 {
 _dataService = new DataService();
 Subjects = _dataService.GetSubjects();
 UpdateOverallAverage();
 }

 [RelayCommand]
 private void NavigateToOverview()
 {
 CurrentPage = "Overview";
 UpdateOverallAverage();
 }

 [RelayCommand]
 private void NavigateToSubjects()
 {
 CurrentPage = "Subjects";
 }

 [RelayCommand]
 private void NavigateToSettings()
 {
 CurrentPage = "Settings";
 }

 [RelayCommand]
 private void NavigateToNotes(Subject subject)
 {
 SelectedSubject = subject;
 CurrentPage = "Notes";
 }

 [RelayCommand]
 private void ShowAddSubjectModal()
 {
 NewSubjectName = string.Empty;
 NewSubjectLessons =3;
 IsAddSubjectModalVisible = true;
 }

 [RelayCommand]
 private void CloseAddSubjectModal()
 {
 IsAddSubjectModalVisible = false;
 }

 [RelayCommand]
 private void AddSubject()
 {
 if (string.IsNullOrWhiteSpace(NewSubjectName))
 {
 Application.Current.MainPage.DisplayAlert("Fehler", "Bitte gib einen Fachnamen ein", "OK");
 return;
 }

 var colors = new[] { "math", "bio", "info", "deutsch", "other" };
 var newSubject = new Subject
 {
 Name = NewSubjectName,
 LessonsPerWeek = NewSubjectLessons,
 Color = colors[(Subjects?.Count ??0) % colors.Length]
 };

 _dataService.AddSubject(newSubject);
 UpdateOverallAverage();
 IsAddSubjectModalVisible = false;
 }

 [RelayCommand]
 private async Task DeleteSubject(Subject subject)
 {
 bool answer = await Application.Current.MainPage.DisplayAlert(
 "Löschen",
 "Möchtest du dieses Fach wirklich löschen?",
 "Ja",
 "Nein");

 if (answer)
 {
 _dataService.DeleteSubject(subject);
 UpdateOverallAverage();
 }
 }

 [RelayCommand]
 private void ShowAddNoteModal()
 {
 NewNoteType = string.Empty;
 NewNoteGrade = string.Empty;
 NewNoteDate = DateTime.Now;
 IsAddNoteModalVisible = true;
 }

 [RelayCommand]
 private void CloseAddNoteModal()
 {
 IsAddNoteModalVisible = false;
 }

 [RelayCommand]
 private void AddNote()
 {
 if (string.IsNullOrWhiteSpace(NewNoteType) || string.IsNullOrWhiteSpace(NewNoteGrade))
 {
 Application.Current.MainPage.DisplayAlert("Fehler", "Bitte fülle alle Felder aus", "OK");
 return;
 }

 if (!double.TryParse(NewNoteGrade.Replace(',', '.'), out double grade) || grade <1 || grade >6)
 {
 Application.Current.MainPage.DisplayAlert("Fehler", "Note muss zwischen1 und6 liegen", "OK");
 return;
 }

 var newNote = new Note
 {
 Type = NewNoteType,
 Grade = grade,
 Date = NewNoteDate
 };

 _dataService.AddNote(SelectedSubject, newNote);
 // Notify UI that SelectedSubject changed (notes list updated)
 OnPropertyChanged(nameof(SelectedSubject));
 UpdateOverallAverage();
 IsAddNoteModalVisible = false;
 }

 [RelayCommand]
 private void ShowEditNoteModal(Note note)
 {
 _editingNote = note;
 NewNoteType = note.Type;
 NewNoteGrade = note.Grade.ToString();
 NewNoteDate = note.Date;
 IsEditNoteModalVisible = true;
 }

 [RelayCommand]
 private void CloseEditNoteModal()
 {
 IsEditNoteModalVisible = false;
 _editingNote = null;
 }

 [RelayCommand]
 private void UpdateNote()
 {
 if (string.IsNullOrWhiteSpace(NewNoteType) || string.IsNullOrWhiteSpace(NewNoteGrade))
 {
 Application.Current.MainPage.DisplayAlert("Fehler", "Bitte fülle alle Felder aus", "OK");
 return;
 }

 if (!double.TryParse(NewNoteGrade.Replace(',', '.'), out double grade) || grade <1 || grade >6)
 {
 Application.Current.MainPage.DisplayAlert("Fehler", "Note muss zwischen1 und6 liegen", "OK");
 return;
 }

 var updatedNote = new Note
 {
 Type = NewNoteType,
 Grade = grade,
 Date = NewNoteDate
 };

 _dataService.UpdateNote(SelectedSubject, _editingNote, updatedNote);
 OnPropertyChanged(nameof(SelectedSubject));
 UpdateOverallAverage();
 IsEditNoteModalVisible = false;
 }

 [RelayCommand]
 private async Task DeleteNote(Note note)
 {
 bool answer = await Application.Current.MainPage.DisplayAlert(
 "Löschen",
 "Möchtest du diese Note wirklich löschen?",
 "Ja",
 "Nein");

 if (answer)
 {
 _dataService.DeleteNote(SelectedSubject, note);
 OnPropertyChanged(nameof(SelectedSubject));
 UpdateOverallAverage();

 if (IsEditNoteModalVisible)
 {
 IsEditNoteModalVisible = false;
 }
 }
 }

 private void UpdateOverallAverage()
 {
 OverallAverage = _dataService.GetOverallAverage();
 // OverallAverage property setter raises PropertyChanged via SetProperty
 }
 }
}
