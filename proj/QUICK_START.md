# Quick Start Guide

## 🚀 Szybki Start dla Zespołu

### Krok 1: Setup Projektu (1 godzina)

1. **Wybór technologii** (wspólna decyzja zespołu):
   - Opcja A: **C# + WPF** (dobry dla Windows desktop)
   - Opcja B: **Python + Qt/Tkinter** (cross-platform, łatwy prototyping)
   - Opcja C: **Java + JavaFX** (cross-platform, znany język)
   - Opcja D: **Electron + React** (web technologies)

2. **Konfiguracja repozytorium Git**:

   ```bash
   cd "c:\Users\dawid\Desktop\studia\MAG\semestr 2\PrzetwarzanieObrazow\proj"
   git init
   git add .
   git commit -m "Initial project structure"
   ```

3. **Setup środowiska developerskiego**:
   - Zainstaluj IDE (Visual Studio / PyCharm / IntelliJ / VS Code)
   - Zainstaluj SDK/Runtime dla wybranej technologii
   - Zweryfikuj, że wszystko działa

### Krok 2: Podział Ról (30 minut)

**Rekomendowany podział**:

- **2 osoby**: Frontend/UI (Presentation Layer)
- **2 osoby**: Backend/Algorithms (Infrastructure Layer)
- **1 osoba**: Architecture/Integration (Domain + Application Layer)

**Alternatywnie**:

- Wszyscy zaczynają od algorytmów (równoległa implementacja różnych algorytmów)
- Później integracja

### Krok 3: Sprint 1 - MVP Week 1 (1 tydzień)

**Priorytetowe zadania**:

1. Setup projektu w wybranej technologii
2. Implementacja Domain Model (Workspace, Grid2D, Square)
3. Podstawowy UI z możliwością klikania
4. Zapisywanie/wczytywanie do JSON

**Definition of Done**:

- [ ] Można utworzyć workspace 10x10
- [ ] Można kliknąć i umieścić Square
- [ ] Można zapisać do pliku
- [ ] Można wczytać z pliku

### Krok 4: Sprint 2 - Algorytmy (1 tydzień)

**Priorytetowe zadania**:

1. UL22 Converter (najprostsze)
2. Fragmentacja - Connected Components
3. Preprocessing - Median Filter
4. Testy jednostkowe

**Definition of Done**:

- [ ] Wszystkie algorytmy mają testy
- [ ] Testy przechodzą
- [ ] Przykłady testowe z TEST_CASES.md działają

---

## 📋 Checklist - Pierwsze Spotkanie Zespołu

- [ ] Wszyscy przeczytali README.md
- [ ] Wszyscy przeczytali Specyfikację projektu
- [ ] Ustalono technologię
- [ ] Ustalono sposób komunikacji (Discord/Slack)
- [ ] Ustalono harmonogram spotkań
- [ ] Podzielono role
- [ ] Wyznaczono Scrum Master / Project Lead
- [ ] Ustalono Definition of Done
- [ ] Setup repozytorium Git (+ uprawnienia dla wszystkich)
- [ ] Zaplanowano Sprint 1

---

## 🎯 Cele na Pierwsze 3 Tygodnie

### Tydzień 1: Infrastruktura

**Cel**: Działający edytor do umieszczania Square
**Punkty**: 7 pkt (Standard US#1,2 + Minor US#1,2,5)

### Tydzień 2: Algorytmy

**Cel**: Działająca fragmentacja + testy
**Punkty**: 10 pkt (Major US#7 + Standard US#11,12,16)

### Tydzień 3: Integracja

**Cel**: MVP (21 pkt)
**Punkty**: 4 pkt (Standard US#14 + Minor US#3,7,10)

---

## 💡 Wskazówki

### Dla Frontend Team:

1. Zacznij od mockupu UI na papierze
2. Użyj Grid/Canvas widget dla 2D grid
3. Event handlers dla kliknięć myszy
4. Data binding do ViewModels (MVVM)

### Dla Backend/Algorithms Team:

1. Zacznij od TEST_CASES.md
2. Implementuj test → implementuj funkcję → przejdź test
3. Wizualizuj pośrednie wyniki (print/debug)
4. Używaj małych przykładów (3x3, 5x5)

### Dla Architecture Team:

1. Zdefiniuj interfejsy (contracts)
2. Setup Dependency Injection
3. Koordynuj między Frontend a Backend
4. Code review

---

## 📚 Must-Read Documents (Priorytet)

1. **README.md** (5 min) - Przegląd projektu
2. **TASKS.md** (20 min) - Lista User Stories
3. **ALGORITHMS.md** (30 min) - Szczegóły algorytmów
4. **TEST_CASES.md** (15 min) - Konkretne przykłady
5. **ARCHITECTURE.md** (20 min) - Struktura systemu

**Total reading time**: ~90 min

---

## 🔧 Przykładowy Setup (Python + Qt)

```bash
# Create virtual environment
python -m venv venv
source venv/bin/activate  # Windows: venv\Scripts\activate

# Install dependencies
pip install PyQt5 pytest numpy pillow

# Create basic structure
mkdir -p src/Domain/Editing/Entities
mkdir -p src/Infrastructure/Algorithms
mkdir -p tests/Unit

# First file: src/Domain/Editing/Entities/Workspace.py
```

---

## 🔧 Przykładowy Setup (C# + WPF)

```bash
# Create solution
dotnet new sln -n MapEditor

# Create projects
dotnet new wpf -n MapEditor.Presentation
dotnet new classlib -n MapEditor.Domain
dotnet new classlib -n MapEditor.Application
dotnet new classlib -n MapEditor.Infrastructure
dotnet new xunit -n MapEditor.Tests

# Add to solution
dotnet sln add **/*.csproj

# Build
dotnet build
```

---

## ❓ FAQ

**Q: Którą technologię wybrać?**
A: C#/WPF jeśli zespół zna .NET. Python/Qt jeśli chcecie szybki prototyping. Java/JavaFX jako middle ground.

**Q: Jak dzielić pracę?**
A: Równolegle: jedni UI, drudzy algorytmy. Integracja co tydzień.

**Q: Jak zacząć algorytmy?**
A: Od UL22 (najprostsze) → Preprocessing → Fragmentacja → Skeletonization → Branch Detection

**Q: Jak testować?**
A: Użyj TEST_CASES.md. Zacznij od małych przykładów (3x3). Wizualizuj wyniki.

**Q: Co gdy ktoś ugrzęźnie?**
A: Pair programming lub code review. Pytaj na grupie.

**Q: Ile czasu na projekt?**
A: Realistycznie 6-8 tygodni dla pełnej implementacji. MVP w 3 tygodnie.

---

## 🎓 Learning Resources

### Domain-Driven Design:

- [DDD Basics](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- Agregaty, Encje, Value Objects

### Algorytmy:

- Zhang-Suen: "A Fast Parallel Algorithm for Thinning Digital Patterns" (1984)
- Connected Components: klasyczny algorytm grafowy
- Crossing Number: metoda z rozpoznawania odcisków palców

### Clean Architecture:

- Dependency Injection
- SOLID principles
- Testable code

---

**Powodzenia! 🚀**

Pytania? Dodaj je do dokumentacji lub dyskutuj na spotkaniach zespołu.
