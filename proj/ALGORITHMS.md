# Algorytmy Biometryczne - Szczegółowy Opis

## 📖 Przegląd

Ten dokument zawiera szczegółowe opisy algorytmów biometrycznych wymaganych w projekcie. **UWAGA**: Wszystkie algorytmy muszą być zaimplementowane **bez użycia zewnętrznych bibliotek** biometrycznych.

---

## 1️⃣ UL22 - Konwersja do Reprezentacji Binarnej

### Cel

Przekształcenie Workspace lub obrazu RGB do uproszczonej binarnej reprezentacji używanej jako wejście dla algorytmów biometrycznych.

### Opis

UL22 to dwuwymiarowa macierz binarna gdzie:

- **0** = tło (Background)
- **1** = obiekt (Map/Square)

### Algorytm Konwersji

#### A. Z Workspace do UL22

```
Input: Workspace (2D grid z Square)
Output: UL22 (binarna macierz)

Algorithm:
1. Utwórz macierz [Height x Width] wypełnioną zerami
2. For każdej Cell w Grid:
   a. Jeśli Cell zawiera Square:
      - UL22[Cell.Y][Cell.X] = 1
   b. W przeciwnym razie:
      - UL22[Cell.Y][Cell.X] = 0
3. Return UL22
```

#### B. Z Obrazu RGB do UL22

```
Input: Image (RGB bitmap)
Output: UL22 (binarna macierz)

Algorithm:
1. Konwersja do grayscale:
   Gray[y][x] = 0.299*R + 0.587*G + 0.114*B

2. Binaryzacja (threshold T):
   For każdego piksela (x, y):
     If Gray[y][x] > T:
       UL22[y][x] = 1  // Jasny = obiekt
     Else:
       UL22[y][x] = 0  // Ciemny = tło

3. Return UL22
```

### Przykład

```
Workspace:    UL22:
□ □ ■ □       0 0 1 0
□ ■ ■ ■   →   0 1 1 1
□ □ ■ □       0 0 1 0
```

### Implementacja

- Funkcja: `ConvertWorkspaceToUL22(Workspace): UL22`
- Funkcja: `ConvertImageToUL22(Image, threshold): UL22`
- Testy: Sprawdzić na małych przykładach (3x3, 5x5)

---

## 2️⃣ Preprocessing - Przygotowanie Obrazu

### Cel

Poprawa jakości obrazu przed analizą biometryczną poprzez usunięcie szumu i wzmocnienie kontrastu.

### A. Usuwanie Szumu - Filtr Medianowy

#### Opis

Każdy piksel zastępowany przez medianę z jego sąsiedztwa (skuteczny na szum impulsowy).

#### Algorytm

```
Input: Image (grayscale lub binary), KernelSize (3, 5, 7...)
Output: FilteredImage

Algorithm:
1. For każdego piksela (x, y):
   a. Zbierz wartości z sąsiedztwa KernelSize × KernelSize
   b. Posortuj wartości
   c. Wybierz medianę (środkową wartość)
   d. FilteredImage[y][x] = median

2. Obsłuż brzegi (padding lub ignore)
3. Return FilteredImage
```

#### Przykład (3x3)

```
Przed:           Sąsiedztwo:      Po:
10 15 90         [10,15,20,       15
20 15 25    →     15,15,20,   →
30 35 40          25,30,35]
```

### B. Binaryzacja - Metoda Otsu

#### Opis

Automatyczne obliczanie optymalnego progu binaryzacji maksymalizując wariancję międzyklasową.

#### Algorytm

```
Input: GrayscaleImage
Output: BinaryImage, OptimalThreshold

Algorithm:
1. Oblicz histogram intensywności [0..255]

2. For każdego możliwego threshold T (0..255):
   a. Podziel piksele na 2 klasy:
      - C0: piksele < T (tło)
      - C1: piksele ≥ T (obiekt)

   b. Oblicz wagi klas:
      w0 = liczba_pikseli(C0) / total_pixels
      w1 = liczba_pikseli(C1) / total_pixels

   c. Oblicz średnie intensywności:
      μ0 = średnia(C0)
      μ1 = średnia(C1)

   d. Wariancja międzyklasowa:
      σ²(T) = w0 * w1 * (μ0 - μ1)²

3. OptimalThreshold = T, dla którego σ²(T) jest maksymalne

4. Zastosuj threshold:
   For każdego piksela (x, y):
     BinaryImage[y][x] = (GrayscaleImage[y][x] >= OptimalThreshold) ? 1 : 0

5. Return BinaryImage, OptimalThreshold
```

### C. Filtr Gaussowski (opcjonalny)

