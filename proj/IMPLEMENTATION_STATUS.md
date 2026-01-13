# Map Editor - .NET Implementation

## 🚀 Status Projektu

**Faza 1 (MVP) - W REALIZACJI**

### ✅ Ukończone Funkcjonalności

#### Setup Projektu

- [x] Solution utworzony (.NET 10.0)
- [x] Clean Architecture (Domain, Application, Infrastructure, Presentation)
- [x] Dependency Injection skonfigurowany
- [x] Projekty połączone referencjami

#### Domain Model (Tydzień 1)

- [x] **Point** - Value Object dla współrzędnych
- [x] **Size** - Value Object dla rozmiaru
- [x] **SquareType** - Enum typów terenu
- [x] **EntityType** - Enum typów entity
- [x] **Square** - Entity reprezentująca teren
- [x] **Cell** - Element Grid2D
- [x] **Grid2D** - Siatka 2D
- [x] **Group** - Warstwa elementów
- [x] **Workspace** - Główny agregat

#### Infrastructure (Tydzień 1)

- [x] **WorkspaceFileRepository** - Zapis/odczyt do JSON
- [x] Serializacja/Deserializacja Workspace

#### Application Layer (Tydzień 1)

- [x] **EditingService** - Serwis edycji
- [x] Operacje: Create, Save, Load, PlaceSquare, RemoveSquare

#### Presentation Layer (Tydzień 1)

- [x] **MainWindow** - Główne okno WPF
- [x] **MainViewModel** - ViewModel z MVVM
- [x] **GridCanvas** - Custom control do renderowania Grid2D
- [x] Toolbar z Square Types
- [x] Menu Bar (File, Edit, Tools, Biometrics)
- [x] Status Bar

#### Testy (Tydzień 1)

- [x] **WorkspaceTests** - 6 testów jednostkowych
- [x] **Grid2DTests** - 6 testów jednostkowych
- [x] Wszystkie testy przechodzą ✅

### 📊 Punktacja

| User Story                             | Status  | Punkty         |
| -------------------------------------- | ------- | -------------- |
| Standard US#1: Tworzenie Workspace     | ✅ DONE | 2 pkt          |
| Standard US#2: Zapisywanie/Wczytywanie | ✅ DONE | 2 pkt          |
| Standard US#16: Dependency Injection   | ✅ DONE | 2 pkt          |
| Minor US#1: Square Tool                | ✅ DONE | 1 pkt          |
| Minor US#2: Element Remover            | ✅ DONE | 1 pkt          |
| Minor US#5: User-friendly Interface    | ✅ DONE | 1 pkt          |
| **ŁĄCZNIE**                            |         | **9 pkt / 56** |

---

## 🛠️ Technologie

- **.NET 10.0**
- **WPF** dla GUI
- **xUnit** dla testów
- **Microsoft.Extensions.DependencyInjection** dla IoC
- **System.Text.Json** dla serializacji

---

## 🏗️ Struktura Projektu

```
MapEditor.sln
├── src/
│   ├── Domain/                         # Domain Layer (Core Business Logic)
│   │   ├── Editing/
│   │   │   ├── Entities/              # Workspace, Grid2D, Square, Cell, Group
│   │   │   ├── ValueObjects/          # Point, Size
│   │   │   └── Services/              # IWorkspaceRepository
│   │   └── Shared/
│   │       └── Enums/                 # SquareType, EntityType
│   │
│   ├── Application/                    # Application Layer (Use Cases)
│   │   └── Services/                  # EditingService
│   │
│   ├── Infrastructure/                 # Infrastructure Layer (Technical Concerns)
│   │   └── Repositories/              # WorkspaceFileRepository
│   │
│   └── Presentation/                   # Presentation Layer (WPF UI)
│       ├── Commands/                  # RelayCommand
│       ├── Controls/                  # GridCanvas
│       ├── ViewModels/                # MainViewModel, ViewModelBase
│       ├── MainWindow.xaml            # Główne okno
│       └── App.xaml                   # Aplikacja + DI Setup
│
└── tests/
    └── Unit/                          # Testy jednostkowe
        └── Domain/                    # WorkspaceTests, Grid2DTests
```

---

## 🚀 Jak Uruchomić

### Wymagania

- **.NET SDK 10.0** lub nowszy
- **Windows 10/11** (dla WPF)
- Visual Studio 2022 lub VS Code z C# extension

### Kroki

1. **Sklonuj/pobierz projekt**

   ```bash
   cd "c:\Users\dawid\Desktop\studia\MAG\semestr 2\PrzetwarzanieObrazow\proj"
   ```

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Build solution**

   ```bash
   dotnet build
   ```

