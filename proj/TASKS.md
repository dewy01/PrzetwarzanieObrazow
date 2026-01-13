# Lista Zadań do Realizacji

## 📊 Strategia Realizacji

### Priorytety

1. **Faza 1 (MVP)**: Podstawowa funkcjonalność edytora + jeden algorytm biometryczny (23+ pkt)
2. **Faza 2**: Rozszerzenie algorytmów biometrycznych (do 35+ pkt)
3. **Faza 3**: Zaawansowane funkcje i polish (do 56 pkt)

### Zalecany Porządek Realizacji

- Najpierw Minor US niezbędne do działania Major/Standard US
- Następnie Standard US dla infrastruktury
- Na końcu Major US wymagające pełnej infrastruktury

---

## 🎯 FAZA 1: MVP - Minimalna Funkcjonalność (23+ punkty)

### Cel: Podstawowy edytor + jeden algorytm biometryczny

#### Infrastruktura (8 pkt) ✅ UKOŃCZONE

- [x] **[Standard, 2 pkt]** US#1: Tworzenie Workspace ✅
- [x] **[Standard, 2 pkt]** US#2: Zapisywanie i wczytywanie Workspace ✅
- [x] **[Standard, 2 pkt]** US#16: Dependency Injection i IoC ✅
- [x] **[Minor, 1 pkt]** US#5: User-friendly interface ✅
- [x] **[Minor, 1 pkt]** US#1: Umieszczanie Square (Square tool) ✅

#### Algorytmy Biometryczne - Podstawy (7 pkt) ✅ UKOŃCZONE

- [x] **[Major, 3 pkt]** US#7: Testowanie Fragmentation niezależnie na obrazach ✅
- [x] **[Standard, 2 pkt]** US#11: Testy jednostkowe algorytmów biometrycznych ✅
- [x] **[Standard, 2 pkt]** US#12: Konwersja Workspace do UL22 ✅

#### Edycja (4/4 pkt) ✅ UKOŃCZONE

- [x] **[Minor, 1 pkt]** US#2: Usuwanie elementów (Element remover) ✅
- [x] **[Minor, 1 pkt]** US#3: Przełączanie między Group ✅
- [x] **[Minor, 1 pkt]** US#7: Wybór Square types ✅
- [x] **[Minor, 1 pkt]** US#10: Automatyczna detekcja Grid border ✅

#### Wizualizacja (2 pkt)

- [x] **[Standard, 2 pkt]** US#14: Real-time preview ✅

**Łącznie Faza 1: 21 punktów**

---

## 🚀 FAZA 2: Rozszerzone Algorytmy (35+ punktów)

### Cel: Pełne wsparcie algorytmów biometrycznych

#### Szkieletyzacja (8/8 pkt) ✅ UKOŃCZONE

- [x] **[Major, 3 pkt]** US#2: Skeletonization na Background (Zhang-Suen) ✅
- [x] **[Major, 3 pkt]** US#6: Skeletonization na Square ✅
- [x] **[Standard, 2 pkt]** US#4: Co najmniej 2 algorytmy Skeletonization ✅ (Zhang-Suen + K3M)

#### Branch Detection (6/6 pkt) ✅ UKOŃCZONE

- [x] **[Major, 3 pkt]** US#3: Branch Detection Algorithm + automatyczna rotacja Square ✅
- [x] **[Minor, 1 pkt]** US#8: Podświetlanie Structural features ✅
- [x] **[Minor, 1 pkt]** US#13: Podświetlanie na podstawie Branch Detection ✅
- [x] **[Minor, 1 pkt]** US#11: Automatyczna adnotacja Workspace ✅

#### Fragmentacja i Preprocessing (6/7 pkt) ⚠️ CZĘŚCIOWO

- [x] **[Major, 3 pkt]** US#1: Import RGB + Fragmentation do Preset ✅
- [x] **[Major, 3 pkt]** US#5: Preprocessing (usuwanie szumu) ✅
- [x] **[Standard, 2 pkt]** US#3: Parametryzacja Fragmentation ✅ (Median 3x3, 5x5, 7x7)

