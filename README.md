# FoglalóPlusz

Szalon-alapú **időpontfoglaló** alkalmazás: vendégek, szakemberek és tulajdonosok számára regisztráció, bejelentkezés, szolgáltatások, munkaidő és foglalások kezelése böngészőből.

## Működés röviden

- **Backend:** ASP.NET Core 8 **REST API**, **JWT** Bearer hitelesítés, **Entity Framework Core** + **PostgreSQL**, induláskor **migráció** (éles/PG esetén). Statikus feltöltések (pl. szalon logó) a `wwwroot/uploads` alá kerülnek.
- **Frontend:** **Node.js** + **Express** kiszolgálja a HTML/JS/CSS nézeteket, és **proxyként** továbbítja a `/api` és `/uploads` kéréseket a .NET API felé — így a böngésző egy porton (alapértelmezés: **3000**) kommunikál.
- **Rétegek:** `Foglaloplusz` (presentation), `Foglaloplusz.BLL`, `Foglaloplusz.DAL`, tesztek: `Foglaloplusz.Tests`.

## Közreműködők (összefoglalva)

| Név | Főbb terület (commitok alapján) |
|-----|--------------------------------|
| **Sátori Kázmér Kevin** | Backend és API, auth (JWT), Express proxy, frontend integráció, Docker/élesítési finomítások, merge-ök és javítások. |
| **Magyar Dóra Brigitta** | Adatmodell, migrációk, szalon/tulaj funkciók, témák és űrlapok, UI részletek (stílus, animációk), hibajavítások. |

*(A részletes bemutatkozások a `docs/leadando-dokumentaciok/` mappában találhatók.)*

## Előfeltételek

- [.NET SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/) (LTS ajánlott)
- **PostgreSQL** (helyi vagy távoli példány)
- Opcionális: Docker (backend konténerhez)

## Helyi futtatás

1. **Adatbázis:** állítsd be a `Foglaloplusz/appsettings.Development.json` (vagy környezeti változók) **`ConnectionStrings:DefaultConnection`** értékét a saját PostgreSQL példányodra. Ne commitolj jelszót — éles titkok környezeti változóból jöjjenek.

2. **API** (a repó gyökeréből):

   ```bash
   dotnet run --project Foglaloplusz/Foglaloplusz.presentation.csproj
   ```

   Alap profil szerint HTTP: **http://localhost:5181** — **Swagger:** `/swagger`.

3. **Böngészős felület** (külön terminálban):

   ```bash
   cd Foglaloplusz/Frontend/express-server
   npm install
   npm start
   ```

   Nyisd meg: **http://localhost:3000** (vagy a `PORT` env szerint).

4. **Proxy cél:** ha az API nem a `127.0.0.1:5181`-en fut, állítsd:

   ```bash
   set SALONIC_API_PROXY_TARGET=http://127.0.0.1:5181
   npm start
   ```

   (Linux/macOS: `export SALONIC_API_PROXY_TARGET=...`)

## Tesztek

```bash
dotnet test Foglaloplusz.Tests/Foglaloplusz.Tests.csproj
```

## Docker (csak backend)

A repó gyökerében lévő `Dockerfile` a .NET API-t publikálja (pl. **8080**). A PostgreSQL-t külön kell biztosítani; a kapcsolati sztringet futtatáskor add át (env / `appsettings`).

## Dokumentáció

- Részletes leírások, UML, leadandó Word anyagok: **`docs/`** (Markdown, PlantUML, `leadando-dokumentaciok/`).
- API próba: **Swagger UI** (fejlesztői profilban alapból megnyílik).

## Licenc / státusz

Egyetemi / vizsgaremek jellegű projekt; a licenc nincs külön megadva.