4. **Uruchom testy**

   ```bash
   dotnet test
   ```

5. **Uruchom aplikację**
   ```bash
   dotnet run --project src/Presentation/MapEditor.Presentation.csproj
   ```

---

## 📖 Jak Używać

### Tworzenie Nowego Workspace

1. Uruchom aplikację
2. Menu: **File → New Workspace**
3. Workspace 20x15 zostanie utworzony automatycznie

### Edycja Mapy

1. **Wybierz Square Type** z listy po lewej stronie
2. **Lewy przycisk myszy** - umieszcza wybrany Square
3. **Prawy przycisk myszy** - usuwa Square

### Zapisywanie/Wczytywanie

- **File → Save** - zapisz workspace do pliku .workspace (JSON)
- **File → Open** - wczytaj workspace z pliku

### Square Types

- **Grass** (zielony) - trawa
- **Stone** (szary) - kamień
- **Water** (niebieski) - woda
- **Sand** (beżowy) - piasek
- **Wood** (brązowy) - drewno
- **Metal** (srebrny) - metal
- **Lava** (pomarańczowy) - lawa

---

## 🧪 Testy

```bash
# Uruchom wszystkie testy
dotnet test

# Uruchom z szczegółami
dotnet test --verbosity detailed

# Uruchom konkretny test
dotnet test --filter "FullyQualifiedName~WorkspaceTests"
```

**Wszystkie 12 testów przechodzą** ✅

---

## 📝 Następne Kroki (Tydzień 2)

### Do Zaimplementowania

#### Algorytmy Biometryczne

- [ ] **UL22 Converter** (Standard US#12) - 2 pkt
- [ ] **Preprocessing** - Median Filter, Otsu (Major US#5) - 3 pkt
- [ ] **Fragmentation** - Connected Components (Major US#7) - 3 pkt
- [ ] **Testy algorytmów** (Standard US#11) - 2 pkt

#### UI Improvements

- [ ] **Real-time preview** (Standard US#14) - 2 pkt
- [ ] **Group management** (Minor US#3) - 1 pkt
- [ ] **Auto-detect Grid Border** (Minor US#10) - 1 pkt

**Target: 14 pkt → Total: 23 pkt (MVP Complete!)**

---

## 🏛️ Architektura

### Clean Architecture Layers

```
Presentation → Application → Domain ← Infrastructure
```

### Dependency Flow

- **Presentation** zależy od Application, Domain, Infrastructure
- **Application** zależy od Domain
- **Infrastructure** zależy od Domain, Application
- **Domain** - niezależna warstwa

### Wzorce

- **MVVM** (Model-View-ViewModel) w Presentation
- **Repository Pattern** dla dostępu do danych
- **Dependency Injection** dla IoC
- **Domain-Driven Design** (Aggregates, Entities, Value Objects)

---

## 🐛 Znane Problemy

1. **New Workspace Dialog** - obecnie hardcoded rozmiar 20x15
   - TODO: Dodać dialog do wyboru nazwy i rozmiaru

2. **Undo/Redo** - nie zaimplementowane
   - TODO: Command Pattern z historią

3. **Group Management** - tylko default group
   - TODO: UI do tworzenia/przełączania grup

---

## 📚 Dokumentacja

- [README.md](../README.md) - Przegląd projektu
- [TASKS.md](../TASKS.md) - Wszystkie User Stories
- [ARCHITECTURE.md](../ARCHITECTURE.md) - Szczegółowa architektura
- [ALGORITHMS.md](../ALGORITHMS.md) - Algorytmy biometryczne
- [TEST_CASES.md](../TEST_CASES.md) - Przykłady testowe

---

## 🤝 Wkład Zespołu

- **Setup projektu** - Done ✅
- **Domain Model** - Done ✅
- **Infrastructure** - Done ✅
- **WPF UI** - Done ✅
- **Testy** - Done ✅

### Następni do realizacji

- **Frontend Team**: Group management UI, dialogi
- **Backend/Algorithms Team**: UL22, Preprocessing, Fragmentacja

---

## 📞 Kontakt

Zespół: Dorota Maliszewska, Jakub Modzelewski, Mateusz Kondraciuk, Dawid Waszkiewicz, Emilia Stypułkowska

---

**Status**: MVP Tydzień 1 - Zakończony ✅  
**Punkty**: 9 / 23 (39% do MVP)  
**Ostatnia aktualizacja**: 2026-01-11
