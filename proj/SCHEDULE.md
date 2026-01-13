# Harmonogram Realizacji Projektu

## 🗓️ Timeboxing i Milestones

### MILESTONE 1: MVP - Podstawowa Funkcjonalność (Tydzień 1-3)

**Cel**: Działający edytor + jeden algorytm biometryczny (23+ pkt)
**Deadline**: [TBD]

#### Tydzień 1: Infrastruktura + Podstawy Edytora

- [ ] Setup projektu (wybór technologii, konfiguracja)
- [ ] Domain Model - podstawowe encje (Workspace, Grid2D, Square)
- [ ] Tworzenie Workspace (Standard US#1) - 2 pkt
- [ ] Zapisywanie/wczytywanie Workspace (Standard US#2) - 2 pkt
- [ ] Square Tool (Minor US#1) - 1 pkt
- [ ] Element Remover (Minor US#2) - 1 pkt
- [ ] Podstawowy UI (Minor US#5) - 1 pkt

**Wynik tygodnia 1**: Działający edytor do umieszczania/usuwania Square + zapis/odczyt

#### Tydzień 2: Algorytmy Biometryczne - Podstawy

- [ ] Konwersja do UL22 (Standard US#12) - 2 pkt
- [ ] Preprocessing - podstawowe filtry (Major US#5) - 3 pkt
- [ ] Fragmentacja - Connected Components (Major US#7) - 3 pkt
- [ ] Testy jednostkowe algorytmów (Standard US#11) - 2 pkt
- [ ] Dependency Injection (Standard US#16) - 2 pkt

**Wynik tygodnia 2**: Działające algorytmy fragmentacji z testami

#### Tydzień 3: Integracja + Polishing MVP

- [ ] Real-time preview (Standard US#14) - 2 pkt
- [ ] Przełączanie Group (Minor US#3) - 1 pkt
- [ ] Square Types (Minor US#7) - 1 pkt
- [ ] Auto-detekcja Grid Border (Minor US#10) - 1 pkt
- [ ] Testy integracyjne
- [ ] Bug fixing

**Wynik tygodnia 3**: Kompletne MVP (21 pkt)

---

### MILESTONE 2: Rozszerzone Algorytmy (Tydzień 4-6)

**Cel**: Pełne wsparcie biometrii (45+ pkt)
**Deadline**: [TBD]

#### Tydzień 4: Skeletonization

- [ ] Algorytm Zhang-Suen (Major US#2, US#6) - 6 pkt
- [ ] Drugi algorytm skeletonization (Standard US#4) - 2 pkt
- [ ] Testy porównawcze algorytmów
- [ ] Wizualizacja szkieletu

**Wynik tygodnia 4**: +8 pkt (suma: 29 pkt)

#### Tydzień 5: Branch Detection + Fill Tool

- [ ] Branch Detection Algorithm (Major US#3) - 3 pkt
- [ ] Auto-rotacja Square (część Major US#3)
- [ ] Fill Tool z fragmentacją (Major US#4) - 3 pkt
- [ ] Podświetlanie Structural Features (Minor US#8, US#13) - 2 pkt
- [ ] Automatyczna adnotacja (Minor US#11) - 1 pkt

**Wynik tygodnia 5**: +9 pkt (suma: 38 pkt)

#### Tydzień 6: Import Obrazów + Parametryzacja

- [ ] Import RGB + fragmentacja → Preset (Major US#1) - 3 pkt
- [ ] Parametryzacja fragmentacji (Standard US#3) - 2 pkt
- [ ] Asynchroniczne zadania (Standard US#15) - 2 pkt

**Wynik tygodnia 6**: +7 pkt (suma: 45 pkt)

---

### MILESTONE 3: Kompletny System (Tydzień 7-8)

**Cel**: Wszystkie funkcje (56 pkt)
**Deadline**: [TBD]

#### Tydzień 7: Presety + Entities

- [ ] Umieszczanie Preset (Standard US#5) - 2 pkt
- [ ] Entity Tool (Minor US#6) - 1 pkt
- [ ] Grid Features (Minor US#4) - 1 pkt
- [ ] Undo/Redo (Minor US#9) - 1 pkt

**Wynik tygodnia 7**: +5 pkt (suma: 50 pkt)

#### Tydzień 8: Finalizacja + Developer Features

- [ ] Auto-wczytywanie workspace (Standard US#13) - 2 pkt
- [ ] Synchronizacja danych (Minor US#12) - 1 pkt
- [ ] Pełna dokumentacja
- [ ] Code review
- [ ] Wszystkie testy przechodzą
- [ ] Prezentacja projektu

**Wynik tygodnia 8**: +3 pkt (suma: 53 pkt)
**Buffer**: +3 pkt dodatkowe funkcje lub polerowanie

---

## ⚠️ Punkty Krytyczne

### Risk Management

1. **Algorytmy biometryczne są trudne**
   - Mitigacja: Zacząć wcześnie, testować na prostych przykładach
   - Buffer: Priorytyzować jeden działający algorytm nad wieloma połowicznymi

2. **Integracja może być problematyczna**
   - Mitigacja: Testy integracyjne od początku
   - Buffer: Dependency Injection ułatwi mockowanie

3. **UI może być czasochłonne**
   - Mitigacja: Minimalistyczny UI w MVP, rozbudowa później
   - Buffer: Użyć gotowych kontrolek z frameworka

4. **Członkowie zespołu mogą mieć różne tempo**
   - Mitigacja: Regularne spotkania, podział zadań na małe części
   - Buffer: Code review wzajemny, pair programming dla trudnych części

---

## 📅 Szablon Weekly Sprint

### Sprint Planning (Poniedziałek)

- [ ] Review poprzedniego tygodnia
- [ ] Wybór User Stories na tydzień
- [ ] Podział zadań między członków zespołu
- [ ] Aktualizacja TASKS.md

### Daily Standup (codziennie, 15 min)

- Co zrobiłem wczoraj?
- Co planuję dzisiaj?
- Czy są jakieś blokady?

### Sprint Review (Piątek)

- [ ] Demo działającej funkcjonalności
- [ ] Update punktacji
- [ ] Merge code
- [ ] Retrospekcja: co poszło dobrze, co poprawić

---

## 📊 Tracking Progress

### Punktacja Team

| Milestone   | Target | Current | % Complete |
| ----------- | ------ | ------- | ---------- |
| MVP         | 21 pkt | 0 pkt   | 0%         |
| Rozszerzone | 45 pkt | 0 pkt   | 0%         |
| Kompletne   | 56 pkt | 0 pkt   | 0%         |

### Individual Contributions

| Członek Zespołu     | User Stories | Punkty | Status |
| ------------------- | ------------ | ------ | ------ |
| Dorota Maliszewska  | TBD          | 0      | -      |
| Jakub Modzelewski   | TBD          | 0      | -      |
| Mateusz Kondraciuk  | TBD          | 0      | -      |
| Dawid Waszkiewicz   | TBD          | 0      | -      |
| Emilia Stypułkowska | TBD          | 0      | -      |

---

## 🎯 Definition of Done

Każdy User Story uważany jest za ukończony gdy:

- [ ] Kod zaimplementowany zgodnie ze specyfikacją
- [ ] Testy jednostkowe napisane i przechodzą
- [ ] Code review wykonane przez innego członka zespołu
- [ ] Dokumentacja zaktualizowana (jeśli dotyczy)
- [ ] Funkcjonalność zintegrowana z resztą systemu
- [ ] Demo możliwe do pokazania

---

## 📝 Notatki

**Pierwsze kroki**:

1. Spotkanie zespołu - ustalenie technologii
2. Setup repozytorium Git
3. Podział na podzespoły (Frontend, Backend/Algorithms)
4. Rozpoczęcie od tygodnia 1

**Rekomendowane narzędzia**:

- Git + GitHub/GitLab do wersjonowania
- Discord/Slack do komunikacji
- Trello/Jira do trackingu zadań
- Visual Studio Code / IDE dla wybranej technologii

---

**Status**: Projekt do rozpoczęcia  
**Ostatnia aktualizacja**: 2026-01-11