#### Fill Tool (3 pkt) ✅ UKOŃCZONE

- [x] **[Major, 3 pkt]** US#4: Fill tool z użyciem Fragmentation ✅

**Łącznie Faza 2: +27 punktów (suma: 48 pkt)**

---

## ✨ FAZA 3: Zaawansowane Funkcje (56 punktów)

### Cel: Pełna funkcjonalność + Grid features

#### Presety (2 pkt)

- [x] **[Standard, 2 pkt]** US#5: Umieszczanie Preset ✅

#### Entities (1 pkt) ✅ UKOŃCZONE

- [x] **[Minor, 1 pkt]** US#6: Umieszczanie Entity (Entity tool) ✅

#### Grid Features (1 pkt) ✅ UKOŃCZONE

- [x] **[Minor, 1 pkt]** US#4: Tworzenie i wyświetlanie Grid features ✅

#### UX Improvements (2 pkt) ✅ UKOŃCZONE

- [x] **[Minor, 1 pkt]** US#9: Undo/Redo ✅
- [x] **[Minor, 1 pkt]** US#12: Synchronizacja danych w czasie rzeczywistym ✅

#### Developer Features (5 pkt)

- [x] **[Standard, 2 pkt]** US#13: Automatyczne wczytywanie domyślnego Workspace ✅
- [x] **[Standard, 2 pkt]** US#15: Asynchroniczne długie zadania ✅
- [ ] **[Minor, 1 pkt]** Dodatkowe testy i dokumentacja

**Łącznie Faza 3: +16 punktów (suma: 53 pkt)**

---

## 📋 Szczegółowe User Stories

### 🔴 Major User Stories (3 pkt każdy)

#### US#1: Import RGB + Fragmentation → Preset

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Import obrazu RGB
- Algorytm fragmentacji odróżniający Map od Background
- Konwersja wyniku do Preset
- Możliwość zapisania Preset

**Zależności**: US Standard #1, #2 (infrastruktura)

**Definicja Ukończenia**:

- [ ] Import pliku PNG/JPG
- [ ] Działający algorytm fragmentacji
- [ ] Konwersja fragmentu na Preset
- [ ] Zapisanie i wczytanie Preset

---

#### US#2: Skeletonization Background

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Algorytm szkieletyzacji
- Wykrywanie punktów charakterystycznych (skrzyżowania, bifurkacje)
- Wizualizacja szkieletu

**Zależności**: US#12 (konwersja do UL22), US#5 (preprocessing)

**Definicja Ukończenia**:

- [ ] Implementacja algorytmu szkieletyzacji
- [ ] Detekcja punktów charakterystycznych
- [ ] Wizualizacja wyników na Background

---

#### US#3: Branch Detection + Auto-rotacja

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Branch Detection Algorithm
- Automatyczne ustawianie rotacji Square tool
- Detekcja: końcówek, skrzyżowań, rozgałęzień

**Zależności**: US#2 (skeletonization), US Minor #1 (Square tool)

**Definicja Ukończenia**:

- [ ] Implementacja Branch Detection Algorithm
- [ ] Klasyfikacja punktów (końcówka/bifurkacja/skrzyżowanie)
- [ ] Automatyczna rotacja Square przy umieszczaniu

---

#### US#4: Fill Tool z Fragmentation

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Narzędzie Fill tool
- Użycie algorytmu fragmentacji do określenia Canvas
- Wypełnienie regionu wybranym Square

**Zależności**: US#7 (fragmentacja), US Minor #1 (Square tool)

**Definicja Ukończenia**:

- [ ] Fill tool w UI
- [ ] Detekcja spójnego obszaru (Canvas)
- [ ] Wypełnienie regionu Square

---

#### US#5: Preprocessing

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Algorytmy usuwania szumu
- Binaryzacja
- Filtracja

**Zależności**: Brak

**Definicja Ukończenia**:

