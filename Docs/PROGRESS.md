# Postęp prac — Osiedle '77

Claude dopisuje tu wpis na końcu każdego etapu. Najnowszy wpis na górze.

## Stan obecny

- Etap: M1 „Sztacheta i manekin” gotowy na gałęzi `m1-sztacheta`, czeka na test ręczny i scalenie do `main`.
- Następny krok: M2 — proca na śruby (strzał za Złom, pociski z puli, odbicie od ściany).
- Kolejność ustalona z autorem: M2 = proca, M3 = pierwsi wrogowie z telegrafami.

## Dziennik

### 2026-09-24 — M1 (poprawka) Widoczny zamach sztachety
- Zgłoszenie autora: „nie widzę uderzeń” — cios nie miał żadnego wyglądu, był widoczny tylko po trafieniu manekina (błysk, odrzut, śrubki).
- Działa:
  - `MeleeSwingVisual`: szara sztacheta pojawia się tylko w trakcie ciosu; ciosy 1 i 2 zamiatają poziomo na przemian, 3. uderza z góry. Łuk = `arcDegrees` broni.
  - `PlayerMelee` udostępnia fazę i postęp ciosu (`MeleePhase`, `PhaseProgress`, `IsFinisher`).
  - Nowe pola w `Sztacheta`: `visualLength`, `overheadRaiseDegrees`, `overheadEndDegrees`.
  - Sprawdzone zrzutami z kamery gry (sztacheta widoczna, manekin błyska po trafieniu). Test sceny sprawdza też widoczność broni. Testy 40/40.
- Uwaga: proca (LPM) to etap M2 — w M1 lewy przycisk nic nie robi.

### 2026-09-24 — M1 (dodatek) Kamera 50° i wzorzec wyglądu
- Działa:
  - Kamera: kąt 50° (było 55°), FOV 35° (było 40°), odległość 14,3 m — postać zajmuje ok. 1/10 wysokości ekranu. Dalej bez obrotu i z wyprzedzeniem w stronę kursora. Wszystko w `PlayerData` (sekcja Kamera).
  - Nowe menu `Osiedle/Dane/Kamera — przywróć wartości domyślne` (budowniczy nie nadpisuje strojenia, więc nowe wartości wgrywa się tym menu). Wgrane do obecnego `PlayerData`.
  - Test_Combat: wysokie ściany — północ 4 m (blok), wschód 3 m (garaże), zachód 3,5 m (pawilon) i wolnostojący garaż 3 m; południe zostaje niskie, żeby nie zasłaniać gracza.
  - Wzorzec wyglądu `Docs/Concept/wzorzec_walka.png` (skopiowany z pobranego „Obraz Codex 24 wrz 2026, 16_50_33.png”); GDD sekcja 10 „Wzorzec wyglądu”, sekcja 3 kamera 50°; CLAUDE.md — linia o wzorcu.
  - Poprawione ostrzeżenie o przestarzałym `FindObjectsSortMode` w teście. Testy 40/40.
- Znane błędy:
  - Ściany przed postacią (od strony kamery) zasłaniają ją — półprzezroczystość ścian wciąż odłożona.
- Porządki zgodności z CLAUDE.md (po akceptacji autora):
  - CLAUDE.md: w strukturze dopisane `Data/Player`, `Data/Input`, `Prefabs/Pickups`.
  - Rozmiary ciał przeniesione do danych: `PlayerData` (bodyHeight, bodyRadius, stepOffset), `EnemyData` (bodyHeight, bodyRadius). Zmiana wymaga ponownego zbudowania sceny.
  - Rozrzut śrubek: `PlayerData.scrapScatterMinFraction` zamiast stałej w kodzie.
  - Bez zmian (decyzja): pola manekina w `EnemyData` z nagłówkiem, `DebugHud` jako tymczasowe narzędzie.
- Do decyzji autora: czy Unity 6000.6.2f1 to wersja LTS (CLAUDE.md wymaga LTS).

