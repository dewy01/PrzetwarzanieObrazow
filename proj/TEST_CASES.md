# Przykładowe Testy Algorytmów

## Test Cases dla Implementacji

Ten plik zawiera konkretne przykłady testowe dla każdego algorytmu. Użyj ich do weryfikacji poprawności implementacji.

---

## 1. UL22 Conversion Tests

### Test 1.1: Simple 3x3 Workspace

```
Input Workspace (3x3):
□ □ □
□ ■ □
□ □ □

Expected UL22:
[[0, 0, 0],
 [0, 1, 0],
 [0, 0, 0]]
```

### Test 1.2: Line Pattern

```
Input Workspace (5x3):
□ ■ ■ ■ □
□ □ □ □ □
□ □ □ □ □

Expected UL22:
[[0, 1, 1, 1, 0],
 [0, 0, 0, 0, 0],
 [0, 0, 0, 0, 0]]
```

### Test 1.3: L-Shape

```
Input Workspace (4x4):
■ □ □ □
■ □ □ □
■ ■ ■ □
□ □ □ □

Expected UL22:
[[1, 0, 0, 0],
 [1, 0, 0, 0],
 [1, 1, 1, 0],
 [0, 0, 0, 0]]
```

---

## 2. Preprocessing Tests

### Test 2.1: Median Filter 3x3

```
Input (noisy image):
[[10, 15, 90],
 [20, 15, 25],
 [30, 35, 40]]

Expected Output (after median filter):
[[10, 15, 25],
 [20, 20, 25],
 [30, 30, 35]]

Center pixel: sorted [10,15,15,20,25,30,35,40,90] → median = 25
```

### Test 2.2: Otsu Binarization

```
Input (grayscale, histogram bimodal):
[[100, 110, 105, 200, 210],
 [95,  100, 200, 205, 195],
 [105, 98,  190, 200, 210]]

Expected:
- Optimal Threshold ≈ 150
- Binary Output:
  [[0, 0, 0, 1, 1],
   [0, 0, 1, 1, 1],
   [0, 0, 1, 1, 1]]
```

---

## 3. Fragmentation Tests

### Test 3.1: Two Separate Regions

```
Input (binary):
[[1, 1, 0, 0, 0],
 [1, 0, 0, 1, 1],
 [0, 0, 0, 1, 0]]

Expected Labels:
[[1, 1, 0, 0, 0],
 [1, 0, 0, 2, 2],
 [0, 0, 0, 2, 0]]

Number of regions: 2
Region 1: {(0,0), (1,0), (0,1)}
Region 2: {(3,1), (4,1), (3,2)}
```

### Test 3.2: Complex Shape

```
Input:
[[1, 1, 0, 1, 1],
 [0, 1, 0, 1, 0],
 [0, 1, 1, 1, 0],
 [0, 0, 0, 0, 0]]

Expected:
- Region 1 (connected through diagonal): all 1s form one region (4-connectivity)
OR
- Region 1: top-left, Region 2: top-right, Region 3: bottom (zależy od connectivity)

4-connectivity → 2 regions
8-connectivity → 1 region
```

### Test 3.3: Flood Fill (Canvas Detection)

```
Grid with SquareTypes:
A A A B B
A A A B B
C C A B B
C C A A A

Start: (0, 0), TargetType: A
Expected Canvas: {(0,0), (1,0), (2,0), (0,1), (1,1), (2,1), (2,2)}
```

---

## 4. Skeletonization Tests

### Test 4.1: Simple Rectangle

```
Input (3x5 rectangle):
[[1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1]]

Expected Skeleton:
[[0, 0, 0, 0, 0],
 [1, 1, 1, 1, 1],
 [0, 0, 0, 0, 0]]

(single line through center)
```

### Test 4.2: Square

```
Input (5x5 filled):
[[1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1]]

Expected Skeleton:
[[0, 0, 0, 0, 0],
 [0, 0, 0, 0, 0],
 [0, 0, 1, 0, 0],
 [0, 0, 0, 0, 0],
 [0, 0, 0, 0, 0]]

(single central point)
```

### Test 4.3: L-Shape

```
Input:
[[1, 0, 0, 0],
 [1, 0, 0, 0],
 [1, 1, 1, 0],
 [0, 0, 0, 0]]

Expected Skeleton:
[[1, 0, 0, 0],
 [1, 0, 0, 0],
 [1, 1, 1, 0],
 [0, 0, 0, 0]]

(already thin, no change)
```

### Test 4.4: T-Shape

```
Input (thick T):
[[1, 1, 1, 1, 1],
 [1, 1, 1, 1, 1],
 [0, 0, 1, 0, 0],
 [0, 0, 1, 0, 0],
 [0, 0, 1, 0, 0]]

Expected Skeleton:
[[0, 0, 1, 0, 0],
 [1, 1, 1, 1, 1],
 [0, 0, 1, 0, 0],
 [0, 0, 1, 0, 0],
 [0, 0, 1, 0, 0]]
```

---

## 5. Branch Detection Tests

### Test 5.1: Single Endpoint

```
Skeleton:
[[0, 1, 0],
 [0, 1, 0],
 [0, 1, 0]]

Expected Features:
- (1, 0): Endpoint (CN=1)
- (1, 1): Normal (CN=2)
- (1, 2): Endpoint (CN=1)
```