- [ ] Co najmniej 2 metody preprocessing
- [ ] Możliwość wyboru metody
- [ ] Testy pokazujące poprawę wyników

---

#### US#6: Skeletonization na Square

**Priorytet**: ŚREDNI (Faza 2)  
**Wymagania**:

- Aplikacja skeletonization na Square w Workspace
- Reprezentacja strukturalna Workspace

**Zależności**: US#2 (skeletonization), US#12 (UL22)

**Definicja Ukończenia**:

- [ ] Skeletonization działa na Workspace
- [ ] Generowanie reprezentacji strukturalnej
- [ ] Wizualizacja wyników

---

#### US#7: Test Fragmentation Niezależnie

**Priorytet**: WYSOKI (Faza 1)  
**Wymagania**:

- Moduł testowy dla fragmentacji
- Możliwość załadowania obrazu testowego
- Wizualizacja wyników fragmentacji

**Zależności**: Brak (standalone)

**Definicja Ukończenia**:

- [ ] Aplikacja/moduł testowy
- [ ] Wczytanie obrazu
- [ ] Uruchomienie fragmentacji
- [ ] Wyświetlenie wyników z podziałem na regiony

---

### 🟡 Standard User Stories (2 pkt każdy)

#### US#1: Tworzenie Workspace

**Priorytet**: KRYTYCZNY (Faza 1)  
**Definicja Ukończenia**:

- [ ] Dialog/formularz nowego Workspace
- [ ] Ustawienie rozmiaru Grid
- [ ] Inicjalizacja pustego 2D grid

---

#### US#2: Zapisywanie i Wczytywanie Workspace

**Priorytet**: KRYTYCZNY (Faza 1)  
**Definicja Ukończenia**:

- [ ] Serializacja Workspace (JSON/XML/własny format)
- [ ] Zapis do pliku
- [ ] Wczytanie z pliku
- [ ] Zachowanie stanu wszystkich Square, Entity, Group

---

#### US#3: Parametryzacja Fragmentation

**Priorytet**: ŚREDNI (Faza 2)  
**Definicja Ukończenia**:

- [ ] Panel konfiguracji fragmentacji
- [ ] Parametry: threshold, metoda, czułość
- [ ] Podgląd zmian w czasie rzeczywistym

---

#### US#4: Co najmniej 2 algorytmy Skeletonization

**Priorytet**: ŚREDNI (Faza 2)  
**Definicja Ukończenia**:

- [ ] Implementacja 2+ algorytmów (np. Zhang-Suen, morphological)
- [ ] Możliwość wyboru algorytmu
- [ ] Porównanie wyników

---

#### US#5: Umieszczanie Preset

**Priorytet**: NISKI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Lista dostępnych Preset
- [ ] Narzędzie do umieszczania Preset
- [ ] Aplikacja Preset na 2D grid

---

#### US#12: Konwersja do UL22

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Funkcja konwersji Workspace → UL22
- [ ] Funkcja konwersji obrazu → UL22
- [ ] Uproszczona binarna reprezentacja
- [ ] Testy jednostkowe

---

#### US#13: Auto-wczytywanie domyślnego Workspace

**Priorytet**: NISKI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Plik konfiguracyjny z ścieżką
- [ ] Automatyczne wczytanie przy starcie
- [ ] Możliwość wyłączenia tej funkcji

---

#### US#14: Real-time Preview

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Natychmiastowa wizualizacja zmian
- [ ] Odświeżanie bez lagów
- [ ] Smooth UX

---

#### US#15: Asynchroniczne długie zadania

**Priorytet**: ŚREDNI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Skeletonization, fragmentacja w tle
- [ ] Progress bar/indicator
- [ ] Aplikacja nie zamraża się

---

#### US#16: Dependency Injection

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Kontener DI
- [ ] Separacja interfejsów od implementacji
- [ ] Czysta architektura (SOLID)

---

#### US#11: Testy jednostkowe algorytmów

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Testy dla fragmentacji
- [ ] Testy dla skeletonization
- [ ] Testy dla branch detection
- [ ] Coverage > 80% dla algorytmów

