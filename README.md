# Projektarkitektur och Valda Bibliotek

## Översikt

Detta projekt är en modern Blazor-applikation för hantering av skadeärenden (claims). Applikationen är uppdelad i tydliga lager enligt best practices för skalbarhet, testbarhet och vidareutveckling.

---

## Arkitektur

```text
ASP.Claims.API/
├── API/
│   ├── Controllers/           # API-kontrollers (PropertyClaimController, VehicleClaimController, etc.)
│   ├── DTOs/                  # Data Transfer Objects för API requests/responses
│   ├── Validators/            # FluentValidation-klasser för DTOs
│   └── Resources/             # Lokalisering och felmeddelanden
├── Application/
│   ├── CQRS/
│   │   ├── Claims/
│   │   │   ├── Commands/          # Command-objekt för claims
│   │   │   ├── Queries/           # Query-objekt för claims
│   │   │   ├── CommandHandlers/   # Command-handlers för claims
│   │   │   └── QueryHandlers/     # Query-handlers för claims
│   ├── Interfaces/            # Tjänste- och repositorygränssnitt
│   ├── Services/              # Affärslogik, t.ex. ClaimStatusEvaluator
│   └── Profiles/              # AutoMapper-profiler (om används)
├── Domain/
│   ├── Entities/              # Domänmodeller (Claim, PropertyClaim, VehicleClaim, etc.)
│   ├── Enums/                 # Enum-typer för domänen (ClaimStatus, ClaimType, etc.)
│   └── ...                    # Annan domänlogik
├── Infrastructures/
│   ├── Repositories/          # Implementering av datalager (InMemoryClaimRepository, etc.)
│   └── ...                    # Annan infrastruktur (databaser, externa tjänster)
├── Middleware/
│   ├── Filters/               # Action filters (t.ex. FluentValidationActionFilter)
│   └── ExceptionHandlingMiddleware.cs # Global felhantering
├── Program.cs                 # Applikationens startpunkt och DI-setup
├── appsettings.json           # Konfigurationsfil
└── ...                        # Övriga rotfiler
```

```text2
User
  ↓
[Validation Filter]
  ↓
API Controller (DTO)
  ↓
[Authorization]
  ↓
AutoMapper (DTO → Command)
  ↓
Application Layer (CQRS)
  ↓
[Business Logic, Domain Events]
  ↓
AutoMapper (Command → Entity)
  ↓
Repository
  ↓
Database
```

Projektet är organiserat enligt följande lager och mappar:

-	**API**        
    - *Controllers*: API-kontrollers för hantering av HTTP-förfrågningar (t.ex. PropertyClaimController, VehicleClaimController)      
    - *DTOs*: Data Transfer Objects för kommunikation mellan klient och server      
    - *Validators*: Valideringsklasser för DTOs med FluentValidation      
- **Application**  
    - *CQRS*: Kommandon, queries och handlers för affärslogik (t.ex. CreatePropertyClaimCommand, GetAllClaimsQuery, ClaimQueryHandler)
    - *Interfaces*: Tjänste- och repositorygränssnitt (t.ex. IClaimRepository)
    - *Services*: Implementering av affärslogik och tjänster (t.ex. ClaimStatusEvaluator)
-	**Domain**    
    - *Entities*: Domänmodeller (t.ex. Claim, PropertyClaim, VehicleClaim)  
    - *Enums*: Enum-typer för domänen (t.ex. ClaimStatus, ClaimType)  
- **Infrastructures**  
    - *Repositories*: Implementering av datalager och tekniska beroenden (t.ex. InMemoryClaimRepository)  
- **Middleware**  
    -  *Filters*: Globala filter för t.ex. validering (FluentValidationActionFilter)  
    - *ExceptionHandling*: Global felhantering (ExceptionHandlingMiddleware)  
-	**Resources**  
    -	*Resursfiler* för lokalisering och felmeddelanden  

### Flöde

1.	API-kontroller tar emot och validerar inkommande data via DTOs och FluentValidation.
2.	CQRS-handlers i Application-lagret hanterar affärslogik och använder tjänster (t.ex. ClaimStatusEvaluator).
3.	Repositories i Infrastructure-lagret sköter dataåtkomst.
4.	Domänmodeller och enum-typer ligger i Domain-lagret.
5.	Felhantering och lokalisering hanteras via Middleware och Resources.

---

## Valda Bibliotek

### Api.Versioning

**Syfte:**  
Hantera och exponera olika versioner av API:et.

**Varför:**  
Möjliggör vidareutveckling och bakåtkompatibilitet, tydlig versionering i URL och headers.

### MediatR

**Syfte:**  
Implementerar CQRS-mönstret med kommandon, queries och handlers.

**Varför:**  
Tydlig separation av affärslogik och presentation, enkel testning och vidareutveckling.

### FluentValidation

**Syfte:**  
Definiera och applicera valideringsregler på domänmodeller på ett tydligt och återanvändbart sätt.

**Varför:**  
- Separera valideringslogik från modeller och UI.
- Stöd för komplexa regler och cross-property validering.
- Lätt att testa och underhålla.

---

### FluentResults

**Syfte:**  
Standardisera hanteringen av resultat och fel från tjänster och repositorys.

**Varför:**  
- Tydlig separation mellan lyckade och misslyckade operationer utan undantag för kontrollflöde.
- Underlättar felhantering och presentation av felmeddelanden i UI:t.
- Stöd för att skicka med felkoder, meddelanden och orsaker.

---

## Sammanfattning

Denna arkitektur möjliggör:

•	Tydlig separation av ansvar (presentation, affärslogik, data, validering)
•	Enhetlig och testbar felhantering med FluentResults
•	Effektiv och återanvändbar validering med FluentValidation
•	Skalbar och underhållbar kodbas för vidareutveckling
