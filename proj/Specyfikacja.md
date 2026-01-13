
---

Do czytania plików **Markdown** polecam Visual Studio Code z rozszerzeniem *(Shift+Ctrl+X)* **Markdown Preview Enhanced**. Domyślnym skrótem do włączenia podglądu pliku Markdown jest *Ctrl+K V*. W lewym dolnym rogu podglądu jest przycisk do konfiguracji, gdzie można zmienić wiele rzeczy takich jak ciemne/jasne tło.

Maksymalna liczba punktów z projektu **(100%)** to **56**. Aby zdać należy uzyskać **23** punkty. Można użyć dowolnej technologii, języka i frameworków z tym, że algorytmy biometryczne powinny być zaimplementowane bez użycia dodatkowych bibliotek. Punktowane są realizacje **User Stories**. Można uzyskać niepełną liczbę punktów jeśli User Story nie jest zaimplementowany do końca.

Wytłumaczenia pojęć związanych ze specyfikacją projektu (jest to część **Ubiquitous Language** specyfikacji projektu):
1. **User Story** to krótki opis funkcjonalności widziany z kogoś perspektywy, zazwyczaj użytkownika. Opisuje **co** chcemy osiągnąć a nie w jaki sposób.
1. **Ubiquitous Language** to wspólny i jednoznaczny słownik pojęć
1. **Bounded Context** to tematycznie wydzielone fragmenty aplikacji
1. **Domain Aggregate** to grupa tematycznie powiązanych obiektów.
1. **Domain Entity** to unikatowy obiekt domenowy. Posiada Id. Przykładem takiego obiektu będzie użytkownik - dwóch użytkowników o tym samym imieniu i nazwisku to całkowicie niezależne od siebie obiekty.
1. **Value Object** to obiekty nieunikatowe które definiują wartości, na przykład punkt to Value Object, ponieważ jest definiowany poprzez współrzędne X i Y i tylko to nas interesuje w takim punkcie.
1. **Domain Event** to zdarzenia na które aplikacja powinna zareagować
1. **High-level Domain Flow** to jest przepływ działań od początku do końcowego wyniku

**Skład zespołu:**
1.  Dorota Maliszewska
1.  Jakub Modzelewski
1.  Mateusz Kondraciuk
1.  Dawid Waszkiewicz
1.  Emilia Stypułkowska

---

# **Ubiquitous Language**

---

