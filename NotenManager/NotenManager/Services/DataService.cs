using NotenManager.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace NotenManager.Services
{
 public class DataService
 {
 private ObservableCollection<Subject> _subjects;

 public DataService()
 {
 InitializeData();
 }

 private void InitializeData()
 {
 _subjects = new ObservableCollection<Subject>
 {
 new Subject
 {
 Id =1,
 Name = "Mathematik",
 LessonsPerWeek =3,
 Color = "math",
 Notes = new List<Note>
 {
 new Note { Id =1, Type = "Hausaufgabe", Grade =2.3, Date = new DateTime(2024,1,15) },
 new Note { Id =2, Type = "Klassenarbeit", Grade =2.8, Date = new DateTime(2024,1,22) },
 new Note { Id =3, Type = "Test", Grade =3.5, Date = new DateTime(2024,2,5) }
 }
 },
 new Subject
 {
 Id =2,
 Name = "Biologie",
 LessonsPerWeek =4,
 Color = "bio",
 Notes = new List<Note>
 {
 new Note { Id =1, Type = "Test", Grade =3.1, Date = new DateTime(2024,1,20) }
 }
 },
 new Subject
 {
 Id =3,
 Name = "Informatik",
 LessonsPerWeek =2,
 Color = "info",
 Notes = new List<Note>
 {
 new Note { Id =1, Type = "Projekt", Grade =3.8, Date = new DateTime(2024,1,25) }
 }
 },
 new Subject
 {
 Id =4,
 Name = "Deutsch",
 LessonsPerWeek =4,
 Color = "deutsch",
 Notes = new List<Note>
 {
 new Note { Id =1, Type = "Aufsatz", Grade =2.5, Date = new DateTime(2024,1,18) }
 }
 }
 };
 }

 public ObservableCollection<Subject> GetSubjects()
 {
 return _subjects;
 }

 public void AddSubject(Subject subject)
 {
 subject.Id = _subjects.Any() ? _subjects.Max(s => s.Id) +1 :1;
 _subjects.Add(subject);
 }

 public void DeleteSubject(Subject subject)
 {
 _subjects.Remove(subject);
 }

 public void AddNote(Subject subject, Note note)
 {
 note.Id = subject.Notes.Any() ? subject.Notes.Max(n => n.Id) +1 :1;
 subject.Notes.Add(note);
 }

 public void UpdateNote(Subject subject, Note oldNote, Note newNote)
 {
 var index = subject.Notes.IndexOf(oldNote);
 if (index >=0)
 {
 newNote.Id = oldNote.Id;
 subject.Notes[index] = newNote;
 }
 }

 public void DeleteNote(Subject subject, Note note)
 {
 subject.Notes.Remove(note);
 }

 public double GetOverallAverage()
 {
 var allNotes = _subjects.SelectMany(s => s.Notes).ToList();
 if (allNotes.Count ==0)
 return 0;
 return Math.Round(allNotes.Average(n => n.Grade),1);
 }
 }
}