```
Gaussian Kernel 3x3 (σ=1):
1/16 * [1  2  1]
       [2  4  2]
       [1  2  1]

For każdego piksela (x,y):
  Suma = 0
  For każdego punktu (i,j) w kernel:
    Suma += Image[y+j][x+i] * Kernel[j][i]
  FilteredImage[y][x] = Suma
```

---

## 3️⃣ Fragmentation - Segmentacja Regionów

### Cel

Podział obrazu na spójne regiony (Map vs Background) lub określenie Canvas dla Fill Tool.

### A. Connected Component Labeling (4-connectivity)

#### Opis

Znajduje wszystkie spójne regiony pikseli o tej samej wartości.

#### Algorytm - Two-Pass

```
Input: BinaryImage (UL22)
Output: LabeledImage (każdy region ma unikalny label)

Algorithm:
Pass 1 - Provisional Labeling:
1. NextLabel = 1
2. Equivalence = {} // słownik równoważnych labeli

3. For każdego piksela (x, y) od góry do dołu, lewo do prawo:
   a. Jeśli BinaryImage[y][x] == 0: skip (tło)

   b. Sprawdź sąsiadów (4-connectivity):
      - Left:  LabeledImage[y][x-1]
      - Top:   LabeledImage[y-1][x]

   c. Jeśli obaj sąsiedzi == 0:
      - LabeledImage[y][x] = NextLabel
      - NextLabel++

   d. Jeśli jeden sąsiad ma label:
      - LabeledImage[y][x] = ten label

   e. Jeśli obaj mają label:
      - LabeledImage[y][x] = min(left_label, top_label)
      - Equivalence[max_label] = min_label

Pass 2 - Resolve Equivalences:
4. For każdego piksela (x, y):
   a. Jeśli LabeledImage[y][x] > 0:
      - Zastąp labelą najmniejszą równoważną z Equivalence

5. Return LabeledImage, NumberOfRegions
```

#### Przykład

```
Binary:          Pass 1:         Pass 2:
1 1 0 1 1        1 1 0 2 2       1 1 0 2 2
1 0 0 0 1   →    1 0 0 0 2   →   1 0 0 0 2
0 0 1 1 1        0 0 3 3 3       0 0 3 3 3

Regions: {1, 2, 3}
```

### B. Region Growing (dla Fill Tool)

#### Opis

Znajduje spójny obszar (Canvas) startując z punktu (x, y) i rozszerzając na sąsiadów o tym samym SquareType.

#### Algorytm - Flood Fill

```
Input: Grid, StartX, StartY, TargetType
Output: Canvas (lista punktów w tym regionie)

Algorithm:
1. Canvas = []
2. Queue = [(StartX, StartY)]
3. Visited = set()
4. TargetType = Grid[StartY][StartX].Type

5. While Queue not empty:
   a. (x, y) = Queue.dequeue()

   b. Jeśli (x, y) w Visited: skip
   c. Visited.add((x, y))

   d. Jeśli Grid[y][x].Type != TargetType: skip

   e. Canvas.add((x, y))

   f. Dodaj sąsiadów do Queue (4-connectivity):
      - (x+1, y), (x-1, y), (x, y+1), (x, y-1)

6. Return Canvas
```

---

## 4️⃣ Skeletonization - Szkieletyzacja

### Cel

Przekształcenie obiektu binarnego w jednopikselowy szkielet zachowując topologię.

### A. Zhang-Suen Algorithm (Podstawowy)

#### Właściwości

- Iteracyjny algorytm thinning
- 2 sub-iterations na iterację
- Zachowuje łączność

#### Algorytm

```
Input: BinaryImage (1 = obiekt, 0 = tło)
Output: Skeleton

Funkcje pomocnicze:
A(P1) = liczba przejść 0→1 w 8-sąsiedztwie P1 (clockwise)
B(P1) = liczba sąsiadów P1 z wartością 1

Sąsiedztwo (numbered clockwise):
P9 P2 P3
P8 P1 P4
P7 P6 P5

Algorithm:
1. Changed = true
2. While Changed:
   Changed = false

   // Sub-iteration 1
   a. For każdego piksela P1 == 1:
      Warunki:
      (1) 2 ≤ B(P1) ≤ 6
      (2) A(P1) = 1
      (3) P2 * P4 * P6 = 0
      (4) P4 * P6 * P8 = 0

      Jeśli wszystkie spełnione:
        - Oznacz P1 do usunięcia

   b. Usuń oznaczone piksele
      Jeśli coś usunięto: Changed = true

   // Sub-iteration 2
   c. For każdego piksela P1 == 1:
      Warunki:
      (1) 2 ≤ B(P1) ≤ 6
      (2) A(P1) = 1
      (3) P2 * P4 * P8 = 0
      (4) P2 * P6 * P8 = 0

      Jeśli wszystkie spełnione:
        - Oznacz P1 do usunięcia

   d. Usuń oznaczone piksele
      Jeśli coś usunięto: Changed = true

3. Return BinaryImage (skeleton)
```

