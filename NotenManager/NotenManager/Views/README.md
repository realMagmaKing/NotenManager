# NotenManager - Neue Ansichten

## ?? Übersicht der neuen Views

Ich habe 4 moderne, performante Ansichten für deine NotenManager-App erstellt:

### 1. **NotesListPage** - CollectionView für Noten
**Datei:** `Views/NotesListPage.xaml`

**Features:**
- ? Moderne **CollectionView** statt BindableLayout
- ? Gruppierung nach Fächern
- ? Smooth Scrolling & bessere Performance
- ? Swipe-Gesten ready (kann erweitert werden)
- ? Hierarchische Struktur: Fach ? Noten
- ? Datum-Anzeige mit Tag & Monat
- ? Farbcodierte Fach-Header
- ? Empty State mit hilfreicher Message

**Verwendung:**
```csharp
// In MainPage.xaml.cs oder Navigation
var notesListPage = new NotesListPage(_viewModel);
await Navigation.PushAsync(notesListPage);
```

---

### 2. **SubjectsCarouselPage** - CarouselView für Fächer
**Datei:** `Views/SubjectsCarouselPage.xaml`

**Features:**
- ? **CarouselView** mit wischbaren Karten
- ? Peek-Modus: nächste/vorherige Karte sichtbar
- ? Gradient-Hintergründe pro Fach
- ? Große, schöne Karten mit Statistiken
- ? **IndicatorView** (Dots) zur Navigation
- ? Fach-Initial als großes Icon
- ? Durchschnitt & Noten-Anzahl prominent
- ? Schnell-Actions: "Noten" & "Löschen"

**Verwendung:**
```csharp
var carouselPage = new SubjectsCarouselPage(_viewModel);
await Navigation.PushAsync(carouselPage);
```

---

### 3. **NotesTablePage** - Tabellen-Ansicht
**Datei:** `Views/NotesTablePage.xaml`

**Features:**
- ? Strukturierte **Tabellen-Ansicht**
- ? Spalten: Datum | Fach/Typ | Note | Ø Fach | Ø Gesamt | Actions
- ? Farbcodierte Fach-Trenner
- ? Fach-Durchschnitt-Zeilen
- ? Sortierung & Filterung (vorbereitet)
- ? Professional Look
- ? Picker für Fach-Filter (oben rechts)

**Verwendung:**
```csharp
var tablePage = new NotesTablePage(_viewModel);
await Navigation.PushAsync(tablePage);
```

---

### 4. **StatisticsPage** - Dashboard mit Statistiken
**Datei:** `Views/StatisticsPage.xaml`

**Features:**
- ? Dashboard-Layout mit Cards
- ? Gesamtdurchschnitt (Gradient-Card)
- ? Anzahl Fächer & Noten
- ? Ziel-Anzeige
- ? **Top 3 Fächer** mit Rang-Badges
- ? **Notenverteilung** mit ProgressBars
- ? **Letzte 5 Noten** als Liste
- ? Scrollbar für lange Inhalte

**Verwendung:**
```csharp
var statsPage = new StatisticsPage(_viewModel);
await Navigation.PushAsync(statsPage);
```

---

## ?? Integration in deine App

### Option 1: Über Navigation Commands (empfohlen)

Füge im **MainViewModel** neue Commands hinzu:

```csharp
[RelayCommand]
private async void NavigateToNotesList()
{
    var page = new NotesListPage(this);
   await Application.Current.MainPage.Navigation.PushAsync(page);
}

[RelayCommand]
private async void NavigateToSubjectsCarousel()
{
    var page = new SubjectsCarouselPage(this);
    await Application.Current.MainPage.Navigation.PushAsync(page);
}

[RelayCommand]
private async void NavigateToNotesTable()
{
    var page = new NotesTablePage(this);
    await Application.Current.MainPage.Navigation.PushAsync(page);
}

[RelayCommand]
private async void NavigateToStatistics()
{
    var page = new StatisticsPage(this);
    await Application.Current.MainPage.Navigation.PushAsync(page);
}
```

### Option 2: Als Tabs in der Hauptnavigation

Ersetze in `MainPage.xaml` die Sidebar-Buttons:

```xml
<Frame ...>
    <Frame.GestureRecognizers>
     <TapGestureRecognizer Command="{Binding NavigateToNotesListCommand}"/>
    </Frame.GestureRecognizers>
    <HorizontalStackLayout Spacing="12">
        <Label Text="??" FontSize="20"/>
        <Label Text="Notenliste" FontSize="15" TextColor="White"/>
    </HorizontalStackLayout>
</Frame>

<Frame ...>
    <Frame.GestureRecognizers>
        <TapGestureRecognizer Command="{Binding NavigateToSubjectsCarouselCommand}"/>
    </Frame.GestureRecognizers>
    <HorizontalStackLayout Spacing="12">
        <Label Text="??" FontSize="20"/>
        <Label Text="Fächer Carousel" FontSize="15" TextColor="White"/>
    </HorizontalStackLayout>
</Frame>
```

### Option 3: Als modale Popups

```csharp
var page = new NotesListPage(_viewModel);
await Navigation.PushModalAsync(page);
```

---

## ?? Styling & Anpassungen

### Farben ändern