### Test 5.2: Bifurcation (Y-shape)

```
Skeleton:
[[1, 0, 1],
 [0, 1, 0],
 [0, 1, 0],
 [0, 1, 0]]

Expected Features:
- (0, 0): Endpoint (CN=1)
- (2, 0): Endpoint (CN=1)
- (1, 1): Bifurcation (CN=3)
- (1, 3): Endpoint (CN=1)

Neighbors of (1,1):
N = [P2=1, P3=1, P4=0, P5=0, P6=1, P7=0, P8=0, P9=0, P2=1]
Transitions 0→1: at positions 0→1 and 5→6 and 8→0 = CN=3
```

### Test 5.3: Cross (4-way intersection)

```
Skeleton:
[[0, 1, 0],
 [1, 1, 1],
 [0, 1, 0]]

Expected Features:
- (1, 0): Endpoint (CN=1)
- (0, 1): Endpoint (CN=1)
- (1, 1): Crossing (CN=4)
- (2, 1): Endpoint (CN=1)
- (1, 2): Endpoint (CN=1)

Neighbors of (1,1):
N = [P2=1, P3=0, P4=1, P5=0, P6=1, P7=0, P8=1, P9=0, P2=1]
Transitions: 1→0→1 appears 4 times → CN=4
```

### Test 5.4: Complex Structure

```
Skeleton (real map example):
[[1, 1, 1, 0, 0],
 [0, 0, 1, 1, 0],
 [0, 0, 1, 0, 1],
 [0, 1, 1, 0, 0]]

Expected Features:
- (0, 0): Endpoint
- (1, 0): Normal
- (2, 0): Bifurcation (3 neighbors)
- (4, 2): Endpoint
- (1, 3): Endpoint
- etc.
```

---

## 6. Grid Features Tests

### Test 6.1: Simple Grid

```
Workspace (10x10) with skeleton:
- 4 Endpoints
- 2 Bifurcations
- 0 Crossings
- 25 skeleton pixels

Expected GridFeatures:
{
  EndpointCount: 4,
  BifurcationCount: 2,
  CrossingCount: 0,
  TotalMinutiae: 6,
  MinutiaDensity: 6/100 = 0.06,
  SkeletonDensity: 25/100 = 0.25,
  Complexity: 0.06 * 0.25 = 0.015
}
```

### Test 6.2: Comparison

```
Map A: {MinutiaDensity: 0.1, SkeletonDensity: 0.3, ...}
Map B: {MinutiaDensity: 0.15, SkeletonDensity: 0.35, ...}

Distance = sqrt((0.1-0.15)² + (0.3-0.35)²) = sqrt(0.0025 + 0.0025) = 0.071
Similarity = 1/(1+0.071) = 0.933

Maps are 93.3% similar
```

---

## 7. Integration Tests

### Test 7.1: Full Pipeline

```
1. Create Workspace (5x5)
2. Place Squares in L-shape
3. Convert to UL22
4. Run Skeletonization
5. Run Branch Detection
6. Compute Grid Features

Expected:
- UL22: L-shape binary
- Skeleton: L-shape (already thin)
- Features: 2 endpoints at ends of L
- GridFeatures: low complexity
```

### Test 7.2: Image Import

```
1. Load RGB image (simple 2-color pattern)
2. Binarize (Otsu)
3. Fragment (find regions)
4. Convert largest region to Preset
5. Place Preset in Workspace

Expected:
- Binarization: clean separation
- Fragmentation: correct number of regions
- Preset: matches original shape
```

---

## 8. Edge Cases & Error Handling

### Test 8.1: Empty Workspace

```
Input: 5x5 workspace, no squares
Expected UL22: all zeros
Expected Skeleton: all zeros
Expected Features: empty list
```

### Test 8.2: Full Workspace

```
Input: 5x5 workspace, all squares
Expected UL22: all ones
Expected Skeleton: center point or cross pattern
Expected Features: depends on algorithm
```

### Test 8.3: Single Pixel

```
Input: 3x3 workspace, one square at center
Expected UL22: center = 1, rest = 0
Expected Skeleton: center = 1, rest = 0 (no change)
Expected Features: isolated point (CN depends on definition)
```

### Test 8.4: Border Handling

```
Workspace with Square at edge:
■ ■ □
□ □ □
□ □ □

Median filter should handle border correctly (padding or ignore)
```

---

## 📝 Jak Używać Tych Testów

1. **Testy jednostkowe**: Każdy test powinien być osobną funkcją testową
2. **Assert equality**: Porównaj wynik z oczekiwanym
3. **Visual debugging**: Wyświetl wynik obok oczekiwanego dla łatwego porównania
4. **Tolerance**: Dla float (np. Grid Features) użyj tolerancji ±0.01

### Przykład Testu (pseudokod)

```python
def test_ul22_simple():
    # Arrange
    workspace = CreateWorkspace(3, 3)
    PlaceSquare(workspace, 1, 1)

    # Act
    ul22 = ConvertToUL22(workspace)

    # Assert
    expected = [[0, 0, 0],
                [0, 1, 0],
                [0, 0, 0]]
    assert ul22 == expected, f"Expected {expected}, got {ul22}"
```

---

**Uwaga**: Te testy są **minimalne**. Dodaj więcej przypadków testowych dla pełnego pokrycia!