---

### 🟢 Minor User Stories (1 pkt każdy)

#### US#1: Square Tool

**Priorytet**: KRYTYCZNY (Faza 1)  
**Definicja Ukończenia**:

- [ ] Kliknięcie umieszcza Square
- [ ] Wybór typu Square
- [ ] Wizualne potwierdzenie

---

#### US#2: Element Remover

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Narzędzie usuwania
- [ ] Kliknięcie usuwa Element
- [ ] Działa na Square i Entity

---

#### US#3: Przełączanie Group

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Lista Group
- [ ] Przełącznik aktywnego Group
- [ ] Edycja tylko aktywnego Group

---

#### US#4: Grid Features

**Priorytet**: NISKI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Generowanie Grid features
- [ ] Numeryczna reprezentacja Structural features
- [ ] Wyświetlanie/eksport danych

---

#### US#5: User-friendly Interface

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Przejrzysty layout
- [ ] Intuicyjne toolbary
- [ ] Skróty klawiszowe

---

#### US#6: Entity Tool

**Priorytet**: NISKI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Narzędzie umieszczania Entity
- [ ] Lista Entity types
- [ ] Umieszczanie na 2D grid

---

#### US#7: Square Types

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Lista dostępnych Square types
- [ ] Wybór aktywnego typu
- [ ] Ikony/preview

---

#### US#8: Podświetlanie Structural Features

**Priorytet**: ŚREDNI (Faza 2)  
**Definicja Ukończenia**:

- [ ] Overlay z wykrytymi features
- [ ] Różne kolory dla różnych typów
- [ ] Możliwość włączenia/wyłączenia

---

#### US#9: Undo/Redo

**Priorytet**: ŚREDNI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Stack akcji
- [ ] Undo (Ctrl+Z)
- [ ] Redo (Ctrl+Y)
- [ ] Limit historii

---

#### US#10: Auto-detekcja Grid Border

**Priorytet**: WYSOKI (Faza 1)  
**Definicja Ukończenia**:

- [ ] Automatyczne określenie granic przy otwieraniu
- [ ] Automatyczne określenie granic przy zapisie
- [ ] Brak konieczności ręcznego ustawiania

---

#### US#11: Automatyczna adnotacja Workspace

**Priorytet**: ŚREDNI (Faza 2)  
**Definicja Ukończenia**:

- [ ] Automatyczne oznaczanie Structural features
- [ ] Update w czasie rzeczywistym
- [ ] Przydatne informacje dla Map Designer

---

#### US#12: Synchronizacja danych

**Priorytet**: ŚREDNI (Faza 3)  
**Definicja Ukończenia**:

- [ ] Workspace data i wyniki analiz zsynchronizowane
- [ ] Brak rozbieżności
- [ ] Real-time consistency

---

#### US#13: Podświetlanie z Branch Detection

**Priorytet**: ŚREDNI (Faza 2)  
**Definicja Ukończenia**:

- [ ] Wizualizacja wyników Branch Detection
- [ ] Oznaczenie końcówek, skrzyżowań, bifurkacji
- [ ] Pomocne dla Domain Expert

---

## 📈 Tracking Progress

### Punktacja według faz:

- **Faza 1 (MVP)**: 21 pkt → Podstawowy działający edytor
- **Faza 2 (Rozszerzona)**: 45 pkt → Pełne algorytmy biometryczne
- **Faza 3 (Kompletna)**: 56 pkt → Wszystkie funkcje

### Zalecenia:

1. **Zaczynaj od testów** - szczególnie dla algorytmów biometrycznych
2. **Implementuj iteracyjnie** - najpierw prosta wersja, potem rozszerzenia
3. **Dokumentuj na bieżąco** - komentarze w kodzie zgodne z Ubiquitous Language
4. **Code review** - szczególnie algorytmów biometrycznych
5. **Integration testing** - po połączeniu modułów

---

**Status aktualizacji**: Ustawiaj checkboxy [ ] na [x] po ukończeniu zadania
