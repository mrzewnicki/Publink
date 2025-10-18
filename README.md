# Publink — Architektura rozwiązania (API i Klient)

Niniejszy dokument opisuje architekturę warstwy serwerowej (API) oraz aplikacji klienckiej (SPA) w zdaniu rekrutacyjnym.

## Przegląd na wysokim poziomie

Projekt składa się z dwóch głównych części:
- Server (ASP.NET Core, .NET 9): ekspozycja REST API, logika aplikacyjna (CQRS) oraz dostęp do danych (EF Core, repozytoria).
- Client (Vite + React + TypeScript): aplikacja SPA komunikująca się z API.

Struktura rozwiązania na poziomie katalogów:

```
Publink
├─ Server
│  ├─ Publink.Api        (warstwa interfejsu HTTP: kontrolery, konfiguracja API)
│  ├─ Publink.Core       (warstwa aplikacyjna: CQRS, logika, reguły domenowe)
│  ├─ Publink.Data       (warstwa dostępu do danych: EF Core, repozytoria, encje, DbContext)
│  └─ Publink.Shared     (modele współdzielone/DTO)
└─ Client
   └─ publink_app        (frontend: Vite, React, TypeScript)
```

## Warstwa serwerowa (API)

Warstwa serwerowa przestrzega podziału na warstwy, co ułatwia rozwijanie i utrzymanie kodu:

- Publink.Api
  - ASP.NET Core Web API
  - Kontrolery odpowiadają za mapowanie żądań HTTP na przypadki użycia (Queries/Commands) w warstwie aplikacyjnej.
  - Przykładowe kontrolery: `OrganisationsController`, `LogsController`.
- Publink.Core
  - Warstwa aplikacyjna, w której stosujemy wzorzec CQRS (rozdzielenie odczytów i zapisów).
  - Zawiera zapytania/komendy (np. w katalogu `CQRS/Queries`) wraz z handlerami przetwarzającymi logikę.
  - Odpowiada za walidację reguł biznesowych i orkiestrację dostępu do danych poprzez repozytoria.
- Publink.Data
  - Warstwa infrastrukturalna dla danych: EF Core (DbContext), encje domenowe oraz repozytoria.
  - Encje: `AuditLog`, `DocumentHeader`
  - Repozytoria: `AuditLogRepository`, `OrganisationRepository`.
- Publink.Shared
  - Kontrakty wymiany danych pomiędzy warstwami i z klientem (DTO), typy wspólne.

Typowa ścieżka przepływu żądania w API:

```
HTTP Request → Controller → (Query/Command) → Handler → Repository → DbContext/EF Core → DB
                                              ↘ mapowanie → DTO → Controller → HTTP Response
```

### CQRS
- Zapytania (Queries) oraz komendy (Commands) są definiowane jako osobne przypadki użycia.
- Handlery odpowiadają za realizację pojedynczego przypadku użycia i korzystają z repozytoriów oraz innych usług.
- Rozdzielenie odczytu od zapisu zwiększa czytelność i skalowalność logiki.

### Repozytoria i dostęp do danych
- Repozytoria izolują resztę systemu od szczegółów EF Core i modelu bazy danych.
- Encje odwzorowują strukturę danych, a DbContext odpowiada za sesję pracy z bazą.
- Logowanie/audyt zdarzeń aplikacyjnych jest realizowane z wykorzystaniem encji `AuditLog` oraz odpowiadającego repozytorium.

### Endpointy
- `GET /api/organisations` — pobranie listy organizacji (np. `OrganisationsController`).
- `GET /api/logs` — pobranie logów audytowych dla organizacji (np. `LogsController`).
- 'GET /swagger' — dokumentacja API (w trybie DEV).
- `GET /openapi/v1.json` — specyfikacja OpenAPI generowana automatycznie (w trybie DEV).

## Warstwa kliencka (SPA)

Aplikacja kliencka została zbudowana w oparciu o Vite, React i TypeScript.

- Vite (bundler/dev server) — szybkie środowisko deweloperskie i budowanie paczek produkcyjnych.
- React + TypeScript — komponentowy interfejs użytkownika z silnym typowaniem.
- Struktura katalogów (kluczowe miejsca):
  - `src/components` — komponenty prezentacyjne i kontenerowe gdzie najważniejszym, wiążącym komponentem jest LogsTable
  - `public` — statyczne zasoby.
  - `vite.config.ts` — konfiguracja narzędzia Vite (opcjonalne proxy do API podczas dev).

Komunikacja z API
- Klient komunikuje się z serwerem poprzez REST (HTTP). 
- Warstwa wywołań do API może być zorganizowana w serwisach/klientach HTTP (np. funkcje pomocnicze) używanych przez komponenty.
- Komponenty (np. `LogsTable`) pobierają dane z API i renderują je w tabelach/widokach.

Prosty przepływ danych end-to-end:

```
Użytkownik → UI (React) → Wywołanie HTTP do API → Kontroler → CQRS → Repozytorium/EF Core → DB
        ↘ render danych z DTO w komponentach ↙
```