1. **Map Designer** = Użytkownik tworzący **Workspace**
1. **Developer** = Osoba odpowiedzialna za rozwój i utrzymanie edytora **Workspace**
1. **Domain Expert** = Osoba określająca i oceniająca wymagania dotyczące projektu
1. **Workspace** = Dwuwymiarowa przestrzeń robocza **2D grid** zawierająca **Element** oraz dodatkowe informacje takie jak rozmiar
1. **2D grid** = Dyskretny układ współrzędnych 2D, zbudowany z komórek, w których można umieszczać **Element**  
1. **Element** = Dowolny element umieszczony na **2D grid** w **Workspace**, jest to **Square** lub **Entity**  
1. **Square** = Podstawowa jednostka **Workspace** umieszczana w **2D grid** reprezentująca teren. Przetwarzana przez algorytmy biometryczne.
1. **Entity** = Nie-**Square** element umieszczany na **Workspace**, np. przeciwnicy, start, koniec. Nieprzetwarzana przez algorytmy biometryczne.
1. **Preset** = Predefiniowana konfiguracja wielu **Square** lub wielu **Entity** umieszczana jako jedna całość (np. most, wodospad) 
1. **Map** = Obszar istotny, wynik **Fragmentation**
1. **Background** = Obszar nieistotny, może być poddany **Skeletonization**
1. **Group** = Logiczna warstwa grupująca **Element**, umożliwia edycję różnych warstw oddzielnie  
1. **Square types** = Zestaw dostępnych typów **Square** możliwych do umieszczenia na **Workspace**  
1. **Entity types** = Zestaw dostępnych typów **Entity** możliwych do umieszczenia na **Workspace**  
1. **Preset types** = Zestaw dostępnych typów **Preset** możliwych do umieszczenia na **Workspace**  
1. **Tool** = Narzędzie edycji, takie jak **Square tool**, **Element remover**,**Fill tool**  
1. **Active tool** = Aktywne narzędzie edycji, takie jak **Square tool**, **Element remover**,**Fill tool**  
1. **Fill tool** = Narzędzie do wypełniania lokalnego obszaru **2D grid** wybranym **Square** w **Workspace**  
1. **Square tool** = Narzędzie do umieszczania **Square** w **2D grid**  
1. **Entity tool** = Narzędzie do umieszczania **Entity** w **2D grid**  
1. **Element remover** = Narzędzie do usuwania **Element** w **2D grid**  
1. **UL22** = Uproszczona binarna reprezentacja **2D grid** stosowana jako wejście do **Branch Detection Algorithm**  
1. **Skeletonization** = Algorytm przekształcający tło z **2D grid** w jednopikselowy szkielet  
1. **Skeleton** = Wynik **Skeletonization**  
1. **Fragmentation** = Proces dzielenia regionu na podobszary na podstawie typu **Square** lub rozdzielania obiektu od tła na obrazie  
1. **Branch Detection Algorithm** = Algorytm który na podstawie przejść między sąsiadującymi obiektami określa końcówki, skrzyżowania i rozgałęzienia  
1. **Structural features** = Charakterystyczne cechy **Skeleton** oraz punkt sklasyfikowany poprzez **Branch Detection Algorithm** (np. końcówka, bifurkacja)  
1. **Grid features** = Numeryczna reprezentacja wszystkich **Structural features** z danego obrazu lub **2D grid**  
1. **Grid border** = Nieprzekraczalna granica **2D grid**. Wyznacza rozmiar zapisany w **Workspace**  
1. **Preprocessing** = Przygotowanie regionu do analizy, np. binaryzacja, filtracja, usuwanie szumu  
1. **Selection** = Bezpośrednio zaznaczony obszar siatki  
1. **Canvas** = Spójny obszar **2D grid**, wykorzystywany przez**Fill tool** i **Fragmentation**  
1. **Cell** = pojedynczy element z **2D grid** która może przetrzymywać jeden **Element**, posiadająca dwie współrzędne: X i Y.

---

## **Major User Stories (3 points each, max 21)**
 1. As **Map Designer**, I want to import an RGB image and distinguish **Map** from **Background** by applying **Fragmentation** to it, so that I can quickly create **Preset**.  
 1. As **Map Designer**, I want to apply **Skeletonization** to the **Background**, so that I can see where points of interests like crossings and bifurcations are. 
 1. As **Map Designer**, I want to automatically apply results of **Branch Detection Algorithm** to my **Square tool**, so that placed groups of **Square** are correctly set in the correct rotation/direction.
 1. As **Map Designer**, I want to use**Fill tool**, so that I can quickly fill regions with selected **Square** by using **Fragmentation**. 
 1. As **Developer**, I want to apply **Preprocessing**, so that noise does not distort biometrics algorithms.
 1. As **Developer**, I want to run **Skeletonization** on **Square**, so that I can compute structural representation of **Workspace**.
 1. As **Domain Expert**, I want to test **Fragmentation** independently on images, so that I can check whether the implementation is correct.  

## **Standard User Stories (2 points each, max 22)**
 1. As **Map Designer**, I want to create **Workspace**, so that I can start building a platformer level from scratch.  
 1. As **Map Designer**, I want to save and load **Workspace**, so that I can continue editing later.  
 1. As **Map Designer**, I want to have a parametrization of **Fragmentation**, so that I adjust it for my specific use case.
 1. As **Map Designer**, I want to have at least 2 **Skeletonization** algorithms, so that I can compare the results and choose the best suited.
 1. As **Map Designer**, I want to place **Preset**, so that I can insert larger structures more quickly.  
 1. As **Developer**, I want to convert **Workspace** region to **UL22**, so that biometric algorithms can operate on it.  
 1. As **Developer**, I want to be able to load the default **Workspace** automatically at launch, so that the debugging process will be faster.  
 1. As **Developer**, I want to update and preview results in real time, so that the user sees changes immediately. 
 1. As **Developer**, I want to have long tasks done asynchronously, so that the application does not freeze.  
 1. As **Developer**, I want to use Dependency Injection in order to achieve Inversion of Control so that the project architecture will be clean.
 1. As **Domain Expert**, I want to make unit tests of biometric algorithms so that I will be confident in their correctness.

