# Postęp prac — Osiedle '77

Claude dopisuje tu wpis na końcu każdego etapu. Najnowszy wpis na górze.

## Stan obecny

- Etap: M0 „Ruch i kamera” ukończony, sprawdzony ręcznie, scalony do `main` (tag `m0`).
- Następny krok: M1 — walka wręcz z manekinem (Hitbox/Hurtbox/Health/DamageInfo, Złom, hit-stop).

## Dziennik

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
