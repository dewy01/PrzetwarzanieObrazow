# Architektura Projektu - Edytor Map

## 🏛️ Przegląd Architektury

Projekt oparty na **Domain-Driven Design (DDD)** z podziałem na **Bounded Contexts** i wykorzystaniem **Dependency Injection** dla osiągnięcia czystej architektury.

## 📐 Bounded Contexts

### 1. Editing Context

**Odpowiedzialność**: Tworzenie, edycja i przechowywanie Workspace

**Kluczowe komponenty**:

- `Workspace` (Domain Entity)
- `2D Grid` (Domain Entity)
- `Square` (Domain Entity)
- `Entity` (Domain Entity)
- `Preset` (Domain Entity)
- `Group` (Domain Entity)
- `Tools` (Square tool, Fill tool, Element remover, Entity tool)

**Operacje**:

- Tworzenie nowego Workspace
- Zapisywanie/wczytywanie z pliku
- Umieszczanie/usuwanie Element
- Zarządzanie Group
- Undo/Redo

**Interfejsy**:

```
IWorkspaceRepository
  - Save(Workspace): void
  - Load(path): Workspace
  - Create(size): Workspace

IEditingService
  - PlaceSquare(x, y, squareType): void
  - PlaceEntity(x, y, entityType): void
  - RemoveElement(x, y): void
  - FillArea(x, y, squareType): void

IToolManager
  - SetActiveTool(tool): void
  - GetActiveTool(): ITool

IUndoRedoManager
  - ExecuteAction(action): void
  - Undo(): void
  - Redo(): void
```

---

### 2. Biometric Context

**Odpowiedzialność**: Algorytmy biometryczne do analizy obrazów i Workspace

**Kluczowe komponenty**:

- `UL22` (Value Object - binarna reprezentacja)
- `Skeleton` (Value Object - wynik szkieletyzacji)
- `StructuralFeatures` (Value Object - lista punktów charakterystycznych)
- `GridFeatures` (Value Object - numeryczna reprezentacja)

**Algorytmy**:

1. **Preprocessing**
   - Usuwanie szumu
   - Binaryzacja
   - Filtracja

2. **Fragmentation**
   - Podział na regiony (Map/Background)
   - Connected Component Analysis
   - Region Growing

3. **Skeletonization**
   - Zhang-Suen
   - Morphological thinning
   - Inne algorytmy

4. **Branch Detection**
   - Detekcja końcówek
   - Detekcja bifurkacji
   - Detekcja skrzyżowań

**Interfejsy**:

```
IUL22Converter
  - ConvertWorkspace(workspace): UL22
  - ConvertImage(image): UL22

IPreprocessingService
  - RemoveNoise(image): Image
  - Binarize(image, threshold): BinaryImage
  - Filter(image, filterType): Image

IFragmentationService
  - SegmentRegions(image, params): List<Region>
  - FindCanvas(grid, x, y): Canvas

ISkeletonizationService
  - Skeletonize(image, algorithm): Skeleton
  - GetAvailableAlgorithms(): List<string>

IBranchDetectionService
  - DetectFeatures(skeleton): List<StructuralFeature>
  - ClassifyPoint(skeleton, x, y): FeatureType

IGridFeaturesService
  - ComputeFeatures(workspace): GridFeatures
  - CompareFeatures(f1, f2): float
```

---

### 3. Prefab Context

**Odpowiedzialność**: Zarządzanie presetami

**Kluczowe komponenty**:

- `Preset` (Domain Entity)
- `PresetLayout` (Value Object)

**Operacje**:

- Import obrazu → Preset
- Zapisywanie/wczytywanie Preset
- Umieszczanie Preset w Workspace

**Interfejsy**:

```
IPresetService
  - CreateFromImage(image, fragmentationParams): Preset
  - SavePreset(preset, path): void
  - LoadPreset(path): Preset

IPresetRepository
  - GetAll(): List<Preset>
  - GetById(id): Preset
  - Save(preset): void
```

---

### 4. Visualization Context

**Odpowiedzialność**: Renderowanie i wizualizacja w czasie rzeczywistym

**Kluczowe komponenty**:

- `Renderer`
- `OverlayManager`
- `ViewportController`

**Operacje**:

- Renderowanie 2D grid
- Wyświetlanie overlays (Skeleton, Structural features)
- Real-time preview
- Asynchroniczne aktualizacje

**Interfejsy**:

```
IRenderer
  - RenderWorkspace(workspace): void
  - RenderGrid(grid): void
  - RenderOverlay(overlay): void

IOverlayManager
  - ShowSkeleton(skeleton): void
  - ShowStructuralFeatures(features): void
  - HideOverlay(type): void

IAsyncTaskManager
  - RunAsync<T>(task): Task<T>
  - ShowProgress(task): void
```

