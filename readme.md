# WebAPI dla enova365 — integracja ze sklepem internetowym

Dodatek wystawiający dane towarowe i stany magazynowe z enova365.
Sklep internetowy odpytuje endpoint i synchronizuje u siebie asortyment, ceny i dostępność.

## Struktura

| Plik | Zawartość |
|---|---|
| `Towary.cs` | Kontrakt API: interfejs, filtry, obiekty odpowiedzi. Udokumentowany komentarzami XML — to jego czyta strona integrująca się z systemem. |
| `TowaryService.cs` | Implementacja: pobranie towarów, wyliczenie stanów, stronicowanie, oznaczanie synchronizacji. |
| `ZmianyStanuTowaru.cs` | Nasłuch zmian na dokumentach handlowych — oznaczanie towarów wymagających ponownej wysyłki. |
| `EksportCSV.cs` | Alternatywna ścieżka: zrzut tych samych danych do pliku, dla odbiorców bez integracji przez API. |

## Decyzje projektowe

**Wąski kontrakt zamiast odwzorowania modelu enovy.** API zwraca konkretny zestaw pól
potrzebnych sklepowi, a nie pełną kartotekę towaru. Zmiany po stronie ERP nie wymuszają
wtedy zmian u odbiorcy.

**Synchronizacja przyrostowa przez cechę.** Zmiana dokumentu handlowego oznacza powiązane
towary jako zmienione; sklep pobiera tylko te pozycje. Pobranie zdejmuje flagę, więc jest
jednocześnie potwierdzeniem synchronizacji — stąd nietypowe stronicowanie, opisane
w komentarzu przy metodzie.

**Niezależna sesja przy zapisie z eventu.** Modyfikacja danych z poziomu zdarzenia
na dokumencie kończy się konfliktem zapisu w trakcie edycji. Rozwiązane przez osobną
sesję z własną transakcją.

**Magazyny wskazane w kodzie.** Do sklepu trafia konkretny podzbiór magazynów.
Świadomie nie wystawione jako konfiguracja użytkownika — przypadkowa zmiana oznaczałaby
błędne stany widoczne dopiero u kupującego.

**Rozdzielone stany.** API zwraca `Stan`, eksport CSV `StanFizyczny`

## Wymagania

- enova365 z modułami Handel, Towary, Magazyny
- Cecha `TowarZmieniony` na kartotece towaru
- Visual Studio obsługujące format `.slnx`

---

*README przygotowane z pomocą Claude.*
