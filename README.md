# Amatorska Liga Tenisowa - dokumentacja projektu

## Autor

*Kacper Lipiec*

## Opis aplikacji

Aplikacja służy do zarządzania amatorską ligą tenisową: rejestracja graczy i sezonów, planowanie meczów, wpisywanie wyników i szczegółowych statystyk meczowych, generowanie rankingu w stylu ATP oraz analiz statystycznych (profil gracza, head-to-head, rekordy ligi).

## Tabele bazy danych


| Tabela          | Opis                                                                      |
| --------------- | ------------------------------------------------------------------------- |
| Sezony          | Nazwa, daty, flaga aktywnego sezonu                                       |
| Gracze          | Dane osobowe, ręka, styl gry                                              |
| Mecze           | Para graczy, sezon, data, nawierzchnia, format, zwycięzca                 |
| Sety            | Wynik setów (gemy, opcjonalnie tie-break)                                 |
| StatystykiMeczu | Asy, serwis, winnery, błędy niewymuszone, break pointy, czas, publiczność |
| Uzytkownicy     | Login, hash hasła, token API, rola admin                                  |


## Uruchomienie

```bash
cd LigaTenisowa
dotnet run
```

Aplikacja web: [http://localhost:5118](http://localhost:5118)

Przy **pierwszym uruchomieniu** tworzona jest baza SQLite i wczytywane są dane z `Data/seed.json`.

### Dane logowania (seed)

**Administrator**


| Pole      | Wartość                          |
| --------- | -------------------------------- |
| Login     | `admin`                          |
| Hasło     | `admin123`                       |
| API Token | `liga-tenisowa-admin-token-demo` |


**Użytkownik zwykły** (konto powiązane z graczem Hubert Hurkacz)


| Pole      | Wartość                           |
| --------- | --------------------------------- |
| Login     | `hubert`                          |
| Hasło     | `hubert123`                       |
| API Token | `liga-tenisowa-hubert-token-demo` |


Zwykły użytkownik ma dostęp do przeglądania (ranking, profile, mecze, statystyki) i API GET; bez panelu admina i bez operacji wymagających uprawnień administratora (np. DELETE meczu, CRUD graczy w API). Wyniki może wpisywać **tylko do meczów, w których gra jego powiązany gracz** (hubert -> mecze Hurkacza).

Token API można też skopiować w **Panel admina** po zalogowaniu jako admin.

Aby wczytać seed od nowa: usuń plik `LigaTenisowa/tenisliga.db` i uruchom aplikację ponownie.

## Interfejs webowy (MVC)

Po zalogowaniu dostępne są m.in.:

- **Gracze** - lista, dodawanie, edycja, usuwanie, profil statystyczny
- **Sezony** - CRUD, szczegóły z listą meczów
- **Mecze** - CRUD, karta meczu, wpisywanie wyniku i statystyk
- **Kalendarz** - zaplanowane mecze bez wyniku
- **Ranking** - tabela aktywnego sezonu i all-time (punktacja: 120/100/20/10)
- **Statystyki** - Head-to-Head, rekordy ligi
- **Panel admina** - użytkownicy, reset/kopiowanie tokenów API

Niezalogowany użytkownik jest przekierowywany na stronę logowania.

## REST API

Każde żądanie wymaga nagłówków:

```
X-Username: admin
X-Api-Token: liga-tenisowa-admin-token-demo
```

### Endpointy


| Metoda | Endpoint                | Opis                               |
| ------ | ----------------------- | ---------------------------------- |
| GET    | `/api/gracze`           | Lista graczy z rankingiem sezonu   |
| GET    | `/api/gracze/{id}`      | Profil gracza                      |
| POST   | `/api/gracze`           | Dodaj gracza (admin)               |
| PUT    | `/api/gracze/{id}`      | Edytuj gracza (admin)              |
| DELETE | `/api/gracze/{id}`      | Usuń gracza (admin)                |
| GET    | `/api/mecze`            | Lista meczów                       |
| GET    | `/api/mecze/{id}`       | Szczegóły meczu (sety, statystyki) |
| POST   | `/api/mecze`            | Dodaj mecz                         |
| PUT    | `/api/mecze/{id}/wynik` | Wpisz wynik                        |
| DELETE | `/api/mecze/{id}`       | Usuń mecz (admin)                  |
| GET    | `/api/sezony`           | Lista sezonów                      |
| GET    | `/api/ranking`          | Ranking aktywnego sezonu           |
| GET    | `/api/ranking/alltime`  | Ranking wszech czasów              |


### Przykładowe zapytania (terminal)

W **jednym terminalu** uruchom aplikację (`cd LigaTenisowa && dotnet run`).

W drugim terminalu wklej poniższe komendy:

```bash
BASE="http://localhost:5118"
ADMIN_USER="admin"
ADMIN_TOKEN="liga-tenisowa-admin-token-demo"
HUBERT_USER="hubert"
HUBERT_TOKEN="liga-tenisowa-hubert-token-demo"
```

Po **świeżym seedzie** w bazie jest m.in.: 6 graczy (ID 1–6, Hurkacz = 6), sezon ID 1, 10 meczów (ID 1–10). Zaplanowane bez wyniku: mecz **9** (Nadal vs Hurkacz), mecz **10** (Alcaraz vs Federer).

#### Odczyt (GET) - admin

```bash
curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/gracze"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/gracze/1"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/mecze"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/mecze/1"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/sezony"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/ranking"

curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" "$BASE/api/ranking/alltime"
```

#### Odczyt (GET) - użytkownik hubert (działa tak samo dla GET)

```bash
curl -s -H "X-Username: $HUBERT_USER" -H "X-Api-Token: $HUBERT_TOKEN" "$BASE/api/ranking"
```

#### Zły token (oczekiwany błąd autoryzacji)

```bash
curl -s -o /dev/null -w "HTTP %{http_code}\n" \
  -H "X-Username: $ADMIN_USER" -H "X-Api-Token: zly-token" \
  "$BASE/api/gracze"
```

#### Dodanie gracza (POST) - tylko admin

Enumy w JSON jako liczby: `reka` 0=Prawa, 1=Lewa; `stylGry` 0=Allcourt, 1=Baseliner, 2=ServeAndVolley.

```bash
curl -s -X POST "$BASE/api/gracze" \
  -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "imie": "Jan",
    "nazwisko": "Kowalski",
    "dataUrodzenia": "1995-03-20",
    "kraj": "Polska",
    "reka": 0,
    "stylGry": 1
  }'
```

#### Dodanie zaplanowanego meczu (POST)

`nawierzchnia`: 0=Hard, 1=Clay, 2=Grass. `format`: 0=Bo3, 1=Bo5.

```bash
curl -s -X POST "$BASE/api/mecze" \
  -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "gracz1Id": 4,
    "gracz2Id": 5,
    "sezonId": 1,
    "dataMeczu": "2026-07-15",
    "nawierzchnia": 0,
    "format": 0
  }'
```

#### Wpisanie wyniku (PUT) - mecz 9, hubert (Hurkacz) może

Mecz 9 w seedzie: Nadal (1) vs Hurkacz (6). Konto `hubert` jest powiązane z graczem 6.

```bash
curl -s -X PUT "$BASE/api/mecze/9/wynik" \
  -H "X-Username: $HUBERT_USER" -H "X-Api-Token: $HUBERT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "zwyciezcaId": 6,
    "sety": [
      { "numerSeta": 1, "gemyGracz1": 4, "gemyGracz2": 6 },
      { "numerSeta": 2, "gemyGracz1": 6, "gemyGracz2": 7, "tiebreakGracz1": 4, "tiebreakGracz2": 7 }
    ],
    "statystyki": {
      "asyGracz1": 4, "asyGracz2": 9,
      "doubleFaultsGracz1": 2, "doubleFaultsGracz2": 1,
      "pierwszySerwisProcentGracz1": 65.0, "pierwszySerwisProcentGracz2": 74.0,
      "pktNa1SerGracz1": 70, "pktNa1SerGracz2": 80,
      "pktNa2SerGracz1": 50, "pktNa2SerGracz2": 58,
      "winnersGracz1": 22, "winnersGracz2": 28,
      "unforcedErrorsGracz1": 18, "unforcedErrorsGracz2": 14,
      "breakPktWykorzystaneGracz1": 2, "breakPktWykorzystaneGracz2": 5,
      "breakPktOkazjeGracz1": 4, "breakPktOkazjeGracz2": 6,
      "czasMeczuMin": 118,
      "publicznosc": 5000
    }
  }'
```

#### Wpisanie wyniku - mecz 10, hubert **nie może** (401)

Mecz 10: Alcaraz vs Federer - Hurkacz nie gra w tym meczu.

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -X PUT "$BASE/api/mecze/10/wynik" \
  -H "X-Username: $HUBERT_USER" -H "X-Api-Token: $HUBERT_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"zwyciezcaId": 4, "sety": [{"numerSeta": 1, "gemyGracz1": 6, "gemyGracz2": 4}], "statystyki": {"asyGracz1": 1, "asyGracz2": 1, "doubleFaultsGracz1": 0, "doubleFaultsGracz2": 0, "pierwszySerwisProcentGracz1": 70, "pierwszySerwisProcentGracz2": 70, "pktNa1SerGracz1": 1, "pktNa1SerGracz2": 1, "pktNa2SerGracz1": 1, "pktNa2SerGracz2": 1, "winnersGracz1": 1, "winnersGracz2": 1, "unforcedErrorsGracz1": 1, "unforcedErrorsGracz2": 1, "breakPktWykorzystaneGracz1": 0, "breakPktWykorzystaneGracz2": 0, "breakPktOkazjeGracz1": 0, "breakPktOkazjeGracz2": 0, "czasMeczuMin": 90, "publicznosc": 1000}}'
```

#### Usunięcie meczu (DELETE) - tylko admin

```bash
curl -s -w "\nHTTP %{http_code}\n" \
  -X DELETE "$BASE/api/mecze/10" \
  -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN"
```

#### Usunięcie gracza z meczami (DELETE) - błąd (400)

```bash
curl -s -H "X-Username: $ADMIN_USER" -H "X-Api-Token: $ADMIN_TOKEN" \
  -X DELETE "$BASE/api/gracze/1"
```

## Programy konsolowe

### LigaTenisowaClient - pełny klient interaktywny

```bash
dotnet run --project LigaTenisowaClient
```

Menu obejmuje CRUD graczy i meczów, wpisywanie wyniku, sezony, rankingi. Wymaga podania loginu i tokenu API.

### LigaTenisowaDemo - skrypt demonstracyjny

```bash
dotnet run --project LigaTenisowaDemo
```

Automatycznie wykonuje serię żądań GET/POST i wyświetla kody HTTP.

## Projekty

- `LigaTenisowa` - aplikacja webowa
- `LigaTenisowaClient` - klient REST API
- `LigaTenisowaDemo` - skrypt demonstracyjny