#### Przykład

```
Oryginał:        Skeleton:
■ ■ ■ ■ ■        □ □ □ □ □
■ ■ ■ ■ ■   →    □ □ ■ □ □
■ ■ ■ ■ ■        □ □ □ □ □
```

### B. Morphological Thinning (Alternatywny)

#### Opis

Erozja warunkowa używając strukturalnych elementów.

#### Algorytm

```
Input: BinaryImage
Output: Skeleton

Strukturalne elementy (8 rotacji dla każdej):
Hit-or-Miss patterns dla detekcji pikseli do usunięcia

Algorithm:
1. Changed = true
2. While Changed:
   Changed = false

   For każdego structural element SE:
     a. Znajdź piksele pasujące do SE (Hit-or-Miss transform)
     b. Usuń te piksele jeśli nie zepsuje to łączności
     c. Jeśli coś usunięto: Changed = true

3. Return BinaryImage
```

---

## 5️⃣ Branch Detection - Detekcja Punktów Charakterystycznych

### Cel

Identyfikacja końcówek, bifurkacji i skrzyżowań na szkielecie.

### Crossing Number (CN) Method

#### Opis

Klasyfikacja punktu na podstawie liczby przejść 0→1 w jego 8-sąsiedztwie.

#### Algorytm

```
Input: Skeleton (binary image)
Output: List<StructuralFeature>

Funkcja CN(x, y):
  // 8-sąsiedztwo clockwise
  N = [P2, P3, P4, P5, P6, P7, P8, P9, P2]  // P2 powtórzone na końcu

  CN = 0
  For i = 0 to 7:
    If N[i] == 0 AND N[i+1] == 1:
      CN++

  Return CN

Algorithm:
1. Features = []

2. For każdego piksela (x, y) w Skeleton:
   a. Jeśli Skeleton[y][x] == 0: skip

   b. CN_value = CN(x, y)

   c. Klasyfikacja:
      If CN_value == 1:
        Type = Endpoint (końcówka)
      Else If CN_value == 2:
        Type = Normal (zwykły punkt szkieletu)
      Else If CN_value == 3:
        Type = Bifurcation (rozgałęzienie)
      Else If CN_value >= 4:
        Type = Crossing (skrzyżowanie)

   d. Jeśli Type != Normal:
      Features.add({Position: (x,y), Type: Type})

3. Return Features
```

#### Przykład

```
Skeleton:          CN Values:
□ ■ □              □ 1 □        <- Endpoint
□ ■ □              □ 2 □        <- Normal
□ ■ ■     →        □ 3 2        <- Bifurcation
□ □ ■              □ □ 1        <- Endpoint

Features:
- (1, 0): Endpoint
- (1, 2): Bifurcation
- (2, 3): Endpoint
```

### Rozszerzenie: Kierunki w Bifurkacjach

```
Dla każdej bifurkacji/skrzyżowania:
1. Znajdź sąsiadów które są częścią szkieletu
2. Oblicz kąty między nimi
3. Użyj do automatycznej rotacji Square

GetNeighborDirections(x, y, Skeleton):
  Directions = []

  For każdego z 8 sąsiadów (dx, dy):
    If Skeleton[y+dy][x+dx] == 1:
      Angle = atan2(dy, dx) * 180 / π
      Directions.add(Angle)

  Return Directions
```

---

## 6️⃣ Grid Features - Numeryczna Reprezentacja

### Cel

Stworzenie liczbowej reprezentacji Workspace do porównania złożoności map.

### Algorytm Obliczania Grid Features

```
Input: Workspace (z wyliczonymi Structural Features)
Output: GridFeatures

Algorithm:
1. Policz typy features:
   EndpointCount = liczba(Endpoint)
   BifurcationCount = liczba(Bifurcation)
   CrossingCount = liczba(Crossing)
   TotalMinutiae = EndpointCount + BifurcationCount + CrossingCount

2. Oblicz gęstość:
   GridArea = Width * Height
   MinutiaDensity = TotalMinutiae / GridArea

3. Oblicz złożoność szkieletu:
   SkeletonPixels = liczba(Skeleton == 1)
   SkeletonDensity = SkeletonPixels / GridArea

4. Oblicz średnią długość gałęzi:
   Branches = ExtractBranches(Skeleton, Features)
   AvgBranchLength = średnia(Branch.Length for Branch in Branches)

5. GridFeatures = {
     EndpointCount: EndpointCount,
     BifurcationCount: BifurcationCount,
     CrossingCount: CrossingCount,
     TotalMinutiae: TotalMinutiae,
     MinutiaDensity: MinutiaDensity,
     SkeletonDensity: SkeletonDensity,
     AvgBranchLength: AvgBranchLength,
     Complexity: MinutiaDensity * SkeletonDensity
   }

6. Return GridFeatures
```