---

## 🔄 Domain Events

### Editing Events

```
WorkspaceCreated { workspaceId, size, timestamp }
WorkspaceLoaded { workspaceId, path, timestamp }
WorkspaceSaved { workspaceId, path, timestamp }
SquarePlaced { x, y, squareType, groupId }
EntityPlaced { x, y, entityType, groupId }
PresetPlaced { x, y, presetId }
ElementErased { x, y }
GroupSwitched { fromGroupId, toGroupId }
UndoPerformed { }
RedoPerformed { }
```

### Biometric Events

```
PreprocessingCompleted { imageId, result }
FragmentationCompleted { imageId, regions }
UL22Generated { sourceId, ul22 }
SkeletonGenerated { sourceId, skeleton, algorithm }
BranchDetectionCompleted { skeletonId, features }
StructuralFeaturesDetected { features }
GridFeaturesCreated { workspaceId, features }
```

### Tool Events

```
ToolActivated { toolType }
FillToolAreaComputed { x, y, canvas }
```

### System Events

```
RealTimePreviewUpdated { }
AsyncTaskStarted { taskId, description }
AsyncTaskCompleted { taskId, result }
```

---

## 📦 Domain Aggregates

### 1. Map Aggregate

**Root**: `Workspace`

**Struktura**:

```
Workspace (Entity)
├── Id: Guid
├── Name: string
├── Size: Size (Value Object)
│   ├── Width: int
│   └── Height: int
├── Grid: Grid2D (Entity)
│   └── Cells: Cell[Width, Height]
├── GridBorder: GridBorder (Value Object)
├── Groups: List<Group> (Entity)
├── Metadata: WorkspaceMetadata (Value Object)
│   ├── Author: string
│   ├── CreatedAt: DateTime
│   └── ModifiedAt: DateTime
├── UL22: UL22 (Value Object, optional)
└── GridFeatures: GridFeatures (Value Object, optional)
```

**Invarianty**:

- Size.Width > 0 && Size.Height > 0
- Grid.Cells.Length == Width \* Height
- Każda Cell może zawierać max 1 Element
- GridBorder musi mieścić się w Grid

---

### 2. Layer Aggregate

**Root**: `Group`

**Struktura**:

```
Group (Entity)
├── Id: Guid
├── Name: string
├── IsVisible: bool
├── IsActive: bool
└── Elements: List<Element>
    ├── Square (Entity)
    │   ├── Position: Point (Value Object)
    │   ├── Type: SquareType (Enum)
    │   └── Rotation: int
    └── Entity (Entity)
        ├── Position: Point (Value Object)
        └── Type: EntityType (Enum)
```

**Invarianty**:

- Dokładnie jeden Group jest Active w danym czasie
- Position każdego Element musi być w granicach Grid

---

### 3. Preset Aggregate

**Root**: `Preset`

**Struktura**:

```
Preset (Entity)
├── Id: Guid
├── Name: string
├── OriginPoint: Point (Value Object)
└── Layout: PresetLayout (Value Object)
    └── Squares: List<SquareDefinition>
        ├── RelativePosition: Point
        ├── Type: SquareType
        └── Rotation: int
```

---

### 4. Biometrics Aggregate

**Root**: `BiometricProcessingSession`

**Struktura**:

```
BiometricProcessingSession (Entity)
├── Id: Guid
├── SourceId: Guid (Workspace lub Image)
├── PreprocessingParams: PreprocessingParams (Value Object)
│   ├── NoiseRemoval: bool
│   ├── FilterType: FilterType
│   └── BinarizationThreshold: int
├── FragmentationParams: FragmentationParams (Value Object)
│   ├── Method: FragmentationMethod
│   ├── Threshold: float
│   └── MinRegionSize: int
├── FragmentationResult: List<Region> (Value Object)
├── UL22: UL22 (Value Object)
├── Skeleton: Skeleton (Value Object)
│   ├── Algorithm: string
│   └── Points: List<Point>
├── StructuralFeatures: List<StructuralFeature> (Value Object)
│   ├── Position: Point
│   ├── Type: FeatureType (Endpoint, Bifurcation, Crossing)
│   └── Neighbors: List<Point>
└── GridFeatures: GridFeatures (Value Object)
    ├── MinutiaCount: int
    ├── BifurcationCount: int
    ├── EndpointCount: int
    └── Complexity: float
```

---

## 🧱 Warstwa Implementacji

### Clean Architecture Layers