## **Minor User Stories (1 point each, max 13)**
 1. As **Map Designer**, I want to place **Square** using a **Square tool**, so that I can quickly build terrain.  
 1. As **Map Designer**, I want to erase **Element** using an **Element remover**, so that I can correct mistakes easily.  
 1. As **Map Designer**, I want to switch between **Group**s, so that I can edit groups of **Element** independently.  
 1. As **Map Designer**, I want to create and display **Grid features**, so that I can compare it with other **Workspace** in order to assess the complexity.  
 1. As **Map Designer**, I want the application to have a user-friendly interface, so it is easy to use for me.
 1. As **Map Designer**, I want to place **Entity** using **Entity tool**, so that I can add player and enemies.
 1. As **Map Designer**, I want to use **Square types**, so that I can select different **Square** types efficiently.  
 1. As **Map Designer**, I want to highlight detected **Structural features**, so that I know where intersections appear.  
 1. As **Map Designer**, I want to undo and redo my actions, so that I can safely experiment.  
 1. As **Map Designer**, I want to have **Grid border** automatically detected, so that I do not need to state it when I open or save the map.  
 1. As **Developer**, I want to automatically annotate **Workspace** with detected **Structural features**, so that **Map Designer** receive useful information.
 1. As **Developer**, I want to ensure data consistency, so that **Workspace** data and analysis results stay synchronized in real time.  
 1. As **Domain Expert**, I want to highlight **Structural features** based on **Branch Detection Algorithm**, so that I can understand what is labeled as an intersection.  

---

# **Bounded Contexts**

---

## **1. Editing Context**
Responsible for everything related to editing and storing of **Workspace**.

UL terms: **Workspace**, **2D grid**, **Grid border**, **Square**, **Entity**, **Preset**, **Group**, **Square types**, **Entity types**, **Preset types**, **Square tool**, **Element remover**,**Fill tool**, **Entity tool** (all **Active tool**)

**Typical responsibilities:**
- Creating, saving, loading **Workspace**
- Editing **2D grid**
- Managing placement and removal of **Element**
- Handling **Layer** visibility and editing
- Undo/redo operations


## **2. Biometric Context**
Handles all biometric algorithms used to analyze **2D grid** or imported images.

UL terms: **UL22**, **Skeletonization**, **Skeleton**, **Fragmentation**, **Branch Detection Algorithm**, **Structural features**, **Structural features**, **Grid features**, **Preprocessing**

**Typical responsibilities:**
- Converting **Workspace** or an image into **UL22**
- Running **Preprocessing** (noise removal, filtering, etc.)
- Computing **Skeletonization**
- Detecting **Structural features**
- Running **Branch Detection Algorithm**
- Generating **Grid features** for biometric analysis

## **3. Prefab Context**
Dedicated to constructing reusable building blocks for **Workspace**.

Includes UL terms: **Preset**, **Preset types**, **Fragmentation** (when used to convert images to **Preset**)

**Typical responsibilities:**
- Importing an image and transforming parts into **Preset**
- Editing or configuring **Preset**
- Saving and loading **Preset** definitions
- Managing the **Preset types**

## **4. Visualization Context**

Focused on real-time visual representation and overlays.
UL terms: **Structural features**, **Structural features**, **Workspace**, **Group**

**Responsibilities:**
- Displaying **Workspace** and all its **Group** instances
- Rendering and highlighting results of biometrics
- Previewing changes in real-time
- Ensuring asynchronous updates in order to not freeze the UI

---


# **Domain Model - Aggregates, Value Objects, Entities**

---

## **1. Map Aggregate**

**Root Entity:** **Workspace**
**Description:** Represents the entire editable level. It is the main consistency boundary between the editor and biometrics.

