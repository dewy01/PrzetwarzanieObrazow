# Struktura Projektu

## Katalogi

```
proj/
├── README.md                                    # Główna instrukcja projektu
├── TASKS.md                                     # Lista User Stories z priorytetami
├── ARCHITECTURE.md                              # Szczegółowa architektura systemu
├── ALGORITHMS.md                                # Szczegółowe opisy algorytmów biometrycznych
├── .gitignore                                   # Pliki ignorowane przez Git
│
├── docs/                                        # Dodatkowa dokumentacja
│   └── (dodatkowe dokumenty projektowe)
│
├── src/                                         # Kod źródłowy
│   ├── Domain/                                  # Warstwa domenowa (DDD)
│   │   ├── Editing/                            # Editing Context
│   │   │   ├── Entities/                       # Workspace, Grid2D, Group, Square, Entity
│   │   │   ├── ValueObjects/                   # Point, Size, GridBorder
│   │   │   ├── Events/                         # Domain Events dla edycji
│   │   │   └── Services/                       # IEditingService, IToolManager
│   │   │
│   │   ├── Biometric/                          # Biometric Context
│   │   │   ├── ValueObjects/                   # UL22, Skeleton, StructuralFeature, GridFeatures
│   │   │   ├── Services/                       # Interfejsy algorytmów biometrycznych
│   │   │   └── Events/                         # Domain Events dla biometrii
│   │   │
│   │   ├── Prefab/                             # Prefab Context
│   │   │   ├── Entities/                       # Preset
│   │   │   ├── ValueObjects/                   # PresetLayout
│   │   │   └── Services/                       # IPresetService
│   │   │
│   │   └── Shared/                             # Współdzielone komponenty
│   │       └── Enums/                          # SquareType, EntityType, FeatureType
│   │
│   ├── Application/                            # Warstwa aplikacji
│   │   ├── UseCases/                           # Use cases (CreateWorkspace, SaveWorkspace, etc.)
│   │   ├── DTOs/                               # Data Transfer Objects
│   │   └── Services/                           # Application Services
│   │
│   ├── Infrastructure/                         # Warstwa infrastruktury
│   │   ├── Repositories/                       # Implementacje repozytoriów
│   │   ├── Algorithms/                         # Implementacje algorytmów biometrycznych
│   │   │   ├── Preprocessing/                  # NoiseRemoval, Binarization, Filters
│   │   │   ├── Fragmentation/                  # ConnectedComponents, RegionGrowing
│   │   │   ├── Skeletonization/                # ZhangSuen, MorphologicalThinning
│   │   │   └── BranchDetection/                # CrossingNumber, FeatureExtraction
│   │   └── Serialization/                      # Serializacja/deserializacja
│   │
│   ├── Presentation/                           # Warstwa prezentacji (UI)
│   │   ├── Views/                              # Widoki GUI
│   │   ├── ViewModels/                         # ViewModels (MVVM)
│   │   └── Converters/                         # Konwertery wartości dla UI
│   │
│   └── DependencyInjection/                    # Konfiguracja DI Container
│
├── tests/                                       # Testy
│   ├── Unit/                                    # Testy jednostkowe
│   │   └── Algorithms/                         # Testy algorytmów biometrycznych
│   ├── Integration/                            # Testy integracyjne
│   └── TestData/                               # Dane testowe
│       ├── Images/                             # Obrazy testowe
│       ├── Workspaces/                         # Przykładowe workspace
│       └── ExpectedResults/                    # Oczekiwane wyniki
│
└── assets/                                      # Zasoby
    ├── Icons/                                   # Ikony narzędzi
    └── ExampleMaps/                            # Przykładowe mapy
```

## Cel Katalogów

### Domain Layer

Reguły biznesowe, logika domenowa, niezależna od technologii.

### Application Layer

Koordynacja use cases, orkiestracja operacji domenowych.

### Infrastructure Layer

Implementacje techniczne: algorytmy, I/O, serializacja.

### Presentation Layer

Interfejs użytkownika, binding danych, interakcje.

## Rozpoczęcie Pracy

1. Wybierz technologię (C#/WPF, Python/Qt, Java/JavaFX, etc.)
2. Stwórz projekt w wybranej technologii w katalogu `src/`
3. Zacznij od Domain Layer - zdefiniuj encje i value objects
4. Zaimplementuj algorytmy w Infrastructure Layer z testami
5. Dodaj Application Services do koordynacji
6. Na końcu stwórz Presentation Layer
