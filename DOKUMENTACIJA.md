# Planer Putovanja – Tehnička dokumentacija

**Projekt:** Planer Putovanja  
**Tehnologije:** ASP.NET Core 8 MVC, C#, Bootstrap 5, CSS3, Chart.js  
**Autor:** Petar  
**Datum:** Lipanj 2026  

---

## Sadržaj

1. [Pregled projekta](#1-pregled-projekta)
2. [Struktura projekta](#2-struktura-projekta)
3. [Baza podataka i modeli](#3-baza-podataka-i-modeli)
4. [Kontroleri i logika](#4-kontroleri-i-logika)
5. [Oblikovanje web stranica – CSS i dizajn](#5-oblikovanje-web-stranica--css-i-dizajn)
6. [Shared Layout – zajednički predložak](#6-shared-layout--zajednički-predložak)
7. [Stranice i njihov izgled](#7-stranice-i-njihov-izgled)
8. [Responzivni dizajn (mobilni uređaji)](#8-responzivni-dizajn-mobilni-uređaji)
9. [Animacije i 3D efekti](#9-animacije-i-3d-efekti)
10. [Vanjske biblioteke](#10-vanjske-biblioteke)

---

## 1. Pregled projekta

**Planer Putovanja** je web aplikacija za organiziranje putovanja. Korisnik se može registrirati, prijaviti, kreirati putovanje, dodati destinacije, aktivnosti i troškove, pratiti budžet putem dashboarda te spremiti uspomene u albume sa fotografijama. Plan putovanja moguće je izvesti kao PDF dokument.

### Glavne funkcionalnosti

| Funkcionalnost | Opis |
|---|---|
| Kreiranje putovanja | Unos naziva, destinacije, datuma, budžeta i prijevoza |
| Višestruke destinacije | Jedno putovanje može imati više destinacija s redoslijedom |
| Aktivnosti | Dodavanje aktivnosti po danu i satu |
| Troškovi | Evidencija troškova s kategorijama, usporedba s budžetom |
| Dashboard | Grafikoni troškova, top destinacije, upozorenja o budžetu |
| Albumi i uspomene | Polaroid-stil albumi s fotografijama, ocjenom i osvrtom |
| Kontakt forma | Poruka se sprema u bazu podataka |
| PDF izvoz | Generiranje PDF dokumenta plana putovanja |
| Autentikacija | Prijava i registracija korisnika (ASP.NET Identity) |

---

## 2. Struktura projekta

```
PlanerPutovanja/
├── Controllers/          ← Kontroleri (logika stranica)
│   ├── HomeController.cs
│   ├── TripsController.cs
│   ├── DashboardController.cs
│   ├── AlbumsController.cs
│   ├── ExpensesController.cs
│   └── TripActivitiesController.cs
│
├── Models/               ← Modeli (klase koje predstavljaju podatke)
│   ├── Trip.cs
│   ├── TripDestination.cs
│   ├── TripActivity.cs
│   ├── Expense.cs
│   ├── TripAlbum.cs
│   ├── TripPhoto.cs
│   ├── ContactMessage.cs
│   ├── User.cs
│   ├── DashboardViewModel.cs
│   └── ApplicationDbContext.cs
│
├── Views/                ← Razor pogledi (HTML predlošci)
│   ├── Shared/
│   │   ├── _Layout.cshtml        ← Glavni predložak (navigacija + footer)
│   │   └── _LoginPartial.cshtml  ← Partial za prijavu/odjavu
│   ├── Home/
│   │   ├── Index.cshtml          ← Početna stranica
│   │   ├── Kontakt.cshtml        ← Kontakt stranica
│   │   ├── Destinacije.cshtml    ← Pregled destinacija
│   │   ├── Galerija.cshtml       ← Galerija slika
│   │   ├── Planer.cshtml         ← Info o planeru
│   │   └── Budzet.cshtml         ← Info o budžetu
│   ├── Trips/            ← Stranice za upravljanje putovanjima
│   ├── Dashboard/        ← Dashboard sa grafikonima
│   ├── Albums/           ← Albumi i uspomene
│   └── Expenses/         ← Troškovi
│
├── wwwroot/
│   └── css/
│       └── site.css      ← Glavni CSS stilovi (6500+ linija)
│
├── Services/
│   ├── TripPdfService.cs         ← Generiranje PDF-a
│   ├── GoogleMapsService.cs      ← Google Maps API (ruta i udaljenost)
│   └── WeatherService.cs         ← Vremenska prognoza API
│
├── Migrations/           ← EF Core migracije baze
└── Program.cs            ← Konfiguracija aplikacije
```

---

## 3. Baza podataka i modeli

Aplikacija koristi **Entity Framework Core** s **SQL Server** bazom podataka (LocalDB za razvoj). Baza se kreira automatski putem migracija.

### Dijagram veza između tablica

```
User (ASP.NET Identity)
 └── Trip (1 korisnik : više putovanja)
      ├── TripDestination (1 putovanje : više destinacija)
      ├── TripActivity (1 putovanje : više aktivnosti)
      ├── Expense (1 putovanje : više troškova)
      └── TripAlbum (1 putovanje : 1 album)
           └── TripPhoto (1 album : više fotografija)

ContactMessage (neovisno – bez veze s korisnikom)
```

### Opis ključnih modela

**Trip.cs** – Model putovanja:
```csharp
public class Trip {
    public int Id { get; set; }
    public string Name { get; set; }         // Naziv putovanja
    public string Destination { get; set; }  // Glavna destinacija
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Budget { get; set; }      // Planirani budžet
    public TransportMode Transport { get; set; } // Enum: Auto, Avion, Vlak...
    public string UserId { get; set; }       // Veza s korisnikom
    
    // Navigacijska svojstva (EF Core učitava vezane podatke)
    public List<TripDestination> Destinations { get; set; }
    public List<TripActivity> Activities { get; set; }
    public List<Expense> Expenses { get; set; }
    public TripAlbum Album { get; set; }
}
```

**DashboardViewModel.cs** – Podaci za dashboard (nije direktno u bazi, konstruira se u kontroleru):
```csharp
public class DashboardViewModel {
    public int TotalTrips { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalBudget { get; set; }
    public decimal BudgetUsagePercent { get; set; }  // Postotak potrošnje
    public List<Trip> UpcomingTrips { get; set; }    // Nadolazeća putovanja
    public List<OverBudgetTripItem> OverBudgetTrips { get; set; } // Upozorenja
    public List<MonthlyExpenseItem> MonthlyExpenses { get; set; } // Za grafikon
    public List<TopDestinationItem> TopDestinations { get; set; } // Za grafikon
}
```

---

## 4. Kontroleri i logika

### HomeController.cs

Upravljuje statičkim stranicama i kontakt formom.

```csharp
// Prikazuje početnu stranicu
public IActionResult Index() => View();

// GET – prikazuje praznu kontakt formu
public IActionResult Kontakt() => View(new ContactMessage());

// POST – prima i sprema kontakt poruku u bazu
[HttpPost]
public async Task<IActionResult> Kontakt(ContactMessage model)
{
    if (ModelState.IsValid) {
        _context.ContactMessages.Add(model);
        await _context.SaveChangesAsync();
        TempData["ContactSuccess"] = true;
        return RedirectToAction("Kontakt");
    }
    return View(model);
}
```

### TripsController.cs

CRUD operacije za putovanja. Koristi `[Authorize]` atribut – samo prijavljeni korisnici mogu koristiti.

Ključna logika u `Details` akciji:
- Učitava putovanje zajedno s destinacijama, aktivnostima, troškovima i albumom (`Include`)
- Poziva Google Maps servis za izračun rute između destinacija
- Poziva Weather servis za vremensku prognozu
- Proslijeđuje sve podatke u View

### DashboardController.cs

Gradi `DashboardViewModel` iz podataka u bazi:
- Zbraja sve troškove i budžete korisnika
- Računa postotak potrošnje: `(troškovi / budžet) * 100`
- Pronalazi nadolazeća putovanja: `StartDate > DateTime.Today`
- Pronalazi putovanja iznad budžeta: troškovi ≥ 90% budžeta
- Grupira troškove po putovanju za grafikon
- Broji najčešće destinacije za grafikon

### AlbumsController.cs

Upravljanje albumima i fotografijama. Fotografija se ne uploaduje kao datoteka, nego se sprema kao URL adresa slike (npr. Unsplash link ili direktan URL).

---

## 5. Oblikovanje web stranica – CSS i dizajn

Sav vizualni dizajn nalazi se u jednoj datoteci: `wwwroot/css/site.css` (6500+ linija).

### 5.1 CSS varijable – dizajn sustav

Na samom vrhu `site.css` definirane su CSS custom properties (varijable) koje definiraju cijeli dizajn sustav:

```css
:root {
    --primary:      #0f172a;  /* Tamno plava – glavna boja teksta i pozadina */
    --accent:       #14b8a6;  /* Teal/zelena – akcent boja (gumbi, highlighti) */
    --accent-2:     #38bdf8;  /* Svijetlo plava – sekundarni akcent */
    --accent-dark:  #0f766e;  /* Tamna teal – hover stanja, linkovi */
    --soft:         #f8fafc;  /* Vrlo svijetla siva – pozadine sekcija */
    --muted:        #64748b;  /* Siva – sekundarni tekst, opisi */
    --danger:       #ef4444;  /* Crvena – greške, opasnost */
    
    --shadow-soft: 0 14px 34px rgba(15,23,42,.08);   /* Blag shadow za kartice */
    --shadow:      0 24px 58px rgba(15,23,42,.14);   /* Srednji shadow */
    --shadow-dark: 0 34px 88px rgba(0,0,0,.32);      /* Jak shadow (3D elementi) */
    
    --radius:    24px;  /* Zaobljeni kutovi kartica */
    --radius-lg: 34px;  /* Veće zaobljenje (hero elementi) */
}
```

**Zašto varijable?** Korištenjem varijabli sve stranice dijele isti skup boja i sjenki. Ako bi se promijenila `--accent` boja, automatski bi se promijenila na svim elementima koji je koriste – gumbima, linkovima, ikonama itd.

### 5.2 Globalni stilovi

```css
* {
    box-sizing: border-box;  /* Padding ne povećava veličinu elementa */
}

html {
    font-size: 16px;          /* Osnova za rem jedinice */
    scroll-behavior: smooth;  /* Glatko skrolanje na sidra */
}

body {
    font-family: "Segoe UI", Arial, sans-serif;
    background: var(--soft);  /* Svjetlosiva pozadina */
    color: #172033;
    overflow-x: hidden;       /* Sprječava horizontalni scroll */
}

a {
    text-decoration: none;    /* Uklanja podcrtavanje svih linkova */
}
```

### 5.3 Boje i gradijenti

Aplikacija koristi **tamnu paletu** za hero sekcije (navigacija, footer, hero banneri) i **bijelu/svjetlosivu** za sadržajne sekcije.

**Hero pozadina** (koristi se na svim hero sekcijama):
```css
background: 
    radial-gradient(circle at 18% 18%, rgba(20,184,166,.22), transparent 28%),
    radial-gradient(circle at 85% 18%, rgba(56,189,248,.18), transparent 30%),
    linear-gradient(135deg, #020617 0%, #0f172a 55%, #082f49 100%);
```
Objašnjenje:
- Dva radijalna gradijenta stvaraju svjetleće "orb" efekte (teal i plava)
- Linearni gradijent čini tamnu pozadinu od gotovo crne prema tamno plavoj
- Kombinirani daju efekt dubine i premium izgleda

**Gumbi (btn-main)**:
```css
.btn-main {
    background: linear-gradient(135deg, var(--accent), var(--accent-2));
    box-shadow: 0 16px 35px rgba(20,184,166,.28);
}
```
Dijagonalni gradijent od teal prema plavoj, s mekim sjenom koji daje "lebdeći" efekt.

### 5.4 Tipografija

```css
.section-title {
    font-size: clamp(2rem, 4vw, 3.2rem);  /* Responzivna veličina */
    font-weight: 950;                       /* Extra-bold */
    letter-spacing: -1.5px;                /* Negativni razmak slova – moderan izgled */
    color: var(--primary);
}
```

**`clamp(minimum, preferirano, maksimum)`** – automatski prilagođava veličinu fonta:
- Na malim ekranima: 2rem
- Na średnjim ekranima: 4% širine viewporta
- Na velikim ekranima: max 3.2rem

**Font weight 950** – aplikacija koristi jako visoke težine fonta (850, 950) kako bi naslovi izgledali moderno i snažno.

### 5.5 Kartice (Cards)

Svi paneli, kartice i okviri dijele isti vizualni stil:

```css
.glass-card, .card-premium, .dashboard-card, .details-panel, 
.create-trip-card, .side-card, .simple-form-card {
    background: white;
    border-radius: var(--radius);           /* 24px zaobljeni kutovi */
    box-shadow: var(--shadow-soft);         /* Blag shadow */
    border: 1px solid rgba(15,23,42,.06);  /* Jedva vidljivi rub */
}
```

### 5.6 Navigacijska traka

```css
.custom-navbar, .premium-navbar {
    padding: 14px 0;
    background: rgba(15,23,42,.78);      /* Tamna pozadina s 78% prozirnosti */
    border-bottom: 1px solid rgba(255,255,255,.12);
    backdrop-filter: blur(20px);          /* Efekt zamućenog stakla (glassmorphism) */
    -webkit-backdrop-filter: blur(20px);  /* Za Safari pregledač */
    box-shadow: 0 18px 45px rgba(15,23,42,.22);
}
```

**`backdrop-filter: blur(20px)`** je CSS svojstvo koje zamućuje sadržaj iza elementa – tzv. **glassmorphism** efekt. Navigacija je poluprozirna i zamućuje stranicu ispod, što daje moderan izgled. Navigacija je fiksirana (`fixed-top`) i ostaje vidljiva pri skrolanju.

**Logo navigacije:**
```css
.brand-icon {
    width: 42px;
    height: 42px;
    border-radius: 16px;
    background: linear-gradient(135deg, var(--accent), var(--accent-2));
    box-shadow: 0 14px 30px rgba(20,184,166,.28);
    transform: rotate(-8deg);  /* Blagi naklon za dinamičan izgled */
}
```

**Nav linkovi:**
```css
.custom-navbar .nav-link {
    color: rgba(255,255,255,.78) !important;
    font-weight: 850;
    border-radius: 999px;          /* Potpuno zaobljeni */
    transition: .22s ease;
}

.custom-navbar .nav-link:hover {
    color: white !important;
    background: rgba(255,255,255,.1);  /* Bijela pozadina s 10% prozirnosti */
    transform: translateY(-1px);        /* Lagano se pomiče gore pri hoveru */
}
```

**Highlightani link** (Novo putovanje):
```css
.nav-highlight {
    color: white !important;
    background: linear-gradient(135deg, var(--accent), var(--accent-2)) !important;
    box-shadow: 0 14px 30px rgba(20,184,166,.25);
}
```

### 5.7 Footer

Footer ima isti vizualni stil kao navigacija (tamna pozadina s gradijentom). Sadrži 4 stupca s linkovima, kratki opis aplikacije, mini statistike i copyright.

```css
.site-footer, .premium-footer {
    padding: 70px 0 28px;
    color: white;
    background: 
        radial-gradient(circle at 16% 20%, rgba(20,184,166,.18), transparent 28%),
        radial-gradient(circle at 86% 24%, rgba(56,189,248,.15), transparent 30%),
        linear-gradient(135deg, #020617 0%, #0f172a 58%, #082f49 100%);
}
```

Footer linkovi pri hoveru pomiču se desno:
```css
.footer-links a:hover {
    color: white;
    padding-left: 4px;  /* Pomak u desno – vizualna interakcija */
}
```

### 5.8 Forme i unosi podataka

```css
.form-control, .form-select {
    border-radius: 18px;                       /* Zaobljeni inputi */
    padding: 14px 16px;
    border: 1px solid rgba(15,23,42,.12);
    min-height: 54px;                          /* Viši od defaultnog (pristupačnost) */
}

.form-control:focus, .form-select:focus {
    border-color: var(--accent);               /* Teal rub pri fokusu */
    box-shadow: 0 0 0 4px rgba(20,184,166,.12) !important; /* Glow efekt */
}
```

**Validacijska poruka greške:**
```css
.validation-summary, .validation-banner {
    border-radius: 18px;
    padding: 14px 18px;
    color: #991b1b;
    background: linear-gradient(135deg, #fef2f2, #fee2e2);
    border: 1px solid #fecaca;
}

/* Sakriva praznu validaciju (kad nema greške) */
.validation-summary:empty, .validation-summary-valid {
    display: none !important;
    height: 0 !important;
    margin: 0 !important;
    padding: 0 !important;
}
```

### 5.9 Gumbi

Aplikacija ima vlastite CSS klase za gumbe (ne defaultni Bootstrap gumbi):

| Klasa | Namjena | Izgled |
|---|---|---|
| `.btn-main` | Primarni gumb | Teal gradijent, bijeli tekst, shadow |
| `.btn-ghost` | Sekundarni gumb | Bijeli, tanki rub, taman tekst |
| `.btn-card` | Gumb unutar kartice | Manji padding (10px 16px) |
| `.btn-small` | Mali gumb | Ista veličina kao btn-card |
| `.mini-btn` | Ikonica gumb | 34x34px, zaobljeni, za akcije u listama |

```css
.btn-main {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    padding: 14px 26px;
    border: none;
    border-radius: 999px;   /* Pill oblik */
    font-weight: 900;
    color: white;
    background: linear-gradient(135deg, var(--accent), var(--accent-2));
    box-shadow: 0 16px 35px rgba(20,184,166,.28);
    transition: .25s ease;
}

.btn-main:hover {
    transform: translateY(-3px);                   /* Lebdi gore */
    box-shadow: 0 20px 44px rgba(20,184,166,.38); /* Jači shadow */
}
```

### 5.10 Statusne oznake putovanja

```css
.trip-status {
    display: inline-flex;
    border-radius: 999px;
    padding: 6px 11px;
    font-size: .76rem;
    font-weight: 900;
}

.status-upcoming { color: #047857; background: #d1fae5; }  /* Zelena */
.status-active   { color: #1d4ed8; background: #dbeafe; }  /* Plava */
.status-past     { color: #475569; background: #e2e8f0; }  /* Siva */
```

Svaki status dobiva drugu boju pozadine i teksta za jasnu vizualnu razliku.

### 5.11 Filter bar (pill navigacija)

```css
.filter-pill {
    display: inline-flex;
    align-items: center;
    border-radius: 999px;
    padding: 10px 16px;
    color: var(--primary);
    background: white;
    box-shadow: var(--shadow-soft);
    border: 1px solid rgba(15,23,42,.06);
    transition: .25s ease;
}

.filter-pill:hover, .filter-pill.active {
    color: white;
    background: linear-gradient(135deg, var(--accent), var(--accent-2));
    transform: translateY(-2px);
}
```

Aktivni filter dobiva isti stil kao `.btn-main`.

---

## 6. Shared Layout – zajednički predložak

Datoteka `Views/Shared/_Layout.cshtml` je **master predložak** koji se primjenjuje na svaku stranicu. Sadrži:
- `<head>` s meta tagovima, linkovima na CSS
- Navigacijsku traku
- `@RenderBody()` – gdje se ubacuje sadržaj svake stranice
- Footer
- Skripte (jQuery, Bootstrap JS)
- `@await RenderSectionAsync("Scripts")` – za skripte specifične stranici

```html
<link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
<link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
```

**`asp-append-version="true"`** automatski dodaje hash u URL fajla (npr. `site.css?v=abc123`) kako bi preglednik uvijek učitao najnoviju verziju CSS-a (cache busting).

### Navigacijska traka (detalji)

```html
<nav class="navbar navbar-expand-lg custom-navbar premium-navbar fixed-top">
```

- `navbar-expand-lg` – Bootstrap klasa: na lg ekranima (≥992px) navigacija je horizontalna, ispod te širine se sklapa u hamburger izbornik
- `fixed-top` – Bootstrap klasa: navigacija ostaje fiksirana na vrhu pri skrolanju
- `custom-navbar premium-navbar` – vlastite CSS klase za glazmorphism efekt

**Hamburger gumb za mobilne uređaje:**
```html
<button class="navbar-toggler custom-toggler premium-toggler"
        data-bs-toggle="collapse"
        data-bs-target="#mainNavbar">
    <span></span>
    <span></span>
    <span></span>
</button>
```

Tri `<span>` elementa su tri crte hamburger ikone, stilizirane CSS-om:
```css
.custom-toggler span {
    display: block;
    width: 22px;
    height: 2px;
    margin: 0 auto;
    border-radius: 999px;
    background: white;
}
```

---

## 7. Stranice i njihov izgled

### 7.1 Početna stranica (Home/Index)

**Struktura:**
```
Section 1: Hero sekcija (home-3d-hero)
  └── Grid: Tekst lijevo | 3D mockup dashboard desno

Section 2: Mogućnosti aplikacije (3 kartice)

Section 3: Kako radi? (3 koraka)

Section 4: Call-to-action (CTA)
```

**Hero sekcija – CSS layout:**
```css
.home-3d-grid {
    display: grid;
    grid-template-columns: minmax(0,1fr) 520px;  /* Tekst fleksibilan | mockup fiksnih 520px */
    gap: 70px;
    align-items: center;
}
```

**Dashboard mockup** (lažni prikaz aplikacije na hero sekciji):
```css
.dashboard-mockup {
    width: 430px;
    border-radius: 36px;
    background: linear-gradient(145deg, rgba(255,255,255,.96), rgba(226,232,240,.92));
    box-shadow: var(--shadow-dark), inset 0 1px 0 rgba(255,255,255,.9);
    transform: rotateY(-17deg) rotateX(8deg) rotateZ(2deg);  /* 3D nagib */
    transform-style: preserve-3d;
}

.dashboard-mockup:hover {
    transform: rotateY(-10deg) rotateX(5deg) rotateZ(1deg) translateY(-8px);
}
```

Mockup je zarotiran u 3D prostoru (`rotateX`, `rotateY`, `rotateZ`) kako bi izgledao kao da lebdi pod kutom. `transform-style: preserve-3d` omogućuje da djeca (floating badges) budu u istom 3D prostoru.

**"Orb" efekti:**
```css
.home-orb {
    position: absolute;
    border-radius: 999px;
    opacity: .7;
    pointer-events: none;
    animation: orbFloat 8s ease-in-out infinite;
}

.orb-one { width: 230px; height: 230px; left: 7%; top: 18%; background: rgba(20,184,166,.15); }
.orb-two { width: 340px; height: 340px; right: -90px; top: 18%; background: rgba(56,189,248,.13); animation-delay: -2s; }
.orb-three { width: 180px; height: 180px; left: 45%; bottom: 8%; background: rgba(14,165,233,.12); animation-delay: -4s; }
```

Tri kružna elementa s prozirnim teal/plavim bojama lebde animacijom. `pointer-events: none` znači da ne blokiraju klikove.

**Feature kartice (3D efekt na hover):**
```css
.feature-3d-card {
    transform: perspective(1000px) rotateX(0) rotateY(0);
    transition: .3s ease;
}

.feature-3d-card:hover {
    transform: perspective(1000px) rotateX(3deg) rotateY(-4deg) translateY(-8px);
    box-shadow: var(--shadow);
}
```

`perspective(1000px)` definira koliko je daleko "gledatelj" od elementa. Veća vrijednost = blaži 3D efekt. Na hoveru kartica se nakloni i podigne.

### 7.2 Dashboard (Dashboard/Index)

Dashboard prikazuje analitiku i statistike korisnikovih putovanja.

**Struktura:**
```
Section 1: Hero s budget ring-om
Section 2: 4 stat kartice (ukupno putovanja, troškovi, budžet, iskorištenost)
Section 3: Grafikoni (Chart.js) + nadolazeća putovanja + upozorenja budžeta
```

**Dinamička boja progress bara:**
```razor
@{
    var usageColorClass = "dashboard-danger";
    
    if (Model.BudgetUsagePercent < 70m)      usageColorClass = "dashboard-success";
    else if (Model.BudgetUsagePercent <= 90m) usageColorClass = "dashboard-warning";
}

<div class="command-progress">
    <div class="@usageColorClass" style="width: @usageWidth;"></div>
</div>
```

Postotak budžeta dinamički mijenja CSS klasu i širinu progress bara.

**Chart.js grafikoni** koriste se za:
- **Linijski grafikon** – troškovi po putovanjima
- **Horizontalni bar grafikon** – top destinacije

Podaci se prosljeđuju iz C# u JavaScript kroz JSON:
```razor
var monthlyLabelsJson = JsonSerializer.Serialize(Model.MonthlyExpenses.Select(x => x.Label).ToList());

@* U JavaScript sekciji: *@
const monthlyLabels = JSON.parse('@JavaScriptEncoder.Default.Encode(monthlyLabelsJson)');
```

### 7.3 Moja putovanja (Trips/Index)

Prikazuje popis svih putovanja korisnika u grid prikazu (karte).

**Statistike na vrhu** (uzimaju se direktno iz podataka modela u Razor predlošku):
```razor
var totalTrips    = trips.Count;
var upcomingTrips = trips.Count(t => t.StartDate > DateTime.Today);
var activeTrips   = trips.Count(t => t.StartDate <= DateTime.Today && t.EndDate >= DateTime.Today);
var pastTrips     = trips.Count(t => t.EndDate < DateTime.Today);
```

**Trip kartica:**
```css
.trip-card {
    height: 100%;
    padding: 20px;
    border-radius: 24px;
    background: white;
    box-shadow: var(--shadow-soft);
    border: 1px solid rgba(15,23,42,.06);
}

.trip-card:hover {
    transform: perspective(1000px) rotateX(3deg) rotateY(-4deg) translateY(-8px);
    box-shadow: var(--shadow);
}
```

**Grid raspored kartica** koristi Bootstrap:
```html
<div class="col-md-6 col-xl-4">
```
- Na md ekranima (≥768px): 2 kartice u redu
- Na xl ekranima (≥1200px): 3 kartice u redu

### 7.4 Albumi i uspomene (Albums/Index)

Polaroid-stil prikaz albuma koji imitira fizičke Polaroid fotografije.

**Polaroid efekt:**
```css
.memory-album-polaroid {
    padding: 14px 14px 22px;      /* Deblji padding na dnu = bijeli okvir Polaroida */
    border-radius: 16px;
    background: linear-gradient(180deg, #ffffff 0%, #fefefe 72%, #f8fafc 100%);
    box-shadow: 0 24px 58px rgba(15,23,42,.15), inset 0 1px 0 rgba(255,255,255,.9);
    transform: rotate(-1.2deg);   /* Blagi nagib ulijevo */
}

.memory-album-polaroid:nth-child(even) {
    transform: rotate(1.2deg);    /* Svaki drugi je nagnut udesno */
}

.memory-album-polaroid:nth-child(3n) {
    transform: rotate(-0.4deg);   /* Svaki treći = minimalni nagib */
}
```

**Scotch tape efekt** (lepljiva traka) CSS pseudo-elementom:
```css
.memory-album-polaroid::before {
    content: "";
    position: absolute;
    width: 78px;
    height: 24px;
    left: 50%;
    top: -13px;
    transform: translateX(-50%) rotate(-2deg);
    border-radius: 5px;
    background: rgba(203,213,225,.55);   /* Prozirna siva = traka */
    box-shadow: 0 5px 14px rgba(15,23,42,.08);
}
```

Hover efekt izravna Polaroid i lagano ga povisi:
```css
.memory-album-polaroid:hover {
    transform: rotate(0deg) translateY(-10px) scale(1.018);
    border-color: rgba(20,184,166,.18);
    z-index: 5;   /* Da se prikaže iznad ostalih */
}
```

### 7.5 Kontakt stranica (Home/Kontakt)

Stranica ima hero sekciju, kontakt formu i informacijski panel.

**Forma s validacijom:**
```html
<form asp-action="Kontakt" asp-controller="Home" method="post">
    @Html.AntiForgeryToken()
    <div asp-validation-summary="ModelOnly" class="validation-banner"></div>
    
    <input asp-for="Name" class="form-control" placeholder="Unesi ime" />
    <span asp-validation-for="Name"></span>
```

- `@Html.AntiForgeryToken()` – sigurnosni token koji sprječava CSRF napade (lažne zahtjeve)
- `asp-validation-summary` – prikazuje sve greške validacije
- `asp-validation-for` – prikazuje grešku za konkretno polje

**Poruka uspjeha:**
```razor
@if (TempData["ContactSuccess"] != null)
{
    <div class="contact-success-premium show">
        <strong>Poruka je uspješno poslana!</strong>
    </div>
}
```

`TempData` se koristi za privremene poruke koje preživljavaju jedan redirect.

### 7.6 Stranica za prijavu i registraciju

Koristi ASP.NET Identity Razor Pages (u `Areas/Identity/`).

```css
.auth-page {
    min-height: calc(100vh - 80px);    /* Puna visina minus navigacija */
    display: flex;
    align-items: center;
    justify-content: center;
    background: radial-gradient(circle at 20% 20%, rgba(20,184,166,.18), transparent 32%),
                radial-gradient(circle at 80% 10%, rgba(56,189,248,.18), transparent 30%),
                linear-gradient(135deg, #f8fafc, #eef2ff);
}

.auth-shell {
    width: min(1120px, 100%);              /* Max 1120px, responsive */
    display: grid;
    grid-template-columns: 1.15fr .85fr;  /* Slika lijevo (veće) | forma desno */
    border-radius: 34px;
    overflow: hidden;
    background: white;
    box-shadow: var(--shadow);
}
```

Auth stranica je podijeljena na:
- **Lijeva strana** (`auth-visual`): pozadinska fotografija planine s tekstom
- **Desna strana** (`auth-card`): forma za prijavu/registraciju

```css
.auth-visual {
    min-height: 650px;
    background: 
        linear-gradient(135deg, rgba(15,23,42,.92), rgba(15,23,42,.64)),
        url("https://images.unsplash.com/photo-1500530855697-b586d89ba3ee...");
    background-size: cover;
    background-position: center;
}
```

---

## 8. Responzivni dizajn (mobilni uređaji)

Aplikacija je potpuno responzivna kroz tri pristupa:

### 8.1 Bootstrap grid

Bootstrap `col-md-6 col-xl-4` automatski slaže kartice ovisno o širini zaslona.

### 8.2 CSS Grid s auto-prilagodbom

```css
.home-3d-grid {
    grid-template-columns: minmax(0,1fr) 520px;  /* Desktop: 2 stupca */
}

@media (max-width: 1100px) {
    .home-3d-grid {
        grid-template-columns: 1fr;  /* Tablet: 1 stupac, jedno ispod drugog */
    }
}
```

### 8.3 Media query breakpointi

| Breakpoint | Promjene |
|---|---|
| `max-width: 1100px` | Hero gridi prelaze u 1 stupac, galerija u 2 stupca |
| `max-width: 991px` | Navigacija kolapsira (hamburger), layouti u 1 stupac, auth visual se smanjuje |
| `max-width: 768px` | Badge elementi mijenjaju poziciju |
| `max-width: 576px` | Sve stoga, gumbi puni width, polaroid galerija 1 stupac, padding reduciran |

**Primjer: hamburger navigacija na mobilnom:**
```css
@media (max-width: 991px) {
    .custom-navbar .navbar-collapse {
        margin-top: 14px;
        padding: 16px;
        border-radius: 24px;
        background: rgba(15,23,42,.96);  /* Tamna pozadina izbornika */
        border: 1px solid rgba(255,255,255,.12);
    }
    
    .premium-register, .premium-login, .btn-logout {
        width: 100%;  /* Gumbi na punu širinu na mobilnom */
    }
}
```

**Primjer: 3D mockup na mobilnom:**
```css
@media (max-width: 576px) {
    .dashboard-mockup {
        width: min(360px, 96vw);   /* Max 360px ili 96% širine zaslona */
        transform: rotateY(-6deg) rotateX(4deg) rotateZ(1deg);  /* Manji 3D kut */
    }
    
    .floating-badge {
        position: relative;   /* Više nije apsolutno pozicioniran */
        left: auto !important;
        animation: none;      /* Bez animacije na mobilnom (performanse) */
        transform: none !important;
    }
}
```

---

## 9. Animacije i 3D efekti

### 9.1 Orb animacija (lebdeće kugle)

```css
@keyframes orbFloat {
    0%, 100% {
        transform: translate3d(0, 0, 0) scale(1);
    }
    50% {
        transform: translate3d(0, -22px, 0) scale(1.05);  /* Gore 22px + blago uvećanje */
    }
}

.home-orb {
    animation: orbFloat 8s ease-in-out infinite;  /* 8 sekundi, zauvijek */
}

.orb-two  { animation-delay: -2s; }  /* Počinje 2s ranije – asinkrono lebdenje */
.orb-three { animation-delay: -4s; }
```

`ease-in-out` znači spori start i kraj, brz srednji dio – kao fizičko lebdenje.

### 9.2 Badge animacija

```css
@keyframes badgeFloat {
    0%, 100% { transform: translateY(0); }
    50%       { transform: translateY(-14px); }
}

.floating-badge {
    animation: badgeFloat 5s ease-in-out infinite;
}

.badge-two { animation-delay: -1.4s; }  /* Asinkroni s ostalim badgeovima */
.badge-three { animation-delay: -2.8s; }
```

### 9.3 CSS 3D transformacije

Aplikacija intenzivno koristi CSS 3D:

```css
/* Perspektiva definira "dubinu" scene */
.home-3d-stage { perspective: 1200px; }

/* Mockup zarotiran u 3D */
.dashboard-mockup {
    transform: rotateY(-17deg) rotateX(8deg) rotateZ(2deg);
    transform-style: preserve-3d;  /* Djeca su u istom 3D prostoru */
}

/* Badge izlazi "prema gledatelju" */
.hero-floating-badge.badge-destinations {
    transform: rotate(-8deg) translateZ(40px);  /* Z-os = prema naprijed */
}
```

### 9.4 Hover efekti kartica

```css
.feature-3d-card:hover, .trip-card:hover {
    transform: perspective(1000px) rotateX(3deg) rotateY(-4deg) translateY(-8px);
    box-shadow: var(--shadow);
}
```

`transition: .3s ease` na elementu osigurava glatki prijelaz između normalnog i hover stanja.

### 9.5 Polaroid rotacija

```css
/* nth-child selektori za individualne rotacije */
.polaroid-card { transform: rotate(-2deg); }

.polaroid-card:nth-child(2) { transform: rotate(2deg); }
.polaroid-card:nth-child(3) { transform: rotate(-1deg); }

.rotate-left        { transform: rotate(-2deg); }
.rotate-right       { transform: rotate(2deg); }
.rotate-small-left  { transform: rotate(-1deg); }
.rotate-small-right { transform: rotate(1deg); }
```

---

## 10. Vanjske biblioteke

### Bootstrap 5.x

Lokalno instaliran u `wwwroot/lib/bootstrap/`.

Koristi se za:
- **Grid sustav** (`col-md-6`, `col-xl-4`, `row`, `g-4`)
- **Navbar** komponenta (`navbar`, `navbar-expand-lg`, `collapse`)
- **Utility klase** (`ms-auto`, `align-items-center`, `text-center`, `d-flex`)
- **Form klase** – `form-control`, `form-select`, `form-label` (vizualno predefinirano, pa se zatim override-a u `site.css`)

### jQuery

Lokalno instaliran. Koristi se za jednostavnu DOM manipulaciju i validacijske skripte.

### jQuery Validate + Unobtrusive Validation

Klijentska validacija formi. Radi automatski s ASP.NET Data Annotations:

```csharp
// U modelu:
[Required(ErrorMessage = "Ime je obavezno")]
[StringLength(100)]
public string Name { get; set; }
```

Bootstrap validacija se pokreće bez refresh stranice zahvaljujući `jquery.validate.unobtrusive.js`.

### Chart.js (CDN)

```html
<script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
```

Koristi se na Dashboard stranici za:
- **Line chart** – trend troškova po putovanjima
- **Bar chart** (horizontalni) – top destinacije

Stiliziran je ručno da odgovara dizajnu:
```javascript
new Chart(ctx, {
    options: {
        plugins: {
            tooltip: {
                backgroundColor: '#020617',   /* Tamna tooltips pozadina */
                titleColor: '#ffffff',
                cornerRadius: 12              /* Zaobljeni tooltips */
            }
        },
        scales: {
            y: {
                ticks: { color: '#64748b' },                  /* Siva boja osi */
                grid:  { color: 'rgba(148,163,184,.22)' }     /* Prozirna rešetka */
            }
        }
    }
});
```

### iTextSharp (PDF generiranje)

Koristi se u `TripPdfService.cs` za generiranje PDF dokumenta s planom putovanja (destinacije, aktivnosti, troškovi).

---

## Zaključak

**Planer Putovanja** je cjelovita web aplikacija izgrađena na modernim principima web dizajna:

- **Glassmorphism** – zamućene poluprozirne površine (navigacija, kartice na hero sekcijama)
- **3D transformacije** – CSS `rotateX/Y/Z`, `perspective`, `preserve-3d`
- **CSS varijable** – centralizirani dizajn sustav (boje, sjenke, radijusi)
- **Responzivni grid** – CSS Grid + Bootstrap, media queries za 4 breakpointa
- **Animacije** – `@keyframes` za lebdeće orb elemente i badge animacije
- **Polaroid efekt** – CSS rotacija, box-shadow i `::before` pseudo-elementi
- **Gradijenti** – kombinirani radijalni i linearni gradijenti za bogat vizualni dojam

Vizualni identitet je konzistentan kroz sve stranice zahvaljujući:
1. Jedinstvenoj CSS datoteci s varijablama
2. Zajedničkom predlošku `_Layout.cshtml`
3. Ponovnoj upotrebi CSS klasa (npr. `.btn-main`, `.eyebrow`, `.section-title`)