```
┌─────────────────────────────────────┐
│     Presentation Layer (UI)         │
│  - ViewModels                       │
│  - Views                            │
│  - Controllers                      │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│     Application Layer               │
│  - Use Cases                        │
│  - Application Services             │
│  - DTOs                             │
│  - Command/Query Handlers           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│     Domain Layer                    │
│  - Aggregates                       │
│  - Entities                         │
│  - Value Objects                    │
│  - Domain Events                    │
│  - Domain Services                  │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│     Infrastructure Layer            │
│  - Repositories                     │
│  - File System Access               │
│  - Serialization                    │
│  - Algorithm Implementations        │
└─────────────────────────────────────┘
```

---

## 🔌 Dependency Injection

### Container Setup

```
// Domain Services
container.Register<IEditingService, EditingService>();
container.Register<IUL22Converter, UL22Converter>();
container.Register<IPreprocessingService, PreprocessingService>();
container.Register<IFragmentationService, FragmentationService>();
container.Register<ISkeletonizationService, SkeletonizationService>();
container.Register<IBranchDetectionService, BranchDetectionService>();
container.Register<IGridFeaturesService, GridFeaturesService>();

// Repositories
container.Register<IWorkspaceRepository, WorkspaceFileRepository>();
container.Register<IPresetRepository, PresetFileRepository>();

// Tools
container.Register<IToolManager, ToolManager>();
container.Register<ITool, SquareTool>("SquareTool");
container.Register<ITool, FillTool>("FillTool");
container.Register<ITool, EntityTool>("EntityTool");
container.Register<ITool, ElementRemover>("ElementRemover");

// Infrastructure
container.Register<IRenderer, Renderer>();
container.Register<IAsyncTaskManager, AsyncTaskManager>();
container.Register<IUndoRedoManager, UndoRedoManager>();
```

---

## 📊 Data Flow

### Przykład: Umieszczanie Square

```
1. User kliknie na Grid
   ↓
2. SquareTool.OnClick(x, y)
   ↓
3. EditingService.PlaceSquare(x, y, activeSquareType)
   ↓
4. Workspace.PlaceSquare(x, y, square)
   ↓
5. Domain Event: SquarePlaced(x, y, squareType, groupId)
   ↓
6. EventHandler → Renderer.RenderWorkspace()
   ↓
7. UI Update (real-time preview)
```

### Przykład: Skeletonization Pipeline

```
1. User wybiera "Run Skeletonization"
   ↓
2. AsyncTaskManager.RunAsync(() => {
     a. UL22Converter.ConvertWorkspace(workspace)
     b. PreprocessingService.RemoveNoise(ul22)
     c. SkeletonizationService.Skeletonize(ul22, algorithm)
     d. BranchDetectionService.DetectFeatures(skeleton)
   })
   ↓
3. Domain Events:
   - UL22Generated
   - PreprocessingCompleted
   - SkeletonGenerated
   - BranchDetectionCompleted
   ↓
4. EventHandlers → Update UI
   - OverlayManager.ShowSkeleton(skeleton)
   - OverlayManager.ShowStructuralFeatures(features)
```

---

## 🧪 Testowanie

### Poziomy testów

#### 1. Unit Tests (Algorytmy biometryczne)

```
- PreprocessingServiceTests
- FragmentationServiceTests
- SkeletonizationServiceTests
- BranchDetectionServiceTests
```

#### 2. Integration Tests

```
- Workspace Creation → Save → Load
- Image Import → Fragmentation → Preset Creation
- Square Placement → Skeletonization → Feature Detection
```

#### 3. End-to-End Tests

```
- Pełny workflow edycji mapy
- Import obrazu → analiza → edycja → export
```

### Test Data

```
tests/
├── testdata/
│   ├── images/
│   │   ├── simple_pattern.png
│   │   ├── complex_map.png
│   │   └── noisy_image.png
│   ├── workspaces/
│   │   ├── empty.workspace
│   │   └── example.workspace
│   └── expected_results/
│       ├── simple_pattern_skeleton.json
│       └── complex_map_features.json
```

---

## 📁 Struktura Katalogów Kodu

