# Edytor Map z Algorytmami Biometrycznymi

## 📋 Opis Projektu

Projekt realizowany w ramach przedmiotu Przetwarzanie Obrazów. Aplikacja to edytor map dla gier platformowych wykorzystujący zaawansowane algorytmy biometryczne do analizy i przetwarzania terenu.

**Zespół:**

- Dorota Maliszewska
- Jakub Modzelewski
- Mateusz Kondraciuk
- Dawid Waszkiewicz
- Emilia Stypułkowska

## 🎯 Cel Projektu

Stworzenie aplikacji edytora map, która łączy:

- **Intuicyjną edycję**: Tworzenie poziomów gier za pomocą narzędzi graficznych
- **Algorytmy biometryczne**: Automatyczna analiza struktury map (szkieletyzacja, detekcja rozgałęzień, fragmentacja)
- **Import obrazów**: Konwersja obrazów RGB na presety map

## 📊 System Punktacji

- **Maksymalna liczba punktów**: 56
- **Minimum do zaliczenia**: 23 punkty
- **Major User Stories**: 3 pkt każdy (max 21)
- **Standard User Stories**: 2 pkt każdy (max 22)
- **Minor User Stories**: 1 pkt każdy (max 13)

### 🎉 Aktualny Stan: **28/23 punkty** ✅ MINIMUM ZALICZONE + 5 PKT!

**Zrealizowane funkcje:**

- ✅ Tworzenie i zarządzanie workspace (2 pkt)
- ✅ Zapis/odczyt workspace (2 pkt)
- ✅ Dependency Injection (2 pkt)
- ✅ Konwersja UL22 (2 pkt)
- ✅ Preprocessing: Median Filter + Otsu (3 pkt)
- ✅ Fragmentacja: Connected Components (3 pkt)
- ✅ Szkieletyzacja: Zhang-Suen (3 pkt)
- ✅ Drugi algorytm szkieletyzacji: K3M (2 pkt)
- ✅ Branch Detection: CN Method (3 pkt)
- ✅ Fill Tool z Fragmentacją (3 pkt)
- ✅ Narzędzie do umieszczania kwadratów (1 pkt)
- ✅ Narzędzie do usuwania elementów (1 pkt)
- ✅ User-friendly interface (1 pkt)

**Testy:** 87/87 ✅

## 🏗️ Architektura Systemu

### Bounded Contexts

1. **Editing Context** - Edycja i przechowywanie Workspace
2. **Biometric Context** - Algorytmy biometryczne
3. **Prefab Context** - Zarządzanie presetami
4. **Visualization Context** - Wizualizacja w czasie rzeczywistym

### Kluczowe Komponenty

- **Workspace**: Dwuwymiarowa przestrzeń robocza (2D grid)
- **Square**: Podstawowa jednostka terenu
- **Entity**: Elementy nie-terenowe (wrogowie, start, koniec)
- **Preset**: Predefiniowane konfiguracje elementów
- **Tools**: Narzędzia edycji (Square tool, Fill tool, Element remover)

## 🔧 Wymagania Techniczne

- ✅ Dowolna technologia/język programowania
- ✅ Dowolne frameworki
- ⚠️ **WAŻNE**: Algorytmy biometryczne muszą być zaimplementowane **bez dodatkowych bibliotek**

## 🚀 Rozpoczęcie Pracy

1. Zapoznaj się z pełną [Specyfikacją projektu](../../../Downloads/Specyfikacja.md)
2. Przejrzyj [Listę zadań](TASKS.md) z priorytetami
3. Zapoznaj się z [Architekturą](ARCHITECTURE.md)
4. Przeczytaj [Opis algorytmów](ALGORITHMS.md)

## 📁 Struktura Projektu

```
proj/
├── README.md                 # Ten plik
├── TASKS.md                  # Lista zadań z priorytetami
├── ARCHITECTURE.md           # Szczegółowa architektura
├── ALGORITHMS.md             # Opis algorytmów biometrycznych
├── docs/                     # Dodatkowa dokumentacja
├── src/                      # Kod źródłowy
│   ├── editing/             # Editing Context
│   ├── biometric/           # Biometric Context
│   ├── prefab/              # Prefab Context
│   └── visualization/       # Visualization Context
├── tests/                    # Testy jednostkowe
└── assets/                   # Zasoby (ikony, przykładowe mapy)
```

## 🧪 Testowanie

Zgodnie z wymaganiami projektu:

- Testy jednostkowe algorytmów biometrycznych (Standard US #11)
- Niezależne testowanie fragmentacji na obrazach (Major US #7)

## 📚 Słownik Pojęć (Ubiquitous Language)

Pełny słownik znajduje się w specyfikacji. Najważniejsze terminy:

- **Workspace**: Przestrzeń robocza zawierająca 2D grid
- **2D grid**: Dyskretny układ współrzędnych z komórkami
- **Fragmentation**: Podział regionu na podobszary
- **Skeletonization**: Przekształcenie tła w jednopikselowy szkielet
- **Branch Detection Algorithm**: Detekcja końcówek i rozgałęzień
- **UL22**: Uproszczona binarna reprezentacja 2D grid

## 📝 Konwencje Projektu

- Nazwy zgodne z Ubiquitous Language
- Dependency Injection dla czystej architektury
- Operacje asynchroniczne dla długich zadań
- Real-time preview dla natychmiastowych zmian

## 🎓 Przydatne Zasoby

- [Specyfikacja pełna](../../../Downloads/Specyfikacja.md)
- [Lista zadań do realizacji](TASKS.md)
- [Dokumentacja architektury](ARCHITECTURE.md)
- [Algorytmy biometryczne](ALGORITHMS.md)

## 🔄 Status Realizacji

Aktualny status realizacji User Stories znajduje się w pliku [TASKS.md](TASKS.md).

---

**Uwaga**: Projekt wymaga implementacji algorytmów biometrycznych od podstaw. Skoncentruj się na poprawnej implementacji przed dodawaniem dodatkowych funkcji.