## Konwencje i rozszerzanie

Dodawanie nowego przypadku użycia (Query):
1. Warstwa Core: dodaj Query + Handler (CQRS) implementujący logikę odczytu.
2. Warstwa Data: w razie potrzeby rozszerz repozytorium lub dodaj nowe metody.
3. Warstwa Api: dodaj action w odpowiednim kontrolerze i mapowanie DTO.
4. Klient: dodaj funkcję wywołującą endpoint oraz komponent/widok prezentujący dane.

Dodawanie nowego przypadku użycia (Command):
1. Warstwa Core: dodaj Command + Handler (CQRS) z walidacją reguł.
2. Warstwa Data: operacje zapisu w repozytoriach i DbContext.
3. Warstwa Api: endpoint przyjmujący dane wejściowe (DTO), mapowanie i obsługa wyników/błędów.
4. Klient: formularz/akcja w UI wywołująca endpoint, obsługa odpowiedzi i błędów.

## Zależności i technologia

- .NET 9, ASP.NET Core Web API (Server)
- Entity Framework Core (dostęp do danych)
- React 18+, TypeScript, Vite (Client)

## Uruchamianie (skrót)

Serwer (w katalogu `Server/Publink.Api`):
- dotnet restore
- dotnet run

Klient (w katalogu `Client/publink_app`):
- npm install
- npm run dev (dev)

## Wzorce projektowe i dobre praktyki — co i dlaczego

Poniżej zestawiam wzorce i praktyki zastosowane w projekcie wraz z krótkim uzasadnieniem wyborów

- CQRS (Command Query Responsibility Segregation)
  - Co: rozdzielenie operacji odczytu (Queries) od zapisu (Commands/Handlers) w warstwie aplikacyjnej (Publink.Core/CQRS)
  - Dlaczego: poprawa czytelności i skalowalności kodu. Łatwiejsze utrzymanie i testowanie przypadków użycia.Możliwość niezależnej optymalizacji ścieżek odczytu i zapisu

- Repozytorium (Repository) nad EF Core
  - Co: warstwa pośrednia między logiką aplikacyjną a DbContext/EF Core (np. AuditLogRepository).
  - Dlaczego: enkapsulacja dostępu do danych i zapytań. Ograniczenie zależności od szczegółów implementacyjnych EF. Łatwiejsze testowanie (możliwość mockowania), spójniejsze miejsce do optymalizacji zapytań.

- DTO (Data Transfer Objects) i kontrakty w warstwie Shared
  - Co: osobne modele przesyłania danych między API/klientem a logiką aplikacyjną (Publink.Shared/Dtos)
  - Dlaczego: separacja kontraktów API od encji/struktur bazodanowych. Kontrola ekspozycji pól (bezpieczeństwo) i stabilność publicznego API.

- Wstrzykiwanie zależności (Dependency Injection)
  - Co: wykorzystanie wbudowanego DI w ASP.NET Core do rejestrowania repozytoriów/usług i handlerów
  - Dlaczego: luźne powiązania między warstwami, łatwiejsza wymiana implementacji. wyższa testowalność i czytelna konfiguracja kompozycji aplikacji.

- Asynchroniczność (async/await)
  - Co: stosowanie asynchronicznych operacji IO przy pracy z bazą i siecią.
  - Dlaczego: lepsza skalowalność serwera pod obciążeniem oraz responsywność. Efektywne wykorzystanie zasobów.

- Spójność i prostota warstw
  - Co: wyraźny podział na Api/Core/Data/Shared oraz mirrorowanie tych granic w strukturze katalogów.
  - Dlaczego: mniejsza złożoność poznawcza, krótszy czas wdrożenia nowych osób, łatwiejsza nawigacja i refaktoryzacja

- Frontend: React + TypeScript + Vite
  - Co: komponenty funkcyjne, silne typowanie, szybki cykl dev/build. Konfiguracja w vite.config.ts
  - Dlaczego: czytelna i przewidywalna praca nad UI, wczesne wykrywanie błędów typów, szybkie środowisko developerskie.

- OpenAPI i Swagger
  - Co: automatyczna specyfikacja OpenAPI generowana przez ASP.NET Core (AddOpenApi/MapOpenApi) oraz dokumentacja UI przez Swashbuckle/Swagger UI.
  - Dlaczego: samoopisujące się API.Łatwiejsza integracja, szybkie poznanie endpointów przez UI

- Redux Toolkit
  - Co: Zarządzanie stanem w kliencie w celu strukturyzacji wykorzystywanych przez client danych.
  - Dlaczego: Ułatwia zarządzanie stanem aplikacji, szczególnie przy większej liczbie komponentów i złożonych interakcjach. Umożliwia łatwe debugowanie i śledzenie zmian stanu. Dzięki temu Client pobiera dane tylko dla nowych stron paginacji lub przy zmianie konfiguracji sortowania.