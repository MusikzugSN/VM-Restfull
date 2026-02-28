![Lizenz: GPL v3](https://img.shields.io/badge/Lizenz-GPLv3-blue.svg)
![.NET](https://img.shields.io/badge/.NET-512BD4.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![C#](https://img.shields.io/badge/C%23-239120.svg?style=for-the-badge&logo=c-sharp&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-5C2D91.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![Entity Framework](https://img.shields.io/badge/Entity_Framework-512BD4.svg?style=for-the-badge&logo=dotnet&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED.svg?style=for-the-badge&logo=docker&logoColor=white)

Note: This repo is stil in development. Do not use in production. 

# VM-Restfull
Dieses Repository enthält das Backend der Verwaltungsplattform für deinen Musikverein. Die Anwendung basiert auf ASP.NET Core und stellt eine sichere, skalierbare REST-API für das Angular-Frontend bereit.

## Lizenz

Dieses Projekt steht unter der GNU General Public License v3.0 (GPLv3).
Der vollständige Lizenztext befindet sich in der Datei `LICENSE` in diesem Repository.
Mit Beiträgen zu diesem Projekt erklärst du dich einverstanden, dass deine Änderungen ebenfalls unter der GPLv3 veröffentlicht werden.

### Was bedeutet GPLv3 für dich?

- Du darfst die Software frei nutzen, verändern und weitergeben.
- Wenn du veränderte Versionen veröffentlichst, müssen diese ebenfalls unter GPLv3 stehen.
- Die Software wird ohne Garantie bereitgestellt (siehe Lizenztext).

[Vollständige README](https://github.com/MusikzugSN/VM-Web/blob/main/README.md)


## 4. Architektur
### Projektstruktur (Backend – C#)

- **Autofac/**
  Enthält das Konfigurationsmodul für Dependency Injection (`ServerModule.cs`).

- **Controllers/**
  Beinhaltet die API‑Schnittstellen des Backends, aufgeteilt nach fachlichen Bereichen  
  (z. B. `AuthController`, `GroupController`, `RoleController`, `UserController`).

  - **DataTransferObjects/**
    Enthält DTO‑Klassen, die für den Datenaustausch zwischen Backend und Frontend genutzt werden.  
    Sie sorgen für klar definierte, serialisierbare Datenstrukturen und entkoppeln interne Modelle von externen Schnittstellen.

- **Database/**
  Enthält den Datenbankkontext (`ServerDatabaseContext.cs`), Metadaten (`MetaData.cs`) sowie Basisklassen im Unterordner **Base/**.

- **Migrations/**
  Verwaltung und Versionierung der Datenbankänderungen über Entity Framework Migrations.

- **Services/**
  Implementiert die Geschäftslogik des Systems. Dazu gehören u. a. Benutzerverwaltung, Rollen‑ und Gruppenlogik sowie Token‑Erstellung.

  - **Models/**
    Enthält interne Datenmodelle, die innerhalb der Services verwendet werden und nicht direkt an das Frontend gehen.

  Beispiele:  
  `UserService.cs`, `RoleService.cs`, `GroupService.cs`, `JwtTokenService.cs`, `PermissionService.cs`.

- **Utils/**
  Sammlung von Hilfsfunktionen und technischen Komponenten.

  - **Middleware/**
    Enthält Middleware‑Klassen, die in der Request‑Pipeline ausgeführt werden (z. B. Fehlerbehandlung, Kontextaufbau).

  Weitere Dateien:  
  `ErrorUtils.cs` (Fehlerverarbeitung),  
  `ReturnValue.cs` (standardisierte Rückgabeobjekte),  
  `UserContext.cs` (Informationen über den aktuellen Benutzer).

- **Properties/** und **Dependencies/**
  Projektkonfiguration sowie externe Abhängigkeiten.

Diese Struktur bietet eine klare Trennung zwischen API‑Schicht, Geschäftslogik, Datenzugriff und technischen Hilfskomponenten und sorgt damit für gute Wartbarkeit und Erweiterbarkeit.


## 5. Mitwirken
### Coderichtlinien (ASP.NET / C#)

- Verwende moderne C#- und .NET-Best Practices.
- Schreibe klar strukturierten, wartbaren und testbaren Code.
- Nutze Dependency Injection konsequent.
- Trenne Verantwortlichkeiten strikt (Clean Architecture / SOLID).
- Dokumentiere öffentliche Klassen, Methoden und komplexe Logik ausreichend.

#### Dateistruktur & Benennung

- Controller enden auf `Controller` (z. B. `MembersController.cs`).
- Interfaces beginnen mit `I` (z. B. `IMemberService.cs`).
- Services enden auf `Service` (z. B. `MemberService.cs`).
- Repository-Klassen enden auf `Repository` (z. B. `MemberRepository.cs`).
- Models/Entities verwenden reine Substantive (z. B. `Member.cs`, `Note.cs`).

#### Typisierung & Codequalität

- Alle Felder, Parameter und Rückgabewerte müssen typisiert sein.
- `dynamic` und `object` werden nur in begründeten Ausnahmefällen verwendet.
- Nullable Reference Types (`string?`) sollen bewusst und konsistent eingesetzt werden.
- Magic Strings und Magic Numbers sind zu vermeiden – nutze Konstanten oder Enums.

#### Asynchrone Programmierung

- Verwende `async`/`await` konsequent.
- Asynchrone Methoden enden auf `Async` (z. B. `GetMemberAsync()`).
- Blockierende Aufrufe wie `.Result` oder `.Wait()` sind zu vermeiden.

#### Dependency Injection & Architektur

- Services, Repositories und andere Abhängigkeiten werden über den Konstruktor injiziert.
- Keine statischen Klassen für Geschäftslogik.
- Business-Logik gehört in Services, nicht in Controller.
- Controller sollten schlank sein und nur Routing, Validierung und Response-Handling übernehmen.

#### Datenzugriff (Entity Framework Core)

- Nutze EF Core mit Migrations.
- LINQ-Abfragen sollen klar und lesbar sein.
- Keine komplexe Logik in LINQ-Statements – ggf. in Services auslagern.


#### Benennungskonventionen

- Klassen und Methoden im **PascalCase** (z. B. `GetMemberById`).
- Parameter und lokale Variablen im **camelCase** (z. B. `memberId`).
- Private Felder beginnen mit `_` (z. B. `_memberRepository`).
- Konstanten im **UPPER_CASE** (z. B. `MAX_FILE_SIZE`).
- Enums im PascalCase, Enum-Werte ebenfalls im PascalCase.


#### Fehlerbehandlung

- Exceptions werden nicht geschluckt.
- Nutze spezifische Exception-Typen.
- Verwende Middleware für globale Fehlerbehandlung.
- Rückgaben aus Controllern folgen HTTP-Standards (`NotFound()`, `BadRequest()`, `Ok()`, etc.).