Alle Views verwenden deine bestehenden Colors aus `Resources/Styles/Colors.xaml`:
- `Primary` (#667eea)
- `Secondary` (#764ba2)
- Theme-aware: Light/Dark Mode

### Icons anpassen

```xml
<!-- Emoji Icons -->
<Label Text="??" />  <!-- Statistik -->
<Label Text="??" />  <!-- Noten -->
<Label Text="??" />  <!-- Fächer -->
<Label Text="??" />  <!-- Top -->

<!-- Oder Font Icons (wenn Font Awesome installiert) -->
<Label FontFamily="FontAwesome" Text="&#xf080;" />
```

### Layout anpassen

```xml
<!-- Spaltenbreiten in Tabelle -->
<Grid ColumnDefinitions="60,*,80,100,80,60">

<!-- Peek-Bereich in Carousel -->
<CarouselView PeekAreaInsets="40">  <!-- Größer = mehr Peek -->

<!-- Card-Abstände -->
<Frame Margin="10">  <!-- Ändern für mehr/weniger Platz -->
```

---

## ?? Performance-Tipps

### CollectionView vs. BindableLayout

**Vorher (BindableLayout):**
```xml
<FlexLayout BindableLayout.ItemsSource="{Binding Items}">
    <!-- Rendert ALLE Items auf einmal -->
</FlexLayout>
```

**Jetzt (CollectionView):**
```xml
<CollectionView ItemsSource="{Binding Items}">
    <!-- Virtualisierung: nur sichtbare Items werden gerendert -->
</CollectionView>
```

**Vorteile:**
- ? 10x-100x schnellere Ladezeiten bei vielen Items
- ? Smooth Scrolling
- ? Weniger RAM-Verbrauch
- ? Item-Selection
- ? Pull-to-Refresh ready
- ? Incremental Loading möglich

### CarouselView Best Practices

```xml
<!-- EMPFOHLEN -->
<CarouselView Loop="False">  <!-- Kein Loop bei wenigen Items -->
    <CarouselView.ItemsLayout>
   <LinearItemsLayout Orientation="Horizontal" 
  SnapPointsType="MandatorySingle"/>
    </CarouselView.ItemsLayout>
</CarouselView>
```

---

## ?? Swipe-Aktionen hinzufügen (optional)

Für iOS/Android kannst du Swipe-to-Delete hinzufügen:

```xml
<CollectionView.ItemTemplate>
    <DataTemplate>
        <SwipeView>
            <SwipeView.RightItems>
                <SwipeItems>
                <SwipeItem Text="Löschen"
         BackgroundColor="#ff6b6b"
         Command="{Binding DeleteCommand}"
           CommandParameter="{Binding .}"/>
       </SwipeItems>
   </SwipeView.RightItems>
     
   <!-- Dein Item Content -->
      <Frame>...</Frame>
        </SwipeView>
    </DataTemplate>
</CollectionView.ItemTemplate>
```

---

## ?? Troubleshooting

### "Page not found" Error
```csharp
// Stelle sicher, dass der Namespace stimmt
using NotenManager.Views;

// Und die Datei im .csproj ist:
<MauiXaml Include="Views\NotesListPage.xaml" />
```

### "BindingContext is null"
```csharp
// IMMER ViewModel übergeben!
var page = new NotesListPage(_viewModel);  // ? GUT
var page = new NotesListPage();            // ? SCHLECHT
```

### CarouselView zeigt nichts
```xml
<!-- HeightRequest hinzufügen -->
<CarouselView ItemsSource="{Binding Subjects}"
    HeightRequest="500">
```

---

## ?? Nächste Schritte

### Empfohlene Erweiterungen:

1. **Pull-to-Refresh** hinzufügen:
```xml
<RefreshView IsRefreshing="{Binding IsRefreshing}"
            Command="{Binding RefreshCommand}">
    <CollectionView ...>
</RefreshView>
```

2. **Search Bar** für Filterung:
```xml
<SearchBar Placeholder="Suche..."
    Text="{Binding SearchText}"
          SearchCommand="{Binding SearchCommand}"/>
```

3. **Sortierung** implementieren:
```csharp
public ObservableCollection<Note> SortedNotes => 
    new(Notes.OrderByDescending(n => n.Date));
```

4. **Infinite Scroll** für große Listen:
```xml
<CollectionView RemainingItemsThreshold="5"
          RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}">
```

---

## ? Checkliste

- [x] NotesListPage erstellt
- [x] SubjectsCarouselPage erstellt
- [x] NotesTablePage erstellt
- [x] StatisticsPage erstellt
- [ ] Navigation Commands im ViewModel hinzufügen
- [ ] Buttons in MainPage.xaml verlinken
- [ ] Testen auf Windows
- [ ] Testen auf Android (optional)
- [ ] Pull-to-Refresh hinzufügen (optional)
- [ ] Search/Filter implementieren (optional)

---

## ?? Weitere Ressourcen

- [CollectionView Docs](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/collectionview/)
- [CarouselView Docs](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/carouselview/)
- [SwipeView Docs](https://learn.microsoft.com/en-us/dotnet/maui/user-interface/controls/swipeview/)

---

**Viel Erfolg mit den neuen Views! ??**
