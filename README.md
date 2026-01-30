# Musical Journey - Dokumentacja Projektu

## Spis Treści

1. [Opis Projektu](#opis-projektu)
2. [Wymagania Systemowe](#wymagania-systemowe)
3. [Instalacja](#instalacja)
4. [Architektura](#architektura)
5. [Struktura Katalogów](#struktura-katalogów)
6. [Komponenty Systemu](#komponenty-systemu)
7. [Baza Danych](#baza-danych)
8. [Używanie Aplikacji](#używanie-aplikacji)
9. [Rozwój Aplikacji](#rozwój-aplikacji)

---

## Opis Projektu

Musical Journey to odtwarzacz muzyki oparty na technologii Avalonia UI dla platformy .NET. Aplikacja umożliwia odtwarzanie plików audio, zarządzanie playlistami oraz przeglądanie biblioteki muzycznej z automatycznym odczytem metadanych.

### Główne Funkcjonalności

- Odtwarzanie plików audio (MP3, FLAC, WAV, OGG, M4A, AAC, WMA, ALAC, APE, OPUS)
- Tworzenie i zarządzanie playlistami
- Automatyczne grupowanie utworów według albumów
- Odczyt metadanych z plików audio (tagów ID3)
- Kontrola odtwarzania (play, pause, stop, next, previous)
- Regulacja głośności
- Śledzenie pozycji odtwarzania
- Zapisywanie danych w bazie SQLite

---

## Wymagania Systemowe

### Wymagania Podstawowe

- .NET 9.0 SDK lub nowszy
- System operacyjny: Windows lub Linux (macOS nie testowany)
- VLC Media Player (dla obsługi audio)

### Zależności NuGet

```xml
<PackageReference Include="Avalonia" Version="11.x" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.x" />
<PackageReference Include="TagLibSharp" Version="2.x" />
<PackageReference Include="Microsoft.Data.Sqlite" Version="8.x" />
<PackageReference Include="ReactiveUI" Version="19.x" />
```
### Zależności zewnętrzne

Program używa jednej natywnej zależności zewnętrznej. Musi być zainstalowana na systemie by program funkcjonował.
```
LibVLCSharp
```
Może być ona zainstalowana osobno, lub wraz z programem VLC.

---

## Instalacja

### 1. Klonowanie Repozytorium

```bash
git clone https://github.com/KnifeTheKnife/musical-journey.git
cd musical-journey
```

### 2. Przywracanie Zależności

```bash
dotnet restore
```

### 3. Budowanie Projektu

```bash
dotnet build
```

### 4. Uruchomienie Aplikacji

```bash
dotnet run
```

---

## Architektura

Aplikacja wykorzystuje wzorzec MVVM (Model-View-ViewModel).

### Wzorce Projektowe

- **MVVM** - separacja logiki od interfejsu
- **Repository** - abstrakcja dostępu do danych
- **Service Layer** - logika biznesowa w serwisach
- **Observer** - ReactiveUI dla reaktywnych bindingów
- **Command Pattern** - obsługa akcji użytkownika

---

## Struktura Katalogów

```
├── App.axaml
├── App.axaml.cs
├── app.manifest
├── Assets
│   └── musical_journey.ico
├── cache.db
├── musical-journey.csproj
├── musical-journey.sln
├── Program.cs
├── README.md
├── Services
│   ├── AudioService.cs
│   ├── DbCreate.cs
│   ├── Db.cs
│   ├── FsRead.cs
│   ├── GetTags.cs
│   ├── Interfaces
│   │   ├── IAudioService.cs
│   │   ├── IDbCreate.cs
│   │   ├── IDb.cs
│   │   ├── IFsRead.cs
│   │   ├── IGetTags.cs
│   │   ├── IPlaylist.cs
│   │   └── IPlaylistService.cs
│   ├── Playlist.cs
│   └── PlaylistDatabaseService.cs
├── ViewLocator.cs
├── ViewModels
│   ├── AsyncCommand.cs
│   ├── LibraryViewModel.cs
│   ├── MainWindowViewModel.cs
│   ├── PlaybackViewModel.cs
│   ├── PlaylistViewModel.cs
│   ├── SongWrapper.cs
│   └── ViewModelBase.cs
└── Views
    ├── MainWindow.axaml
    └── MainWindow.axaml.cs
```

---

## Komponenty Systemu

### 1. ViewModels

#### MainWindowViewModel

Koordynator wszystkich ViewModeli. Zarządza komunikacją między komponentami.

**Odpowiedzialności:**
- Inicjalizacja ViewModeli potomnych
- Konfiguracja reaktywnych subskrypcji
- Przekazywanie referencji do okna

**Właściwości:**
```csharp
public PlaybackViewModel PlaybackViewModel { get; }
public LibraryViewModel LibraryViewModel { get; }
public PlaylistViewModel PlaylistViewModel { get; }
```

#### PlaybackViewModel

Zarządza odtwarzaniem audio.

**Odpowiedzialności:**
- Kontrola odtwarzania (play, pause, stop)
- Nawigacja między utworami (next, previous)
- Regulacja głośności i pozycji
- Wyświetlanie informacji o bieżącym utworze

**Główne właściwości:**
```csharp
IAudioService AudioService              // Serwis audio
int Volume                              // Głośność (0-100)
float PlaybackPosition                  // Pozycja odtwarzania (0-1)
SongWrapper CurrentSong                 // Bieżący utwór
ObservableCollection<SongWrapper> CurrentPlaylist  // Bieżąca playlista
```

**Główne komendy:**
```csharp
ICommand PlayPauseCommand
ICommand StopCommand
ICommand NextCommand
ICommand PreviousCommand
```

#### LibraryViewModel

Zarządza biblioteką muzyczną.

**Odpowiedzialności:**
- Przeglądanie katalogów z muzyką
- Skanowanie plików audio
- Odczyt metadanych
- Grupowanie utworów według albumów
- Sortowanie utworów

**Główne właściwości:**
```csharp
ObservableCollection<AlbumFolder> AlbumFolders    // Foldery albumów
ObservableCollection<AlbumGroup> AlbumGroups      // Utwory grupowane po albumach
ObservableCollection<SongWrapper> Songs           // Wszystkie utwory
```

**Główne komendy:**
```csharp
ICommand BrowseMusicFilesCommand  // Wybór folderu z muzyką
```

#### PlaylistViewModel

Zarządza playlistami.

**Odpowiedzialności:**
- Tworzenie i usuwanie playlist
- Dodawanie/usuwanie utworów
- Dodawanie całych albumów
- Czyszczenie playlist
- Zapis do bazy danych

**Główne właściwości:**
```csharp
ObservableCollection<Playlist> Playlists     // Wszystkie playlisty
Playlist SelectedPlaylist                    // Wybrana playlista
```

**Główne komendy:**
```csharp
ICommand CreatePlaylistCommand
ICommand DeletePlaylistCommand
ICommand AddSongToPlaylistCommand
ICommand RemoveSongFromPlaylistCommand
ICommand AddAlbumToPlaylistCommand
ICommand ClearPlaylistCommand
```

### 2. Serwisy

#### AudioService

Implementuje interfejs `IAudioService` używając LibVLC.

**Metody:**
```csharp
void Play(string path)    // Odtwarzanie pliku
void Stop()               // Zatrzymanie odtwarzania
MediaPlayer MediaPlayer   // Instancja VLC MediaPlayer
```

#### PlaylistDatabaseService

Zarządza playlistami w bazie SQLite.

**Metody:**
```csharp
Playlist CreatePlaylist(string name)
bool DeletePlaylist(string playlistId)
Playlist GetPlaylistById(string playlistId)
List<Playlist> GetAllPlaylists()
bool AddSongToPlaylist(string playlistId, Song song)
bool RemoveSongFromPlaylist(string playlistId, string songPath)
bool ClearPlaylist(string playlistId)
```

#### FsRead

Skanuje system plików w poszukiwaniu plików muzycznych.

**Metody:**
```csharp
List<string> GetMusicFiles(string musicPath)    // Rekursywne skanowanie
List<string> ScanForDirectory(string dirPath)   // Pobranie podkatalogów
List<string> DirectoryNames(string dirPath)     // Nazwy podkatalogów
```

**Obsługiwane formaty:**
- .mp3, .flac, .wav, .ogg, .m4a, .aac, .wma, .alac, .ape, .opus

#### GetTag

Odczytuje metadane z plików audio używając TagLib.

**Metody:**
```csharp
Song GetTags(string path)  // Zwraca strukturę Song z tagami
```

**Odczytywane tagi:**
- Tytuł utworu
- Artysta
- Album
- Numer utworu
- Gatunek
- Rok wydania
- Numer dysku

### 3. Modele Danych

#### Song (struct)

```csharp
public struct Song
{
    public string title;
    public string artist;
    public string album;
    public string trackNo;
    public string genre;
    public string date;
    public string discNo;
    public string path;
}
```

#### SongWrapper (class)

Wrapper dla struktury Song zapewniający właściwości dla bindingu.

```csharp
public class SongWrapper
{
    public string Title { get; }
    public string Artist { get; }
    public string Album { get; }
    public string TrackNo { get; }
    public string Genre { get; }
    public string Date { get; }
    public string DiscNo { get; }
    public string Path { get; }
    
    public Song GetSong();
}
```

#### AlbumFolder

```csharp
public class AlbumFolder
{
    public string Name { get; set; }
    public string Path { get; set; }
    public ICommand ClickCommand { get; set; }
}
```

#### AlbumGroup

```csharp
public class AlbumGroup
{
    public string AlbumName { get; set; }
    public ObservableCollection<SongWrapper> Songs { get; set; }
}
```

#### Playlist

```csharp
public class Playlist : IPlaylist
{
    public string Id { get; set; }
    public string Name { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ModifiedDate { get; set; }
    public ObservableCollection<Song> Songs { get; }
    
    public void AddSong(Song song);
    public bool RemoveSong(string songPath);
    public void Clear();
}
```

---

## Baza Danych

Aplikacja używa SQLite do przechowywania danych.

### Schemat Bazy Danych

#### Tabela: Songs

Przechowuje metadane utworów.

```sql
CREATE TABLE IF NOT EXISTS Songs (
    Path TEXT PRIMARY KEY,
    Title TEXT,
    Artist TEXT,
    Album TEXT,
    TrackNo INT,
    Date Date,
    Genre TEXT,
    DiscNo INT
);
```

#### Tabela: Playlists

Przechowuje informacje o playlistach.

```sql
CREATE TABLE IF NOT EXISTS Playlists (
    Id TEXT PRIMARY KEY,
    Name TEXT NOT NULL,
    CreatedDate TEXT NOT NULL,
    ModifiedDate TEXT NOT NULL
);
```

#### Tabela: PlaylistSongs

Tabela łącząca playlisty z utworami (relacja many-to-many).

```sql
CREATE TABLE IF NOT EXISTS PlaylistSongs (
    PlaylistId TEXT NOT NULL,
    SongTitle TEXT,
    SongArtist TEXT,
    SongAlbum TEXT,
    SongTrackNo TEXT,
    SongGenre TEXT,
    SongDate TEXT,
    SongDiscNo TEXT,
    SongPath TEXT NOT NULL,
    FOREIGN KEY(PlaylistId) REFERENCES Playlists(Id) ON DELETE CASCADE,
    PRIMARY KEY(PlaylistId, SongPath)
);
```

### Lokalizacja Bazy

Baza danych jest tworzona automatycznie w katalogu roboczym aplikacji:
```
cache.db
```

---

## Używanie Aplikacji

### Uruchomienie

1. Uruchom aplikację
2. Główne okno aplikacji zostanie wyświetlone z czterema panelami

### Przeglądanie Muzyki

1. Kliknij przycisk "Choose Folder" u dołu okna
2. Wybierz folder zawierający muzykę
3. Aplikacja automatycznie:
   - Przeskanuje folder i podfoldery
   - Wyświetli foldery albumów w lewym panelu
   - Odczyta metadane z plików

### Odtwarzanie Muzyki

1. Kliknij na folder albumu w lewym panelu
2. Rozwiń album w środkowym panelu
3. Kliknij na utwór aby go odtworzyć
4. Użyj kontrolek u dołu:
   - Przycisk środkowy: Play/Pause
   - Strzałka w lewo: Poprzedni utwór
   - Strzałka w prawo: Następny utwór
   - Suwak: Pozycja odtwarzania
   - Suwak Volume: Regulacja głośności

### Zarządzanie Playlistami

#### Tworzenie Playlisty

1. Kliknij przycisk "+" w panelu playlist
2. Wprowadź nazwę playlisty
3. Kliknij OK

#### Dodawanie Utworów

**Pojedynczy utwór:**
1. Wybierz utwór w panelu albumów
2. Kliknij "Add to Playlist" w panelu informacji o utworze

**Cały album:**
1. Wybierz album w panelu albumów
2. Kliknij "Add Album to Playlist"

#### Usuwanie Utworów

1. Przejdź do zakładki "Playlist Songs"
2. Wybierz utwór z playlisty
3. Kliknij "Remove from Playlist"

#### Usuwanie Playlisty

1. Wybierz playlistę
2. Kliknij przycisk "-"

---

## Rozwój Aplikacji

### Dodawanie Nowych Funkcji

#### 1. Dodanie Nowego ViewModelu

```csharp
// ViewModels/NewFeatureViewModel.cs
public class NewFeatureViewModel : ViewModelBase
{
    public NewFeatureViewModel()
    {
        // Inicjalizacja
    }
}
```

Dodaj do MainWindowViewModel:
```csharp
public NewFeatureViewModel NewFeatureViewModel { get; }

public MainWindowViewModel()
{
    // ...
    NewFeatureViewModel = new NewFeatureViewModel();
}
```

#### 2. Dodanie Nowego Serwisu

Utworz interfejs:
```csharp
// Services/Interfaces/INewService.cs
public interface INewService
{
    void DoSomething();
}
```

Implementuj interfejs:
```csharp
// Services/NewService.cs
public class NewService : INewService
{
    public void DoSomething()
    {
        // Implementacja
    }
}
```

#### 3. Dodanie Nowej Komendy

```csharp
public ICommand NewCommand { get; }

public SomeViewModel()
{
    NewCommand = new AsyncCommand(ExecuteNewCommand);
}

private async Task ExecuteNewCommand()
{
    // Logika komendy
}
```


### Konwencje Kodowania

#### Nazewnictwo

- **Klasy:** PascalCase (np. `PlaybackViewModel`)
- **Metody:** PascalCase (np. `LoadPlaylists()`)
- **Właściwości:** PascalCase (np. `CurrentSong`)
- **Pola prywatne:** camelCase z prefiksem _ (np. `_audioService`)
- **Interfejsy:** PascalCase z prefiksem I (np. `IAudioService`)

#### Komentarze

Większość funkcji jest opisana w poniższym stylu.

```csharp
/// <summary>
/// Odtwarza utwór z podanej ścieżki
/// </summary>
/// <param name="path">Ścieżka do pliku audio</param>
public void Play(string path)
{
    // Implementacja
}
```