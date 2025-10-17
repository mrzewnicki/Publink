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