### Porównywanie Map

```
CompareMaps(Features1, Features2):
  // Euclidean distance w przestrzeni features

  Normalize features:
  F1_norm = NormalizeVector(Features1)
  F2_norm = NormalizeVector(Features2)

  Distance = sqrt(
    (F1.MinutiaDensity - F2.MinutiaDensity)² +
    (F1.SkeletonDensity - F2.SkeletonDensity)² +
    ...
  )

  Similarity = 1 / (1 + Distance)

  Return Similarity  // [0, 1], 1 = identyczne
```

---

## 🧪 Testowanie Algorytmów

### Test Cases

#### 1. UL22 Conversion

```python
TestSimpleConversion():
  Workspace = CreateWorkspace(3, 3)
  PlaceSquare(Workspace, 1, 1)

  UL22 = ConvertToUL22(Workspace)

  Expected = [
    [0, 0, 0],
    [0, 1, 0],
    [0, 0, 0]
  ]

  Assert(UL22 == Expected)
```

#### 2. Skeletonization

```python
TestSimpleSkeleton():
  Input = [
    [1, 1, 1],
    [1, 1, 1],
    [1, 1, 1]
  ]

  Skeleton = ZhangSuen(Input)

  Expected = [
    [0, 0, 0],
    [0, 1, 0],
    [0, 0, 0]
  ]

  Assert(Skeleton == Expected)
```

#### 3. Branch Detection

```python
TestBranchDetection():
  Skeleton = [
    [0, 1, 0],
    [0, 1, 0],
    [0, 1, 1],
    [0, 0, 1]
  ]

  Features = DetectBranches(Skeleton)

  Expected = [
    {Position: (1,0), Type: Endpoint},
    {Position: (1,2), Type: Bifurcation},
    {Position: (2,3), Type: Endpoint}
  ]

  Assert(Features == Expected)
```

#### 4. Fragmentation

```python
TestFragmentation():
  Image = [
    [1, 1, 0, 0],
    [1, 0, 0, 1],
    [0, 0, 1, 1]
  ]

  Regions = ConnectedComponents(Image)

  Expected = 3 regions
  Assert(len(Regions) == 3)
```

---

## 📊 Metryki Wydajności

### Complexity Analysis

- **UL22 Conversion**: O(W × H) - liniowa względem rozmiaru
- **Median Filter**: O(W × H × K²) - K = kernel size
- **Connected Components**: O(W × H) - two-pass algorithm
- **Zhang-Suen**: O(W × H × I) - I = liczba iteracji (zazwyczaj < 20)
- **Branch Detection**: O(W × H) - single pass

### Zalecane Optymalizacje

1. **Parallel processing** dla dużych obrazów
2. **Caching** dla wielokrotnie używanych wyników
3. **ROI (Region of Interest)** - przetwarzaj tylko zmieniony fragment
4. **Asynchroniczne** wykonanie dla operacji > 100ms

---

## 📚 Literatura

### Skeletonization

- Zhang, T.Y., Suen, C.Y. (1984). "A fast parallel algorithm for thinning digital patterns"
- Lam, L., Lee, S-W., Suen, C.Y. (1992). "Thinning Methodologies - A Comprehensive Survey"

### Branch Detection

- Rutovitz, D. (1966). "Pattern recognition"
- Arcelli, C., Baja, G.S. (1993). "A width-independent fast thinning algorithm"

### Fragmentation

- Rosenfeld, A., Pfaltz, J.L. (1966). "Sequential operations in digital picture processing"
- Otsu, N. (1979). "A threshold selection method from gray-level histograms"

---

## 💡 Wskazówki Implementacyjne

1. **Zacznij od małych przykładów** (3×3, 5×5) do testowania
2. **Wizualizuj pośrednie wyniki** każdego kroku
3. **Używaj asserts** do walidacji invariantów
4. **Testuj brzegowe przypadki** (puste, pełne, pojedynczy piksel)
5. **Dokumentuj założenia** (4-connectivity vs 8-connectivity)
6. **Profiluj wydajność** na większych mapach (100×100+)

---

**PAMIĘTAJ**: Wszystkie algorytmy muszą być zaimplementowane **od podstaw** bez użycia bibliotek biometrycznych!