### 2026-09-24 — M1 Sztacheta i manekin
- Działa:
  - Walka: `DamageInfo`, `Element`, `Hitbox` (łuk przed postacią, każdy cel raz na cios), `Hurtbox` (filtry `IDamageFilter`, odrzut, `GameEvents.OnHit`), `Health` + czysta logika `HealthPool`, `Knockback`, `HitStop`.
  - Sztacheta (`Data/Weapons/Sztacheta`): 30 obrażeń, seria 3 ciosów (trzeci x1,5, najmocniejszy odrzut 14 m/s, hit-stop 0,08 s), łuk 120°, zasięg 2 m, wolniejszy ruch w trakcie ciosu, krok do przodu.
  - `PlayerMelee`: PPM z buforem wejścia, okno serii 0,5 s, dash przerywa cios.
  - Złom i Moc (`PlayerResources` + `ResourcePool`): trafienie wybija 1–3 śrubki z puli (`ScrapSpawner`, `ObjectPool`), śrubki same lecą do gracza (nie, gdy Złom pełny); +4 Mocy za trafienie. Start: 10/30 Złomu.
  - Nietykalność dasha działa jako filtr obrażeń gracza. Gracz ma Health (100) i nieśmiertelność po trafieniu 0,8 s (na razie nic go nie bije).
  - Manekin (`Data/Enemies/Manekin`, `TrainingDummy`): 300 HP, biały błysk, odrzut, leczenie po 2 s spokoju, wstaje 1 s po „śmierci”.
  - `DebugHud` (tymczasowy, lewy górny róg): HP, Złom, Moc, dash, numer ciosu.
  - Nowe zdarzenia: `OnHit`, `OnKill`, `OnDamageTaken`, `OnScrapChanged`, `OnPowerChanged`.
  - Budowniczy rozbity na wspólne części (`SharedAssets`, `PlayerPrefabBuilder`, `CombatAssetsBuilder`, `SceneKit`, `InputControlsBuilder`, `BuilderMaterials`, `BuilderUtils`). Nowe menu `Osiedle/Build/Test_Combat`.
  - Testy: 40/40 (m.in. HealthPool, ComboCounter, ResourcePool, ScrapRoll, test dymny Test_Combat).
- Znane błędy:
  - Budowniczy i testy w trybie wsadowym działają tylko przy zamkniętym edytorze Unity.
  - DebugHud ma teksty w kodzie (tymczasowo, do czasu prawdziwego HUD-u z Texts_PL).
- Odłożone na później:
  - Wstrząs ekranu (Cinemachine Impulse), dźwięki trafień, efekty.
  - Rozbijanie tarcz 3. ciosem (przy Milicjancie), krytyki, pozostałe bronie białe.
  - Czar na Q (Moc już się ładuje).
- Tag git: brak (po akceptacji: `m1`).

### 2026-09-24 — M0 Ruch i kamera
- Zakres ustalony z GDD (sekcja 3 i 13), bo zakładka „Plan produkcji” nie była dostępna.
- Działa:
  - Projekt Unity 6000.6.2f1 URP; pakiety: Input System, Cinemachine, ProBuilder, AI Navigation, Test Framework, uGUI/TMP. Usunięte pliki szablonu.
  - Budowniczy `Osiedle/Build/Test_Movement`: PlayerData, mapa klawiszy OsiedleControls (ustawiona jako akcje projektu), szare materiały, prefab Player, scena Test_Movement (dodana do Build Settings). Ponowne uruchomienie nie nadpisuje strojenia PlayerData.
  - Ruch WASD 6 m/s bez przyspieszenia, grawitacja, coyote time 0,1 s.
  - Celowanie: promień z kursora na płaszczyznę na wysokości biodra, postać zawsze patrzy na kursor.
  - Dash 4 m / 0,18 s, odnowienie 0,8 s, 1 ładunek, nieśmiertelność 0,3 s (`PlayerDash.IsInvulnerable`), bufor wejścia 0,15 s.
  - Dash kontekstowy: przy obiekcie z `Vaultable` skok na niego (sprawdza wysokość, powierzchnię i miejsce na lądowanie); strzałka nad najbliższą krawędzią.
  - Kamera Cinemachine 55°, bez obrotu, wyprzedza postać w stronę kursora (maks. 3 m). Wszystkie liczby w PlayerData, strojenie działa w Play Mode.
  - `GameEvents.OnDash`, `GameEvents.OnVault`; logi `[Osiedle]`.
  - Testy: 16/16 (DashCharges, InputBuffer, test dymny sceny: dash ~4 m i skok na skrzynię).
- Znane błędy:
  - Ponowne zbudowanie sceny zmienia wewnętrzne ID obiektów w pliku .unity (duży diff w git, zachowanie bez zmian).
- Odłożone na później:
  - Półprzezroczyste ściany zasłaniające postać.
  - Sterowanie padem, perfekcyjny dash (M1+, potrzebuje ataków wroga), +20% obrażeń procy z podwyższenia.
  - Łuk ładowania dasha pod postacią (UI).
- Test ręczny (2026-09-24): autor zagrał w Test_Movement — wszystko w porządku.
- Tag git: `m0`.

<!-- Szablon wpisu:
### RRRR-MM-DD — M? nazwa etapu
- Działa:
- Znane błędy:
- Odłożone na później:
- Tag git:
-->