```
src/
├── Domain/
│   ├── Editing/
│   │   ├── Entities/
│   │   │   ├── Workspace.cs
│   │   │   ├── Grid2D.cs
│   │   │   ├── Group.cs
│   │   │   ├── Square.cs
│   │   │   └── Entity.cs
│   │   ├── ValueObjects/
│   │   │   ├── Point.cs
│   │   │   ├── Size.cs
│   │   │   └── GridBorder.cs
│   │   ├── Events/
│   │   │   └── EditingEvents.cs
│   │   └── Services/
│   │       └── IEditingService.cs
│   ├── Biometric/
│   │   ├── ValueObjects/
│   │   │   ├── UL22.cs
│   │   │   ├── Skeleton.cs
│   │   │   ├── StructuralFeature.cs
│   │   │   └── GridFeatures.cs
│   │   ├── Services/
│   │   │   ├── IUL22Converter.cs
│   │   │   ├── IPreprocessingService.cs
│   │   │   ├── IFragmentationService.cs
│   │   │   ├── ISkeletonizationService.cs
│   │   │   └── IBranchDetectionService.cs
│   │   └── Events/
│   │       └── BiometricEvents.cs
│   ├── Prefab/
│   │   ├── Entities/
│   │   │   └── Preset.cs
│   │   ├── ValueObjects/
│   │   │   └── PresetLayout.cs
│   │   └── Services/
│   │       └── IPresetService.cs
│   └── Shared/
│       └── Enums/
│           ├── SquareType.cs
│           ├── EntityType.cs
│           └── FeatureType.cs
├── Application/
│   ├── UseCases/
│   │   ├── CreateWorkspace/
│   │   ├── SaveWorkspace/
│   │   ├── PlaceSquare/
│   │   ├── RunSkeletonization/
│   │   └── ImportImage/
│   ├── DTOs/
│   └── Services/
│       ├── EditingService.cs
│       └── BiometricPipelineService.cs
├── Infrastructure/
│   ├── Repositories/
│   │   ├── WorkspaceFileRepository.cs
│   │   └── PresetFileRepository.cs
│   ├── Algorithms/
│   │   ├── Preprocessing/
│   │   │   ├── NoiseRemoval.cs
│   │   │   └── Binarization.cs
│   │   ├── Fragmentation/
│   │   │   ├── ConnectedComponents.cs
│   │   │   └── RegionGrowing.cs
│   │   ├── Skeletonization/
│   │   │   ├── ZhangSuen.cs
│   │   │   └── MorphologicalThinning.cs
│   │   └── BranchDetection/
│   │       └── CrossingNumber.cs
│   └── Serialization/
│       └── WorkspaceSerializer.cs
├── Presentation/
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   ├── WorkspaceView.xaml
│   │   └── ToolbarView.xaml
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   ├── WorkspaceViewModel.cs
│   │   └── ToolbarViewModel.cs
│   └── Converters/
└── DependencyInjection/
    └── ContainerConfig.cs
```

---

## 🎨 Wzorce Projektowe

### 1. Repository Pattern

- Abstrakcja dostępu do danych
- `IWorkspaceRepository`, `IPresetRepository`

### 2. Strategy Pattern

- Wybór algorytmu skeletonization
- `ISkeletonizationStrategy`

### 3. Command Pattern

- Undo/Redo
- `ICommand`, `CommandManager`

### 4. Observer Pattern

- Domain Events
- Event Handlers

### 5. Factory Pattern

- Tworzenie Tools
- `IToolFactory`

### 6. Dependency Injection

- Inversion of Control
- IoC Container

---

## 🚀 Deployment

### Wymagania Runtime

- .NET 6+ / Python 3.9+ / Java 11+ (w zależności od technologii)
- GUI Framework (WPF / Electron / JavaFX)
- Min 4GB RAM
- Obsługa plików PNG/JPG

### Build Process

1. Kompilacja projektu
2. Uruchomienie testów jednostkowych
3. Package aplikacji
4. Tworzenie instalatora (opcjonalne)

---

## 📚 Słownik Techniczny

**UL22**: Uproszczona binarna reprezentacja 2D grid. Każdy piksel to 0 (tło) lub 1 (obiekt).

**Skeletonization**: Algorytm morfologiczny redukujący obiekt do jednopikselowego szkieletu przy zachowaniu topologii.

**Branch Detection**: Algorytm analizujący szkielet używając Crossing Number (CN) do klasyfikacji punktów:

- CN = 1: Endpoint (końcówka)
- CN = 2: Zwykły punkt szkieletu
- CN = 3: Bifurcation (rozgałęzienie)
- CN ≥ 4: Crossing (skrzyżowanie)

**Fragmentation**: Segmentacja obrazu na regiony o podobnych właściwościach (Connected Component Analysis, Region Growing).

**Grid Features**: Zestaw liczb opisujących Workspace:

- Liczba bifurkacji
- Liczba końcówek
- Złożoność strukturalna
- Średnia długość gałęzi

---

## 🔒 Bezpieczeństwo i Wydajność

### Wydajność

- Algorytmy biometryczne na dużych mapach: **asynchroniczne**
- Cache dla często używanych operacji
- Lazy loading dla overlays
- Throttling dla real-time preview

### Bezpieczeństwo Danych

- Walidacja rozmiaru Grid
- Limity rozmiaru pliku
- Obsługa błędów przy wczytywaniu/zapisie

---

**Ostatnia aktualizacja**: 2026-01-11