### **Contains**
- **2D grid** (Entity)
- **Grid border** (Value Object)
- **Group** (Entity, multiple)
- **Element** (Entity, multiple)
- **Grid features** (Value Object, optional)
- **UL22** (Value Object)
- **Map Metadata** (Value Object, name/author/size)

### **Responsibilities**
- Manage creating, loading, saving the **Workspace**.
- Maintain consistent placement/removal of a **Element**.
- Synchronize biometric results (**Skeleton**, **Structural features**, **Structural features**).
- Define the **Workspace** constraints via **Grid border**.

---

## **2. Layer Aggregate**
**Root Entity:** **Group**
**Description:** Each map is split into logical layers (**Square**, **Entity**).

### **Contains**
- **Element** (Value Object, ordered per layer)
- **Square types** **Reference** (Value Object)
- **Entity types** **Reference** (Value Object)

### **Responsibilities**
- Isolate editing operations per **Group**.
- Contain all objects that visually belong to that **Group**.
- Handle all elements of **Tool** type.

---

## **3. **Preset** Aggregate**
**Root Entity:** **Preset**
**Description:** A predefined structure made of **Square**.

### **Contains**
- **Preset** **Layout** (Value Object - a list of **Square** and their local positions)
- **Position Point** (Value Object) - point of origin

### **Responsibilities**
- Hold reusable building blocks.
- Allow applying **Preset** to **2D grid** in one operation.

---

## **4. Biometrics Aggregate**
**Root Entity:** **Biometric Processing Session**
**Description:** Encapsulates all biometrics algorithms and ensures correctness of transformations.

### **Contains**
- **Preprocessing** **Parameters** (Value Object)
- **Fragmentation** **Result** (Value Object)
- **Skeletonization** **Result** / **Skeleton** (Value Object)
- **Branch Detection Algorithm** **Result** (Value Object)
- **Detected** **Structural features** (Value Object list)
- **Structural features** (Value Object list)
- **Grid features** (Value Object)

### **Responsibilities**
- Produce outputs from biometric algorithms for **Workspace**.

---

# **Domain Events**

---

## **Workspace** **Editing Events**
- **Workspace** **Created**
- **Workspace** **Loaded**
- **Workspace** **Saved**
- **Workspace** **GridInitialized**
- **Group** **Added**
- **Group** **Removed**
- **Group** **Switched**
- **Square** **Placed**
- **Entity** **Placed**
- **Preset** **Placed**
- **Element** **Erased**
- **UndoPerformed**
- **RedoPerformed**
- **Grid border** **Detected**

## **Tool Events**
- **Square tool** **Activated**
- **Fill tool** **Activated**
- **Entity tool** **Activated**
- **Element remover** **Activated**
- **Fill tool** **AreaComputed**
- **Canvas** **Filled**
- **Square tool** **RotationUpdated**

## **Biometric Events**
- **Preprocessing** **Completed**
- **Fragmentation** **Completed**
- **UL22** **Generated**
- **Skeleton** **Generated**
- **Branch Detection Algorithm** **Computed**
- **Structural features**s **Detected**
- **Structural features** **Detected**
- **Grid features** **Created**

## **System Events**
- **RealTimePreviewUpdated**

## **Domain Expert Validation Events** 
- **Fragmentation** **Displayed**
- **Structural features** **Highlighted**

---

# **High-level Domain Flow**

---

## **1. **Workspace** Manipulation**
1. **Workspace** **Created**
2. **Workspace** **GridInitialized**
3. **Group** **Added**
4. **Square** **Placed** / **Entity** **Placed** / **Preset** **Placed**
5. **Element** **Erased** (optional)
6. **Grid border** **Detected**
7. **Workspace** **Saved**

## **2. Process Image**
1. **RGBImageImported**
2. **Run** **Preprocessing**
3. **Preprocessing** **Completed**
4. **Run** **Fragmentation**
5. **Fragmentation** **Completed**  

## **3. Biometric Pipeline**
1. **Skeletonization** **Completed**  
2. **Branch Detection Algorithm** **Computed**  
3. **Structural features** **Detected**  
4. **Grid features** **Created**  
